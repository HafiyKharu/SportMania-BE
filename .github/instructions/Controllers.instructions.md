---
description: "Use when writing or refactoring API controllers. Covers routing, action methods, dependency injection, async patterns, model binding, and error handling in .NET 10 controllers."
applyTo: "Controllers/**/*.cs"
---

# Controller Guidelines (.NET 10)

## Structure & Naming

- **Class naming**: `{Resource}Controller` (e.g., `PaymentController`, `AuthController`)
- **Single responsibility**: One controller per resource/domain area
- **Dependency injection**: Constructor injection for services, repositories, and loggers
- **Attributes**: Use `[ApiController]` and `[Route("api/[controller]")]` for consistency

## Routing & Actions

```csharp
[HttpGet("{id}")]
[ProduceResponseType(StatusCodes.Status200OK)]
[ProduceResponseType(StatusCodes.Status404NotFound)]
public async Task<ActionResult<PaymentDto>> GetPayment(int id)
{
    var payment = await _paymentService.GetPaymentAsync(id);
    if (payment == null)
        return NotFound();
    return Ok(payment);
}
```

**Rules**:
- Use verb-based routing: `[HttpGet]`, `[HttpPost]`, `[HttpPut]`, `[HttpDelete]`
- Include `[Route(...)]` attributes for non-standard names
- Always return `ActionResult<T>` or `ActionResult` for testability
- Use `ProduceResponseType` attributes for OpenAPI/Swagger documentation

## Async & Cancellation

- All I/O operations must be `async Task<...>` (never `async void`)
- Accept `CancellationToken cancellationToken` parameter from routing
- Pass token to all downstream service/repository calls
- Use `ConfigureAwait(false)` in service layers, not controllers

```csharp
[HttpPost]
public async Task<ActionResult> CreateTransaction(
    [FromBody] CreateTransactionRequest request,
    CancellationToken cancellationToken)
{
    var result = await _transactionService.CreateAsync(request, cancellationToken);
    return CreatedAtAction(nameof(GetTransaction), new { id = result.Id }, result);
}
```

## Validation & Model Binding

- Use **model validation attributes** on DTOs: `[Required]`, `[StringLength]`, `[Range]`, etc.
- Leverage **automatic model validation** via `[ApiController]` attribute
- Bind query params from `[FromQuery]`, body from `[FromBody]`, route params implicitly
- Custom validation logic belongs in services, not controllers

## Dependency Injection

- Inject **services** (business logic), **repositories** (data access), and **loggers** via constructor
- Never inject the `DbContext` directly into controllers
- Use interface contracts (`IPaymentService`, `IKeyRepository`)
- Keep constructor lean; move complex setup to services

```csharp
public PaymentController(
    IPaymentService paymentService,
    ITransactionService transactionService,
    ILogger<PaymentController> logger)
{
    _paymentService = paymentService;
    _transactionService = transactionService;
    _logger = logger;
}
```

## Error Handling

- Let services throw domain exceptions; controllers translate to HTTP responses
- Return **`BadRequest`** for validation errors
- Return **`NotFound`** for missing resources
- Return **`Unauthorized`** / **`Forbidden`** for auth/permission issues
- Return **`InternalServerError`** (500) sparingly; log first

```csharp
try
{
    return Ok(await _service.DoWorkAsync(id, cancellationToken));
}
catch (ResourceNotFoundException ex)
{
    _logger.LogWarning(ex, "Resource not found: {Id}", id);
    return NotFound(new { message = ex.Message });
}
catch (UnauthorizedAccessException ex)
{
    _logger.LogWarning(ex, "Forbidden access");
    return Forbid();
}
```

## Response Types

- Use **DTOs** (Data Transfer Objects) for responses, not domain models
- Use **ProblemDetails** or custom error response DTOs for errors
- Include `[Produces]` and `[Consumes]` attributes for content types
- JSON responses must follow consistent naming: `camelCase` properties

## Testing Considerations

- Controllers should be thin; most logic in services for easier unit testing
- Avoid complex conditional logic in controller actions
- Each public action should have clear request → response flow
- Consider testability when choosing between `Ok()` and custom response helpers
