using System.Collections.Generic;
using SolastaUnfinishedBusiness.Api.Extensions;
using SolastaUnfinishedBusiness.Builders;
using SolastaUnfinishedBusiness.Builders.Features;
using SolastaUnfinishedBusiness.CustomBehaviors;
using SolastaUnfinishedBusiness.CustomInterfaces;
using SolastaUnfinishedBusiness.CustomUI;
using SolastaUnfinishedBusiness.Models;
using SolastaUnfinishedBusiness.Properties;
using static RuleDefinitions;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper.FeatureDefinitionActionAffinitys;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper.FeatureDefinitionAdditionalDamages;

namespace SolastaUnfinishedBusiness.Subclasses;

internal sealed class WayOfTheDistantHand : AbstractSubclass
{
    private const string ZenArrowTag = "ZenArrow";

    // Zen Archer's Monk weapons are bows and darts ranged weapons.
    private static readonly List<WeaponTypeDefinition> ZenArcherWeapons = new()
    {
        WeaponTypeDefinitions.ShortbowType, WeaponTypeDefinitions.LongbowType
    };

    internal WayOfTheDistantHand()
    {
        // proficient with all two handed range weapons
        // ignore cover and long range disadvantage
        var featureSetDistantHandSharpShooter = FeatureDefinitionFeatureSetBuilder
            .Create("FeatureSetDistantHandSharpShooter")
            .SetGuiPresentation(Category.Feature)
            .AddFeatureSet(
                FeatureDefinitionProficiencyBuilder
                    .Create("ProficiencyDistantHandRangeWeapon")
                    .SetGuiPresentationNoContent(true)
                    .SetProficiencies(
                        ProficiencyType.Weapon,
                        WeaponTypeDefinitions.HeavyCrossbowType.Name,
                        WeaponTypeDefinitions.LongbowType.Name)
                    .AddToDB(),
                FeatureDefinitionCombatAffinityBuilder
                    .Create("CombatAffinityDistantHandRangeAttack")
                    .SetGuiPresentationNoContent(true)
                    .SetIgnoreCover()
                    .SetCustomSubFeatures(new BumpWeaponAttackRangeToMax(ValidatorsWeapon.AlwaysValid))
                    .AddToDB())
            .AddToDB();

        var zenArrow = Sprites.GetSprite("ZenArrow", Resources.ZenArrow, 128, 64);

        //
        // LEVEL 03
        //

        var proficiencyWayOfTheDistantHandCombat = FeatureDefinitionProficiencyBuilder
            .Create("ProficiencyWayOfTheDistantHandCombat")
            .SetGuiPresentation(Category.Feature)
            .SetProficiencies(
                ProficiencyType.Weapon,
                WeaponTypeDefinitions.LongbowType.Name,
                WeaponTypeDefinitions.ShortbowType.Name)
            .SetCustomSubFeatures(
                new ZenArcherMarker(),
                new RangedAttackInMeleeDisadvantageRemover(
                    IsMonkWeapon, ValidatorsCharacter.NoArmor, ValidatorsCharacter.NoShield),
                new AddTagToWeaponAttack(ZenArrowTag, IsZenArrowAttack)
            )
            .AddToDB();

        // ZEN ARROW

        var powerWayOfTheDistantHandZenArrowTechnique = FeatureDefinitionPowerBuilder
            .Create("PowerWayOfTheDistantHandZenArrowTechnique")
            .SetGuiPresentation(Category.Feature, zenArrow)
            .SetUsesFixed(ActivationTime.OnAttackHit, RechargeRate.KiPoints)
            .SetCustomSubFeatures(
                new RestrictReactionAttackMode((mode, _, _) =>
                    mode != null && mode.AttackTags.Contains(ZenArrowTag)))
            .SetEffectDescription(EffectDescriptionBuilder.Create()
                .SetTargetingData(Side.Enemy, RangeType.Distance, 1, TargetType.Individuals)
                .Build())
            .AddToDB();

        var powerWayOfTheDistantHandZenArrowProne = FeatureDefinitionPowerBuilder
            .Create("PowerWayOfTheDistantHandZenArrowProne")
            .SetGuiPresentation(Category.Feature)
            .SetUsesFixed(ActivationTime.NoCost, RechargeRate.KiPoints)
            .SetEffectDescription(EffectDescriptionBuilder.Create()
                .SetDurationData(DurationType.Round, 1)
                .SetTargetingData(Side.Enemy, RangeType.Touch, 1, TargetType.Individuals)
                .SetTargetFiltering(TargetFilteringMethod.CharacterOnly)
                .SetSavingThrowData(
                    true,
                    AttributeDefinitions.Dexterity,
                    true,
                    EffectDifficultyClassComputation.AbilityScoreAndProficiency)
                .SetEffectForms(EffectFormBuilder.Create()
                    .HasSavingThrow(EffectSavingThrowType.Negates)
                    .SetLevelAdvancement(EffectForm.LevelApplianceType.No, LevelSourceType.ClassLevel)
                    .SetMotionForm(MotionForm.MotionType.FallProne)
                    .Build())
                .Build())
            .AddToDB();

        var powerWayOfTheDistantHandZenArrowPush = FeatureDefinitionPowerBuilder
            .Create("PowerWayOfTheDistantHandZenArrowPush")
            .SetGuiPresentation(Category.Feature)
            .SetUsesFixed(ActivationTime.NoCost, RechargeRate.KiPoints)
            .SetEffectDescription(EffectDescriptionBuilder.Create()
                .SetDurationData(DurationType.Round, 1)
                .SetTargetingData(Side.Enemy, RangeType.Touch, 1, TargetType.Individuals)
                .SetTargetFiltering(TargetFilteringMethod.CharacterOnly)
                .SetSavingThrowData(
                    true,
                    AttributeDefinitions.Strength,
                    true,
                    EffectDifficultyClassComputation.AbilityScoreAndProficiency)
                .SetEffectForms(EffectFormBuilder.Create()
                    .HasSavingThrow(EffectSavingThrowType.Negates)
                    .SetLevelAdvancement(EffectForm.LevelApplianceType.No, LevelSourceType.ClassLevel)
                    .SetMotionForm(MotionForm.MotionType.PushFromOrigin, 2)
                    .Build())
                .Build())
            .AddToDB();

        var powerWayOfTheDistantHandZenArrowDistract = FeatureDefinitionPowerBuilder
            .Create("PowerWayOfTheDistantHandZenArrowDistract")
            .SetGuiPresentation(Category.Feature)
            .SetUsesFixed(ActivationTime.NoCost, RechargeRate.KiPoints)
            .SetEffectDescription(EffectDescriptionBuilder.Create()
                .SetDurationData(DurationType.Round, 1)
                .SetTargetingData(Side.Enemy, RangeType.Touch, 1, TargetType.Individuals)
                .SetTargetFiltering(TargetFilteringMethod.CharacterOnly)
                .SetSavingThrowData(
                    true,
                    AttributeDefinitions.Wisdom,
                    true,
                    EffectDifficultyClassComputation.AbilityScoreAndProficiency)
                .SetEffectForms(EffectFormBuilder.Create()
                    .SetLevelAdvancement(EffectForm.LevelApplianceType.No, LevelSourceType.ClassLevel)
                    .HasSavingThrow(EffectSavingThrowType.Negates)
                    .SetConditionForm(ConditionDefinitionBuilder
                            .Create("ConditionWayOfTheDistantHandDistract")
                            .SetGuiPresentation(Category.Condition, ConditionDefinitions.ConditionDazzled)
                            .SetTurnOccurence(TurnOccurenceType.EndOfTurn)
                            .SetConditionType(ConditionType.Detrimental)
                            .SetSpecialInterruptions(ConditionInterruption.Attacks)
                            .SetFeatures(FeatureDefinitionCombatAffinityBuilder
                                .Create("CombatAffinityWayOfTheDistantHandDistract")
                                .SetGuiPresentationNoContent(true)
                                .SetMyAttackAdvantage(AdvantageType.Disadvantage)
                                .AddToDB())
                            .AddToDB(),
                        ConditionForm.ConditionOperation.Add)
                    .Build())
                .Build())
            .AddToDB();

        PowerBundle.RegisterPowerBundle(
            powerWayOfTheDistantHandZenArrowTechnique,
            true,
            powerWayOfTheDistantHandZenArrowProne,
            powerWayOfTheDistantHandZenArrowPush,
            powerWayOfTheDistantHandZenArrowDistract);

        //
        // LEVEL 06
        //

        var additionalActionWayOfTheDistantHandExtraAttack1 = FeatureDefinitionAdditionalActionBuilder
            .Create("AdditionalActionWayOfTheDistantHandExtraAttack1")
            .SetGuiPresentationNoContent(true)
            .SetCustomSubFeatures(
                new AddExtraMainHandAttack(ActionDefinitions.ActionType.Bonus, true,
                    ValidatorsCharacter.NoArmor, ValidatorsCharacter.NoShield, WieldsZenArcherWeapon))
            .SetActionType(ActionDefinitions.ActionType.Bonus)
            .SetRestrictedActions(ActionDefinitions.Id.AttackOff)
            .AddToDB();

        var additionalActionWayOfTheDistantHandExtraAttack2 = FeatureDefinitionAdditionalActionBuilder
            .Create("AdditionalActionWayOfTheDistantHandExtraAttack2")
            .SetGuiPresentationNoContent(true)
            .SetActionType(ActionDefinitions.ActionType.Bonus)
            .SetRestrictedActions(ActionDefinitions.Id.AttackOff)
            .AddToDB();

        //
        // LEVEL 06
        //

        var flurryOfArrowsSprite = Sprites.GetSprite("FlurryOfArrows", Resources.FlurryOfArrows, 128, 64);

        var powerWayOfTheDistantHandZenArcherFlurryOfArrows = FeatureDefinitionPowerBuilder
            .Create("PowerWayOfTheDistantHandZenArcherFlurryOfArrows")
            .SetGuiPresentation(Category.Feature, flurryOfArrowsSprite)
            .SetUsesFixed(ActivationTime.BonusAction, RechargeRate.KiPoints, 2)
            .SetCustomSubFeatures(new ValidatorsPowerUse(
                ValidatorsCharacter.HasAnyOfConditions(ConditionDefinitionBuilder
                    .Create("ConditionWayOfTheDistantHandAttackedWithMonkWeapon")
                    .SetGuiPresentationNoContent(true)
                    .SetSilent(Silent.WhenAddedOrRemoved)
                    .SetSpecialDuration(DurationType.Round, 1)
                    .SetTurnOccurence(TurnOccurenceType.StartOfTurn)
                    .SetSpecialInterruptions(
                        ConditionInterruption.BattleEnd,
                        ConditionInterruption.AnyBattleTurnEnd)
                    .AddToDB()),
                ValidatorsCharacter.NoShield,
                ValidatorsCharacter.NoArmor))
            .SetEffectDescription(EffectDescriptionBuilder.Create()
                .SetEffectForms(EffectFormBuilder.Create()
                    .SetConditionForm(ConditionDefinitionBuilder
                            .Create("ConditionWayOfTheDistantHandZenArcherFlurryOfArrows")
                            .SetGuiPresentationNoContent(true)
                            .SetSilent(Silent.WhenAddedOrRemoved)
                            .SetSpecialDuration(DurationType.Round, 0, false)
                            .SetTurnOccurence(TurnOccurenceType.EndOfTurn)
                            .SetSpecialInterruptions(
                                ConditionInterruption.BattleEnd,
                                ConditionInterruption.AnyBattleTurnEnd)
                            .SetFeatures(
                                additionalActionWayOfTheDistantHandExtraAttack1,
                                additionalActionWayOfTheDistantHandExtraAttack2)
                            .AddToDB(),
                        ConditionForm.ConditionOperation.Add, true, true)
                    .Build())
                .Build())
            .SetShowCasting(false)
            .AddToDB();

        var wayOfDistantHandsKiPoweredArrows = FeatureDefinitionBuilder
            .Create("FeatureWayOfTheDistantHandKiPoweredArrows")
            .SetGuiPresentation(Category.Feature)
            .SetCustomSubFeatures(
                new AddTagToWeaponAttack(TagsDefinitions.Magical, (mode, _, character) =>
                    IsZenArcherWeapon(character, mode.SourceDefinition as ItemDefinition)))
            .AddToDB();

        //
        // LEVEL 11
        //

        var wayOfDistantHandsZenArcherStunningArrows = FeatureDefinitionBuilder
            .Create("FeatureWayOfTheDistantHandZenArcherStunningArrows")
            .SetGuiPresentation(Category.Feature)
            .SetCustomSubFeatures(new ZenArcherStunningArrows())
            .AddToDB();

        // UPGRADE ZEN ARROW

        var powerWayOfTheDistantHandZenArrowUpgradedTechnique = FeatureDefinitionPowerBuilder
            .Create("PowerWayOfTheDistantHandZenArrowUpgradedTechnique")
            .SetGuiPresentation(Category.Feature, zenArrow)
            .SetUsesFixed(ActivationTime.OnAttackHit, RechargeRate.KiPoints)
            .SetOverriddenPower(powerWayOfTheDistantHandZenArrowTechnique)
            .SetCustomSubFeatures(new RestrictReactionAttackMode((mode, _, _) =>
                mode != null && mode.AttackTags.Contains(ZenArrowTag)))
            .AddToDB();

        var powerWayOfTheDistantHandZenArrowUpgradedProne = FeatureDefinitionPowerBuilder
            .Create("PowerWayOfTheDistantHandZenArrowUpgradedProne")
            .SetGuiPresentation(Category.Feature)
            .SetUsesFixed(ActivationTime.NoCost, RechargeRate.KiPoints)
            .SetEffectDescription(EffectDescriptionBuilder.Create()
                .SetDurationData(DurationType.Round, 1)
                .SetTargetingData(Side.Enemy, RangeType.Touch, 1, TargetType.Individuals)
                .SetTargetFiltering(TargetFilteringMethod.CharacterOnly)
                .SetSavingThrowData(
                    true,
                    AttributeDefinitions.Dexterity,
                    true,
                    EffectDifficultyClassComputation.AbilityScoreAndProficiency)
                .SetEffectForms(EffectFormBuilder.Create()
                        .HasSavingThrow(EffectSavingThrowType.Negates)
                        .SetLevelAdvancement(EffectForm.LevelApplianceType.No, LevelSourceType.ClassLevel)
                        .SetMotionForm(MotionForm.MotionType.FallProne)
                        .Build(),
                    EffectFormBuilder.Create()
                        .HasSavingThrow(EffectSavingThrowType.Negates)
                        .SetConditionForm(ConditionDefinitionBuilder
                            .Create("ConditionWayOfTheDistantHandZenArrowUpgradedSlow")
                            .SetGuiPresentation(Category.Condition, ConditionDefinitions.ConditionEncumbered)
                            .SetTurnOccurence(TurnOccurenceType.EndOfTurn)
                            .SetConditionType(ConditionType.Detrimental)
                            .SetFeatures(FeatureDefinitionMovementAffinityBuilder
                                .Create("MovementAffinityWayOfTheDistantHandUpgradedSlow")
                                .SetGuiPresentationNoContent(true)
                                .SetBaseSpeedMultiplicativeModifier(0)
                                .AddToDB())
                            .AddToDB(), ConditionForm.ConditionOperation.Add)
                        .Build())
                .Build())
            .AddToDB();

        var powerWayOfTheDistantHandUpgradedPush = FeatureDefinitionPowerBuilder
            .Create("PowerWayOfTheDistantHandUpgradedPush")
            .SetGuiPresentation(Category.Feature)
            .SetUsesFixed(ActivationTime.NoCost, RechargeRate.KiPoints)
            .SetEffectDescription(EffectDescriptionBuilder.Create()
                .SetDurationData(DurationType.Round, 1)
                .SetTargetingData(Side.Enemy, RangeType.Touch, 1, TargetType.Individuals)
                .SetTargetFiltering(TargetFilteringMethod.CharacterOnly)
                .SetSavingThrowData(
                    true,
                    AttributeDefinitions.Strength,
                    true,
                    EffectDifficultyClassComputation.AbilityScoreAndProficiency)
                .SetEffectForms(EffectFormBuilder.Create()
                    .HasSavingThrow(EffectSavingThrowType.Negates)
                    .SetLevelAdvancement(EffectForm.LevelApplianceType.No, LevelSourceType.ClassLevel)
                    .SetMotionForm(MotionForm.MotionType.PushFromOrigin, 4)
                    .Build())
                .Build())
            .AddToDB();

        var powerWayOfTheDistantHandUpgradedDistract = FeatureDefinitionPowerBuilder
            .Create("PowerWayOfTheDistantHandUpgradedDistract")
            .SetGuiPresentation(Category.Feature)
            .SetUsesFixed(ActivationTime.NoCost, RechargeRate.KiPoints)
            .SetEffectDescription(EffectDescriptionBuilder.Create()
                .SetDurationData(DurationType.Round, 1)
                .SetTargetingData(Side.Enemy, RangeType.Touch, 1, TargetType.Individuals)
                .SetTargetFiltering(TargetFilteringMethod.CharacterOnly)
                .SetSavingThrowData(
                    true,
                    AttributeDefinitions.Wisdom,
                    true,
                    EffectDifficultyClassComputation.AbilityScoreAndProficiency)
                .SetEffectForms(
                    EffectFormBuilder.Create()
                        .SetLevelAdvancement(EffectForm.LevelApplianceType.No, LevelSourceType.ClassLevel)
                        .HasSavingThrow(EffectSavingThrowType.Negates)
                        .SetConditionForm(ConditionDefinitionBuilder
                            .Create("ConditionWayOfTheDistantHandUpgradedDistract")
                            .SetGuiPresentation(Category.Condition, ConditionDefinitions.ConditionDazzled)
                            .SetTurnOccurence(TurnOccurenceType.EndOfTurn)
                            .SetConditionType(ConditionType.Detrimental)
                            .SetFeatures(FeatureDefinitionCombatAffinityBuilder
                                .Create("CombatAffinityWayOfTheDistantHandUpgradedDistract")
                                .SetGuiPresentationNoContent(true)
                                .SetMyAttackAdvantage(AdvantageType.Disadvantage)
                                .AddToDB())
                            .AddToDB(), ConditionForm.ConditionOperation.Add)
                        .Build())
                .Build())
            .AddToDB();

        PowerBundle.RegisterPowerBundle(
            powerWayOfTheDistantHandZenArrowUpgradedTechnique,
            true,
            powerWayOfTheDistantHandZenArrowUpgradedProne,
            powerWayOfTheDistantHandUpgradedPush,
            powerWayOfTheDistantHandUpgradedDistract);

        //
        // PROGRESSION
        //

        Subclass = CharacterSubclassDefinitionBuilder
            .Create("WayOfTheDistantHand")
            .SetOrUpdateGuiPresentation(Category.Subclass, CharacterSubclassDefinitions.RangerMarksman)
            .AddFeaturesAtLevel(3,
                ActionAffinityRogueCunningAction,
                AdditionalDamageRogueSneakAttack,
                featureSetDistantHandSharpShooter,
                BuildHeartSeekingShot(),
                proficiencyWayOfTheDistantHandCombat,
                powerWayOfTheDistantHandZenArrowTechnique)
            .AddFeaturesAtLevel(6,
                wayOfDistantHandsKiPoweredArrows,
                powerWayOfTheDistantHandZenArcherFlurryOfArrows)
            .AddFeaturesAtLevel(11,
                wayOfDistantHandsZenArcherStunningArrows,
                powerWayOfTheDistantHandZenArrowUpgradedTechnique)
            .AddToDB();
    }

