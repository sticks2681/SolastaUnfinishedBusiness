﻿using System.Linq;
using SolastaUnfinishedBusiness.Api.Extensions;
using SolastaUnfinishedBusiness.Api.Helpers;
using SolastaUnfinishedBusiness.Builders;
using SolastaUnfinishedBusiness.Builders.Features;
using SolastaUnfinishedBusiness.CustomBehaviors;
using SolastaUnfinishedBusiness.CustomInterfaces;
using SolastaUnfinishedBusiness.Models;
using static ActionDefinitions;
using static FeatureDefinitionAttributeModifier;
using static RuleDefinitions;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper.CharacterSubclassDefinitions;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper.SpellDefinitions;

namespace SolastaUnfinishedBusiness.Subclasses;

internal sealed class RangerWildMaster : AbstractSubclass
{
    private const string SpiritBeastTag = "SpiritBeast";
    private const string CommandSpiritBeastCondition = "ConditionWildMasterSpiritBeastCommand";
    private const string SummonSpiritBeastPower = "PowerWildMasterSummonSpiritBeast";

    internal RangerWildMaster()
    {
        #region COMMON

        var actionAffinitySpiritBeast =
            FeatureDefinitionActionAffinityBuilder
                .Create("ActionAffinityWildMasterSpiritBeast")
                .SetGuiPresentationNoContent()
                .SetDefaultAllowedActionTypes()
                .SetForbiddenActions(Id.AttackMain, Id.AttackOff, Id.AttackReadied, Id.AttackOpportunity, Id.Ready,
                    Id.PowerMain, Id.PowerBonus, Id.PowerReaction, Id.SpendPower)
                .SetCustomSubFeatures(new SummonerHasConditionOrKOd())
                .AddToDB();

        var combatAffinityWildMasterSummonerIsNextToBeast = FeatureDefinitionCombatAffinityBuilder
            .Create(FeatureDefinitionCombatAffinitys.CombatAffinityPackTactics,
                "CombatAffinityWildMasterSummonerIsNextToBeast")
            .SetSituationalContext(ExtraSituationalContext.SummonerIsNextToBeast)
            .AddToDB();

        var conditionAffinityWildMasterSpiritBeastInitiative =
            FeatureDefinitionConditionAffinityBuilder
                .Create("ConditionAffinityWildMasterSpiritBeastInitiative")
                .SetGuiPresentationNoContent()
                .SetConditionAffinityType(ConditionAffinityType.Immunity)
                .SetConditionType(ConditionDefinitions.ConditionSurprised)
                .SetCustomSubFeatures(ForceInitiativeToSummoner.Mark)
                .AddToDB();

        var perceptionAffinitySpiritBeast =
            FeatureDefinitionPerceptionAffinityBuilder
                .Create("PerceptionAffinityWildMasterSpiritBeast")
                .SetGuiPresentationNoContent()
                .CannotBeSurprised()
                .AddToDB();

        var powerWildMasterSummonSpiritBeastPool03 = FeatureDefinitionPowerBuilder
            .Create("PowerWildMasterSummonSpiritBeastPool03")
            .SetGuiPresentation("PowerWildMasterSummonSpiritBeastPool", Category.Feature,
                MonsterDefinitions.KindredSpiritWolf)
            .SetUsesFixed(ActivationTime.Action, RechargeRate.LongRest)
            .AddToDB();

        var powerWildMasterSummonSpiritBeastPool07 = FeatureDefinitionPowerBuilder
            .Create("PowerWildMasterSummonSpiritBeastPool07")
            .SetGuiPresentation("PowerWildMasterSummonSpiritBeastPool", Category.Feature,
                MonsterDefinitions.KindredSpiritWolf)
            .SetUsesFixed(ActivationTime.Action, RechargeRate.LongRest)
            .SetOverriddenPower(powerWildMasterSummonSpiritBeastPool03)
            .AddToDB();

        var powerWildMasterSummonSpiritBeastPool11 = FeatureDefinitionPowerBuilder
            .Create("PowerWildMasterSummonSpiritBeastPool11")
            .SetGuiPresentation("PowerWildMasterSummonSpiritBeastPool", Category.Feature,
                MonsterDefinitions.KindredSpiritWolf)
            .SetUsesFixed(ActivationTime.Action, RechargeRate.LongRest)
            .SetOverriddenPower(powerWildMasterSummonSpiritBeastPool07)
            .AddToDB();

        var powerWildMasterSummonSpiritBeastPool15 = FeatureDefinitionPowerBuilder
            .Create("PowerWildMasterSummonSpiritBeastPool15")
            .SetGuiPresentation("PowerWildMasterSummonSpiritBeastPool", Category.Feature,
                MonsterDefinitions.KindredSpiritWolf)
            .SetUsesFixed(ActivationTime.Action, RechargeRate.LongRest)
            .SetOverriddenPower(powerWildMasterSummonSpiritBeastPool11)
            .AddToDB();

        var powerWildMasterInvisibility = FeatureDefinitionPowerBuilder
            .Create(FeatureDefinitionPowers.PowerFunctionPotionOfInvisibility, "PowerWildMasterInvisibility")
            .SetOrUpdateGuiPresentation(Category.Feature)
            .SetUsesFixed(ActivationTime.BonusAction, RechargeRate.ShortRest, 2)
            .AddToDB();

        #endregion

        #region EAGLE

        var powerKindredSpiritEagle03 = BuildSpiritBeastPower(powerWildMasterSummonSpiritBeastPool03,
            MonsterDefinitions.KindredSpiritEagle, 3, false,
            FeatureDefinitionPowers.PowerDragonbornBreathWeaponBlue,
            CharacterContext.FeatureDefinitionPowerHelpAction,
            actionAffinitySpiritBeast,
            combatAffinityWildMasterSummonerIsNextToBeast,
            conditionAffinityWildMasterSpiritBeastInitiative);

        var powerKindredSpiritEagle07 = BuildSpiritBeastPower(powerWildMasterSummonSpiritBeastPool07,
            MonsterDefinitions.KindredSpiritEagle, 7, false,
            FeatureDefinitionPowers.PowerDragonbornBreathWeaponBlue,
            FeatureDefinitionPowers.PowerFiendishResilienceLightning,
            CharacterContext.FeatureDefinitionPowerHelpAction,
            actionAffinitySpiritBeast,
            combatAffinityWildMasterSummonerIsNextToBeast,
            conditionAffinityWildMasterSpiritBeastInitiative);

        var powerKindredSpiritEagle11 = BuildSpiritBeastPower(powerWildMasterSummonSpiritBeastPool11,
            MonsterDefinitions.KindredSpiritEagle, 11, true,
            FeatureDefinitionPowers.PowerDragonbornBreathWeaponBlue,
            FeatureDefinitionPowers.PowerFiendishResilienceLightning,
            powerWildMasterInvisibility,
            CharacterContext.FeatureDefinitionPowerHelpAction,
            actionAffinitySpiritBeast,
            combatAffinityWildMasterSummonerIsNextToBeast,
            conditionAffinityWildMasterSpiritBeastInitiative);

        var powerKindredSpiritEagle15 = BuildSpiritBeastPower(powerWildMasterSummonSpiritBeastPool15,
            MonsterDefinitions.KindredSpiritEagle, 15, true,
            FeatureDefinitionPowers.PowerDragonbornBreathWeaponBlue,
            FeatureDefinitionPowers.PowerFiendishResilienceLightning,
            powerWildMasterInvisibility,
            FeatureDefinitionPowers.PowerEyebitePanicked,
            CharacterContext.FeatureDefinitionPowerHelpAction,
            perceptionAffinitySpiritBeast,
            combatAffinityWildMasterSummonerIsNextToBeast,
            conditionAffinityWildMasterSpiritBeastInitiative);

        #endregion

        #region BEAR

        var powerKindredSpiritBear03 = BuildSpiritBeastPower(powerWildMasterSummonSpiritBeastPool03,
            MonsterDefinitions.KindredSpiritBear, 3, false,
            FeatureDefinitionPowers.PowerDragonbornBreathWeaponGold,
            CharacterContext.FeatureDefinitionPowerHelpAction,
            actionAffinitySpiritBeast,
            combatAffinityWildMasterSummonerIsNextToBeast,
            conditionAffinityWildMasterSpiritBeastInitiative);

        var powerKindredSpiritBear07 = BuildSpiritBeastPower(powerWildMasterSummonSpiritBeastPool07,
            MonsterDefinitions.KindredSpiritBear, 7, false,
            FeatureDefinitionPowers.PowerDragonbornBreathWeaponGold,
            FeatureDefinitionPowers.PowerFiendishResilienceFire,
            CharacterContext.FeatureDefinitionPowerHelpAction,
            actionAffinitySpiritBeast,
            combatAffinityWildMasterSummonerIsNextToBeast,
            conditionAffinityWildMasterSpiritBeastInitiative);

        var powerKindredSpiritBear11 = BuildSpiritBeastPower(powerWildMasterSummonSpiritBeastPool11,
            MonsterDefinitions.KindredSpiritBear, 11, true,
            FeatureDefinitionPowers.PowerDragonbornBreathWeaponGold,
            FeatureDefinitionPowers.PowerFiendishResilienceFire,
            powerWildMasterInvisibility,
            CharacterContext.FeatureDefinitionPowerHelpAction,
            actionAffinitySpiritBeast,
            combatAffinityWildMasterSummonerIsNextToBeast,
            conditionAffinityWildMasterSpiritBeastInitiative);

        var powerKindredSpiritBear15 = BuildSpiritBeastPower(powerWildMasterSummonSpiritBeastPool15,
            MonsterDefinitions.KindredSpiritBear, 15, true,
            FeatureDefinitionPowers.PowerDragonbornBreathWeaponGold,
            FeatureDefinitionPowers.PowerFiendishResilienceFire,
            powerWildMasterInvisibility,
            FeatureDefinitionPowers.PowerEyebitePanicked,
            CharacterContext.FeatureDefinitionPowerHelpAction,
            perceptionAffinitySpiritBeast,
            combatAffinityWildMasterSummonerIsNextToBeast,
            conditionAffinityWildMasterSpiritBeastInitiative);

        #endregion

        #region WOLF

        var powerKindredSpiritWolf03 = BuildSpiritBeastPower(powerWildMasterSummonSpiritBeastPool03,
            MonsterDefinitions.KindredSpiritWolf, 3, false,
            FeatureDefinitionPowers.PowerDragonbornBreathWeaponSilver,
            CharacterContext.FeatureDefinitionPowerHelpAction,
            actionAffinitySpiritBeast,
            combatAffinityWildMasterSummonerIsNextToBeast,
            conditionAffinityWildMasterSpiritBeastInitiative);

        var powerKindredSpiritWolf07 = BuildSpiritBeastPower(powerWildMasterSummonSpiritBeastPool07,
            MonsterDefinitions.KindredSpiritWolf, 7, false,
            FeatureDefinitionPowers.PowerDragonbornBreathWeaponSilver,
            FeatureDefinitionPowers.PowerFiendishResilienceCold,
            CharacterContext.FeatureDefinitionPowerHelpAction,
            actionAffinitySpiritBeast,
            combatAffinityWildMasterSummonerIsNextToBeast,
            conditionAffinityWildMasterSpiritBeastInitiative);

        var powerKindredSpiritWolf11 = BuildSpiritBeastPower(powerWildMasterSummonSpiritBeastPool11,
            MonsterDefinitions.KindredSpiritWolf, 11, true,
            FeatureDefinitionPowers.PowerDragonbornBreathWeaponSilver,
            FeatureDefinitionPowers.PowerFiendishResilienceCold,
            powerWildMasterInvisibility,
            CharacterContext.FeatureDefinitionPowerHelpAction,
            actionAffinitySpiritBeast,
            combatAffinityWildMasterSummonerIsNextToBeast,
            conditionAffinityWildMasterSpiritBeastInitiative);

        var powerKindredSpiritWolf15 = BuildSpiritBeastPower(powerWildMasterSummonSpiritBeastPool15,
            MonsterDefinitions.KindredSpiritWolf, 15, true,
            FeatureDefinitionPowers.PowerDragonbornBreathWeaponSilver,
            FeatureDefinitionPowers.PowerFiendishResilienceCold,
            powerWildMasterInvisibility,
            FeatureDefinitionPowers.PowerEyebiteAsleep,
            CharacterContext.FeatureDefinitionPowerHelpAction,
            perceptionAffinitySpiritBeast,
            combatAffinityWildMasterSummonerIsNextToBeast,
            conditionAffinityWildMasterSpiritBeastInitiative);

        #endregion

        #region SUBCLASS

        var featureSetWildMasterBeastIsNextToSummoner = FeatureDefinitionBuilder
            .Create("FeatureWildMasterBeastIsNextToSummoner")
            .SetGuiPresentation(Category.Feature)
            .AddToDB();

        var featureSetWildMaster03 = FeatureDefinitionFeatureSetBuilder
            .Create("FeatureSetWildMaster03")
            .SetGuiPresentation(Category.Feature)
            .AddFeatureSet(
                BuildCommandSpiritBeast(),
                BuildPowerWildMasterSpiritBeastRecuperate(),
                BuildSpiritBeastAffinityLevel03(),
                powerWildMasterSummonSpiritBeastPool03,
                powerKindredSpiritBear03,
                powerKindredSpiritEagle03,
                powerKindredSpiritWolf03)
            .AddToDB();

        var featureSetWildMaster07 = FeatureDefinitionFeatureSetBuilder
            .Create("FeatureSetWildMaster07")
            .SetGuiPresentation(Category.Feature)
            .AddFeatureSet(
                BuildSpiritBeastAffinityLevel07(),
                powerWildMasterSummonSpiritBeastPool07,
                powerKindredSpiritBear07,
                powerKindredSpiritEagle07,
                powerKindredSpiritWolf07)
            .AddToDB();

        var featureSetWildMaster11 = FeatureDefinitionFeatureSetBuilder
            .Create("FeatureSetWildMaster11")
            .SetGuiPresentation(Category.Feature)
            .AddFeatureSet(
                BuildSpiritBeastAffinityLevel11(),
                powerWildMasterSummonSpiritBeastPool11,
                powerKindredSpiritBear11,
                powerKindredSpiritEagle11,
                powerKindredSpiritWolf11)
            .AddToDB();

        var featureSetWildMaster15 = FeatureDefinitionFeatureSetBuilder
            .Create("FeatureSetWildMaster15")
            .SetGuiPresentation(Category.Feature)
            .AddFeatureSet(
                powerWildMasterSummonSpiritBeastPool15,
                powerKindredSpiritBear15,
                powerKindredSpiritEagle15,
                powerKindredSpiritWolf15)
            .AddToDB();

        Subclass = CharacterSubclassDefinitionBuilder
            .Create("RangerWildMaster")
            .SetGuiPresentation(Category.Subclass, PatronFiend)
            .AddFeaturesAtLevel(3,
                featureSetWildMaster03,
                featureSetWildMasterBeastIsNextToSummoner)
            .AddFeaturesAtLevel(7,
                featureSetWildMaster07)
            .AddFeaturesAtLevel(11,
                featureSetWildMaster11)
            .AddFeaturesAtLevel(15,
                featureSetWildMaster15)
            .AddToDB();

        #endregion

        PowerBundle.RegisterPowerBundle(powerWildMasterSummonSpiritBeastPool03, true,
            powerKindredSpiritBear03,
            powerKindredSpiritEagle03,
            powerKindredSpiritWolf03);

        PowerBundle.RegisterPowerBundle(powerWildMasterSummonSpiritBeastPool07, true,
            powerKindredSpiritBear07,
            powerKindredSpiritEagle07,
            powerKindredSpiritWolf07);

        PowerBundle.RegisterPowerBundle(powerWildMasterSummonSpiritBeastPool11, true,
            powerKindredSpiritBear11,
            powerKindredSpiritEagle11,
            powerKindredSpiritWolf11);

        PowerBundle.RegisterPowerBundle(powerWildMasterSummonSpiritBeastPool15, true,
            powerKindredSpiritBear15,
            powerKindredSpiritEagle15,
            powerKindredSpiritWolf15);

        // required to avoid beast duplicates when they get upgraded from 6 to 7, 10 to 11, 14 to 15
        GlobalUniqueEffects.AddToGroup(GlobalUniqueEffects.Group.WildMasterBeast,
            powerKindredSpiritBear03,
            powerKindredSpiritEagle03,
            powerKindredSpiritWolf03,
            powerKindredSpiritBear07,
            powerKindredSpiritEagle07,
            powerKindredSpiritWolf07,
            powerKindredSpiritBear11,
            powerKindredSpiritEagle11,
            powerKindredSpiritWolf11,
            powerKindredSpiritBear15,
            powerKindredSpiritEagle15,
            powerKindredSpiritWolf15);

        //
        // required for a better UI presentation on level 15
        //

        FeatureDefinitionPowers.PowerEyebiteAsleep.guiPresentation.spriteReference =
            EyebiteAsleep.guiPresentation.spriteReference;

        FeatureDefinitionPowers.PowerEyebitePanicked.guiPresentation.spriteReference =
            EyebitePanicked.guiPresentation.spriteReference;

        FeatureDefinitionPowers.PowerEyebiteSickened.guiPresentation.spriteReference =
            EyebiteSickened.guiPresentation.spriteReference;
    }

