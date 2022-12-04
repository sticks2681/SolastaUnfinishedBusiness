﻿using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using SolastaUnfinishedBusiness.Api.Infrastructure;
using SolastaUnfinishedBusiness.Displays;
using SolastaUnfinishedBusiness.Models;
using UnityModManagerNet;

namespace SolastaUnfinishedBusiness;

public sealed class Core
{
}

[Serializable]
[XmlRoot(ElementName = "Settings")]
public class Settings : UnityModManager.ModSettings
{
    //
    // Welcome Message
    //

    public int EnableDiagsDump { get; set; }
    public bool HideWelcomeMessage { get; set; }

    //
    // SETTINGS UI TOGGLES
    //
    public bool DisplayRacesToggle { get; set; } = true;
    public bool DisplayClassesToggle { get; set; } = true;
    public bool DisplaySubclassesToggle { get; set; } = true;
    public bool DisplayFeatsToggle { get; set; } = true;
    public bool DisplayFeatGroupsToggle { get; set; } = true;
    public bool DisplayFightingStylesToggle { get; set; } = true;
    public bool DisplayInvocationsToggle { get; set; } = true;
    public bool DisplayCraftingToggle { get; set; }
    public bool DisplayMerchantsToggle { get; set; } = true;
    public SerializableDictionary<string, bool> DisplaySpellListsToggle { get; set; } = new();

    //
    // SETTINGS HIDDEN ON UI
    //

    public bool EnableCommandAllUndead { get; set; } = true;
    public bool EnableCtrlClickOnlySwapsMainHand { get; set; } = true;
    public bool EnableDisplaySorceryPointBoxSorcererOnly { get; set; } = true;
    public bool EnableMultiLinePowerPanel { get; set; } = true;
    public bool EnableMultiLineSpellPanel { get; set; } = true;
    public bool EnableSameWidthFeatSelection { get; set; } = true;
    public bool EnableSameWidthInvocationSelection { get; set; } = true;
    public bool EnableSortingFightingStyles { get; set; } = true;
    public bool EnableSortingSubclasses { get; set; } = true;
    public bool EnableSortingFutureFeatures { get; set; } = true;
    public bool KeepCharactersPanelOpenAndHeroSelectedAfterLevelUp { get; set; } = true;

    //
    // Character - General
    //

    // Initial Choices
    public bool AddHelpActionToAllRaces { get; set; }
    public bool EnableAlternateHuman { get; set; }
    public bool EnableFlexibleBackgrounds { get; set; }
    public bool EnableFlexibleRaces { get; set; }
    public bool EnableEpicPointsAndArray { get; set; }
    public int TotalFeatsGrantedFirstLevel { get; set; }

    // Progression
    public bool EnablesAsiAndFeat { get; set; }
    public bool EnableFeatsAtEvenLevels { get; set; }
    public bool EnableFighterArmamentAdroitness { get; set; }
    public bool EnableLevel20 { get; set; }

    public bool EnableMulticlass { get; set; }
    public int MaxAllowedClasses { get; set; }
    public bool EnableMinInOutAttributes { get; set; }
    public bool EnableRelearnSpells { get; set; }
    public bool DisplayAllKnownSpellsDuringLevelUp { get; set; }
    public bool DisplayPactSlotsOnSpellSelectionPanel { get; set; }

    // Visuals
    public bool OfferAdditionalLoreFriendlyNames { get; set; }
    public bool UnlockAllNpcFaces { get; set; }
    public bool AllowUnmarkedSorcerers { get; set; }
    public bool UnlockMarkAndTattoosForAllCharacters { get; set; }
    public bool UnlockEyeStyles { get; set; }
    public bool AddNewBrightEyeColors { get; set; }
    public bool UnlockGlowingEyeColors { get; set; }
    public bool UnlockGlowingColorsForAllMarksAndTattoos { get; set; }
    public bool UnlockSkinColors { get; set; }
    public bool AllowBeardlessDwarves { get; set; }

    //
    // Characters - Races, Classes & Subclasses
    //