    private static FeatureDefinitionFeatureSet BuildHeartSeekingShot()
    {
        var concentrationProvider = new StopPowerConcentrationProvider("HeartSeekingShot",
            "Tooltip/&HeartSeekingShotConcentration",
            Sprites.GetSprite("DeadeyeConcentrationIcon",
                Resources.DeadeyeConcentrationIcon, 64, 64));

        var conditionDistantHandHeartSeekingShotTrigger = ConditionDefinitionBuilder
            .Create("ConditionDistantHandHeartSeekingShotTrigger")
            .SetGuiPresentationNoContent(true)
            .SetSilent(Silent.WhenAddedOrRemoved)
            .SetFeatures(
                FeatureDefinitionBuilder
                    .Create("TriggerFeatureDistantHandHeartSeekingShot")
                    .SetGuiPresentationNoContent(true)
                    .SetCustomSubFeatures(concentrationProvider)
                    .AddToDB())
            .AddToDB();

        // -4 attack roll but critical threshold is 18 and deal 3d6 additional damage
        var conditionDistantHandHeartSeekingShot = ConditionDefinitionBuilder
            .Create("ConditionDistantHandHeartSeekingShot")
            .SetGuiPresentation("FeatureSetDistantHandHeartSeekingShot", Category.Feature)
            .AddFeatures(
                FeatureDefinitionAttributeModifierBuilder
                    .Create("AttributeModifierDistantHandHeartSeekingShotCriticalThreshold")
                    .SetGuiPresentation(Category.Feature)
                    .SetModifier(FeatureDefinitionAttributeModifier.AttributeModifierOperation.Additive,
                        AttributeDefinitions.CriticalThreshold, -2)
                    .SetCustomSubFeatures(
                        new ValidatorsDefinitionApplication(ValidatorsCharacter.HasTwoHandedRangeWeapon))
                    .AddToDB(),
                FeatureDefinitionAttackModifierBuilder
                    .Create("AttackModifierDistantHandHeartSeekingShot")
                    .SetGuiPresentation(Category.Feature)
                    .SetAttackRollModifier(-4)
                    .SetCustomSubFeatures(
                        new RestrictedContextValidator(OperationType.Set, ValidatorsCharacter.HasTwoHandedRangeWeapon))
                    .SetRequiredProperty(RestrictedContextRequiredProperty.RangeWeapon)
                    .AddToDB(),
                FeatureDefinitionAdditionalDamageBuilder
                    .Create("AdditionalDamageDistantHandHeartSeekingShot")
                    .SetGuiPresentationNoContent(true)
                    .SetNotificationTag("HeartSeekingShot")
                    .SetFrequencyLimit(FeatureLimitedUsage.None)
                    .SetTriggerCondition(AdditionalDamageTriggerCondition.CriticalHit)
                    .SetAdditionalDamageType(AdditionalDamageType.SameAsBaseDamage)
                    .SetCustomSubFeatures(
                        new RestrictedContextValidator(OperationType.Set, ValidatorsCharacter.HasTwoHandedRangeWeapon))
                    .SetRequiredProperty(RestrictedContextRequiredProperty.RangeWeapon)
                    .SetDamageDice(DieType.D6, 1)
                    .SetAdvancement(AdditionalDamageAdvancement.ClassLevel, 2, 1, 4, 3)
                    .AddToDB()
            )
            .AddToDB();

        var deadEyeSprite = Sprites.GetSprite("DeadeyeIcon", Resources.DeadeyeIcon, 128, 64);

        var powerDistantHandHeartSeekingShot = FeatureDefinitionPowerBuilder
            .Create("PowerDistantHandHeartSeekingShot")
            .SetGuiPresentation("FeatureSetDistantHandHeartSeekingShot", Category.Feature, deadEyeSprite)
            .SetUsesFixed(ActivationTime.NoCost)
            .SetEffectDescription(EffectDescriptionBuilder
                .Create()
                .SetTargetingData(Side.Ally, RangeType.Self, 0, TargetType.Self)
                .SetDurationData(DurationType.Permanent)
                .SetEffectForms(
                    EffectFormBuilder
                        .Create()
                        .SetConditionForm(conditionDistantHandHeartSeekingShotTrigger, ConditionForm.ConditionOperation.Add)
                        .Build(),
                    EffectFormBuilder
                        .Create()
                        .SetConditionForm(conditionDistantHandHeartSeekingShot, ConditionForm.ConditionOperation.Add)
                        .Build())
                .Build())
            .SetCustomSubFeatures(new ValidatorsPowerUse(ValidatorsCharacter.HasTwoHandedRangeWeapon))
            .AddToDB();

        Global.PowersThatIgnoreInterruptions.Add(powerDistantHandHeartSeekingShot);

        var powerDistantHandTurnOffHeartSeekingShot = FeatureDefinitionPowerBuilder
            .Create("PowerDistantHandTurnOffHeartSeekingShot")
            .SetGuiPresentationNoContent(true)
            .SetUsesFixed(ActivationTime.NoCost)
            .SetEffectDescription(EffectDescriptionBuilder
                .Create()
                .SetTargetingData(Side.Ally, RangeType.Self, 0, TargetType.Self)
                .SetDurationData(DurationType.Round, 1)
                .SetEffectForms(
                    EffectFormBuilder
                        .Create()
                        .SetConditionForm(
                            conditionDistantHandHeartSeekingShotTrigger,
                            ConditionForm.ConditionOperation.Remove)
                        .Build(),
                    EffectFormBuilder
                        .Create()
                        .SetConditionForm(conditionDistantHandHeartSeekingShot, ConditionForm.ConditionOperation.Remove)
                        .Build())
                .Build())
            .AddToDB();

        Global.PowersThatIgnoreInterruptions.Add(powerDistantHandTurnOffHeartSeekingShot);
        concentrationProvider.StopPower = powerDistantHandTurnOffHeartSeekingShot;

        return FeatureDefinitionFeatureSetBuilder
            .Create("FeatureSetDistantHandHeartSeekingShot")
            .SetGuiPresentation(Category.Feature)
            .AddFeatureSet(powerDistantHandHeartSeekingShot, powerDistantHandTurnOffHeartSeekingShot)
            .AddToDB();
    }

