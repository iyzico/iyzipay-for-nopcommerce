using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Plugin.Payments.Iyzipay.Components;
using Nop.Plugin.Payments.Iyzipay.Models;
using Nop.Plugin.Payments.Iyzipay.Services;
using Nop.Services.Configuration;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Plugins;
using Nop.Web.Framework.Validators;
using Iyzipay;
using Iyzipay.Model;
using Iyzipay.Request;

namespace Nop.Plugin.Payments.Iyzipay;

/// <summary>
/// Iyzipay payment processor
/// </summary>
public class IyzipayPaymentProcessor : BasePlugin, IPaymentMethod
{
    #region Fields

    protected readonly ILocalizationService _localizationService;
    protected readonly IOrderTotalCalculationService _orderTotalCalculationService;
    protected readonly IOrderService _orderService;
    protected readonly IOrderProcessingService _orderProcessingService;
    protected readonly ISettingService _settingService;
    protected readonly IWebHelper _webHelper;
    protected readonly IWorkContext _workContext;
    protected readonly ICustomerService _customerService;
    protected readonly IAddressService _addressService;
    protected readonly CustomerSettings _customerSettings;
    protected readonly IyzipayPaymentSettings _iyzipayPaymentSettings;
    protected readonly IyzipayConfigurationService _configurationService;
    protected readonly IyzipayCheckoutFormService _checkoutFormService;
    protected readonly IyzipayOrderDataService _orderDataService;
    protected readonly IyzipayApiClient _apiClient;
    protected readonly IyzipayWebhookService _webhookService;
    protected readonly Validators.PaymentInfoValidator _paymentInfoValidator;
    protected readonly IStoreContext _storeContext;
    protected readonly IShoppingCartService _shoppingCartService;

    #endregion

    #region Ctor

    public IyzipayPaymentProcessor(ILocalizationService localizationService,
        IOrderTotalCalculationService orderTotalCalculationService,
        IOrderService orderService,
        IOrderProcessingService orderProcessingService,
        ISettingService settingService,
        IWebHelper webHelper,
        IWorkContext workContext,
        ICustomerService customerService,
        IAddressService addressService,
        CustomerSettings customerSettings,
        IyzipayPaymentSettings iyzipayPaymentSettings,
        IyzipayConfigurationService configurationService,
        IyzipayCheckoutFormService checkoutFormService,
        IyzipayOrderDataService orderDataService,
        IyzipayApiClient apiClient,
        IyzipayWebhookService webhookService,
        Validators.PaymentInfoValidator paymentInfoValidator,
        IStoreContext storeContext,
        IShoppingCartService shoppingCartService
)
    {
        _localizationService = localizationService;
        _orderTotalCalculationService = orderTotalCalculationService;
        _orderService = orderService;
        _orderProcessingService = orderProcessingService;
        _settingService = settingService;
        _webHelper = webHelper;
        _workContext = workContext;
        _customerService = customerService;
        _addressService = addressService;
        _customerSettings = customerSettings;
        _iyzipayPaymentSettings = iyzipayPaymentSettings;
        _configurationService = configurationService;
        _checkoutFormService = checkoutFormService;
        _orderDataService = orderDataService;
        _apiClient = apiClient;
        _webhookService = webhookService;
        _paymentInfoValidator = paymentInfoValidator;
        _storeContext = storeContext;
        _shoppingCartService = shoppingCartService;
    }

    #endregion

    #region Methods

    /// <summary>
    /// Gets additional handling fee
    /// </summary>
    /// <param name="cart">Shopping cart</param>
    /// <returns>Additional handling fee</returns>
    public async Task<decimal> GetAdditionalHandlingFeeAsync(IList<ShoppingCartItem> cart)
    {
        return await Task.FromResult(_iyzipayPaymentSettings.AdditionalFee);
    }

    /// <summary>
    /// Gets a type of a view component for displaying plugin in public store ("payment info" checkout step)
    /// </summary>
    /// <returns>View component type</returns> 
    public Type GetPublicViewComponent()
    {
        return typeof(PaymentInfoViewComponent);
    }

