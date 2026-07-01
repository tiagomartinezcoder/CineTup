using CineTup.Application.Exceptions;
using System.Text.Json;

namespace CineTup.Presentation.Middlewares
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
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
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var (statusCode, message) = exception switch
            {
                ValidationException ex => (StatusCodes.Status400BadRequest, ex.Message),
                NotFoundException ex => (StatusCodes.Status404NotFound, ex.Message),
                ConflictException ex => (StatusCodes.Status409Conflict, ex.Message),
                UnauthorizedException ex => (StatusCodes.Status401Unauthorized, ex.Message),
                DatabaseException ex => (StatusCodes.Status500InternalServerError, ex.Message),
                _ => (StatusCodes.Status500InternalServerError, "Ocurrió un error inesperado.")
            };

            if (exception is not (ValidationException or NotFoundException or ConflictException or UnauthorizedException))
                _logger.LogError(exception, "Excepción no controlada: {Message}", exception.Message);

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = statusCode;

            var response = new
            {
                status = statusCode,
                error = message,
                exception = exception.Message,
                innerException = exception.InnerException?.Message,
                innerExceptionComplete = exception.InnerException?.ToString(),
                type = exception.GetType().Name
            };

            var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            await context.Response.WriteAsync(json);
        }
    }
}
