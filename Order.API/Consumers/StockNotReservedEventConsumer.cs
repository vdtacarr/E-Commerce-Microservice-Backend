using MassTransit;
using Order.API.Models.Context;
using Order.API.Models.Enums;
using System;
using System.Threading.Tasks;
using Shared.Events;

namespace Order.API.Consumers
{
    public class StockNotReservedEventConsumer : IConsumer<StockNotReservedEvent>
    {
        readonly ApplicationDbContext _context;
        public StockNotReservedEventConsumer(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task Consume(ConsumeContext<StockNotReservedEvent> context)
        {
            Models.Order order = await _context.FindAsync<Models.Order>(context.Message.OrderId);
            if (order != null)
            {
                order.OrderStatus = OrderStatus.Fail;
                await _context.SaveChangesAsync();
                Console.WriteLine(context.Message.Message);
            }

        }
    }
}
