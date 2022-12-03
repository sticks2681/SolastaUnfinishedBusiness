using System;
using System.Collections.Generic;
using System.Linq;
using SolastaUnfinishedBusiness.Api;
using SolastaUnfinishedBusiness.Api.Extensions;
using SolastaUnfinishedBusiness.Api.Helpers;
using SolastaUnfinishedBusiness.Builders;
using SolastaUnfinishedBusiness.Builders.Features;
using SolastaUnfinishedBusiness.Classes.Armathor.Subclasses;
using SolastaUnfinishedBusiness.CustomBehaviors;
using SolastaUnfinishedBusiness.CustomDefinitions;
using SolastaUnfinishedBusiness.CustomInterfaces;
using SolastaUnfinishedBusiness.CustomUI;
using SolastaUnfinishedBusiness.Feats;
using SolastaUnfinishedBusiness.Properties;
using UnityEngine.AddressableAssets;
using static RuleDefinitions;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper.CharacterClassDefinitions;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper.FeatureDefinitionActionAffinitys;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper.FeatureDefinitionAdditionalDamages;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper.FeatureDefinitionAttributeModifiers;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper.FeatureDefinitionDieRollModifiers;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper.FeatureDefinitionFeatureSets;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper.FeatureDefinitionPowers;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper.FeatureDefinitionProficiencys;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper.FeatureDefinitionSavingThrowAffinitys;

namespace SolastaUnfinishedBusiness.Classes.Armathor;

internal static class ArmathorClass
{
    public const string ClassName = "Armathor";

    private static readonly AssetReferenceSprite Sprite =
        Sprites.GetSprite("Inventor", Resources.Inventor, 1024, 576);

    internal static readonly AssetReferenceSprite Pictogram =
        Sprites.GetSprite("InventorPictogram", Resources.InventorPictogram, 128);

    private static SpellListDefinition _spellList;

    internal static CharacterClassDefinition Class { get; private set; }

    public static SpellListDefinition SpellList => _spellList ??= BuildSpellList();

    private static FeatureDefinitionCastSpell SpellCasting { get; set; }

