using System;
using contracts;

namespace saga
{
    public class LogOrderCommand : LogOrder
    {
        public LogOrderCommand(Guid orderId)
        {
            OrderId = orderId;
        }

        public Guid OrderId { get; }
    }
}