    internal override CharacterSubclassDefinition Subclass { get; }

    // private class ExtendWeaponRange : IModifyAttackModeForWeapon
    // {
    //     internal void ModifyAttackMode(RulesetCharacter character, RulesetAttackMode attackMode, RulesetItem weapon)
    //     {
    //         if (attackMode == null || attackMode.Magical || (!attackMode.Ranged && !attackMode.Thrown))
    //         {
    //             return;
    //         }
    //
    //         if (!Monk.IsMonkWeapon(character, attackMode))
    //         {
    //             return;
    //         }
    //
    //         attackMode.CloseRange = Math.Min(16, attackMode.CloseRange * 2);
    //         attackMode.MaxRange = Math.Min(32, attackMode.MaxRange * 2);
    //     }
    // }

    internal override FeatureDefinitionSubclassChoice SubclassChoice =>
        FeatureDefinitionSubclassChoices.SubclassChoiceMonkMonasticTraditions;

    private static bool IsMonkWeapon(RulesetAttackMode attackMode, RulesetItem weapon, RulesetCharacter character)
    {
        return IsMonkWeapon(character, attackMode) || IsMonkWeapon(character, weapon);
    }

    private static bool IsMonkWeapon(RulesetActor character, RulesetAttackMode attackMode)
    {
        return attackMode is { SourceDefinition: ItemDefinition item } && IsMonkWeapon(character, item);
    }

