﻿using SolastaUnfinishedBusiness.Api.Extensions;
using SolastaUnfinishedBusiness.Builders;
using SolastaUnfinishedBusiness.Builders.Features;
using SolastaUnfinishedBusiness.CustomBehaviors;
using SolastaUnfinishedBusiness.CustomUI;
using UnityEngine;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper.ConditionDefinitions;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper.SpellDefinitions;
using static RuleDefinitions;
using Resources = SolastaUnfinishedBusiness.Properties.Resources;

namespace SolastaUnfinishedBusiness.Spells;

internal static partial class SpellBuilders
{
    #region CANTRIPS

    private static ConditionDefinition _acidClawCondition;
    internal static ConditionDefinition AcidClawCondition => _acidClawCondition ??= BuildAcidClawCondition();

    private static ConditionDefinition BuildAcidClawCondition()
    {
        return ConditionDefinitionBuilder
            .Create("ConditionAcidClaws")
            .SetGuiPresentation(Category.Condition, ConditionAcidSpit)
            .SetConditionType(ConditionType.Detrimental)
            .SetSpecialDuration(DurationType.Round, 1)
            .SetFeatures(FeatureDefinitionAttributeModifierBuilder
                .Create("AttributeModifierAcidClawsACDebuff")
                .SetGuiPresentation("ConditionAcidClaws", Category.Condition)
                .SetModifier(FeatureDefinitionAttributeModifier.AttributeModifierOperation.Additive,
                    AttributeDefinitions.ArmorClass, -1)
                .AddToDB())
            .AddToDB();
    }

    internal static SpellDefinition BuildAcidClaw()
    {
        const string NAME = "AcidClaws";

        var spriteReference = Sprites.GetSprite(NAME, Resources.AcidClaws, 128, 128);

        var effectDescription = EffectDescriptionBuilder
            .Create()
            .SetEffectAdvancement(EffectIncrementMethod.CasterLevelTable, 5, additionalDicePerIncrement: 1)
            .SetDurationData(DurationType.Instantaneous)
            .SetTargetingData(Side.Enemy, RangeType.MeleeHit, 1, TargetType.Individuals)
            .SetEffectForms(
                EffectFormBuilder
                    .Create()
                    .SetDamageForm(dieType: DieType.D8, diceNumber: 1, damageType: DamageTypeAcid)
                    .HasSavingThrow(EffectSavingThrowType.None)
                    .Build(),
                EffectFormBuilder
                    .Create()
                    .SetConditionForm(AcidClawCondition, ConditionForm.ConditionOperation.Add)
                    .HasSavingThrow(EffectSavingThrowType.None)
                    .Build())
            .Build();

        var spell = SpellDefinitionBuilder
            .Create(NAME)
            .SetGuiPresentation(Category.Spell, spriteReference)
            .SetEffectDescription(effectDescription)
            .SetCastingTime(ActivationTime.Action)
            .SetSpellLevel(0)
            .SetVerboseComponent(false)
            .SetVocalSpellSameType(VocalSpellSemeType.Attack)
            .SetSchoolOfMagic(SchoolOfMagicDefinitions.SchoolTransmutation)
            .AddToDB();

        return spell;
    }

    internal static SpellDefinition BuildAirBlast()
    {
        const string NAME = "AirBlast";

        var spriteReference = Sprites.GetSprite(NAME, Resources.AirBlast, 128, 128);

        var effectDescription = EffectDescriptionBuilder
            .Create()
            .SetEffectAdvancement(EffectIncrementMethod.CasterLevelTable, 1, 0, 1)
            .SetSavingThrowData(
                false,
                AttributeDefinitions.Strength,
                false,
                EffectDifficultyClassComputation.SpellCastingFeature,
                AttributeDefinitions.Wisdom,
                15)
            .SetDurationData(DurationType.Instantaneous)
            .SetTargetingData(Side.Enemy, RangeType.Distance, 6, TargetType.Individuals)
            .SetEffectForms(
                EffectFormBuilder
                    .Create()
                    .SetMotionForm(MotionForm.MotionType.PushFromOrigin, 1)
                    .HasSavingThrow(EffectSavingThrowType.Negates).Build(),
                EffectFormBuilder
                    .Create()
                    .SetDamageForm(DamageTypeBludgeoning, 1, DieType.D6)
                    .HasSavingThrow(EffectSavingThrowType.Negates)
                    .Build())
            .Build();

        var spell = SpellDefinitionBuilder
            .Create(NAME)
            .SetGuiPresentation(Category.Spell, spriteReference)
            .SetEffectDescription(effectDescription)
            .SetCastingTime(ActivationTime.Action)
            .SetSpellLevel(0)
            .SetVocalSpellSameType(VocalSpellSemeType.Attack)
            .SetMaterialComponent(MaterialComponentType.None)
            .SetSchoolOfMagic(SchoolOfMagicDefinitions.SchoolTransmutation)
            .AddToDB();

        return spell;
    }

