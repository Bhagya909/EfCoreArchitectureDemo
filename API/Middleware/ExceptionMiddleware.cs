using Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace API.Middleware
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;
        private readonly IHostEnvironment _env;

        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        public ExceptionMiddleware(
            RequestDelegate next,
            ILogger<ExceptionMiddleware> logger,
            IHostEnvironment env)
        {
            _next = next;
            _logger = logger;
            _env = env;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Unhandled exception on {Method} {Path}",
                    context.Request.Method,
                    context.Request.Path);

                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(
            HttpContext context,
            Exception exception)
        {
            var (statusCode, title) = exception switch
            {
                KeyNotFoundException =>
                    (StatusCodes.Status404NotFound, "Resource Not Found"),

                ArgumentException =>
                    (StatusCodes.Status400BadRequest, "Bad Request"),

                ConcurrencyConflictException =>
                    (StatusCodes.Status409Conflict, "Conflict"),

                InvalidOperationException =>
                    (StatusCodes.Status409Conflict, "Conflict"),

                _ =>
                    (StatusCodes.Status500InternalServerError,
                        "Internal Server Error")
            };

            var problem = new ProblemDetails
            {
                Status = statusCode,
                Title = title,
                Detail = _env.IsDevelopment() || _env.IsEnvironment("Testing")
                    ? $"{exception.Message} | INNER: {exception.InnerException?.Message} | INNER2: {exception.InnerException?.InnerException?.Message}"
                    : "An error occurred. Please try again.",
                Instance = context.Request.Path
            };

            if (_env.IsDevelopment() ||
                _env.IsEnvironment("Testing"))
            {
                problem.Extensions["stackTrace"] =
                    exception.StackTrace;

                problem.Extensions["exceptionType"] =
                    exception.GetType().Name;
            }

            context.Response.ContentType =
                "application/problem+json";

            context.Response.StatusCode = statusCode;

            await context.Response.WriteAsync(
                JsonSerializer.Serialize(problem, _jsonOptions));
        }
    }
}
