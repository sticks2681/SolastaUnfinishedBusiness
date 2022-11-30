﻿using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using SolastaUnfinishedBusiness.Api;
using SolastaUnfinishedBusiness.Models;

namespace SolastaUnfinishedBusiness.CustomBehaviors;

internal delegate bool IsWeaponValidHandler(RulesetAttackMode attackMode, RulesetItem weapon,
    RulesetCharacter character);

internal static class ValidatorsWeapon
{
    internal static readonly IsWeaponValidHandler AlwaysValid = (_, _, _) => true;

    // internal static readonly IsWeaponValidHandler IsUnarmed = IsUnarmedWeapon;

    // internal static readonly IsWeaponValidHandler IsReactionAttack = IsReactionAttackMode;

    // internal static readonly IsWeaponValidHandler IsLight = (mode, weapon, _) =>
    //     HasActiveTag(mode, weapon, TagsDefinitions.WeaponTagLight);

    // internal static readonly IsWeaponValidHandler Melee = (_, weapon, _) => IsMelee(weapon);

    internal static bool IsPolearm([CanBeNull] RulesetItem weapon)
    {
        return weapon != null
               && IsPolearm(weapon.ItemDefinition);
    }

    internal static bool IsPolearm([CanBeNull] ItemDefinition weapon)
    {
        return weapon != null
               && weapon.IsWeapon
               && CustomWeaponsContext.PolearmWeaponTypes.Contains(weapon.WeaponDescription?.WeaponType);
    }

    internal static bool IsMelee([CanBeNull] RulesetItem weapon)
    {
        return weapon == null // for unarmed
               || weapon.ItemDefinition.WeaponDescription?.WeaponTypeDefinition.WeaponProximity ==
               RuleDefinitions.AttackProximity.Melee
               || weapon.ItemDefinition.IsArmor;
    }

    internal static bool IsMelee([NotNull] RulesetAttackMode attack)
    {
        //TODO: test if this is enough, or we need to check SourceDefinition too
        return !attack.ranged;
    }

    internal static bool IsRanged(RulesetItem weapon)
    {
        return HasAnyWeaponTag(weapon, TagsDefinitions.WeaponTagRange, TagsDefinitions.WeaponTagThrown);
    }

    internal static bool IsOneHanded(RulesetAttackMode attackMode)
    {
        if (attackMode is not { SourceDefinition: ItemDefinition weapon })
        {
            return false;
        }

        return !HasAnyWeaponTag(weapon, TagsDefinitions.WeaponTagTwoHanded);
    }
    
    internal static bool IsOneHanded(RulesetItem weapon)
    {
        return !HasAnyWeaponTag(weapon, TagsDefinitions.WeaponTagTwoHanded);
    }

    private static bool IsUnarmedWeapon(
        [CanBeNull] RulesetAttackMode attackMode,
        RulesetItem weapon)
    {
        var item = attackMode?.SourceDefinition as ItemDefinition ?? weapon?.ItemDefinition;

        if (item != null)
        {
            return item.WeaponDescription?.WeaponTypeDefinition ==
                   DatabaseHelper.WeaponTypeDefinitions.UnarmedStrikeType;
        }

        return weapon == null;
    }

    internal static bool IsUnarmedWeapon(RulesetAttackMode attackMode)
    {
        return IsUnarmedWeapon(attackMode, null);
    }

    internal static bool IsUnarmedWeapon(RulesetItem weapon)
    {
        return IsUnarmedWeapon(null, weapon);
    }

    internal static bool IsTwoHanded([CanBeNull] RulesetItem weapon)
    {
        return weapon != null && weapon.itemDefinition.isWeapon &&
               weapon.itemDefinition.WeaponDescription.WeaponTags.Contains(TagsDefinitions.WeaponTagTwoHanded);
    }

    internal static bool IsMagic(RulesetAttackMode attackMode, RulesetItem weapon, RulesetCharacter character)
    {
        if (attackMode.Magical)
        {
            return true;
        }

        if (weapon == null)
        {
            return false;
        }

        return weapon.IsMagicalWeapon() || ShieldAttack.IsMagicShield(weapon);
    }

    internal static bool IsThrownWeapon([CanBeNull] RulesetItem weapon)
    {
        return weapon != null && weapon.itemDefinition.isWeapon &&
               weapon.itemDefinition.WeaponDescription.WeaponTags.Contains(TagsDefinitions.WeaponTagThrown);
    }

    // internal static bool IsReactionAttackMode(RulesetAttackMode attackMode, RulesetItem weapon,
    //     RulesetCharacter character)
    // {
    //     return attackMode is {ActionType: ActionDefinitions.ActionType.Reaction};
    // }

    // internal static bool HasAnyTag(RulesetItem item, params string[] tags)
    // {
    //     var tagsMap = new Dictionary<string, TagsDefinitions.Criticity>();
    //     item?.FillTags(tagsMap, null, true);
    //     return tagsMap.Keys.Any(tags.Contains);
    // }

    internal static bool HasAnyWeaponTag([CanBeNull] RulesetItem item, [NotNull] params string[] tags)
    {
        return HasAnyWeaponTag(item?.ItemDefinition, tags);
    }

    private static bool HasAnyWeaponTag(ItemDefinition item, [NotNull] params string[] tags)
    {
        var weaponTags = GetWeaponTags(item);

        return tags.Any(t => weaponTags.Contains(t));
    }

    // private static bool HasActiveTag(RulesetAttackMode mode, RulesetItem weapon, string tag)
    // {
    //     var hasTag = false;
    //     if (mode != null)
    //     {
    //         hasTag = mode.AttackTags.Contains(tag);
    //         if (!hasTag)
    //         {
    //             var tags = GetWeaponTags(mode.SourceDefinition as ItemDefinition);
    //             if (tags != null && tags.Contains(tag))
    //             {
    //                 hasTag = true;
    //             }
    //         }
    //
    //         return hasTag;
    //     }
    //
    //     if (weapon != null)
    //     {
    //         var tags = GetWeaponTags(weapon.ItemDefinition);
    //         if (tags != null && tags.Contains(tag))
    //         {
    //             hasTag = true;
    //         }
    //     }
    //
    //     return hasTag;
    // }

    private static List<string> GetWeaponTags([CanBeNull] ItemDefinition item)
    {
        if (item != null && item.IsWeapon)
        {
            return item.WeaponDescription.WeaponTags;
        }

        return new List<string>();
    }
}
