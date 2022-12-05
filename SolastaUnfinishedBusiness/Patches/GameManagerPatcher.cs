﻿using System.Diagnostics.CodeAnalysis;
using System.Linq;
using HarmonyLib;
using SolastaUnfinishedBusiness.Models;

namespace SolastaUnfinishedBusiness.Patches;

public static class GameManagerPatcher
{
    [HarmonyPatch(typeof(GameManager), "BindPostDatabase")]
    [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
    public static class BindPostDatabase_Patch
    {
        public static void Postfix()
        {
            //PATCH: loads all mod contexts
            BootContext.Startup();
        }
    }

    [HarmonyPatch(typeof(GameManager), "BindServiceSettings")]
    internal static class BindServiceSettings_Patch
    {
        public static void Prefix(GameManager __instance)
        {
            //PATCH: add unofficial languages before game tries to load the game settings xml
            var languageByCode = __instance.languageByCode;

            if (languageByCode == null)
            {
                return;
            }

            foreach (var language in TranslatorContext.Languages
                         .Where(language => !languageByCode.ContainsKey(language.Code)))
            {
                languageByCode.Add(language.Code, language.Text);
            }
        }
    }
}
