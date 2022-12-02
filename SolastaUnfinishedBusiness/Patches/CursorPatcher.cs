﻿using System.Diagnostics.CodeAnalysis;
using HarmonyLib;

namespace SolastaUnfinishedBusiness.Patches;

public static class CursorPatcher
{
    [HarmonyPatch(typeof(Cursor), "OnClickSecondaryPointer")]
    [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
    public static class OnClickSecondaryPointer_Patch
    {
        internal static void Postfix(Cursor __instance)
        {
            //PATCH: Enable cancel action on right mouse click
            if (!Main.Settings.EnableCancelEditOnRightMouseClick)
            {
                return;
            }

            if (__instance is CursorCampaignDefault or CursorEditableGraphDefault or CursorLocationBattleDefault
                or CursorLocationEditorDefault or CursorLocationExplorationDefault)
            {
                return;
            }

            var screen = Gui.CurrentLocationScreen;

            // Don't use ?? on Unity object
            if (screen == null)
            {
                screen = Gui.GuiService.GetScreen<UserLocationEditorScreen>();
            }

            if (screen == null || !screen.Visible)
            {
                return;
            }

            Main.Log($"Cancelling {screen.GetType().Name} cursor");
            screen.HandleInput(InputCommands.Id.Cancel);
        }
    }
}
