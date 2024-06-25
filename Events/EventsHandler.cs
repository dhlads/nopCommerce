using Azure.Identity;
using Microsoft.Extensions.Logging;
using Nop.Core.Domain.Customers;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Events;

namespace Nop.Plugin.WholeSalePet.Customization.Events
{
    public class CustomerRegisteredConsumer : IConsumer<CustomerRegisteredEvent>
    {
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly ILogger<CustomerRegisteredConsumer> _logger;
        private readonly ICustomerService _customerService;

        public CustomerRegisteredConsumer(
            IGenericAttributeService genericAttributeService,
            ICustomerService customerService,
            ILogger<CustomerRegisteredConsumer> logger            
        )
        {
            _genericAttributeService = genericAttributeService;
            _customerService = customerService;
            _logger = logger;            
        }
        
        public async Task HandleEventAsync(CustomerRegisteredEvent eventMessage)
        {
            //var customer = eventMessage.Customer;            
            // TODO: Here we are going to call Klaviyto system

            await Task.CompletedTask;
        }
    }
}
