using System.Collections.Generic;

namespace api.Endpoints.Orders
{
    public interface IOrderService
    {
        IEnumerable<IOrder> Get();
        IOrder Get(string id);
        IOrder Create(IOrder order);
        void Remove(IOrder order);
        void Remove(string id);
    }
}