    internal override CharacterSubclassDefinition Subclass { get; }

    internal override FeatureDefinitionSubclassChoice SubclassChoice =>
        FeatureDefinitionSubclassChoices.SubclassChoiceRangerArchetypes;

    private static FeatureDefinition BuildPowerWildMasterSpiritBeastRecuperate()
    {
        const string NAME = "PowerWildMasterSpiritBeastRecuperate";

        RestActivityDefinitionBuilder
            .Create("RestActivityWildMasterSpiritBeastRecuperate")
            .SetGuiPresentation(NAME, Category.Feature)
            .SetRestData(
                RestDefinitions.RestStage.AfterRest,
                RestType.ShortRest,
                RestActivityDefinition.ActivityCondition.CanUsePower,
                PowerBundleContext.UseCustomRestPowerFunctorName,
                NAME)
            .AddToDB();

        var powerWildMasterSpiritBeastRecuperate = FeatureDefinitionPowerBuilder
            .Create(NAME)
            .SetGuiPresentation(Category.Feature)
            .SetCustomSubFeatures(
                PowerVisibilityModifier.Hidden,
                HasModifiedUses.Marker,
                new ValidatorsPowerUse(HasInjuredBeast),
                new ModifyRestPowerTitleHandler(GetRestPowerTitle),
                new RetargetSpiritBeast())
            .SetUsesFixed(ActivationTime.Rest, RechargeRate.LongRest, 1, 0)
            .SetEffectDescription(EffectDescriptionBuilder.Create()
                .SetTargetingData(Side.Ally, RangeType.Self, 0, TargetType.Self)
                .SetEffectForms(EffectFormBuilder.Create()
                    .SetHealingForm(HealingComputation.Dice, 0, DieType.D8, 1, false, HealingCap.MaximumHitPoints)
                    .Build())
                .Build())
            .AddToDB();

        powerWildMasterSpiritBeastRecuperate.AddCustomSubFeatures(new PowerUseModifier
        {
            PowerPool = powerWildMasterSpiritBeastRecuperate,
            Type = PowerPoolBonusCalculationType.ClassLevel,
            Attribute = RangerClass
        });

        return powerWildMasterSpiritBeastRecuperate;
    }

