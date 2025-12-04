namespace Nop.Plugin.Payments.Iyzipay.Models;

/// <summary>
/// Iyzipay item transaction
/// </summary>
public class IyzipayItemTransaction
{
    /// <summary>
    /// Gets or sets the item ID
    /// </summary>
    public string ItemId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the payment transaction ID
    /// </summary>
    public string PaymentTransactionId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the transaction status
    /// </summary>
    public int TransactionStatus { get; set; }

    /// <summary>
    /// Gets or sets the price
    /// </summary>
    public decimal Price { get; set; }

    /// <summary>
    /// Gets or sets the paid price
    /// </summary>
    public decimal PaidPrice { get; set; }

    /// <summary>
    /// Gets or sets the merchant commission rate
    /// </summary>
    public decimal MerchantCommissionRate { get; set; }

    /// <summary>
    /// Gets or sets the merchant commission rate amount
    /// </summary>
    public decimal MerchantCommissionRateAmount { get; set; }

    /// <summary>
    /// Gets or sets the Iyzi commission rate amount
    /// </summary>
    public decimal IyziCommissionRateAmount { get; set; }

    /// <summary>
    /// Gets or sets the Iyzi commission fee
    /// </summary>
    public decimal IyziCommissionFee { get; set; }

    /// <summary>
    /// Gets or sets the merchant payout amount
    /// </summary>
    public decimal MerchantPayoutAmount { get; set; }
}
