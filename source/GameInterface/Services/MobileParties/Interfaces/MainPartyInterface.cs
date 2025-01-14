﻿using Common;
using TaleWorlds.CampaignSystem.Party;

namespace GameInterface.Services.MobileParties.Interfaces;

internal interface IMainPartyInterface : IGameAbstraction
{
    void RemoveMainParty();
}

internal class MainPartyInterface : IMainPartyInterface
{
    public void RemoveMainParty()
    {
        GameLoopRunner.RunOnMainThread(() =>
        {
            if(MobileParty.MainParty != null)
            {
                MobileParty.MainParty.RemoveParty();
            }
            
        }, bBlocking: false);
    }
}
