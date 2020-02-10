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
                    e.CorrelateById(selector: ctx => ctx.Message.Id);

                    e.InsertOnInitial = true;

                    e.SetSagaFactory(factoryMethod: ctx => new OrderState
                    {
                        CorrelationId = ctx.Message.Id,
                        Amount = ctx.Message.Amount,
                        CreatedAt = ctx.Message.CreatedAt
                    });
                }
            );

            Event(
                propertyExpression: () => OrderLogged,
                configureEventCorrelation: e => { e.CorrelateById(selector: ctx => ctx.Message.Id); }
            );

            //InstanceState(instanceStateProperty: x => x.CurrentState, Created, Logged);
            InstanceState(instanceStateProperty: x => x.CurrentState);

            Initially(
                When(@event: OrderCreated)
                    .SendAsync(
                        messageFactory: ctx => ctx.Init<LogOrder>(values: ctx.Data)
                    )
                    .TransitionTo(toState: Created)
            );

            During(
                state: Created,
                When(@event: OrderLogged)
                    .Then(action: x => x.Instance.LoggedAt = x.Data.LoggedAt)
                    .TransitionTo(toState: Logged)
            );

            //DuringAny(When(@event: OrderLogged).Finalize());

            //SetCompletedWhenFinalized();
        }
    }
}