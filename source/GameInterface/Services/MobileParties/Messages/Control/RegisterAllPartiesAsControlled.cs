﻿using Common.Messaging;
using System;

namespace GameInterface.Services.MobileParties.Messages.Control;

/// <summary>
/// Registers all parties in the game as controlled
/// </summary>
/// <remarks>
/// This is meant to be used by the server at startup so the server can
/// control all AI movement.
/// </remarks>
public readonly struct RegisterAllPartiesAsControlled : ICommand
{
    public Guid TransactionID => Guid.Empty;
}
