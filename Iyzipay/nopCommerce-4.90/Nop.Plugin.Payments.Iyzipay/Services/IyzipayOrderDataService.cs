using System;
using System.Text.Json;
using System.Threading.Tasks;
using Nop.Core.Domain.Orders;
using Nop.Plugin.Payments.Iyzipay.Models;
using Nop.Services.Orders;

namespace Nop.Plugin.Payments.Iyzipay.Services;

/// <summary>
/// Iyzipay order data service
/// </summary>
public class IyzipayOrderDataService
{
    #region Fields

    private readonly IOrderService _orderService;

    #endregion

    #region Ctor

    public IyzipayOrderDataService(IOrderService orderService)
    {
        _orderService = orderService;
    }

    #endregion

    #region Methods

    /// <summary>
    /// Save Iyzipay data to order
    /// </summary> 
    /// <param name="order">Order</param>
    /// <param name="iyzipayData">Iyzipay data</param>
    public async Task SaveIyzipayDataToOrderAsync(Order order, IyzipayOrderData iyzipayData)
    {
        try
        {
            var jsonData = iyzipayData.ToJson();
            order.CustomOrderNumber = jsonData; // Geçici olarak CustomOrderNumber kullanıyoruz
            await _orderService.UpdateOrderAsync(order);
        }
        catch (Exception)
        {
            // Log error
        }
    }

    /// <summary>
    /// Get Iyzipay data from order
    /// </summary>
    /// <param name="order">Order</param>
    /// <returns>Iyzipay data</returns>
    public IyzipayOrderData GetIyzipayDataFromOrder(Order order)
    {
        try
        {
            if (string.IsNullOrEmpty(order.CustomOrderNumber))
                return new IyzipayOrderData();

            return IyzipayOrderData.FromJson(order.CustomOrderNumber);
        }
        catch
        {
            return new IyzipayOrderData();
        }
    }

    /// <summary>
    /// Update Iyzipay data from retrieve response
    /// </summary>
    /// <param name="order">Order</param>
    /// <param name="retrieveResponse">Retrieve response</param>
    public async Task UpdateIyzipayDataFromRetrieveAsync(Order order, object retrieveResponse)
    {
        try
        {
            // Retrieve response'dan IyzipayOrderData oluştur
            var iyzipayData = new IyzipayOrderData();

            // JSON parse et ve IyzipayOrderData'ya map et
            var json = JsonSerializer.Serialize(retrieveResponse);
            var jsonDoc = JsonDocument.Parse(json);
            var root = jsonDoc.RootElement;

            if (root.TryGetProperty("paymentId", out var paymentId))
                iyzipayData.PaymentId = paymentId.GetString() ?? string.Empty;

            if (root.TryGetProperty("conversationId", out var conversationId))
                iyzipayData.ConversationId = conversationId.GetString() ?? string.Empty;

            if (root.TryGetProperty("token", out var token))
                iyzipayData.Token = token.GetString() ?? string.Empty;

            if (root.TryGetProperty("basketId", out var basketId))
                iyzipayData.BasketId = basketId.GetString() ?? string.Empty;

            if (root.TryGetProperty("paymentStatus", out var paymentStatus))
                iyzipayData.PaymentStatus = paymentStatus.GetString() ?? string.Empty;

            if (root.TryGetProperty("fraudStatus", out var fraudStatus))
                iyzipayData.FraudStatus = fraudStatus.GetInt32();

            if (root.TryGetProperty("cardType", out var cardType))
                iyzipayData.CardType = cardType.GetString() ?? string.Empty;

            if (root.TryGetProperty("cardAssociation", out var cardAssociation))
                iyzipayData.CardAssociation = cardAssociation.GetString() ?? string.Empty;

            if (root.TryGetProperty("cardFamily", out var cardFamily))
                iyzipayData.CardFamily = cardFamily.GetString() ?? string.Empty;

            if (root.TryGetProperty("binNumber", out var binNumber))
                iyzipayData.BinNumber = binNumber.GetString() ?? string.Empty;

            if (root.TryGetProperty("lastFourDigits", out var lastFourDigits))
                iyzipayData.LastFourDigits = lastFourDigits.GetString() ?? string.Empty;

            if (root.TryGetProperty("authCode", out var authCode))
                iyzipayData.AuthCode = authCode.GetString() ?? string.Empty;

            if (root.TryGetProperty("phase", out var phase))
                iyzipayData.Phase = phase.GetString() ?? string.Empty;

            if (root.TryGetProperty("mdStatus", out var mdStatus))
                iyzipayData.MdStatus = mdStatus.GetInt32();

            if (root.TryGetProperty("hostReference", out var hostReference))
                iyzipayData.HostReference = hostReference.GetString() ?? string.Empty;

            // Item transactions
            if (root.TryGetProperty("itemTransactions", out var itemTransactions) && itemTransactions.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in itemTransactions.EnumerateArray())
                {
                    var itemTransaction = new IyzipayItemTransaction();

                    if (item.TryGetProperty("itemId", out var itemId))
                        itemTransaction.ItemId = itemId.GetString() ?? string.Empty;

                    if (item.TryGetProperty("paymentTransactionId", out var paymentTransactionId))
                        itemTransaction.PaymentTransactionId = paymentTransactionId.GetString() ?? string.Empty;

                    if (item.TryGetProperty("transactionStatus", out var transactionStatus))
                        itemTransaction.TransactionStatus = transactionStatus.GetInt32();

                    if (item.TryGetProperty("price", out var price))
                        itemTransaction.Price = price.GetDecimal();

                    if (item.TryGetProperty("paidPrice", out var paidPrice))
                        itemTransaction.PaidPrice = paidPrice.GetDecimal();

                    if (item.TryGetProperty("merchantCommissionRate", out var merchantCommissionRate))
                        itemTransaction.MerchantCommissionRate = merchantCommissionRate.GetDecimal();

                    if (item.TryGetProperty("merchantCommissionRateAmount", out var merchantCommissionRateAmount))
                        itemTransaction.MerchantCommissionRateAmount = merchantCommissionRateAmount.GetDecimal();

                    if (item.TryGetProperty("iyziCommissionRateAmount", out var iyziCommissionRateAmount))
                        itemTransaction.IyziCommissionRateAmount = iyziCommissionRateAmount.GetDecimal();

                    if (item.TryGetProperty("iyziCommissionFee", out var iyziCommissionFee))
                        itemTransaction.IyziCommissionFee = iyziCommissionFee.GetDecimal();

                    if (item.TryGetProperty("merchantPayoutAmount", out var merchantPayoutAmount))
                        itemTransaction.MerchantPayoutAmount = merchantPayoutAmount.GetDecimal();

                    iyzipayData.ItemTransactions.Add(itemTransaction);
                }
            }

            await SaveIyzipayDataToOrderAsync(order, iyzipayData);
        }
        catch (Exception)
        {
            // Log error
        }
    }

    #endregion
}
