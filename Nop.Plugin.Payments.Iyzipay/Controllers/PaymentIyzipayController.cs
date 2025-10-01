using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Plugin.Payments.Iyzipay.Models;
using Nop.Plugin.Payments.Iyzipay.Services;
using Nop.Services;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;
using System.Text.Json;

namespace Nop.Plugin.Payments.Iyzipay.Controllers;

[AuthorizeAdmin]
[Area(AreaNames.ADMIN)]
[AutoValidateAntiforgeryToken]
public class PaymentIyzipayController : BasePaymentController
{
    #region Fields

    private readonly ILocalizationService _localizationService;
    private readonly INotificationService _notificationService;
    private readonly IOrderService _orderService;
    private readonly ISettingService _settingService;
    private readonly IWebHelper _webHelper;
    private readonly IyzipayPaymentSettings _iyzipayPaymentSettings;
    private readonly IyzipayPaymentProcessor _iyzipayPaymentProcessor;
    private readonly IyzipayCheckoutFormService _checkoutFormService;

    #endregion

    #region Ctor

    public PaymentIyzipayController(ILocalizationService localizationService,
        INotificationService notificationService,
        IOrderService orderService,
        ISettingService settingService,
        IWebHelper webHelper,
        IyzipayPaymentSettings iyzipayPaymentSettings,
        IyzipayPaymentProcessor iyzipayPaymentProcessor,
        IyzipayCheckoutFormService checkoutFormService)
    {
        _localizationService = localizationService;
        _notificationService = notificationService;
        _orderService = orderService;
        _settingService = settingService;
        _webHelper = webHelper;
        _iyzipayPaymentSettings = iyzipayPaymentSettings;
        _iyzipayPaymentProcessor = iyzipayPaymentProcessor;
        _checkoutFormService = checkoutFormService;
    }

    #endregion

    #region Methods

    public async Task<IActionResult> Configure()
    {
        var webhookUrl = $"{_webHelper.GetStoreLocation()}Plugins/PaymentIyzipay/Webhook";

        var model = new ConfigurationModel
        {
            ApiKey = _iyzipayPaymentSettings.ApiKey,
            SecretKey = _iyzipayPaymentSettings.SecretKey,
            BaseUrl = _iyzipayPaymentSettings.BaseUrl,
            EnvironmentId = _iyzipayPaymentSettings.BaseUrl.Contains("sandbox") ? 0 : 1,
            UseSandbox = _iyzipayPaymentSettings.UseSandbox,
            WebhookUrl = webhookUrl,
            OrderStatusAfterPayment = _iyzipayPaymentSettings.OrderStatusAfterPayment,
            PaymentFormLanguage = _iyzipayPaymentSettings.PaymentFormLanguage,
            EnableProtectedShopView = _iyzipayPaymentSettings.EnableProtectedShopView,
            ProtectedShopViewPosition = _iyzipayPaymentSettings.ProtectedShopViewPosition,
            PaymentFormMode = _iyzipayPaymentSettings.PaymentFormMode,
            PaymentOptionTitle = _iyzipayPaymentSettings.PaymentOptionTitle,
            Enable3DSecure = _iyzipayPaymentSettings.Enable3DSecure,
            EnableInstallmentOptions = _iyzipayPaymentSettings.EnableInstallmentOptions,
            MaxInstallmentCount = _iyzipayPaymentSettings.MaxInstallmentCount,
            PassProductNamesAndTotals = _iyzipayPaymentSettings.PassProductNamesAndTotals,
            SendInvoiceInfo = _iyzipayPaymentSettings.SendInvoiceInfo,
            AdditionalFee = _iyzipayPaymentSettings.AdditionalFee,
            AdditionalFeePercentage = _iyzipayPaymentSettings.AdditionalFeePercentage
        };

        // Fill select lists
        await PrepareModelAsync(model);

        return View("~/Plugins/Payments.Iyzipay/Views/Configure.cshtml", model);
    }

