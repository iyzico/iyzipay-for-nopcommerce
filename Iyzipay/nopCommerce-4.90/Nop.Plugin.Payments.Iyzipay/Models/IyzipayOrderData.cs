using System.Collections.Generic;
using System.Text.Json;

namespace Nop.Plugin.Payments.Iyzipay.Models;

/// <summary>
/// Iyzipay order data
/// </summary>
public class IyzipayOrderData
{
    /// <summary>
    /// Gets or sets the payment ID
    /// </summary>
    public string PaymentId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the conversation ID
    /// </summary>
    public string ConversationId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the token
    /// </summary>
    public string Token { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the basket ID
    /// </summary>
    public string BasketId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the payment status
    /// </summary>
    public string PaymentStatus { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the fraud status
    /// </summary>
    public int FraudStatus { get; set; }

    /// <summary>
    /// Gets or sets the card type
    /// </summary>
    public string CardType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the card association
    /// </summary>
    public string CardAssociation { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the card family
    /// </summary>
    public string CardFamily { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the BIN number
    /// </summary>
    public string BinNumber { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the last four digits
    /// </summary>
    public string LastFourDigits { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the authorization code
    /// </summary>
    public string AuthCode { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the phase
    /// </summary>
    public string Phase { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the MD status
    /// </summary>
    public int MdStatus { get; set; }

    /// <summary>
    /// Gets or sets the host reference
    /// </summary>
    public string HostReference { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the item transactions
    /// </summary>
    public List<IyzipayItemTransaction> ItemTransactions { get; set; } = new();

    /// <summary>
    /// Convert to JSON
    /// </summary>
    /// <returns>JSON string</returns>
    public string ToJson()
    {
        return JsonSerializer.Serialize(this);
    }

    /// <summary>
    /// Create from JSON
    /// </summary>
    /// <param name="json">JSON string</param>
    /// <returns>IyzipayOrderData instance</returns>
    public static IyzipayOrderData FromJson(string json)
    {
        if (string.IsNullOrEmpty(json))
            return new IyzipayOrderData();

        try
        {
            return JsonSerializer.Deserialize<IyzipayOrderData>(json) ?? new IyzipayOrderData();
        }
        catch
        {
            return new IyzipayOrderData();
        }
    }
}