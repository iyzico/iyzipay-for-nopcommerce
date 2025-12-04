namespace Nop.Plugin.Payments.Iyzipay.Models;

/// <summary>
/// Checkout form initialization result
/// </summary>
public class CheckoutFormInitializeResult
{
    /// <summary>
    /// Gets or sets a value indicating whether the operation was successful
    /// </summary>
    public string Success { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the error message
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the error message (alias for Message)
    /// </summary>
    public string ErrorMessage { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the checkout form content
    /// </summary>
    public string CheckoutFormContent { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the payment page URL
    /// </summary>
    public string PaymentPageUrl { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the token
    /// </summary>
    public string Token { get; set; } = string.Empty;
}