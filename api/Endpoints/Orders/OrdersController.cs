using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using contracts;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;

namespace api.Endpoints.Orders
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly IBus bus;
        private readonly IOrderService orderService;

        public OrdersController(
            IOrderService orderService,
            IBus bus
        )
        {
            this.orderService = orderService;
            this.bus = bus;
        }

        // GET: api/orders
        [HttpGet]
        public IEnumerable<IOrder> Get()
        {
            return this.orderService.Get();
        }

        // GET: api/orders/abc
        [HttpGet("{id:length(24)}", Name = "Get")]
        public ActionResult<IOrder> Get(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return NotFound();

            if (!ObjectId.TryParse(id, out _))
                return NotFound();

            var order = this.orderService.Get(id);

            if (order == null)
                return NotFound();

            return new ActionResult<IOrder>(order);
        }

        // POST: api/orders
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Order order)
        {
            var result = this.orderService.Create(order);

            var sendEndpoint = await this.bus.GetSendEndpoint(new Uri("exchange:contracts:OrderSubmitted"));

            await sendEndpoint.Send<OrderSubmitted>(new
            {
                OrderId = result.Id
            });

            return Ok(result);
        }


        public async Task NotifyOrderSubmitted(IPublishEndpoint publishEndpoint)
        {
            await publishEndpoint.Publish<OrderSubmitted>(new
            {
                OrderId = "27",
                OrderDate = DateTime.UtcNow
            });
        }

        // DELETE: api/orders/abc
        [HttpDelete("{id:length(24)}")]
        public IActionResult Delete(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return NotFound();

            if (!ObjectId.TryParse(id, out _))
                return NotFound();

            var order = this.orderService.Get(id);

            if (order == null)
                return NotFound();

            this.orderService.Remove(order);

            return Ok(order);
        }
    }
}