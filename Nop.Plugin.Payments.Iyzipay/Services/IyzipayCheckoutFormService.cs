using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Iyzipay;
using Iyzipay.Model;
using Iyzipay.Request;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Plugin.Payments.Iyzipay.Models;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Customers;
using Nop.Core.Domain.Customers;

namespace Nop.Plugin.Payments.Iyzipay.Services;

/// <summary>
/// Iyzipay checkout form service
/// </summary>
public class IyzipayCheckoutFormService
{
    #region Fields

    private readonly IOrderService _orderService;
    private readonly IWebHelper _webHelper;
    private readonly IyzipayPaymentSettings _iyzipayPaymentSettings;
    private readonly IyzipayConfigurationService _configurationService;
    private readonly IyzipayDataMapper _dataMapper;
    private readonly IyzipayApiClient _apiClient;
    private readonly IyzipayOrderDataService _orderDataService;
    private readonly IShoppingCartService _shoppingCartService;
    private readonly IWorkContext _workContext;
    private readonly IOrderTotalCalculationService _orderTotalCalculationService;

    #endregion

    #region Ctor

    public IyzipayCheckoutFormService(
        IOrderService orderService,
        IWebHelper webHelper,
        IyzipayPaymentSettings iyzipayPaymentSettings,
        IyzipayConfigurationService configurationService,
        IyzipayDataMapper dataMapper,
        IyzipayApiClient apiClient,
        IyzipayOrderDataService orderDataService,
        IShoppingCartService shoppingCartService,
        IWorkContext workContext,
        IOrderTotalCalculationService orderTotalCalculationService)
    {
        _orderService = orderService;
        _webHelper = webHelper;
        _iyzipayPaymentSettings = iyzipayPaymentSettings;
        _configurationService = configurationService;
        _dataMapper = dataMapper;
        _apiClient = apiClient;
        _orderDataService = orderDataService;
        _shoppingCartService = shoppingCartService;
        _workContext = workContext;
        _orderTotalCalculationService = orderTotalCalculationService;
    }

    #endregion

    #region Methods

    /// <summary>
    /// Create checkout form for Iyzipay payment using cart data
    /// </summary>
    /// <param name="orderGuid">Order GUID (can be temporary for iframe/popup mode)</param>
    /// <param name="mode">Payment form mode (iframe, popup, responsive)</param>
    /// <returns>Checkout form result</returns>
    public async Task<CheckoutFormInitializeResult> CreateCheckoutFormAsync(Guid orderGuid, string mode = "responsive")
    {
        var result = new CheckoutFormInitializeResult();

        try
        {   
            var customer = await _workContext.GetCurrentCustomerAsync();
            var cart = await _shoppingCartService.GetShoppingCartAsync(customer, Core.Domain.Orders.ShoppingCartType.ShoppingCart);

            if (!cart.Any())
            {
                result.Success = "failure";
                result.Message = "Sepet boş";
                result.ErrorMessage = "Sepet boş";
                return result;
            }

            var cartTotalResult = await _orderTotalCalculationService.GetShoppingCartTotalAsync(cart);
            var cartTotal = cartTotalResult.shoppingCartTotal ?? 0;

            var options = _configurationService.GetOptions();
            var callbackUrl = $"{_webHelper.GetStoreLocation()}PaymentIyzipay/Confirmation";
            
            var request = new CreateCheckoutFormInitializeRequest
            {
                Locale = _configurationService.GetLocale(),
                ConversationId = orderGuid.ToString(),
                Price = cartTotal.ToString("F2", System.Globalization.CultureInfo.InvariantCulture),
                PaidPrice = cartTotal.ToString("F2", System.Globalization.CultureInfo.InvariantCulture),
                Currency = Currency.TRY.ToString(),
                BasketId = orderGuid.ToString(),
                PaymentGroup = PaymentGroup.PRODUCT.ToString(),
                CallbackUrl = callbackUrl
            };

            if (_iyzipayPaymentSettings.EnableInstallmentOptions)
            {
                var enabledInstallments = new List<int>();
                for (int i = 1; i <= _iyzipayPaymentSettings.MaxInstallmentCount; i++)
                {
                    enabledInstallments.Add(i);
                }
                request.EnabledInstallments = enabledInstallments;
            }

            request.Buyer = _dataMapper.CreateBuyerFromCustomer(customer);
            request.BasketItems = await _dataMapper.CreateBasketItemsFromCartAsync(cart);
            request.ShippingAddress = _dataMapper.CreateShippingAddressFromCustomer(customer);
            request.BillingAddress = _dataMapper.CreateBillingAddressFromCustomer(customer);

            var checkoutFormInitialize = await _apiClient.CreateCheckoutFormAsync(request, options);
            if (checkoutFormInitialize.Status == Status.SUCCESS.ToString())
            {
                result.Success = "success";
                result.CheckoutFormContent = checkoutFormInitialize.CheckoutFormContent;

                if (mode.ToLower() == "iframe")
                {
                    result.PaymentPageUrl = checkoutFormInitialize.PaymentPageUrl + "&iframe=true";
                }

                result.PaymentPageUrl = checkoutFormInitialize.PaymentPageUrl;
                result.Token = checkoutFormInitialize.Token;
            }
            else
            {
                result.Success = "failure";
                result.Message = checkoutFormInitialize.ErrorMessage;
                result.ErrorMessage = checkoutFormInitialize.ErrorMessage;
            }
        }
        catch (Exception ex)
        {
            result.Success = "failure";
            result.Message = ex.Message;
            result.ErrorMessage = ex.Message;
        }

        return result;
    }

