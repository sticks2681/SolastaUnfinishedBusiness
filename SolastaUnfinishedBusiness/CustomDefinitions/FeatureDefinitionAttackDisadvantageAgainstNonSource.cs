﻿using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using JetBrains.Annotations;
using SolastaUnfinishedBusiness.Builders.Features;
using static RuleDefinitions;

namespace SolastaUnfinishedBusiness.CustomDefinitions;

/// <summary>
///     Imposes disadvantage when attacking anyone but the source of the specified condition.
/// </summary>
internal sealed class FeatureDefinitionAttackDisadvantageAgainstNonSource : FeatureDefinition, ICombatAffinityProvider
{
    internal string ConditionName { get; set; }

    public SituationalContext SituationalContext => SituationalContext.None;
    public bool CanRageToOvercomeSurprise => false;
    public bool AutoCritical => false;
    public bool CriticalHitImmunity => false;
    [CanBeNull] public ConditionDefinition RequiredCondition => null;
    public bool IgnoreCover => false;
    public CoverType PermanentCover => CoverType.None;
    public AdvantageType ReadyAttackAdvantage => AdvantageType.None;
    public bool ShoveOnReadyAttackHit => false;

    public void ComputeAttackModifier(RulesetCharacter myself, RulesetCharacter defender, RulesetAttackMode attackMode,
        ActionModifier attackModifier, FeatureOrigin featureOrigin, int bardicDieRoll, float distance)
    {
        if (!myself.ConditionsByCategory.SelectMany(keyValuePair => keyValuePair.Value).Any(rulesetCondition =>
                rulesetCondition.ConditionDefinition.IsSubtypeOf(ConditionName) &&
                rulesetCondition.SourceGuid != defender.Guid))
        {
            return;
        }

        attackModifier.AttackAdvantageTrends.Add(new TrendInfo(-1,
            featureOrigin.sourceType, featureOrigin.sourceName, featureOrigin.source));
    }

    public void ComputeDefenseModifier(RulesetCharacter myself, RulesetCharacter attacker, int sustainedAttacks,
        bool defenderAlreadyAttackedByAttackerThisTurn, ActionModifier attackModifier,
        FeatureOrigin featureOrigin, float distance)
    {
        // Intentionally empty?
    }

    public AdvantageType GetAdvantageOnOpportunityAttackOnMe(RulesetCharacter myself,
        RulesetCharacter attacker, float distance)
    {
        return AdvantageType.None;
    }

    public bool IsImmuneToOpportunityAttack(RulesetCharacter myself, RulesetCharacter attacker, float distance)
    {
        return false;
    }
}

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
internal class FeatureDefinitionAttackDisadvantageAgainstNonSourceBuilder
    : FeatureDefinitionBuilder<FeatureDefinitionAttackDisadvantageAgainstNonSource,
        FeatureDefinitionAttackDisadvantageAgainstNonSourceBuilder>
{
    protected FeatureDefinitionAttackDisadvantageAgainstNonSourceBuilder(string name, Guid namespaceGuid) : base(
        name, namespaceGuid)
    {
    }

    [NotNull]
    internal FeatureDefinitionAttackDisadvantageAgainstNonSourceBuilder SetConditionName(string conditionName)
    {
        Definition.ConditionName = conditionName;
        return this;
    }
}
