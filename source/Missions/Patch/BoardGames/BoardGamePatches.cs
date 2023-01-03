﻿using Common.Messaging;
using Coop.Mod.Missions;
using HarmonyLib;
using Missions.Network;
using SandBox;
using SandBox.BoardGames;
using SandBox.BoardGames.AI;
using SandBox.BoardGames.MissionLogics;
using SandBox.BoardGames.Pawns;
using SandBox.BoardGames.Tiles;
using SandBox.Source.Missions.AgentBehaviors;
using System;
using System.Collections.Generic;
using System.Reflection;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace Coop.Mod.Patch.BoardGames
{

    [HarmonyPatch(typeof(BoardGameAgentBehavior), nameof(BoardGameAgentBehavior.RemoveBoardGameBehaviorOfAgent))]
    public class RemoveBoardGameBehaviorOfAgentPatch
    {
        static bool Prefix(Agent ownerAgent)
        {
            return BoardGameLogic.IsPlayingOtherPlayer == false;
        }

        static void Postfix(Agent ownerAgent)
        {
            BoardGameLogic.IsPlayingOtherPlayer = false;
        }
    }

    [HarmonyPatch(typeof(MissionBoardGameLogic), "StartConversationWithOpponentAfterGameEnd")]
    public class StartConversationAfterGamePatch
    {
        private static readonly PropertyInfo AgentNavigatorPropertyInfo = typeof(CampaignAgentComponent).GetProperty("AgentNavigator");
        public static event Action<MissionBoardGameLogic> OnGameOver;
        static bool Prefix(MissionBoardGameLogic __instance, Agent conversationAgent)
        {
            if (NetworkAgentRegistry.Instance.AgentToId.ContainsKey(conversationAgent))
            {
                OnGameOver?.Invoke(__instance);

                //Set AgentNavigator to null as this gets set in SetGameOver by default and breaks all future interactions
                AgentNavigatorPropertyInfo.SetValue(conversationAgent.GetComponent<CampaignAgentComponent>(), null);
                return false;
            }
            else
            {
                return true;
            }
        }
    }

    [HarmonyPatch(typeof(BoardGameBase), "HandlePlayerInput")]
    public class HandlePlayerInputPatch
    {
        public static event Action<Move> OnHandlePlayerInput;
        static void Postfix(ref BoardGameBase __instance, ref Move __result)
        {
            OnHandlePlayerInput?.Invoke(__result);
        }
    }

    [HarmonyPatch(typeof(MissionBoardGameLogic), nameof(MissionBoardGameLogic.ForfeitGame))]
    public class ForfeitGamePatch
    {
        public static event Action<MissionBoardGameLogic> OnForfeitGame; 
        static bool Prefix(MissionBoardGameLogic __instance)
        {
            if (BoardGameLogic.IsPlayingOtherPlayer)
            {
                OnForfeitGame?.Invoke(__instance);
            }

             return true;

        }
    }

    [HarmonyPatch]
    public class Board
    {
        static IEnumerable<MethodBase> TargetMethods()
        {
            yield return AccessTools.Method(typeof(BoardGameAIBase), nameof(BoardGameAIBase.WantsToForfeit));
            yield return AccessTools.Method(typeof(BoardGameAISeega), nameof(BoardGameAIBase.WantsToForfeit));
        }

        static bool Postfix(bool result)
        {
            if (BoardGameLogic.IsPlayingOtherPlayer) return false;
            return result;
        }
    }

    [HarmonyPatch(typeof(BoardGameAIBase), "CalculateMovementStageMoveOnSeparateThread")]
    public class CalculateMovePatch
    {
        public static bool Prefix()
        {
            return !BoardGameLogic.IsPlayingOtherPlayer;
        }
    }

    [HarmonyPatch(typeof(BoardGameKonane), "HandlePreMovementStage")]
    public class HandlePreMovementStagePatch
    {
        public static event Action OnHandlePreMovementStage;
        public static void Prefix()
        {
            OnHandlePreMovementStage?.Invoke();
        }
    }

    [HarmonyPatch(typeof(BoardGameSeega), "FocusBlockingPawns")]
    public class FocusBlockingPawnsPatch
    {
        public static bool ForceRemove = false;
        public static void Postfix()
        {
            if (BoardGameLogic.IsPlayingOtherPlayer)
            {
                ForceRemove = true;
            }
        }
    }

    [HarmonyPatch(typeof(BoardGameBase), nameof(BoardGameBase.SetPawnCaptured))]
    public class SetPawnCapturedPatch
    {
        public static event Action<PawnBase> OnSetPawnCaptured;
        public static void Postfix(PawnBase pawn, bool fake)
        {
            OnSetPawnCaptured?.Invoke(pawn);
        }
    }

    [HarmonyPatch(typeof(BoardGameSeega), "PreplaceUnits")]
    public class PreplaceUnitsPatch
    {
        public static event Action OnPreplaceUnits;

        static bool Prefix()
        {

            if (BoardGameLogic.IsPlayingOtherPlayer && BoardGameLogic.IsChallenged) { return false; }

            OnPreplaceUnits?.Invoke();

            return true;

        }
    }
}
