using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Iyzipay.Model;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Services.Catalog;
using Nop.Services.Customers;
using Nop.Services.Orders;
using Address = Iyzipay.Model.Address;

namespace Nop.Plugin.Payments.Iyzipay.Services;

/// <summary>
/// Iyzipay data mapper service
/// </summary>
public class IyzipayDataMapper
{
    #region Fields

    private readonly ICustomerService _customerService;
    private readonly IProductService _productService;
    private readonly IShoppingCartService _shoppingCartService;

    #endregion

    #region Ctor

    public IyzipayDataMapper(ICustomerService customerService, IProductService productService, IShoppingCartService shoppingCartService)
    {
        _customerService = customerService;
        _productService = productService;
        _shoppingCartService = shoppingCartService;
    }

    #endregion

    #region Methods



    /// <summary>
    /// Create buyer from customer
    /// </summary>
    /// <param name="customer">Customer</param>
    /// <returns>Buyer</returns>
    public Buyer CreateBuyerFromCustomer(Customer customer)
    {
        return new Buyer
        {
            Id = customer.Id.ToString(),
            Name = customer.FirstName ?? "Müşteri",
            Surname = customer.LastName ?? "Adı",
            GsmNumber = customer.Phone ?? "+905350000000",
            Email = customer.Email ?? "customer@example.com",
            IdentityNumber = "11111111111", // TC kimlik numarası gerekli
            LastLoginDate = customer.LastLoginDateUtc?.ToString("yyyy-MM-dd HH:mm:ss") ?? DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"),
            RegistrationDate = customer.CreatedOnUtc.ToString("yyyy-MM-dd HH:mm:ss"),
            RegistrationAddress = "Adres bilgisi",
            City = "Istanbul",
            Country = "Turkey",
            ZipCode = "34000"
        };
    }

    /// <summary>
    /// Create basket items from shopping cart
    /// </summary>
    /// <param name="cart">Shopping cart</param>
    /// <returns>List of basket items</returns>
    public async Task<List<BasketItem>> CreateBasketItemsFromCartAsync(IList<ShoppingCartItem> cart)
    {
        var basketItems = new List<BasketItem>();
        
        foreach (var item in cart)
        {
            var product = await _productService.GetProductByIdAsync(item.ProductId);
            
            var unitPriceResult = await _shoppingCartService.GetUnitPriceAsync(item, true);
            var itemTotalPrice = unitPriceResult.unitPrice * item.Quantity;
            
            basketItems.Add(new BasketItem
            {
                Id = item.ProductId.ToString(),
                Name = product?.Name ?? "Ürün",
                Category1 = "Kategori", // TODO: Product categories yüklenebilir
                Category2 = "Alt Kategori",
                ItemType = BasketItemType.PHYSICAL.ToString(),
                Price = itemTotalPrice.ToString("F2", System.Globalization.CultureInfo.InvariantCulture)
            });
        }

        return basketItems;
    }

    /// <summary>
    /// Create shipping address from customer (for cart checkout)
    /// </summary>
    /// <param name="customer">Customer</param>
    /// <returns>Address</returns>
    public Address CreateShippingAddressFromCustomer(Customer customer)
    {
        var contactName = $"{customer.FirstName} {customer.LastName}".Trim();
        
        return new Address
        {
            ContactName = contactName,
            City = "Istanbul",
            Country = "Turkey",
            Description = "Teslimat adresi",
            ZipCode = "34000"
        };
    }

    /// <summary>
    /// Create billing address from customer (for cart checkout)
    /// </summary>
    /// <param name="customer">Customer</param>
    /// <returns>Address</returns>
    public Address CreateBillingAddressFromCustomer(Customer customer)
    {
        var contactName = $"{customer.FirstName} {customer.LastName}".Trim();
        
        return new Address
        {
            ContactName = contactName,
            City = "Istanbul",
            Country = "Turkey",
            Description = "Fatura adresi",
            ZipCode = "34000"
        };
    }

    #endregion
}