    /// <summary>
    /// Process a payment
    /// </summary>
    /// <param name="processPaymentRequest">Payment info required for an order processing</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the process payment result
    /// </returns>
    public async Task<ProcessPaymentResult> ProcessPaymentAsync(ProcessPaymentRequest processPaymentRequest)
    {
        var result = new ProcessPaymentResult();
        var formMode = _iyzipayPaymentSettings.PaymentFormMode;

        try
        {
            var checkoutResult = await _checkoutFormService.CreateCheckoutFormAsync(processPaymentRequest.OrderGuid, _iyzipayPaymentSettings.PaymentFormMode);

            if (checkoutResult.Success == "success")
                result.NewPaymentStatus = PaymentStatus.Pending;
            else
                result.AddError(checkoutResult.ErrorMessage);

            if (formMode == "iframe" || formMode == "popup")
                result.AuthorizationTransactionResult = checkoutResult.CheckoutFormContent;
            else
                result.AuthorizationTransactionResult = checkoutResult.PaymentPageUrl;

            return await Task.FromResult(result);
        }
        catch (Exception ex)
        {
            result.AddError($"Payment processing error: {ex.Message}");
        }

        return await Task.FromResult(result);
    }

    /// <summary>
    /// Refunds a payment
    /// </summary>
    /// <param name="refundPaymentRequest">Request</param>
    /// <returns>Result</returns>
    public async Task<RefundPaymentResult> RefundAsync(RefundPaymentRequest refundPaymentRequest)
    {
        var result = new RefundPaymentResult();

        try
        {
            var order = await _orderService.GetOrderByIdAsync(refundPaymentRequest.Order.Id);
            if (order == null)
            {
                result.AddError("Order not found");
                return result;
            }

            // CheckoutFormService kullanarak refund işlemini yap
            var (success, errorMessage) = await _checkoutFormService.RefundPaymentAsync(order, refundPaymentRequest.AmountToRefund);

            if (success)
            {
                result.NewPaymentStatus = PaymentStatus.Refunded;
            }
            else
            {
                result.AddError(errorMessage);
            }
        }
        catch (Exception ex)
        {
            result.AddError($"Refund error: {ex.Message}");
        }

        return await Task.FromResult(result);
    }

    /// <summary>
    /// Voids a payment (Cancel - 24 saat içinde)
    /// </summary>
    /// <param name="voidPaymentRequest">Request</param>
    /// <returns>Result</returns>
    public async Task<VoidPaymentResult> VoidAsync(VoidPaymentRequest voidPaymentRequest)
    {
        var result = new VoidPaymentResult();

        try
        {
            var order = voidPaymentRequest.Order;

            // CheckoutFormService kullanarak void işlemini yap
            var (success, errorMessage) = await _checkoutFormService.VoidPaymentAsync(order);

            if (success)
            {
                result.NewPaymentStatus = PaymentStatus.Voided;
            }
            else
            {
                result.AddError(errorMessage);
            }
        }
        catch (Exception ex)
        {
            result.AddError($"Cancel error: {ex.Message}");
        }

        return await Task.FromResult(result);
    }

    /// <summary>
    /// Process recurring payment
    /// </summary>
    /// <param name="processPaymentRequest">Payment info required for an order processing</param>
    /// <returns>Process payment result</returns>
    public async Task<ProcessPaymentResult> ProcessRecurringPaymentAsync(ProcessPaymentRequest processPaymentRequest)
    {
        var result = new ProcessPaymentResult();
        result.AddError("Recurring payment not supported");
        return await Task.FromResult(result);
    }

    /// <summary>
    /// Cancels a recurring payment
    /// </summary>
    /// <param name="cancelPaymentRequest">Request</param>
    /// <returns>Result</returns>
    public async Task<CancelRecurringPaymentResult> CancelRecurringPaymentAsync(CancelRecurringPaymentRequest cancelPaymentRequest)
    {
        var result = new CancelRecurringPaymentResult();
        result.AddError("Recurring payment not supported");
        return await Task.FromResult(result);
    }