    internal static SpellDefinition BuildBurstOfRadiance()
    {
        const string NAME = "BurstOfRadiance";

        var spriteReference = Sprites.GetSprite(NAME, Resources.BurstOfRadiance, 128, 128);

        var effectDescription = EffectDescriptionBuilder
            .Create()
            .SetEffectAdvancement(EffectIncrementMethod.CasterLevelTable, 1, 0, 1)
            .SetSavingThrowData(
                false,
                AttributeDefinitions.Constitution,
                false,
                EffectDifficultyClassComputation.SpellCastingFeature,
                AttributeDefinitions.Wisdom,
                13)
            .SetDurationData(DurationType.Instantaneous)
            .SetParticleEffectParameters(BurningHands)
            .SetTargetingData(Side.Enemy, RangeType.Self, 0, TargetType.Sphere)
            .SetEffectForms(
                EffectFormBuilder
                    .Create()
                    .SetDamageForm(DamageTypeRadiant, 1, DieType.D6)
                    .HasSavingThrow(EffectSavingThrowType.Negates)
                    .Build())
            .Build();

        var spell = SpellDefinitionBuilder
            .Create(NAME)
            .SetGuiPresentation(Category.Spell, spriteReference)
            .SetEffectDescription(effectDescription)
            .SetCastingTime(ActivationTime.Action)
            .SetSpellLevel(0)
            .SetSomaticComponent(false)
            .SetVocalSpellSameType(VocalSpellSemeType.Attack)
            .SetMaterialComponent(MaterialComponentType.Mundane)
            .SetSchoolOfMagic(SchoolOfMagicDefinitions.SchoolEvocation)
            .AddToDB();

        return spell;
    }

    internal static SpellDefinition BuildIlluminatingSphere()
    {
        const string NAME = "IlluminatingSphere";

        var spell = SpellDefinitionBuilder
            .Create(Sparkle, NAME)
            .SetGuiPresentation(Category.Spell, Shine)
            .SetVocalSpellSameType(VocalSpellSemeType.Detection)
            .SetEffectDescription(EffectDescriptionBuilder
                .Create(Sparkle.EffectDescription)
                .SetTargetingData(Side.All, RangeType.Distance, 18, TargetType.Sphere, 5)
                .SetParticleEffectParameters(SacredFlame_B.EffectDescription.EffectParticleParameters)
                .Build())
            .AddToDB();

        return spell;
    }

    internal static SpellDefinition BuildMinorLifesteal()
    {
        var spriteReference = Sprites.GetSprite("MinorLifesteal", Resources.MinorLifesteal, 128);

        return SpellDefinitionBuilder
            .Create("MinorLifesteal")
            .SetGuiPresentation(Category.Spell, spriteReference)
            .SetSchoolOfMagic(SchoolOfMagicDefinitions.SchoolNecromancy)
            .SetMaterialComponent(MaterialComponentType.Mundane)
            .SetVerboseComponent(false)
            .SetVocalSpellSameType(VocalSpellSemeType.Attack)
            .SetSpellLevel(0)
            .SetEffectDescription(EffectDescriptionBuilder
                .Create()
                .SetTargetingData(Side.Enemy, RangeType.RangeHit, 12, TargetType.Individuals)
                .AddImmuneCreatureFamilies(CharacterFamilyDefinitions.Construct, CharacterFamilyDefinitions.Undead)
                .SetEffectAdvancement(EffectIncrementMethod.CasterLevelTable, 5, additionalDicePerIncrement: 1)
                .SetEffectForms(
                    EffectFormBuilder
                        .Create()
                        .SetDamageForm(DamageTypeNecrotic, 1, DieType.D8, 0, HealFromInflictedDamage.Half)
                        .HasSavingThrow(EffectSavingThrowType.None)
                        .Build(),
                    EffectFormBuilder
                        .Create()
                        .SetTempHpForm(0, DieType.D4, 1, true)
                        .HasSavingThrow(EffectSavingThrowType.None)
                        .Build())
                .SetParticleEffectParameters(VampiricTouch)
                .Build())
            .AddToDB();
    }

