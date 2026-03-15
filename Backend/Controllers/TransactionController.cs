using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportMania.Models.Requests;
using SportMania.Repository.Interface;
using SportMania.Services.Interface;

namespace SportMania.Controllers;

[ApiController]
[Route("api/transactions")]
public class TransactionController (ITransactionService _transactionService, ITransactionRepository _transactionRepository) : ControllerBase
{
    [HttpPost("initiate-payment")]
    [AllowAnonymous]
    public async Task<IActionResult> InitiatePayment([FromBody] RequestInitiatePayment req)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Backend callback URL for processing payment status
            var callbackUrl = Url.Action("PaymentCallback", "Transaction", null, Request.Scheme);

            if (string.IsNullOrEmpty(callbackUrl))
                return StatusCode(500, "Could not generate payment callback URL.");

            var requestTransaction = new RequestTransaction
            {
                Email = req.Email,
                PlanId = req.PlanId,
                PhoneNumber = req.PhoneNumber
            };

            var (isSuccess, result) = await _transactionService.InitiatePaymentAsync(requestTransaction, callbackUrl);

            return isSuccess ? Ok(new { redirectUrl = result }) : BadRequest(new { error = result });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    [HttpGet("payment-callback")]
    [AllowAnonymous]
    public async Task<IActionResult> PaymentCallback(Guid transactionId, string status_id)
    {
        try
        {
            var transaction = await _transactionService.ProcessPaymentCallbackAsync(transactionId, status_id);

            if (transaction == null)
                return NotFound("Transaction not found.");

            // Redirect to Frontend based on payment status
            var frontendUrl = transaction.PaymentStatus == "Success"
                ? $"http://localhost:5103/transactions/success/{transaction.TransactionId}"
                : $"http://localhost:5103/transactions/failed/{transaction.TransactionId}";

            return Redirect(frontendUrl);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var transactions = await _transactionRepository.GetAllTransactionsAsync();
            return Ok(transactions);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    [HttpGet("{transactionId:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetById(Guid transactionId)
    {
        try
        {
            var transaction = await _transactionRepository.GetTransactionByIdAsync(transactionId);

            if (transaction == null)
                return NotFound("Transaction not found.");

            return Ok(transaction);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }
}