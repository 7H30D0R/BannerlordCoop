﻿using Common.Messaging;
using ProtoBuf;

namespace Coop.Core.Server.Connections.Messages
{
    [ProtoContract(SkipConstructor = true)]
    public record NetworkTransferedHero : IEvent
    {
        [ProtoMember(1)]
        public byte[] PlayerHero { get; }

        public NetworkTransferedHero(byte[] playerHero)
        {
            PlayerHero = playerHero;
        }
    }
}
