﻿using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Order.API.Models;
using Order.API.Models.Context;
using Order.API.Models.Enums;
using Order.API.ViewModels;
using Shared;
using Shared.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Order.API.Controllers
{
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase
    {
        readonly ApplicationDbContext _applicationDbContext;
        readonly IPublishEndpoint _publishEndpoint;
        public OrdersController(
            ApplicationDbContext applicationDbContext,
            IPublishEndpoint publishEndpoint)
        {
            _applicationDbContext = applicationDbContext;
            _publishEndpoint = publishEndpoint;
        }

        [Route("[action]")]
        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] OrderVM model)
        {
            Order.API.Models.Order order = new()
            {
                BuyerId = model.BuyerId,
                OrderItems = model.OrderItems.Select(oi => new OrderItem
                {
                    Count = oi.Count,
                    Price = oi.Price,
                    ProductId = oi.ProductId
                }).ToList(),
                OrderStatus = OrderStatus.Suspend,
                TotalPrice = model.OrderItems.Sum(oi => oi.Count * oi.Price),
                CreatedDate = DateTime.Now
            };

            await _applicationDbContext.AddAsync<Order.API.Models.Order>(order);

            await _applicationDbContext.SaveChangesAsync();

            OrderCreatedEvent orderCreatedEvent = new()
            {
                OrderId = order.Id,
                BuyerId = order.BuyerId,
                TotalPrice = order.TotalPrice,
                OrderItems = order.OrderItems.Select(oi => new OrderItemMessage
                {
                    Price = oi.Price,
                    Count = oi.Count,
                    ProductId = oi.ProductId
                }).ToList()
            };
            await _publishEndpoint.Publish(orderCreatedEvent);
            return Ok(true);
        }
    }
}
