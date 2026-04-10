---
description: "Generate global error handling middleware or exception filters for consistent API error responses in .NET 10"
argument-hint: "Middleware or filter type (e.g., 'global exception middleware', 'action filter for validation')"
agent: ".NET 10 API Services"
---

# Error Handling Middleware Generator

You are a specialist at building robust, user-friendly error handling middleware and filters for .NET 10 APIs.

## Task

Generate global error handling middleware or exception filter code for consistent error responses across your API. Include:

1. **Exception catching** for different exception types (domain, validation, system)
2. **Logging** at appropriate levels (Info, Warning, Error)
3. **Error response mapping** to `ProblemDetails` or custom error DTOs
4. **HTTP status code selection** based on exception type
5. **Request context** (correlation ID, timestamp, request path)
6. **Sensitive information filtering** (no stack traces in production)
7. **Async support** where applicable

## Requirements

- Map domain exceptions to appropriate HTTP status codes (400, 404, 409, 422, etc.)
- Return consistent error response format (ProblemDetails or custom DTO)
- Include correlation IDs for error tracking
- Log full exception details (including stack trace) server-side only
- Do NOT expose stack traces to clients in production
- Support both middleware (`IMiddleware` or `UseMiddleware()`) and filter approaches
- Include request context: timestamp, path, method, user

## Output Format

Provide the complete middleware or filter implementation, ready to register in `Program.cs`. Include inline comments explaining each exception mapping.

Example Middleware:
```csharp
/// <summary>
/// Global exception handling middleware that catches unhandled exceptions
/// and returns consistent error responses as ProblemDetails.
/// </summary>
public class ExceptionHandlingMiddleware : IMiddleware
{
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(ILogger<ExceptionHandlingMiddleware> logger)
    {
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var correlationId = Guid.NewGuid().ToString();
        context.Items["CorrelationId"] = correlationId;

        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception. CorrelationId: {CorrelationId}", correlationId);
            await HandleExceptionAsync(context, ex, correlationId);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception, string correlationId)
    {
        context.Response.ContentType = "application/json";

        var response = new ProblemDetails
        {
            Extensions = new Dictionary<string, object?> { { "correlationId", correlationId } },
            Title = "An error occurred processing your request.",
            Detail = exception.Message,
            Instance = context.Request.Path
        };

        return exception switch
        {
            ResourceNotFoundException ex =>
                RespondWithProblem(context, response, StatusCodes.Status404NotFound, "Resource Not Found", ex.Message),

            ValidationException ex =>
                RespondWithProblem(context, response, StatusCodes.Status422UnprocessableEntity, "Validation Failed", string.Join(", ", ex.Errors)),

            UnauthorizedAccessException =>
                RespondWithProblem(context, response, StatusCodes.Status403Forbidden, "Access Denied", "You do not have permission to access this resource."),

            OperationCanceledException =>
                RespondWithProblem(context, response, 499, "Request Cancelled", "The client cancelled the request."),

            _ =>
                RespondWithProblem(context, response, StatusCodes.Status500InternalServerError, "Internal Server Error", "An unexpected error occurred. Please contact support with correlation ID: " + correlationId)
        };
    }

    private static Task RespondWithProblem(HttpContext context, ProblemDetails problem, int statusCode, string title, string detail)
    {
        context.Response.StatusCode = statusCode;
        problem.Status = statusCode;
        problem.Title = title;
        problem.Detail = detail;

        return context.Response.WriteAsJsonAsync(problem);
    }
}

// In Program.cs:
// services.AddScoped<ExceptionHandlingMiddleware>();
// app.UseMiddleware<ExceptionHandlingMiddleware>();
```
