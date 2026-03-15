using SportMania.Models.Requests;
using SportMania.Models;

namespace SportMania.Services.Interface
{
    public interface IToyyibPayService
    {
        Task<(bool IsSuccess, string Result)> CreateBillAsync(RequestToyyibPay request);
        RequestToyyibPay BuildRequest(
            string categoryCode,
            string billName,
            string billDescription,
            int billAmount,
            string returnUrl,
            string externalReferenceNo,
            string customerEmail,
            string customerPhone,
            Plan plan,
            Key key);
    }
}