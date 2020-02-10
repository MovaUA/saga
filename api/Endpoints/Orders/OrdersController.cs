using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using contracts;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace api.Endpoints.Orders
{
    [Route(template: "api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly ILogger logger;
        private readonly IOrderService orderService;
        private readonly ISendEndpointProvider sendEndpointProvider;

        public OrdersController(
            IOrderService orderService,
            ISendEndpointProvider sendEndpointProvider,
            ILoggerFactory loggerFactory
        )
        {
            this.orderService = orderService;
            this.sendEndpointProvider = sendEndpointProvider;
            this.logger = loggerFactory.CreateLogger<OrdersController>();
        }

        // GET: api/orders
        [HttpGet]
        public IEnumerable<IOrder> Get()
        {
            this.logger.LogInformation(message: "GET /api/orders");

            return this.orderService.Get();
        }

        // GET: api/orders/abc
        [HttpGet(template: "{id:guid}")]
        public ActionResult<IOrder> Get(Guid id)
        {
            this.logger.LogInformation(message: "GET /api/orders/{0}", id);

            var order = this.orderService.Get(id: id);

            if (order == null)
                return NotFound();

            return new ActionResult<IOrder>(value: order);
        }

        // POST: api/orders
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Order order)
        {
            this.logger.LogInformation(message: "POST /api/orders {0}", JsonConvert.SerializeObject(value: order));

            var createdOrder = this.orderService.Create(order: order);


            await this.sendEndpointProvider.Send<OrderCreated>(values: createdOrder);

            return Ok(value: createdOrder);
        }

        [HttpPut(template: "{id:guid}")]
        public IActionResult Put(Guid id, [FromBody] Order order)
        {
            this.logger.LogInformation(message: "PUT /api/orders/{0} {1}", id,
                JsonConvert.SerializeObject(value: order));

            var current = (Order) this.orderService.Get(id: id);

            if (current == null)
                return NotFound();

            current.Amount = order.Amount;

            var updated = this.orderService.Update(order: current);

            if (updated == null)
                return Conflict();

            return Ok(value: updated);
        }

        // DELETE: api/orders/abc
        [HttpDelete(template: "{id:guid}")]
        public IActionResult Delete(Guid id)
        {
            this.logger.LogInformation(message: "DELETE /api/orders/{0}", id);

            var order = this.orderService.Delete(id: id);

            if (order == null)
                return NotFound();

            return Ok(value: order);
        }
    }
}