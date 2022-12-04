﻿using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using SolastaUnfinishedBusiness.Api.Extensions;
using SolastaUnfinishedBusiness.Api.Helpers;
using SolastaUnfinishedBusiness.Builders;
using SolastaUnfinishedBusiness.Builders.Features;
using SolastaUnfinishedBusiness.CustomBehaviors;
using SolastaUnfinishedBusiness.CustomUI;
using SolastaUnfinishedBusiness.Properties;
using UnityEngine.AddressableAssets;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper;
using static SolastaUnfinishedBusiness.Models.ItemPropertyDescriptionsContext;
using static RuleDefinitions;
using static RuleDefinitions.ItemRarity;

namespace SolastaUnfinishedBusiness.Models;

internal static class CustomWeaponsContext
{
    internal const string CeHandXbowType = "CEHandXbowType";
    private const string PolearmWeaponTag = "PolearmWeapon";

    internal const string AttackedWithLauncherConditionName = "ConditionLauncherAttackMarker";
    internal static WeaponTypeDefinition HalberdWeaponType, PikeWeaponType, LongMaceWeaponType, HandXbowWeaponType;
    internal static ItemDefinition HandwrapsOfForce, HandwrapsOfPulling;
    internal static ItemDefinition Halberd, HalberdPrimed, HalberdPlus1, HalberdPlus2, HalberdLightning;
    internal static ItemDefinition Pike, PikePrimed, PikePlus1, PikePlus2, PikePsychic;
    internal static ItemDefinition LongMace, LongMacePrimed, LongMacePlus1, LongMacePlus2, LongMaceThunder;
    internal static ItemDefinition HandXbow, HandXbowPrimed, HandXbowPlus1, HandXbowPlus2, HandXbowAcid;
    internal static ItemDefinition ProducedFlameDart;
    internal static WeaponTypeDefinition ThunderGauntletType, LightningLauncherType;
    internal static ItemDefinition ThunderGauntlet, LightningLauncher;

    internal static readonly List<string> PolearmWeaponTypes = new()
    {
        WeaponTypeDefinitions.QuarterstaffType.Name, WeaponTypeDefinitions.SpearType.Name
    };

    internal static void Load()
    {
        BuildHandwraps();
        BuildHalberds();
        BuildPikes();
        BuildLongMaces();
        BuildHandXbow();
        WeaponizeProducedFlame();
        BuildThunderGauntlet();
        BuildLightningLauncher();

        PolearmWeaponTypes.AddRange(new[] { HalberdWeaponType.Name, PikeWeaponType.Name, LongMaceWeaponType.Name });
    }

    [NotNull]
    internal static ItemPresentation BuildPresentation(
        string unIdentifiedName,
        [NotNull] ItemPresentation basePresentation,
        float scale = 1.0f, bool hasUnidentifiedDescription = false)
    {
        //TODO: either create a builder for ItemPresentation, or add setter with custom values to ItemDefinitionBuilder
        var presentation = new ItemPresentation(basePresentation);

        presentation.ItemFlags.Clear();
        presentation.assetReference = basePresentation.AssetReference;
        presentation.unidentifiedTitle = GuiPresentationBuilder.CreateTitleKey(unIdentifiedName, Category.Item);
        presentation.unidentifiedDescription = hasUnidentifiedDescription
            ? GuiPresentationBuilder.CreateDescriptionKey(unIdentifiedName, Category.Item)
            : Gui.NoLocalization;

        presentation.scaleFactorWhileWielded = scale;

        return presentation;
    }

    [NotNull]
    private static ItemDefinition BuildWeapon(string name, ItemDefinition baseItem, int goldCost, bool noDescription,
        ItemRarity rarity,
        ItemPresentation basePresentation = null,
        WeaponDescription baseDescription = null,
        AssetReferenceSprite icon = null,
        bool needId = true,
        float scale = 1.0f,
        bool twoHanded = true,
        params ItemPropertyDescription[] properties)
    {
        basePresentation ??= baseItem.ItemPresentation;
        baseDescription ??= new WeaponDescription(baseItem.WeaponDescription);
        icon ??= baseItem.GuiPresentation.SpriteReference;

        var builder = ItemDefinitionBuilder
            .Create(baseItem, name)
            .SetGold(goldCost)
            .SetMerchantCategory(MerchantCategoryDefinitions.Weapon)
            .SetStaticProperties(properties)
            .SetWeaponDescription(baseDescription)
            .SetItemPresentation(BuildPresentation($"{name}Unidentified", basePresentation, scale))
            .SetItemRarity(rarity);

        if (twoHanded)
        {
            _ = builder
                .SetSlotTypes(SlotTypeDefinitions.MainHandSlot, SlotTypeDefinitions.ContainerSlot)
                .SetSlotsWhereActive(SlotTypeDefinitions.MainHandSlot);
        }
        else
        {
            _ = builder
                .SetSlotTypes(SlotTypeDefinitions.MainHandSlot, SlotTypeDefinitions.OffHandSlot,
                    SlotTypeDefinitions.ContainerSlot)
                .SetSlotsWhereActive(SlotTypeDefinitions.MainHandSlot, SlotTypeDefinitions.OffHandSlot);
        }

        if (!properties.Empty())
        {
            _ = builder.MakeMagical();

            if (needId)
            {
                _ = builder.SetRequiresIdentification(true);
            }
        }

        if (noDescription)
        {
            _ = builder.SetGuiPresentation(Category.Item, Gui.NoLocalization, icon);
        }
        else
        {
            _ = builder.SetGuiPresentation(Category.Item, icon);
        }

        var weapon = builder.AddToDB();

        weapon.inDungeonEditor = Main.Settings.AddNewWeaponsAndRecipesToEditor;

        return weapon;
    }

