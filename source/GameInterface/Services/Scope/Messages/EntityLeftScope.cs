using Common.Messaging;
using GameInterface.Services.Scope.Enums;
using System;

namespace GameInterface.Services.Scope.Messages
{
    public record EntityLeftScope : IEvent
    {
        public Guid ClientId { get; }
        public string EntityId { get; }
        public EntityType EntityType { get; }

        public EntityLeftScope(Guid clientId, string entityId, EntityType entityType)
        {
            ClientId = clientId;
            EntityId = entityId;
            EntityType = entityType;
        }
    }
}