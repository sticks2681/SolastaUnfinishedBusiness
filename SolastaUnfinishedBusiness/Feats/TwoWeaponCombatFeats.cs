﻿using System.Collections.Generic;
using JetBrains.Annotations;
using SolastaUnfinishedBusiness.Api.Infrastructure;
using SolastaUnfinishedBusiness.Builders;
using SolastaUnfinishedBusiness.Builders.Features;
using SolastaUnfinishedBusiness.CustomBehaviors;
using SolastaUnfinishedBusiness.CustomInterfaces;
using static RuleDefinitions;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper.FeatureDefinitionAdditionalActions;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper.FeatureDefinitionAttributeModifiers;

namespace SolastaUnfinishedBusiness.Feats;

internal static class TwoWeaponCombatFeats
{
    internal static void CreateFeats([NotNull] List<FeatDefinition> feats)
    {
        var featDualFlurry = BuildDualFlurry();
        var featDualWeaponDefense = BuildDualWeaponDefense();

        feats.AddRange(featDualFlurry, featDualWeaponDefense);

        GroupFeats.MakeGroup("FeatGroupTwoWeaponCombat", null,
            FeatDefinitions.Ambidextrous,
            FeatDefinitions.TwinBlade,
            featDualFlurry,
            featDualWeaponDefense);
    }

    private static FeatDefinition BuildDualFlurry()
    {
        var conditionDualFlurryApply = ConditionDefinitionBuilder
            .Create("ConditionDualFlurryApply")
            .SetGuiPresentationNoContent()
            .SetSpecialDuration(DurationType.Round, 0, false)
            .SetTurnOccurence(TurnOccurenceType.EndOfTurn)
            .SetPossessive()
            .SetSpecialInterruptions(ConditionInterruption.AnyBattleTurnEnd)
            .SetSilent(Silent.WhenAddedOrRemoved)
            .AddToDB();

        var conditionDualFlurryGrant = ConditionDefinitionBuilder
            .Create("ConditionDualFlurryGrant")
            .SetGuiPresentation(Category.Condition)
            .SetSpecialDuration(DurationType.Round, 0, false)
            .SetTurnOccurence(TurnOccurenceType.EndOfTurn)
            .SetPossessive()
            .SetSpecialInterruptions(ConditionInterruption.AnyBattleTurnEnd)
            .SetFeatures(
                FeatureDefinitionAdditionalActionBuilder
                    .Create(AdditionalActionSurgedMain, "AdditionalActionDualFlurry")
                    .SetGuiPresentationNoContent(true)
                    .SetActionType(ActionDefinitions.ActionType.Bonus)
                    .SetRestrictedActions(ActionDefinitions.Id.AttackOff)
                    .AddToDB())
            .AddToDB();

        return FeatDefinitionBuilder
            .Create("FeatDualFlurry")
            .SetGuiPresentation(Category.Feat)
            .SetFeatures(
                FeatureDefinitionBuilder
                    .Create("OnAttackDamageEffectFeatDualFlurry")
                    .SetGuiPresentation("FeatDualFlurry", Category.Feat)
                    .SetCustomSubFeatures(
                        new OnAttackHitEffectFeatDualFlurry(conditionDualFlurryGrant, conditionDualFlurryApply))
                    .AddToDB())
            .AddToDB();
    }

    private static FeatDefinition BuildDualWeaponDefense()
    {
        return FeatDefinitionBuilder
            .Create("FeatDualWeaponDefense")
            .SetGuiPresentation(Category.Feat)
            .SetFeatures(AttributeModifierSwiftBladeBladeDance)
            .SetAbilityScorePrerequisite(AttributeDefinitions.Dexterity, 13)
            .AddToDB();
    }

    private sealed class OnAttackHitEffectFeatDualFlurry : IAfterAttackEffect
    {
        private readonly ConditionDefinition _conditionDualFlurryApply;
        private readonly ConditionDefinition _conditionDualFlurryGrant;

        internal OnAttackHitEffectFeatDualFlurry(
            ConditionDefinition conditionDualFlurryGrant,
            ConditionDefinition conditionDualFlurryApply)
        {
            _conditionDualFlurryGrant = conditionDualFlurryGrant;
            _conditionDualFlurryApply = conditionDualFlurryApply;
        }

        public void AfterOnAttackHit(
            GameLocationCharacter attacker,
            GameLocationCharacter defender,
            RollOutcome outcome,
            CharacterActionParams actionParams,
            RulesetAttackMode attackMode,
            ActionModifier attackModifier)
        {
            if (attackMode == null || !ValidatorsWeapon.IsOneHanded(attackMode) ||
                outcome is RollOutcome.Failure or RollOutcome.CriticalFailure)
            {
                return;
            }

            var condition = attacker.RulesetCharacter.HasConditionOfType(_conditionDualFlurryApply.Name)
                ? _conditionDualFlurryGrant
                : _conditionDualFlurryApply;

            var rulesetCondition = RulesetCondition.CreateActiveCondition(
                attacker.RulesetCharacter.Guid,
                condition,
                DurationType.Round,
                0,
                TurnOccurenceType.EndOfTurn,
                attacker.RulesetCharacter.Guid,
                attacker.RulesetCharacter.CurrentFaction.Name);

            attacker.RulesetCharacter.AddConditionOfCategory(AttributeDefinitions.TagCombat, rulesetCondition);
        }
    }
}
