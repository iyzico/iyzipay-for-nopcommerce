using Iyzipay;

namespace Nop.Plugin.Payments.Iyzipay.Services;

/// <summary>
/// Iyzipay configuration service
/// </summary>
public class IyzipayConfigurationService
{
    #region Fields

    private readonly IyzipayPaymentSettings _iyzipayPaymentSettings;

    #endregion

    #region Ctor

    public IyzipayConfigurationService(IyzipayPaymentSettings iyzipayPaymentSettings)
    {
        _iyzipayPaymentSettings = iyzipayPaymentSettings;
    }

    #endregion

    #region Methods

    /// <summary>
    /// Get Iyzipay options
    /// </summary>
    /// <returns>Iyzipay options</returns> 
    public Options GetOptions()
    {
        return new Options
        {
            ApiKey = _iyzipayPaymentSettings.ApiKey,
            SecretKey = _iyzipayPaymentSettings.SecretKey,
            BaseUrl = _iyzipayPaymentSettings.BaseUrl
        };
    }

    /// <summary>
    /// Get locale
    /// </summary>
    /// <returns>Locale string</returns>
    public string GetLocale()
    {
        return _iyzipayPaymentSettings.PaymentFormLanguage switch
        {
            "en" => "en",
            "tr" => "tr",
            "auto" => "tr", // Site diline göre ayarlanabilir
            _ => "tr"
        };
    }

    #endregion
}
