﻿using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using JetBrains.Annotations;
using SolastaUnfinishedBusiness.Api.Infrastructure;
using SolastaUnfinishedBusiness.Builders;
using SolastaUnfinishedBusiness.CustomUI;
using SolastaUnfinishedBusiness.Properties;
using UnityEngine.AddressableAssets;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper.CharacterClassDefinitions;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper.FactionStatusDefinitions;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper.FeatureDefinitionCharacterPresentations;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper.ItemDefinitions;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper.MerchantDefinitions;

namespace SolastaUnfinishedBusiness.Models;

internal static class ItemCraftingMerchantContext
{
    internal static string[] EmpressGarbAppearances { get; } =
    {
        Gui.Localize("Modal/&TravelPaceNormalTitle"),
        Gui.Localize("Equipment/&Barbarian_Clothes_Title"),
        Gui.Localize("Equipment/&Druid_Leather_Title"),
        Gui.Localize("Equipment/&ElvenChain_Unidentified_Title"),
        CultureInfo.CurrentCulture.TextInfo.ToTitleCase(Gui.Localize("Equipment/&Armor_Sorcerer_Outfit_Title")),
        Gui.Localize("Equipment/&Armor_StuddedLeatherTitle"), Gui.Localize("Equipment/&GreenmageArmor_Title"),
        Gui.Localize("Equipment/&Armor_Adventuring_Wizard_OutfitTitle"),
        Gui.Localize("Equipment/&Armor_Scavenger_Outfit_01_Title"),
        Gui.Localize("Equipment/&Armor_Scavenger_Outfit_02_Title"),
        Gui.Localize("Equipment/&Armor_Bard_Title"),
        Gui.Localize("Equipment/&Armor_Warlock_Title")
    };

    private static void LoadClothingGorimStock()
    {
        if (!Main.Settings.StockGorimStoreWithAllNonMagicalClothing)
        {
            return;
        }

        foreach (var item in DatabaseRepository.GetDatabase<ItemDefinition>().Where(
                     x => x.ArmorDescription?.ArmorType == "ClothesType" && !x.Magical &&
                          !x.SlotsWhereActive.Contains("TabardSlot") && x != ClothesCommon_Tattoo &&
                          x != ClothesWizard_B))
        {
            var stockClothing = new StockUnitDescription
            {
                itemDefinition = item,
                initialAmount = 2,
                initialized = true,
                factionStatus = Indifference.Name,
                maxAmount = 4,
                minAmount = 2,
                stackCount = 1,
                reassortAmount = 1,
                reassortRateValue = 1,
                reassortRateType = RuleDefinitions.DurationType.Day
            };

            Store_Merchant_Gorim_Ironsoot_Cyflen_GeneralStore.StockUnitDescriptions.Add(stockClothing);
        }

        //rename valley noble's clothes by color to avoid confusion
        var silverNoble = ClothesNoble_Valley_Silver;
        silverNoble.GuiPresentation.title = "Equipment/&Armor_Noble_ClothesTitle_Silver";

        var redNoble = ClothesNoble_Valley_Red;
        redNoble.GuiPresentation.title = "Equipment/&Armor_Noble_ClothesTitle_Red";

        var purpleNoble = ClothesNoble_Valley_Purple;
        purpleNoble.GuiPresentation.title = "Equipment/&Armor_Noble_ClothesTitle_Purple";

        var pinkNoble = ClothesNoble_Valley_Pink;
        pinkNoble.GuiPresentation.title = "Equipment/&Armor_Noble_ClothesTitle_Pink";

        var orangeNoble = ClothesNoble_Valley_Orange;
        orangeNoble.GuiPresentation.title = "Equipment/&Armor_Noble_ClothesTitle_Orange";

        var greenNoble = ClothesNoble_Valley_Green;
        greenNoble.GuiPresentation.title = "Equipment/&Armor_Noble_ClothesTitle_Green";

        var cherryNoble = ClothesNoble_Valley_Cherry;
        cherryNoble.GuiPresentation.title = "Equipment/&Armor_Noble_ClothesTitle_Cherry";

        var valleyNoble = ClothesNoble_Valley;
        valleyNoble.GuiPresentation.title = "Equipment/&Armor_Noble_ClothesTitle_Valley";
    }

