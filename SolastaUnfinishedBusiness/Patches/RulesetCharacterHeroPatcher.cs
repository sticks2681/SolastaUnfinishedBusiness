﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using JetBrains.Annotations;
using SolastaUnfinishedBusiness.Api.Extensions;
using SolastaUnfinishedBusiness.Api.Helpers;
using SolastaUnfinishedBusiness.Api.Infrastructure;
using SolastaUnfinishedBusiness.CustomBehaviors;
using SolastaUnfinishedBusiness.CustomInterfaces;
using SolastaUnfinishedBusiness.Models;

namespace SolastaUnfinishedBusiness.Patches;

public static class RulesetCharacterHeroPatcher
{
    [HarmonyPatch(typeof(RulesetCharacterHero), "RefreshArmorClass")]
    [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
    public static class RefreshArmorClass_Patch
    {
        public static IEnumerable<CodeInstruction> Transpiler([NotNull] IEnumerable<CodeInstruction> instructions)
        {
            //PATCH: pass condition amount to `RefreshArmorClassInFeatures` - allows AC modification by Condition Amount for heroes
            //in vanilla this only works on monsters, but for heroes only default 0 is passed

            var codes = instructions.ToList();
            object rulesetConditionVar = null;
            var found = false;

            var getAmount = typeof(RulesetCondition).GetMethod("get_Amount");

            for (var index = 0; index < codes.Count; index++)
            {
                var code = codes[index];

                if (found)
                {
                    continue;
                }

                if (rulesetConditionVar == null && code.opcode == OpCodes.Ldloc_S &&
                    $"{code.operand}".Contains("RulesetCondition"))
                {
                    rulesetConditionVar = code.operand;
                    continue;
                }

                if (rulesetConditionVar != null && code.opcode == OpCodes.Ldc_I4_0)
                {
                    codes[index] = new CodeInstruction(OpCodes.Ldloc_S, rulesetConditionVar);
                    codes.Insert(index + 1, new CodeInstruction(OpCodes.Callvirt, getAmount));
                    found = true;
                    continue;
                }

                if (rulesetConditionVar != null && $"{code.operand}".Contains("RefreshArmorClassInFeatures"))
                {
                    //abort if we reached refresh call after reaching RulesetCondition local var, but haven't found place of insertion
                    //this means code has changed and we need to look at it - maybe this patch is not needed anymore in this case
                    break;
                }
            }

            if (!found)
            {
                Main.Error("Couldn't patch RulesetCharacterHero.RefreshArmorClass");
            }

            return codes;
        }
    }

    [HarmonyPatch(typeof(RulesetCharacterHero), "FindClassHoldingFeature")]
    [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
    public static class FindClassHoldingFeature_Patch
    {
        public static void Postfix(
            RulesetCharacterHero __instance,
            FeatureDefinition featureDefinition,
            ref CharacterClassDefinition __result)
        {
            //PATCH: replaces feature holding class with one provided by custom interface
            //used for features that are not granted directly through class but need to scale with class levels
            var classHolder = featureDefinition.GetFirstSubFeatureOfType<IClassHoldingFeature>()?.Class;

            if (classHolder == null)
            {
                return;
            }

            // Only override if the character actually has levels in the class, to prevent errors
            if (__instance.ClassesAndLevels.TryGetValue(classHolder, out var levelsInClass) && levelsInClass > 0)
            {
                __result = classHolder;
            }
        }
    }

    [HarmonyPatch(typeof(RulesetCharacterHero), "CanCastInvocation")]
    [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
    public static class CanCastInvocation_Patch
    {
        public static bool Prefix(RulesetCharacterHero __instance, ref bool __result, RulesetInvocation invocation)
        {
            //PATCH: make sure we can't cast hidden invocations, so they will be hidden
            if (invocation.invocationDefinition.HasSubFeatureOfType<Hidden>())
            {
                __result = false;

                return false;
            }

            //PATCH: report invocation as cast-able if this is a power we can use
            var power = invocation.invocationDefinition.GetPower();

            if (!power)
            {
                return true;
            }

            __result = __instance.CanUsePower(power);
            return false;
        }
    }

    [HarmonyPatch(typeof(RulesetCharacterHero), "GrantInvocations")]
    [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
    public static class GrantInvocations_Patch
    {
        public static void Postfix(RulesetCharacterHero __instance)
        {
            foreach (var invocation in __instance.Invocations)
            {
                //PATCH: mark some invocation as disabled by default
                invocation.active = !invocation.invocationDefinition.HasSubFeatureOfType<InvocationDisabledByDefault>();

                //PATCH: allow customized repertoire matching for invocation
                var matcher = invocation.InvocationDefinition
                    .GetFirstSubFeatureOfType<RepertoireValidForAutoPreparedFeature>();

                if (matcher == null)
                {
                    continue;
                }

                foreach (var repertoire in __instance.SpellRepertoires)
                {
                    if (!matcher(repertoire, __instance))
                    {
                        continue;
                    }

                    invocation.invocationRepertoire = repertoire;
                    invocation.spellCastingFeature = repertoire.spellCastingFeature;
                    break;
                }
            }
        }
    }

    [HarmonyPatch(typeof(RulesetCharacterHero), "RefreshAttackMode")]
    [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
    public static class RefreshAttackMode_Patch
    {
        public static IEnumerable<CodeInstruction> Transpiler([NotNull] IEnumerable<CodeInstruction> instructions)
        {
            //PATCH: enables `AttackRollModifierMethod` support for hero attack modification
            //default implementation just gets flat value and ignores other methods
            //replaces call to `AttackRollModifier` getter with ciustom method that returns proper values
            var method = typeof(IAttackModificationProvider).GetMethod("get_AttackRollModifier");
            var custom = new Func<IAttackModificationProvider, RulesetCharacterHero, int>(GetAttackRollModifier)
                .Method;

            return instructions.ReplaceCalls(method,
                "RulesetCharacterHero.RefreshAttackMode",
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Call, custom));
        }

        private static int GetAttackRollModifier(IAttackModificationProvider provider, RulesetCharacterHero hero)
        {
            var num = provider.AttackRollModifier;

            if (provider.AttackRollModifierMethod == RuleDefinitions.AttackModifierMethod.SourceConditionAmount)
            {
                num = hero.FindFirstConditionHoldingFeature(provider as FeatureDefinition).Amount;
            }
            else if (provider.AttackRollModifierMethod == RuleDefinitions.AttackModifierMethod.AddAbilityScoreBonus &&
                     !string.IsNullOrEmpty(provider.AttackRollAbilityScore))
            {
                num += AttributeDefinitions.ComputeAbilityScoreModifier(
                    hero.TryGetAttributeValue(provider.AttackRollAbilityScore));
            }

            return num;
        }

        public static void Postfix(RulesetCharacterHero __instance,
            RulesetAttackMode __result,
            ActionDefinitions.ActionType actionType,
            ItemDefinition itemDefinition,
            WeaponDescription weaponDescription,
            bool freeOffHand,
            bool canAddAbilityDamageBonus,
            string slotName,
            List<IAttackModificationProvider> attackModifiers,
            Dictionary<FeatureDefinition, RuleDefinitions.FeatureOrigin> featuresOrigin,
            RulesetItem weapon
        )
        {
            //PATCH: Allows changing what attribute is used for weapon's attack and damage rolls
            var modifiers = __instance.GetSubFeaturesByType<IModifyAttackAttributeForWeapon>();

            var mods = modifiers;
            var attackMode = __result;
            
            if (attackMode.sourceObject is RulesetItem item)
            {
                mods = item.GetSubFeaturesByType<IModifyAttackAttributeForWeapon>();
                mods.AddRange(modifiers);
            }

            foreach (var modifier in mods)
            {
                modifier.ModifyAttribute(__instance, attackMode, weapon, canAddAbilityDamageBonus);
            }
        }
    }

    [HarmonyPatch(typeof(RulesetCharacterHero), "RefreshAttackModes")]
    [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
    public static class RefreshAttackModes_Patch
    {
        private static bool _callRefresh;

        public static void Prefix(ref bool callRefresh)
        {
            //save refresh flag, so it can be used in postfix
            _callRefresh = callRefresh;
            //reset refresh flag, so default code won't do refresh before postfix
            callRefresh = false;
        }

        public static void Postfix(RulesetCharacterHero __instance)
        {
            //PATCH: Allows adding extra attack modes
            __instance.GetSubFeaturesByType<IAddExtraAttack>()
                .ForEach(provider => provider.TryAddExtraAttack(__instance));

            //PATCH: Allows changing damage and other stats of an attack mode
            var modifiers = __instance.GetSubFeaturesByType<IModifyAttackModeForWeapon>();

            foreach (var attackMode in __instance.AttackModes)
            {
                var mods = modifiers;
                if (attackMode.sourceObject is RulesetItem item)
                {
                    mods = item.GetSubFeaturesByType<IModifyAttackModeForWeapon>();
                    mods.AddRange(modifiers);
                }

                foreach (var modifier in mods)
                {
                    modifier.ModifyAttackMode(__instance, attackMode);
                }
            }

            //refresh character if needed after postfix
            if (_callRefresh && __instance.CharacterRefreshed != null)
            {
                __instance.CharacterRefreshed(__instance);
            }
        }
    }

    [HarmonyPatch(typeof(RulesetCharacterHero), "RefreshAll")]
    [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
    public static class RefreshAll_Patch
    {
        public static void Prefix(RulesetCharacterHero __instance)
        {
            //PATCH: clears cached customized spell effects
            PowerBundle.ClearSpellEffectCache(__instance);

#if false
            //PATCH: Support for `IHeroRefreshed`
            __instance.GetSubFeaturesByType<IHeroRefreshed>()
                .ForEach(listener => listener.OnHeroRefreshed(__instance));
#endif
        }
    }

    [HarmonyPatch(typeof(RulesetCharacterHero), "RefreshActiveFightingStyles")]
    [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
    public static class RefreshActiveFightingStyles_Patch
    {
        public static void Postfix(RulesetCharacterHero __instance)
        {
            //PATCH: enables some corner-case fighting styles (like archery for hand crossbows and dual wielding for shield expert)
            FightingStyleContext.RefreshFightingStylesPatch(__instance);
        }
    }

    [HarmonyPatch(typeof(RulesetCharacterHero), "AcknowledgeAttackUse")]
    [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
    public static class AcknowledgeAttackUse_Patch
    {
        // ReSharper disable once RedundantAssignment
        public static void Prefix(RulesetCharacterHero __instance,
            RulesetAttackMode mode,
            ref RuleDefinitions.AttackProximity proximity)
        {
            //PATCH: supports turning Produced Flame into a weapon
            //destroys Produced Flame after attacking with it
            CustomWeaponsContext.ProcessProducedFlameAttack(__instance, mode);

            //PATCH: Support for returning weapons
            //Sets proximity to `Melee` if this was ranged attack with thrown weapon that has returning sub-feature
            //this will skip removal of the weapon from hand and attempt to get new one from inventory
            proximity = ReturningWeapon.Process(__instance, mode, proximity);
        }
    }

    [HarmonyPatch(typeof(RulesetCharacterHero), "ComputeCraftingDurationHours")]
    [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
    public static class ComputeCraftingDurationHours_Patch
    {
        public static void Postfix(ref int __result)
        {
            //PATCH: reduces the total crafting time by a given percentage
            __result = (int)((100f - Main.Settings.TotalCraftingTimeModifier) / 100 * __result);
        }
    }

    //PATCH: DisableAutoEquip
    [HarmonyPatch(typeof(RulesetCharacterHero), "GrantItem")]
    [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
    public static class GrantItem_Patch
    {
        public static void Prefix(RulesetCharacterHero __instance, ref bool tryToEquip)
        {
            if (!Main.Settings.DisableAutoEquip || !tryToEquip)
            {
                return;
            }

            tryToEquip = __instance.TryGetHeroBuildingData(out _);
        }
    }

    //PATCH: ensures ritual spells from all spell repertoires are made available (Multiclass)
    [HarmonyPatch(typeof(RulesetCharacterHero), "EnumerateUsableRitualSpells")]
    [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
    public static class EnumerateUsableRitualSpells_Patch
    {
        public static bool Prefix(
            RulesetCharacterHero __instance,
            List<SpellDefinition> ritualSpells)
        {
            if (!SharedSpellsContext.IsMulticaster(__instance))
            {
                return true;
            }

            var allRitualSpells = new List<SpellDefinition>();
            var magicAffinities = new List<FeatureDefinition>();

            ritualSpells.SetRange(allRitualSpells);

            __instance.EnumerateFeaturesToBrowse<FeatureDefinitionMagicAffinity>(magicAffinities);

            // ReSharper disable once PossibleInvalidCastExceptionInForeachLoop
            foreach (FeatureDefinitionMagicAffinity featureDefinitionMagicAffinity in magicAffinities)
            {
                if (featureDefinitionMagicAffinity.RitualCasting == RuleDefinitions.RitualCasting.None)
                {
                    continue;
                }

                foreach (var spellRepertoire in __instance.SpellRepertoires)
                {
                    // this is very similar to switch statement TA wrote but with spell loops outside
                    switch (featureDefinitionMagicAffinity.RitualCasting)
                    {
                        case RuleDefinitions.RitualCasting.PactTomeRitual:
                        {
                            var maxSpellLevel = SharedSpellsContext.MaxSpellLevelOfSpellCastingLevel(spellRepertoire);

                            foreach (var kvp in spellRepertoire.ExtraSpellsByTag.Where(kvp =>
                                         kvp.Key.Contains("PactTomeRitual")))
                            {
                                var spell = kvp.Value
                                    .Where(spellDefinition =>
                                        spellDefinition.Ritual && maxSpellLevel >= spellDefinition.SpellLevel);

                                allRitualSpells.AddRange(spell);
                            }

                            break;
                        }

                        case RuleDefinitions.RitualCasting.Selection:
                        {
                            var spells = spellRepertoire.KnownSpells
                                .Where(knownSpell =>
                                    knownSpell.Ritual && spellRepertoire.MaxSpellLevelOfSpellCastingLevel >=
                                    knownSpell.SpellLevel);

                            allRitualSpells.AddRange(spells);

                            break;
                        }

                        case RuleDefinitions.RitualCasting.Prepared
                            when spellRepertoire.SpellCastingFeature.SpellReadyness ==
                                 RuleDefinitions.SpellReadyness.Prepared &&
                                 spellRepertoire.SpellCastingFeature.SpellKnowledge ==
                                 RuleDefinitions.SpellKnowledge.WholeList:
                        {
                            var maxSpellLevel = SharedSpellsContext.MaxSpellLevelOfSpellCastingLevel(spellRepertoire);
                            var spells = spellRepertoire.PreparedSpells
                                .Where(s => s.Ritual)
                                .Where(s => maxSpellLevel >= s.SpellLevel);

                            allRitualSpells.AddRange(spells);

                            break;
                        }
                        case RuleDefinitions.RitualCasting.Spellbook
                            when spellRepertoire.SpellCastingFeature.SpellKnowledge ==
                                 RuleDefinitions.SpellKnowledge.Spellbook:
                        {
                            __instance.CharacterInventory.EnumerateAllItems(__instance.Items);

                            var maxSpellLevel = SharedSpellsContext.MaxSpellLevelOfSpellCastingLevel(spellRepertoire);
                            var spells = __instance.Items
                                .OfType<RulesetItemSpellbook>()
                                .SelectMany(x => x.ScribedSpells)
                                .ToList();

                            spells = spells
                                .Where(s => s.Ritual)
                                .Where(s => maxSpellLevel >= s.SpellLevel)
                                .ToList();

                            __instance.Items.Clear();

                            allRitualSpells.AddRange(spells);

                            break;
                        }

#if false
                        // special case for Witch
                        case (RuleDefinitions.RitualCasting)ExtraRitualCasting.Known:
                        {
                            var maxSpellLevel = SharedSpellsContext.MaxSpellLevelOfSpellCastingLevel(spellRepertoire);
                            var spells = spellRepertoire.KnownSpells
                                .Where(s => s.Ritual)
                                .Where(s => maxSpellLevel >= s.SpellLevel);

                            allRitualSpells.AddRange(spells);

                            if (spellRepertoire.AutoPreparedSpells == null)
                            {
                                return true;
                            }

                            spells = spellRepertoire.AutoPreparedSpells
                                .Where(s => s.Ritual)
                                .Where(s => maxSpellLevel >= s.SpellLevel);

                            allRitualSpells.AddRange(spells);
                            break;
                        }
#endif
                    }
                }
            }

            ritualSpells.SetRange(allRitualSpells.Distinct());

            return false;
        }
    }

    [HarmonyPatch(typeof(RulesetCharacterHero), "GrantExperience")]
    [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
    public static class GrantExperience_Patch
    {
        public static void Prefix(ref int experiencePoints)
        {
            if (Main.Settings.MultiplyTheExperienceGainedBy is 100 or <= 0)
            {
                return;
            }

            // ReSharper disable once RedundantAssignment
            experiencePoints =
                (int)Math.Round(experiencePoints * Main.Settings.MultiplyTheExperienceGainedBy / 100.0f,
                    MidpointRounding.AwayFromZero);
        }
    }

    //PATCH: enables the No Experience on Level up cheat (NoExperienceOnLevelUp)
    [HarmonyPatch(typeof(RulesetCharacterHero), "CanLevelUp", MethodType.Getter)]
    [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
    public static class CanLevelUp_Patch
    {
        public static bool Prefix(RulesetCharacterHero __instance, ref bool __result)
        {
            if (Main.Settings.NoExperienceOnLevelUp)
            {
                var levelCap = Main.Settings.EnableLevel20
                    ? Level20Context.ModMaxLevel
                    : Level20Context.GameMaxLevel;

                __result = __instance.ClassesHistory.Count < levelCap;

                return false;
            }

            if (!Main.Settings.EnableLevel20)
            {
                return true;
            }

            {
                var levelCap = Main.Settings.EnableLevel20
                    ? Level20Context.ModMaxLevel
                    : Level20Context.GameMaxLevel;
                // If the game doesn't know how much XP to reach the next level it uses -1 to determine if the character can level up.
                // When a character is level 20, this ends up meaning the character can now level up forever unless we stop it here.
                if (__instance.ClassesHistory.Count < levelCap)
                {
                    return true;
                }

                __result = false;

                return false;
            }
        }
    }

    [HarmonyPatch(typeof(RulesetCharacterHero), "AddClassLevel")]
    [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
    public static class AddClassLevel_Patch
    {
        public static bool Prefix([NotNull] RulesetCharacterHero __instance, CharacterClassDefinition classDefinition)
        {
            var isLevelingUp = LevelUpContext.IsLevelingUp(__instance);

            if (!isLevelingUp)
            {
                return true;
            }

            //PATCH: only adds the dice max value on level 1 (MULTICLASS)
            __instance.ClassesHistory.Add(classDefinition);
            __instance.ClassesAndLevels.TryAdd(classDefinition, 0);
            __instance.ClassesAndLevels[classDefinition]++;
            __instance.hitPointsGainHistory.Add(HeroDefinitions.RollHitPoints(classDefinition.HitDice));
            __instance.ComputeCharacterLevel();
            __instance.ComputeProficiencyBonus();

            return false;
        }
    }

    [HarmonyPatch(typeof(RulesetCharacterHero), "EnumerateAvailableDevices")]
    [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
    public static class EnumerateAvailableDevices_Patch
    {
        public static void Postfix(
            RulesetCharacterHero __instance,
            ref IEnumerable<RulesetItemDevice> __result)
        {
            //PATCH: enabled `PowerPoolDevice` by adding fake device to hero's usable devices list
            if (__instance.UsableDeviceFromMenu != null)
            {
                return;
            }

            var providers = __instance.GetSubFeaturesByType<PowerPoolDevice>();
            if (providers.Empty())
            {
                return;
            }

            var tmp = __result.ToList();

            foreach (var provider in providers)
            {
                tmp.Add(provider.GetDevice(__instance));
            }

            __result = tmp;
        }
    }

    [HarmonyPatch(typeof(RulesetCharacterHero), "UseDeviceFunction")]
    [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
    public static class UseDeviceFunction_Patch
    {
        public static void Postfix(RulesetCharacterHero __instance,
            RulesetItemDevice usableDevice,
            RulesetDeviceFunction function,
            int additionalCharges)
        {
            //PATCH: enables `PowerPoolDevice` to consume usage for power pool
            var feature = PowerPoolDevice.GetFromRulesetItem(__instance, usableDevice);
            if (feature == null)
            {
                return;
            }

            var useAmount = function.DeviceFunctionDescription.UseAmount + additionalCharges;
            __instance.UpdateUsageForPower(feature.Pool, useAmount);
        }
    }

    [HarmonyPatch(typeof(RulesetCharacterHero), "Unregister")]
    [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
    public static class Unregister_Patch
    {
        public static void Postfix(RulesetCharacterHero __instance)
        {
            //PATCH: clears cached devices for a hero
            PowerPoolDevice.Clear(__instance);
        }
    }

    [HarmonyPatch(typeof(RulesetCharacterHero), "EnumerateAfterRestActions")]
    [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
    public static class EnumerateAfterRestActions_Patch
    {
        public static void Postfix(RulesetCharacterHero __instance)
        {
            __instance.afterRestActions.RemoveAll(activity =>
            {
                if (activity.functor != PowerBundleContext.UseCustomRestPowerFunctorName)
                {
                    return false;
                }

                var power = __instance.UsablePowers.FirstOrDefault(usablePower =>
                    usablePower.PowerDefinition.Name == activity.StringParameter);

                if (power == null)
                {
                    return false;
                }

                return !__instance.CanUsePower(power.PowerDefinition);
            });
        }
    }
}