    /// <summary>
    /// Refund payment
    /// </summary>
    /// <param name="order">Order to refund</param>
    /// <param name="amountToRefund">Amount to refund</param>
    /// <returns>Refund result</returns>
    public async Task<(bool Success, string ErrorMessage)> RefundPaymentAsync(Order order, decimal amountToRefund)
    {
        try
        {   
            var options = _configurationService.GetOptions();
            var iyzipayData = _orderDataService.GetIyzipayDataFromOrder(order);
            
            var paymentId = iyzipayData?.PaymentId;
            if (string.IsNullOrEmpty(paymentId))
            {
                paymentId = order.AuthorizationTransactionId;
            }

            if (string.IsNullOrEmpty(paymentId))
            {
                return (false, "Payment ID not found in order");
            }

            var request = new CreateAmountBasedRefundRequest
            {
                Locale = _configurationService.GetLocale(),
                ConversationId = order.OrderGuid.ToString(),
                Ip = _webHelper.GetCurrentIpAddress(),
                Price = amountToRefund.ToString("F2", System.Globalization.CultureInfo.InvariantCulture),
                PaymentId = paymentId
            };
            
            var refund = await _apiClient.CreateRefundAsync(request, options);
            if (refund.Status == Status.SUCCESS.ToString())
            {
                return (true, string.Empty);
            }
            else
            {
                return (false, $"Refund failed: {refund.ErrorMessage}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Iyzipay RefundPaymentAsync] Exception: {ex.Message}");
            return (false, $"Refund error: {ex.Message}");
        }
    }

    /// <summary>
    /// Void payment (Cancel - 24 saat içinde)
    /// </summary>
    /// <param name="order">Order to void</param>
    /// <returns>Void result</returns>
    public async Task<(bool Success, string ErrorMessage)> VoidPaymentAsync(Order order)
    {
        try
        {
            var orderAge = DateTime.UtcNow - order.CreatedOnUtc;
            if (orderAge.TotalHours > 24)
            {
                return (false, "Payment can only be cancelled within 24 hours");
            }

            var options = _configurationService.GetOptions();
            var iyzipayData = _orderDataService.GetIyzipayDataFromOrder(order);
        
            var paymentId = iyzipayData?.PaymentId;
            if (string.IsNullOrEmpty(paymentId))
            {
                paymentId = order.AuthorizationTransactionId;
            }

            if (string.IsNullOrEmpty(paymentId))
            {
                return (false, "Payment ID not found in order");
            }
            

            var request = new CreateCancelRequest
            {
                Locale = _configurationService.GetLocale(),
                ConversationId = order.OrderGuid.ToString(),
                PaymentId = paymentId,
                Ip = _webHelper.GetCurrentIpAddress()
            };

            
            var cancel = await _apiClient.CreateCancelAsync(request, options);
            if (cancel.Status == Status.SUCCESS.ToString())
            {
                return (true, string.Empty);
            }
            else
            {
                return (false, $"Cancel failed: {cancel.ErrorMessage}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Iyzipay VoidPaymentAsync] Exception: {ex.Message}");
            return (false, $"Cancel error: {ex.Message}");
        }
    }

    /// <summary>
    /// Retrieve payment
    /// </summary>
    /// <param name="token">Token</param>
    /// <returns>Payment response</returns>
    public async Task<CheckoutForm> RetrievePaymentAsync(string token)
    {
        try
        {   
            var options = _configurationService.GetOptions();
            var request = new RetrieveCheckoutFormRequest
            {
                Locale = _configurationService.GetLocale(),
                Token = token
            };
   
            return await _apiClient.RetrievePaymentAsync(request, options);    
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Iyzipay RetrievePaymentAsync] Exception: {ex.Message}");
            throw new Exception($"Retrieve payment error: {ex.Message}");
        }
    }

    #endregion
}
