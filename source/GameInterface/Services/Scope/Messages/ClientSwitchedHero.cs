using Common.Messaging;
using System;

namespace GameInterface.Services.Scope.Messages
{
    public record ClientSwitchedHero : IEvent
    {
        public Guid ClientId { get; }
        public string HeroId { get; }

        public ClientSwitchedHero(Guid clientId, string heroId)
        {
            ClientId = clientId;
            HeroId = heroId;
        }
    }
}