using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Nop.Web.Framework.Mvc.Routing;

namespace Nop.Plugin.WholeSalePet.Customization.Infrastructure
{
    public class RouteProvider : IRouteProvider
    {
        /// <summary>
        /// Register routes
        /// </summary>
        /// <param name="endpointRouteBuilder">Route builder</param>
        public void RegisterRoutes(IEndpointRouteBuilder endpointRouteBuilder)
        {

            // Override default Register route
            endpointRouteBuilder.MapControllerRoute(
                name: "Register",
                pattern: "customer/register",
                defaults: new { controller = "PluginCustomer", action = "Register" }
            );

            // Override default customer account route
            endpointRouteBuilder.MapControllerRoute(name: "CustomerInfo",
                pattern: "customer/myaccount",
                defaults: new { controller = "PluginCustomer", action = "Info" });
        }

        public int Priority => 2000; // THIS IS REALLY IMPORTANT
    }
}