    internal static SpellDefinition BuildResonatingStrike()
    {
        var resonanceLeap = SpellDefinitionBuilder
            .Create("ResonatingStrikeLeap")
            .SetGuiPresentationNoContent()
            .SetSpellLevel(1)
            .SetSchoolOfMagic(SchoolOfMagicDefinitions.SchoolEvocation)
            .SetSomaticComponent(false)
            .SetVerboseComponent(false)
            .SetCustomSubFeatures(new BonusSlotLevelsByClassLevel())
            .SetCastingTime(ActivationTime.Action)
            .SetEffectDescription(EffectDescriptionBuilder.Create()
                .SetParticleEffectParameters(Thunderwave)
                .SetTargetFiltering(TargetFilteringMethod.CharacterOnly)
                .SetTargetingData(Side.Enemy, RangeType.Touch, 1, TargetType.Individuals)
                .SetEffectForms(EffectFormBuilder.Create()
                    .SetBonusMode(AddBonusMode.AbilityBonus)
                    .SetDamageForm(DamageTypeThunder, diceNumber: 0, dieType: DieType.D8)
                    .Build())
                .SetEffectAdvancement(EffectIncrementMethod.PerAdditionalSlotLevel, 1, additionalDicePerIncrement: 1)
                .Build())
            .AddToDB();

        return SpellDefinitionBuilder
            .Create("ResonatingStrike")
            .SetGuiPresentation(Category.Spell,
                Sprites.GetSprite("ResonatingStrike", Resources.ResonatingStrike, 128, 128))
            .SetSpellLevel(0)
            .SetSchoolOfMagic(SchoolOfMagicDefinitions.SchoolEvocation)
            .SetVerboseComponent(false)
            .SetMaterialComponent(MaterialComponentType.Specific)
            .SetSpecificMaterialComponent(TagsDefinitions.WeaponTagMelee, 0, false)
            .SetCustomSubFeatures(
                PerformAttackAfterMagicEffectUse.MeleeAttack,
                CustomSpellEffectLevel.ByCasterLevel,
                new ChainSpellEffectOnAttackHit(resonanceLeap, "ResonatingStrike")
            )
            .SetCastingTime(ActivationTime.Action)
            .SetEffectDescription(EffectDescriptionBuilder.Create()
                .SetParticleEffectParameters(Thunderwave)
                .SetTargetProximityData(true, 1)
                .SetTargetingData(Side.Enemy, RangeType.Distance, 5, TargetType.IndividualsUnique, 2)
                .SetIgnoreCover()
                .SetEffectAdvancement(EffectIncrementMethod.CasterLevelTable, additionalDicePerIncrement: 1,
                    incrementMultiplier: 1)
                .SetDurationData(DurationType.Round, 1)
                .SetEffectForms(EffectFormBuilder.Create()
                    .HasSavingThrow(EffectSavingThrowType.None)
                    .SetConditionForm(ConditionDefinitionBuilder
                            .Create("ConditionResonatingStrike")
                            .SetGuiPresentation(Category.Condition)
                            .SetSpecialInterruptions(ConditionInterruption.Attacks)
                            .SetSilent(Silent.WhenAddedOrRemoved)
                            .SetTurnOccurence(TurnOccurenceType.EndOfTurn)
                            .SetFeatures(FeatureDefinitionAdditionalDamageBuilder
                                .Create("AdditionalDamageResonatingStrike")
                                .SetGuiPresentationNoContent(true)
                                .SetNotificationTag("ResonatingStrike")
                                .SetDamageDice(DieType.D8, 0)
                                .SetRequiredProperty(RestrictedContextRequiredProperty.MeleeWeapon)
                                .SetAttackModeOnly()
                                .SetSpecificDamageType(DamageTypeThunder)
                                .SetAdvancement(ExtraAdditionalDamageAdvancement.CharacterLevel, 0, 1, 5)
                                .SetIgnoreCriticalDoubleDice(true)
                                .AddToDB()
                            )
                            .AddToDB(),
                        ConditionForm.ConditionOperation.Add,
                        true,
                        false)
                    .Build())
                .Build())
            .AddToDB();
    }

