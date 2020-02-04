using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using contracts;
using MassTransit;
using Microsoft.AspNetCore.Mvc;

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
        [HttpGet("{id}")]
        public ActionResult<IOrder> Get(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return NotFound();

            if (!Guid.TryParse(id, out var idValue))
                return NotFound();

            var order = this.orderService.Get(idValue);

            if (order == null)
                return NotFound();

            return new ActionResult<IOrder>(order);
        }

        // POST: api/orders
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Order order)
        {
            var result = this.orderService.Create(order);

            var sendEndpoint = await this.bus.GetSendEndpoint(new Uri("exchange:contracts:OrderCreated"));

            await sendEndpoint.Send<OrderCreated>(new
            {
                OrderId = result.Id
            });

            return Ok(result);
        }

        [HttpPut("{id}")]
        public IActionResult Put(string id, [FromBody] Order order)
        {
            if (!Guid.TryParse(id, out var idValue))
                return BadRequest();

            var current = (Order) this.orderService.Get(idValue);

            if (current == null)
                return NotFound();

            current.Amount = order.Amount;

            var updated = this.orderService.Update(current);

            if (updated == null)
                return Conflict();

            return Ok(updated);
        }

        // DELETE: api/orders/abc
        [HttpDelete("{id}")]
        public IActionResult Delete(string id)
        {
            if (!Guid.TryParse(id, out var idValue))
                return BadRequest();

            var order = this.orderService.Delete(idValue);

            if (order == null)
                return NotFound();

            return Ok(order);
        }
    }
}