﻿using SolastaUnfinishedBusiness.Builders;
using SolastaUnfinishedBusiness.Builders.Features;
using static RuleDefinitions;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper.CharacterSubclassDefinitions;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper.SpellDefinitions;

namespace SolastaUnfinishedBusiness.Subclasses;

internal sealed class SorcerousDivineHeart : AbstractSubclass
{
    internal SorcerousDivineHeart()
    {
        var autoPreparedSpellsDivineHeartArun = FeatureDefinitionAutoPreparedSpellsBuilder
            .Create("AutoPreparedSpellsDivineHeartArun")
            .SetGuiPresentation(Category.Feature)
            .SetSpellcastingClass(CharacterClassDefinitions.Sorcerer)
            .AddPreparedSpellGroup(1, ProtectionFromEvilGood)
            .AddToDB();

        var autoPreparedSpellsDivineHeartEinar = FeatureDefinitionAutoPreparedSpellsBuilder
            .Create("AutoPreparedSpellsDivineHeartEinar")
            .SetGuiPresentation(Category.Feature)
            .SetSpellcastingClass(CharacterClassDefinitions.Sorcerer)
            .AddPreparedSpellGroup(1, InflictWounds)
            .AddToDB();

        var autoPreparedSpellsDivineHeartMariake = FeatureDefinitionAutoPreparedSpellsBuilder
            .Create("AutoPreparedSpellsDivineHeartMariake")
            .SetGuiPresentation(Category.Feature)
            .SetSpellcastingClass(CharacterClassDefinitions.Sorcerer)
            .AddPreparedSpellGroup(1, CureWounds)
            .AddToDB();

        var autoPreparedSpellsDivineHeartMisaye = FeatureDefinitionAutoPreparedSpellsBuilder
            .Create("AutoPreparedSpellsDivineHeartMisaye")
            .SetGuiPresentation(Category.Feature)
            .SetSpellcastingClass(CharacterClassDefinitions.Sorcerer)
            .AddPreparedSpellGroup(1, Bane)
            .AddToDB();

        var autoPreparedSpellsDivineHeartPakri = FeatureDefinitionAutoPreparedSpellsBuilder
            .Create("AutoPreparedSpellsDivineHeartPakri")
            .SetGuiPresentation(Category.Feature)
            .SetSpellcastingClass(CharacterClassDefinitions.Sorcerer)
            .AddPreparedSpellGroup(1, Bless)
            .AddToDB();

        var featureSetDivineHeartDeityChoice = FeatureDefinitionFeatureSetBuilder
            .Create(FeatureDefinitionFeatureSets.FeatureSetSorcererDraconicChoice, "FeatureSetDivineHeartDeityChoice")
            .SetGuiPresentation(Category.Feature)
            .ClearFeatureSet()
            .AddFeatureSet(
                autoPreparedSpellsDivineHeartArun,
                autoPreparedSpellsDivineHeartEinar,
                autoPreparedSpellsDivineHeartMariake,
                autoPreparedSpellsDivineHeartMisaye,
                autoPreparedSpellsDivineHeartPakri)
            .AddToDB();

        var attributeModifierDivineHeartDivineFortitude = FeatureDefinitionAttributeModifierBuilder
            .Create(FeatureDefinitionAttributeModifiers.AttributeModifierDwarfHillToughness,
                "AttributeModifierDivineHeartDivineFortitude")
            .SetGuiPresentation(Category.Feature)
            .AddToDB();

        var magicAffinityDivineHeartClericSpellsList = FeatureDefinitionMagicAffinityBuilder
            .Create(FeatureDefinitionMagicAffinitys.MagicAffinityGreenmageGreenMagicList,
                "MagicAffinityDivineHeartClericSpellsList")
            .SetGuiPresentation(Category.Feature)
            .SetExtendedSpellList(SpellListDefinitions.SpellListCleric)
            .AddToDB();

        var conditionDivineHeartEmpoweredHealing = ConditionDefinitionBuilder
            .Create(ConditionDefinitions.ConditionSorcererChildRiftDeflection, "ConditionDivineHeartEmpoweredHealing")
            .SetOrUpdateGuiPresentation(Category.Condition)
            .SetFeatures(
                FeatureDefinitionDieRollModifierBuilder
                    .Create(FeatureDefinitionDieRollModifiers.DieRollModifierEmpoweredSpell,
                        "DieRollModifierDivineHeartEmpoweredHealing")
                    .SetGuiPresentation("PowerDivineHeartEmpoweredHealing", Category.Feature)
                    .SetModifiers(
                        RollContext.HealValueRoll,
                        1,
                        0,
                        2,
                        "Feature/&PowerDivineHeartEmpoweredHealingReroll")
                    .AddToDB())
            .AddToDB();

        var powerDivineHeartEmpoweredHealing = FeatureDefinitionPowerBuilder
            .Create(FeatureDefinitionPowers.PowerSorcererChildRiftDeflection, "PowerDivineHeartEmpoweredHealing")
            .SetGuiPresentation(Category.Feature, HealingWord)
            .SetEffectDescription(EffectDescriptionBuilder.Create()
                .SetDurationData(DurationType.Round, 1)
                .SetTargetingData(Side.Ally, RangeType.Self, 0, TargetType.Self)
                .SetEffectForms(EffectFormBuilder.Create()
                    .SetConditionForm(
                        conditionDivineHeartEmpoweredHealing,
                        ConditionForm.ConditionOperation.Add,
                        false,
                        false)
                    .Build())
                .Build())
            .AddToDB();

        var powerDivineHeartPlanarPortal = FeatureDefinitionPowerBuilder
            .Create("PowerDivineHeartPlanarPortal")
            .SetGuiPresentation(Category.Feature, DimensionDoor)
            .SetUsesFixed(ActivationTime.Action)
            .SetEffectDescription(DimensionDoor.EffectDescription)
            .AddToDB();

        var powerDivineHeartDivineRecovery = FeatureDefinitionPowerBuilder
            .Create("PowerDivineHeartDivineRecovery")
            .SetGuiPresentation(Category.Feature, Heal)
            .SetUsesFixed(ActivationTime.BonusAction, RechargeRate.LongRest)
            .SetEffectDescription(
                EffectDescriptionBuilder
                    .Create(Heal.EffectDescription)
                    .SetTargetingData(Side.Ally, RangeType.Self, 0, TargetType.Self)
                    .Build())
            .AddToDB();

        Subclass = CharacterSubclassDefinitionBuilder
            .Create("SorcerousDivineHeart")
            .SetGuiPresentation(Category.Subclass, DomainLife)
            .AddFeaturesAtLevel(1,
                featureSetDivineHeartDeityChoice,
                attributeModifierDivineHeartDivineFortitude,
                magicAffinityDivineHeartClericSpellsList)
            .AddFeaturesAtLevel(6,
                powerDivineHeartEmpoweredHealing)
            .AddFeaturesAtLevel(14,
                powerDivineHeartPlanarPortal)
            .AddFeaturesAtLevel(18,
                powerDivineHeartDivineRecovery)
            .AddToDB();
    }

    internal override CharacterSubclassDefinition Subclass { get; }

    internal override FeatureDefinitionSubclassChoice SubclassChoice =>
        FeatureDefinitionSubclassChoices.SubclassChoiceSorcerousOrigin;
}
