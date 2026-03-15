using SportMania.Models;
using SportMania.Models.Requests;
using SportMania.Repository.Interface;
using SportMania.Services.Interface;

namespace SportMania.Services;

public class TransactionService(
        ITransactionRepository _transactionRepository,
        ICustomerRepository _customerRepository,
        IPlanRepository _planRepository,
        IKeyService _keyService,
        IToyyibPayService _toyyibPayService,
        IConfiguration _configuration) : ITransactionService
{
    public async Task<(bool IsSuccess, string Result)> InitiatePaymentAsync(RequestTransaction req, string returnUrl)
    {
        try
        {
            var guildIdString = _configuration["Discord:DefaultGuildId"] ?? throw new Exception("Default Guild ID not configured. Please set 'Discord:DefaultGuildId' in appsetting.");
            if (string.IsNullOrWhiteSpace(req.Email) || string.IsNullOrWhiteSpace(req.PhoneNumber))
                throw new Exception("Email or Phone Number is empty.");

            var customer = await _customerRepository.GetCustomerByEmailAsync(req.Email);
            if (customer == null)
            {
                customer = await _customerRepository.CreateCustomerAsync(new Customer
                {
                    Email = req.Email,
                    PhoneNumber = req.PhoneNumber
                });
            }
            else if (string.IsNullOrWhiteSpace(customer.PhoneNumber))
            {
                customer.PhoneNumber = req.PhoneNumber;
                await _customerRepository.UpdateCustomerAsync(customer);
            }

            var transaction = new Transaction
            {
                Customer = customer,
                Plan = await _planRepository.GetByIdAsync(req.PlanId) ?? throw new Exception("Plan not found."),
                PaymentStatus = "Pending",
            };
            transaction.Amount = transaction.Plan.Price.ToString();

            if (!int.TryParse(transaction.Plan.Duration, out var duration))
            {
                throw new FormatException("Invalid plan duration format.");
            }

            //generate Key
            transaction.Key = await _keyService.GenerateKeyAsync(ulong.Parse(guildIdString), req.PlanId, duration);

            //Save Transaction
            var createdTransaction = await _transactionRepository.CreateTransactionAsync(transaction);

            // Prepare ToyyibPay request
            var categoryCode = transaction.Plan.CategoryCode;
            var finalReturnUrl = $"{returnUrl}?transactionId={createdTransaction.TransactionId}";
            var billAmount = ConvertPriceToCents(transaction.Plan.Price);

            var toyyibPayRequest = _toyyibPayService.BuildRequest(
                categoryCode: categoryCode,
                billName: transaction.Plan.Name,
                billDescription: $"Subscription for {transaction.Plan.Name}",
                billAmount: billAmount,
                returnUrl: finalReturnUrl,
                externalReferenceNo: createdTransaction.TransactionId.ToString(),
                customerEmail: transaction.Customer.Email,
                customerPhone: transaction.Customer.PhoneNumber,
                plan: transaction.Plan,
                key: transaction.Key
            );

            // Call ToyyibPay API
            return await _toyyibPayService.CreateBillAsync(toyyibPayRequest);
        }
        catch (Exception ex)
        {
            return (false, $"Error initiating payment: {ex.Message}");
        }
    }

    public async Task<Transaction> ProcessPaymentCallbackAsync(Guid transactionId, string statusId)
    {
        try
        {
            var transaction = await _transactionRepository.GetTransactionByIdAsync(transactionId) ?? throw new KeyNotFoundException($"Transaction with ID {transactionId} not found.");
            transaction.PaymentStatus = statusId switch
            {
                "1" => "Success",
                _ => "Failed",
            };
            await _transactionRepository.UpdateTransactionAsync(transaction);
            return transaction;
        }
        catch (Exception ex)
        {
            throw new ApplicationException($"Error processing payment callback: {ex.Message}");
        }
    }

    private static int ConvertPriceToCents(string price)
    {
        decimal priceValue = 0;
        decimal.TryParse(price.Replace("RM", "").Trim(), out priceValue);
        return (int)(priceValue * 100);
    }
}