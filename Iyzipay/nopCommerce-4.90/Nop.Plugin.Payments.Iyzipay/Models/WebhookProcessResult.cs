namespace Nop.Plugin.Payments.Iyzipay.Models;

/// <summary>
/// Webhook process result
/// </summary>
public class WebhookProcessResult
{
    /// <summary>
    /// Gets or sets a value indicating whether the operation was successful
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Gets or sets the error message
    /// </summary>
    public string Message { get; set; } = string.Empty;
}