    public static CharacterClassDefinition Build()
    {
        if (Class != null)
        {
            throw new ArgumentException("Trying to build Armathor class additional time.");
        }

        SpellCasting = BuildSpellCasting();

        var builder = CharacterClassDefinitionBuilder
            .Create(ClassName)

        #region Presentation

            .SetGuiPresentation(Category.Class, Sprite)
            .SetAnimationId(AnimationDefinitions.ClassAnimationId.Paladin)
            .SetPictogram(Pictogram);

        Paladin.personalityFlagOccurences
            .ForEach(fo => builder.AddPersonality(fo.personalityFlag, fo.weight));

        #endregion

        #region Priorities

        builder
            .SetAbilityScorePriorities(
                AttributeDefinitions.Strength,
                AttributeDefinitions.Charisma,
                AttributeDefinitions.Constitution,
                AttributeDefinitions.Dexterity,
                AttributeDefinitions.Wisdom,
                AttributeDefinitions.Intelligence
            )
            .AddSkillPreferences(
                DatabaseHelper.SkillDefinitions.Persuasion,
                DatabaseHelper.SkillDefinitions.Religion,
                DatabaseHelper.SkillDefinitions.Athletics,
                DatabaseHelper.SkillDefinitions.Insight,
                DatabaseHelper.SkillDefinitions.Intimidation,
                DatabaseHelper.SkillDefinitions.Medecine
            )
            .AddToolPreferences(
                ToolTypeDefinitions.HerbalismKitType
            )
            //TODO: Add more preferred feats
            .AddFeatPreferences(
                "Robust",
                "Robust")

        #endregion

            .SetBattleAI(DecisionPackageDefinitions.DefaultMeleeWithBackupRangeDecisions)
            .SetIngredientGatheringOdds(Paladin.IngredientGatheringOdds)
            .SetHitDice(DieType.D10)

        #region Equipment

            .AddEquipmentRow(
                new List<CharacterClassDefinition.HeroEquipmentOption>
                {
                    EquipmentOptionsBuilder.Option(ItemDefinitions.Mace,
                        EquipmentDefinitions.OptionWeaponSimpleChoice, 1),
                    EquipmentOptionsBuilder.Option(ItemDefinitions.Shield,
                        EquipmentDefinitions.OptionArmor, 1)
                },
                new List<CharacterClassDefinition.HeroEquipmentOption>
                {
                    EquipmentOptionsBuilder.Option(ItemDefinitions.Dagger,
                        EquipmentDefinitions.OptionWeaponSimpleChoice, 1),
                    EquipmentOptionsBuilder.Option(ItemDefinitions.Dagger,
                        EquipmentDefinitions.OptionWeaponSimpleChoice, 1)
                }
            )
            .AddEquipmentRow(
                EquipmentOptionsBuilder.Option(ItemDefinitions.LightCrossbow,
                    EquipmentDefinitions.OptionWeapon, 1),
                EquipmentOptionsBuilder.Option(ItemDefinitions.Bolt,
                    EquipmentDefinitions.OptionAmmoPack, 1)
            )
            .AddEquipmentRow(
                new List<CharacterClassDefinition.HeroEquipmentOption>
                {
                    EquipmentOptionsBuilder.Option(ItemDefinitions.ComponentPouch,
                        EquipmentDefinitions.OptionFocus, 1)
                },
                new List<CharacterClassDefinition.HeroEquipmentOption>
                {
                    EquipmentOptionsBuilder.Option(ItemDefinitions.ComponentPouch_Belt,
                        EquipmentDefinitions.OptionFocus, 1)
                },
                new List<CharacterClassDefinition.HeroEquipmentOption>
                {
                    EquipmentOptionsBuilder.Option(ItemDefinitions.ComponentPouch_Bracers,
                        EquipmentDefinitions.OptionFocus, 1)
                }
            )
            .AddEquipmentRow(new List<CharacterClassDefinition.HeroEquipmentOption>
                {
                    EquipmentOptionsBuilder.Option(ItemDefinitions.StuddedLeather,
                        EquipmentDefinitions.OptionArmor, 1)
                },
                new List<CharacterClassDefinition.HeroEquipmentOption>
                {
                    EquipmentOptionsBuilder.Option(ItemDefinitions.ScaleMail,
                        EquipmentDefinitions.OptionArmor, 1)
                }
            )
            .AddEquipmentRow(
                new List<CharacterClassDefinition.HeroEquipmentOption>
                {
                    EquipmentOptionsBuilder.Option(ItemDefinitions.DungeoneerPack,
                        EquipmentDefinitions.OptionStarterPack, 1)
                },
                new List<CharacterClassDefinition.HeroEquipmentOption>
                {
                    EquipmentOptionsBuilder.Option(ItemDefinitions.ExplorerPack,
                        EquipmentDefinitions.OptionStarterPack, 1)
                }
            )

        #endregion

        #region Proficiencies

            // Weapons
            .AddFeaturesAtLevel(1, FeatureDefinitionProficiencyBuilder
                .Create("ProficiencyArmathorWeapon")
                .SetGuiPresentation(Category.Feature, "Feature/&WeaponTrainingShortDescription")
                .SetProficiencies(ProficiencyType.Weapon,
                    EquipmentDefinitions.SimpleWeaponCategory,
                    EquipmentDefinitions.MartialWeaponCategory)
                .AddToDB())

            // Armor
            .AddFeaturesAtLevel(1, FeatureDefinitionProficiencyBuilder
                .Create("ProficiencyArmathorArmor")
                .SetGuiPresentation(Category.Feature, "Feature/&ArmorTrainingShortDescription")
                .SetProficiencies(ProficiencyType.Armor,
                    EquipmentDefinitions.LightArmorCategory,
                    EquipmentDefinitions.MediumArmorCategory,
                    EquipmentDefinitions.HeavyArmorCategory,
                    EquipmentDefinitions.ShieldCategory)
                .AddToDB())

            // Saves
            .AddFeaturesAtLevel(1, FeatureDefinitionProficiencyBuilder
                .Create("ProficiencyArmathorSavingThrow")
                .SetGuiPresentation("SavingThrowProficiency", Category.Feature)
                .SetProficiencies(ProficiencyType.SavingThrow,
                    AttributeDefinitions.Wisdom,
                    AttributeDefinitions.Charisma)
                .AddToDB())

            // Tools Proficiency
            .AddFeaturesAtLevel(1, FeatureDefinitionProficiencyBuilder
                .Create("ProficiencyArmathorTools")
                .SetGuiPresentation(Category.Feature, "Feature/&ToolProficiencyPluralDescription")
                .SetProficiencies(ProficiencyType.Tool,
                    ToolTypeDefinitions.ArtisanToolSmithToolsType.Name,
                    ToolTypeDefinitions.EnchantingToolType.Name,
                    ToolTypeDefinitions.HerbalismKitType.Name,
                    ToolTypeDefinitions.ScrollKitType.Name)
                .AddToDB())

            // Skill points
            .AddFeaturesAtLevel(1, FeatureDefinitionPointPoolBuilder
                .Create("PointPoolArmathorSkills")
                .SetGuiPresentation(Category.Feature, "Feature/&SkillGainChoicesPluralDescription")
                .SetPool(HeroDefinitions.PointsPoolType.Skill, 13)
                .OnlyUniqueChoices()
                .RestrictChoices(
                    SkillDefinitions.Acrobatics,
                    SkillDefinitions.AnimalHandling,
                    SkillDefinitions.Arcana,
                    SkillDefinitions.Athletics,
                    SkillDefinitions.Deception,
                    SkillDefinitions.History,
                    SkillDefinitions.Insight,
                    SkillDefinitions.Intimidation,
                    SkillDefinitions.Investigation,
                    SkillDefinitions.Medecine,
                    SkillDefinitions.Nature,
                    SkillDefinitions.Perception,
                    SkillDefinitions.Performance,
                    SkillDefinitions.Persuasion,
                    SkillDefinitions.Religion,
                    SkillDefinitions.SleightOfHand,
                    SkillDefinitions.Stealth,
                    SkillDefinitions.Survival)
                .AddToDB())

            // Tools Proficiency
            .AddFeaturesAtLevel(1, FeatureDefinitionProficiencyBuilder
                .Create("ProficiencyArmathorFighting")
                .SetGuiPresentation(Category.Feature, "Feature/&ArmathorFightingStylesDescription")
                .SetProficiencies(ProficiencyType.FightingStyle,
                    FightingStyleDefinitions.BlindFighting.Name,
                    FightingStyleDefinitions.TwoWeapon.Name)
                .AddToDB())

        #endregion

        #region Level 01

            .AddFeaturesAtLevel(1,
                SpellCasting,
                BuildBonusCantrips(),
                BuildRitualCasting(),
                AttributeModifierPaladinHealingPoolBase,
                AttributeModifierPaladinHealingPoolMultiplier,
                FeatureSetMonkPurityOfBody,
                FeatureSetMonkStillnessOfMind,
                FeatureSetMonkTimelessBody,
                PowerFighterSecondWind,
                PowerPaladinCureDisease,
                PowerPaladinDivineSense,
                PowerPaladinLayOnHands,
                PowerPaladinNeutralizePoison
            )

        #endregion

        #region Level 02

            .AddFeaturesAtLevel(2,
                ActionAffinityRogueCunningAction,
                AdditionalDamagePaladinDivineSmite,
                AttributeModifierClericChannelDivinity,
                PowerFighterActionSurge,
                PowerWizardArcaneRecovery
            )

        #endregion

        #region Level 03

        #endregion

        #region Level 04

            .AddFeaturesAtLevel(4,
                FeatureDefinitionFeatureSets.FeatureSetAbilityScoreChoice
            )

        #endregion

        #region Level 05

            .AddFeaturesAtLevel(5,
                AttributeModifierFighterExtraAttack
            )

        #endregion

        #region Level 06

            .AddFeaturesAtLevel(6,
                AttributeModifierClericChannelDivinityAdd,
                PowerPaladinAuraOfProtection
            )

        #endregion

        #region Level 07

            .AddFeaturesAtLevel(7,
                SavingThrowAffinityMonkEvasion
            )

        #endregion

        #region Level 08

            .AddFeaturesAtLevel(8,
                FeatureDefinitionFeatureSets.FeatureSetAbilityScoreChoice
            )

        #endregion

        #region Level 09

            .AddFeaturesAtLevel(9,
                AttributeModifierFighterIndomitable,
                ProficiencyMonkDiamondSoulSavingThrow
            )

        #endregion

        #region Level 10

            .AddFeaturesAtLevel(10,
                PowerPaladinAuraOfCourage
            )

        #endregion

        #region Level 11

            .AddFeaturesAtLevel(11,
                AdditionalDamagePaladinImprovedDivineSmite,
                AttributeModifierFighterExtraAttack,
                DieRollModifierRogueReliableTalent
            )

        #endregion

        #region Level 12

            .AddFeaturesAtLevel(12,
                FeatureDefinitionFeatureSets.FeatureSetAbilityScoreChoice
            )

        #endregion

        #region Level 13

        #endregion

        #region Level 14

        #endregion

        #region Level 15

        #endregion

        #region Level 16

            .AddFeaturesAtLevel(16,
                FeatureDefinitionFeatureSets.FeatureSetAbilityScoreChoice
            )

        #endregion

        #region Level 17

        #endregion

        #region Level 18

        #endregion

        #region Level 19

            .AddFeaturesAtLevel(19,
                FeatureDefinitionFeatureSets.FeatureSetAbilityScoreChoice
            );

        #endregion

        #region Level 20

        #endregion

        Class = builder.AddToDB();

        #region Subclasses

        builder.AddFeaturesAtLevel(1, FeatureDefinitionSubclassChoiceBuilder
            .Create("SubclassChoiceArmathor")
            .SetGuiPresentation("ArmathorOath", Category.Subclass)
            .SetSubclassSuffix("ArmathorOath")
            .SetFilterByDeity(false)
            .SetSubclasses(
                ArmathorAlinor.Build()
            )
            .AddToDB());

        #endregion

        return Class;
    }