    private static void BuildHandwraps()
    {
        HandwrapsOfForce = BuildHandwrapsCommon("HandwrapsOfForce", 2000, true, false, Rare, ForceImpactVFX,
            WeaponPlus1AttackOnly);
        HandwrapsOfForce.WeaponDescription.EffectDescription.effectForms.Add(EffectFormBuilder
            .Create()
            .SetDamageForm(DamageTypeForce, 1, DieType.D4)
            .Build());

        HandwrapsOfPulling = BuildHandwrapsCommon("HandwrapsOfPulling", 2000, true, false, Rare, WeaponPlus1AttackOnly);
        HandwrapsOfPulling.IsUsableDevice = true;
        HandwrapsOfPulling.usableDeviceDescription = new UsableDeviceDescriptionBuilder()
            .SetRecharge(RechargeRate.AtWill)
            .SetSaveDc(EffectHelpers.BasedOnUser)
            .AddFunctions(new DeviceFunctionDescriptionBuilder()
                .SetPower(FeatureDefinitionPowerBuilder
                    .Create("PowerHandwrapsOfPulling")
                    .SetGuiPresentation(Category.Feature)
                    .SetUsesFixed(ActivationTime.BonusAction)
                    .SetEffectDescription(
                        EffectDescriptionBuilder
                            .Create()
                            .SetTargetingData(Side.All, RangeType.Distance, 3, TargetType.Individuals)
                            .ExcludeCaster()
                            .SetSavingThrowData(
                                true,
                                AttributeDefinitions.Strength,
                                false,
                                EffectDifficultyClassComputation.AbilityScoreAndProficiency)
                            .SetParticleEffectParameters(FeatureDefinitionPowers.PowerShadowTamerRopeGrapple)
                            .SetDurationData(DurationType.Instantaneous)
                            .SetEffectForms(EffectFormBuilder
                                .Create()
                                .SetMotionForm(MotionForm.MotionType.DragToOrigin, 2)
                                .Build())
                            .Build())
                    .AddToDB())
                .Build())
            .Build();

        MerchantContext.AddItem(RecipeHelper.BuildRecipeManual(HandwrapsOfForce, 48, 16,
            ItemDefinitions.Ingredient_Enchant_Soul_Gem, ItemDefinitions.Primed_Gauntlet), ShopItemType.ShopCrafting);

        MerchantContext.AddItem(RecipeHelper.BuildRecipeManual(HandwrapsOfPulling, 48, 16,
            ItemDefinitions.Ingredient_Enchant_Stardust, ItemDefinitions.Primed_Gauntlet), ShopItemType.ShopCrafting);
    }

    [NotNull]
    private static ItemDefinition BuildHandwrapsCommon(string name, int goldCost, bool noDescription, bool needId,
        ItemRarity rarity,
        params ItemPropertyDescription[] properties)
    {
        return BuildWeapon(
            name,
            ItemDefinitions.Primed_Gauntlet,
            goldCost,
            noDescription, rarity, needId: needId,
            properties: properties
        );
    }