    private static RulesetCharacter GetSpiritBeast(RulesetCharacter character)
    {
        var spiritBeastEffect =
            character.powersUsedByMe.Find(p => p.sourceDefinition.Name.StartsWith(SummonSpiritBeastPower));
        var summons = EffectHelpers.GetSummonedCreatures(spiritBeastEffect);

        return summons.Empty() ? null : summons[0];
    }

    private static bool HasInjuredBeast(RulesetCharacter character)
    {
        var spiritBeast = GetSpiritBeast(character);

        return spiritBeast is { IsMissingHitPoints: true };
    }

    private static string GetRestPowerTitle(RulesetCharacter character)
    {
        var spiritBeast = GetSpiritBeast(character);

        if (spiritBeast == null)
        {
            return string.Empty;
        }

        return Gui.Format("Feature/&PowerWildMasterSpiritBeastRecuperateFormat",
            spiritBeast.CurrentHitPoints.ToString(),
            spiritBeast.TryGetAttributeValue(AttributeDefinitions.HitPoints).ToString());
    }

    private static FeatureDefinitionPower BuildSpiritBeastPower(
        FeatureDefinitionPower sharedPoolPower,
        MonsterDefinition monsterDefinition,
        int level,
        bool groupAttacks,
        params FeatureDefinition[] monsterAdditionalFeatures)
    {
        var name = SummonSpiritBeastPower + monsterDefinition.name + level;
        var spiritBeast = BuildSpiritBeastMonster(monsterDefinition, level, groupAttacks, monsterAdditionalFeatures);

        var title =
            Gui.Format("Feature/&PowerWildMasterSummonSpiritBeastTitle", spiritBeast.FormatTitle());
        var description =
            Gui.Format("Feature/&PowerWildMasterSummonSpiritBeastDescription", spiritBeast.FormatTitle());

        return FeatureDefinitionPowerSharedPoolBuilder
            .Create(name)
            .SetGuiPresentation(title, description, monsterDefinition, true)
            .SetSharedPool(ActivationTime.Action, sharedPoolPower)
            .SetEffectDescription(EffectDescriptionBuilder
                .Create()
                .SetDurationData(DurationType.Permanent)
                .SetTargetingData(Side.Ally, RangeType.Distance, 3, TargetType.Position)
                .SetEffectForms(
                    EffectFormBuilder
                        .Create()
                        .SetSummonCreatureForm(1, spiritBeast.Name)
                        .Build())
                .SetParticleEffectParameters(ConjureElementalAir)
                .Build())
            .SetUniqueInstance()
            .SetCustomSubFeatures(SkipEffectRemovalOnLocationChange.Always, ValidatorsPowerUse.NotInCombat)
            .AddToDB();
    }

