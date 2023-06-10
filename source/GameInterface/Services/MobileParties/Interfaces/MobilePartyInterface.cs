using GameInterface.Services.Entity;
using GameInterface.Services.MobileParties.Data;
using GameInterface.Services.MobileParties.Patches;
using GameInterface.Services.ObjectManager;
using System;
using System.Reflection;
using System.Runtime.Serialization;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Map;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Library;

namespace GameInterface.Services.MobileParties.Interfaces;

internal interface IMobilePartyInterface : IGameAbstraction
{
    void ManageNewParty(MobileParty party);

    void RegisterAllPartiesAsControlled(Guid ownerId);
    void UpdatePartyBehavior(PartyBehaviorUpdateData data);
}

internal class MobilePartyInterface : IMobilePartyInterface
{
    private static readonly MethodInfo PartyBase_OnFinishLoadState = typeof(PartyBase).GetMethod("OnFinishLoadState", BindingFlags.NonPublic | BindingFlags.Instance);
    private static readonly MethodInfo AddMobileParty = typeof(CampaignObjectManager).GetMethod("AddMobileParty", BindingFlags.Instance | BindingFlags.NonPublic);

    private readonly IMobilePartyRegistry partyRegistry;
    private readonly IControlledEntityRegistry controlledEntityRegistry;
    private readonly IObjectManager objectManager;

    public MobilePartyInterface(
        IMobilePartyRegistry partyRegistry,
        IControlledEntityRegistry controlledEntityRegistry,
        IObjectManager objectManager)
    {
        this.partyRegistry = partyRegistry;
        this.controlledEntityRegistry = controlledEntityRegistry;
        this.objectManager = objectManager;
    }

    public void UpdatePartyBehavior(PartyBehaviorUpdateData data)
    {
        if (!objectManager.TryGetObject(data.PartyId, out MobileParty party))
            return;

        IMapEntity targetMapEntity = null;
        if (data.HasTarget && !objectManager.TryGetObject(data.TargetId, out targetMapEntity))
            return;

        Settlement targetSettlement = null;
        if (data.HasTargetSettlement && !objectManager.TryGetObject(data.TargetSettlementId, out targetSettlement))
            return;


        Vec2 targetPoint = new Vec2(data.TargetPointX, data.TargetPointY);

        PartyBehaviorPatch.SetAiBehavior(
            party.Ai,
            data.Behavior,
            data.DefaultBehavior,
            targetMapEntity,
            targetPoint,
            targetSettlement
        );
    }

    public void ManageNewParty(MobileParty party)
    {
        AddMobileParty.Invoke(Campaign.Current.CampaignObjectManager, new object[] { party });

        party.IsVisible = true;

        PartyBase_OnFinishLoadState.Invoke(party.Party, null);
    }

    public void RegisterAllPartiesAsControlled(Guid ownerId)
    {
        foreach(var party in partyRegistry)
        {
            controlledEntityRegistry.RegisterAsControlled(ownerId, party.Key);
        }
    }
}
