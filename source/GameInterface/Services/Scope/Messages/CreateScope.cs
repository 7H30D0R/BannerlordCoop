using Common.Messaging;
using System;

namespace GameInterface.Services.Scope.Messages
{
    public record CreateScope : ICommand
    {
        public Guid ClientId { get; }

        public CreateScope(Guid clientId)
        {
            ClientId = clientId;
        }
    }
}