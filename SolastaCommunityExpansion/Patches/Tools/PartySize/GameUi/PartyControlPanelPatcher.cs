﻿using System;
using System.Diagnostics.CodeAnalysis;
using HarmonyLib;
using SolastaCommunityExpansion.Models;
using UnityEngine;

namespace SolastaCommunityExpansion.Patches.Tools.PartySize.GameUi;

// this patch scales down the party control panel whenever the party size is bigger than 4
//
// this patch is protected by partyCount result
//
[HarmonyPatch(typeof(PartyControlPanel), "OnBeginShow")]
[SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
internal static class PartyControlPanel_OnBeginShow
{
    internal static void Prefix(PartyControlPanel __instance)
    {
        var partyCount = Gui.GameCampaign.Party.CharactersList.Count;

        if (partyCount > DungeonMakerContext.GAME_PARTY_SIZE)
        {
            var scale = (float)Math.Pow(DungeonMakerContext.PARTY_CONTROL_PANEL_DEFAULT_SCALE,
                partyCount - DungeonMakerContext.GAME_PARTY_SIZE);

            __instance.partyPlatesTable.localScale = new Vector3(scale, scale, scale);
            __instance.guestPlatesTable.localScale = new Vector3(scale, scale, scale);
        }
        else
        {
            __instance.partyPlatesTable.localScale = new Vector3(1, 1, 1);
            __instance.guestPlatesTable.localScale = new Vector3(1, 1, 1);
        }
    }
}
