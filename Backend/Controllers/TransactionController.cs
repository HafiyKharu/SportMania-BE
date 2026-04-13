using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportMania.Models.Requests;
using SportMania.Models.Responses;
using SportMania.Repository.Interface;
using SportMania.Services.Interface;

namespace SportMania.Controllers;

[ApiController]
[Route("api/transactions")]
public class TransactionController (ITransactionService _transactionService, ITransactionRepository _transactionRepository, IConfiguration _configuration, ILogger<TransactionController> _logger) : ControllerBase
{
    [HttpPost("initiate-payment")]
    [AllowAnonymous]
    public async Task<IActionResult> InitiatePayment([FromBody] RequestInitiatePayment req, CancellationToken cancellationToken)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Backend callback URL for processing payment status
            var callbackUrl = BuildBackendCallbackUrl();

            if (string.IsNullOrEmpty(callbackUrl))
                return StatusCode(500, "Could not generate payment callback URL.");

            var requestTransaction = new RequestTransaction
            {
                Email = req.Email,
                PlanId = req.PlanId,
                PhoneNumber = req.PhoneNumber
            };

            var (isSuccess, result) = await _transactionService.InitiatePaymentAsync(requestTransaction, callbackUrl, cancellationToken);

            return isSuccess ? Ok(new { redirectUrl = result }) : BadRequest(new { error = result });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    [HttpGet("payment-callback")]
    [AllowAnonymous]
    public async Task<IActionResult> PaymentCallback(Guid transactionId, string status_id, CancellationToken cancellationToken)
    {
        try
        {
            var transaction = await _transactionService.ProcessPaymentCallbackAsync(transactionId, status_id, cancellationToken);

            if (transaction == null)
                return NotFound("Transaction not found.");

            // Redirect to Frontend based on payment status
            var frontendBaseUrl = GetFrontendBaseUrl();
            var frontendUrl = transaction.PaymentStatus == "Success"
                ? $"{frontendBaseUrl}/transactions/success/{transaction.TransactionId}"
                : $"{frontendBaseUrl}/transactions/failed/{transaction.TransactionId}";

            return Redirect(frontendUrl);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        try
        {
            var transactions = await _transactionRepository.GetAllTransactionsAsync(cancellationToken);
            return Ok(transactions);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    [HttpGet("{transactionId:guid}")]
    [AllowAnonymous]
    /// <summary>
    /// Retrieves a transaction by identifier and returns the license key only on first successful view.
    /// </summary>
    /// <remarks>
    /// If payment status is <c>Success</c> and <c>IsKeyViewed</c> is <c>false</c>, the response includes
    /// <see cref="TransactionViewResponse.LicenseKey"/> and the transaction is marked as viewed.
    /// Subsequent calls return the same transaction metadata without exposing the key again.
    /// </remarks>
    /// <param name="transactionId">The unique transaction identifier from route.</param>
    /// <param name="cancellationToken">Cancellation token for propagating request cancellation to service and repository layers.</param>
    /// <returns>
    /// A <see cref="TransactionViewResponse"/> with one-time key visibility rules applied, or 404 when not found.
    /// Returns 500 for unexpected server failures.
    /// </returns>
    /// <exception cref="OperationCanceledException">
    /// Thrown by downstream async operations when <paramref name="cancellationToken"/> is canceled before completion.
    /// </exception>
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(TransactionViewResponse))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<TransactionViewResponse>> GetById([FromRoute] Guid transactionId, CancellationToken cancellationToken)
    {
        try
        {
            var transaction = await _transactionService.GetTransactionForViewAsync(transactionId, cancellationToken);

            if (transaction == null)
            {
                _logger.LogWarning("Transaction not found for id {TransactionId}", transactionId);
                return NotFound("Transaction not found.");
            }

            _logger.LogInformation("Transaction {TransactionId} retrieved. IsKeyViewed={IsKeyViewed}", transactionId, transaction.IsKeyViewed);
            return Ok(transaction);
        }
        catch (Exception ex)
        {
            // Unexpected errors are logged and mapped to 500.
            _logger.LogError(ex, "Error retrieving transaction by id {TransactionId}", transactionId);
            return StatusCode(StatusCodes.Status500InternalServerError, "Internal server error.");
        }
    }

    private string BuildBackendCallbackUrl()
    {
        var backendBaseUrl = (_configuration["Backend:PublicBaseUrl"] ?? $"{Request.Scheme}://{Request.Host}").TrimEnd('/');
        return $"{backendBaseUrl}/api/transactions/payment-callback";
    }

    private string GetFrontendBaseUrl()
    {
        return (_configuration["Frontend:BaseUrl"] ?? "http://localhost:3000").TrimEnd('/');
    }
}