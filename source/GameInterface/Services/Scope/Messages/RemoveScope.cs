using Common.Messaging;
using System;

namespace GameInterface.Services.Scope.Messages
{
    public record RemoveScope : ICommand
    {
        public Guid ClientId { get; }

        public RemoveScope(Guid clientId)
        {
            ClientId = clientId;
        }
    }
}