    private static MonsterDefinition BuildSpiritBeastMonster(
        MonsterDefinition monsterDefinition,
        int level,
        bool groupAttacks,
        params FeatureDefinition[] monsterAdditionalFeatures)
    {
        return MonsterDefinitionBuilder
            .Create(monsterDefinition, "WildMasterSpiritBeast" + monsterDefinition.name + level)
            .AddFeatures(monsterAdditionalFeatures)
            .SetCreatureTags(SpiritBeastTag)
            .SetChallengeRating(0)
            .SetFullyControlledWhenAllied(true)
            .NoExperienceGain()
            .SetGroupAttacks(groupAttacks)
            .AddToDB();
    }

    private static FeatureDefinition BuildSpiritBeastAffinityLevel03()
    {
        var acBonus = FeatureDefinitionAttributeModifierBuilder
            .Create("AttributeModifierWildMasterSummonSpiritBeastAC")
            .SetGuiPresentationNoContent()
            .SetModifier(AttributeModifierOperation.AddConditionAmount, AttributeDefinitions.ArmorClass)
            .AddToDB();

        var hpBonus = FeatureDefinitionAttributeModifierBuilder
            .Create("AttributeModifierWildMasterSummonSpiritBeastHP")
            .SetGuiPresentationNoContent()
            .SetModifier(AttributeModifierOperation.AddConditionAmount, AttributeDefinitions.HitPoints)
            .AddToDB();

        var toHit = FeatureDefinitionAttackModifierBuilder
            .Create("AttackModifierWildMasterSummonSpiritBeastToHit")
            .SetGuiPresentationNoContent()
            .SetAttackRollModifier(1, AttackModifierMethod.SourceConditionAmount)
            .AddToDB();

        var toDamage = FeatureDefinitionAttackModifierBuilder
            .Create("AttackModifierWildMasterSummonSpiritBeastDamage")
            .SetGuiPresentationNoContent()
            .SetDamageRollModifier(1, AttackModifierMethod.SourceConditionAmount)
            .AddToDB();

        return FeatureDefinitionSummoningAffinityBuilder
            .Create("SummoningAffinityWildMasterSummonSpiritBeast03")
            .SetGuiPresentationNoContent()
            .SetRequiredMonsterTag(SpiritBeastTag)
            .SetAddedConditions(
                ConditionDefinitionBuilder
                    .Create("ConditionWildMasterSummonSpiritBeastAcBonus")
                    .SetGuiPresentation("Condition/&ConditionWildMasterSummonSpiritBeastBonusTitle", Gui.NoLocalization)
                    .SetSilent(Silent.WhenAddedOrRemoved)
                    .SetPossessive()
                    .SetAmountOrigin(ConditionDefinition.OriginOfAmount.SourceSpellCastingAbility)
                    .SetFeatures(acBonus)
                    .AddToDB(),
                ConditionDefinitionBuilder
                    .Create("ConditionWildMasterSummonSpiritBeastSourceProficiencyBonusToHit")
                    .SetGuiPresentation("Condition/&ConditionWildMasterSummonSpiritBeastBonusTitle", Gui.NoLocalization)
                    .SetSilent(Silent.WhenAddedOrRemoved)
                    .SetPossessive()
                    .SetAmountOrigin(ExtraOriginOfAmount.SourceProficiencyAndAbilityBonus, AttributeDefinitions.Wisdom)
                    .SetFeatures(toHit)
                    .AddToDB(),
                ConditionDefinitionBuilder
                    .Create("ConditionWildMasterSummonSpiritBeastProficiencyBonusToDamage")
                    .SetGuiPresentation("Condition/&ConditionWildMasterSummonSpiritBeastBonusTitle", Gui.NoLocalization)
                    .SetSilent(Silent.WhenAddedOrRemoved)
                    .SetPossessive()
                    .SetAmountOrigin(ExtraOriginOfAmount.SourceProficiencyAndAbilityBonus, AttributeDefinitions.Wisdom)
                    .SetFeatures(toDamage)
                    .AddToDB(),
                ConditionDefinitionBuilder
                    .Create("ConditionWildMasterSummonSpiritBeastCopyCharacterLevel")
                    .SetGuiPresentationNoContent()
                    .SetSilent(Silent.WhenAddedOrRemoved)
                    .SetAmountOrigin(ExtraOriginOfAmount.SourceCopyAttributeFromSummoner,
                        AttributeDefinitions.CharacterLevel)
                    .AddToDB(),
                ConditionDefinitionBuilder
                    .Create("ConditionWildMasterSummonSpiritBeastLevel")
                    .SetGuiPresentationNoContent()
                    .SetSilent(Silent.WhenAddedOrRemoved)
                    .SetAmountOrigin(ExtraOriginOfAmount.SourceClassLevel, RangerClass)
                    .SetFeatures(hpBonus, hpBonus, hpBonus, hpBonus) // 4 HP per level
                    .AddToDB())
            .AddToDB();
    }

