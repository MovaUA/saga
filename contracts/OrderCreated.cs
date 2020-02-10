using System;

namespace contracts
{
    public interface OrderCreated
    {
        Guid Id { get; }
        int Amount { get; }
        DateTimeOffset CreatedAt { get; }
    }
}