    public int RaceSliderPosition { get; set; } = ModUi.DontDisplayDescription;
    public List<string> RaceEnabled { get; } = new();
    public int ClassSliderPosition { get; set; } = ModUi.DontDisplayDescription;
    public List<string> ClassEnabled { get; } = new();
    public int SubclassSliderPosition { get; set; } = ModUi.DontDisplayDescription;
    public List<string> SubclassEnabled { get; } = new();

    //
    // Characters - Feats, Groups, Fighting Styles and Invocations
    //

    public int FeatSliderPosition { get; set; } = ModUi.DontDisplayDescription;
    public List<string> FeatEnabled { get; } = new();

    public int FeatGroupSliderPosition { get; set; } = ModUi.DontDisplayDescription;
    public List<string> FeatGroupEnabled { get; } = new();

    public int FightingStyleSliderPosition { get; set; } = ModUi.DontDisplayDescription;
    public List<string> FightingStyleEnabled { get; } = new();

    public int InvocationSliderPosition { get; set; } = ModUi.DontDisplayDescription;
    public List<string> InvocationEnabled { get; } = new();

    //
    // Characters - Spells
    //

    public SerializableDictionary<string, int> SpellListSliderPosition { get; set; } = new();
    public SerializableDictionary<string, List<string>> SpellListSpellEnabled { get; set; } = new();

    //
    // Gameplay - Rules
    //

    // SRD
    public bool FixSorcererTwinnedLogic { get; set; }
    public bool ApplySrdWeightToFoodRations { get; set; }
    public bool UseOfficialAdvantageDisadvantageRules { get; set; }
    public bool IdentifyAfterRest { get; set; }

    public bool AddBleedingToLesserRestoration { get; set; }
    public bool BlindedConditionDontAllowAttackOfOpportunity { get; set; }

    public bool AllowTargetingSelectionWhenCastingChainLightningSpell { get; set; }
    public bool BestowCurseNoConcentrationRequiredForSlotLevel5OrAbove { get; set; }
    public bool EnableUpcastConjureElementalAndFey { get; set; }
    public bool OnlyShowMostPowerfulUpcastConjuredElementalOrFey { get; set; }
    public bool RemoveHumanoidFilterOnHideousLaughter { get; set; }
    public bool RemoveRecurringEffectOnEntangle { get; set; }

    public bool ChangeSleetStormToCube { get; set; }
    public bool UseHeightOneCylinderEffect { get; set; }

    // House
    public int DeadEyeAndPowerAttackBaseValue { get; set; } = 3;

    public bool AllowStackedMaterialComponent { get; set; }
    public bool AllowAnyClassToWearSylvanArmor { get; set; }
    public bool AllowDruidToWearMetalArmor { get; set; }

    public bool FullyControlConjurations { get; set; }
    public bool MakeAllMagicStaveArcaneFoci { get; set; }

    public int IncreaseSenseNormalVision { get; set; } = SrdAndHouseRulesContext.DefaultVisionRange;

    //
    // Gameplay - Items, Crafting & Merchants
    //

    // General
    public bool AddNewWeaponsAndRecipesToShops { get; set; }
    public bool AddNewWeaponsAndRecipesToEditor { get; set; }
    public bool AddPickPocketableLoot { get; set; }
    public bool AllowAnyClassToUseArcaneShieldstaff { get; set; }
    public bool RemoveAttunementRequirements { get; set; }
    public bool RemoveIdentificationRequirements { get; set; }
    public bool ShowCraftingRecipeInDetailedTooltips { get; set; }
    public int TotalCraftingTimeModifier { get; set; }
    public int RecipeCost { get; set; } = 200;
    public int SetBeltOfDwarvenKindBeardChances { get; set; } = 50;
    public int EmpressGarbAppearanceIndex { get; set; }

    // Crafting
    public List<string> CraftingInStore { get; } = new();
    public List<string> CraftingItemsInDm { get; } = new();
    public List<string> CraftingRecipesInDm { get; } = new();