    /// <summary>
    /// Post process payment (used by redirection payment methods)
    /// </summary>
    /// <param name="postProcessPaymentRequest">Payment info required for an order processing</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    public async Task PostProcessPaymentAsync(PostProcessPaymentRequest postProcessPaymentRequest)
    {
        // Nothing to do here for Iyzipay
        await Task.CompletedTask;
    }

    /// <summary>
    /// Gets a value indicating whether customers can complete a payment after order is placed but not completed (for redirection payment methods)
    /// </summary>
    /// <param name="order">Order</param>
    /// <returns>Result</returns>
    public async Task<bool> CanRePostProcessPaymentAsync(Order order)
    {
        return await Task.FromResult(false);
    }

    /// <summary>
    /// Validate payment form
    /// </summary>
    /// <param name="form">The parsed form values</param>
    /// <returns>List of validating errors</returns>
    public async Task<IList<string>> ValidatePaymentFormAsync(IFormCollection form)
    {
        var warnings = new List<string>();
        
        // Müşteri bilgilerini al ve validasyon yap
        var model = await _paymentInfoValidator.GetCustomerInfoAsync();
        var validationResult = _paymentInfoValidator.Validate(model);
        
        if (!validationResult.IsValid)
        {
            warnings.AddRange(validationResult.Errors.Select(error => error.ErrorMessage));
        }

        return await Task.FromResult<IList<string>>(warnings);
    }

    /// <summary>
    /// Get payment information
    /// </summary>
    /// <param name="form">The parsed form values</param>
    /// <returns>Payment info holder</returns>
    public async Task<ProcessPaymentRequest> GetPaymentInfoAsync(IFormCollection form)
    {
        return await Task.FromResult(new ProcessPaymentRequest());
    }

    /// <summary>
    /// Captures payment
    /// </summary>
    /// <param name="capturePaymentRequest">Capture payment request</param>
    /// <returns>Capture payment result</returns>
    public async Task<CapturePaymentResult> CaptureAsync(CapturePaymentRequest capturePaymentRequest)
    {
        var result = new CapturePaymentResult();
        result.AddError("Capture not supported");
        return await Task.FromResult(result);
    }

    /// <summary>
    /// Gets a value indicating whether payment method should be hidden during checkout
    /// </summary>
    /// <param name="cart">Shopping cart</param>
    /// <returns>True - hide; False - show.</returns>
    public async Task<bool> HidePaymentMethodAsync(IList<ShoppingCartItem> cart)
    {
        return await Task.FromResult(false);
    }

    /// <summary>
    /// Gets a configuration page URL
    /// </summary>
    public override string GetConfigurationPageUrl()
    {
        return $"{_webHelper.GetStoreLocation()}Admin/PaymentIyzipay/Configure";
    }

    /// <summary>
    /// Gets a payment method description that will be displayed on checkout pages in the public store
    /// </summary>
    public async Task<string> GetPaymentMethodDescriptionAsync()
    {
        return await _localizationService.GetResourceAsync("Plugins.Payments.Iyzipay.PaymentMethodDescription");
    }

    #endregion

    #region Properties

    /// <summary>
    /// Gets a value indicating whether capture is supported
    /// </summary>
    public bool SupportCapture => false;

    /// <summary>
    /// Gets a value indicating whether partial refund is supported
    /// </summary>
    public bool SupportPartiallyRefund => true;

    /// <summary>
    /// Gets a value indicating whether refund is supported
    /// </summary>
    public bool SupportRefund => true;

    /// <summary>
    /// Gets a value indicating whether void is supported
    /// </summary>
    public bool SupportVoid => true;

    /// <summary>
    /// Gets a recurring payment type of payment method
    /// </summary>
    public RecurringPaymentType RecurringPaymentType => RecurringPaymentType.NotSupported;

    /// <summary>
    /// Gets a payment method type
    /// </summary>
    public PaymentMethodType PaymentMethodType => PaymentMethodType.Redirection;

    /// <summary>
    /// Gets a value indicating whether we should display a payment information page for this plugin
    /// </summary>
    public bool SkipPaymentInfo => false;


