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
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;
using System.Text.Json;

namespace Nop.Plugin.Payments.Iyzipay.Controllers;

/// <summary>
/// Public controller for frontend payment operations
/// </summary>
public class PaymentIyzipayPublicController : BaseController
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
    private readonly ICustomerService _customerService;
    private readonly IShoppingCartService _shoppingCartService;
    private readonly IOrderTotalCalculationService _orderTotalCalculationService;
    private readonly IWorkContext _workContext;
    private readonly IStoreContext _storeContext;

    #endregion

    #region Ctor

    public PaymentIyzipayPublicController(ILocalizationService localizationService,
        INotificationService notificationService,
        IOrderService orderService,
        ISettingService settingService,
        IWebHelper webHelper,
        IyzipayPaymentSettings iyzipayPaymentSettings,
        IyzipayPaymentProcessor iyzipayPaymentProcessor,
        IyzipayCheckoutFormService checkoutFormService,
        ICustomerService customerService,
        IShoppingCartService shoppingCartService,
        IOrderTotalCalculationService orderTotalCalculationService,
        IWorkContext workContext,
        IStoreContext storeContext)
    {
        _localizationService = localizationService;
        _notificationService = notificationService;
        _orderService = orderService;
        _settingService = settingService;
        _webHelper = webHelper;
        _iyzipayPaymentSettings = iyzipayPaymentSettings;
        _iyzipayPaymentProcessor = iyzipayPaymentProcessor;
        _checkoutFormService = checkoutFormService;
        _customerService = customerService;
        _shoppingCartService = shoppingCartService;
        _orderTotalCalculationService = orderTotalCalculationService;
        _workContext = workContext;
        _storeContext = storeContext;
    }

    #endregion

    #region Methods

    [HttpPost]
    [AllowAnonymous]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> ProcessPayment()
    {
        try
        {   
            var mode = Request.Query["mode"].FirstOrDefault() ?? Request.Form["mode"].FirstOrDefault();
            var orderGuid = Request.Query["orderGuid"].FirstOrDefault() ?? Request.Form["orderGuid"].FirstOrDefault();
            if (string.IsNullOrEmpty(mode) || string.IsNullOrEmpty(orderGuid))
            {
                return Json(new { success = false, message = "Missing required parameters" });
            }

            if (!Guid.TryParse(orderGuid, out var parsedOrderGuid))
            {
                return Json(new { success = false, message = "Invalid OrderGuid format" });
            }

            var customer = await _workContext.GetCurrentCustomerAsync();
            if (customer == null)
            {
                return Json(new { success = false, message = "Customer not found" });
            }

            var store = await _storeContext.GetCurrentStoreAsync();
            if (store == null)
            {
                return Json(new { success = false, message = "Store not found" });
            }

            var cart = await _shoppingCartService.GetShoppingCartAsync(customer, Core.Domain.Orders.ShoppingCartType.ShoppingCart, store.Id);
            if (!cart.Any())
            {
                return Json(new { success = false, message = "Shopping cart is empty" });
            }

            var orderTotal = await _orderTotalCalculationService.GetShoppingCartTotalAsync(cart);
            var processPaymentRequest = new ProcessPaymentRequest
            {
                OrderGuid = parsedOrderGuid,
                CustomerId = customer.Id,
                StoreId = store.Id,
                PaymentMethodSystemName = "Payments.IyzipayCheckoutForm",
                OrderTotal = orderTotal.shoppingCartTotal ?? 0
            };

            
            var processPaymentResult = await _iyzipayPaymentProcessor.ProcessPaymentAsync(processPaymentRequest);
            if (processPaymentResult.Success)
            {
                switch (mode.ToLower())
                {
                    case "iframe":
                    case "popup":
                        return Json(new
                        {
                            success = true,
                            mode = mode,
                            checkoutFormContent = processPaymentResult.AuthorizationTransactionResult,
                            token = parsedOrderGuid.ToString()
                        });

                    case "redirect":
                        return Redirect(processPaymentResult.AuthorizationTransactionResult);

                    default:
                        return Json(new { success = false, message = $"Unsupported payment mode: {mode}" });
                }
            }
            else
            {
                return Json(new { success = false, message = string.Join(", ", processPaymentResult.Errors) });
            }
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    [HttpPost, HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> Confirmation()
    {
        try
        {   
            var token = Request.Form["token"].FirstOrDefault();
            if (string.IsNullOrEmpty(token))
            {
                return BadRequest(new { message = "Missing required parameters - no token found" });
            }

            var result = await _iyzipayPaymentProcessor.ProcessConfirmationAsync(token);
            if (result.Success)
            {
                return RedirectToAction("Completed", "Checkout", new { orderId = result.OrderId });
            }
            else
            {
                return BadRequest(new { message = result.ErrorMessage });
            }

        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> Webhook()
    {
        try
        {
            using var reader = new StreamReader(Request.Body);

            var webhookData = await reader.ReadToEndAsync();
            var signature = Request.Headers["X-Iyz-Signature-V3"].FirstOrDefault();
            
            var webhookResult = JsonSerializer.Deserialize<Dictionary<string, object>>(webhookData);
            if (webhookResult == null)
            {
                return BadRequest(new { message = "Missing required parameters" });
            }

            var result = await _iyzipayPaymentProcessor.ProcessWebhookAsync(webhookResult, signature);
            return result.Success ? Ok() : BadRequest(new { message = result.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    #endregion
}
