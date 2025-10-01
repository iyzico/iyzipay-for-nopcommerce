using System.Threading.Tasks;
using Iyzipay;
using Iyzipay.Model;
using Iyzipay.Request;

namespace Nop.Plugin.Payments.Iyzipay.Services;

/// <summary>
/// Iyzipay API client service
/// </summary>
public class IyzipayApiClient
{
    #region Methods

    /// <summary>
    /// Create checkout form
    /// </summary>
    /// <param name="request">Checkout form initialize request</param>
    /// <param name="options">Iyzipay options</param>
    /// <returns>Checkout form initialize response</returns>
    public async Task<CheckoutFormInitialize> CreateCheckoutFormAsync(CreateCheckoutFormInitializeRequest request, Options options)
    {
        return await CheckoutFormInitialize.Create(request, options);
    }

    /// <summary>
    /// Create refund
    /// </summary>
    /// <param name="request">Amount based refund request</param>
    /// <param name="options">Iyzipay options</param>
    /// <returns>Refund response</returns>
    public async Task<Refund> CreateRefundAsync(CreateAmountBasedRefundRequest request, Options options)
    {
        return await Refund.CreateAmountBasedRefundRequest(request, options);
    }

    /// <summary>
    /// Create cancel
    /// </summary>
    /// <param name="request">Cancel request</param>
    /// <param name="options">Iyzipay options</param>
    /// <returns>Cancel response</returns>
    public async Task<Cancel> CreateCancelAsync(CreateCancelRequest request, Options options)
    {
        return await Cancel.Create(request, options);
    }

    /// <summary>
    /// Retrieve payment
    /// </summary>
    /// <param name="request">Retrieve payment request</param>
    /// <param name="options">Iyzipay options</param>
    /// <returns>Payment response</returns>
    public async Task<CheckoutForm> RetrievePaymentAsync(RetrieveCheckoutFormRequest request, Options options)
    {
        return await CheckoutForm.Retrieve(request, options);
    }

    #endregion
}
