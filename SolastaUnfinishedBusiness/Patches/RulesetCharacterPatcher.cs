﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using JetBrains.Annotations;
using SolastaUnfinishedBusiness.Api;
using SolastaUnfinishedBusiness.Api.Extensions;
using SolastaUnfinishedBusiness.Api.Helpers;
using SolastaUnfinishedBusiness.Api.Infrastructure;
using SolastaUnfinishedBusiness.CustomBehaviors;
using SolastaUnfinishedBusiness.CustomInterfaces;
using SolastaUnfinishedBusiness.Models;

namespace SolastaUnfinishedBusiness.Patches;

public static class RulesetCharacterPatcher
{
    [HarmonyPatch(typeof(RulesetCharacter), "TerminateMatchingUniquePower")]
    [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
    public static class TerminateMatchingUniquePower_Patch
    {
        public static void Postfix(RulesetCharacter __instance, FeatureDefinitionPower powerDefinition)
        {
            //PATCH: terminates all matching spells and powers of same group
            GlobalUniqueEffects.TerminateMatchingUniquePower(__instance, powerDefinition);
        }
    }

    [HarmonyPatch(typeof(RulesetCharacter), "TerminateMatchingUniqueSpell")]
    [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
    public static class TerminateMatchingUniqueSpell_Patch
    {
        public static void Postfix(RulesetCharacter __instance, SpellDefinition spellDefinition)
        {
            //PATCH: terminates all matching spells and powers of same group
            GlobalUniqueEffects.TerminateMatchingUniqueSpell(__instance, spellDefinition);
        }
    }

    [HarmonyPatch(typeof(RulesetCharacter), "OnConditionAdded")]
    [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
    public static class OnConditionAdded_Patch
    {
        public static void Postfix(RulesetCharacter __instance, RulesetCondition activeCondition)
        {
            //PATCH: notifies custom condition features that condition is applied
            var definition = activeCondition.ConditionDefinition;
            definition.GetAllSubFeaturesOfType<ICustomConditionFeature>()
                .ForEach(c => c.ApplyFeature(__instance, activeCondition));

            definition.Features
                .SelectMany(f => f.GetAllSubFeaturesOfType<ICustomConditionFeature>())
                .ToList()
                .ForEach(c => c.ApplyFeature(__instance, activeCondition));
        }
    }

    [HarmonyPatch(typeof(RulesetCharacter), "OnConditionRemoved")]
    [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
    public static class OnConditionRemoved_Patch
    {
        public static void Postfix(RulesetCharacter __instance, RulesetCondition activeCondition)
        {
            //PATCH: notifies custom condition features that condition is removed 
            var definition = activeCondition.ConditionDefinition;
            definition.GetAllSubFeaturesOfType<ICustomConditionFeature>()
                .ForEach(c => c.RemoveFeature(__instance, activeCondition));

            definition.Features
                .SelectMany(f => f.GetAllSubFeaturesOfType<ICustomConditionFeature>())
                .ToList()
                .ForEach(c => c.RemoveFeature(__instance, activeCondition));
        }
    }

    [HarmonyPatch(typeof(RulesetCharacter), "AcknowledgeAttackedCharacter")]
    [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
    public static class AcknowledgeAttackedCharacter_Patch
    {
        public static void Postfix([CanBeNull] RulesetCharacter target)
        {
            //PATCH: Allows condition interruption after target was attacked
            target?.ProcessConditionsMatchingInterruption(
                (RuleDefinitions.ConditionInterruption)ExtraConditionInterruption.AfterWasAttacked);
        }
    }

    [HarmonyPatch(typeof(RulesetCharacter), "GetLowestSlotLevelAndRepertoireToCastSpell")]
    [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
    public static class GetLowestSlotLevelAndRepertoireToCastSpell_Patch
    {
        public static void Postfix(RulesetCharacter __instance,
            SpellDefinition spellDefinitionToCast,
            ref int __result,
            ref RulesetSpellRepertoire matchingRepertoire)
        {
            //BUGFIX: as of (v1.4.20) game doesn't consider cantrips gained from BonusCantrips feature
            //because of this issue Inventor can't use Light cantrip from quick-cast button on UI
            //this patch tries to find requested cantrip in repertoire's ExtraSpellsByTag
            if (spellDefinitionToCast.spellLevel != 0 || matchingRepertoire != null)
            {
                return;
            }

            foreach (var repertoire in __instance.SpellRepertoires
                         .Where(repertoire => repertoire.ExtraSpellsByTag
                             .Any(x => x.Value.Contains(spellDefinitionToCast))))
            {
                matchingRepertoire = repertoire;
                __result = 0;

                break;
            }
        }
    }

    [HarmonyPatch(typeof(RulesetCharacter), "IsComponentSomaticValid")]
    [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
    public static class IsComponentSomaticValid_Patch
    {
        public static void Postfix(
            RulesetCharacter __instance, ref bool __result, SpellDefinition spellDefinition, ref string failure)
        {
            if (__result)
            {
                return;
            }

            //PATCH: Allows valid Somatic component if specific material component is held in main hand or off hand slots
            // allows casting somatic spells with full hands if one of the hands holds material component for the spell
            ValidateIfMaterialInHand(__instance, spellDefinition, ref __result, ref failure);

            if (__result)
            {
                return;
            }

            //PATCH: Allows valid Somatic component if Inventor has infused item in main hand or off hand slots
            // allows casting somatic spells with full hands if one of the hands holds item infused by the caster
            ValidateIfInfusedInHand(__instance, spellDefinition, ref __result, ref failure);
        }

        //TODO: move to separate file
        private static void ValidateIfMaterialInHand(RulesetCharacter caster, SpellDefinition spellDefinition,
            ref bool result, ref string failure)
        {
            if (spellDefinition.MaterialComponentType != RuleDefinitions.MaterialComponentType.Specific)
            {
                return;
            }

            var materialTag = spellDefinition.SpecificMaterialComponentTag;
            var mainHand = caster.GetItemInSlot(EquipmentDefinitions.SlotTypeMainHand);
            var offHand = caster.GetItemInSlot(EquipmentDefinitions.SlotTypeOffHand);
            var tagsMap = new Dictionary<string, TagsDefinitions.Criticity>();

            mainHand?.FillTags(tagsMap, caster, true);
            offHand?.FillTags(tagsMap, caster, true);

            if (!tagsMap.ContainsKey(materialTag))
            {
                return;
            }

            result = true;
            failure = string.Empty;
        }

        //TODO: move to separate file
        private static void ValidateIfInfusedInHand(RulesetCharacter caster, SpellDefinition spell,
            ref bool result, ref string failure)
        {
            var mainHand = caster.GetItemInSlot(EquipmentDefinitions.SlotTypeMainHand);
            var offHand = caster.GetItemInSlot(EquipmentDefinitions.SlotTypeOffHand);

            if (!caster.HoldsMyInfusion(mainHand) && !caster.HoldsMyInfusion(offHand))
            {
                return;
            }

            result = true;
            failure = string.Empty;
        }
    }

    [HarmonyPatch(typeof(RulesetCharacter), "IsComponentMaterialValid")]
    [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
    public static class IsComponentMaterialValid_Patch
    {
        public static void Postfix(
            RulesetCharacter __instance,
            ref bool __result,
            SpellDefinition spellDefinition,
            ref string failure)
        {
            //PATCH: Allow spells to satisfy material components by using stack of equal or greater value
            StackedMaterialComponent.IsComponentMaterialValid(__instance, spellDefinition, ref failure, ref __result);

            if (__result)
            {
                return;
            }

            //PATCH: Allows spells to satisfy specific material components by actual active tags on an item that are not directly defined in ItemDefinition (like "Melee")
            //Used mostly for melee cantrips requiring melee weapon to cast
            ValidateSpecificComponentsByTags(__instance, spellDefinition, ref __result, ref failure);

            if (__result)
            {
                return;
            }

            //PATCH: Allows spells to satisfy mundane material components if Inventor has infused item equipped
            //Used mostly for melee cantrips requiring melee weapon to cast
            ValidateInfusedFocus(__instance, spellDefinition, ref __result, ref failure);
        }

        //TODO: move to separate file
        private static void ValidateSpecificComponentsByTags(
            RulesetCharacter caster,
            SpellDefinition spell,
            ref bool result,
            ref string failure)
        {
            if (spell.MaterialComponentType != RuleDefinitions.MaterialComponentType.Specific)
            {
                return;
            }

            var materialTag = spell.SpecificMaterialComponentTag;
            var requiredCost = spell.SpecificMaterialComponentCostGp;

            List<RulesetItem> items = new();
            caster.CharacterInventory.EnumerateAllItems(items);

            var tagsMap = new Dictionary<string, TagsDefinitions.Criticity>();

            foreach (var rulesetItem in items)
            {
                tagsMap.Clear();
                rulesetItem.FillTags(tagsMap, caster, true);

                var itemItemDefinition = rulesetItem.ItemDefinition;
                var costInGold = EquipmentDefinitions.GetApproximateCostInGold(itemItemDefinition.Costs);

                if (tagsMap.ContainsKey(materialTag) && costInGold >= requiredCost)
                {
                    continue;
                }

                result = true;
                failure = string.Empty;
            }
        }

        //TODO: move to separate file
        private static void ValidateInfusedFocus(
            RulesetCharacter caster,
            SpellDefinition spell,
            ref bool result,
            ref string failure)
        {
            if (spell.MaterialComponentType != RuleDefinitions.MaterialComponentType.Mundane)
            {
                return;
            }

            List<RulesetItem> items = new();
            caster.CharacterInventory.EnumerateAllItems(items);

            if (!items.Any(caster.HoldsMyInfusion))
            {
                return;
            }

            result = true;
            failure = string.Empty;
        }
    }

    [HarmonyPatch(typeof(RulesetCharacter), "SpendSpellMaterialComponentAsNeeded")]
    [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
    public static class SpendSpellMaterialComponentAsNeeded_Patch
    {
        public static bool Prefix(RulesetCharacter __instance, RulesetEffectSpell activeSpell)
        {
            //PATCH: Modify original code to spend enough of a stack to meet component cost
            return StackedMaterialComponent.SpendSpellMaterialComponentAsNeeded(__instance, activeSpell);
        }
    }

    [HarmonyPatch(typeof(RulesetCharacter), "IsValidReadyCantrip")]
    [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
    public static class IsValidReadyCantrip_Patch
    {
        public static void Postfix(RulesetCharacter __instance, ref bool __result,
            SpellDefinition cantrip)
        {
            //PATCH: Modifies validity of ready cantrip action to include attack cantrips even if they don't have damage forms
            //makes melee cantrips valid for ready action
            if (__result)
            {
                return;
            }

            var effect = PowerBundle.ModifySpellEffect(cantrip, __instance);
            var hasDamage = effect.HasFormOfType(EffectForm.EffectFormType.Damage);
            var hasAttack = cantrip.HasSubFeatureOfType<IPerformAttackAfterMagicEffectUse>();
            var notGadgets = effect.TargetFilteringMethod != RuleDefinitions.TargetFilteringMethod.GadgetOnly;
            var componentsValid = __instance.AreSpellComponentsValid(cantrip);

            __result = (hasDamage || hasAttack) && notGadgets && componentsValid;
        }
    }

    [HarmonyPatch(typeof(RulesetCharacter), "IsSubjectToAttackOfOpportunity")]
    [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
    public static class IsSubjectToAttackOfOpportunity_Patch
    {
        // ReSharper disable once RedundantAssignment
        public static void Postfix(
            RulesetCharacter __instance, ref bool __result, RulesetCharacter attacker, float distance)
        {
            //PATCH: allows custom exceptions for attack of opportunity triggering
            //Mostly for Sentinel feat
            __result = AttacksOfOpportunity.IsSubjectToAttackOfOpportunity(__instance, attacker, __result, distance);
        }
    }

    [HarmonyPatch(typeof(RulesetCharacter), "ComputeSaveDC")]
    [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
    // ReSharper disable once InconsistentNaming
    public static class ComputeSaveDC_Patch
    {
        public static void Postfix(RulesetCharacter __instance, ref int __result)
        {
            //PATCH: support for `IIncreaseSpellDC`
            //Adds extra modifiers to spell DC

            var features = __instance.GetSubFeaturesByType<IIncreaseSpellDc>();

            __result += features.Where(feature => feature != null).Sum(feature => feature.GetSpellModifier(__instance));
        }
    }

    //PATCH: ensures that the wildshape heroes or heroes under rage cannot cast any spells (Multiclass)
    [HarmonyPatch(typeof(RulesetCharacter), "CanCastSpells")]
    [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
    public static class CanCastSpells_Patch
    {
        public static void Postfix(RulesetCharacter __instance, ref bool __result)
        {
            // wildshape
            if (__instance.OriginalFormCharacter is RulesetCharacterHero hero && hero != __instance &&
                hero.classesAndLevels.TryGetValue(DatabaseHelper.CharacterClassDefinitions.Druid, out var level) &&
                level < 18)
            {
                __result = false;
            }

            // raging
            if (__instance.AllConditions
                .Any(x => x.ConditionDefinition == DatabaseHelper.ConditionDefinitions.ConditionRaging))
            {
                __result = false;
            }
        }
    }

    //PATCH: ensures that the wildshape hero has access to spell repertoires for calculating slot related features (Multiclass)
    [HarmonyPatch(typeof(RulesetCharacter), "SpellRepertoires", MethodType.Getter)]
    [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
    public static class SpellRepertoires_Getter_Patch
    {
        public static void Postfix(RulesetCharacter __instance, ref List<RulesetSpellRepertoire> __result)
        {
            if (__instance.OriginalFormCharacter is RulesetCharacterHero hero && hero != __instance)
            {
                __result = hero.SpellRepertoires;
            }
        }
    }

    //PATCH: ensures that original character sorcery point pool is in sync with substitute (Multiclass)
    [HarmonyPatch(typeof(RulesetCharacter), "CreateSorceryPoints")]
    [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
    public static class CreateSorceryPoints_Patch
    {
        public static void Postfix(RulesetCharacter __instance, int slotLevel, RulesetSpellRepertoire repertoire)
        {
            if (__instance.OriginalFormCharacter is RulesetCharacterHero hero && hero != __instance)
            {
                hero.CreateSorceryPoints(slotLevel, repertoire);
            }
        }
    }

    //PATCH: ensures that original character sorcery point pool is in sync with substitute (Multiclass)
    [HarmonyPatch(typeof(RulesetCharacter), "GainSorceryPoints")]
    [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
    public static class GainSorceryPoints_Patch
    {
        public static void Postfix(RulesetCharacter __instance, int sorceryPointsGain)
        {
            if (__instance.OriginalFormCharacter is RulesetCharacterHero hero && hero != __instance)
            {
                hero.GainSorceryPoints(sorceryPointsGain);
            }
        }
    }

    [HarmonyPatch(typeof(RulesetCharacter), "UsePower")]
    [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
    public static class UsePower_Patch
    {
        public static void Postfix(RulesetCharacter __instance, RulesetUsablePower usablePower)
        {
            if (__instance.OriginalFormCharacter is RulesetCharacterHero hero && hero != __instance)
            {
                // ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
                switch (usablePower.PowerDefinition.RechargeRate)
                {
                    //PATCH: ensures that original character rage pool is in sync with substitute (Multiclass)
                    case RuleDefinitions.RechargeRate.RagePoints:
                        hero.SpendRagePoint();

                        break;
                    //PATCH: ensures that original character ki pool is in sync with substitute (Multiclass)
                    case RuleDefinitions.RechargeRate.KiPoints:
                        hero.ForceKiPointConsumption(usablePower.PowerDefinition.CostPerUse);

                        break;
                }
            }

            //PATCH: update usage for power pools 
            __instance.UpdateUsageForPower(usablePower, usablePower.PowerDefinition.CostPerUse);

            //PATCH: support for counting uses of power in the UsedSpecialFeatures dictionary of the GameLocationCharacter
            CountPowerUseInSpecialFeatures.Count(__instance, usablePower);
        }
    }

    [HarmonyPatch(typeof(RulesetCharacter), "RefreshAttributeModifiersFromConditions")]
    [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
    public static class RefreshAttributeModifiersFromConditions_Patch
    {
        [NotNull]
        public static IEnumerable<CodeInstruction> Transpiler([NotNull] IEnumerable<CodeInstruction> instructions)
        {
            //PATCH: support for validation of attribute modifications applied through conditions
            //Replaces first `IsInst` operator with custom validator

            var validate = new Func<
                FeatureDefinition,
                RulesetCharacter,
                FeatureDefinition
            >(FeatureApplicationValidation.ValidateAttributeModifier).Method;

            return instructions.ReplaceCode(instruction => instruction.opcode == OpCodes.Isinst,
                -1, "RulesetCharacter.RefreshAttributeModifiersFromConditions",
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Call, validate));
        }
    }

    [HarmonyPatch(typeof(RulesetCharacter), "RollAttack")]
    [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
    public static class RollAttack_Patch
    {
        [NotNull]
        public static IEnumerable<CodeInstruction> Transpiler([NotNull] IEnumerable<CodeInstruction> instructions)
        {
            //PATCH: support for Mirror Image - replaces target's AC with 10 + DEX bonus if we targeting mirror image
            var currentValueMethod = typeof(RulesetAttribute).GetMethod("get_CurrentValue");
            var method =
                new Func<RulesetAttribute, RulesetActor, List<RuleDefinitions.TrendInfo>, int>(MirrorImageLogic.GetAC)
                    .Method;

            return instructions.ReplaceCall(currentValueMethod,
                1, "RulesetCharacter.RollAttack",
                new CodeInstruction(OpCodes.Ldarg_2),
                new CodeInstruction(OpCodes.Ldarg, 4),
                new CodeInstruction(OpCodes.Call, method));
        }
    }

    [HarmonyPatch(typeof(RulesetCharacter), "RollAttackMode")]
    [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
    public static class RollAttackMode_Patch
    {
        public static void Prefix(
            [NotNull] RulesetCharacter __instance,
            RulesetActor target,
            List<RuleDefinitions.TrendInfo> toHitTrends,
            bool testMode)
        {
            //PATCH: support for Mirror Image - checks if we have Mirror Images, rolls for it and adds proper to hit trend to mark this roll
            MirrorImageLogic.AttackRollPrefix(__instance, target, toHitTrends, testMode);
        }

        public static void Postfix(
            [NotNull] RulesetCharacter __instance,
            RulesetAttackMode attackMode,
            RulesetActor target,
            List<RuleDefinitions.TrendInfo> toHitTrends,
            ref RuleDefinitions.RollOutcome outcome,
            ref int successDelta,
            bool testMode)
        {
            //PATCH: support for Mirror Image - checks if we have Mirror Images, and makes attack miss target and removes 1 image if it was hit
            MirrorImageLogic.AttackRollPostfix(__instance, attackMode, target, toHitTrends,
                ref outcome,
                ref successDelta,
                testMode);
        }
    }

    [HarmonyPatch(typeof(RulesetCharacter), "RollMagicAttack")]
    [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
    public static class RollMagicAttack_Patch
    {
        public static void Prefix(
            [NotNull] RulesetCharacter __instance,
            RulesetActor target,
            List<RuleDefinitions.TrendInfo> toHitTrends,
            bool testMode)
        {
            //PATCH: support for Mirror Image - checks if we have Mirror Images, rolls for it and adds proper to hit trend to mark this roll
            MirrorImageLogic.AttackRollPrefix(__instance, target, toHitTrends, testMode);
        }

        public static void Postfix(
            [NotNull] RulesetCharacter __instance,
            RulesetActor target,
            List<RuleDefinitions.TrendInfo> toHitTrends,
            ref RuleDefinitions.RollOutcome outcome,
            ref int successDelta,
            bool testMode)
        {
            //PATCH: support for Mirror Image - checks if we have Mirror Images, and makes attack miss target and removes 1 image if it was hit
            MirrorImageLogic.AttackRollPostfix(__instance, null, target, toHitTrends, ref outcome, ref successDelta,
                testMode);
        }
    }

    //PATCH: IChangeAbilityCheck
    [HarmonyPatch(typeof(RulesetCharacter), "RollAbilityCheck")]
    [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
    public static class RollAbilityCheck_Patch
    {
        public static void Prefix(
            [NotNull] RulesetCharacter __instance,
            int baseBonus,
            string abilityScoreName,
            string proficiencyName,
            List<RuleDefinitions.TrendInfo> modifierTrends,
            List<RuleDefinitions.TrendInfo> advantageTrends,
            int rollModifier,
            ref int minRoll)
        {
            var features = __instance.GetSubFeaturesByType<IChangeAbilityCheck>();

            if (features.Count <= 0)
            {
                return;
            }

            var newMinRoll = features
                .Max(x => x.MinRoll(__instance, baseBonus, rollModifier, abilityScoreName, proficiencyName,
                    advantageTrends, modifierTrends));

            if (minRoll < newMinRoll)
            {
                minRoll = newMinRoll;
            }
        }
    }

    //PATCH: IChangeAbilityCheck
    [HarmonyPatch(typeof(RulesetCharacter), "ResolveContestCheck")]
    [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
    public static class ResolveContestCheck_Patch
    {
        public static int ExtendedRollDie(
            [NotNull] RulesetCharacter rulesetCharacter,
            RuleDefinitions.DieType dieType,
            RuleDefinitions.RollContext rollContext,
            bool isProficient,
            RuleDefinitions.AdvantageType advantageType,
            out int firstRoll,
            out int secondRoll,
            bool enumerateFeatures,
            bool canRerollDice,
            string skill,
            int baseBonus,
            int rollModifier,
            string abilityScoreName,
            string proficiencyName,
            List<RuleDefinitions.TrendInfo> advantageTrends,
            List<RuleDefinitions.TrendInfo> modifierTrends)
        {
            var features = rulesetCharacter.GetSubFeaturesByType<IChangeAbilityCheck>();
            var result = rulesetCharacter.RollDie(dieType, rollContext, isProficient, advantageType,
                out firstRoll, out secondRoll, enumerateFeatures, canRerollDice, skill);

            if (features.Count <= 0)
            {
                return result;
            }

            var newMinRoll = features
                .Max(x => x.MinRoll(rulesetCharacter, baseBonus, rollModifier, abilityScoreName, proficiencyName,
                    advantageTrends, modifierTrends));

            if (result < newMinRoll)
            {
                result = newMinRoll;
            }

            return result;
        }

        //
        // there are 2 calls to RollDie on this method
        // we replace them to allow us to compare the die result vs. the minRoll value from any IChangeAbilityCheck feature
        //
        public static IEnumerable<CodeInstruction> Transpiler([NotNull] IEnumerable<CodeInstruction> instructions)
        {
            var rollDieMethod = typeof(RulesetActor).GetMethod("RollDie");
            var extendedRollDieMethod = typeof(ResolveContestCheck_Patch).GetMethod("ExtendedRollDie");

            return instructions
                // first call to roll die checks the initiator
                .ReplaceCall(rollDieMethod,
                    1, "RulesetCharacter.ResolveContestCheck.RollDie1",
                    new CodeInstruction(OpCodes.Ldarg, 1), // baseBonus
                    new CodeInstruction(OpCodes.Ldarg, 2), // rollModifier
                    new CodeInstruction(OpCodes.Ldarg, 3), // abilityScoreName
                    new CodeInstruction(OpCodes.Ldarg, 4), // proficiencyName
                    new CodeInstruction(OpCodes.Ldarg, 5), // advantageTrends
                    new CodeInstruction(OpCodes.Ldarg, 6), // modifierTrends
                    new CodeInstruction(OpCodes.Call, extendedRollDieMethod))
                // second call to roll die checks the opponent
                .ReplaceCall(
                    rollDieMethod, // in fact this is 2nd occurence on game code but as we replaced on previous step we set to 1
                    1, "RulesetCharacter.ResolveContestCheck.RollDie2",
                    new CodeInstruction(OpCodes.Ldarg, 7), // opponentBaseBonus
                    new CodeInstruction(OpCodes.Ldarg, 8), // opponentRollModifier
                    new CodeInstruction(OpCodes.Ldarg, 9), // opponentAbilityScoreName
                    new CodeInstruction(OpCodes.Ldarg, 10), // opponentProficiencyName
                    new CodeInstruction(OpCodes.Ldarg, 11), // opponentAdvantageTrends
                    new CodeInstruction(OpCodes.Ldarg, 12), // opponentModifierTrends
                    new CodeInstruction(OpCodes.Call, extendedRollDieMethod));
        }
    }

    //PATCH: logic to correctly calculate spell slots under MC (Multiclass)
    [HarmonyPatch(typeof(RulesetCharacter), "RefreshSpellRepertoires")]
    [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
    public static class RefreshSpellRepertoires_Patch
    {
        [NotNull]
        public static IEnumerable<CodeInstruction> Transpiler([NotNull] IEnumerable<CodeInstruction> instructions)
        {
            var enumerate = new Action<
                RulesetCharacter,
                List<FeatureDefinition>,
                Dictionary<FeatureDefinition, RuleDefinitions.FeatureOrigin>
            >(CustomEnumerate).Method;

            //PATCH: make ISpellCastingAffinityProvider from dynamic item properties apply to repertoires
            return instructions.ReplaceEnumerateFeaturesToBrowse("ISpellCastingAffinityProvider",
                -1, "RulesetCharacter.RefreshSpellRepertoires",
                new CodeInstruction(OpCodes.Call, enumerate));
        }

        private static void CustomEnumerate(
            RulesetCharacter __instance,
            List<FeatureDefinition> featuresToBrowse,
            Dictionary<FeatureDefinition,
                RuleDefinitions.FeatureOrigin> featuresOrigin)
        {
            __instance.EnumerateFeaturesToBrowse<ISpellCastingAffinityProvider>(featuresToBrowse, featuresOrigin);

            if (__instance is not RulesetCharacterHero hero)
            {
                return;
            }

            foreach (var definition in hero.CharacterInventory.InventorySlotsByName
                         .Select(keyValuePair => keyValuePair.Value)
                         .Where(slot => slot.EquipedItem != null && !slot.Disabled && !slot.ConfigSlot)
                         .Select(slot => slot.EquipedItem)
                         .SelectMany(equipedItem => equipedItem.DynamicItemProperties
                             .Select(dynamicItemProperty => dynamicItemProperty.FeatureDefinition)
                             .Where(definition => definition != null && definition is ISpellCastingAffinityProvider)))
            {
                featuresToBrowse.Add(definition);

                if (featuresOrigin.ContainsKey(definition))
                {
                    continue;
                }

                featuresOrigin.Add(definition, new RuleDefinitions.FeatureOrigin(
                    RuleDefinitions.FeatureSourceType.CharacterFeature, definition.Name,
                    null, definition.ParseSpecialFeatureTags()));
            }
        }

        public static void Postfix(RulesetCharacter __instance)
        {
            if (__instance is not RulesetCharacterHero hero || !SharedSpellsContext.IsMulticaster(hero))
            {
                return;
            }

            var slots = new Dictionary<int, int>();

            // adds features slots
            foreach (var additionalSlot in hero.FeaturesToBrowse
                         .OfType<ISpellCastingAffinityProvider>()
                         .SelectMany(x => x.AdditionalSlots))
            {
                slots[additionalSlot.SlotLevel] = additionalSlot.SlotsNumber;
            }

            // adds spell slots
            var sharedCasterLevel = SharedSpellsContext.GetSharedCasterLevel(hero);
            var sharedSpellLevel = SharedSpellsContext.GetSharedSpellLevel(hero);

            for (var i = 1; i <= sharedSpellLevel; i++)
            {
                slots.TryAdd(i, 0);
                slots[i] += SharedSpellsContext.FullCastingSlots[sharedCasterLevel - 1].Slots[i - 1];
            }

            // adds warlock slots
            var warlockCasterLevel = SharedSpellsContext.GetWarlockCasterLevel(hero);
            var warlockSpellLevel = SharedSpellsContext.GetWarlockSpellLevel(hero);

            for (var i = 1; i <= warlockSpellLevel; i++)
            {
                slots.TryAdd(i, 0);
                slots[i] += SharedSpellsContext.WarlockCastingSlots[warlockCasterLevel - 1].Slots[i - 1];
            }

            // reassign slots back to repertoires except for race ones
            foreach (var spellRepertoire in hero.SpellRepertoires
                         .Where(x => x.SpellCastingFeature.SpellCastingOrigin
                             is FeatureDefinitionCastSpell.CastingOrigin.Class
                             or FeatureDefinitionCastSpell.CastingOrigin.Subclass))
            {
                spellRepertoire.spellsSlotCapacities = slots.DeepCopy();
                spellRepertoire.RepertoireRefreshed?.Invoke(spellRepertoire);
            }
        }
    }

    [HarmonyPatch(typeof(RulesetCharacter), "RechargePowersForTurnStart")]
    [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
    public static class RechargePowersForTurnStart_Patch
    {
        public static void Postfix(RulesetCharacter __instance)
        {
            //PATCH: support for powers that recharge on turn start
            foreach (var usablePower in __instance.UsablePowers)
            {
                if (usablePower.RemainingUses >= usablePower.MaxUses)
                {
                    continue;
                }

                var startOfTurnRecharge = usablePower.PowerDefinition.GetFirstSubFeatureOfType<IStartOfTurnRecharge>();

                if (startOfTurnRecharge == null)
                {
                    continue;
                }

                usablePower.Recharge();

                if (!startOfTurnRecharge.IsRechargeSilent && __instance.PowerRecharged != null)
                {
                    __instance.PowerRecharged(__instance, usablePower);
                }
            }
        }
    }

    [HarmonyPatch(typeof(RulesetCharacter), "RepayPowerUse")]
    [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
    public static class RepayPowerUse_Patch
    {
        public static void Postfix(RulesetCharacter __instance, RulesetUsablePower usablePower)
        {
            //PATCH: update usage for power pools
            __instance.UpdateUsageForPower(usablePower, -usablePower.PowerDefinition.CostPerUse);
        }
    }

    [HarmonyPatch(typeof(RulesetCharacter), "GrantPowers")]
    [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
    public static class GrantPowers_Patch
    {
        public static void Postfix(RulesetCharacter __instance)
        {
            //PATCH: update usage for power pools
            PowerBundle.RechargeLinkedPowers(__instance, RuleDefinitions.RestType.LongRest);
        }
    }

    [HarmonyPatch(typeof(RulesetCharacter), "ApplyRest")]
    [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
    public static class ApplyRest_Patch
    {
        public static void Postfix(
            RulesetCharacter __instance, RuleDefinitions.RestType restType, bool simulate)
        {
            //PATCH: update usage for power pools
            if (!simulate)
            {
                PowerBundle.RechargeLinkedPowers(__instance, restType);
            }

            // The player isn't recharging the shared pool features, just the pool.
            // Hide the features that use the pool from the UI.
            foreach (var feature in __instance.RecoveredFeatures.Where(f => f is IPowerSharedPool).ToArray())
            {
                __instance.RecoveredFeatures.Remove(feature);
            }
        }

        public static IEnumerable<CodeInstruction> Transpiler([NotNull] IEnumerable<CodeInstruction> instructions)
        {
            //PATCH: Makes powers that have their max usage extended by pool modifiers show up correctly during rest
            //replace calls to MaxUses getter to custom method that accounts for extended power usage
            var bind = typeof(RulesetUsablePower).GetMethod("get_MaxUses", BindingFlags.Public | BindingFlags.Instance);
            var maxUses =
                new Func<RulesetUsablePower, RulesetCharacter, int>(PowerBundle.GetMaxUsesForPool).Method;
            var restoreAllSpellSlotsMethod = typeof(RulesetSpellRepertoire).GetMethod("RestoreAllSpellSlots");
            var myRestoreAllSpellSlotsMethod =
                new Action<RulesetSpellRepertoire, RulesetCharacter, RuleDefinitions.RestType>(RestoreAllSpellSlots)
                    .Method;

            return instructions
                .ReplaceCalls(bind,
                    "RulesetCharacter.ApplyRest.MaxUses",
                    new CodeInstruction(OpCodes.Ldarg_0),
                    new CodeInstruction(OpCodes.Call, maxUses))
                .ReplaceCalls(restoreAllSpellSlotsMethod,
                    "RulesetCharacter.ApplyRest.RestoreAllSpellSlots",
                    new CodeInstruction(OpCodes.Ldarg_0),
                    new CodeInstruction(OpCodes.Ldarg_1),
                    new CodeInstruction(OpCodes.Call, myRestoreAllSpellSlotsMethod));
        }

        private static void RestoreAllSpellSlots(
            RulesetSpellRepertoire __instance,
            RulesetCharacter rulesetCharacter,
            RuleDefinitions.RestType restType)
        {
            if (restType == RuleDefinitions.RestType.LongRest
                || rulesetCharacter is not RulesetCharacterHero hero
                || !SharedSpellsContext.IsMulticaster(hero))
            {
                rulesetCharacter.RestoreAllSpellSlots();

                return;
            }

            var warlockSpellLevel = SharedSpellsContext.GetWarlockSpellLevel(hero);
            var warlockUsedSlots = SharedSpellsContext.GetWarlockUsedSlots(hero);

            foreach (var spellRepertoire in hero.SpellRepertoires
                         .Where(x => x.SpellCastingFeature.SpellCastingOrigin
                             is FeatureDefinitionCastSpell.CastingOrigin.Class
                             or FeatureDefinitionCastSpell.CastingOrigin.Subclass))
            {
                for (var i = SharedSpellsContext.PactMagicSlotsTab; i <= warlockSpellLevel; i++)
                {
                    if (spellRepertoire.usedSpellsSlots.ContainsKey(i))
                    {
                        spellRepertoire.usedSpellsSlots[i] -= warlockUsedSlots;
                    }
                }

                spellRepertoire.RepertoireRefreshed?.Invoke(spellRepertoire);
            }
        }
    }

    //PATCH: ensures auto prepared spells from subclass are considered on level up
    [HarmonyPatch(typeof(RulesetCharacter), "ComputeAutopreparedSpells")]
    [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
    public static class ComputeAutopreparedSpells_Patch
    {
        public static bool Prefix(
            [NotNull] RulesetCharacter __instance, [NotNull] RulesetSpellRepertoire spellRepertoire)
        {
            //BEGIN PATCH
            var spellcastingClass = spellRepertoire.SpellCastingClass;

            if (spellcastingClass == null && spellRepertoire.SpellCastingSubclass != null)
            {
                spellcastingClass = LevelUpContext.GetClassForSubclass(spellRepertoire.SpellCastingSubclass);
            }
            //END PATCH

            // this includes all the logic for the base function
            spellRepertoire.AutoPreparedSpells.Clear();
            __instance.EnumerateFeaturesToBrowse<FeatureDefinitionAutoPreparedSpells>(__instance.FeaturesToBrowse);
            var features = __instance.FeaturesToBrowse.OfType<FeatureDefinitionAutoPreparedSpells>();
            foreach (var autoPreparedSpells in features)
            {
                var matcher = autoPreparedSpells.GetFirstSubFeatureOfType<RepertoireValidForAutoPreparedFeature>();
                bool matches;

                if (matcher == null)
                {
                    matches = autoPreparedSpells.SpellcastingClass == spellRepertoire.spellCastingClass;
                }
                else
                {
                    matches = matcher(spellRepertoire, __instance);
                }

                if (!matches)
                {
                    continue;
                }

                var classLevel = __instance.GetSpellcastingLevel(spellRepertoire);

                foreach (var preparedSpellsGroup in autoPreparedSpells.AutoPreparedSpellsGroups
                             .Where(preparedSpellsGroup => preparedSpellsGroup.ClassLevel <= classLevel))
                {
                    spellRepertoire.AutoPreparedSpells.AddRange(preparedSpellsGroup.SpellsList);
                    spellRepertoire.AutoPreparedTag = autoPreparedSpells.AutoPreparedTag;
                }
            }

            return false;
        }
    }

    [HarmonyPatch(typeof(RulesetCharacter), "RollInitiative")]
    [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
    public static class RollInitiative_Patch
    {
        public static void Prefix(RulesetCharacter __instance, ref int forcedInitiative)
        {
            //PATCH: allows summons to have forced initiative of a summoner
            if (!__instance.HasSubFeatureOfType<ForceInitiativeToSummoner>())
            {
                return;
            }

            var summoner = __instance.GetMySummoner();

            if (summoner == null)
            {
                return;
            }

            forcedInitiative = summoner.lastInitiative;
        }
    }

    [HarmonyPatch(typeof(RulesetCharacter), "RefreshUsableDeviceFunctions")]
    [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
    public static class RefreshUsableDeviceFunctions_Patch
    {
        public static IEnumerable<CodeInstruction> Transpiler([NotNull] IEnumerable<CodeInstruction> instructions)
        {
            var isFunctionAvailable = typeof(RulesetItemDevice).GetMethod("IsFunctionAvailable");
            var customMethod = typeof(RefreshUsableDeviceFunctions_Patch).GetMethod("IsFunctionAvailable");

            return instructions.ReplaceCalls(isFunctionAvailable,
                "RulesetCharacter.RefreshUsableDeviceFunctions",
                new CodeInstruction(OpCodes.Call, customMethod));
        }

        [UsedImplicitly]
        public static bool IsFunctionAvailable(RulesetItemDevice device,
            RulesetDeviceFunction function,
            RulesetCharacter character,
            bool inCombat,
            bool usedMainSpell,
            bool usedBonusSpell,
            out string failureFlag)
        {
            //PATCH: allow PowerVisibilityModifier to make power device functions visible even if not valid
            //used to make Grenadier's grenade functions not be be hidden when you have not enough charges
            var result = device.IsFunctionAvailable(function, character, inCombat, usedMainSpell, usedBonusSpell,
                out failureFlag);

            if (result || function.DeviceFunctionDescription.type != DeviceFunctionDescription.FunctionType.Power)
            {
                return result;
            }

            var power = function.DeviceFunctionDescription.FeatureDefinitionPower;

            if (PowerVisibilityModifier.IsPowerHidden(character, power, ActionDefinitions.ActionType.Main)
                || !character.CanUsePower(power, false))
            {
                return false;
            }

            failureFlag = string.Empty;

            return true;
        }
    }

    [HarmonyPatch(typeof(RulesetCharacter), "ComputeSpeedAddition")]
    [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
    public static class ComputeSpeedAddition_Patch
    {
        public static void Postfix(RulesetCharacter __instance, IMovementAffinityProvider provider, ref int __result)
        {
            if (provider is not FeatureDefinition feature)
            {
                return;
            }

            var modifier = feature.GetFirstSubFeatureOfType<IModifyMovementSpeedAddition>();

            if (modifier != null)
            {
                __result += modifier.ModifySpeedAddition(__instance, provider);
            }
        }
    }
}
