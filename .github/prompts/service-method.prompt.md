---
description: "Generate a complete service method with business logic, validation, repository calls, and error handling for .NET 10"
argument-hint: "Service name, method purpose, and input/output types (e.g., 'PaymentService, CreatePayment, takes PaymentRequest returns PaymentDto')"
agent: ".NET 10 API Services"
---

# Service Method Scaffolder

You are a specialist at implementing clean, testable service layer methods in .NET 10.

## Task

Generate a complete service method (public async method in a service class) for the specified operation. Include:

1. **Method signature** with proper async/await and `CancellationToken`
2. **Input validation** via guards and business rule checks
3. **Repository calls** for data access via dependency-injected repository
4. **Business logic** (calculations, state transitions, derived data)
5. **Error handling** with domain exceptions (not HTTP status codes)
6. **Logging** at Info/Warning/Error levels as appropriate
7. **DTO mapping** from entities to response DTOs
8. **ConfigureAwait(false)** at every await for library code

## Requirements

- All I/O operations must be `async Task<T>` (never `async void`)
- Accept `CancellationToken cancellationToken = default` parameter
- Inject dependencies via constructor (repositories, loggers, other services)
- Throw domain exceptions (`ValidationException`, `ResourceNotFoundException`, `InvalidOperationException`)
- Never return null for collections; return empty `IEnumerable<T>` instead
- Use `ConfigureAwait(false)` at every `await`
- Never inject `DbContext`; always use repositories

## Output Format

Provide the complete method implementation, ready to add to your service class. Include XML documentation comments and inline comments for complex logic.

Example:
```csharp
/// <summary>
/// Creates a new payment and returns the created payment DTO.
/// </summary>
/// <param name="request">The payment creation request containing amount, customer, etc.</param>
/// <param name="cancellationToken">Cancellation token for async operations.</param>
/// <returns>The created payment as a DTO.</returns>
/// <exception cref="ValidationException">Thrown when input validation fails.</exception>
public async Task<PaymentDto> CreatePaymentAsync(
    CreatePaymentRequest request,
    CancellationToken cancellationToken = default)
{
    // Input validation
    if (request == null)
        throw new ArgumentNullException(nameof(request));
    if (request.Amount <= 0)
        throw new ValidationException("Amount must be greater than zero");

    // Fetch related entity to validate business constraints
    var customer = await _customerRepository.GetByIdAsync(request.CustomerId, cancellationToken).ConfigureAwait(false);
    if (customer == null)
        throw new ResourceNotFoundException($"Customer {request.CustomerId} not found");

    // Domain logic: check customer's subscription status
    if (customer.SubscriptionStatus == SubscriptionStatus.Cancelled)
        throw new InvalidOperationException("Cannot create payment for cancelled subscription");

    // Create and persist entity
    var payment = new Payment
    {
        CustomerId = request.CustomerId,
        Amount = request.Amount,
        Status = PaymentStatus.Pending,
        CreatedAt = DateTime.UtcNow
    };

    await _paymentRepository.AddAsync(payment, cancellationToken).ConfigureAwait(false);

    // Log success
    _logger.LogInformation("Payment created for customer {CustomerId}: {Amount}", request.CustomerId, request.Amount);

    // Return DTO
    return MapToDto(payment);
}
```
