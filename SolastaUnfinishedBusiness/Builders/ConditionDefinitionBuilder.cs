﻿using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using SolastaUnfinishedBusiness.Api.Extensions;
using SolastaUnfinishedBusiness.Api.Infrastructure;
using UnityEngine.AddressableAssets;

namespace SolastaUnfinishedBusiness.Builders;

[Flags]
internal enum Silent
{
    None,
    WhenAdded = 1,
    WhenRemoved = 2,
    WhenAddedOrRemoved = WhenAdded | WhenRemoved
}

[UsedImplicitly]
internal class ConditionDefinitionBuilder
    : DefinitionBuilder<ConditionDefinition, ConditionDefinitionBuilder>
{
    private static void SetEmptyParticleReferencesWhereNull(ConditionDefinition definition)
    {
        var assetReference = new AssetReference();

        definition.conditionStartParticleReference ??= assetReference;
        definition.conditionParticleReference ??= assetReference;
        definition.conditionEndParticleReference ??= assetReference;
        definition.characterShaderReference ??= assetReference;
    }

    protected override void Initialise()
    {
        base.Initialise();
        SetEmptyParticleReferencesWhereNull(Definition);
    }

    internal ConditionDefinitionBuilder SetAllowMultipleInstances(bool value)
    {
        Definition.allowMultipleInstances = value;
        return this;
    }

    internal ConditionDefinitionBuilder SetCancellingConditions(params ConditionDefinition[] values)
    {
        Definition.cancellingConditions.SetRange(values);
        return this;
    }

    internal ConditionDefinitionBuilder SetAmountOrigin(ConditionDefinition.OriginOfAmount value)
    {
        Definition.amountOrigin = value;
        return this;
    }

    internal ConditionDefinitionBuilder SetAmountOrigin(ExtraOriginOfAmount value, string source = null)
    {
        //ExtraOriginOfAmount uses additionalDamageType as value for class or ability to get amount from
        if (source != null)
        {
            Definition.additionalDamageType = source;
        }

        return SetAmountOrigin((ConditionDefinition.OriginOfAmount)value);
    }

    internal ConditionDefinitionBuilder CopyParticleReferences(ConditionDefinition from)
    {
        Definition.conditionParticleReference = from.conditionParticleReference;
        Definition.conditionStartParticleReference = from.conditionStartParticleReference;
        Definition.conditionEndParticleReference = from.conditionEndParticleReference;
        Definition.recurrentEffectParticleReference = from.recurrentEffectParticleReference;
        return this;
    }

    internal ConditionDefinitionBuilder AddConditionTags(params string[] tags)
    {
        Definition.conditionTags.AddRange(tags);
        return this;
    }

    internal ConditionDefinitionBuilder SetConditionType(RuleDefinitions.ConditionType value)
    {
        Definition.conditionType = value;
        return this;
    }

    internal ConditionDefinitionBuilder IsDetrimental()
    {
        Definition.conditionType = RuleDefinitions.ConditionType.Detrimental;
        return this;
    }

    internal ConditionDefinitionBuilder SetTurnOccurence(RuleDefinitions.TurnOccurenceType value)
    {
        Definition.turnOccurence = value;
        return this;
    }

    internal ConditionDefinitionBuilder SetParentCondition(ConditionDefinition value)
    {
        Definition.parentCondition = value;
        return this;
    }

    internal ConditionDefinitionBuilder SetConditionParticleReference(AssetReference value)
    {
        Definition.conditionParticleReference = value;
        return this;
    }

    internal ConditionDefinitionBuilder SetCharacterShaderReference(AssetReference value)
    {
        Definition.characterShaderReference = value;
        return this;
    }

    internal ConditionDefinitionBuilder ClearFeatures()
    {
        Definition.Features.Clear();
        return this;
    }

    internal ConditionDefinitionBuilder AddFeatures(params FeatureDefinition[] value)
    {
        Definition.Features.AddRange(value);
        Definition.Features.Sort(Sorting.Compare);
        return this;
    }

    internal ConditionDefinitionBuilder SetFeatures(params FeatureDefinition[] value)
    {
        Definition.Features.SetRange(value);
        Definition.Features.Sort(Sorting.Compare);
        return this;
    }

    internal ConditionDefinitionBuilder SetFeatures(IEnumerable<FeatureDefinition> value)
    {
        Definition.Features.SetRange(value);
        Definition.Features.Sort(Sorting.Compare);
        return this;
    }

    internal ConditionDefinitionBuilder SetRecurrentEffectForms(params EffectForm[] forms)
    {
        Definition.RecurrentEffectForms.SetRange(forms);
        return this;
    }

    internal ConditionDefinitionBuilder SetAdditionalDamageType(string damageType)
    {
        Definition.additionalDamageType = damageType;
        return this;
    }

    internal ConditionDefinitionBuilder SetTerminateWhenRemoved()
    {
        Definition.terminateWhenRemoved = true;
        return this;
    }

    internal ConditionDefinitionBuilder SetSilent(Silent silent)
    {
        Definition.silentWhenAdded = silent.HasFlag(Silent.WhenAdded);
        Definition.silentWhenRemoved = silent.HasFlag(Silent.WhenRemoved);
        return this;
    }

    internal ConditionDefinitionBuilder SetSpecialDuration(
        RuleDefinitions.DurationType type,
        int duration = 0,
        bool validateDuration = true)
    {
        if (validateDuration)
        {
            PreConditions.IsValidDuration(type, duration);
        }

        if (duration != 0)
        {
            Definition.durationParameterDie = RuleDefinitions.DieType.D1;
        }

        Definition.specialDuration = true;
        Definition.durationParameter = duration;
        Definition.durationType = type;
        return this;
    }

    internal ConditionDefinitionBuilder SetPossessive()
    {
        Definition.possessive = true;
        return this;
    }

    internal ConditionDefinitionBuilder SetSpecialInterruptions(params RuleDefinitions.ConditionInterruption[] value)
    {
        Definition.SpecialInterruptions.SetRange(value);
        return this;
    }

    internal ConditionDefinitionBuilder SetSpecialInterruptions(params ExtraConditionInterruption[] value)
    {
        Definition.SpecialInterruptions.SetRange(value.Select(v => (RuleDefinitions.ConditionInterruption)v));
        return this;
    }

    internal ConditionDefinitionBuilder AddSpecialInterruptions(params RuleDefinitions.ConditionInterruption[] value)
    {
        Definition.SpecialInterruptions.AddRange(value);
        return this;
    }

#if false
    internal ConditionDefinitionBuilder AddSpecialInterruptions(params ExtraConditionInterruption[] value)
    {
        Definition.SpecialInterruptions.AddRange(value.Select(v => (RuleDefinitions.ConditionInterruption)v));
        return this;
    }

    internal ConditionDefinitionBuilder ClearSpecialInterruptions()
    {
        Definition.SpecialInterruptions.Clear();
        return this;
    }
#endif

    internal ConditionDefinitionBuilder SetInterruptionDamageThreshold(int value)
    {
        Definition.interruptionDamageThreshold = value;
        return this;
    }

    #region Constructors

    protected ConditionDefinitionBuilder(string name, Guid namespaceGuid) : base(name, namespaceGuid)
    {
        SetEmptyParticleReferencesWhereNull(Definition);
    }

    protected ConditionDefinitionBuilder(ConditionDefinition original, string name, Guid namespaceGuid)
        : base(original, name, namespaceGuid)
    {
    }

    #endregion
}
