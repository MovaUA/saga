using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;

namespace api.Endpoints.Orders
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService orderService;

        public OrdersController(IOrderService orderService)
        {
            this.orderService = orderService;
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
        public IActionResult Post([FromBody] Order order)
        {
            return Ok(this.orderService.Create(order));
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