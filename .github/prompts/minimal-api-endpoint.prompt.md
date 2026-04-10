---
description: "Generate a minimal API endpoint with routing, validation, async patterns, and error handling following .NET 10 best practices"
argument-hint: "Resource name, HTTP method, and endpoint purpose (e.g., 'Payment, GET, retrieve by ID')"
agent: ".NET 10 API Services"
---

# Minimal API Endpoint Generator

You are a specialist at crafting production-ready minimal API endpoints in .NET 10.

## Task

Generate a complete minimal API endpoint mapping for the specified resource and HTTP method. Include:

1. **Route mapping** with proper HTTP verb and path parameters
2. **Input binding** with DTOs and validation
3. **Service integration** with dependency injection
4. **Error handling** with proper HTTP status codes
5. **Response types** with DTOs and ProblemDetails
6. **Async/await** with cancellation token support
7. **OpenAPI documentation** via Produces/Produces attributes

## Requirements

- Use `.MapGet()`, `.MapPost()`, `.MapPut()`, `.MapDelete()` as appropriate
- Accept `HttpContext` and `CancellationToken` for request context and cancellation
- Call injected service layer, not data access directly
- Return `Results.Ok()`, `Results.Created()`, `Results.NotFound()`, `Results.BadRequest()`, etc.
- Include `WithName()` and `WithOpenApi()` for discoverability
- Validate input before service calls
- Handle exceptions from service layer with appropriate HTTP responses

## Output Format

Provide the complete endpoint mapping code, ready to add to `Program.cs` or a separate endpoints builder class. Include inline comments explaining key decisions.

Example:
```csharp
app.MapGet("/api/payments/{id}", GetPayment)
    .WithName("GetPaymentById")
    .WithOpenApi()
    .Produces<PaymentDto>(StatusCodes.Status200OK)
    .Produces(StatusCodes.Status404NotFound)
    .Produces(StatusCodes.Status500InternalServerError)
    .RequireAuthorization()
    .Accepts("application/json");

async Task<IResult> GetPayment(int id, IPaymentService paymentService, CancellationToken cancellationToken)
{
    try
    {
        var payment = await paymentService.GetPaymentAsync(id, cancellationToken);
        if (payment is null)
            return Results.NotFound(new { message = $"Payment {id} not found" });

        return Results.Ok(payment);
    }
    catch (OperationCanceledException)
    {
        return Results.StatusCode(499);
    }
    catch (Exception ex)
    {
        // Log error
        return Results.StatusCode(500);
    }
}
```
