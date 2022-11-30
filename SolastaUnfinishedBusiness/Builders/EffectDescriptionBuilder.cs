using System.Linq;
using SolastaUnfinishedBusiness.Api.Infrastructure;
using static RuleDefinitions;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper.SpellDefinitions;

namespace SolastaUnfinishedBusiness.Builders;

internal class EffectDescriptionBuilder
{
    private readonly EffectDescription effect;

    private EffectDescriptionBuilder()
    {
        effect = new EffectDescription
        {
            effectAdvancement = new EffectAdvancement { incrementMultiplier = 1 },
            effectParticleParameters = new EffectParticleParameters()
        };
        effect.effectParticleParameters.Copy(MagicWeapon.EffectDescription.EffectParticleParameters);
    }

    private EffectDescriptionBuilder(EffectDescription effect)
    {
        this.effect = new EffectDescription
        {
            effectAdvancement = new EffectAdvancement { incrementMultiplier = 1 },
            effectParticleParameters = MagicWeapon.EffectDescription.EffectParticleParameters
        };
        this.effect.Copy(effect);
    }

    internal EffectDescription Build()
    {
        return effect;
    }

    internal static EffectDescriptionBuilder Create()
    {
        return new EffectDescriptionBuilder();
    }

    internal static EffectDescriptionBuilder Create(EffectDescription effect)
    {
        return new EffectDescriptionBuilder(effect);
    }

    internal static EffectDescriptionBuilder Create(IMagicEffect effect)
    {
        return new EffectDescriptionBuilder(effect.EffectDescription);
    }

    internal EffectDescriptionBuilder ClearEffectAdvancements()
    {
        effect.effectAdvancement.Clear();
        return this;
    }

    internal EffectDescriptionBuilder ClearRestrictedCreatureFamilies()
    {
        effect.RestrictedCreatureFamilies.Clear();
        return this;
    }

    internal EffectDescriptionBuilder SetRestrictedCreatureFamilies(params CharacterFamilyDefinition[] values)
    {
        effect.RestrictedCreatureFamilies.SetRange(values.Select(x => x.Name));
        return this;
    }

    internal EffectDescriptionBuilder SetCanBePlacedOnCharacter(bool value)
    {
        effect.canBePlacedOnCharacter = value;
        return this;
    }

    internal EffectDescriptionBuilder SetParticleEffectParameters(IMagicEffect reference)
    {
        return SetParticleEffectParameters(reference.EffectDescription.EffectParticleParameters);
    }

    internal EffectDescriptionBuilder SetParticleEffectParameters(EffectParticleParameters parameters)
    {
        effect.effectParticleParameters.Copy(parameters);
        return this;
    }

    internal EffectDescriptionBuilder UseQuickAnimations()
    {
        effect.speedParameter = -1;
        var particles = effect.effectParticleParameters;
        if (particles.casterQuickSpellParticleReference == null
            || !particles.casterQuickSpellParticleReference.IsValid())
        {
            particles.casterQuickSpellParticleReference = particles.casterParticleReference;
        }

        return this;
    }

    internal EffectDescriptionBuilder SetNoSavingThrow()
    {
        effect.hasSavingThrow = false;
        return this;
    }