    private static FeatureDefinition BuildSpiritBeastAffinityLevel07()
    {
        return FeatureDefinitionSummoningAffinityBuilder
            .Create(FeatureDefinitionSummoningAffinitys.SummoningAffinityKindredSpiritMagicalSpirit,
                "SummoningAffinityWildMasterSummonSpiritBeast07")
            .SetRequiredMonsterTag(SpiritBeastTag)
            .AddToDB();
    }

    private static FeatureDefinition BuildSpiritBeastAffinityLevel11()
    {
        return FeatureDefinitionSummoningAffinityBuilder
            .Create("SummoningAffinityWildMasterSummonSpiritBeast11")
            .SetGuiPresentationNoContent()
            .SetRequiredMonsterTag(SpiritBeastTag)
            .SetAddedConditions(
                ConditionDefinitionBuilder
                    .Create("ConditionWildMasterSummonSpiritBeastSaving")
                    .SetGuiPresentationNoContent()
                    .SetSilent(Silent.WhenAddedOrRemoved)
                    .SetAmountOrigin(ConditionDefinition.OriginOfAmount.SourceSpellAttack)
                    .SetFeatures(
                        FeatureDefinitionSavingThrowAffinityBuilder
                            .Create("SavingThrowAffinityWildMasterSummonSpiritBeast")
                            .SetGuiPresentationNoContent()
                            .SetCustomSubFeatures(new AddPBToSummonCheck(1,
                                AttributeDefinitions.Strength,
                                AttributeDefinitions.Dexterity,
                                AttributeDefinitions.Constitution,
                                AttributeDefinitions.Intelligence,
                                AttributeDefinitions.Wisdom,
                                AttributeDefinitions.Charisma))
                            .AddToDB())
                    .AddToDB())
            .AddToDB();
    }

