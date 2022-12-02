﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using JetBrains.Annotations;
using SolastaUnfinishedBusiness.Api;
using SolastaUnfinishedBusiness.Api.Extensions;
using SolastaUnfinishedBusiness.Api.Helpers;
using SolastaUnfinishedBusiness.CustomBehaviors;
using SolastaUnfinishedBusiness.CustomDefinitions;
using SolastaUnfinishedBusiness.CustomInterfaces;
using SolastaUnfinishedBusiness.Feats;
using SolastaUnfinishedBusiness.Models;
using TA;

namespace SolastaUnfinishedBusiness.Patches;

public static class GameLocationBattleManagerPatcher
{
    [HarmonyPatch(typeof(GameLocationBattleManager), "CanCharacterUsePower")]
    [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
    public static class CanCharacterUsePower_Patch
    {
        public static void Postfix(
            GameLocationBattleManager __instance,
            ref bool __result,
            RulesetCharacter caster,
            RulesetUsablePower usablePower)
        {
            //PATCH: support for `IPowerUseValidity` when trying to react with power 
            if (!caster.CanUsePower(usablePower.PowerDefinition))
            {
                __result = false;
            }

            //PATCH: support for `IReactionAttackModeRestriction`
            if (__result)
            {
                __result = RestrictReactionAttackMode.CanCharacterReactWithPower(__instance, usablePower);
            }
        }
    }

    [HarmonyPatch(typeof(GameLocationBattleManager), "CanPerformReadiedActionOnCharacter")]
    [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
    public static class CanPerformReadiedActionOnCharacter_Patch
    {
        [NotNull]
        public static IEnumerable<CodeInstruction> Transpiler([NotNull] IEnumerable<CodeInstruction> instructions)
        {
            //PATCH: Makes only preferred cantrip valid if it is selected and forced
            var customBindMethod =
                new Func<List<SpellDefinition>, SpellDefinition, bool>(CustomReactionsContext.CheckAndModifyCantrips)
                    .Method;

            return instructions.ReplaceCall(
                "Contains",
                -1,
                "GameLocationBattleManager.CanPerformReadiedActionOnCharacter",
                new CodeInstruction(OpCodes.Call, customBindMethod));
        }
    }

    [HarmonyPatch(typeof(GameLocationBattleManager), "IsValidAttackForReadiedAction")]
    [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
    public static class IsValidAttackForReadiedAction_Patch
    {
        public static void Postfix(
            ref bool __result,
            BattleDefinitions.AttackEvaluationParams attackParams)
        {
            //PATCH: Checks if attack cantrip is valid to be cast as readied action on a target
            // Used to properly check if melee cantrip can hit target when used for readied action

            if (!DatabaseHelper.TryGetDefinition<SpellDefinition>(attackParams.effectName, out var cantrip))
            {
                return;
            }

            var canAttack = cantrip.GetFirstSubFeatureOfType<IPerformAttackAfterMagicEffectUse>()?.CanAttack;

            if (canAttack != null)
            {
                __result = canAttack(attackParams.attacker, attackParams.defender);
            }
        }
    }

    [HarmonyPatch(typeof(GameLocationBattleManager), "HandleCharacterMoveStart")]
    [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
    public static class HandleCharacterMoveStart_Patch
    {
        public static void Prefix(
            GameLocationCharacter mover,
            int3 destination)
        {
            //PATCH: support for Polearm Expert AoO
            //Stores character movements to be processed later
            AttacksOfOpportunity.ProcessOnCharacterMoveStart(mover, destination);
        }
    }

    [HarmonyPatch(typeof(GameLocationBattleManager), "HandleCharacterMoveEnd")]
    [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
    public static class HandleCharacterMoveEnd_Patch
    {
        public static IEnumerator Postfix(
            IEnumerator values,
            GameLocationBattleManager __instance,
            GameLocationCharacter mover)
        {
            //PATCH: support for Polearm Expert AoO
            //processes saved movement to trigger AoO when appropriate

            while (values.MoveNext())
            {
                yield return values.Current;
            }

            var extraEvents = AttacksOfOpportunity.ProcessOnCharacterMoveEnd(__instance, mover);

            while (extraEvents.MoveNext())
            {
                yield return extraEvents.Current;
            }
        }
    }

    [HarmonyPatch(typeof(GameLocationBattleManager), "PrepareBattleEnd")]
    [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
    public static class PrepareBattleEnd_Patch
    {
        public static void Prefix(GameLocationBattleManager __instance)
        {
            //PATCH: support for Polearm Expert AoO
            //clears movement cache on battle end

            AttacksOfOpportunity.CleanMovingCache();
        }
    }

    [HarmonyPatch(typeof(GameLocationBattleManager),
        nameof(GameLocationBattleManager.HandleBardicInspirationForAttack))]
    [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
    public static class HandleBardicInspirationForAttack_Patch
    {
        public static IEnumerator Postfix(
            IEnumerator values,
            GameLocationBattleManager __instance,
            CharacterAction action,
            GameLocationCharacter attacker,
            GameLocationCharacter target,
            ActionModifier attackModifier)
        {
            //PATCH: support for IAlterAttackOutcome
            while (values.MoveNext())
            {
                yield return values.Current;
            }

            if (action.BardicDieRoll > 0)
            {
                action.AttackSuccessDelta += action.BardicDieRoll;
            }

            foreach (var feature in attacker.RulesetActor.GetSubFeaturesByType<IAlterAttackOutcome>())
            {
                if (action.AttackRollOutcome != RuleDefinitions.RollOutcome.Failure || action.AttackSuccessDelta >= 0)
                {
                    break;
                }

                var extraEvents = feature.TryAlterAttackOutcome(__instance, action, attacker, target, attackModifier);

                while (extraEvents.MoveNext())
                {
                    yield return extraEvents.Current;
                }
            }
        }
    }

    [HarmonyPatch(typeof(GameLocationBattleManager), "HandleCharacterAttackFinished")]
    [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
    public static class HandleCharacterAttackFinished_Patch
    {
        public static IEnumerator Postfix(
            IEnumerator values,
            GameLocationBattleManager __instance,
            GameLocationCharacter attacker,
            GameLocationCharacter defender,
            RulesetAttackMode attackerAttackMode
        )
        {
            //PATCH: support for Sentinel feat - allows reaction attack on enemy attacking ally 
            while (values.MoveNext())
            {
                yield return values.Current;
            }

            var extraEvents =
                AttacksOfOpportunity.ProcessOnCharacterAttackFinished(__instance, attacker, defender,
                    attackerAttackMode);

            while (extraEvents.MoveNext())
            {
                yield return extraEvents.Current;
            }
        }
    }

    [HarmonyPatch(typeof(GameLocationBattleManager), "HandleCharacterAttackHitConfirmed")]
    [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
    public static class HandleCharacterAttackHitConfirmed_Patch
    {
        public static IEnumerator Postfix(
            IEnumerator values,
            GameLocationBattleManager __instance,
            GameLocationCharacter attacker,
            GameLocationCharacter defender,
            ActionModifier attackModifier,
            RulesetAttackMode attackMode,
            bool rangedAttack,
            RuleDefinitions.AdvantageType advantageType,
            List<EffectForm> actualEffectForms,
            RulesetEffect rulesetEffect,
            bool criticalHit,
            bool firstTarget)
        {
            //PATCH: support for `IDefenderBeforeAttackHitConfirmed`
            var character = defender.RulesetCharacter;

            if (character != null)
            {
                foreach (var feature in character.GetSubFeaturesByType<IDefenderBeforeAttackHitConfirmed>())
                {
                    var extra = feature.DefenderBeforeAttackHitConfirmed(__instance, attacker, defender, attackModifier,
                        attackMode,
                        rangedAttack, advantageType, actualEffectForms, rulesetEffect, criticalHit, firstTarget);

                    while (extra.MoveNext())
                    {
                        yield return extra.Current;
                    }
                }
            }

            while (values.MoveNext())
            {
                yield return values.Current;
            }
        }
    }

    [HarmonyPatch(typeof(GameLocationBattleManager), "HandleAttackerTriggeringPowerOnCharacterAttackHitConfirmed")]
    [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
    public static class HandleAttackerTriggeringPowerOnCharacterAttackHitConfirmed_Patch
    {
        public static void Prefix(
            GameLocationCharacter attacker,
            GameLocationCharacter defender,
            RulesetAttackMode attackMode)
        {
            //PATCH: support for `IReactionAttackModeRestriction`
            RestrictReactionAttackMode.ReactionContext = (attacker, defender, attackMode);
        }

        public static void Postfix(
            GameLocationCharacter attacker,
            GameLocationCharacter defender,
            RulesetAttackMode attackMode)
        {
            //PATCH: support for `IReactionAttackModeRestriction`
            RestrictReactionAttackMode.ReactionContext = (null, null, null);
        }
    }

    [HarmonyPatch(typeof(GameLocationBattleManager), "HandleDefenderBeforeDamageReceived")]
    [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
    public static class HandleDefenderBeforeDamageReceived_Patch
    {
        public static IEnumerator Postfix(
            IEnumerator values,
            GameLocationBattleManager __instance,
            GameLocationCharacter attacker,
            GameLocationCharacter defender,
            RulesetAttackMode attackMode,
            RulesetEffect rulesetEffect,
            ActionModifier attackModifier,
            bool rolledSavingThrow,
            bool saveOutcomeSuccess
        )
        {
            //PATCH: support for features that trigger when defender gets hit, like `FeatureDefinitionSpendSpellSlotToReduceDamage` 
            while (values.MoveNext())
            {
                yield return values.Current;
            }

            var defenderCharacter = defender.RulesetCharacter;

            // Don't proceed if the damaged character cannot act
            if (defenderCharacter == null || defenderCharacter.IsDeadOrDyingOrUnconscious) { yield break; }

            // Can the defender reduce incoming damage using their reaction?
            if (defender.GetActionTypeStatus(ActionDefinitions.ActionType.Reaction) !=
                ActionDefinitions.ActionStatus.Available)
            {
                yield break;
            }

            // Not actually used for react with spell slot feature, but may be useful for future features.
            // var selfDamage = attacker.RulesetCharacter == defenderCharacter;

            // Not actually used for react with spell slot feature, but may be useful for future features.
            // var canPerceiveAttacker = selfDamage
            //                           || defender.PerceivedFoes.Contains(attacker)
            //                           || defender.PerceivedAllies.Contains(attacker);

            var hasNonNegatedDamage = true;

            // In case of a ruleset effect, check that it shall apply damage forms, otherwise don't proceed (e.g. CounterSpell)
            if (rulesetEffect?.EffectDescription != null)
            {
                var canForceHalfDamage = false;
                if (rulesetEffect is RulesetEffectSpell activeSpell)
                {
                    canForceHalfDamage = attacker.RulesetCharacter.CanForceHalfDamage(activeSpell.SpellDefinition);
                }

                var effectDescription = rulesetEffect.EffectDescription;
                if (rolledSavingThrow)
                {
                    if (saveOutcomeSuccess)
                    {
                        hasNonNegatedDamage = effectDescription.HasNonNegatedDamageEffectForm(canForceHalfDamage);
                    }
                    else
                    {
                        hasNonNegatedDamage = effectDescription.FindFirstDamageForm() != null;
                    }
                }
                else
                {
                    hasNonNegatedDamage = effectDescription.FindFirstDamageForm() != null;
                }
            }

            if (!hasNonNegatedDamage)
            {
                yield break;
            }

            // Can I reduce the damage consuming slots? (i.e: Blade Dancer)
            //TODO: check if this properly works under MC
            var repertoire = defenderCharacter.GetClassSpellRepertoire();

            if (repertoire == null)
            {
                yield break;
            }

            foreach (var feature in defenderCharacter
                         .GetFeaturesByType<FeatureDefinitionReduceDamage>())
            {
                var atLeastOneSpellSlotAvailable = false;

                for (var spellLevel = 1;
                     spellLevel <= repertoire.MaxSpellLevelOfSpellCastingLevel;
                     spellLevel++)
                {
                    repertoire.GetSlotsNumber(spellLevel, out var remaining, out _);

                    if (remaining <= 0)
                    {
                        continue;
                    }

                    atLeastOneSpellSlotAvailable = true;
                    break;
                }

                if (!atLeastOneSpellSlotAvailable)
                {
                    yield break;
                }

                var actionService = ServiceRepository.GetService<IGameLocationActionService>();
                var previousReactionCount = actionService.PendingReactionRequestGroups.Count;
                var reactionParams = new CharacterActionParams(defender, ActionDefinitions.Id.SpendSpellSlot)
                {
                    IntParameter = 1, StringParameter = feature.NotificationTag, SpellRepertoire = repertoire
                };

                actionService.ReactToSpendSpellSlot(reactionParams);
                yield return __instance.WaitForReactions(defender, actionService, previousReactionCount);

                if (!reactionParams.ReactionValidated)
                {
                    continue;
                }

                var slot = reactionParams.IntParameter;
                var totalReducedDamage = feature.ReducedDamage * slot;

                attackModifier.damageRollReduction += totalReducedDamage;
                defenderCharacter.DamageReduced(defenderCharacter, feature, totalReducedDamage);
            }
        }
    }

    [HarmonyPatch(typeof(GameLocationBattleManager), "CanAttack")]
    [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
    public static class CanAttack_Patch
    {
        public static void Postfix(
            GameLocationBattleManager __instance,
            BattleDefinitions.AttackEvaluationParams attackParams,
            bool __result
        )
        {
            //PATCH: support for features removing ranged attack disadvantage
            RangedAttackInMeleeDisadvantageRemover.CheckToRemoveRangedDisadvantage(attackParams);

            //PATCH: add modifier or advantage/disadvantage for physical and spell attack
            ApplyCustomModifiers(attackParams, __result);

            //PATCH: Support elven precision feat
            // should come last as adv / dis make diff here
            ElvenAccuracyFeats.CheckElvenPrecisionContext(__result, attackParams.attacker.RulesetCharacter,
                attackParams.attackMode);
        }

        //TODO: move this somewhere else and maybe split?
        private static void ApplyCustomModifiers(BattleDefinitions.AttackEvaluationParams attackParams, bool __result)
        {
            if (!__result)
            {
                return;
            }

            var attacker = attackParams.attacker.RulesetCharacter;
            var defender = attackParams.defender.RulesetCharacter;

            if (attacker == null)
            {
                return;
            }

            switch (attackParams.attackProximity)
            {
                case BattleDefinitions.AttackProximity.PhysicalRange or BattleDefinitions.AttackProximity.PhysicalReach:
                    // handle physical attack roll
                    var attackModifiers = attacker.GetSubFeaturesByType<IOnComputeAttackModifier>();

                    foreach (var feature in attackModifiers)
                    {
                        feature.ComputeAttackModifier(attacker, defender, attackParams.attackMode,
                            ref attackParams.attackModifier);
                    }

                    break;

                case BattleDefinitions.AttackProximity.MagicRange or BattleDefinitions.AttackProximity.MagicReach:
                    // handle magic attack roll
                    var magicAttackModifiers = attacker.GetSubFeaturesByType<IIncreaseSpellAttackRoll>();

                    foreach (var feature in magicAttackModifiers)
                    {
                        var modifier = feature.GetSpellAttackRollModifier(attacker);
                        attackParams.attackModifier.attackRollModifier += modifier;
                        attackParams.attackModifier.attackToHitTrends.Add(new RuleDefinitions.TrendInfo(modifier,
                            feature.SourceType,
                            feature.SourceName, null));
                    }

                    break;
            }
        }
    }

    [HarmonyPatch(typeof(GameLocationBattleManager), "HandleAdditionalDamageOnCharacterAttackHitConfirmed")]
    [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
    public static class HandleAdditionalDamageOnCharacterAttackHitConfirmed_Patch
    {
        public static bool Prefix(
            GameLocationBattleManager __instance,
            out IEnumerator __result,
            GameLocationCharacter attacker,
            GameLocationCharacter defender,
            ActionModifier attackModifier,
            RulesetAttackMode attackMode,
            bool rangedAttack,
            RuleDefinitions.AdvantageType advantageType,
            List<EffectForm> actualEffectForms,
            RulesetEffect rulesetEffect,
            bool criticalHit,
            bool firstTarget)
        {
            //PATCH: Completely replace this method to support several features. Modified method based on TA provided sources.
            __result = GameLocationBattleManagerTweaks.HandleAdditionalDamageOnCharacterAttackHitConfirmed(__instance,
                attacker, defender, attackModifier, attackMode, rangedAttack, advantageType, actualEffectForms,
                rulesetEffect, criticalHit, firstTarget);

            return false;
        }
    }

    [HarmonyPatch(typeof(GameLocationBattleManager), "ComputeAndNotifyAdditionalDamage")]
    [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
    public static class ComputeAndNotifyAdditionalDamage_Patch
    {
        public static bool Prefix(
            GameLocationBattleManager __instance,
            GameLocationCharacter attacker,
            GameLocationCharacter defender,
            IAdditionalDamageProvider provider,
            List<EffectForm> actualEffectForms,
            CharacterActionParams reactionParams,
            RulesetAttackMode attackMode,
            bool criticalHit)
        {
            //PATCH: Completely replace this method to support several features. Modified method based on TA provided sources.
            GameLocationBattleManagerTweaks.ComputeAndNotifyAdditionalDamage(__instance, attacker, defender, provider,
                actualEffectForms, reactionParams, attackMode, criticalHit);

            return false;
        }
    }

    [HarmonyPatch(typeof(GameLocationBattleManager), "HandleTargetReducedToZeroHP")]
    [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
    public static class HandleTargetReducedToZeroHP_Patch
    {
        public static IEnumerator Postfix(
            IEnumerator values,
            GameLocationCharacter attacker,
            GameLocationCharacter downedCreature,
            RulesetAttackMode rulesetAttackMode,
            RulesetEffect activeEffect
        )
        {
            //PATCH: INotifyConditionRemoval
            var rulesetDownedCreature = downedCreature.RulesetCharacter;

            foreach (var rulesetCondition in rulesetDownedCreature.ConditionsByCategory
                         .SelectMany(keyValuePair => keyValuePair.Value))
            {
                if (rulesetCondition.ConditionDefinition == null)
                {
                    continue;
                }

                foreach (var notifyConditionRemoval in rulesetCondition.ConditionDefinition
                             .GetAllSubFeaturesOfType<INotifyConditionRemoval>())
                {
                    notifyConditionRemoval.BeforeDyingWithCondition(rulesetDownedCreature, rulesetCondition);
                }
            }

            //PATCH: Support for `ITargetReducedToZeroHP` feature
            while (values.MoveNext())
            {
                yield return values.Current;
            }

            var features = attacker.RulesetActor.GetSubFeaturesByType<ITargetReducedToZeroHp>();

            foreach (var extraEvents in features
                         .Select(x =>
                             x.HandleCharacterReducedToZeroHp(attacker, downedCreature, rulesetAttackMode,
                                 activeEffect)))
            {
                while (extraEvents.MoveNext())
                {
                    yield return extraEvents.Current;
                }
            }
        }
    }

    [HarmonyPatch(typeof(GameLocationBattleManager), "HandleCharacterMagicalAttackHitConfirmed")]
    [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
    public static class HandleCharacterMagicalAttackHitConfirmed_Patch
    {
        public static IEnumerator Postfix(
            IEnumerator values,
            GameLocationCharacter attacker,
            GameLocationCharacter defender,
            ActionModifier magicModifier,
            RulesetEffect rulesetEffect,
            List<EffectForm> actualEffectForms,
            bool firstTarget,
            bool criticalHit)
        {
            //PATCH: set critical strike global variable
            Global.CriticalHit = criticalHit;

            //PATCH: support for `IOnMagicalAttackDamageEffect`
            var features = attacker.RulesetActor.GetSubFeaturesByType<IOnMagicalAttackDamageEffect>();

            //call all before handlers
#if false
            foreach (var feature in features)
            {
                feature.BeforeOnMagicalAttackDamage(attacker, defender, magicModifier, rulesetEffect,
                    actualEffectForms, firstTarget, criticalHit);
            }
#endif

            while (values.MoveNext())
            {
                yield return values.Current;
            }

            //call all after handlers
            foreach (var feature in features)
            {
                feature.AfterOnMagicalAttackDamage(attacker, defender, magicModifier, rulesetEffect,
                    actualEffectForms, firstTarget, criticalHit);
            }

            Global.CriticalHit = false;
        }
    }

    [HarmonyPatch(typeof(GameLocationBattleManager), "HandleFailedSavingThrow")]
    [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
    internal static class HandleFailedSavingThrow_Patch
    {
        internal static bool QuickWitReactionValidated { get; set; }

        internal static IEnumerator Postfix(
            IEnumerator values,
            GameLocationBattleManager __instance,
            CharacterAction action,
            GameLocationCharacter attacker,
            GameLocationCharacter defender,
            ActionModifier saveModifier,
            bool hasHitVisual,
            bool hasBorrowedLuck
        )
        {
            //PATCH: allow source character of a condition to use power to augment failed save roll
            //used mainly for Inventor's `Quick Wit`
            while (values.MoveNext())
            {
                yield return values.Current;
            }

            var saveOutcome = action.SaveOutcome;

            if (!IsFailed(saveOutcome))
            {
                yield break;
            }

            var rulesetDefender = defender.RulesetCharacter;

            if (rulesetDefender == null)
            {
                yield break;
            }

            var actionService = ServiceRepository.GetService<IGameLocationActionService>();
            var rulesService = ServiceRepository.GetService<IRulesetImplementationService>();

            var allConditions = new List<RulesetCondition>();

            rulesetDefender.GetAllConditions(allConditions);

            foreach (var condition in allConditions)
            {
                var feature = condition.ConditionDefinition
                    .GetFirstSubFeatureOfType<ConditionSourceCanUsePowerToImproveFailedSaveRoll>();

                if (feature == null)
                {
                    continue;
                }

                if (!RulesetEntity.TryGetEntity<RulesetCharacter>(condition.SourceGuid, out var helper))
                {
                    continue;
                }

                var locHelper = GameLocationCharacter.GetFromActor(helper);

                if (locHelper == null)
                {
                    continue;
                }

                if (!feature.ShouldTrigger(action, attacker, defender, locHelper, saveModifier, hasHitVisual,
                        hasBorrowedLuck, saveOutcome, action.saveOutcomeDelta))
                {
                    continue;
                }

                var power = feature.Power;

                if (!helper.CanUsePower(power))
                {
                    continue;
                }

                var usablePower = UsablePowersProvider.Get(power, helper);

                var reactionParams = new CharacterActionParams(locHelper, ActionDefinitions.Id.SpendPower)
                {
                    StringParameter = feature.ReactionName,
                    StringParameter2 = feature.FormatReactionDescription(action, attacker, defender, locHelper,
                        saveModifier, hasHitVisual, hasBorrowedLuck, saveOutcome, action.saveOutcomeDelta),
                    RulesetEffect = rulesService.InstantiateEffectPower(helper, usablePower, false)
                };

                var count = actionService.PendingReactionRequestGroups.Count;

                actionService.ReactToSpendPower(reactionParams);

                yield return __instance.WaitForReactions(locHelper, actionService, count);

                if (reactionParams.ReactionValidated)
                {
                    GameConsoleHelper.LogCharacterUsedPower(helper, power, indent: true);
                    rulesetDefender.UsePower(usablePower);

                    action.RolledSaveThrow = feature.TryModifyRoll(action, attacker, defender, locHelper, saveModifier,
                        hasHitVisual, hasBorrowedLuck, ref saveOutcome, ref action.saveOutcomeDelta);
                    action.SaveOutcome = saveOutcome;
                }

                reactionParams.RulesetEffect.Terminate(true);

                if (!IsFailed(saveOutcome))
                {
                    yield break;
                }
            }
        }

        private static bool IsFailed(RuleDefinitions.RollOutcome outcome)
        {
            return outcome is RuleDefinitions.RollOutcome.Failure or RuleDefinitions.RollOutcome.CriticalFailure;
        }
    }
}
