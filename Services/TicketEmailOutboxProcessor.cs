using System.Text.Json;
using Microsoft.Extensions.Hosting;
using MongoDB.Driver;
using MovieTicketBooking.Api.Data;
using MovieTicketBooking.Api.DTO;
using MovieTicketBooking.Api.Interfaces;
using MovieTicketBooking.Api.Models;

namespace MovieTicketBooking.Api.Services;

public class TicketEmailOutboxProcessor : BackgroundService
{
    private readonly MongoDbContext _db;
    private readonly IEmailSender _emailSender;
    private readonly ILogger<TicketEmailOutboxProcessor> _logger;
    private readonly TimeSpan _pollInterval;

    public TicketEmailOutboxProcessor(
        MongoDbContext db,
        IEmailSender emailSender,
        IConfiguration config,
        ILogger<TicketEmailOutboxProcessor> logger)
    {
        _db = db;
        _emailSender = emailSender;
        _logger = logger;
        var seconds = config.GetValue<int?>("EmailOutbox:PollSeconds") ?? 10;
        _pollInterval = TimeSpan.FromSeconds(seconds);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("TicketEmailOutboxProcessor started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessBatchAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while processing ticket email outbox");
            }

            try
            {
                await Task.Delay(_pollInterval, stoppingToken);
            }
            catch (TaskCanceledException)
            {
                break;
            }
        }

        _logger.LogInformation("TicketEmailOutboxProcessor stopping");
    }

    private async Task ProcessBatchAsync(CancellationToken ct)
    {
        var collection = _db.TicketEmailOutbox;

        var filter = Builders<TicketEmailOutboxItem>.Filter.Eq(x => x.ProcessedAt, null);
        var pending = await collection
            .Find(filter)
            .Limit(10)
            .ToListAsync(ct);

        if (pending.Count == 0)
            return;

        _logger.LogInformation("Processing {Count} ticket email outbox items", pending.Count);

        foreach (var item in pending)
        {
            if (ct.IsCancellationRequested)
                break;

            try
            {
                var message = JsonSerializer.Deserialize<TicketEmailMessage>(item.Payload);
                if (message == null)
                {
                    _logger.LogWarning("Skipping outbox item {Id}: payload deserialization failed", item.Id);
                    await MarkProcessedAsync(item, "Deserialization failed", ct);
                    continue;
                }

                var html = TicketEmailTemplateBuilder.Build(message);

                await _emailSender.SendTicketEmailAsync(
                    message.UserEmail,
                    message.UserName,
                    $"Your Movie Ticket - {message.MovieTitle}",
                    html,
                    message.QrCodeBase64);

                await MarkProcessedAsync(item, null, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process outbox item {Id}", item.Id);
                await MarkProcessedAsync(item, ex.Message, ct);
            }
        }
    }

    private async Task MarkProcessedAsync(
        TicketEmailOutboxItem item,
        string? error,
        CancellationToken ct)
    {
        var collection = _db.TicketEmailOutbox;
        item.ProcessedAt = DateTime.UtcNow;
        item.Error = error;

        await collection.ReplaceOneAsync(
            x => x.Id == item.Id,
            item,
            cancellationToken: ct);
    }
}