    private static FeatureDefinition BuildCommandSpiritBeast()
    {
        var condition = ConditionDefinitionBuilder
            .Create(CommandSpiritBeastCondition)
            .SetGuiPresentationNoContent()
            .SetSilent(Silent.WhenAddedOrRemoved)
            .SetDuration(DurationType.Round, 1)
            .SetSpecialDuration()
            .SetTurnOccurence(TurnOccurenceType.StartOfTurn)
            .AddToDB();

        var powerWildMasterSpiritBeastCommand = FeatureDefinitionPowerBuilder
            .Create("PowerWildMasterSpiritBeastCommand")
            .SetGuiPresentation(Category.Feature, Command)
            .SetUsesFixed(ActivationTime.BonusAction)
            .SetEffectDescription(EffectDescriptionBuilder.Create()
                .SetTargetingData(Side.Ally, RangeType.Self, 0, TargetType.Self)
                .SetEffectForms(
                    EffectFormBuilder
                        .Create()
                        .SetConditionForm(condition, ConditionForm.ConditionOperation.Add)
                        .Build())
                .Build())
            .SetCustomSubFeatures(new ShowInCombatWhenHasSpiritBeast())
            .AddToDB();

        powerWildMasterSpiritBeastCommand.AddCustomSubFeatures(
            new ApplyOnTurnEnd(condition, powerWildMasterSpiritBeastCommand));

        return powerWildMasterSpiritBeastCommand;
    }

