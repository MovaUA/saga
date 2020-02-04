using System;

namespace api.Endpoints.Orders
{
    public class Order : IOrder
    {
        public Guid Id { get; set; }
        public int Version { get; set; }
        public int Amount { get; set; }
    }
}