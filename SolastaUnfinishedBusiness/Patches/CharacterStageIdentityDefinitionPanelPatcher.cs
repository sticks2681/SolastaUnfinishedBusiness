﻿using System.Diagnostics.CodeAnalysis;
using HarmonyLib;
using SolastaUnfinishedBusiness.Models;

namespace SolastaUnfinishedBusiness.Patches;

public static class CharacterStageIdentityDefinitionPanelPatcher
{
    //PATCH: avoids issues during RESPEC if a hero with same name is in the pool
    [HarmonyPatch(typeof(CharacterStageIdentityDefinitionPanel), "CanProceedToNextStage")]
    [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
    public static class CanProceedToNextStage_Patch
    {
        public static void Postfix(CharacterStageIdentityDefinitionPanel __instance, ref bool __result)
        {
            if (Main.Settings.EnableRespec && ToolsContext.FunctorRespec.IsRespecing &&
                !string.IsNullOrEmpty(__instance.currentHero.Name))
            {
                __result = true;
            }
        }
    }
}