    internal static SpellDefinition BuildSunlightBlade()
    {
        var highlight = new ConditionOperationDescription
        {
            hasSavingThrow = false,
            operation = ConditionOperationDescription.ConditionOperation.Add,
            conditionDefinition = ConditionDefinitionBuilder
                .Create(ConditionHighlighted, "ConditionSunlightBladeHighlighted")
                .SetSpecialInterruptions(ConditionInterruption.Attacked)
                .SetSpecialDuration(DurationType.Round, 1)
                .SetTurnOccurence(TurnOccurenceType.StartOfTurn)
                .AddToDB()
        };

        var dimLight = new LightSourceForm
        {
            brightRange = 0,
            dimAdditionalRange = 2,
            lightSourceType = LightSourceType.Basic,
            color = new Color(0.9f, 0.8f, 0.4f),
            graphicsPrefabReference = FeatureDefinitionAdditionalDamages
                .AdditionalDamageBrandingSmite.LightSourceForm.graphicsPrefabReference
        };

        var sunlitMark = ConditionDefinitionBuilder
            .Create("ConditionSunlightBladeMarked")
            .SetGuiPresentationNoContent()
            .SetSpecialInterruptions(ExtraConditionInterruption.AfterWasAttacked)
            .AddSpecialInterruptions(ConditionInterruption.AnyBattleTurnEnd)
            .SetSilent(Silent.WhenAddedOrRemoved)
            .SetTurnOccurence(TurnOccurenceType.EndOfTurn)
            .AddToDB();

        return SpellDefinitionBuilder
            .Create("SunlightBlade")
            .SetGuiPresentation(Category.Spell, Sprites.GetSprite("SunlightBlade", Resources.SunlightBlade, 128, 128))
            .SetSpellLevel(0)
            .SetSchoolOfMagic(SchoolOfMagicDefinitions.SchoolEvocation)
            .SetVerboseComponent(false)
            .SetMaterialComponent(MaterialComponentType.Specific)
            .SetSpecificMaterialComponent(TagsDefinitions.WeaponTagMelee, 0, false)
            .SetCustomSubFeatures(
                PerformAttackAfterMagicEffectUse.MeleeAttack,
                new UpgradeRangeBasedOnWeaponReach())
            .SetCastingTime(ActivationTime.Action)
            .SetEffectDescription(EffectDescriptionBuilder.Create()
                .SetParticleEffectParameters(ScorchingRay)
                .SetTargetingData(Side.Enemy, RangeType.Touch, 1, TargetType.IndividualsUnique)
                .SetIgnoreCover()
                .SetEffectAdvancement( //this is needed for tooltip
                    EffectIncrementMethod.CasterLevelTable,
                    additionalDicePerIncrement: 1,
                    incrementMultiplier: 1)
                .SetDurationData(DurationType.Round, 1)
                .SetEffectForms(EffectFormBuilder.Create()
                        .HasSavingThrow(EffectSavingThrowType.None)
                        .SetConditionForm(ConditionDefinitionBuilder
                                .Create("ConditionSunlightBlade")
                                .SetGuiPresentation(Category.Condition)
                                .SetSpecialInterruptions(ConditionInterruption.AnyBattleTurnEnd)
                                .SetSilent(Silent.WhenAddedOrRemoved)
                                .SetTurnOccurence(TurnOccurenceType.EndOfTurn)
                                .SetFeatures(FeatureDefinitionAdditionalDamageBuilder
                                    .Create("AdditionalDamageSunlightBlade")
                                    .SetGuiPresentationNoContent(true)
                                    .SetNotificationTag("SunlightBlade")
                                    .SetRequiredProperty(RestrictedContextRequiredProperty.MeleeWeapon)
                                    .SetAttackModeOnly()
                                    .SetAttackOnly()
                                    .SetDamageDice(DieType.D8, 1)
                                    .SetSpecificDamageType(DamageTypeRadiant)
                                    .SetAdvancement(ExtraAdditionalDamageAdvancement.CharacterLevel, 1, 1, 6, 5)
                                    .SetTargetCondition(sunlitMark, AdditionalDamageTriggerCondition.TargetHasCondition)
                                    .SetConditionOperations(highlight)
                                    .SetAddLightSource(true)
                                    .SetLightSourceForm(dimLight)
                                    .AddToDB())
                                .AddToDB(),
                            ConditionForm.ConditionOperation.Add,
                            true,
                            false)
                        .Build(),
                    EffectFormBuilder.Create()
                        .HasSavingThrow(EffectSavingThrowType.None)
                        .SetConditionForm(sunlitMark, ConditionForm.ConditionOperation.Add)
                        .Build())
                .Build())
            .AddToDB();
    }