    private static SpellListDefinition BuildSpellList()
    {
        return SpellListDefinitionBuilder
            .Create("SpellListArmathor")
            .SetGuiPresentationNoContent(true) // spell lists don't need Gui presentation
            .ClearSpells()
            .SetSpellsAtLevel(0,
                SpellDefinitions.AcidSplash,
                SpellDefinitions.AnnoyingBee,
                SpellDefinitions.ChillTouch,
                SpellDefinitions.DancingLights,
                SpellDefinitions.Dazzle,
                SpellDefinitions.FireBolt,
                SpellDefinitions.Guidance,
                SpellDefinitions.Light,
                SpellDefinitions.PoisonSpray,
                SpellDefinitions.RayOfFrost,
                SpellDefinitions.Resistance,
                SpellDefinitions.SacredFlame,
                SpellDefinitions.ShadowArmor,
                SpellDefinitions.ShadowDagger,
                SpellDefinitions.Shine,
                SpellDefinitions.ShockingGrasp,
                SpellDefinitions.SpareTheDying,
                SpellDefinitions.Sparkle,
                SpellDefinitions.TrueStrike
            )
            .SetSpellsAtLevel(1,
                SpellDefinitions.AnimalFriendship,
                SpellDefinitions.Bane,
                SpellDefinitions.Bless,
                SpellDefinitions.BurningHands,
                SpellDefinitions.BurningHands_B,
                SpellDefinitions.CharmPerson,
                SpellDefinitions.ColorSpray,
                SpellDefinitions.ComprehendLanguages,
                SpellDefinitions.CureWounds,
                SpellDefinitions.DetectEvilAndGood,
                SpellDefinitions.DetectMagic,
                SpellDefinitions.ExpeditiousRetreat,
                SpellDefinitions.FalseLife,
                SpellDefinitions.FeatherFall,
                SpellDefinitions.FogCloud,
                SpellDefinitions.Grease,
                SpellDefinitions.GuidingBolt,
                SpellDefinitions.HealingWord,
                SpellDefinitions.Heroism,
                SpellDefinitions.HideousLaughter,
                SpellDefinitions.HuntersMark,
                SpellDefinitions.Identify,
                SpellDefinitions.InflictWounds,
                SpellDefinitions.Jump,
                SpellDefinitions.Longstrider,
                SpellDefinitions.Goodberry,
                SpellDefinitions.MageArmor,
                SpellDefinitions.MagicMissile,
                SpellDefinitions.ProtectionFromEvilGood,
                SpellDefinitions.Shield,
                SpellDefinitions.ShieldOfFaith,
                SpellDefinitions.Sleep,
                SpellDefinitions.Thunderwave
            )
            .SetSpellsAtLevel(2,
                SpellDefinitions.AcidArrow,
                SpellDefinitions.Aid,
                SpellDefinitions.Barkskin,
                SpellDefinitions.Blindness,
                SpellDefinitions.Blur,
                SpellDefinitions.CalmEmotions,
                SpellDefinitions.Darkness,
                SpellDefinitions.Darkvision,
                SpellDefinitions.EnhanceAbility,
                SpellDefinitions.FindTraps,
                SpellDefinitions.FlamingSphere,
                SpellDefinitions.HeatMetal,
                SpellDefinitions.HoldPerson,
                SpellDefinitions.Invisibility,
                SpellDefinitions.Knock,
                SpellDefinitions.LesserRestoration,
                SpellDefinitions.Levitate,
                SpellDefinitions.MagicWeapon,
                SpellDefinitions.MistyStep,
                SpellDefinitions.PassWithoutTrace,
                SpellDefinitions.PrayerOfHealing,
                SpellDefinitions.ProtectionFromPoison,
                SpellDefinitions.RayOfEnfeeblement,
                SpellDefinitions.ScorchingRay,
                SpellDefinitions.SeeInvisibility,
                SpellDefinitions.Silence,
                SpellDefinitions.Shatter,
                SpellDefinitions.SpiderClimb,
                SpellDefinitions.SpikeGrowth,
                SpellDefinitions.SpiritualWeapon
            )
            .SetSpellsAtLevel(3,
                SpellDefinitions.BeaconOfHope,
                SpellDefinitions.BestowCurse,
                SpellDefinitions.ConjureAnimals,
                SpellDefinitions.Counterspell,
                SpellDefinitions.CreateFood,
                SpellDefinitions.Daylight,
                SpellDefinitions.DispelMagic,
                SpellDefinitions.Fear,
                SpellDefinitions.Fireball,
                SpellDefinitions.Fly,
                SpellDefinitions.Haste,
                SpellDefinitions.HypnoticPattern,
                SpellDefinitions.LightningBolt,
                SpellDefinitions.MassHealingWord,
                SpellDefinitions.ProtectionFromEnergy,
                SpellDefinitions.RemoveCurse,
                SpellDefinitions.Revivify,
                SpellDefinitions.SleetStorm,
                SpellDefinitions.Slow,
                SpellDefinitions.StinkingCloud,
                SpellDefinitions.SpiritGuardians,
                SpellDefinitions.Tongues,
                SpellDefinitions.VampiricTouch,
                SpellDefinitions.WindWall
            )
            .SetSpellsAtLevel(4,
                SpellDefinitions.Banishment,
                SpellDefinitions.BlackTentacles,
                SpellDefinitions.Blight,
                SpellDefinitions.Confusion,
                SpellDefinitions.ConjureMinorElementals,
                SpellDefinitions.DeathWard,
                SpellDefinitions.DimensionDoor,
                SpellDefinitions.FireShield,
                SpellDefinitions.FreedomOfMovement,
                SpellDefinitions.GreaterInvisibility,
                SpellDefinitions.GuardianOfFaith,
                SpellDefinitions.IdentifyCreatures,
                SpellDefinitions.IceStorm,
                SpellDefinitions.PhantasmalKiller,
                SpellDefinitions.Stoneskin,
                SpellDefinitions.WallOfFire
            )
            .SetSpellsAtLevel(5,
                SpellDefinitions.CloudKill,
                SpellDefinitions.ConeOfCold,
                SpellDefinitions.ConjureElemental,
                SpellDefinitions.Contagion,
                SpellDefinitions.DispelEvilAndGood,
                SpellDefinitions.DominatePerson,
                SpellDefinitions.FlameStrike,
                SpellDefinitions.GreaterRestoration,
                SpellDefinitions.HoldMonster,
                SpellDefinitions.InsectPlague,
                SpellDefinitions.MassCureWounds,
                SpellDefinitions.MindTwist,
                SpellDefinitions.RaiseDead
            )
            .SetSpellsAtLevel(6,
                SpellDefinitions.BladeBarrier,
                SpellDefinitions.ChainLightning,
                SpellDefinitions.CircleOfDeath,
                SpellDefinitions.Disintegrate,
                SpellDefinitions.Eyebite,
                SpellDefinitions.FreezingSphere,
                SpellDefinitions.GlobeOfInvulnerability,
                SpellDefinitions.Harm,
                SpellDefinitions.Heal,
                SpellDefinitions.HeroesFeast,
                SpellDefinitions.Hilarity,
                SpellDefinitions.Sunbeam,
                SpellDefinitions.TrueSeeing
            )
            .FinalizeSpells(maxLevel: 9)
            .AddToDB();
    }