    // Merchants
    public bool ScaleMerchantPricesCorrectly { get; set; }
    public bool StockGorimStoreWithAllNonMagicalClothing { get; set; }
    public bool StockHugoStoreWithAdditionalFoci { get; set; }
    public bool StockGorimStoreWithAllNonMagicalInstruments { get; set; }
    public bool EnableAdditionalFociInDungeonMaker { get; set; }
    public bool RestockAntiquarians { get; set; }
    public bool RestockArcaneum { get; set; }
    public bool RestockCircleOfDanantar { get; set; }
    public bool RestockTowerOfKnowledge { get; set; }

    //
    // Gameplay - Tools
    //

    // General
    public bool EnableBetaContent { get; set; }
    public bool EnableSaveByLocation { get; set; }
    public bool EnableRespec { get; set; }
    public bool EnableCheatMenu { get; set; }
    public bool OverrideMinMaxLevel { get; set; }
    public bool NoExperienceOnLevelUp { get; set; }
    public int MultiplyTheExperienceGainedBy { get; set; } = 100;
    public int OverridePartySize { get; set; } = ToolsContext.GamePartySize;
    public bool AllowAllPlayersOnNarrativeSequences { get; set; }
    public float FasterTimeModifier { get; set; } = ToolsDisplay.DefaultFastTimeModifier;
    public bool EnableTogglesToOverwriteDefaultTestParty { get; set; }
    public List<string> DefaultPartyHeroes { get; } = new();

    //
    // Interface - Game UI
    //

    // Campaigns and Locations
    public bool DontFollowCharacterInBattle { get; set; }
    public int DontFollowMargin { get; set; } = 5;
    public bool EnableStatsOnHeroTooltip { get; set; }
    public bool EnableAdditionalBackstoryDisplay { get; set; }
    public bool EnableLogDialoguesToConsole { get; set; }
    public bool EnableAdditionalIconsOnLevelMap { get; set; }
    public bool MarkInvisibleTeleportersOnLevelMap { get; set; }
    public bool HideExitsAndTeleportersGizmosIfNotDiscovered { get; set; }

    // Input
    public bool AltOnlyHighlightItemsInPartyFieldOfView { get; set; }
    public bool InvertAltBehaviorOnTooltips { get; set; }
    public bool EnableHotkeyToggleHud { get; set; }
    public bool EnableCharacterExport { get; set; }
    public bool EnableTeleportParty { get; set; }
    public bool EnableCancelEditOnRightMouseClick { get; set; }

    // Inventory and Items
    public bool DisableAutoEquip { get; set; }
    public bool EnableInventoryFilteringAndSorting { get; set; }
    public bool EnableInventoryTaintNonProficientItemsRed { get; set; }
    public bool EnableInvisibleCrownOfTheMagister { get; set; }

    // Monsters
    public bool HideMonsterHitPoints { get; set; }
    public bool RemoveBugVisualModels { get; set; }

    //
    // Interface - Dungeon Maker
    //

    public bool EnableSortingDungeonMakerAssets { get; set; }
    public bool EnableHotkeyDebugOverlay { get; set; }
    public bool AllowGadgetsAndPropsToBePlacedAnywhere { get; set; }
    public bool UnleashNpcAsEnemy { get; set; }
    public bool UnleashEnemyAsNpc { get; set; }
    public bool EnableDungeonMakerModdedContent { get; set; }

    //
    // Interface - Translations
    //

    public string SelectedLanguageCode { get; set; } = TranslatorContext.English;

    //
    // Encounters - General
    //

    public bool EnableEnemiesControlledByPlayer { get; set; }
    public bool EnableHeroesControlledByComputer { get; set; }

    // Debug
    public bool DebugDisableVerifyDefinitionNameIsNotInUse { get; set; }
#if DEBUG
    public bool DebugLogDefinitionCreation { get; set; }
    public bool DebugLogFieldInitialization { get; set; }
    public bool DebugLogVariantMisuse { get; set; }
#endif
}