    private static void BuildHalberds()
    {
        var scale = new CustomScale(z: 3.5f);

        HalberdWeaponType = WeaponTypeDefinitionBuilder
            .Create(WeaponTypeDefinitions.GreataxeType, "CEHalberdType")
            .SetGuiPresentation(Category.Item, Gui.NoLocalization)
            .SetWeaponCategory(WeaponCategoryDefinitions.MartialWeaponCategory)
            .AddToDB();

        var baseItem = ItemDefinitions.Greataxe;
        var basePresentation = ItemDefinitions.Battleaxe.ItemPresentation;
        var baseDescription = new WeaponDescription(baseItem.WeaponDescription)
        {
            reachRange = 2,
            weaponType = HalberdWeaponType.Name,
            weaponTags = new List<string>
            {
                TagsDefinitions.WeaponTagHeavy,
                TagsDefinitions.WeaponTagReach,
                TagsDefinitions.WeaponTagTwoHanded
            }
        };
        var damageForm = baseDescription.EffectDescription
            .GetFirstFormOfType(EffectForm.EffectFormType.Damage).DamageForm;

        damageForm.dieType = DieType.D10;
        damageForm.diceNumber = 1;

        Halberd = BuildWeapon("CEHalberd", baseItem,
            20, true, Common, basePresentation, baseDescription, HalberdIcon);
        Halberd.SetCustomSubFeatures(scale);
        MerchantContext.AddItem(Halberd, ShopItemType.ShopGenericMelee);

        HalberdPrimed = BuildWeapon("CEHalberdPrimed", baseItem,
            40, true, Uncommon, basePresentation, baseDescription, HalberdPrimedIcon);
        HalberdPrimed.ItemTags.Add(TagsDefinitions.ItemTagIngredient);
        HalberdPrimed.ItemTags.Remove(TagsDefinitions.ItemTagStandard);
        HalberdPrimed.SetCustomSubFeatures(scale);
        MerchantContext.AddItem(HalberdPrimed, ShopItemType.ShopPrimedMelee);
        MerchantContext.AddItem(RecipeHelper.BuildPrimeManual(Halberd, HalberdPrimed), ShopItemType.ShopCrafting);

        HalberdPlus1 = BuildWeapon("CEHalberd+1", Halberd,
            950, true, Rare, icon: HalberdP1Icon, properties: new[] { WeaponPlus1 });
        HalberdPlus1.SetCustomSubFeatures(scale);
        MerchantContext.AddItem(HalberdPlus1, ShopItemType.ShopMeleePlus1);
        MerchantContext.AddItem(RecipeHelper.BuildRecipeManual(HalberdPlus1, 24, 10,
            HalberdPrimed,
            ItemDefinitions.Ingredient_Enchant_Oil_Of_Acuteness), ShopItemType.ShopCrafting);

        var itemDefinition = ItemDefinitions.BattleaxePlus1;

        HalberdPlus2 = BuildWeapon("CEHalberd+2", Halberd,
            2500, true, VeryRare,
            itemDefinition.ItemPresentation, icon: HalberdP2Icon,
            properties: new[] { WeaponPlus2 });
        HalberdPlus2.SetCustomSubFeatures(scale);
        MerchantContext.AddItem(HalberdPlus2, ShopItemType.ShopMeleePlus2);
        MerchantContext.AddItem(RecipeHelper.BuildRecipeManual(HalberdPlus2, 48, 16,
            HalberdPrimed,
            ItemDefinitions.Ingredient_Enchant_Blood_Gem), ShopItemType.ShopCrafting);

        HalberdLightning = BuildWeapon("CEHalberdLightning", Halberd,
            2500, true, VeryRare,
            itemDefinition.ItemPresentation, icon: HalberdLightningIcon, needId: false,
            properties: new[] { LightningImpactVFX, WeaponPlus1AttackOnly });
        HalberdLightning.SetCustomSubFeatures(scale);
        HalberdLightning.WeaponDescription.EffectDescription.effectForms.Add(EffectFormBuilder
            .Create()
            .SetDamageForm(DamageTypeLightning, 1, DieType.D8)
            .Build());
        MerchantContext.AddItem(RecipeHelper.BuildRecipeManual(HalberdLightning, 48, 16,
            HalberdPrimed,
            ItemDefinitions.Ingredient_Enchant_Stardust), ShopItemType.ShopCrafting);
    }

    private static void BuildPikes()
    {
        var scale = new CustomScale(z: 3.5f);

        PikeWeaponType = WeaponTypeDefinitionBuilder
            .Create(WeaponTypeDefinitions.SpearType, "CEPikeType")
            .SetGuiPresentation(Category.Item, Gui.NoLocalization)
            .SetWeaponCategory(WeaponCategoryDefinitions.MartialWeaponCategory)
            .AddToDB();

        var baseItem = ItemDefinitions.Spear;
        var basePresentation = ItemDefinitions.Morningstar.ItemPresentation;
        var baseDescription = new WeaponDescription(baseItem.WeaponDescription)
        {
            reachRange = 2,
            weaponType = PikeWeaponType.Name,
            weaponTags = new List<string>
            {
                TagsDefinitions.WeaponTagHeavy,
                TagsDefinitions.WeaponTagReach,
                TagsDefinitions.WeaponTagTwoHanded
            }
        };
        var damageForm = baseDescription.EffectDescription
            .GetFirstFormOfType(EffectForm.EffectFormType.Damage).DamageForm;

        damageForm.dieType = DieType.D10;
        damageForm.diceNumber = 1;

        Pike = BuildWeapon("CEPike", baseItem,
            20, true, Common, basePresentation, baseDescription, PikeIcon);
        Pike.SetCustomSubFeatures(scale);
        Pike.ItemTags.Remove(TagsDefinitions.ItemTagMonk);
        MerchantContext.AddItem(Pike, ShopItemType.ShopGenericMelee);

        PikePrimed = BuildWeapon("CEPikePrimed", baseItem,
            40, true, Uncommon, basePresentation, baseDescription, PikePrimedIcon);
        PikePrimed.ItemTags.Add(TagsDefinitions.ItemTagIngredient);
        PikePrimed.ItemTags.Remove(TagsDefinitions.ItemTagStandard);
        PikePrimed.ItemTags.Remove(TagsDefinitions.ItemTagMonk);
        PikePrimed.SetCustomSubFeatures(scale);
        MerchantContext.AddItem(PikePrimed, ShopItemType.ShopPrimedMelee);
        MerchantContext.AddItem(RecipeHelper.BuildPrimeManual(Pike, PikePrimed), ShopItemType.ShopCrafting);

        PikePlus1 = BuildWeapon("CEPike+1", Pike,
            950, true, Rare, icon: PikeP1Icon, properties: new[] { WeaponPlus1 });
        PikePlus1.SetCustomSubFeatures(scale);
        PikePlus1.ItemTags.Remove(TagsDefinitions.ItemTagMonk);
        MerchantContext.AddItem(PikePlus1, ShopItemType.ShopMeleePlus1);
        MerchantContext.AddItem(RecipeHelper.BuildRecipeManual(PikePlus1, 24, 10,
            PikePrimed,
            ItemDefinitions.Ingredient_Enchant_Oil_Of_Acuteness), ShopItemType.ShopCrafting);

        var itemDefinition = ItemDefinitions.MorningstarPlus2;

        PikePlus2 = BuildWeapon("CEPike+2", Pike,
            2500, true, VeryRare,
            itemDefinition.ItemPresentation,
            icon: PikeP2Icon,
            properties: new[] { WeaponPlus2 });
        PikePlus2.SetCustomSubFeatures(scale);
        PikePlus2.ItemTags.Remove(TagsDefinitions.ItemTagMonk);
        MerchantContext.AddItem(PikePlus2, ShopItemType.ShopMeleePlus2);
        MerchantContext.AddItem(RecipeHelper.BuildRecipeManual(PikePlus2, 48, 16,
            PikePrimed,
            ItemDefinitions.Ingredient_Enchant_Blood_Gem), ShopItemType.ShopCrafting);

        PikePsychic = BuildWeapon("CEPikePsychic", Pike,
            2500, true, VeryRare,
            itemDefinition.ItemPresentation,
            icon: PikePsychicIcon, needId: false,
            properties: new[] { PsychicImpactVFX, WeaponPlus1AttackOnly });
        PikePsychic.SetCustomSubFeatures(scale);
        PikePsychic.ItemTags.Remove(TagsDefinitions.ItemTagMonk);
        PikePsychic.WeaponDescription.EffectDescription.effectForms.Add(EffectFormBuilder
            .Create()
            .SetDamageForm(DamageTypePsychic, 1, DieType.D8)
            .Build());
        MerchantContext.AddItem(RecipeHelper.BuildRecipeManual(PikePsychic, 48, 16,
            PikePrimed,
            ItemDefinitions.Ingredient_Enchant_Stardust), ShopItemType.ShopCrafting);
    }

