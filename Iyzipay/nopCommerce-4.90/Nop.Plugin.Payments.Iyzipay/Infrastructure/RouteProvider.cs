using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Nop.Web.Framework.Mvc.Routing;

namespace Nop.Plugin.Payments.Iyzipay.Infrastructure
{
    public partial class RouteProvider : IRouteProvider
    {
        public int Priority { get { return -1; } }

        public void RegisterRoutes(IEndpointRouteBuilder endpointRouteBuilder)
        {
            // Admin configuration
            endpointRouteBuilder.MapControllerRoute("Plugin.Payments.Iyzipay.Configure", "Admin/PaymentIyzipay/Configure",
                new { controller = "PaymentIyzipay", action = "Configure" });

            // Payment processing (public controller)
            endpointRouteBuilder.MapControllerRoute("Plugin.Payments.Iyzipay.ProcessPayment", "PaymentIyzipay/ProcessPayment",
                new { controller = "PaymentIyzipayPublic", action = "ProcessPayment" });

            // Payment confirmation (public controller)
            endpointRouteBuilder.MapControllerRoute("Plugin.Payments.Iyzipay.Confirmation", "PaymentIyzipay/Confirmation",
                new { controller = "PaymentIyzipayPublic", action = "Confirmation" });

            // Webhook (public controller)
            endpointRouteBuilder.MapControllerRoute("Plugin.Payments.Iyzipay.Webhook", "Plugins/PaymentIyzipay/Webhook",
                new { controller = "PaymentIyzipayPublic", action = "Webhook" });
        }
    }
}
