﻿using JetBrains.Annotations;
using SolastaUnfinishedBusiness.Api.Extensions;
using SolastaUnfinishedBusiness.CustomInterfaces;

namespace SolastaUnfinishedBusiness.CustomDefinitions;

// public class SpellDefinitionWithDependentEffects : SpellDefinition, ICustomMagicEffectBasedOnCaster
// {
//     private List<(List<FeatureDefinition>, EffectDescription)> FeaturesEffectList { get; } = new();
//
//     public EffectDescription GetCustomEffect(RulesetCharacter caster)
//     {
//         var casterFeatures = caster.GetFeaturesByType<FeatureDefinition>().ToHashSet();
//
//         foreach (var (featureDefinitions, customEffect) in FeaturesEffectList)
//         {
//             if (featureDefinitions.All(f => casterFeatures.Contains(f)))
//             {
//                 return customEffect;
//             }
//         }
//
//         return EffectDescription;
//     }
// }

// public class SpellModifyingFeatureDefinition : FeatureDefinition, IModifySpellEffect
// {
//     public delegate EffectDescription ModifySpellEffectDelegate(SpellDefinition spell, EffectDescription effect,
//         RulesetCharacter caster);
//
//     public ModifySpellEffectDelegate SpellModifier { get; set; }
//
//     public EffectDescription ModifyEffect(SpellDefinition spell, EffectDescription effect, RulesetCharacter caster)
//     {
//         return SpellModifier != null ? SpellModifier(spell, effect, caster) : effect;
//     }
// }

internal sealed class UpgradeEffectFromLevel : ICustomMagicEffectBasedOnCaster
{
    private readonly int _level;
    private readonly EffectDescription _upgraded;

    public UpgradeEffectFromLevel(EffectDescription upgraded, int level)
    {
        _upgraded = upgraded;
        _level = level;
    }

    [CanBeNull]
    public EffectDescription GetCustomEffect([NotNull] RulesetCharacter caster)
    {
        var casterLevel = caster.GetAttribute(AttributeDefinitions.CharacterLevel).CurrentValue;
        return casterLevel < _level ? null : _upgraded;
    }
}

internal sealed class UpgradeRangeBasedOnWeaponReach : IModifySpellEffect
{
    public EffectDescription ModifyEffect(SpellDefinition spell, EffectDescription effect, RulesetCharacter caster)
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

        var modified = effect.Copy();
        modified.rangeParameter = reach;
        return modified;
    }
}