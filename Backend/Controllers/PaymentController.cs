using Microsoft.AspNetCore.Mvc;
using SportMania.Models.Responses;
using SportMania.Repository.Interface;
using SportMania.Services.Interface;

namespace SportMania.Controllers;

[ApiController]
[Route("api/payments")]
public class PaymentController (ITransactionService _transactionService, ILogger<PaymentController> _logger) : ControllerBase
{
    /// <summary>
    /// Returns the payment completion payload for a successful transaction.
    /// </summary>
    /// <remarks>
    /// This endpoint reuses the one-time view contract: the license key is only included in the first successful
    /// retrieval when <c>IsKeyViewed</c> is still <c>false</c>. Failed or missing transactions return 404.
    /// </remarks>
    /// <param name="transactionId">The unique transaction identifier from route.</param>
    /// <param name="cancellationToken">Cancellation token for asynchronous service calls.</param>
    /// <returns>
    /// A <see cref="TransactionViewResponse"/> for successful transactions; otherwise 404.
    /// Returns 500 when an unexpected server-side error occurs.
    /// </returns>
    /// <exception cref="OperationCanceledException">
    /// Thrown when request processing is canceled through <paramref name="cancellationToken"/>.
    /// </exception>
    [HttpGet("complete/{transactionId:guid}")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(TransactionViewResponse))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<TransactionViewResponse>> PaymentComplete([FromRoute] Guid transactionId, CancellationToken cancellationToken)
    {
        try
        {
            var transaction = await _transactionService.GetTransactionForViewAsync(transactionId, cancellationToken);
            if (transaction == null || transaction.PaymentStatus != "Success")
            {
                return NotFound("Transaction not found or not successful.");
            }

            _logger.LogInformation("Payment completion fetched for transaction {TransactionId}. IsKeyViewed={IsKeyViewed}", transactionId, transaction.IsKeyViewed);
            return Ok(transaction);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving payment completion for transaction {TransactionId}", transactionId);
            return StatusCode(StatusCodes.Status500InternalServerError, "Internal server error.");
        }
    }
}
