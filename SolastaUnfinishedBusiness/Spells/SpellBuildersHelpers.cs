﻿using JetBrains.Annotations;
using SolastaUnfinishedBusiness.Api.Extensions;
using SolastaUnfinishedBusiness.Api.Helpers;
using SolastaUnfinishedBusiness.CustomInterfaces;
using static RuleDefinitions;

namespace SolastaUnfinishedBusiness.Spells;

internal static partial class SpellBuilders
{
    #region HELPERS

    private static class CustomSpellEffectLevel
    {
        internal static readonly ICustomSpellEffectLevel ByCasterLevel = new SpellEffectLevelFromCasterLevel();
    }

    private sealed class SpellEffectLevelFromCasterLevel : ICustomSpellEffectLevel
    {
        public int GetEffectLevel([NotNull] RulesetActor caster)
        {
            return caster.GetAttribute(AttributeDefinitions.CharacterLevel).CurrentValue;
        }
    }

    private sealed class ChainSpellEffectOnAttackHit : IChainMagicEffect
    {
        private readonly string _notificationTag;
        private readonly SpellDefinition _spell;

        internal ChainSpellEffectOnAttackHit(SpellDefinition spell, [CanBeNull] string notificationTag = null)
        {
            _spell = spell;
            _notificationTag = notificationTag;
        }

        [CanBeNull]
        public CharacterActionMagicEffect GetNextMagicEffect(
            [CanBeNull] CharacterActionMagicEffect baseEffect,
            CharacterActionAttack triggeredAttack,
            RollOutcome attackOutcome)
        {
            if (baseEffect == null)
            {
                return null;
            }

            var spellEffect = baseEffect as CharacterActionCastSpell;
            var repertoire = spellEffect?.ActiveSpell.SpellRepertoire;
            var actionParams = baseEffect.actionParams;

            if (actionParams == null)
            {
                return null;
            }

            if (baseEffect.Countered || baseEffect.ExecutionFailed)
            {
                return null;
            }

            if (attackOutcome != RollOutcome.Success
                && attackOutcome != RollOutcome.CriticalSuccess)
            {
                return null;
            }

            var caster = actionParams.ActingCharacter;
            var targets = actionParams.TargetCharacters;

            if (caster == null || targets.Count < 2)
            {
                return null;
            }

            var rulesetCaster = caster.RulesetCharacter;
            var rules = ServiceRepository.GetService<IRulesetImplementationService>();
            var bonusLevelProvider = _spell.GetFirstSubFeatureOfType<IBonusSlotLevels>();
            var slotLevel = _spell.SpellLevel;

            if (bonusLevelProvider != null)
            {
                slotLevel += bonusLevelProvider.GetBonusSlotLevels(rulesetCaster);
            }

            var effectSpell = rules.InstantiateEffectSpell(rulesetCaster, repertoire, _spell, slotLevel, false);

            for (var i = 1; i < targets.Count; i++)
            {
                var rulesetTarget = targets[i].RulesetCharacter;

                if (!string.IsNullOrEmpty(_notificationTag))
                {
                    GameConsoleHelper.LogCharacterAffectsTarget(rulesetCaster, rulesetTarget, _notificationTag, true);
                }

                effectSpell.ApplyEffectOnCharacter(rulesetTarget, true, targets[i].LocationPosition);
            }

            effectSpell.Terminate(true);

            return null;
        }
    }

    private sealed class BonusSlotLevelsByClassLevel : IBonusSlotLevels
    {
        public int GetBonusSlotLevels([NotNull] RulesetCharacter caster)
        {
            var level = caster.GetAttribute(AttributeDefinitions.CharacterLevel).CurrentValue;
            return RuleDefinitions.SpellAdvancementByCasterLevel[level - 1];
        }
    }

    private sealed class UpgradeRangeBasedOnWeaponReach : IModifyMagicEffect
    {
        public EffectDescription ModifyEffect(
            BaseDefinition definition,
            EffectDescription effect,
            RulesetCharacter caster)
        {
            if (caster is not RulesetCharacterHero hero)
            {
                return effect;
            }

            var weapon = hero.GetMainWeapon();

            if (weapon == null || !weapon.itemDefinition.IsWeapon)
            {
                return effect;
            }

            var reach = weapon.itemDefinition.WeaponDescription.ReachRange;

            if (reach <= 1)
            {
                return effect;
            }

            effect.rangeParameter = reach;
            return effect;
        }
    }

    #endregion
}
