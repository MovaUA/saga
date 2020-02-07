using System.Diagnostics.CodeAnalysis;
using Automatonymous;
using contracts;
using MassTransit;

namespace saga
{
    [SuppressMessage(category: "ReSharper", checkId: "UnusedAutoPropertyAccessor.Local")]
    public class OrderStateMachine : MassTransitStateMachine<OrderState>
    {
        public State Created { get; private set; }
        public State Logged { get; private set; }

        public Event<OrderCreated> OrderCreated { get; private set; }
        public Event<OrderLogged> OrderLogged { get; private set; }

        public OrderStateMachine()
        {
            Event(
                propertyExpression: () => OrderCreated,
                configureEventCorrelation: e =>
                {
                    e.CorrelateById(selector: ctx => ctx.Message.OrderId);

                    //e.InsertOnInitial = true;

                    //e.SetSagaFactory(factoryMethod: ctx => new OrderState { CorrelationId = ctx.Message.OrderId });
                }
            );

            Event(
                propertyExpression: () => OrderLogged,
                configureEventCorrelation: e => { e.CorrelateById(selector: ctx => ctx.Message.OrderId); }
            );

            InstanceState(instanceStateProperty: x => x.CurrentState);

            //InstanceState(instanceStateProperty: x => x.CurrentState, Created, Logged);

            Initially(
                When(@event: OrderCreated)
                    .SendAsync(
                        messageFactory: ctx => ctx.Init<LogOrder>(values: new {OrderId = ctx.Instance.CorrelationId})
                    )
                    .TransitionTo(toState: Created)
            );

            During(
                state: Created,
                When(@event: OrderLogged)
                    .TransitionTo(toState: Logged)
            );
        }
    }
}