---
description: "Use when implementing service layer logic. Covers async patterns, business logic encapsulation, dependency injection, error handling, and testable service design in .NET 10."
applyTo: "Services/**/*.cs"
---

# Service Layer Guidelines (.NET 10)

## Structure & Naming

- **Class naming**: `{Resource}Service` (e.g., `PaymentService`, `KeyService`)
- **Interface naming**: `I{Resource}Service` (e.g., `IPaymentService`)
- **Single responsibility**: Each service owns one domain area
- **Stateless**: Services are stateless and thread-safe; no instance fields except dependencies

## Interface Design

```csharp
public interface IPaymentService
{
    Task<PaymentDto> GetPaymentAsync(int id, CancellationToken cancellationToken = default);
    Task<PaymentDto> CreatePaymentAsync(CreatePaymentRequest request, CancellationToken cancellationToken = default);
    Task UpdatePaymentAsync(int id, UpdatePaymentRequest request, CancellationToken cancellationToken = default);
    Task DeletePaymentAsync(int id, CancellationToken cancellationToken = default);
}
```

**Rules**:
- All I/O methods return `Task<T>` or `Task`
- Include `CancellationToken` parameter with default value
- Use DTOs for inputs/outputs, not domain entities
- Define one method per responsibility; keep interfaces focused

## Async & Cancellation

- All async methods must accept `CancellationToken cancellationToken = default`
- Pass token to all downstream calls: repositories, external services, HTTP clients
- Use `ConfigureAwait(false)` at every `await` for library/service code
- Throw `OperationCanceledException` only for user-initiated cancellations, not unhandled timeouts

```csharp
public async Task<PaymentDto> CreatePaymentAsync(
    CreatePaymentRequest request,
    CancellationToken cancellationToken = default)
{
    // Validate input
    var validation = ValidateRequest(request);
    if (!validation.IsValid)
        throw new ValidationException(validation.Errors);

    // Call repository with token
    var entity = new Payment { /* map request */ };
    await _repository.AddAsync(entity, cancellationToken).ConfigureAwait(false);

    // Return DTO
    return MapToDto(entity);
}
```

## Dependency Injection

- Inject **repositories** (data access) and **other services** via constructor
- Never inject `DbContext` directly—always use repositories
- Use interface contracts (`IPaymentRepository`, `IKeyService`)
- Enable constructor-based dependency injection for testability

```csharp
public class PaymentService : IPaymentService
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly ITransactionService _transactionService;
    private readonly ILogger<PaymentService> _logger;

    public PaymentService(
        IPaymentRepository paymentRepository,
        ITransactionService transactionService,
        ILogger<PaymentService> logger)
    {
        _paymentRepository = paymentRepository;
        _transactionService = transactionService;
        _logger = logger;
    }
}
```

## Business Logic & Validation

- **Input validation** happens first (model state, required fields)
- **Domain validation** checks business rules (constraints, state transitions)
- **External calls** (APIs, DB) happen after validation passes
- Raise **domain exceptions** for recoverable errors; let controllers translate to HTTP

```csharp
public async Task UpdatePaymentAsync(
    int id,
    UpdatePaymentRequest request,
    CancellationToken cancellationToken = default)
{
    // Input validation
    if (request?.Amount <= 0)
        throw new ArgumentException("Amount must be positive");

    // Fetch existing entity
    var payment = await _repository.GetByIdAsync(id, cancellationToken).ConfigureAwait(false);
    if (payment == null)
        throw new ResourceNotFoundException($"Payment {id} not found");

    // Domain validation
    if (payment.Status == PaymentStatus.Completed)
        throw new InvalidOperationException("Cannot update completed payment");

    // Update & persist
    payment.Amount = request.Amount;
    await _repository.UpdateAsync(payment, cancellationToken).ConfigureAwait(false);
}
```

## Error Handling

- Define **domain exceptions** in the Models/Exceptions folder
- Services throw exceptions; controllers catch and translate to HTTP responses
- Log at appropriate levels: `Info` for expected scenarios, `Warning` for recoverable issues, `Error` for unexpected failures
- Avoid swallowing exceptions; always re-throw or log

```csharp
public class ResourceNotFoundException : Exception
{
    public ResourceNotFoundException(string message) : base(message) { }
}

public class InvalidOperationException : Exception
{
    public InvalidOperationException(string message) : base(message) { }
}
```

## External Service Integration

- Encapsulate external API calls (ToyyibPay, Discord, etc.) in dedicated services
- Use retry policies (Polly) for resilience
- Handle timeout/network errors gracefully
- Mock external services in unit tests

```csharp
public async Task<PaymentResponse> ProcessToyyibPayAsync(
    PaymentRequest request,
    CancellationToken cancellationToken = default)
{
    try
    {
        var response = await _toyyibPayService.CreateInvoiceAsync(request, cancellationToken).ConfigureAwait(false);
        _logger.LogInformation("ToyyibPay invoice created: {InvoiceId}", response.InvoiceId);
        return response;
    }
    catch (HttpRequestException ex)
    {
        _logger.LogError(ex, "ToyyibPay API error");
        throw new ExternalServiceException("Payment processing failed", ex);
    }
}
```

## Testing Considerations

- Services should be fully unit-testable with mocked dependencies
- Avoid static dependencies; always inject interfaces
- Minimize side effects; pure functions where possible
- Use consistent DTO mapping logic (consider a separate mapper class)