    private static void BuildLongMaces()
    {
        var scale = new CustomScale(z: 3.5f);

        LongMaceWeaponType = WeaponTypeDefinitionBuilder
            .Create(WeaponTypeDefinitions.MaulType, "CELongMaceType")
            .SetGuiPresentation(Category.Item, Gui.NoLocalization)
            .SetWeaponCategory(WeaponCategoryDefinitions.MartialWeaponCategory)
            .AddToDB();

        var baseItem = ItemDefinitions.Warhammer;
        var basePresentation = ItemDefinitions.Mace.ItemPresentation;
        var baseDescription = new WeaponDescription(baseItem.WeaponDescription)
        {
            reachRange = 2,
            weaponType = LongMaceWeaponType.Name,
            weaponTags = new List<string>
            {
                TagsDefinitions.WeaponTagHeavy,
                TagsDefinitions.WeaponTagReach,
                TagsDefinitions.WeaponTagTwoHanded
            }
        };
        var damageForm = baseDescription.EffectDescription
            .GetFirstFormOfType(EffectForm.EffectFormType.Damage).DamageForm;

        damageForm.dieType = DieType.D10;
        damageForm.diceNumber = 1;

        LongMace = BuildWeapon("CELongMace", baseItem,
            20, true, Common, basePresentation, baseDescription, LongMaceIcon);
        LongMace.SetCustomSubFeatures(scale);
        MerchantContext.AddItem(LongMace, ShopItemType.ShopGenericMelee);

        LongMacePrimed = BuildWeapon("CELongMacePrimed", baseItem,
            40, true, Uncommon, basePresentation, baseDescription, LongMacePrimedIcon);
        LongMacePrimed.ItemTags.Add(TagsDefinitions.ItemTagIngredient);
        LongMacePrimed.ItemTags.Remove(TagsDefinitions.ItemTagStandard);
        LongMacePrimed.SetCustomSubFeatures(scale);
        MerchantContext.AddItem(LongMacePrimed, ShopItemType.ShopPrimedMelee);
        MerchantContext.AddItem(RecipeHelper.BuildPrimeManual(LongMace, LongMacePrimed), ShopItemType.ShopCrafting);

        LongMacePlus1 = BuildWeapon("CELongMace+1", LongMace,
            950, true, Rare, icon: LongMaceP1Icon, properties: new[] { WeaponPlus1 });
        LongMacePlus1.SetCustomSubFeatures(scale);
        MerchantContext.AddItem(LongMacePlus1, ShopItemType.ShopMeleePlus1);
        MerchantContext.AddItem(RecipeHelper.BuildRecipeManual(LongMacePlus1, 24, 10,
            LongMacePrimed,
            ItemDefinitions.Ingredient_Enchant_Oil_Of_Acuteness), ShopItemType.ShopCrafting);

        var itemDefinition = ItemDefinitions.MacePlus2;

        LongMacePlus2 = BuildWeapon("CELongMace+2", LongMace,
            2500, true, VeryRare,
            itemDefinition.ItemPresentation, icon: LongMaceP2Icon,
            properties: new[] { WeaponPlus2 });
        LongMacePlus2.SetCustomSubFeatures(scale);
        MerchantContext.AddItem(LongMacePlus2, ShopItemType.ShopMeleePlus2);
        MerchantContext.AddItem(RecipeHelper.BuildRecipeManual(LongMacePlus2, 48, 16,
            LongMacePrimed,
            ItemDefinitions.Ingredient_Enchant_Blood_Gem), ShopItemType.ShopCrafting);

        LongMaceThunder = BuildWeapon("CELongMaceThunder", LongMace,
            2500, true, VeryRare,
            itemDefinition.ItemPresentation, icon: LongMaceThunderIcon, needId: false,
            properties: new[] { ThunderImpactVFX, WeaponPlus1AttackOnly });
        LongMaceThunder.SetCustomSubFeatures(scale);
        LongMaceThunder.WeaponDescription.EffectDescription.effectForms.Add(EffectFormBuilder
            .Create()
            .SetDamageForm(DamageTypeThunder, 1, DieType.D8)
            .Build());
        MerchantContext.AddItem(RecipeHelper.BuildRecipeManual(LongMaceThunder, 48, 16,
            LongMacePrimed,
            ItemDefinitions.Ingredient_Enchant_Stardust), ShopItemType.ShopCrafting);
    }

