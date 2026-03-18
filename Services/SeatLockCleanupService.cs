using MovieTicketBooking.Api.Interfaces;

namespace MovieTicketBooking.Api.Services;

public class SeatLockCleanupService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<SeatLockCleanupService> _logger;

    public SeatLockCleanupService(
        IServiceProvider serviceProvider,
        ILogger<SeatLockCleanupService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    { 
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var seatService = scope.ServiceProvider.GetRequiredService<ISeatService>();

                await seatService.ProcessExpiredLocksAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while processing expired seat locks");
            }

            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }
    }
}