    /// <summary>
    /// Process webhook from Iyzipay
    /// </summary>
    /// <param name="webhookData">Webhook data</param>
    /// <param name="signature">HMAC signature</param>
    /// <returns>Webhook processing result</returns>
    public async Task<WebhookProcessResult> ProcessWebhookAsync(Dictionary<string, object> webhookData, string signature = null)
    {
        return await _webhookService.ProcessWebhookAsync(webhookData, signature);
    }


    /// <summary>
    /// Process Confirmation from Iyzipay
    /// </summary>
    /// <param name="token">Token</param>
    /// <returns>Webhook processing result</returns>
    public async Task<CallbackProcessResult> ProcessConfirmationAsync(string token)
    {
        var result = new CallbackProcessResult();

        try
        {   
            var checkoutForm = await _checkoutFormService.RetrievePaymentAsync(token);
            
            if (checkoutForm == null)
            {
                result.ErrorMessage = "Payment information not found";
                return result;
            }

            if (checkoutForm.Status != "success")
            {
                result.ErrorMessage = $"Payment failed. Status: {checkoutForm.Status}";
                return result;
            }

            var basketId = checkoutForm.BasketId;
            var paymentId = checkoutForm.PaymentId;
            var authCode = checkoutForm.AuthCode;
            var installment = checkoutForm.Installment;
            var originalPrice = checkoutForm.Price;
            
            decimal paidPrice;
            if (decimal.TryParse(checkoutForm.PaidPrice, NumberStyles.Any, CultureInfo.InvariantCulture, out var parsedPaidPrice))
            {
                if (parsedPaidPrice > 1000000)
                {
                    paidPrice = parsedPaidPrice / 100000;
                }
                else
                {
                    paidPrice = parsedPaidPrice;
                }
            }
            else
            {
                paidPrice = 0;
            }
            
            if (!Guid.TryParse(basketId, out var orderGuid))
            {
                result.ErrorMessage = "Invalid basket ID format";
                return result;
            }

            var order = await _orderService.GetOrderByGuidAsync(orderGuid);
            if (order == null)
            {
                
                var customer = await _workContext.GetCurrentCustomerAsync();
                var store = await _storeContext.GetCurrentStoreAsync();
                var cart = await _shoppingCartService.GetShoppingCartAsync(customer, Core.Domain.Orders.ShoppingCartType.ShoppingCart, store.Id);
                
                if (!cart.Any())
                {
                    result.ErrorMessage = "Shopping cart is empty";
                    return result;
                }
                
                var paymentRequest = new ProcessPaymentRequest
                {
                    OrderGuid = orderGuid,
                    StoreId = store.Id,
                    CustomerId = customer.Id,
                    PaymentMethodSystemName = "Payments.IyzipayCheckoutForm"
                };
                
                var placeOrderResult = await _orderProcessingService.PlaceOrderAsync(paymentRequest);
                if (placeOrderResult?.PlacedOrder == null || !placeOrderResult.Success)
                {
                    result.ErrorMessage = "Failed to create order";
                    return result;
                }
                
                order = placeOrderResult.PlacedOrder;
            }


            order.PaymentStatus = PaymentStatus.Paid;
            order.OrderStatus = OrderStatus.Complete;
            order.PaidDateUtc = DateTime.UtcNow;

            order.AuthorizationTransactionId = paymentId;
            order.AuthorizationTransactionResult = authCode;
            order.CaptureTransactionId = paymentId;
            order.CaptureTransactionResult = authCode;

            await _orderService.UpdateOrderAsync(order);

            
            if (installment.HasValue && installment.Value > 1)
            {
                var originalOrderTotal = order.OrderTotal;
                
                if (originalOrderTotal > 0 && paidPrice > originalOrderTotal)
                {
                    var installmentFee = paidPrice - originalOrderTotal;
                    await AddInstallmentFeeToOrderAsync(order, installmentFee, installment.Value);
                }
            }

            result.Success = true;
            result.OrderId = order.Id;
            result.TransactionId = paymentId;

            return result;
        }
        catch (Exception ex)
        {
            result.ErrorMessage = $"Payment confirmation error: {ex.Message}";
            return result;
        }
    }


