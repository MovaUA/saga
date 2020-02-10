using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using contracts;
using MassTransit;
using Microsoft.AspNetCore.Mvc;

namespace api.Endpoints.Orders
{
    [Route(template: "api/[controller]")]
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
        [HttpGet(template: "{id}")]
        public ActionResult<IOrder> Get(string id)
        {
            if (string.IsNullOrWhiteSpace(value: id))
                return NotFound();

            if (!Guid.TryParse(input: id, result: out var idValue))
                return NotFound();

            var order = this.orderService.Get(id: idValue);

            if (order == null)
                return NotFound();

            return new ActionResult<IOrder>(value: order);
        }

        // POST: api/orders
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Order order)
        {
            var createdOrder = this.orderService.Create(order: order);

            var sendEndpoint =
                await this.bus.GetSendEndpoint(address: new Uri(uriString: "exchange:contracts:OrderCreated"));

            await sendEndpoint.Send<OrderCreated>(values: createdOrder);

            return Ok(value: createdOrder);
        }

        [HttpPut(template: "{id}")]
        public IActionResult Put(string id, [FromBody] Order order)
        {
            if (!Guid.TryParse(input: id, result: out var idValue))
                return BadRequest();

            var current = (Order) this.orderService.Get(id: idValue);

            if (current == null)
                return NotFound();

            current.Amount = order.Amount;

            var updated = this.orderService.Update(order: current);

            if (updated == null)
                return Conflict();

            return Ok(value: updated);
        }

        // DELETE: api/orders/abc
        [HttpDelete(template: "{id}")]
        public IActionResult Delete(string id)
        {
            if (!Guid.TryParse(input: id, result: out var idValue))
                return BadRequest();

            var order = this.orderService.Delete(id: idValue);

            if (order == null)
                return NotFound();

            return Ok(value: order);
        }
    }
}