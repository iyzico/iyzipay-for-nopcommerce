using System;
using Nop.Web.Framework.Models;

namespace Nop.Plugin.Payments.Iyzipay.Models;

/// <summary>
/// Represents Iyzipay payment info model
/// </summary>
public record PaymentInfoModel : BaseNopModel
{
    /// <summary>
    /// Gets or sets payment description text
    /// </summary>
    public string DescriptionText { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets payment option title
    /// </summary>
    public string PaymentOptionTitle { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets payment form mode
    /// </summary>
    public string PaymentFormMode { get; set; } = "iframe";

    /// <summary>
    /// Gets or sets payment form language
    /// </summary>
    public string PaymentFormLanguage { get; set; } = "tr";

    /// <summary>
    /// Gets or sets order GUID for payment processing
    /// </summary>
    public Guid OrderGuid { get; set; }

    /// <summary>
    /// Gets or sets customer email for validation
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets customer phone for validation
    /// </summary>
    public string Phone { get; set; } = string.Empty;
}
