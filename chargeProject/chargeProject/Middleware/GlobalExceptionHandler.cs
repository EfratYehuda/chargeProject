using BLL.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Authentication;

namespace chargeProject.Middleware
{
    // Every exception that nobody catches ends up here (registered in Program.cs).
    // The BLL only says WHAT happened (NotFoundException...); this class decides the HTTP code.
    public class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandler> _logger;

        public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) => _logger = logger;

        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            // The first matching line wins.
            var (status, title, detail) = exception switch
            {
                // Our own exceptions, thrown by the BLL. Their messages were written for the client.
                NotFoundException => (StatusCodes.Status404NotFound, "Not found", exception.Message),
                ConflictException => (StatusCodes.Status409Conflict, "Conflict", exception.Message),
                BusinessRuleException => (StatusCodes.Status400BadRequest, "Bad request", exception.Message),

                // Security: missing/invalid token claim (401) or missing permission (403)
                AuthenticationException => (StatusCodes.Status401Unauthorized, "Unauthorized", "Authentication is required or the token is invalid."),
                UnauthorizedAccessException => (StatusCodes.Status403Forbidden, "Forbidden", exception.Message),

                // Database: unique index, foreign key, concurrency. Never show the database message.
                DbUpdateException => (StatusCodes.Status409Conflict, "Conflict", "The change conflicts with existing data."),

                // Anything else is a bug: log everything, tell the client nothing.
                // (KeyNotFoundException and InvalidOperationException were removed from this list:
                //  .NET and EF throw them for their own bugs, and their messages must not reach the client.)
                _ => (StatusCodes.Status500InternalServerError, "Server error", "An unexpected error occurred.")
            };

            if (status == StatusCodes.Status500InternalServerError)
                _logger.LogError(exception, "Unhandled exception on {Path}", httpContext.Request.Path);
            else
                _logger.LogWarning("{Type} on {Path}: {Message}", exception.GetType().Name, httpContext.Request.Path, exception.Message);

            httpContext.Response.StatusCode = status;
            await httpContext.Response.WriteAsJsonAsync(
                new ProblemDetails { Status = status, Title = title, Detail = detail }, cancellationToken);
            return true;
        }
    }
}