    private static void BuildHandXbow()
    {
        var scale = new CustomScale(0.5f);

        HandXbowWeaponType = WeaponTypeDefinitionBuilder
            .Create(WeaponTypeDefinitions.LightCrossbowType, CeHandXbowType)
            .SetGuiPresentation(Category.Item, Gui.NoLocalization)
            .SetWeaponCategory(WeaponCategoryDefinitions.MartialWeaponCategory)
            .SetAnimationTag("Rapier")
            .AddToDB();

        var baseItem = ItemDefinitions.LightCrossbow;
        var basePresentation = new ItemPresentation(baseItem.ItemPresentation);
        var baseDescription = new WeaponDescription(baseItem.WeaponDescription)
        {
            weaponType = HandXbowWeaponType.Name,
            closeRange = 6,
            maxRange = 24,
            weaponTags = new List<string>
            {
                TagsDefinitions.WeaponTagLight,
                TagsDefinitions.WeaponTagRange,
                TagsDefinitions.WeaponTagLoading,
                TagsDefinitions.WeaponTagAmmunition
            }
        };
        var damageForm = baseDescription.EffectDescription
            .GetFirstFormOfType(EffectForm.EffectFormType.Damage).DamageForm;

        damageForm.dieType = DieType.D6;
        damageForm.diceNumber = 1;

        //add hand xbow proficiency to rogues
        var rogueHandXbowProficiency = FeatureDefinitionProficiencys.ProficiencyRogueWeapon;

        rogueHandXbowProficiency.Proficiencies.Add(HandXbowWeaponType.Name);

        HandXbow = BuildWeapon("CEHandXbow", baseItem,
            20, true, Common, basePresentation, baseDescription, HandXbowIcon,
            twoHanded: false);
        HandXbow.SetCustomSubFeatures(scale);
        MerchantContext.AddItem(HandXbow, ShopItemType.ShopGenericRanged);

        HandXbowPrimed = BuildWeapon("CEHandXbowPrimed", HandXbow,
            40, true, Uncommon, icon: HandXbowPrimedIcon, twoHanded: false);
        HandXbowPrimed.SetCustomSubFeatures(scale);
        HandXbowPrimed.ItemTags.Add(TagsDefinitions.ItemTagIngredient);
        HandXbowPrimed.ItemTags.Remove(TagsDefinitions.ItemTagStandard);
        MerchantContext.AddItem(HandXbowPrimed, ShopItemType.ShopPrimedRanged);
        MerchantContext.AddItem(RecipeHelper.BuildPrimeManual(HandXbow, HandXbowPrimed), ShopItemType.ShopCrafting);

        HandXbowPlus1 = BuildWeapon("CEHandXbow+1", HandXbow,
            950, true, Rare, icon: HandXbowP1Icon, twoHanded: false,
            properties: new[] { WeaponPlus1 });
        HandXbowPlus1.SetCustomSubFeatures(scale);
        MerchantContext.AddItem(HandXbowPlus1, ShopItemType.ShopRangedPlus1);
        MerchantContext.AddItem(RecipeHelper.BuildRecipeManual(HandXbowPlus1, 24, 10,
            HandXbowPrimed,
            ItemDefinitions.Ingredient_Enchant_Oil_Of_Acuteness), ShopItemType.ShopCrafting);

        var itemDefinition = ItemDefinitions.LightCrossbowPlus2;

        HandXbowPlus2 = BuildWeapon("CEHandXbow+2", HandXbow,
            2500, true, VeryRare,
            itemDefinition.ItemPresentation, icon: HandXbowP2Icon, twoHanded: false,
            properties: new[] { WeaponPlus2 });
        HandXbowPlus2.SetCustomSubFeatures(scale);
        MerchantContext.AddItem(HandXbowPlus2, ShopItemType.ShopRangedPlus2);
        MerchantContext.AddItem(RecipeHelper.BuildRecipeManual(HandXbowPlus2, 48, 16,
            HandXbowPrimed,
            ItemDefinitions.Ingredient_Enchant_Blood_Gem), ShopItemType.ShopCrafting);

        HandXbowAcid = BuildWeapon("CEHandXbowAcid", HandXbow,
            2500, true, VeryRare,
            itemDefinition.ItemPresentation, icon: HandXbowAcidIcon, needId: false, twoHanded: false,
            properties: new[] { AcidImpactVFX, WeaponPlus1AttackOnly });
        HandXbowAcid.SetCustomSubFeatures(scale);
        HandXbowAcid.WeaponDescription.EffectDescription.effectForms.Add(EffectFormBuilder
            .Create()
            .SetDamageForm(DamageTypeAcid, 1, DieType.D8)
            .Build());
        MerchantContext.AddItem(RecipeHelper.BuildRecipeManual(HandXbowAcid, 48, 16,
            HandXbowPrimed,
            ItemDefinitions.Ingredient_Enchant_Stardust), ShopItemType.ShopCrafting);
    }

