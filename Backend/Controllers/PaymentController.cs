using Microsoft.AspNetCore.Mvc;
using SportMania.Repository.Interface;

namespace SportMania.Controllers;

[ApiController]
[Route("api/payments")]
public class PaymentController (ITransactionRepository _transactionRepository) : ControllerBase
{
    [HttpGet("complete/{transactionId:guid}")]
    public async Task<IActionResult> PaymentComplete(Guid transactionId)
    {
        var transaction = await _transactionRepository.GetTransactionByIdAsync(transactionId);
        if (transaction == null || transaction.PaymentStatus != "Success")
        {
            return NotFound("Transaction not found or not successful.");
        }

        return Ok(transaction);
    }
}
