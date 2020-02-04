using System;
using Automatonymous;
using MassTransit.MongoDbIntegration.Saga;
using MongoDB.Bson.Serialization.Attributes;

namespace saga
{
    public class OrderState : SagaStateMachineInstance, IVersionedSaga
    {
        public int CurrentState { get; set; }
        public int Version { get; set; }

        [BsonId] public Guid CorrelationId { get; set; }
    }
}