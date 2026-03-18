using MovieTicketBooking.Api.DTO;
using System.Net; 

namespace MovieTicketBooking.Api.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred");

            if (context.Response.HasStarted)
                throw;

            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception ex)
    {
        var (statusCode, code, message) = ex switch
        {
            ArgumentNullException or ArgumentException
                => (HttpStatusCode.BadRequest, "BAD_REQUEST", ex.Message),

            InvalidOperationException
                => (HttpStatusCode.Conflict, "CONFLICT", ex.Message),

            UnauthorizedAccessException
                => (HttpStatusCode.Unauthorized, "UNAUTHORIZED", "Access denied"),

            KeyNotFoundException
                => (HttpStatusCode.NotFound, "NOT_FOUND", ex.Message),

            _ => (HttpStatusCode.InternalServerError, "INTERNAL_ERROR", "An unexpected error occurred")
        };

        context.Response.StatusCode = (int)statusCode;

        await context.Response.WriteAsJsonAsync(
            ApiResponse<object>.Fail(code, message)
        );
    }
}