    private static FeatureDefinitionCastSpell BuildSpellCasting()
    {
        var castSpellsArmathor = FeatureDefinitionCastSpellBuilder
            .Create("CastSpellsArmathor")
            .SetGuiPresentation(Category.Feature)
            .SetSpellCastingOrigin(FeatureDefinitionCastSpell.CastingOrigin.Class)
            .SetFocusType(EquipmentDefinitions.FocusType.Divine)
            .SetKnownCantrips(6, 1, FeatureDefinitionCastSpellBuilder.CasterProgression.Full)
            .SetSlotsPerLevel(FeatureDefinitionCastSpellBuilder.CasterProgression.Full)
            .SetSpellKnowledge(SpellKnowledge.WholeList)
            .SetSpellReadyness(SpellReadyness.Prepared)
            .SetSpellPreparationCount(SpellPreparationCount.AbilityBonusPlusLevel)
            .SetSpellCastingAbility(AttributeDefinitions.Charisma)
            .SetSpellList(SpellList)
            .AddToDB();

        return castSpellsArmathor;
    }

    private static FeatureDefinition BuildRitualCasting()
    {
        return FeatureDefinitionFeatureSetBuilder.Create("FeatureSetArmathorRituals")
            .SetGuiPresentationNoContent(true)
            .AddFeatureSet(
                FeatureDefinitionMagicAffinityBuilder
                    .Create("MagicAffinityArmathorRituals")
                    .SetGuiPresentationNoContent(true)
                    .SetRitualCasting(RitualCasting.Prepared)
                    .AddToDB(),
                FeatureDefinitionActionAffinityBuilder
                    .Create("ActionAffinityArmathorRituals")
                    .SetGuiPresentationNoContent(true)
                    .SetDefaultAllowedActionTypes()
                    .SetAuthorizedActions(ActionDefinitions.Id.CastRitual)
                    .AddToDB())
            .AddToDB();
    }

    private static FeatureDefinitionBonusCantrips BuildBonusCantrips()
    {
        return FeatureDefinitionBonusCantripsBuilder
            .Create("BonusCantripsArmathor")
            .SetGuiPresentation(Category.Feature)
            .SetBonusCantrips(
                SpellDefinitions.Light
            )
            .AddToDB();
    }
}

internal class ArmathorClassHolder : IClassHoldingFeature
{
    private ArmathorClassHolder()
    {
    }

    public static ArmathorClassHolder Marker { get; } = new();

    public CharacterClassDefinition Class => ArmathorClass.Class;
}
