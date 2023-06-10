using GameInterface.Services.Scope.Enums;
using System;
using System.Collections.Generic;
using System.Text;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Library;

namespace GameInterface.Services.Scope.Data
{
    internal class ClientScope
    {
        private static readonly float ScopeRangeMultiplier = 2f;
        public Guid ClientId { get; }
        public Hero Hero { get; set; }
        public MobileParty Party => Hero.PartyBelongedTo;
        public Vec2 Position => Party.GetPosition2D;
        public float Range => Party.SeeingRange * ScopeRangeMultiplier;
        public List<(string, EntityType)> EntitiesInScope = new List<(string, EntityType)>();

        public ClientScope(Guid clientId, Hero hero = null) 
        { 
            ClientId = clientId;
            Hero = hero;
        }
    }
}
