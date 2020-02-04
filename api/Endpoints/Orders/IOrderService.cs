using System;
using System.Collections.Generic;

namespace api.Endpoints.Orders
{
    public interface IOrderService
    {
        IEnumerable<IOrder> Get();
        IOrder Get(Guid id);
        IOrder Create(IOrder order);
        IOrder Update(IOrder order);
        IOrder Delete(Guid id);
    }
}