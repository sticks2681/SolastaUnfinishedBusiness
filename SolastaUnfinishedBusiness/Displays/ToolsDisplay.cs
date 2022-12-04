﻿using System;
using System.Linq;
using HarmonyLib;
using SolastaUnfinishedBusiness.Api.Infrastructure;
using SolastaUnfinishedBusiness.Api.ModKit;
using SolastaUnfinishedBusiness.Models;
using UnityEngine;

namespace SolastaUnfinishedBusiness.Displays;

internal static class ToolsDisplay
{
    internal const float DefaultFastTimeModifier = 1.5f;

    private static readonly (string, Func<ItemDefinition, bool>)[] ItemsFilters =
    {
        (Gui.Localize("MainMenu/&CharacterSourceToggleAllTitle"), _ => true),
        (Gui.Localize("Equipment/&ItemTypeAmmunitionTitle"), a => a.IsAmmunition),
        (Gui.Localize("MerchantCategory/&ArmorTitle"), a => a.IsArmor),
        (Gui.Localize("MerchantCategory/&DocumentTitle"), a => a.IsDocument),
        (Gui.Localize("Equipment/&ItemTypeSpellFocusTitle"), a => a.IsFocusItem),
        (Gui.Localize("Screen/&TravelFoodTitle"), a => a.IsFood),
        (Gui.Localize("Equipment/&ItemTypeLightSourceTitle"), a => a.IsLightSourceItem),
        (Gui.Localize("Equipment/&SpellbookTitle"), a => a.IsSpellbook),
        (Gui.Localize("Equipment/&ItemTypeStarterPackTitle"), a => a.IsStarterPack),
        (Gui.Localize("Screen/&ProficiencyToggleToolTitle"), a => a.IsTool),
        (Gui.Localize("Merchant/&DungeonMakerMagicalDevicesTitle"), a => a.IsUsableDevice),
        (Gui.Localize("MerchantCategory/&WeaponTitle"), a => a.IsWeapon),
        (Gui.Localize("Tooltip/&TagFactionRelicTitle"), a => a.IsFactionRelic)
    };

    private static readonly string[] ItemsFiltersLabels = ItemsFilters.Select(x => x.Item1).ToArray();

    private static readonly (string, Func<ItemDefinition, bool>)[] ItemsItemTagsFilters =
        TagsDefinitions.AllItemTags
            .Select<string, (string, Func<ItemDefinition, bool>)>(x =>
                (Gui.Localize($"Tooltip/&Tag{x}Title"), a => a.ItemTags.Contains(x)))
            .AddItem((Gui.Localize("MainMenu/&CharacterSourceToggleAllTitle"), _ => true))
            .OrderBy(x => x.Item1)
            .ToArray();

    private static readonly string[] ItemsItemTagsFiltersLabels = ItemsItemTagsFilters.Select(x => x.Item1).ToArray();

    private static readonly (string, Func<ItemDefinition, bool>)[] ItemsWeaponTagsFilters =
        TagsDefinitions.AllWeaponTags
            .Select<string, (string, Func<ItemDefinition, bool>)>(x =>
                (Gui.Localize($"Tooltip/&Tag{x}Title"),
                    a => a.IsWeapon && a.WeaponDescription.WeaponTags.Contains(x)))
            .AddItem((Gui.Localize("MainMenu/&CharacterSourceToggleAllTitle"), _ => true))
            .AddItem((Gui.Localize("Tooltip/&TagRangeTitle"),
                a => a.IsWeapon && a.WeaponDescription.WeaponTags.Contains("Range")))
            .OrderBy(x => x.Item1)
            .ToArray();

    private static readonly string[] ItemsWeaponTagsFiltersLabels =
        ItemsWeaponTagsFilters.Select(x => x.Item1).ToArray();

    private static Vector2 ItemPosition { get; set; } = Vector2.zero;

    private static int CurrentItemsFilterIndex { get; set; }

    private static int CurrentItemsItemTagsFilterIndex { get; set; }

    private static int CurrentItemsWeaponTagsFilterIndex { get; set; }

    private static void SetFactionRelation(string name, int value)
    {
        var service = ServiceRepository.GetService<IGameFactionService>();

        service?.ExecuteFactionOperation(name, FactionDefinition.FactionOperation.Increase,
            value - service.FactionRelations[name], "",
            null /* this string and monster doesn't matter if we're using "SetValue" */);
    }

    internal static void DisplayTools()
    {
        DisplayGeneral();
        DisplayItems();
        DisplayFactionRelations();
    }

