using FluentValidation;
using RR.AI_Chat.Dto;
using System.Net;
using System.Text.Json;

namespace RR.AI_Chat.Api.Middlewares
{
    /// <summary>
    /// Middleware for handling exceptions globally across the application.
    /// Catches unhandled exceptions, logs them, and returns standardized error responses.
    /// </summary>
    /// <param name="next">The next middleware in the request pipeline.</param>
    /// <param name="logger">Logger instance for recording error information.</param>
    public class ExceptionHandlerMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlerMiddleware> logger)
    {
        private readonly RequestDelegate _next = next;
        private readonly ILogger<ExceptionHandlerMiddleware> _logger = logger;

        /// <summary>
        /// Invokes the middleware to process the HTTP request.
        /// Wraps the next middleware in a try-catch block to handle any exceptions.
        /// </summary>
        /// <param name="context">The HTTP context for the current request.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
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

        /// <summary>
        /// Handles exceptions by logging the error and writing a standardized error response.
        /// </summary>
        /// <param name="context">The HTTP context for the current request.</param>
        /// <param name="exception">The exception that was caught.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            // Handle cancellation exceptions (client disconnect, timeout, etc.)
            if (exception is OperationCanceledException)
            {
                _logger.LogInformation(
                    "Request was cancelled. TraceId: {TraceId}",
                    context.TraceIdentifier);
                
                // If response has already started (e.g., during streaming), we cannot modify headers
                if (!context.Response.HasStarted)
                {
                    context.Response.StatusCode = StatusCodes.Status499ClientClosedRequest;
                }
                return;
            }

            _logger.LogError(exception,
                "An error occurred: {Message}. TraceId: {TraceId}",
                exception.Message,
                context.TraceIdentifier);

            var error = GetError(context, exception);

            // Set response properties
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)error.StatusCode;
            await context.Response.WriteAsync(
                JsonSerializer.Serialize(error));
        }

        /// <summary>
        /// Maps exceptions to appropriate error responses with corresponding HTTP status codes.
        /// </summary>
        /// <param name="context">The HTTP context for the current request.</param>
        /// <param name="exception">The exception to map to an error response.</param>
        /// <returns>An <see cref="ErrorDto"/> containing error details and appropriate status code.</returns>
        private static ErrorDto GetError(HttpContext context, Exception exception)
        {
            return exception switch
            {
                ValidationException validationException => new ErrorDto
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    Errors = [.. validationException.Errors.Select(e => e.ErrorMessage)],
                    TraceId = context.TraceIdentifier,
                    Timestamp = DateTimeOffset.UtcNow
                },
                ArgumentNullException => new ErrorDto
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    Errors = [exception.Message],
                    TraceId = context.TraceIdentifier,
                    Timestamp = DateTimeOffset.UtcNow
                },
                ArgumentException => new ErrorDto
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    Errors = [exception.Message],
                    TraceId = context.TraceIdentifier,
                    Timestamp = DateTimeOffset.UtcNow
                },
                InvalidOperationException => new ErrorDto
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    Errors = [exception.Message],
                    TraceId = context.TraceIdentifier,
                    Timestamp = DateTimeOffset.UtcNow
                },
                KeyNotFoundException => new ErrorDto
                {
                    StatusCode = HttpStatusCode.NotFound,
                    Errors = [exception.Message],
                    TraceId = context.TraceIdentifier,
                    Timestamp = DateTimeOffset.UtcNow
                },
                _ => new ErrorDto
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    Errors = ["An unexpected internal server error occurred."],
                    TraceId = context.TraceIdentifier,
                    Timestamp = DateTimeOffset.UtcNow
                }
            };
        }
    }
}