    /// <summary>
    /// Add installment fee to order
    /// </summary>
    /// <param name="order">Order</param>
    /// <param name="installmentFee">Installment fee amount</param>
    /// <param name="installment">Installment count</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    private async Task AddInstallmentFeeToOrderAsync(Order order, decimal installmentFee, int installment)
    {
        try
        {
            var existingOrderItems = await _orderService.GetOrderItemsAsync(order.Id);
            var firstProductId = existingOrderItems.FirstOrDefault()?.ProductId ?? 1;

            var installmentFeeItem = new Nop.Core.Domain.Orders.OrderItem
            {
                OrderId = order.Id,
                ProductId = firstProductId, // Mevcut bir product ID kullan (foreign key hatası önlemek için)
                Quantity = 1,
                UnitPriceInclTax = installmentFee,
                UnitPriceExclTax = installmentFee,
                PriceInclTax = installmentFee,
                PriceExclTax = installmentFee,
                AttributeDescription = $"Taksit Komisyonu ({installment} Taksit) - Taksit sayısı: {installment}",
                ItemWeight = null,
                RentalStartDateUtc = null,
                RentalEndDateUtc = null
            };

            await _orderService.InsertOrderItemAsync(installmentFeeItem);
            order.OrderTotal += installmentFee;
            await _orderService.UpdateOrderAsync(order);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Iyzipay AddInstallmentFeeToOrderAsync] Exception: {ex.Message}");
        }
    }

    /// <summary>
    /// Process callback from Iyzipay
    /// </summary>
    /// <param name="callbackData">Callback data</param>
    /// <returns>Callback processing result</returns>
    public async Task<CallbackProcessResult> ProcessCallbackAsync(CallbackResponseModel callbackData)
    {
        var result = new CallbackProcessResult();

        try
        {
            if (!Guid.TryParse(callbackData.BasketId, out var orderGuid))
            {
                result.ErrorMessage = "Invalid basket ID format";
                return result;
            }

            var order = await _orderService.GetOrderByGuidAsync(orderGuid);
            if (order == null)
            {
                result.ErrorMessage = "Order not found";
                return result;
            }

            if (callbackData.PaymentStatus != "SUCCESS")
            {
                result.ErrorMessage = $"Payment failed. Status: {callbackData.PaymentStatus}";
                return result;
            }

            order.PaymentStatus = PaymentStatus.Paid;
            order.OrderStatus = OrderStatus.Complete;
            order.PaidDateUtc = DateTime.UtcNow;

            order.AuthorizationTransactionId = callbackData.PaymentId;
            order.AuthorizationTransactionResult = callbackData.AuthCode;
            order.CaptureTransactionId = callbackData.PaymentId;
            order.CaptureTransactionResult = callbackData.AuthCode;

            await _orderService.UpdateOrderAsync(order);

            result.Success = true;
            result.OrderId = order.Id;
            result.TransactionId = callbackData.PaymentId;

            return result;
        }
        catch (Exception ex)
        {
            result.ErrorMessage = $"Callback processing error: {ex.Message}";
            return result;
        }
    }

    /// <summary>
    /// Install the plugin
    /// </summary>
    /// <returns>A task that represents the asynchronous operation</returns>
    public override async Task InstallAsync()
    {
        var settings = new IyzipayPaymentSettings
        {
            UseSandbox = true,
            OrderStatusAfterPayment = (int)OrderStatus.Pending,
            PaymentFormLanguage = "tr",
            EnableProtectedShopView = false,
            ProtectedShopViewPosition = "left",
            PaymentFormMode = "POPUP",
            PaymentOptionTitle = "Kredi Kartı ile Ödeme",
            Enable3DSecure = true,
            EnableInstallmentOptions = true,
            MaxInstallmentCount = 12,
            PassProductNamesAndTotals = true,
            SendInvoiceInfo = true
        };
        await _settingService.SaveSettingAsync(settings);

        await _localizationService.AddOrUpdateLocaleResourceAsync(new Dictionary<string, string>
        {
            ["Plugins.Payments.Iyzipay.Fields.ApiSettings"] = "API Ayarları",
            ["Plugins.Payments.Iyzipay.Fields.ApiKey"] = "API Anahtarı",
            ["Plugins.Payments.Iyzipay.Fields.ApiKey.Hint"] = "Iyzipay API Anahtarınızı girin.",
            ["Plugins.Payments.Iyzipay.Fields.SecretKey"] = "Gizli Anahtar",
            ["Plugins.Payments.Iyzipay.Fields.SecretKey.Hint"] = "Iyzipay Gizli Anahtarınızı girin.",
            ["Plugins.Payments.Iyzipay.Fields.BaseUrl"] = "Base URL",
            ["Plugins.Payments.Iyzipay.Fields.BaseUrl.Hint"] = "Iyzipay Base URL'ini girin.",
            ["Plugins.Payments.Iyzipay.Fields.Environment"] = "Ortam",
            ["Plugins.Payments.Iyzipay.Fields.Environment.Hint"] = "Test veya canlı ortamı seçin.",
            ["Plugins.Payments.Iyzipay.Fields.Environment.Sandbox"] = "Test Ortamı",
            ["Plugins.Payments.Iyzipay.Fields.Environment.Live"] = "Canlı Ortam",
            ["Plugins.Payments.Iyzipay.Fields.UseSandbox"] = "Test Ortamı Kullan",
            ["Plugins.Payments.Iyzipay.Fields.UseSandbox.Hint"] = "Test ortamını etkinleştirmek için işaretleyin.",

            ["Plugins.Payments.Iyzipay.Fields.WebhookSettings"] = "Webhook Ayarları",
            ["Plugins.Payments.Iyzipay.Fields.WebhookUrl"] = "Webhook URL",
            ["Plugins.Payments.Iyzipay.Fields.WebhookUrl.Hint"] = "Webhook URL'i otomatik olarak oluşturulur.",

            ["Plugins.Payments.Iyzipay.Fields.PaymentSettings"] = "Ödeme Ayarları",
            ["Plugins.Payments.Iyzipay.Fields.OrderStatusAfterPayment"] = "Ödeme Sonrası Sipariş Durumu",
            ["Plugins.Payments.Iyzipay.Fields.OrderStatusAfterPayment.Hint"] = "Ödeme sonrası sipariş durumunu seçin.",
            ["Plugins.Payments.Iyzipay.Fields.PaymentFormLanguage"] = "Ödeme Formu Dili",
            ["Plugins.Payments.Iyzipay.Fields.PaymentFormLanguage.Hint"] = "Ödeme formu dilini seçin.",
            ["Plugins.Payments.Iyzipay.Fields.PaymentFormLanguage.Auto"] = "Otomatik",
            ["Plugins.Payments.Iyzipay.Fields.PaymentFormMode"] = "Ödeme Formu Modu",
            ["Plugins.Payments.Iyzipay.Fields.PaymentFormMode.Hint"] = "Ödeme formu modunu seçin.",

            ["Plugins.Payments.Iyzipay.Fields.AdvancedSettings"] = "Gelişmiş Ayarlar",
            ["Plugins.Payments.Iyzipay.Fields.EnableProtectedShopView"] = "Korumalı Mağaza Görünümü Aktif",
            ["Plugins.Payments.Iyzipay.Fields.EnableProtectedShopView.Hint"] = "Korumalı mağaza görünümünü etkinleştirmek için işaretleyin.",
            ["Plugins.Payments.Iyzipay.Fields.ProtectedShopViewPosition"] = "Korumalı Mağaza Görünümü Pozisyonu",
            ["Plugins.Payments.Iyzipay.Fields.ProtectedShopViewPosition.Hint"] = "Korumalı mağaza görünümü pozisyonunu ayarlayın.",
            ["Plugins.Payments.Iyzipay.Fields.ProtectedShopViewPosition.Left"] = "Sol",
            ["Plugins.Payments.Iyzipay.Fields.ProtectedShopViewPosition.Right"] = "Sağ",
            ["Plugins.Payments.Iyzipay.Fields.PaymentOptionTitle"] = "Ödeme Seçeneği Başlığı",
            ["Plugins.Payments.Iyzipay.Fields.PaymentOptionTitle.Hint"] = "Ödeme seçeneği başlığını girin.",
            ["Plugins.Payments.Iyzipay.Fields.Enable3DSecure"] = "3D Secure Aktif",
            ["Plugins.Payments.Iyzipay.Fields.Enable3DSecure.Hint"] = "3D Secure'i etkinleştirmek için işaretleyin.",
            ["Plugins.Payments.Iyzipay.Fields.EnableInstallmentOptions"] = "Taksit Seçenekleri Aktif",
            ["Plugins.Payments.Iyzipay.Fields.EnableInstallmentOptions.Hint"] = "Taksit seçeneklerini etkinleştirmek için işaretleyin.",
            ["Plugins.Payments.Iyzipay.Fields.MaxInstallmentCount"] = "Maksimum Taksit Sayısı",
            ["Plugins.Payments.Iyzipay.Fields.MaxInstallmentCount.Hint"] = "Maksimum taksit sayısını ayarlayın.",

            ["Plugins.Payments.Iyzipay.Fields.AdditionalSettings"] = "Ek Ayarlar",
            ["Plugins.Payments.Iyzipay.Fields.PassProductNamesAndTotals"] = "Ürün Adları ve Toplamları Gönder",
            ["Plugins.Payments.Iyzipay.Fields.PassProductNamesAndTotals.Hint"] = "Ürün adları ve toplamlarını Iyzipay'a göndermek için işaretleyin.",
            ["Plugins.Payments.Iyzipay.Fields.SendInvoiceInfo"] = "Fatura Bilgilerini Gönder",
            ["Plugins.Payments.Iyzipay.Fields.SendInvoiceInfo.Hint"] = "Fatura bilgilerini Iyzipay'a göndermek için işaretleyin.",

            ["Plugins.Payments.Iyzipay.Fields.AdditionalFee"] = "Ek Ücret",
            ["Plugins.Payments.Iyzipay.Fields.AdditionalFee.Hint"] = "Müşterilerinizden alınacak ek ücreti girin.",
            ["Plugins.Payments.Iyzipay.Fields.AdditionalFeePercentage"] = "Ek Ücret Yüzde Olarak",
            ["Plugins.Payments.Iyzipay.Fields.AdditionalFeePercentage.Hint"] = "Ek ücretin yüzde olarak uygulanıp uygulanmayacağını belirler.",

            ["Admin.Common.Copy"] = "Kopyala",
            ["Admin.Common.Copied"] = "Kopyalandı",

            ["Plugins.Payments.Iyzipay.PaymentMethodDescription"] = "Kredi/Banka Kartı ile güvenli ödeme yapın (CheckoutForm)",
            ["Plugins.Payments.Iyzipay.PaymentDescription"] = "Kredi kartı veya banka kartı ile güvenli ödeme yapabilirsiniz.",
            ["Plugins.Payments.Iyzipay.PaymentOptionTitle"] = "Kredi/Banka Kartı ile Öde",

            ["Admin.Orders.OrderStatus.Pending"] = "Beklemede",
            ["Admin.Orders.OrderStatus.Processing"] = "İşleniyor",
            ["Admin.Orders.OrderStatus.Complete"] = "Tamamlandı",
            ["Admin.Orders.OrderStatus.Cancelled"] = "İptal Edildi",

            ["Plugins.Payments.Iyzipay.CustomerNotFound"] = "Müşteri bulunamadı",
            ["Plugins.Payments.Iyzipay.EmailRequired"] = "E-posta adresi gereklidir",
            ["Plugins.Payments.Iyzipay.EmailInvalid"] = "Geçersiz e-posta adresi",
            ["Plugins.Payments.Iyzipay.PhoneRequired"] = "Telefon numarası gereklidir",
            ["Plugins.Payments.Iyzipay.PhoneInvalid"] = "Geçersiz telefon numarası",
        });

        await base.InstallAsync();
    }

    /// <summary>
    /// Uninstall the plugin
    /// </summary>
    /// <returns>A task that represents the asynchronous operation</returns>
    public override async Task UninstallAsync()
    {
        await _settingService.DeleteSettingAsync<IyzipayPaymentSettings>();
        await _localizationService.DeleteLocaleResourcesAsync("Plugins.Payments.Iyzipay");
        await base.UninstallAsync();
    }

    #endregion

}
