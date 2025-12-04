using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Plugin.Payments.Iyzipay.Models;
using Nop.Services.Orders;

namespace Nop.Plugin.Payments.Iyzipay.Services;

/// <summary>
/// Iyzipay webhook service
/// </summary>
public class IyzipayWebhookService
{
    #region Fields

    private readonly IOrderService _orderService;
    private readonly IyzipayPaymentSettings _iyzipayPaymentSettings;

    #endregion

    #region Ctor

    public IyzipayWebhookService(IOrderService orderService, IyzipayPaymentSettings iyzipayPaymentSettings)
    {
        _orderService = orderService;
        _iyzipayPaymentSettings = iyzipayPaymentSettings;
    }

    #endregion

    #region Methods

    /// <summary>
    /// Process webhook from Iyzipay
    /// </summary>
    /// <param name="webhookData">Webhook data</param>
    /// <param name="signature">HMAC signature</param>
    /// <returns>Webhook processing result</returns>
    public async Task<WebhookProcessResult> ProcessWebhookAsync(Dictionary<string, object> webhookData, string signature = null)
    {
        var result = new WebhookProcessResult();

        try
        {
            if (!string.IsNullOrEmpty(signature))
            {
                // HMAC doğrulaması ile işle
                result = await ProcessWebhookWithHmacAsync(webhookData, signature);
            }
            else
            {
                // Retrieve servisi ile işle
                result = await ProcessWebhookWithRetrieveAsync(webhookData);
            }
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.Message = ex.Message;
        }

        return result;
    }

    /// <summary>
    /// Process webhook with HMAC verification
    /// </summary>
    /// <param name="webhookData">Webhook data</param>
    /// <param name="signature">HMAC signature</param>
    /// <returns>Webhook processing result</returns>
    private async Task<WebhookProcessResult> ProcessWebhookWithHmacAsync(Dictionary<string, object> webhookData, string signature)
    {
        var result = new WebhookProcessResult();

        try
        {
            // HMAC doğrulaması
            var iyziEventType = webhookData["iyziEventType"]?.ToString() ?? string.Empty;
            var iyziPaymentId = webhookData["iyziPaymentId"]?.ToString() ?? string.Empty;
            var token = webhookData["token"]?.ToString() ?? string.Empty;
            var paymentConversationId = webhookData["paymentConversationId"]?.ToString() ?? string.Empty;
            var status = webhookData["status"]?.ToString() ?? string.Empty;

            // CheckoutForm ve PayWithIyzico için HMAC key oluştur
            var key = _iyzipayPaymentSettings.SecretKey + iyziEventType + iyziPaymentId + token + paymentConversationId + status;

            // HMAC SHA256 ile signature üret
            var computedSignature = CalculateHMACSHA256(key, _iyzipayPaymentSettings.SecretKey);

            // Signature doğrula
            if (computedSignature != signature.ToLower())
            {
                result.Success = false;
                result.Message = "Invalid signature";
                return result;
            }

            // Siparişi güncelle
            await UpdateOrderStatusAsync(paymentConversationId, status);
            result.Success = true;
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.Message = ex.Message;
        }

        return result;
    }

    /// <summary>
    /// Process webhook with retrieve service
    /// </summary>
    /// <param name="webhookData">Webhook data</param>
    /// <returns>Webhook processing result</returns>
    private async Task<WebhookProcessResult> ProcessWebhookWithRetrieveAsync(Dictionary<string, object> webhookData)
    {
        var result = new WebhookProcessResult();

        try
        {
            var token = webhookData["token"]?.ToString();
            var paymentConversationId = webhookData["paymentConversationId"]?.ToString();

            if (string.IsNullOrEmpty(token))
            {
                result.Success = false;
                result.Message = "Missing required parameters";
                return result;
            }

            // Iyzipay Retrieve API'sini çağır
            var status = webhookData["status"]?.ToString() ?? "FAILURE";
            await UpdateOrderStatusAsync(paymentConversationId, status);
            result.Success = true;
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.Message = ex.Message;
        }

        return result;
    }

    /// <summary>
    /// Update order status
    /// </summary>
    /// <param name="conversationId">Conversation ID</param>
    /// <param name="status">Payment status</param>
    private async Task UpdateOrderStatusAsync(string conversationId, string status)
    {
        if (Guid.TryParse(conversationId, out var orderGuid))
        {
            var order = await _orderService.GetOrderByGuidAsync(orderGuid);

            if (order != null)
            {
                if (status == "SUCCESS")
                {
                    // Ödeme başarılı
                    order.PaymentStatus = PaymentStatus.Paid;
                    order.OrderStatus = (OrderStatus)_iyzipayPaymentSettings.OrderStatusAfterPayment;
                }
                else
                {
                    // Ödeme başarısız
                    order.PaymentStatus = PaymentStatus.Voided;
                    order.OrderStatus = OrderStatus.Cancelled;
                }

                await _orderService.UpdateOrderAsync(order);
            }
        }
    }

    /// <summary>
    /// Calculate HMAC SHA256
    /// </summary>
    /// <param name="data">Data to hash</param>
    /// <param name="key">Secret key</param>
    /// <returns>HMAC SHA256 hash</returns>
    private string CalculateHMACSHA256(string data, string key)
    {
        byte[] keyBytes = Encoding.UTF8.GetBytes(key);
        byte[] dataBytes = Encoding.UTF8.GetBytes(data);

        using (var hmac = new HMACSHA256(keyBytes))
        {
            byte[] hmacBytes = hmac.ComputeHash(dataBytes);
            return BitConverter.ToString(hmacBytes).Replace("-", "").ToLower();
        }
    }

    #endregion
}
