using MassTransit;
using Shared.Events;
using System;
using System.Threading.Tasks;

namespace Notification.API.Consumers
{
    public class PaymentFailedConsumer : IConsumer<OrderCreatedEvent>
    {
        readonly IPublishEndpoint _publishEndpoint;

        public PaymentFailedConsumer(IPublishEndpoint publishEndpoint)
        {
            _publishEndpoint = publishEndpoint;
        }

        public async Task Consume(ConsumeContext<OrderCreatedEvent> context)
        {
            Console.WriteLine(context.Message.OrderItems);
        }
    }
}