    private static void DisplayGeneral()
    {
        bool toggle;
        int intValue;

        UI.Label();
        UI.Label(Gui.Localize("ModUi/&General"));
        UI.Label();

        toggle = Main.Settings.EnableBetaContent;
        if (UI.Toggle(Gui.Localize("ModUi/&EnableBetaContent"), ref toggle, UI.AutoWidth()))
        {
            Main.Settings.EnableBetaContent = toggle;
        }

        toggle = Main.Settings.EnableSaveByLocation;
        if (UI.Toggle(Gui.Localize("ModUi/&EnableSaveByLocation"), ref toggle, UI.AutoWidth()))
        {
            Main.Settings.EnableSaveByLocation = toggle;
        }

        toggle = Main.Settings.EnableCheatMenu;
        if (UI.Toggle(Gui.Localize("ModUi/&EnableCheatMenu"), ref toggle, UI.AutoWidth()))
        {
            Main.Settings.EnableCheatMenu = toggle;
        }

        toggle = Main.Settings.EnableRespec;
        if (UI.Toggle(Gui.Localize("ModUi/&EnableRespec"), ref toggle, UI.AutoWidth()))
        {
            Main.Settings.EnableRespec = toggle;
            ToolsContext.SwitchRespec();
        }

        UI.Label();

        toggle = Main.Settings.EnableHotkeyDebugOverlay;
        if (UI.Toggle(Gui.Localize("ModUi/&EnableHotkeyDebugOverlay"), ref toggle, UI.AutoWidth()))
        {
            Main.Settings.EnableHotkeyDebugOverlay = toggle;
        }

        UI.Label();
        UI.Label(Gui.Localize("ModUi/&Adventure"));
        UI.Label();

        toggle = Main.Settings.EnableTogglesToOverwriteDefaultTestParty;
        if (UI.Toggle(Gui.Localize("ModUi/&EnableTogglesToOverwriteDefaultTestParty"), ref toggle))
        {
            Main.Settings.EnableTogglesToOverwriteDefaultTestParty = toggle;
        }

        UI.Label();

        toggle = Main.Settings.NoExperienceOnLevelUp;
        if (UI.Toggle(Gui.Localize("ModUi/&NoExperienceOnLevelUp"), ref toggle, UI.AutoWidth()))
        {
            Main.Settings.NoExperienceOnLevelUp = toggle;
        }

        toggle = Main.Settings.OverrideMinMaxLevel;
        if (UI.Toggle(Gui.Localize("ModUi/&OverrideMinMaxLevel"), ref toggle))
        {
            Main.Settings.OverrideMinMaxLevel = toggle;
        }

        UI.Label();

        intValue = Main.Settings.MultiplyTheExperienceGainedBy;
        if (UI.Slider(Gui.Localize("ModUi/&MultiplyTheExperienceGainedBy"), ref intValue, 0, 200, 100, string.Empty,
                UI.Width(100)))
        {
            Main.Settings.MultiplyTheExperienceGainedBy = intValue;
        }

        UI.Label();

        intValue = Main.Settings.OverridePartySize;
        if (UI.Slider(Gui.Localize("ModUi/&OverridePartySize"), ref intValue,
                ToolsContext.MinPartySize, ToolsContext.MaxPartySize,
                ToolsContext.GamePartySize, string.Empty, UI.AutoWidth()))
        {
            Main.Settings.OverridePartySize = intValue;

            while (Main.Settings.DefaultPartyHeroes.Count > intValue)
            {
                Main.Settings.DefaultPartyHeroes.RemoveAt(Main.Settings.DefaultPartyHeroes.Count - 1);
            }
        }

        if (Main.Settings.OverridePartySize > ToolsContext.GamePartySize)
        {
            UI.Label();

            toggle = Main.Settings.AllowAllPlayersOnNarrativeSequences;
            if (UI.Toggle(Gui.Localize("ModUi/&AllowAllPlayersOnNarrativeSequences"), ref toggle))
            {
                Main.Settings.AllowAllPlayersOnNarrativeSequences = toggle;
            }
        }

        UI.Label();

        var floatValue = Main.Settings.FasterTimeModifier;
        if (UI.Slider(Gui.Localize("ModUi/&FasterTimeModifier"), ref floatValue,
                DefaultFastTimeModifier, 10f, DefaultFastTimeModifier, 1, string.Empty, UI.AutoWidth()))
        {
            Main.Settings.FasterTimeModifier = floatValue;
        }
    }

    private static void DisplayFactionRelations()
    {
        UI.Label();
        UI.Label(Gui.Localize("ModUi/&FactionRelations"));
        UI.Label();

        var flip = true;
        var gameCampaign = Gui.GameCampaign;
        var gameFactionService = ServiceRepository.GetService<IGameFactionService>();

        // NOTE: don't use gameCampaign?. which bypasses Unity object lifetime check
        if (gameFactionService != null && gameCampaign != null &&
            gameCampaign.CampaignDefinitionName != "UserCampaign")
        {
            foreach (var faction in gameFactionService.RegisteredFactions)
            {
                if (faction.BuiltIn)
                {
                    // These are things like monster factions, generally set to a specific relation and can't be changed.
                    continue;
                }

                if (faction.GuiPresentation.Hidden)
                {
                    // These are things like Silent Whispers and Church Of Einar that are not fully implemented factions.
                    continue;
                }

                var title = faction.FormatTitle();

                title = flip ? title.Khaki() : title.White();

                var intValue = gameFactionService.FactionRelations[faction.Name];

                if (UI.Slider("                              " + title, ref intValue, faction.MinRelationCap,
                        faction.MaxRelationCap, 0, "", UI.AutoWidth()))
                {
                    SetFactionRelation(faction.Name, intValue);
                }

                flip = !flip;
            }
        }
        else
        {
            UI.Label(Gui.Localize("ModUi/&FactionHelp"));
        }

        UI.Label();
    }

