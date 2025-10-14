using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Payments.Iyzipay.Models;

/// <summary>
/// Represents Iyzipay payment configuration model
/// </summary>
public record ConfigurationModel : BaseNopModel
{
    [NopResourceDisplayName("Plugins.Payments.Iyzipay.Fields.ApiKey")]
    public string ApiKey { get; set; } = string.Empty;

    [NopResourceDisplayName("Plugins.Payments.Iyzipay.Fields.SecretKey")]
    public string SecretKey { get; set; } = string.Empty;

    [NopResourceDisplayName("Plugins.Payments.Iyzipay.Fields.BaseUrl")]
    public string BaseUrl { get; set; } = "https://sandbox-api.iyzipay.com";

    [NopResourceDisplayName("Plugins.Payments.Iyzipay.Fields.Environment")]
    public int EnvironmentId { get; set; } = 0; // 0: Sandbox, 1: Live

    [NopResourceDisplayName("Plugins.Payments.Iyzipay.Fields.UseSandbox")]
    public bool UseSandbox { get; set; } = true;

    [NopResourceDisplayName("Plugins.Payments.Iyzipay.Fields.WebhookUrl")]
    public string WebhookUrl { get; set; } = string.Empty;

    [NopResourceDisplayName("Plugins.Payments.Iyzipay.Fields.OrderStatusAfterPayment")]
    public int OrderStatusAfterPayment { get; set; } = 20;

    [NopResourceDisplayName("Plugins.Payments.Iyzipay.Fields.PaymentFormLanguage")]
    public string PaymentFormLanguage { get; set; } = "tr";

    [NopResourceDisplayName("Plugins.Payments.Iyzipay.Fields.EnableProtectedShopView")]
    public bool EnableProtectedShopView { get; set; } = false;

    [NopResourceDisplayName("Plugins.Payments.Iyzipay.Fields.ProtectedShopViewPosition")]
    public string ProtectedShopViewPosition { get; set; } = "left";

    [NopResourceDisplayName("Plugins.Payments.Iyzipay.Fields.PaymentFormMode")]
    public string PaymentFormMode { get; set; } = "iframe";

    [NopResourceDisplayName("Plugins.Payments.Iyzipay.Fields.PaymentOptionTitle")]
    public string PaymentOptionTitle { get; set; } = string.Empty;

    [NopResourceDisplayName("Plugins.Payments.Iyzipay.Fields.Enable3DSecure")]
    public bool Enable3DSecure { get; set; } = true;

    [NopResourceDisplayName("Plugins.Payments.Iyzipay.Fields.EnableInstallmentOptions")]
    public bool EnableInstallmentOptions { get; set; } = true;

    [NopResourceDisplayName("Plugins.Payments.Iyzipay.Fields.MaxInstallmentCount")]
    public int MaxInstallmentCount { get; set; } = 12;

    [NopResourceDisplayName("Plugins.Payments.Iyzipay.Fields.PassProductNamesAndTotals")]
    public bool PassProductNamesAndTotals { get; set; } = true;

    [NopResourceDisplayName("Plugins.Payments.Iyzipay.Fields.SendInvoiceInfo")]
    public bool SendInvoiceInfo { get; set; } = true;

    [NopResourceDisplayName("Plugins.Payments.Iyzipay.Fields.AdditionalFee")]
    public decimal AdditionalFee { get; set; } = 0;

    [NopResourceDisplayName("Plugins.Payments.Iyzipay.Fields.AdditionalFeePercentage")]
    public bool AdditionalFeePercentage { get; set; } = false;

    public IList<SelectListItem> OrderStatusValues { get; set; } = new List<SelectListItem>();
    public IList<SelectListItem> PaymentFormLanguageValues { get; set; } = new List<SelectListItem>();
    public IList<SelectListItem> PaymentFormModeValues { get; set; } = new List<SelectListItem>();
    public IList<SelectListItem> ProtectedShopViewPositionValues { get; set; } = new List<SelectListItem>();
    public IList<SelectListItem> EnvironmentValues { get; set; } = new List<SelectListItem>();
}
