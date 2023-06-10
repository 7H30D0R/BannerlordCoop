using System;
using System.Collections.Generic;
using System.Linq;
using Common.Messaging;
using GameInterface.Services.Scope.Data;
using GameInterface.Services.Scope.Enums;
using GameInterface.Services.Scope.Messages;
using HarmonyLib;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Map;
using TaleWorlds.CampaignSystem.Party;

namespace GameInterface.Services.Scope
{
    internal interface IScopeManager
    {
        public void CreateScope(Guid clientId);
        public void RemoveScope(Guid clientId);
        public void SetClientHero(Guid clientId, Hero hero);
    }

    [HarmonyPatch]
    internal class ScopeManager : IScopeManager
    {
        private static ScopeManager Instance;

        private static readonly float UpdatesPerSecond = 10;
        private static float timeSinceLastUpdate = 0;

        private List<ClientScope> scopes = new List<ClientScope>();

        public ScopeManager() 
        {
            Instance = this;
        }
        public void CreateScope(Guid clientId)
        {
            scopes.Add(new ClientScope(clientId));
        }

        public void RemoveScope(Guid clientId)
        {
            scopes.Remove(scopes.Find(scope => scope.ClientId == clientId));
        }

        public void SetClientHero(Guid clientId, Hero hero)
        {
            ClientScope scope = scopes.Find(scope => scope.ClientId == clientId);
            
            if (scope == null) 
                return;

            scope.Hero = hero;
        }

        public void Update()
        {
            foreach (ClientScope scope in scopes)
            {
                if (scope.Hero == null) continue;

                List<(string, EntityType)> newScope = new List<(string, EntityType)>();

                LocatableSearchData<MobileParty> locatableSearchData =
                    MobileParty.StartFindingLocatablesAroundPosition(scope.Position, scope.Range);

                MobileParty party = MobileParty.FindNextLocatable(ref locatableSearchData);
                while (party != null)
                {
                    newScope.Add( (party.StringId, EntityType.MobileParty) );

                    party = MobileParty.FindNextLocatable(ref locatableSearchData);
                }

                foreach ((string Id, EntityType Type) entity in scope.EntitiesInScope.Except(newScope))
                {
                    MessageBroker.Instance.Publish(this, new EntityLeftScope(
                        scope.ClientId,
                        entity.Id,
                        entity.Type
                    ));
                }

                foreach ((string Id, EntityType Type) entity in newScope.Except(scope.EntitiesInScope))
                {
                    MessageBroker.Instance.Publish(this, new EntityEnteredScope(
                        scope.ClientId,
                        entity.Id,
                        entity.Type
                    ));
                }

                scope.EntitiesInScope = newScope;
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Campaign), "RealTick")]
        private static bool RealTickPrefix(ref Campaign __instance, float realDt)
        {
            if (ModInformation.IsClient)
                return true;

            timeSinceLastUpdate += realDt;
            if (timeSinceLastUpdate > 1 / UpdatesPerSecond)
            {
                timeSinceLastUpdate = 0;
                Instance.Update();
            }

            return true;
        }
    }
}