    internal static SpellDefinition BuildThornyVines()
    {
        var spriteReference = Sprites.GetSprite("ThornyVines", Resources.ThornyVines, 128);

        return SpellDefinitionBuilder
            .Create("ThornyVines")
            .SetGuiPresentation(Category.Spell, spriteReference)
            .SetSpellLevel(0)
            .SetSchoolOfMagic(SchoolOfMagicDefinitions.SchoolTransmutation)
            .SetVocalSpellSameType(VocalSpellSemeType.Attack)
            .SetMaterialComponent(MaterialComponentType.Mundane)
            .SetCastingTime(ActivationTime.Action)
            .SetEffectDescription(EffectDescriptionBuilder
                .Create()
                .SetTargetingData(Side.Enemy, RangeType.RangeHit, 6, TargetType.Individuals)
                .SetParticleEffectParameters(VenomousSpike)
                .SetEffectAdvancement(EffectIncrementMethod.CasterLevelTable, 5,
                    additionalDicePerIncrement: 1)
                .SetEffectForms(
                    EffectFormBuilder
                        .Create()
                        .SetDamageForm(diceNumber: 1, dieType: DieType.D6, damageType: DamageTypePiercing)
                        .Build(),
                    EffectFormBuilder
                        .Create()
                        .SetMotionForm(MotionForm.MotionType.DragToOrigin, 2)
                        .Build())
                .Build())
            .AddToDB();
    }

    internal static SpellDefinition BuildThunderStrike()
    {
        const string NAME = "ThunderStrike";

        var spriteReference = Shield;

        var effectDescription = EffectDescriptionBuilder
            .Create()
            .SetEffectAdvancement(
                EffectIncrementMethod.CasterLevelTable, 1, 0, 1)
            .SetSavingThrowData(
                false,
                AttributeDefinitions.Constitution,
                false,
                EffectDifficultyClassComputation.SpellCastingFeature,
                AttributeDefinitions.Wisdom,
                15)
            .SetDurationData(DurationType.Instantaneous)
            .SetTargetingData(Side.All, RangeType.Self, 0, TargetType.Sphere)
            .ExcludeCaster()
            .SetEffectForms(
                EffectFormBuilder
                    .Create()
                    .SetDamageForm(DamageTypeThunder, 1, DieType.D6)
                    .HasSavingThrow(EffectSavingThrowType.Negates)
                    .Build())
            .Build();

        var spell = SpellDefinitionBuilder
            .Create(NAME)
            .SetGuiPresentation(Category.Spell, spriteReference)
            .SetEffectDescription(effectDescription)
            .SetCastingTime(ActivationTime.Action)
            .SetSpellLevel(0)
            .SetVocalSpellSameType(VocalSpellSemeType.Attack)
            .SetSchoolOfMagic(SchoolOfMagicDefinitions.SchoolEvocation)
            .AddToDB();

        return spell;
    }

    #endregion
}