    private static void LoadInstrumentsGorimStock()
    {
        if (!Main.Settings.StockGorimStoreWithAllNonMagicalInstruments)
        {
            return;
        }

        foreach (var item in DatabaseRepository.GetDatabase<ItemDefinition>().Where(
                     x => x.IsMusicalInstrument && !x.Magical))
        {
            var stockInstruments = new StockUnitDescription
            {
                itemDefinition = item,
                initialAmount = 2,
                initialized = true,
                factionStatus = Indifference.Name,
                maxAmount = 4,
                minAmount = 2,
                stackCount = 1,
                reassortAmount = 1,
                reassortRateValue = 1,
                reassortRateType = RuleDefinitions.DurationType.Day
            };

            Store_Merchant_Gorim_Ironsoot_Cyflen_GeneralStore.StockUnitDescriptions.Add(stockInstruments);
        }
    }

    internal static void SwitchSetBeltOfDwarvenKindBeardChances()
    {
        CharacterPresentationBeltOfDwarvenKind.occurencePercentage =
            Main.Settings.SetBeltOfDwarvenKindBeardChances;
        CharacterPresentationBeltOfDwarvenKind.GuiPresentation.description = Gui.Format(
            "Feature/&AlwaysBeardDescription",
            Main.Settings.SetBeltOfDwarvenKindBeardChances.ToString());
    }

    internal static void SwitchFociItems()
    {
        if (Main.Settings.StockHugoStoreWithAdditionalFoci)
        {
            Store_Merchant_Hugo_Requer_Cyflen_Potions.StockUnitDescriptions.AddRange(FocusDefinitionBuilder
                .StockFocus);
        }
        else
        {
            foreach (var stockFocus in FocusDefinitionBuilder.StockFocus)
            {
                Store_Merchant_Hugo_Requer_Cyflen_Potions.StockUnitDescriptions.Remove(stockFocus);
            }
        }

        FocusDefinitionBuilder.ArcaneStaff.GuiPresentation.hidden = !Main.Settings.StockHugoStoreWithAdditionalFoci;
        FocusDefinitionBuilder.DruidicAmulet.GuiPresentation.hidden = !Main.Settings
            .StockHugoStoreWithAdditionalFoci;
        FocusDefinitionBuilder.LivewoodClub.GuiPresentation.hidden = !Main.Settings
            .StockHugoStoreWithAdditionalFoci;
        FocusDefinitionBuilder.LivewoodStaff.GuiPresentation.hidden = !Main.Settings
            .StockHugoStoreWithAdditionalFoci;
    }

    internal static void SwitchFociItemsDungeonMaker()
    {
        FocusDefinitionBuilder.ArcaneStaff.inDungeonEditor = Main.Settings.EnableAdditionalFociInDungeonMaker;
        FocusDefinitionBuilder.DruidicAmulet.inDungeonEditor = Main.Settings.EnableAdditionalFociInDungeonMaker;
        FocusDefinitionBuilder.LivewoodClub.inDungeonEditor = Main.Settings.EnableAdditionalFociInDungeonMaker;
        FocusDefinitionBuilder.LivewoodStaff.inDungeonEditor = Main.Settings.EnableAdditionalFociInDungeonMaker;
    }

    internal static void SwitchAttuneArcaneShieldstaff()
    {
        if (Main.Settings.AllowAnyClassToUseArcaneShieldstaff)
        {
            ArcaneShieldstaff.RequiredAttunementClasses.Clear();
        }
        else
        {
            ArcaneShieldstaff.RequiredAttunementClasses.SetRange(Wizard, Cleric, Paladin, Ranger, Sorcerer, Warlock);
        }
    }

    internal static void SwitchRestockAntiquarian()
    {
        if (!Main.Settings.RestockAntiquarians)
        {
            return;
        }

        foreach (var stock in Store_Merchant_Antiquarians_Halman_Summer.StockUnitDescriptions.Where(
                     x => !x.ItemDefinition.Name.Contains("Manual") && !x.ItemDefinition.Name.Contains("Tome")))
        {
            stock.reassortAmount = 1;
            stock.reassortRateValue = 7;
        }
    }

    internal static void SwitchRestockArcaneum()
    {
        if (!Main.Settings.RestockArcaneum)
        {
            return;
        }

        foreach (var stock in Store_Merchant_Arcaneum_Heddlon_Surespell.StockUnitDescriptions)
        {
            stock.reassortAmount = 1;
        }
    }

    internal static void SwitchRestockCircleOfDanantar()
    {
        if (!Main.Settings.RestockCircleOfDanantar)
        {
            return;
        }

        foreach (var stock in Store_Merchant_CircleOfDanantar_Joriel_Foxeye.StockUnitDescriptions)
        {
            stock.reassortAmount = 1;
        }
    }