    private class SummonerHasConditionOrKOd : IDefinitionApplicationValidator, ICharacterTurnStartListener
    {
        public void OnCharacterTurnStarted(GameLocationCharacter locationCharacter)
        {
            // if commanded allow anything
            if (IsCommanded(locationCharacter.RulesetCharacter))
            {
                return;
            }

            // force dodge action if not at level 7 yet
            if (locationCharacter.RulesetCharacter.GetMySummoner()?.RulesetCharacter is RulesetCharacterHero hero
                && hero.ClassesAndLevels[CharacterClassDefinitions.Ranger] < 7)
            {
                ServiceRepository.GetService<ICommandService>()
                    ?.ExecuteAction(new CharacterActionParams(locationCharacter, Id.Dodge), null, false);
            }
        }

        public bool IsValid(BaseDefinition definition, RulesetCharacter character)
        {
            //Apply limits if not commanded
            return !IsCommanded(character);
        }

        private static bool IsCommanded(RulesetCharacter character)
        {
            //can act freely outside of battle
            if (Gui.Battle == null)
            {
                return true;
            }

            var summoner = character.GetMySummoner()?.RulesetCharacter;

            //shouldn't happen, but consider being commanded in this case
            if (summoner == null)
            {
                return true;
            }

            //can act if summoner is KO
            return summoner.IsUnconscious ||
                   //can act if summoner commanded
                   summoner.HasConditionOfType(CommandSpiritBeastCondition);
        }
    }

