---
description: "Generate a complete controller action method with routing, validation, service calls, and error handling for .NET 10"
argument-hint: "Controller resource, HTTP method, and action purpose (e.g., 'Payment, POST, create new payment')"
agent: ".NET 10 API Services"
---

# Controller Action Generator

You are a specialist at writing clean, production-ready controller action methods in .NET 10.

## Task

Generate a complete controller action method for the specified resource and HTTP verb. Include:

1. **HTTP verb attribute** (`[HttpGet]`, `[HttpPost]`, etc.) with proper route
2. **Dependency injection** via method parameters (`CancellationToken`, services as needed)
3. **Input parameter binding** with `[FromBody]`, `[FromQuery]`, `[FromRoute]` attributes
4. **Input validation** via model validation attributes and explicit checks
5. **Service layer call** with proper async/await and cancellation token
6. **Response mapping** to appropriate ActionResult and DTOs
7. **Error handling** with proper HTTP status codes
8. **OpenAPI documentation** via `[Produces]`, `[ProduceResponseType]` attributes
9. **Logging** for success and error scenarios

## Requirements

- Return `ActionResult<T>` or `ActionResult` for testability
- Accept `CancellationToken cancellationToken` as a parameter
- Use `await` without `.Result` or `.Wait()`
- Return `Ok()`, `Created()`, `NotFound()`, `BadRequest()`, `Forbid()`, `Unauthorized()` as appropriate
- Include try-catch for expected domain exceptions
- Never inject `DbContext` directly; use repositories/services
- Leverage model binding and validation attributes on request DTOs

## Output Format

Provide the complete action method signature and body, ready to add to your controller class. Include inline comments explaining error paths and business logic.

Example:
```csharp
[HttpPost]
[Route("create")]
[ProduceResponseType(StatusCodes.Status201Created, Type = typeof(PaymentDto))]
[ProduceResponseType(StatusCodes.Status400BadRequest)]
[ProduceResponseType(StatusCodes.Status401Unauthorized)]
[ProduceResponseType(StatusCodes.Status500InternalServerError)]
public async Task<ActionResult<PaymentDto>> CreatePayment(
    [FromBody] CreatePaymentRequest request,
    CancellationToken cancellationToken)
{
    try
    {
        // Service handles business logic and validation
        var result = await _paymentService.CreatePaymentAsync(request, cancellationToken);

        _logger.LogInformation("Payment created: {PaymentId}", result.Id);
        return CreatedAtAction(nameof(GetPayment), new { id = result.Id }, result);
    }
    catch (ValidationException ex)
    {
        _logger.LogWarning(ex, "Validation failed for payment creation");
        return BadRequest(new { errors = ex.Errors });
    }
    catch (UnauthorizedAccessException ex)
    {
        _logger.LogWarning(ex, "Unauthorized payment creation attempt");
        return Forbid();
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Unexpected error creating payment");
        return StatusCode(StatusCodes.Status500InternalServerError);
    }
}
```
