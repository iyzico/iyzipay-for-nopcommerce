using System;
using System.Threading.Tasks;
using FluentValidation;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Plugin.Payments.Iyzipay.Models;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;

namespace Nop.Plugin.Payments.Iyzipay.Validators;

/// <summary>
/// Represents payment info validator
/// </summary>
public class PaymentInfoValidator : BaseNopValidator<PaymentInfoModel>
{
    private readonly IWorkContext _workContext;
    private readonly ICustomerService _customerService;
    private readonly IAddressService _addressService;

    public PaymentInfoValidator(ILocalizationService localizationService, 
        CustomerSettings customerSettings,
        IWorkContext workContext,
        ICustomerService customerService,
        IAddressService addressService)
    {
        _workContext = workContext;
        _customerService = customerService;
        _addressService = addressService;

        // E-posta validasyonu
        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessageAwait(localizationService.GetResourceAsync("Plugins.Payments.Iyzipay.EmailRequired"));

        RuleFor(x => x.Email)
            .IsEmailAddress()
            .WithMessageAwait(localizationService.GetResourceAsync("Plugins.Payments.Iyzipay.EmailInvalid"));

        // Telefon numarası validasyonu
        RuleFor(x => x.Phone)
            .NotEmpty()
            .WithMessageAwait(localizationService.GetResourceAsync("Plugins.Payments.Iyzipay.PhoneRequired"));

        RuleFor(x => x.Phone)
            .IsPhoneNumber(customerSettings)
            .WithMessageAwait(localizationService.GetResourceAsync("Plugins.Payments.Iyzipay.PhoneInvalid"));
    }

    /// <summary>
    /// Müşteri bilgilerini al ve model'i doldur
    /// </summary>
    /// <returns>Doldurulmuş PaymentInfoModel</returns>
    public async Task<PaymentInfoModel> GetCustomerInfoAsync()
    {
        var customer = await _workContext.GetCurrentCustomerAsync();
        if (customer == null)
            return new PaymentInfoModel();

        string email = string.Empty;
        string phone = string.Empty;

        // Misafir müşteri ise billing address'ten bilgileri al
        if (await _customerService.IsGuestAsync(customer))
        {
            if (customer.BillingAddressId.HasValue)
            {
                var billingAddress = await _addressService.GetAddressByIdAsync(customer.BillingAddressId.Value);
                if (billingAddress != null)
                {
                    email = billingAddress.Email ?? string.Empty;
                    phone = billingAddress.PhoneNumber ?? string.Empty;
                }
            }
        }
        else
        {
            // Kayıtlı müşteri ise customer bilgilerini kullan
            email = customer.Email ?? string.Empty;
            phone = customer.Phone ?? string.Empty;
        }

        return new PaymentInfoModel
        {
            Email = email,
            Phone = phone
        };
    }
}
