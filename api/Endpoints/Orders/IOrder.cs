﻿using System;

namespace api.Endpoints.Orders
{
    public interface IOrder
    {
        Guid Id { get; }
        int Version { get; }
        int Amount { get; }
        DateTimeOffset CreatedAt { get; }
        DateTimeOffset? UpdatedAt { get; }
        DateTimeOffset? DeletedAt { get; }
    }
}