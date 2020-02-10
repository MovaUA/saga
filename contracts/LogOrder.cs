using System;

namespace contracts
{
    public interface LogOrder
    {
        Guid Id { get; }
        int Amount { get; }
        DateTimeOffset CreatedAt { get; }
    }
}