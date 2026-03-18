using Microsoft.OpenApi.Models;
using Serilog;
using MovieTicketBooking.Api.Data;
using MovieTicketBooking.Api.Interfaces;
using MovieTicketBooking.Api.Repo;
using MovieTicketBooking.Api.Services;
using MovieTicketBooking.Api.Middleware; 
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;

var builder = WebApplication.CreateBuilder(args);
    
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();

//use Serilog instead of default logger from Microsoft
builder.Host.UseSerilog();
builder.Services.AddApplicationInsightsTelemetry();
builder.Services.AddControllers();
builder.Services.AddMemoryCache();

//Used for scanning endpoints for Swagger
builder.Services.AddEndpointsApiExplorer();
//Registers swagger generator
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Movie Ticket Booking API",
        Version = "v1",
        Description = "API for movie ticket booking system with Razorpay integration"
    });

    options.EnableAnnotations();
});

// MongoDB (Atlas or local)
// Keep element names camelCase to match existing JSON property names.
ConventionRegistry.Register(
    "mtbsConventions",
    new ConventionPack
    {
        new CamelCaseElementNameConvention(),
        new IgnoreExtraElementsConvention(true)
    },
    _ => true);

builder.Services.AddSingleton<IMongoClient>(_ =>
{
    var connectionString = builder.Configuration["MongoDb:ConnectionString"];
    if (string.IsNullOrWhiteSpace(connectionString))
        throw new InvalidOperationException("MongoDb:ConnectionString is not configured");

    return new MongoClient(connectionString);
});

builder.Services.AddSingleton<MongoDbContext>();

// Scoped as only one instance per request
builder.Services.AddScoped<ICityRepository, CityRepository>(); 
builder.Services.AddScoped<IMovieRepository, MovieRepository>();
builder.Services.AddScoped<ITheatreRepository, TheatreRepository>();
builder.Services.AddScoped<IShowRepository, ShowRepository>();
builder.Services.AddScoped<ISeatLockRepository, SeatLockRepository>();
builder.Services.AddScoped<IBookingRepository, BookingRepository>();
builder.Services.AddScoped<ITicketRepository, TicketRepository>();
builder.Services.AddScoped<ILocationService, LocationService>();
builder.Services.AddScoped<IMovieService, MovieService>();
builder.Services.AddScoped<ITheatreService, TheatreService>();
builder.Services.AddScoped<IShowService, ShowService>();
builder.Services.AddScoped<ISeatService, SeatService>();
builder.Services.AddScoped<IBookingService, BookingService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<ITicketService, TicketService>(); 
builder.Services.AddSingleton<IQRCodeService, QRCodeService>();
builder.Services.AddSingleton<ICityMovieRepository, CityMovieRepository>();
builder.Services.AddScoped<IMessagePublisher, OutboxMessagePublisher>(); 
builder.Services.AddSingleton<IEmailSender, SmtpEmailSender>();

builder.Services.AddHostedService<SeatLockCleanupService>();
builder.Services.AddHostedService<TicketEmailOutboxProcessor>();

builder.Services.AddCors(options =>
{
    var allowedOrigins = builder.Configuration
        .GetSection("App:AllowedOrigins")
        .Get<string[]>() ?? Array.Empty<string>();

    options.AddPolicy("AllowReactFrontend", policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

// Allows service to access current req, headers. Give access to HttpContext
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Movie Ticket Booking API v1");
    });
}
//Forces HTTP to HTTPS
app.UseHttpsRedirection();
app.UseCors("AllowReactFrontend");

/*
    To catch any unhandled exceptions globally. Must be before controllers - 
        It acts as a wrapper and ensure a proper response to the client.
*/
app.UseMiddleware<ExceptionHandlingMiddleware>();

//Activates [ApiController] classes. WIthout it, no endpoints will work
app.MapControllers();

app.MapGet("/health", () =>
    Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow })
);

app.Run();