    private static bool IsMonkWeapon(RulesetActor actor, RulesetItem weapon)
    {
        return weapon == null || IsMonkWeapon(actor, weapon.ItemDefinition);
    }

    private static bool IsMonkWeapon(RulesetActor actor, ItemDefinition weapon)
    {
        return weapon != null && IsMonkWeapon(actor, weapon.WeaponDescription);
    }

    internal static bool IsMonkWeapon(RulesetActor actor, WeaponDescription weapon)
    {
        if (weapon == null)
        {
            return false;
        }

        return weapon.IsMonkWeaponOrUnarmed() || IsZenArcherWeapon(actor, weapon);
    }

    private static bool IsZenArcherWeapon(RulesetActor actor, ItemDefinition item)
    {
        return IsZenArcherWeapon(actor, item.WeaponDescription);
    }

    private static bool IsZenArcherWeapon(RulesetActor actor, WeaponDescription weapon)
    {
        if (actor == null || weapon == null)
        {
            return false;
        }

        var typeDefinition = weapon.WeaponTypeDefinition;

        if (typeDefinition == null)
        {
            return false;
        }

        return actor.HasSubFeatureOfType<ZenArcherMarker>() && ZenArcherWeapons.Contains(typeDefinition);
    }

    private static bool IsZenArrowAttack(RulesetAttackMode mode, RulesetItem weapon, RulesetCharacter character)
    {
        return mode != null
               && (mode.Ranged || mode.Thrown)
               && IsZenArcherWeapon(character, mode.SourceDefinition as ItemDefinition);
    }

    private static bool WieldsZenArcherWeapon(RulesetCharacter character)
    {
        var mainHandItem =
            character.CharacterInventory.InventorySlotsByName[EquipmentDefinitions.SlotTypeMainHand].EquipedItem;

        return IsZenArcherWeapon(character, mainHandItem?.ItemDefinition);
    }

    private sealed class ZenArcherMarker
    {
        // used for easier detection of Zen Archer characters to extend their Monk weapon list
    }

    private sealed class ZenArcherStunningArrows
    {
        // used for easier detection of Zen Archer characters to allow stunning strike on arrows
    }
}
