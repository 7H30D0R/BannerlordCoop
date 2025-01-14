﻿using Common.Messaging;

namespace GameInterface.Services.MobileParties.Messages.Control
{
    /// <summary>
    /// Updates whether a mobile party is controlled locally. 
    /// </summary>
    public record UpdateMobilePartyControl : ICommand
    {
        public string PartyId { get; }

        /// <summary>
        /// Indicates whether control is being revoked (true) or granted (false).
        /// </summary>
        public bool IsRevocation { get; }

        /// <param name="isRevocation">Indicates whether control is being revoked (true) or granted (false).</param>
        public UpdateMobilePartyControl(string partyId, bool isRevocation = false)
        {
            PartyId = partyId;
            IsRevocation = isRevocation;
        }
    }
}
