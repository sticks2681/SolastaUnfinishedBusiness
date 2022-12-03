using System;
using SolastaUnfinishedBusiness.Api.Extensions;
using SolastaUnfinishedBusiness.Api.Helpers;
using SolastaUnfinishedBusiness.Builders;
using SolastaUnfinishedBusiness.Builders.Features;
using SolastaUnfinishedBusiness.CustomBehaviors;
using SolastaUnfinishedBusiness.CustomInterfaces;
using SolastaUnfinishedBusiness.CustomUI;
using SolastaUnfinishedBusiness.Models;
using SolastaUnfinishedBusiness.Properties;
using SolastaUnfinishedBusiness.Spells;
using UnityEngine.AddressableAssets;
using static RuleDefinitions;
using static RuleDefinitions.EffectIncrementMethod;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper.ConditionDefinitions;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper.FeatureDefinitionAttackModifiers;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper.FeatureDefinitionAutoPreparedSpellss;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper.FeatureDefinitionHealingModifiers;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper.FeatureDefinitionMagicAffinitys;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper.FeatureDefinitionMovementAffinitys;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper.FeatureDefinitionPowers;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper.SpellDefinitions;
using static SolastaUnfinishedBusiness.Subclasses.CommonBuilders;

namespace SolastaUnfinishedBusiness.Classes.Armathor.Subclasses;

public static class ArmathorAlinor
{
    public static CharacterSubclassDefinition Build()
    {
        ArmathorBladesong = BuildArmathorBladesong();
        ArmathorEverVigilant = BuildArmathorEverVigilant();
        ArmathorExtraPrepared = BuildArmathorExtraPrepared();
        ArmathorEyesOfTruth = BuildArmathorEyesOfTruth();
        ArmathorPierceTheDarkness = BuildArmathorPierceTheDarkness();

        return CharacterSubclassDefinitionBuilder
            .Create("ArmathorAlinor")
            .SetGuiPresentation(Category.Subclass, CharacterSubclassDefinitions.OathOfDevotion)
            .AddFeaturesAtLevel(1,
                ArmathorBladesong,
                ArmathorEverVigilant,
                ArmathorExtraPrepared,
                ArmathorEyesOfTruth,
                ArmathorPierceTheDarkness,
                AttackModifierMartialSpellBladeMagicWeapon,
                //AttributeModifierSwiftBladeBladeDance,
                AutoPreparedSpellsDomainBattle,
                AutoPreparedSpellsDomainLife,
                AutoPreparedSpellsDomainSun,
                AutoPreparedSpellsOathOfDevotion,
                //FeatureSetLoremasterKeenMind
                HealingModifierDomainLifeDiscipleOfLife,
                MagicAffinityCasterFightingCombatMagic,
                MagicAffinityDomainSunHolyRadiance,
                MagicAffinitySpellBladeIntoTheFray,
                MovementAffinityRangerSwiftBladeQuickStep
            )
            .AddFeaturesAtLevel(2,
                PowerOathOfDevotionSacredWeapon,
                PowerOathOfDevotionTurnUnholy
            )
            /*.AddFeaturesAtLevel(3,
                FeatureSetMarshalKnowYourEnemy
            )*/
            .AddFeaturesAtLevel(6,
                HealingModifierDomainLifeBlessedHealer,
                MagicAffinityCourtMageCounterspellMastery,
                PowerDomainBattleHeraldOfBattle
            )
            .AddFeaturesAtLevel(7,
                PowerCasterFightingWarMagic,
                PowerOathOfDevotionAuraDevotion,
                ReplaceAttackWithCantripCasterFighting
            )
            .AddFeaturesAtLevel(10,
                PowerClericDivineInterventionPaladin
            )
            /*.AddFeaturesAtLevel(14,
                FeatureSetBladeDancerDanceOfVictory
            )*/
            .AddToDB();
    }
    private static FeatureDefinition ArmathorBladesong { get; set; }
    private static FeatureDefinition ArmathorEverVigilant { get; set; }
    private static FeatureDefinition ArmathorExtraPrepared { get; set; }
    private static FeatureDefinition ArmathorEyesOfTruth { get; set; }
    private static FeatureDefinition ArmathorPierceTheDarkness { get; set; }

    private static FeatureDefinition BuildArmathorBladesong()
    {
        return FeatureDefinitionAttributeModifierBuilder
            .Create("AttributeModifierArmathorBladesong")
            .SetGuiPresentation(Category.Feature)
            .SetModifierAbilityScore(AttributeDefinitions.ArmorClass, AttributeDefinitions.Intelligence)
            .AddToDB();
    }

    private static FeatureDefinition BuildArmathorEverVigilant()
    {
        return FeatureDefinitionAttributeModifierBuilder
            .Create("AttributeModifierArmathorEverVigilant")
            .SetGuiPresentation(Category.Feature)
            .SetModifierAbilityScore(AttributeDefinitions.Initiative, AttributeDefinitions.Intelligence)
            .AddToDB();
    }

    private static FeatureDefinition BuildArmathorExtraPrepared()
    {
        return FeatureDefinitionMagicAffinityBuilder
            .Create("MagicAffinityArmathorExtraPrepared")
            .SetGuiPresentation(Category.Feature)
            .SetSpellLearnAndPrepModifiers(1f, 1f, 0, AdvantageType.None,
                PreparedSpellsModifier.SpellcastingAbilityBonus)
            .AddToDB();
    }

    private static FeatureDefinition BuildArmathorPierceTheDarkness()
    {
        return FeatureDefinitionFeatureSetBuilder
            .Create("FeatureSetArmathorPierceTheDarkness")
            .SetGuiPresentation(Category.Feature)
            .AddFeatureSet(FeatureDefinitionSenses.SenseSuperiorDarkvision)
            .AddToDB();
    }

    private static FeatureDefinition BuildArmathorEyesOfTruth()
    {
        return FeatureDefinitionPowerBuilder
            .Create("PowerArmathorEyesOfTruth")
            .SetGuiPresentation(Category.Feature, SeeInvisibility)
            .SetUsesFixed(ActivationTime.Permanent)
            .SetEffectDescription(EffectDescriptionBuilder
                .Create()
                .SetDurationData(DurationType.Permanent, 0, TurnOccurenceType.StartOfTurn)
                .SetTargetingData(Side.Ally, RangeType.Self, 0, TargetType.Self)
                .SetEffectForms(
                    EffectFormBuilder
                        .Create()
                        .SetConditionForm(
                            ConditionDefinitionBuilder
                                .Create("ConditionArmathorEyesOfTruth")
                                .SetGuiPresentation(Category.Condition, ConditionSeeInvisibility)
                                .SetSilent(Silent.WhenAddedOrRemoved)
                                .AddFeatures(FeatureDefinitionSenses.SenseSeeInvisible16)
                                .AddToDB(),
                            ConditionForm.ConditionOperation.Add)
                        .Build())
                .Build())
            .SetShowCasting(false)
            .AddToDB();
    }
}
