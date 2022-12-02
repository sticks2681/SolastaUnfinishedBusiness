﻿using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using SolastaUnfinishedBusiness.Builders;
using SolastaUnfinishedBusiness.Builders.Features;
using SolastaUnfinishedBusiness.CustomInterfaces;
using static RuleDefinitions;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper.CharacterSubclassDefinitions;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper.ConditionDefinitions;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper.FeatureDefinitionPowers;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper.SpellDefinitions;


namespace SolastaUnfinishedBusiness.Subclasses;

internal sealed class PathOfTheLight : AbstractSubclass
{
    private const string ConditionPathOfTheLightIlluminatedName = "ConditionPathOfTheLightIlluminated";

    private const string AdditionalDamagePathOfTheLightIlluminatingStrikeName =
        "AdditionalDamagePathOfTheLightIlluminatingStrike";

    private const string PowerPathOfTheLightIlluminatingBurstName = "PowerPathOfTheLightIlluminatingBurst";

    private static readonly List<ConditionDefinition> InvisibleConditions =
        new() { ConditionInvisibleBase, ConditionDefinitions.ConditionInvisible, ConditionInvisibleGreater };

    internal PathOfTheLight()
    {
        var faerieFireLightSource =
            FaerieFire.EffectDescription.GetFirstFormOfType(EffectForm.EffectFormType.LightSource);

        var attackDisadvantageAgainstNonSourcePathOfTheLightIlluminated =
            FeatureDefinitionAttackDisadvantageBuilder
                .Create("AttackDisadvantageAgainstNonSourcePathOfTheLightIlluminated")
                .SetGuiPresentation(Category.Feature)
                .SetConditionName(ConditionPathOfTheLightIlluminatedName)
                .AddToDB();

        var featureSetPathOfTheLightIlluminatedPreventInvisibility = FeatureDefinitionFeatureSetBuilder
            .Create("FeatureSetPathOfTheLightIlluminatedPreventInvisibility")
            .SetGuiPresentation(Category.Feature)
            .AddFeatureSet(
                InvisibleConditions
                    .Select(x => FeatureDefinitionConditionAffinityBuilder
                        .Create("ConditionAffinityPathOfTheLightIlluminatedPrevent" +
                                x.Name.Replace("Condition", string.Empty))
                        .SetGuiPresentationNoContent(true)
                        .SetConditionAffinityType(ConditionAffinityType.Immunity)
                        .SetConditionType(x)
                        .AddToDB())
                    .OfType<FeatureDefinition>()
                    .ToArray())
            .AddToDB();

        var conditionPathOfTheLightIlluminated = ConditionDefinitionBuilder
            .Create(ConditionPathOfTheLightIlluminatedName)
            .SetGuiPresentation(Category.Condition, ConditionBranded)
            .SetAllowMultipleInstances(true)
            .SetConditionType(ConditionType.Detrimental)
            .SetSpecialDuration(DurationType.Irrelevant)
            .SetSilent(Silent.WhenAdded)
            .AddFeatures(
                attackDisadvantageAgainstNonSourcePathOfTheLightIlluminated,
                featureSetPathOfTheLightIlluminatedPreventInvisibility)
            .SetCustomSubFeatures(new ConditionIlluminated())
            .AddToDB();

        var lightSourceForm = new LightSourceForm();

        lightSourceForm.Copy(faerieFireLightSource.LightSourceForm);
        lightSourceForm.brightRange = 4;
        lightSourceForm.dimAdditionalRange = 4;

        var additionalDamagePathOfTheLightIlluminatingStrike = FeatureDefinitionAdditionalDamageBuilder
            .Create(AdditionalDamagePathOfTheLightIlluminatingStrikeName)
            .SetGuiPresentationNoContent()
            .SetNotificationTag("IlluminatingStrike")
            .SetSpecificDamageType(DamageTypeRadiant)
            .SetTriggerCondition(AdditionalDamageTriggerCondition.AlwaysActive)
            .SetDamageDice(DieType.D6, 1)
            .SetAddLightSource(true)
            .SetFrequencyLimit(FeatureLimitedUsage.OnceInMyTurn)
            .SetConditionOperations(new ConditionOperationDescription
            {
                Operation = ConditionOperationDescription.ConditionOperation.Add,
                ConditionDefinition = conditionPathOfTheLightIlluminated
            })
            .SetLightSourceForm(lightSourceForm)
            .SetRequiredProperty(RestrictedContextRequiredProperty.None)
            .SetAdvancement(AdditionalDamageAdvancement.ClassLevel, 1, 1, 10)
            .SetCustomSubFeatures(new BarbarianHolder())
            .AddToDB();

        foreach (var invisibleCondition in InvisibleConditions)
        {
            additionalDamagePathOfTheLightIlluminatingStrike.ConditionOperations.Add(new ConditionOperationDescription
            {
                Operation = ConditionOperationDescription.ConditionOperation.Remove,
                ConditionDefinition = invisibleCondition
            });
        }

        var featureSetPathOfTheLightIlluminatingStrike = FeatureDefinitionFeatureSetBuilder
            .Create("FeatureSetPathOfTheLightIlluminatingStrike")
            .SetGuiPresentation(Category.Feature)
            .AddFeatureSet(
                FeatureDefinitionPowerBuilder
                    .Create("PowerPathOfTheLightIlluminatingStrike")
                    .SetGuiPresentationNoContent(true)
                    .SetUsesFixed(ActivationTime.OnRageStartAutomatic)
                    .SetEffectDescription(
                        EffectDescriptionBuilder
                            .Create()
                            .SetDurationData(DurationType.Minute, 1, TurnOccurenceType.StartOfTurn)
                            .SetEffectForms(
                                EffectFormBuilder
                                    .Create()
                                    .SetConditionForm(
                                        ConditionDefinitionBuilder
                                            .Create("ConditionPathOfTheLightIlluminatingStrikeInitiator")
                                            .SetGuiPresentationNoContent(true)
                                            .SetTerminateWhenRemoved()
                                            .SetSilent(Silent.WhenAddedOrRemoved)
                                            .SetSpecialInterruptions(ConditionInterruption.RageStop)
                                            .SetFeatures(additionalDamagePathOfTheLightIlluminatingStrike)
                                            .AddToDB(),
                                        ConditionForm.ConditionOperation.Add)
                                    .Build())
                            .Build())
                    .AddToDB())
            .AddToDB();

        var featureSetPathOfTheLightPierceTheDarkness = FeatureDefinitionFeatureSetBuilder
            .Create("FeatureSetPathOfTheLightPierceTheDarkness")
            .SetGuiPresentation(Category.Feature)
            .AddFeatureSet(FeatureDefinitionSenses.SenseSuperiorDarkvision)
            .AddToDB();

        var featureSetPathOfTheLightLightsProtection = FeatureDefinitionFeatureSetBuilder
            .Create("FeatureSetPathOfTheLightLightsProtection")
            .SetGuiPresentation(Category.Feature)
            .AddFeatureSet(
                FeatureDefinitionOpportunityAttackImmunityBuilder
                    .Create("OpportunityAttackImmunityIfAttackerHasConditionPathOfTheLightLightsProtection")
                    .SetGuiPresentationNoContent(true)
                    .SetConditionName(ConditionPathOfTheLightIlluminatedName)
                    .AddToDB())
            .AddToDB();

        var pathOfTheLightIlluminatingStrikeImprovement = FeatureDefinitionBuilder
            .Create("FeaturePathOfTheLightIlluminatingStrikeImprovement")
            .SetGuiPresentation(Category.Feature)
            .AddToDB();

        var powerPathOfTheLightEyesOfTruth = FeatureDefinitionPowerBuilder
            .Create("PowerPathOfTheLightEyesOfTruth")
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
                                .Create("ConditionPathOfTheLightEyesOfTruth")
                                .SetGuiPresentation(Category.Condition, ConditionSeeInvisibility)
                                .SetSilent(Silent.WhenAddedOrRemoved)
                                .AddFeatures(FeatureDefinitionSenses.SenseSeeInvisible16)
                                .AddToDB(),
                            ConditionForm.ConditionOperation.Add)
                        .Build())
                .Build())
            .SetShowCasting(false)
            .AddToDB();

        //
        // Illuminating Burst
        //

        var conditionPathOfTheLightSuppressedIlluminatingBurst = ConditionDefinitionBuilder
            .Create("ConditionPathOfTheLightSuppressedIlluminatingBurst")
            .SetGuiPresentationNoContent(true)
            .SetConditionType(ConditionType.Neutral)
            .SetSilent(Silent.WhenAddedOrRemoved)
            .AddToDB();

        var powerPathOfTheLightIlluminatingBurstSuppressor = FeatureDefinitionPowerBuilder
            .Create("PowerPathOfTheLightIlluminatingBurstSuppressor")
            .SetGuiPresentationNoContent(true)
            .SetUsesFixed(ActivationTime.Permanent)
            .SetEffectDescription(
                EffectDescriptionBuilder
                    .Create()
                    .SetDurationData(DurationType.Permanent, 0, TurnOccurenceType.StartOfTurn)
                    .SetTargetingData(Side.Ally, RangeType.Self, 0, TargetType.Self)
                    .SetRecurrentEffect(RecurrentEffect.OnActivation | RecurrentEffect.OnTurnStart)
                    .SetEffectForms(
                        EffectFormBuilder
                            .Create()
                            .SetConditionForm(
                                conditionPathOfTheLightSuppressedIlluminatingBurst,
                                ConditionForm.ConditionOperation.Add)
                            .Build())
                    .Build())
            .AddToDB();

        var effectDescription = EffectDescriptionBuilder
            .Create()
            .SetSavingThrowData(
                false,
                AttributeDefinitions.Constitution,
                false,
                EffectDifficultyClassComputation.AbilityScoreAndProficiency,
                AttributeDefinitions.Constitution)
            .SetDurationData(DurationType.Minute, 1)
            .SetTargetingData(Side.Enemy, RangeType.Distance, 6, TargetType.IndividualsUnique, 3)
            .SetSpeed(SpeedType.CellsPerSeconds, 9.5f)
            .SetParticleEffectParameters(GuidingBolt)
            .SetEffectForms(
                EffectFormBuilder
                    .Create()
                    .SetDamageForm(DamageTypeRadiant, 4, DieType.D6)
                    .HasSavingThrow(EffectSavingThrowType.Negates)
                    .Build(),
                EffectFormBuilder
                    .Create()
                    .SetConditionForm(
                        ConditionDefinitionBuilder
                            .Create("ConditionPathOfTheLightIlluminatedByBurst")
                            .SetGuiPresentation("ConditionPathOfTheLightIlluminated", Category.Condition,
                                ConditionBranded)
                            .SetAllowMultipleInstances(true)
                            .SetConditionType(ConditionType.Detrimental)
                            .SetParentCondition(conditionPathOfTheLightIlluminated)
                            .SetSilent(Silent.WhenAdded)
                            .SetCustomSubFeatures(new ConditionIlluminatedByBurst())
                            .AddToDB(),
                        ConditionForm.ConditionOperation.Add)
                    .CanSaveToCancel(TurnOccurenceType.EndOfTurn)
                    .HasSavingThrow(EffectSavingThrowType.Negates)
                    .Build(),
                EffectFormBuilder
                    .Create()
                    .SetLightSourceForm(
                        LightSourceType.Basic,
                        4,
                        4,
                        faerieFireLightSource.lightSourceForm.color,
                        faerieFireLightSource.lightSourceForm.graphicsPrefabReference)
                    .HasSavingThrow(EffectSavingThrowType.Negates)
                    .Build())
            .Build();

        var featureSetPathOfTheLightIlluminatingBurst = FeatureDefinitionFeatureSetBuilder
            .Create("FeatureSetPathOfTheLightIlluminatingBurst")
            .SetGuiPresentation(Category.Feature)
            .AddFeatureSet(
                FeatureDefinitionPowerBuilder
                    .Create("PowerPathOfTheLightIlluminatingBurstInitiator")
                    .SetGuiPresentationNoContent(true)
                    .SetUsesFixed(ActivationTime.OnRageStartAutomatic)
                    .SetEffectDescription(
                        EffectDescriptionBuilder
                            .Create()
                            .SetDurationData(DurationType.Round, 1)
                            .SetEffectForms(
                                EffectFormBuilder
                                    .Create()
                                    .SetConditionForm(
                                        conditionPathOfTheLightSuppressedIlluminatingBurst,
                                        ConditionForm.ConditionOperation.Remove)
                                    .Build())
                            .Build())
                    .SetShowCasting(false)
                    .AddToDB(),
                FeatureDefinitionPowerBuilder
                    .Create(PowerPathOfTheLightIlluminatingBurstName)
                    .SetGuiPresentation(Category.Feature, PowerDomainSunHeraldOfTheSun)
                    .SetUsesFixed(ActivationTime.NoCost, RechargeRate.OneMinute)
                    .SetEffectDescription(effectDescription)
                    .SetDisableIfConditionIsOwned(conditionPathOfTheLightSuppressedIlluminatingBurst)
                    .SetShowCasting(false)
                    .SetCustomSubFeatures(new PowerIlluminatingBurst())
                    .AddToDB(),
                powerPathOfTheLightIlluminatingBurstSuppressor)
            .AddToDB();

        Subclass = CharacterSubclassDefinitionBuilder
            .Create("PathOfTheLight")
            .SetGuiPresentation(Category.Subclass, DomainSun)
            .AddFeaturesAtLevel(3,
                featureSetPathOfTheLightIlluminatingStrike,
                featureSetPathOfTheLightPierceTheDarkness)
            .AddFeaturesAtLevel(6,
                featureSetPathOfTheLightLightsProtection)
            .AddFeaturesAtLevel(10,
                powerPathOfTheLightEyesOfTruth,
                pathOfTheLightIlluminatingStrikeImprovement)
            .AddFeaturesAtLevel(14,
                featureSetPathOfTheLightIlluminatingBurst)
            .AddToDB();
    }

    internal override CharacterSubclassDefinition Subclass { get; }

    internal override FeatureDefinitionSubclassChoice SubclassChoice =>
        FeatureDefinitionSubclassChoices.SubclassChoiceBarbarianPrimalPath;

    private static void ApplyLightsProtectionHealing(ulong sourceGuid)
    {
        if (RulesetEntity.GetEntity<RulesetCharacter>(sourceGuid) is not RulesetCharacterHero conditionSource ||
            conditionSource.IsDead)
        {
            return;
        }

        if (!conditionSource.ClassesAndLevels.TryGetValue(CharacterClassDefinitions.Barbarian, out var levelsInClass))
        {
            return;
        }

        var amountHealed = (levelsInClass + 1) / 2;

        conditionSource.ReceiveHealing(amountHealed, true, sourceGuid);
    }

    private static void HandleAfterIlluminatedConditionRemoved(RulesetActor removedFrom)
    {
        if (removedFrom is not RulesetCharacter character)
        {
            return;
        }

        // includes conditions that have Illuminated as their parent (like the Illuminating Burst condition)
        if (character.HasConditionOfTypeOrSubType(ConditionPathOfTheLightIlluminatedName) ||
            (character.PersonalLightSource?.SourceName != AdditionalDamagePathOfTheLightIlluminatingStrikeName &&
             character.PersonalLightSource?.SourceName != PowerPathOfTheLightIlluminatingBurstName))
        {
            return;
        }

        var visibilityService = ServiceRepository.GetService<IGameLocationVisibilityService>();
        var gameLocationCharacter = GameLocationCharacter.GetFromActor(removedFrom);

        visibilityService.RemoveCharacterLightSource(gameLocationCharacter, character.PersonalLightSource);
        character.PersonalLightSource = null;
    }

    //
    // behavior classes
    //

    private sealed class PowerIlluminatingBurst : IStartOfTurnRecharge
    {
        public bool IsRechargeSilent => true;
    }

    private sealed class ConditionIlluminated : IConditionRemovedOnSourceTurnStart, INotifyConditionRemoval
    {
        public void AfterConditionRemoved(RulesetActor removedFrom, RulesetCondition rulesetCondition)
        {
            HandleAfterIlluminatedConditionRemoved(removedFrom);
        }

        public void BeforeDyingWithCondition(RulesetActor rulesetActor, [NotNull] RulesetCondition rulesetCondition)
        {
            ApplyLightsProtectionHealing(rulesetCondition.SourceGuid);
        }
    }

    private sealed class ConditionIlluminatedByBurst : INotifyConditionRemoval
    {
        public void AfterConditionRemoved(RulesetActor removedFrom, RulesetCondition rulesetCondition)
        {
            HandleAfterIlluminatedConditionRemoved(removedFrom);
        }

        public void BeforeDyingWithCondition(RulesetActor rulesetActor, [NotNull] RulesetCondition rulesetCondition)
        {
            ApplyLightsProtectionHealing(rulesetCondition.SourceGuid);
        }
    }

    private sealed class BarbarianHolder : IClassHoldingFeature
    {
        // allows Illuminating Strike damage to scale with barbarian level
        public CharacterClassDefinition Class => CharacterClassDefinitions.Barbarian;
    }
}