    private static void WeaponizeProducedFlame()
    {
        var flame = ItemDefinitions.ProducedFlame;

        flame.GuiPresentation = new GuiPresentationBuilder(flame.GuiPresentation)
            .SetTitle("Item/&CEProducedFlameTitle")
            .Build();

        ProducedFlameDart = BuildWeapon("CEProducedFlameDart", ItemDefinitions.Dart, 0, true, Common,
            flame.ItemPresentation, icon: ProducedFlameThrow);

        var damageForm = ProducedFlameDart.WeaponDescription.EffectDescription.FindFirstDamageForm();

        damageForm.damageType = DamageTypeFire;
        damageForm.dieType = DieType.D8;

        var weapon = new WeaponDescription(ItemDefinitions.UnarmedStrikeBase.weaponDefinition);

        weapon.EffectDescription.effectForms.Add(EffectFormBuilder
            .Create()
            .SetDamageForm(DamageTypeFire, 1, DieType.D8)
            .Build());
        flame.staticProperties.Add(BuildFrom(FeatureDefinitionBuilder
            .Create("FeatureProducedFlameThrower")
            .SetGuiPresentationNoContent()
            .SetCustomSubFeatures(
                new ModifyProducedFlameDice(),
                new AddThrowProducedFlameAttack()
            )
            .AddToDB(), false, EquipmentDefinitions.KnowledgeAffinity.ActiveAndHidden));

        flame.IsWeapon = true;
        flame.weaponDefinition = weapon;
    }

    private static void BuildThunderGauntlet()
    {
        ThunderGauntletType = WeaponTypeDefinitionBuilder
            .Create(WeaponTypeDefinitions.UnarmedStrikeType, "CEThunderGauntletType")
            .SetGuiPresentation("Item/&CEThunderGauntletTitle", Gui.NoLocalization)
            .SetWeaponCategory(WeaponCategoryDefinitions.SimpleWeaponCategory)
            .AddToDB();

        var baseItem = ItemDefinitions.UnarmedStrikeBase;
        var basePresentation = baseItem.ItemPresentation;
        var baseDescription = new WeaponDescription(baseItem.WeaponDescription)
        {
            reachRange = 1, weaponType = ThunderGauntletType.Name, weaponTags = new List<string>()
        };
        var damageForm = baseDescription.EffectDescription
            .GetFirstFormOfType(EffectForm.EffectFormType.Damage).DamageForm;

        damageForm.dieType = DieType.D8;
        damageForm.diceNumber = 1;
        damageForm.damageType = DamageTypeThunder;

        const string CONDITION_NAME = "ConditionThunderGauntletDistract";

        baseDescription.EffectDescription.EffectForms.Add(EffectFormBuilder.Create()
            .SetConditionForm(ConditionDefinitionBuilder
                .Create(CONDITION_NAME)
                .SetGuiPresentation(Category.Condition, ConditionDefinitions.ConditionDistracted)
                .SetConditionType(ConditionType.Detrimental)
                .SetAllowMultipleInstances(true)
                .SetSpecialDuration(DurationType.Round, 1)
                .SetFeatures(FeatureDefinitionCombatAffinityBuilder
                    .Create("CombatAffinityThunderGauntletDistract")
                    .SetGuiPresentation(CONDITION_NAME, Category.Condition)
                    .SetMyAttackAdvantage(AdvantageType.Disadvantage)
                    .SetSituationalContext(ExtraSituationalContext.TargetIsNotEffectSource)
                    .AddToDB())
                .AddToDB(), ConditionForm.ConditionOperation.Add)
            .Build());

        ThunderGauntlet = BuildWeapon("CEThunderGauntlet", baseItem, 0, true, Common, basePresentation, baseDescription,
            Sprites.ItemThunderGauntlet, properties: new[] { ThunderImpactVFX });
        ThunderGauntlet.inDungeonEditor = false;
    }

    private static void BuildLightningLauncher()
    {
        LightningLauncherType = WeaponTypeDefinitionBuilder
            .Create(WeaponTypeDefinitions.ShortbowType, "CELightningLauncherType")
            .SetGuiPresentation("Item/&CELightningLauncherTitle", Gui.NoLocalization)
            .SetWeaponCategory(WeaponCategoryDefinitions.SimpleWeaponCategory)
            .SetAnimationTag("Rapier")
            .AddToDB();

        var baseItem = ItemDefinitions.Shortbow;
        var basePresentation = baseItem.ItemPresentation;
        var baseDescription = new WeaponDescription(baseItem.WeaponDescription)
        {
            //TODO: add custom ammunition that looks like lightning
            closeRange = 18, maxRange = 60, weaponType = LightningLauncherType.Name, weaponTags = new List<string>()
        };
        var damageForm = baseDescription.EffectDescription
            .GetFirstFormOfType(EffectForm.EffectFormType.Damage).DamageForm;

        damageForm.dieType = DieType.D6;
        damageForm.diceNumber = 1;
        damageForm.damageType = DamageTypeLightning;

        baseDescription.EffectDescription.EffectForms.Add(EffectFormBuilder.Create()
            .SetConditionForm(ConditionDefinitionBuilder
                .Create(AttackedWithLauncherConditionName)
                .SetGuiPresentationNoContent(true)
                .SetSpecialDuration(DurationType.Round, 1)
                .SetTurnOccurence(TurnOccurenceType.StartOfTurn)
                .AddToDB(), ConditionForm.ConditionOperation.Add, true, false)
            .Build());

        LightningLauncher = BuildWeapon("CELightningLauncher", baseItem, 0, true, Common, basePresentation,
            baseDescription, Sprites.ItemGemLightning, properties: new[] { LightningImpactVFX });
        LightningLauncher.inDungeonEditor = false;
    }