    internal EffectDescriptionBuilder SetEffectAdvancement(
        EffectIncrementMethod effectIncrementMethod,
        int incrementMultiplier = 1,
        int additionalTargetsPerIncrement = 0,
        int additionalDicePerIncrement = 0,
        int additionalSpellLevelPerIncrement = 0,
        int additionalSummonsPerIncrement = 0,
        int additionalHpPerIncrement = 0,
        int additionalTempHpPerIncrement = 0,
        int additionalTargetCellsPerIncrement = 0,
        int additionalItemBonus = 0,
        AdvancementDuration alteredDuration = AdvancementDuration.None)
    {
        effect.effectAdvancement = new EffectAdvancement
        {
            effectIncrementMethod = effectIncrementMethod,
            incrementMultiplier = incrementMultiplier,
            additionalTargetsPerIncrement = additionalTargetsPerIncrement,
            additionalDicePerIncrement = additionalDicePerIncrement,
            additionalSpellLevelPerIncrement = additionalSpellLevelPerIncrement,
            additionalSummonsPerIncrement = additionalSummonsPerIncrement,
            additionalHPPerIncrement = additionalHpPerIncrement,
            additionalTempHPPerIncrement = additionalTempHpPerIncrement,
            additionalTargetCellsPerIncrement = additionalTargetCellsPerIncrement,
            additionalItemBonus = additionalItemBonus,
            alteredDuration = alteredDuration
        };
        return this;
    }

    internal EffectDescriptionBuilder SetTargetingData(
        Side targetSide,
        RangeType rangeType,
        int rangeParameter,
        TargetType targetType,
        int targetParameter = 1,
        int targetParameter2 = 2,
        ActionDefinitions.ItemSelectionType itemSelectionType = ActionDefinitions.ItemSelectionType.None)
    {
        effect.targetSide = targetSide;
        effect.rangeType = rangeType;
        effect.rangeParameter = rangeParameter;
        effect.targetType = targetType;
        effect.targetParameter = targetParameter;
        effect.targetParameter2 = targetParameter2;
        effect.itemSelectionType = itemSelectionType;
        return this;
    }

    internal EffectDescriptionBuilder ExcludeCaster()
    {
        effect.targetExcludeCaster = true;
        return this;
    }

    internal EffectDescriptionBuilder SetTargetProximityData(
        bool requiresTargetProximity,
        int targetProximityDistance)
    {
        effect.requiresTargetProximity = requiresTargetProximity;
        effect.targetProximityDistance = targetProximityDistance;
        return this;
    }

    internal EffectDescriptionBuilder SetTargetFiltering(
        TargetFilteringMethod targetFilteringMethod,
        TargetFilteringTag targetFilteringTag = TargetFilteringTag.No,
        int poolFilterDiceNumber = 0,
        DieType poolFilterDieType = DieType.D1
    )
    {
        effect.targetFilteringMethod = targetFilteringMethod;
        effect.targetFilteringTag = targetFilteringTag;
        effect.poolFilterDiceNumber = poolFilterDiceNumber;
        effect.poolFilterDieType = poolFilterDieType;
        return this;
    }

    internal EffectDescriptionBuilder SetRecurrentEffect(RecurrentEffect recurrentEffect)
    {
        effect.recurrentEffect = recurrentEffect;
        return this;
    }

    internal EffectDescriptionBuilder SetRequiredCondition(ConditionDefinition targetConditionAsset)
    {
        effect.targetConditionAsset = targetConditionAsset;
        effect.targetConditionName = targetConditionAsset.Name;
        return this;
    }

    internal EffectDescriptionBuilder SetDurationData(
        DurationType durationType,
        int durationParameter = 0,
        TurnOccurenceType endOfEffect = TurnOccurenceType.EndOfTurn,
        bool validateDuration = true)
    {
        if (validateDuration)
        {
            PreConditions.IsValidDuration(durationType, durationParameter);
        }

        effect.durationParameter = durationParameter;
        effect.durationType = durationType;
        effect.endOfEffect = endOfEffect;
        return this;
    }

    internal EffectDescriptionBuilder SetIgnoreCover()
    {
        effect.ignoreCover = true;
        return this;
    }

    internal EffectDescriptionBuilder SetRequiresVisibilityForPosition()
    {
        effect.requiresVisibilityForPosition = true;
        return this;
    }

