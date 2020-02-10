using System;

namespace contracts
{
    public interface OrderLogged
    {
        Guid Id { get; }
        DateTimeOffset LoggedAt { get; }
    }
}