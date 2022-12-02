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
using SolastaUnfinishedBusiness.CustomBehaviors;
using SolastaUnfinishedBusiness.CustomInterfaces;
using SolastaUnfinishedBusiness.Feats;
using SolastaUnfinishedBusiness.Subclasses;
using TA;
using UnityEngine;
using static ConsoleStyleDuplet;
using static FeatureDefinitionAttributeModifier;

namespace SolastaUnfinishedBusiness.Patches;

public static class RulesetActorPatcher
{
    [HarmonyPatch(typeof(RulesetActor), "AddConditionOfCategory")]
    [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
    public static class AddConditionOfCategory_Patch
    {
        public static void Prefix(RulesetActor __instance,
            ref string category,
            RulesetCondition newCondition,
            bool refresh,
            bool registerCondition)
        {
            //PATCH: allow conditions to force specific category
            IForceConditionCategory feature;
            if (newCondition.conditionDefinition == null)
            {
                return;
            }

            feature = newCondition.conditionDefinition.GetFirstSubFeatureOfType<IForceConditionCategory>();

            if (feature == null)
            {
                return;
            }

            category = feature.GetForcedCategory(__instance, newCondition, category);
        }
    }

    [HarmonyPatch(typeof(RulesetActor), "InflictCondition")]
    [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
    public static class InflictCondition_Patch
    {
        public static void Prefix(RulesetActor __instance,
            string conditionDefinitionName,
            RuleDefinitions.DurationType durationType,
            int durationParameter,
            RuleDefinitions.TurnOccurenceType endOccurence,
            string tag,
            ulong sourceGuid,
            string sourceFaction,
            int effectLevel,
            string effectDefinitionName,
            ref int sourceAmount,
            int sourceAbilityBonus,
            int sourceProficiencyBonus,
            RuleDefinitions.DieType bardicInspirationDie)
        {
            //PATCH: Implements `ExtraOriginOfAmount`
            var sourceCharacter = EffectHelpers.GetCharacterByGuid(sourceGuid);

            if (sourceCharacter == null)
            {
                return;
            }

            if (!DatabaseHelper.TryGetDefinition<ConditionDefinition>(conditionDefinitionName, out var addedCondition))
            {
                return;
            }

            // Find a better place to put this in?
            var source = addedCondition.AdditionalDamageType;
            RulesetAttribute attribute;

            switch (addedCondition.AmountOrigin)
            {
                case (ConditionDefinition.OriginOfAmount)ExtraOriginOfAmount.SourceProficiencyBonus:
                    sourceAmount =
                        sourceCharacter.TryGetAttributeValue(AttributeDefinitions.ProficiencyBonus);
                    break;

                case (ConditionDefinition.OriginOfAmount)ExtraOriginOfAmount.SourceProficiencyBonusNegative:
                    sourceAmount =
                        -sourceCharacter.TryGetAttributeValue(AttributeDefinitions.ProficiencyBonus);
                    break;

                case (ConditionDefinition.OriginOfAmount)ExtraOriginOfAmount.SourceCharacterLevel:
                    sourceAmount =
                        sourceCharacter.TryGetAttributeValue(AttributeDefinitions.CharacterLevel);
                    break;

                case (ConditionDefinition.OriginOfAmount)ExtraOriginOfAmount.SourceClassLevel:
                    sourceAmount = sourceCharacter.GetClassLevel(source);
                    break;
                case (ConditionDefinition.OriginOfAmount)ExtraOriginOfAmount.SourceAbilityBonus:
                    if (sourceCharacter.TryGetAttribute(source, out attribute))
                    {
                        sourceAmount = AttributeDefinitions.ComputeAbilityScoreModifier(attribute.CurrentValue);
                    }

                    break;
                case (ConditionDefinition.OriginOfAmount)ExtraOriginOfAmount.SourceCopyAttributeFromSummoner:
                    if (sourceCharacter.TryGetAttribute(source, out attribute))
                    {
                        __instance.Attributes.Add(source, attribute);
                    }

                    break;
                case (ConditionDefinition.OriginOfAmount)ExtraOriginOfAmount.SourceProficiencyAndAbilityBonus:
                    sourceAmount =
                        sourceCharacter.TryGetAttributeValue(AttributeDefinitions.ProficiencyBonus);

                    if (sourceCharacter.TryGetAttribute(source, out attribute))
                    {
                        sourceAmount += AttributeDefinitions.ComputeAbilityScoreModifier(attribute.CurrentValue);
                    }

                    break;
            }
        }
    }

    [HarmonyPatch(typeof(RulesetActor), "RemoveCondition")]
    [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
    public static class RemoveCondition_Patch
    {
        public static void Postfix(RulesetActor __instance, RulesetCondition rulesetCondition)
        {
            //PATCH: INotifyConditionRemoval
            if (rulesetCondition == null || rulesetCondition.ConditionDefinition == null)
            {
                return;
            }

            foreach (var notifyConditionRemoval in rulesetCondition.ConditionDefinition
                         .GetAllSubFeaturesOfType<INotifyConditionRemoval>())
            {
                notifyConditionRemoval.AfterConditionRemoved(__instance, rulesetCondition);
            }
        }
    }

    [HarmonyPatch(typeof(RulesetActor), "ProcessConditionsMatchingOccurenceType")]
    [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
    public static class ProcessConditionsMatchingOccurenceType_Patch
    {
        public static void Postfix(RulesetActor __instance, RuleDefinitions.TurnOccurenceType occurenceType)
        {
            //PATCH: support for `IConditionRemovedOnSourceTurnStart` - removes appropriately marked conditions
            ConditionRemovedOnSourceTurnStartPatch.RemoveConditionIfNeeded(__instance, occurenceType);
        }
    }

#if false
    [HarmonyPatch(typeof(RulesetActor), "ProcessConditionsMatchingInterruption")]
    [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
    public static class ProcessConditionsMatchingInterruption_Patch
    {
        public static void Prefix(RulesetActor __instance,
            RuleDefinitions.ConditionInterruption interruption,
            int amount)
        {
            //PATCH: support for 'ProcessConditionInterruptionHandler'
            foreach (var handler in __instance.GetSubFeaturesByType<ProcessConditionInterruptionHandler>())
            {
                handler(__instance, interruption, amount);
            }
        }
    }
#endif

    [HarmonyPatch(typeof(RulesetActor), "ModulateSustainedDamage")]
    [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
    public static class ModulateSustainedDamage_Patch
    {
        public static IEnumerable<CodeInstruction> Transpiler([NotNull] IEnumerable<CodeInstruction> instructions)
        {
            //PATCH: add `IDamageAffinityProvider` from dynamic item properties
            //fixes game not applying damage reductions from dynamic item properties
            //used for Inventor's Resistant Armor infusions

            var myEnumerate = new Action<
                RulesetActor,
                List<FeatureDefinition>,
                Dictionary<FeatureDefinition, RuleDefinitions.FeatureOrigin>
            >(MyEnumerate).Method;

            return instructions.ReplaceEnumerateFeaturesToBrowse("IDamageAffinityProvider",
                -1, "RulesetActor.ModulateSustainedDamage",
                new CodeInstruction(OpCodes.Call, myEnumerate));
        }

        private static void MyEnumerate(
            RulesetActor actor,
            List<FeatureDefinition> featuresToBrowse,
            Dictionary<FeatureDefinition, RuleDefinitions.FeatureOrigin> featuresOrigin)
        {
            actor.EnumerateFeaturesToBrowse<IDamageAffinityProvider>(actor.featuresToBrowse, featuresOrigin);

            if (actor is not RulesetCharacterHero hero)
            {
                return;
            }

            foreach (var keyValuePair in hero.CharacterInventory.InventorySlotsByName)
            {
                var slot = keyValuePair.Value;

                if (slot.EquipedItem == null || slot.Disabled || slot.ConfigSlot)
                {
                    continue;
                }

                var equipedItem = slot.EquipedItem;

                foreach (var dynamicItemProperty in equipedItem.DynamicItemProperties)
                {
                    var definition = dynamicItemProperty.FeatureDefinition;
                    if (definition == null || definition is not IDamageAffinityProvider)
                    {
                        continue;
                    }

                    featuresToBrowse.Add(definition);
                }
            }
        }
    }

    [HarmonyPatch(typeof(RulesetActor), "RollDie")]
    [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
    public static class RollDie_Patch
    {
        public static IEnumerable<CodeInstruction> Transpiler([NotNull] IEnumerable<CodeInstruction> instructions)
        {
            var rollDieMethod = typeof(RuleDefinitions).GetMethod("RollDie", BindingFlags.Public | BindingFlags.Static);
            var myRollDieMethod = typeof(RollDie_Patch).GetMethod("RollDie");

            return instructions.ReplaceCalls(rollDieMethod, "RulesetActor.RollDie",
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Ldarg_2),
                new CodeInstruction(OpCodes.Call, myRollDieMethod));
        }

        public static int RollDie(
            RuleDefinitions.DieType dieType,
            RuleDefinitions.AdvantageType advantageType,
            out int firstRoll,
            out int secondRoll,
            float rollAlterationScore,
            RulesetActor actor,
            RuleDefinitions.RollContext rollContext
        )
        {
            if (rollContext == RuleDefinitions.RollContext.AttackRoll &&
                advantageType == RuleDefinitions.AdvantageType.Advantage && IsElvenPrecisionContextQualified(actor))
            {
                return Roll3DicesAndKeepBest(actor.Name, dieType, out firstRoll, out secondRoll, rollAlterationScore);
            }

            return RuleDefinitions.RollDie(dieType, advantageType, out firstRoll, out secondRoll,
                rollAlterationScore);
        }

        private static bool IsElvenPrecisionContextQualified(RulesetActor actor)
        {
            var character = GameLocationCharacter.GetFromActor(actor);

            return character.RulesetCharacter is RulesetCharacterHero hero && hero.TrainedFeats
                .Where(feat => feat.Name.Contains(ElvenAccuracyFeats.ElvenAccuracyTag))
                .Select(feat => feat.GetFirstSubFeatureOfType<ElvenPrecisionContext>())
                .Where(context => context != null)
                .Any(sub => sub.Qualified);
        }

        private static int Roll3DicesAndKeepBest(
            string roller,
            RuleDefinitions.DieType diceType,
            out int firstRoll,
            out int secondRoll,
            float rollAlterationScore
        )
        {
            var karmic = rollAlterationScore != 0.0;

            int DoRoll()
            {
                return karmic
                    ? RuleDefinitions.RollKarmicDie(diceType, rollAlterationScore)
                    : 1 + DeterministicRandom.Range(0, RuleDefinitions.DiceMaxValue[(int)diceType]);
            }

            var roll1 = DoRoll();
            var roll2 = DoRoll();
            var roll3 = DoRoll();

            var kept = Math.Max(roll1, roll2);
            var replaced = Math.Min(roll1, roll2);

            var entry = new GameConsoleEntry("Feedback/&ElvenAccuracyTriggered",
                Gui.Game.GameConsole.consoleTableDefinition);

            entry.AddParameter(ParameterType.Player, roller);
            entry.AddParameter(ParameterType.AttackSpellPower, "Tooltip/&FeatElvenAccuracyBaseTitle",
                tooltipContent: "Tooltip/&FeatElvenAccuracyBaseDescription");
            entry.AddParameter(ParameterType.AbilityInfo, kept.ToString());
            entry.AddParameter(ParameterType.AbilityInfo, replaced.ToString());
            entry.AddParameter(ParameterType.AbilityInfo, roll3.ToString());

            Gui.Game.GameConsole.AddEntry(entry);

            firstRoll = kept;
            secondRoll = roll3;

            return Mathf.Max(firstRoll, secondRoll);
        }

        // TODO: make this more generic
        public static void Prefix(RulesetActor __instance, RuleDefinitions.RollContext rollContext,
            ref bool enumerateFeatures, ref bool canRerollDice)
        {
            //PATCH: support for `RoguishRaven` Rogue subclass
            if (!__instance.HasSubFeatureOfType<RoguishRaven.RavenRerollAnyDamageDieMarker>() ||
                rollContext != RuleDefinitions.RollContext.AttackDamageValueRoll)
            {
                return;
            }

            enumerateFeatures = true;
            canRerollDice = true;
        }
    }

    //PATCH: uses class level instead of character level on attributes calculation (Multiclass)
    [HarmonyPatch(typeof(RulesetActor), "RefreshAttributes")]
    [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
    public static class RefreshAttributes_Patch
    {
        // private static readonly Regex ClassPattern = new($"{AttributeDefinitions.TagClass}(.*)\\d+");

        private static void RefreshClassModifiers(RulesetActor actor)
        {
            var hero = actor as RulesetCharacterHero;

            if (hero == null && actor is RulesetCharacterMonster monster)
            {
                hero = monster.OriginalFormCharacter as RulesetCharacterHero;
            }

            if (hero == null)
            {
                return;
            }

            foreach (var attribute in actor.Attributes)
            {
                foreach (var modifier in attribute.Value.ActiveModifiers
                             .Where(x => x.Operation
                                 is AttributeModifierOperation.MultiplyByClassLevel
                                 or AttributeModifierOperation.MultiplyByClassLevelBeforeAdditions))
                {
                    var level = attribute.Key switch
                    {
                        AttributeDefinitions.HealingPool =>
                            hero.GetClassLevel(DatabaseHelper.CharacterClassDefinitions.Paladin),
                        AttributeDefinitions.KiPoints =>
                            hero.GetClassLevel(DatabaseHelper.CharacterClassDefinitions.Monk),
                        AttributeDefinitions.SorceryPoints =>
                            hero.GetClassLevel(DatabaseHelper.CharacterClassDefinitions.Sorcerer),
                        _ => 0
                    };

                    if (level > 0)
                    {
                        modifier.Value = level;
                    }
                }
            }
        }

        [NotNull]
        public static IEnumerable<CodeInstruction> Transpiler([NotNull] IEnumerable<CodeInstruction> instructions)
        {
            // needed for sorcery points, healing pools, ki points to be of proper sizes when multiclass
            // adds custom method right before the end that recalculates modifier values specifically for class-level modifiers
            var refreshAttributes = typeof(RulesetEntity).GetMethod("RefreshAttributes");
            var custom = new Action<RulesetActor>(RefreshClassModifiers).Method;

            return instructions.ReplaceCalls(refreshAttributes, "RulesetActor.RefreshAttributes",
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Call, custom),
                new CodeInstruction(OpCodes.Call, refreshAttributes)); // checked for Call vs CallVirtual
        }
    }
}