    internal static void ProcessProducedFlameAttack([NotNull] RulesetCharacterHero hero,
        [NotNull] RulesetAttackMode mode)
    {
        var num = hero.characterInventory.CurrentConfiguration;
        var configurations = hero.characterInventory.WieldedItemsConfigurations;

        if (num == configurations.Count - 1)
        {
            num = configurations[num].MainHandSlot.ShadowedSlot != configurations[0].MainHandSlot ? 1 : 0;
        }

        var itemsConfiguration = configurations[num];
        RulesetItem item = null;

        if (mode.SlotName == EquipmentDefinitions.SlotTypeMainHand)
        {
            item = itemsConfiguration.MainHandSlot.EquipedItem;
        }
        else if (mode.SlotName == EquipmentDefinitions.SlotTypeOffHand)
        {
            item = itemsConfiguration.OffHandSlot.EquipedItem;
        }

        if (item == null || item.ItemDefinition != ItemDefinitions.ProducedFlame)
        {
            return;
        }

        hero.CharacterInventory.DefineWieldedItemsConfiguration(num, null, mode.SlotName);
    }

    internal static void AddCustomTags(ItemDefinition item, Dictionary<string, TagsDefinitions.Criticity> tags)
    {
        if (ValidatorsWeapon.IsPolearm(item))
        {
            tags.TryAdd(PolearmWeaponTag, TagsDefinitions.Criticity.Normal);
        }
    }

    internal static ItemDefinition GetStandardWeaponOfType(string type)
    {
        //Darts for some reason are not marked as `Standard`, so return regular Dart for this type 
        if (type == WeaponTypeDefinitions.DartType.Name)
        {
            return ItemDefinitions.Dart;
        }

        var allElements = DatabaseRepository.GetDatabase<ItemDefinition>().GetAllElements();
        foreach (var item in allElements)
        {
            if (item.ItemTags.Contains(TagsDefinitions.ItemTagStandard)
                && item.IsWeapon
                && item.WeaponDescription.WeaponTypeDefinition.Name == type)
            {
                return item;
            }
        }

        return null;
    }

    #region Halberd Icons

    private static AssetReferenceSprite _halberdIcon,
        _halberdPrimedIcon,
        _halberdP1Icon,
        _halberdP2Icon,
        _halberdLightningIcon;

    [NotNull]
    private static AssetReferenceSprite HalberdIcon =>
        _halberdIcon ??= Sprites.GetSprite("Halberd", Resources.Halberd, 128);

    [NotNull]
    private static AssetReferenceSprite HalberdPrimedIcon => _halberdPrimedIcon ??=
        Sprites.GetSprite("HalberdPrimed", Resources.HalberdPrimed, 128);

    [NotNull]
    private static AssetReferenceSprite HalberdP1Icon => _halberdP1Icon ??=
        Sprites.GetSprite("Halberd_1", Resources.Halberd_1, 128);

    [NotNull]
    private static AssetReferenceSprite HalberdP2Icon => _halberdP2Icon ??=
        Sprites.GetSprite("Halberd_2", Resources.Halberd_2, 128);

    [NotNull]
    private static AssetReferenceSprite HalberdLightningIcon => _halberdLightningIcon ??=
        Sprites.GetSprite("HalberdLightning", Resources.HalberdLightning, 128);

    #endregion

    #region Pike Icons

    private static AssetReferenceSprite _pikeIcon,
        _pikePrimedIcon,
        _pikeP1Icon,
        _pikeP2Icon,
        _pikeLightningIcon;

    [NotNull]
    private static AssetReferenceSprite PikeIcon =>
        _pikeIcon ??= Sprites.GetSprite("Pike", Resources.Pike, 128);

    [NotNull]
    private static AssetReferenceSprite PikePrimedIcon => _pikePrimedIcon ??=
        Sprites.GetSprite("PikePrimed", Resources.PikePrimed, 128);

    [NotNull]
    private static AssetReferenceSprite PikeP1Icon => _pikeP1Icon ??=
        Sprites.GetSprite("Pike_1", Resources.Pike_1, 128);

    [NotNull]
    private static AssetReferenceSprite PikeP2Icon => _pikeP2Icon ??=
        Sprites.GetSprite("Pike_2", Resources.Pike_2, 128);

    [NotNull]
    private static AssetReferenceSprite PikePsychicIcon => _pikeLightningIcon ??=
        Sprites.GetSprite("PikePsychic", Resources.PikePsychic, 128);

    #endregion

    #region Long Mace Icons

    private static AssetReferenceSprite _longMaceIcon,
        _longMacePrimedIcon,
        _longMaceP1Icon,
        _longMaceP2Icon,
        _longMaceLightningIcon;

    [NotNull]
    private static AssetReferenceSprite LongMaceIcon =>
        _longMaceIcon ??= Sprites.GetSprite("LongMace", Resources.LongMace, 128);

