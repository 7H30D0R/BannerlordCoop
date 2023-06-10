using Common.Messaging;
using GameInterface.Services.Scope.Enums;
using System;

namespace GameInterface.Services.Scope.Messages
{
    public record EntityEnteredScope : IEvent
    {
        public Guid ClientId { get; }
        public string EntityId { get; }
        public EntityType EntityType { get; }

        public EntityEnteredScope(Guid clientId, string entityId, EntityType entityType) 
        { 
            ClientId = clientId;
            EntityId = entityId;
            EntityType = entityType;
        }
    }
}