    internal EffectDescriptionBuilder SetHasSavingThrow(
        string savingThrowAbility,
        EffectDifficultyClassComputation difficultyClassComputation,
        string savingThrowDifficultyAbility = AttributeDefinitions.Wisdom,
        int fixedSavingThrowDifficultyClass = 10,
        bool ignoreCover = false,
        bool disableSavingThrowOnAllies = false,
        bool advantageForEnemies = false,
        params SaveAffinityBySenseDescription[] savingThrowAffinitiesBySense
    )
    {
        effect.hasSavingThrow = true;
        effect.disableSavingThrowOnAllies = disableSavingThrowOnAllies;
        effect.savingThrowAbility = savingThrowAbility;
        effect.ignoreCover = ignoreCover;
        effect.difficultyClassComputation = difficultyClassComputation;
        effect.savingThrowDifficultyAbility = savingThrowDifficultyAbility;
        effect.fixedSavingThrowDifficultyClass = fixedSavingThrowDifficultyClass;
        effect.advantageForEnemies = advantageForEnemies;
        effect.savingThrowAffinitiesBySense.SetRange(savingThrowAffinitiesBySense);
        return this;
    }

    internal EffectDescriptionBuilder SetSavingThrowData(
        bool disableSavingThrowOnAllies,
        string savingThrowAbility,
        bool ignoreCover,
        EffectDifficultyClassComputation difficultyClassComputation,
        string savingThrowDifficultyAbility = AttributeDefinitions.Wisdom,
        int fixedSavingThrowDifficultyClass = 10,
        bool advantageForEnemies = false,
        params SaveAffinityBySenseDescription[] savingThrowAffinitiesBySense)
    {
        effect.hasSavingThrow = true;
        effect.disableSavingThrowOnAllies = disableSavingThrowOnAllies;
        effect.savingThrowAbility = savingThrowAbility;
        effect.ignoreCover = ignoreCover;
        effect.difficultyClassComputation = difficultyClassComputation;
        effect.savingThrowDifficultyAbility = savingThrowDifficultyAbility;
        effect.fixedSavingThrowDifficultyClass = fixedSavingThrowDifficultyClass;
        effect.advantageForEnemies = advantageForEnemies;
        effect.savingThrowAffinitiesBySense.SetRange(savingThrowAffinitiesBySense);
        return this;
    }

    internal EffectDescriptionBuilder AddImmuneCreatureFamilies(params CharacterFamilyDefinition[] families)
    {
        effect.ImmuneCreatureFamilies.AddRange(families.Select(f => f.Name));
        return this;
    }

#if false
    internal EffectDescriptionBuilder InviteOptionalAlly(bool value = true)
    {
        effect.inviteOptionalAlly = value;
        return this;
    }
#endif

    internal EffectDescriptionBuilder SetSpeed(SpeedType speedType, float speedParameter = 0f)
    {
        effect.speedType = speedType;
        effect.speedParameter = speedParameter;
        return this;
    }

    internal EffectDescriptionBuilder SetAnimationMagicEffect(AnimationDefinitions.AnimationMagicEffect value)
    {
        effect.animationMagicEffect = value;
        return this;
    }

    internal EffectDescriptionBuilder SetEffectForms(params EffectForm[] effectForms)
    {
        effect.EffectForms.SetRange(effectForms);
        return this;
    }

    internal EffectDescriptionBuilder AddEffectForms(params EffectForm[] effectForms)
    {
        effect.EffectForms.AddRange(effectForms);
        return this;
    }

    internal EffectDescriptionBuilder SetupImpactOffsets(
        bool offsetImpactTimeBasedOnDistance = false,
        float offsetImpactTimeBasedOnDistanceFactor = 0.1f,
        float offsetImpactTimePerTarget = 0.0f)
    {
        effect.offsetImpactTimeBasedOnDistance = offsetImpactTimeBasedOnDistance;
        effect.offsetImpactTimeBasedOnDistanceFactor = offsetImpactTimeBasedOnDistanceFactor;
        effect.offsetImpactTimePerTarget = offsetImpactTimePerTarget;
        return this;
    }
}