    [HttpPost]
    public async Task<IActionResult> Configure(ConfigurationModel model)
    {
        if (!ModelState.IsValid)
            return await Configure();

        _iyzipayPaymentSettings.ApiKey = model.ApiKey;
        _iyzipayPaymentSettings.SecretKey = model.SecretKey;
        _iyzipayPaymentSettings.BaseUrl = model.EnvironmentId == 0 ? "https://sandbox-api.iyzipay.com" : "https://api.iyzipay.com";
        _iyzipayPaymentSettings.UseSandbox = model.EnvironmentId == 0;
        _iyzipayPaymentSettings.OrderStatusAfterPayment = model.OrderStatusAfterPayment;
        _iyzipayPaymentSettings.PaymentFormLanguage = model.PaymentFormLanguage;
        _iyzipayPaymentSettings.EnableProtectedShopView = model.EnableProtectedShopView;
        _iyzipayPaymentSettings.ProtectedShopViewPosition = model.ProtectedShopViewPosition;
        _iyzipayPaymentSettings.PaymentFormMode = model.PaymentFormMode;
        _iyzipayPaymentSettings.PaymentOptionTitle = model.PaymentOptionTitle;
        _iyzipayPaymentSettings.Enable3DSecure = model.Enable3DSecure;
        _iyzipayPaymentSettings.EnableInstallmentOptions = model.EnableInstallmentOptions;
        _iyzipayPaymentSettings.MaxInstallmentCount = model.MaxInstallmentCount;
        _iyzipayPaymentSettings.PassProductNamesAndTotals = model.PassProductNamesAndTotals;
        _iyzipayPaymentSettings.SendInvoiceInfo = model.SendInvoiceInfo;
        _iyzipayPaymentSettings.AdditionalFee = model.AdditionalFee;
        _iyzipayPaymentSettings.AdditionalFeePercentage = model.AdditionalFeePercentage;

        await _settingService.SaveSettingAsync(_iyzipayPaymentSettings);

        _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Plugins.Saved"));

        return await Configure();
    }


    private async Task PrepareModelAsync(ConfigurationModel model)
    {
        // OrderStatus values
        model.OrderStatusValues = new List<SelectListItem>
        {
            new SelectListItem { Value = "10", Text = await _localizationService.GetResourceAsync("Admin.Orders.OrderStatus.Pending") },
            new SelectListItem { Value = "20", Text = await _localizationService.GetResourceAsync("Admin.Orders.OrderStatus.Processing") },
            new SelectListItem { Value = "30", Text = await _localizationService.GetResourceAsync("Admin.Orders.OrderStatus.Complete") },
            new SelectListItem { Value = "40", Text = await _localizationService.GetResourceAsync("Admin.Orders.OrderStatus.Cancelled") }
        };

        // PaymentFormLanguage values
        model.PaymentFormLanguageValues = new List<SelectListItem>
        {
            new SelectListItem { Value = "tr", Text = "Türkçe" },
            new SelectListItem { Value = "en", Text = "English" },
            new SelectListItem { Value = "auto", Text = await _localizationService.GetResourceAsync("Plugins.Payments.Iyzipay.Fields.PaymentFormLanguage.Auto") }
        };

        // PaymentFormMode values
        model.PaymentFormModeValues = new List<SelectListItem>
        {
            new SelectListItem { Value = "iframe", Text = "Iframe" },
            new SelectListItem { Value = "redirect", Text = "Redirect" },
            new SelectListItem { Value = "popup", Text = "Popup" }
        };

        // ProtectedShopViewPosition values
        model.ProtectedShopViewPositionValues = new List<SelectListItem>
        {
            new SelectListItem { Value = "left", Text = await _localizationService.GetResourceAsync("Plugins.Payments.Iyzipay.Fields.ProtectedShopViewPosition.Left") },
            new SelectListItem { Value = "right", Text = await _localizationService.GetResourceAsync("Plugins.Payments.Iyzipay.Fields.ProtectedShopViewPosition.Right") }
        };

        // Environment values
        model.EnvironmentValues = new List<SelectListItem>
        {
            new SelectListItem { Value = "0", Text = await _localizationService.GetResourceAsync("Plugins.Payments.Iyzipay.Fields.Environment.Sandbox") },
            new SelectListItem { Value = "1", Text = await _localizationService.GetResourceAsync("Plugins.Payments.Iyzipay.Fields.Environment.Live") }
        };
    }




    #endregion
}