    internal static void SwitchRestockTowerOfKnowledge()
    {
        if (!Main.Settings.RestockTowerOfKnowledge)
        {
            return;
        }

        foreach (var stock in Store_Merchant_TowerOfKnowledge_Maddy_Greenisle.StockUnitDescriptions)
        {
            stock.reassortAmount = 1;
        }
    }

    private static void LoadRemoveIdentification()
    {
        if (Main.Settings.RemoveIdentificationRequirements)
        {
            foreach (var item in DatabaseRepository.GetDatabase<ItemDefinition>())
            {
                item.requiresIdentification = false;
            }
        }

        if (!Main.Settings.RemoveAttunementRequirements)
        {
            return;
        }

        {
            foreach (var item in DatabaseRepository.GetDatabase<ItemDefinition>())
            {
                item.requiresAttunement = false;
            }
        }
    }

    internal static void Load()
    {
        // sort of same sequence as Mod UI
        CraftingContext.Load();
        PickPocketContext.Load();
        LoadRemoveIdentification();
        SwitchAttuneArcaneShieldstaff();
        SwitchSetBeltOfDwarvenKindBeardChances();
        LoadClothingGorimStock();
        LoadInstrumentsGorimStock();
        SwitchFociItems();
        SwitchFociItemsDungeonMaker();
        SwitchRestockAntiquarian();
        SwitchRestockArcaneum();
        SwitchRestockCircleOfDanantar();
        SwitchRestockTowerOfKnowledge();
    }

    private sealed class FocusDefinitionBuilder : ItemDefinitionBuilder
    {
        internal static readonly HashSet<StockUnitDescription> StockFocus = new();

        internal static readonly ItemDefinition ArcaneStaff = CreateAndAddToDB(
            "CEArcaneStaff",
            Quarterstaff,
            EquipmentDefinitions.FocusType.Arcane,
            QuarterstaffPlus1.GuiPresentation.SpriteReference);

        internal static readonly ItemDefinition DruidicAmulet = CreateAndAddToDB(
            "CEDruidicAmulet",
            ComponentPouch_ArcaneAmulet,
            EquipmentDefinitions.FocusType.Druidic,
            BeltOfGiantHillStrength.GuiPresentation.SpriteReference);

        internal static readonly ItemDefinition LivewoodClub = CreateAndAddToDB(
            "CELivewoodClub",
            Club,
            EquipmentDefinitions.FocusType.Druidic,
            Sprites.GetSprite("LivewoodClub", Resources.LivewoodClub, 128, 128));

        internal static readonly ItemDefinition LivewoodStaff = CreateAndAddToDB(
            "CELivewoodStaff",
            Quarterstaff,
            EquipmentDefinitions.FocusType.Druidic,
            StaffOfHealing.GuiPresentation.SpriteReference);

        private FocusDefinitionBuilder(
            string name,
            ItemDefinition original,
            EquipmentDefinitions.FocusType type,
            [CanBeNull] AssetReferenceSprite assetReferenceSprite,
            [NotNull] params string[] slotTypes) : base(original, name, CeNamespaceGuid)
        {
            // Use IsXXXItem = true/SetIsXXXItem(true) before using the XXXItemDescription
            Definition.IsFocusItem = true;
            Definition.FocusItemDescription.focusType = type;
            Definition.GuiPresentation = GuiPresentationBuilder.Build(name, Category.Item);

            if (assetReferenceSprite != null)
            {
                Definition.GuiPresentation.spriteReference = assetReferenceSprite;
            }

            Definition.costs = ComponentPouch.Costs;

            if (slotTypes.Length > 0)
            {
                Definition.SlotTypes.SetRange(slotTypes);
                Definition.SlotTypes.Add(EquipmentDefinitions.SlotTypeContainer);
                Definition.SlotsWhereActive.SetRange(slotTypes);
            }

            var stockFocus = new StockUnitDescription
            {
                itemDefinition = Definition,
                initialAmount = 1,
                initialized = true,
                factionStatus = Indifference.Name,
                maxAmount = 2,
                minAmount = 1,
                stackCount = 1,
                reassortAmount = 1,
                reassortRateValue = 1,
                reassortRateType = RuleDefinitions.DurationType.Day
            };

            StockFocus.Add(stockFocus);
        }

        private static ItemDefinition CreateAndAddToDB(
            string name,
            ItemDefinition original,
            EquipmentDefinitions.FocusType type,
            AssetReferenceSprite assetReferenceSprite,
            [NotNull] params string[] slotTypes)
        {
            return new FocusDefinitionBuilder(name, original, type, assetReferenceSprite, slotTypes).AddToDB();
        }
    }
}