    private class ApplyOnTurnEnd : ICharacterTurnEndListener
    {
        private readonly ConditionDefinition condition;
        private readonly FeatureDefinitionPower power;

        public ApplyOnTurnEnd(ConditionDefinition condition, FeatureDefinitionPower power)
        {
            this.condition = condition;
            this.power = power;
        }

        public void OnCharacterTurnEnded(GameLocationCharacter locationCharacter)
        {
            var status = locationCharacter.GetActionStatus(Id.PowerBonus, ActionScope.Battle);

            if (status != ActionStatus.Available)
            {
                return;
            }

            var character = locationCharacter.RulesetCharacter;
            var newCondition = RulesetCondition.CreateActiveCondition(character.Guid, condition, DurationType.Round, 1,
                TurnOccurenceType.StartOfTurn, locationCharacter.Guid, character.CurrentFaction.Name);

            character.AddConditionOfCategory(AttributeDefinitions.TagCombat, newCondition);
            GameConsoleHelper.LogCharacterUsedPower(character, power);
        }
    }

    private class ShowInCombatWhenHasSpiritBeast : IPowerUseValidity
    {
        public bool CanUsePower(RulesetCharacter character, FeatureDefinitionPower featureDefinitionPower)
        {
            return ServiceRepository.GetService<IGameLocationBattleService>().IsBattleInProgress &&
                   character.powersUsedByMe.Any(p => p.sourceDefinition.Name.StartsWith(SummonSpiritBeastPower));
        }
    }

    private class RetargetSpiritBeast : IRetargetCustomRestPower
    {
        public GameLocationCharacter GetTarget(RulesetCharacter character)
        {
            var spiritBeast = GetSpiritBeast(character);

            return spiritBeast == null ? null : GameLocationCharacter.GetFromActor(spiritBeast);
        }
    }
}
