﻿using Common.LogicStates;
using Coop.Communication.MessageBroker;
using Coop.Mod.LogicStates;
using Coop.Mod.LogicStates.Client;
using NLog;

namespace Coop.Mod.Client
{
    public class ClientLogic : IClientLogic
    {
        public ILogger Logger { get; }
        public IMessageBroker MessageBroker { get; }
        public IState State { get => _state; set => _state = (IClientState)value; }
        private IClientState _state;
        
        public ClientLogic(ILogger logger, IMessageBroker messageBroker)
        {
            Logger = logger;
            MessageBroker = messageBroker;
            State = new InitialClientState(this, messageBroker);
        }
    }
}