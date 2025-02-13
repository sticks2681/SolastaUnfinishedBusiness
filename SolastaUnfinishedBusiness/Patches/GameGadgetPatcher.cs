﻿using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using HarmonyLib;
using SolastaUnfinishedBusiness.Api.Extensions;
using SolastaUnfinishedBusiness.Models;

namespace SolastaUnfinishedBusiness.Patches;

public static class GameGadgetPatcher
{
    [HarmonyPatch(typeof(GameGadget), "ComputeIsRevealed")]
    [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
    public static class ComputeIsRevealed_Patch
    {
        public static void Postfix(GameGadget __instance, ref bool __result)
        {
            //PATCH: HideExitsAndTeleportersGizmosIfNotDiscovered
            //hides certain element from the map on custom dungeons unless already discovered
            if (!Main.Settings.HideExitsAndTeleportersGizmosIfNotDiscovered
                || Gui.GameLocation.UserLocation == null
                || !__instance.Revealed)
            {
                return;
            }

            GameUiContext.ComputeIsRevealedExtended(__instance, ref __result);
        }
    }

    [HarmonyPatch(typeof(GameGadget), "SetCondition")]
    [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
    public static class SetCondition_Patch
    {
        public static void Postfix(GameGadget __instance, int conditionIndex, bool state)
        {
            //BUGFIX: fix issue where a button activator fires Triggered event with state=true first time and
            // correctly activates attached gadget, but fires Triggered event with state=false second time and
            // doesn't activate attached gadget.
            if (conditionIndex >= 0 && conditionIndex < __instance.conditionNames.Count)
            {
                var param = __instance.conditionNames[conditionIndex];

                // NOTE: only handling 'button activator'
                // TODO: check other activators for same issue
                if (param == GameGadgetExtensions.Triggered && !state &&
                    __instance.UniqueNameId.StartsWith("ActivatorButton"))
                {
                    // Reset 'Triggered' to true otherwise we have to press the activator twice
                    __instance.SetCondition(conditionIndex, true, new List<GameLocationCharacter>());
                }
            }

            //PATCH: HideExitsAndTeleportersGizmosIfNotDiscovered
            if (!Main.Settings.HideExitsAndTeleportersGizmosIfNotDiscovered)
            {
                return;
            }

            GameUiContext.HideExitsAndTeleportersGizmosIfNotDiscovered(__instance, conditionIndex, state);
        }
    }
}