    [NotNull]
    private static AssetReferenceSprite LongMacePrimedIcon => _longMacePrimedIcon ??=
        Sprites.GetSprite("LongMacePrimed", Resources.LongMacePrimed, 128);

    [NotNull]
    private static AssetReferenceSprite LongMaceP1Icon => _longMaceP1Icon ??=
        Sprites.GetSprite("LongMace_1", Resources.LongMace_1, 128);

    [NotNull]
    private static AssetReferenceSprite LongMaceP2Icon => _longMaceP2Icon ??=
        Sprites.GetSprite("LongMace_2", Resources.LongMace_2, 128);

    [NotNull]
    private static AssetReferenceSprite LongMaceThunderIcon => _longMaceLightningIcon ??=
        Sprites.GetSprite("LongMaceThunder", Resources.LongMaceThunder, 128);

    #endregion

    #region Hand Crossbow Icons

    private static AssetReferenceSprite _handXbowIcon,
        _handXbowPrimedIcon,
        _handXbowP1Icon,
        _handXbowP2Icon,
        _handXbowAcidIcon;

    [NotNull]
    private static AssetReferenceSprite HandXbowIcon =>
        _handXbowIcon ??= Sprites.GetSprite("HandXbow", Resources.HandXbow, 128);

    [NotNull]
    private static AssetReferenceSprite HandXbowPrimedIcon => _handXbowPrimedIcon ??=
        Sprites.GetSprite("HandXbowPrimed", Resources.HandXbowPrimed, 128);

    [NotNull]
    private static AssetReferenceSprite HandXbowP1Icon => _handXbowP1Icon ??=
        Sprites.GetSprite("HandXbow_1", Resources.HandXbow_1, 128);

    [NotNull]
    private static AssetReferenceSprite HandXbowP2Icon => _handXbowP2Icon ??=
        Sprites.GetSprite("HandXbow_2", Resources.HandXbow_2, 128);

    [NotNull]
    private static AssetReferenceSprite HandXbowAcidIcon => _handXbowAcidIcon ??=
        Sprites.GetSprite("HandXbowAcid", Resources.HandXbowAcid, 128);

    #endregion

    #region Produced Flame Icons

    private static AssetReferenceSprite _producedFlameThrow;

    [NotNull]
    private static AssetReferenceSprite ProducedFlameThrow => _producedFlameThrow ??=
        Sprites.GetSprite("ProducedFlameThrow", Resources.ProducedFlameThrow, 128);

    #endregion
}

internal sealed class ModifyProducedFlameDice : ModifyAttackModeForWeaponBase
{
    internal ModifyProducedFlameDice() : base((_, weapon, _) =>
        weapon != null && weapon.ItemDefinition == ItemDefinitions.ProducedFlame)
    {
    }

    protected override void TryModifyAttackMode(
        RulesetCharacter character,
        [NotNull] RulesetAttackMode attackMode,
        RulesetItem weapon)
    {
        DamageForm damage = null;

        foreach (var effectForm in attackMode.EffectDescription.effectForms
                     .Where(effectForm => effectForm.FormType == EffectForm.EffectFormType.Damage))
        {
            damage = effectForm.DamageForm;
        }

        if (damage == null)
        {
            return;
        }

        var casterLevel = character.GetAttribute(AttributeDefinitions.CharacterLevel).CurrentValue;

        damage.diceNumber = 1 + SpellAdvancementByCasterLevel[casterLevel - 1];
    }
}

internal sealed class AddThrowProducedFlameAttack : AddExtraAttackBase
{
    internal AddThrowProducedFlameAttack() : base(ActionDefinitions.ActionType.Main, false)
    {
    }

    [NotNull]
    protected override List<RulesetAttackMode> GetAttackModes([NotNull] RulesetCharacterHero hero)
    {
        var result = new List<RulesetAttackMode>();
        AddItemAttack(result, EquipmentDefinitions.SlotTypeMainHand, hero);
        AddItemAttack(result, EquipmentDefinitions.SlotTypeOffHand, hero);
        return result;
    }

    private static void AddItemAttack(ICollection<RulesetAttackMode> attackModes, [NotNull] string slot,
        [NotNull] RulesetCharacterHero hero)
    {
        var item = hero.CharacterInventory.InventorySlotsByName[slot].EquipedItem;
        if (item == null || item.ItemDefinition != ItemDefinitions.ProducedFlame)
        {
            return;
        }

        var strikeDefinition = CustomWeaponsContext.ProducedFlameDart;

        var action = slot == EquipmentDefinitions.SlotTypeOffHand
            ? ActionDefinitions.ActionType.Bonus
            : ActionDefinitions.ActionType.Main;

        var attackMode = hero.RefreshAttackMode(
            action,
            strikeDefinition,
            strikeDefinition.WeaponDescription,
            false,
            false,
            slot,
            hero.attackModifiers,
            hero.FeaturesOrigin,
            item
        );

        attackMode.closeRange = attackMode.maxRange = 6;
        attackMode.Reach = false;
        attackMode.Ranged = true;
        attackMode.Thrown = true;
        attackMode.AttackTags.Remove(TagsDefinitions.WeaponTagMelee);

        attackModes.Add(attackMode);
    }
}

internal sealed class CustomScale
{
    internal readonly float X, Y, Z;

    internal CustomScale(float s) : this(s, s, s)
    {
    }

    internal CustomScale(float x = 1f, float y = 1f, float z = 1f)
    {
        X = x;
        Y = y;
        Z = z;
    }
}
