using System;
using Automatonymous;
using MassTransit.MongoDbIntegration.Saga;
using MongoDB.Bson.Serialization.Attributes;

namespace saga
{
    public class OrderState : SagaStateMachineInstance, IVersionedSaga
    {
        [BsonId]
        public Guid CorrelationId { get; set; }

        public string CurrentState { get; set; }
        public int Version { get; set; }
        public int Amount { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset? LoggedAt { get; set; }
    }
}