    private static void DisplayItems()
    {
        UI.Label();
        UI.Label(Gui.Localize("ModUi/&Items"));
        UI.Label();

        var characterInspectionScreen = Gui.GuiService.GetScreen<CharacterInspectionScreen>();

        if (!characterInspectionScreen.Visible || characterInspectionScreen.externalContainer == null)
        {
            UI.Label(Gui.Localize("ModUi/&ItemsHelp1"));

            return;
        }

        using (UI.HorizontalScope())
        {
            UI.Space(40f);
            UI.Label("Category".Bold(), UI.Width(100));

            if (CurrentItemsFilterIndex == 11 /* Weapons */)
            {
                UI.Space(40f);
                UI.Label("Weapon Tag".Bold(), UI.Width(100));
            }

            UI.Space(40f);
            UI.Label("Item Tag".Bold(), UI.Width(100));

            UI.Space(40f);
            UI.Label(Gui.Localize("ModUi/&ItemsHelp2"));
        }

        using (UI.HorizontalScope(UI.Width(800), UI.Height(400)))
        {
            var intValue = CurrentItemsFilterIndex;
            if (UI.SelectionGrid(
                    ref intValue,
                    ItemsFiltersLabels,
                    ItemsFiltersLabels.Length,
                    1, UI.Width(140)))
            {
                CurrentItemsFilterIndex = intValue;

                if (CurrentItemsFilterIndex != 11 /* Weapons */)
                {
                    CurrentItemsWeaponTagsFilterIndex = 0;
                }
            }

            if (CurrentItemsFilterIndex == 11 /* Weapons */)
            {
                intValue = CurrentItemsWeaponTagsFilterIndex;
                if (UI.SelectionGrid(
                        ref intValue,
                        ItemsWeaponTagsFiltersLabels,
                        ItemsWeaponTagsFiltersLabels.Length,
                        1, UI.Width(140)))
                {
                    CurrentItemsWeaponTagsFilterIndex = intValue;
                }
            }

            intValue = CurrentItemsItemTagsFilterIndex;
            if (UI.SelectionGrid(
                    ref intValue,
                    ItemsItemTagsFiltersLabels,
                    ItemsItemTagsFiltersLabels.Length,
                    1, UI.Width(140)))
            {
                CurrentItemsItemTagsFilterIndex = intValue;
            }

            DisplayItemsBox();
        }
    }

    private static void DisplayItemsBox()
    {
        var characterInspectionScreen = Gui.GuiService.GetScreen<CharacterInspectionScreen>();
        var rulesetItemFactoryService = ServiceRepository.GetService<IRulesetItemFactoryService>();
        var characterName = characterInspectionScreen.InspectedCharacter.Name;

        var items = DatabaseRepository.GetDatabase<ItemDefinition>()
            .Where(x => !x.guiPresentation.Hidden)
            .Where(x => ItemsFilters[CurrentItemsFilterIndex].Item2(x))
            .Where(x => ItemsItemTagsFilters[CurrentItemsItemTagsFilterIndex].Item2(x))
            .Where(x => ItemsWeaponTagsFilters[CurrentItemsWeaponTagsFilterIndex].Item2(x))
            .OrderBy(x => x.FormatTitle());

        using var scrollView =
            new GUILayout.ScrollViewScope(ItemPosition, UI.AutoWidth(), UI.AutoHeight());

        ItemPosition = scrollView.scrollPosition;

        foreach (var item in items)
        {
            using (UI.HorizontalScope())
            {
                UI.ActionButton("+".Bold().Red(), () =>
                    {
                        var rulesetItem = rulesetItemFactoryService.CreateStandardItem(item, true, characterName);

                        characterInspectionScreen.externalContainer.AddSubItem(rulesetItem);
                    },
                    UI.Width(30));

                var label = item.GuiPresentation.Title.StartsWith("Equipment/&CraftingManual")
                    ? Gui.Format(item.GuiPresentation.Title,
                        item.DocumentDescription.RecipeDefinition.CraftedItem.FormatTitle())
                    : item.FormatTitle();

                UI.Label(label, UI.AutoWidth());
            }
        }
    }
}
