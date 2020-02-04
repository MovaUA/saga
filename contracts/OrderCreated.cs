using System;

namespace contracts
{
    public interface OrderCreated
    {
        Guid OrderId { get; }
    }
}