namespace SportMania.Models.Requests;

public class RequestToyyibPay
{
    public string UserSecretKey { get; set; } = string.Empty;
    public string CategoryCode { get; set; } = string.Empty;
    public string BillName { get; set; } = string.Empty;
    public string BillDescription { get; set; } = string.Empty;
    public int BillPriceSetting { get; set; } = 1;
    public int BillPayorInfo { get; set; } = 1;
    public int BillAmount { get; set; }
    public string BillReturnUrl { get; set; } = string.Empty;
    public string BillCallbackUrl { get; set; } = string.Empty;
    public string BillExternalReferenceNo { get; set; } = string.Empty;
    public string BillTo { get; set; } = string.Empty;
    public string BillEmail { get; set; } = string.Empty;
    public string BillPhone { get; set; } = string.Empty;
    public int BillSplitPayment { get; set; } = 0;    
    public string? BillSplitPaymentArgs { get; set; }
    public int BillPaymentChannel { get; set; } = 0;
    public string? BillContentEmail { get; set; }
    public string? BillChargeToCustomer { get; set; }
    public int? BillChargeToPrepaid { get; set; }
    public string? BillExpiryDate { get; set; }
    public int? BillExpiryDays { get; set; }
    public int? EnableFPXB2B { get; set; }
    public int? ChargeFPXB2B { get; set; }
    
    public List<KeyValuePair<string, string>> ToFormData()
    {
        var formData = new List<KeyValuePair<string, string>>
        {
            new("userSecretKey", UserSecretKey),
            new("categoryCode", CategoryCode),
            new("billName", BillName),
            new("billDescription", BillDescription),
            new("billPriceSetting", BillPriceSetting.ToString()),
            new("billPayorInfo", BillPayorInfo.ToString()),
            new("billAmount", BillAmount.ToString()),
            new("billReturnUrl", BillReturnUrl),
            new("billCallbackUrl", BillCallbackUrl),
            new("billExternalReferenceNo", BillExternalReferenceNo),
            new("billTo", BillTo),
            new("billEmail", BillEmail),
            new("billPhone", BillPhone),
            new("billSplitPayment", BillSplitPayment.ToString()),
            new("billPaymentChannel", BillPaymentChannel.ToString())
        };

        // Add optional fields if they have values
        if (!string.IsNullOrEmpty(BillSplitPaymentArgs))
            formData.Add(new("billSplitPaymentArgs", BillSplitPaymentArgs));

        if (!string.IsNullOrEmpty(BillContentEmail))
            formData.Add(new("billContentEmail", BillContentEmail));

        if (!string.IsNullOrEmpty(BillChargeToCustomer))
            formData.Add(new("billChargeToCustomer", BillChargeToCustomer));

        if (BillChargeToPrepaid.HasValue)
            formData.Add(new("billChargeToPrepaid", BillChargeToPrepaid.Value.ToString()));

        if (!string.IsNullOrEmpty(BillExpiryDate))
            formData.Add(new("billExpiryDate", BillExpiryDate));

        if (BillExpiryDays.HasValue)
            formData.Add(new("billExpiryDays", BillExpiryDays.Value.ToString()));

        if (EnableFPXB2B.HasValue)
            formData.Add(new("enableFPXB2B", EnableFPXB2B.Value.ToString()));

        if (ChargeFPXB2B.HasValue)
            formData.Add(new("chargeFPXB2B", ChargeFPXB2B.Value.ToString()));

        return formData;
    }
}