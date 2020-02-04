using System;

namespace contracts
{
    public interface OrderLogged
    {
        Guid OrderId { get; }
    }
}