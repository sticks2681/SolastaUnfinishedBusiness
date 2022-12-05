﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using SolastaUnfinishedBusiness.Api.Extensions;
using SolastaUnfinishedBusiness.Api.Helpers;
using SolastaUnfinishedBusiness.Builders;
using SolastaUnfinishedBusiness.Builders.Features;
using SolastaUnfinishedBusiness.CustomInterfaces;
using SolastaUnfinishedBusiness.CustomUI;
using UnityEngine.AddressableAssets;
using static RuleDefinitions;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper.CharacterFamilyDefinitions;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper.CharacterSubclassDefinitions;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper.ItemDefinitions;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper.MonsterDefinitions;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper.SpellDefinitions;
using static SolastaUnfinishedBusiness.Subclasses.CommonBuilders;

namespace SolastaUnfinishedBusiness.Subclasses;

internal sealed class WizardDeadMaster : AbstractSubclass
{
    private const string WizardDeadMasterName = "WizardDeadMaster";
    private const string CreateDeadTag = "DeadMasterMinion";

    internal WizardDeadMaster()
    {
        var autoPreparedSpellsDeadMaster = FeatureDefinitionAutoPreparedSpellsBuilder
            .Create("AutoPreparedSpellsDeadMaster")
            .SetGuiPresentation(Category.Feature)
            .SetSpellcastingClass(CharacterClassDefinitions.Wizard)
            .SetAutoTag("DeadMaster")
            .SetPreparedSpellGroups(GetDeadSpellAutoPreparedGroups())
            .AddToDB();

        var targetReducedToZeroHpDeadMasterStarkHarvest = FeatureDefinitionBuilder
            .Create("TargetReducedToZeroHpDeadMasterStarkHarvest")
            .SetGuiPresentation(Category.Feature)
            .AddToDB();

        targetReducedToZeroHpDeadMasterStarkHarvest.SetCustomSubFeatures(
            new StarkHarvest(targetReducedToZeroHpDeadMasterStarkHarvest));

        const string ChainsName = "SummoningAffinityDeadMasterUndeadChains";

        var hpBonus = FeatureDefinitionAttributeModifierBuilder
            .Create("AttributeModifierDeadMasterUndeadChains")
            .SetGuiPresentation(ChainsName, Category.Feature)
            .SetModifier(FeatureDefinitionAttributeModifier.AttributeModifierOperation.AddConditionAmount,
                AttributeDefinitions.HitPoints)
            .AddToDB();

        var attackBonus = FeatureDefinitionAttackModifierBuilder
            .Create("AttackModifierDeadMasterUndeadChains")
            .SetGuiPresentation(ChainsName, Category.Feature)
            .SetAttackRollModifier(method: AttackModifierMethod.SourceConditionAmount)
            .AddToDB();

        var deadMasterUndeadChains = FeatureDefinitionSummoningAffinityBuilder
            .Create(ChainsName)
            .SetGuiPresentation(Category.Feature)
            .SetRequiredMonsterTag(CreateDeadTag)
            .SetAddedConditions(ConditionDefinitionBuilder
                    .Create("ConditionDeadMasterUndeadChainsProficiency")
                    .SetGuiPresentation(ChainsName, Category.Feature)
                    .SetSilent(Silent.WhenAddedOrRemoved)
                    .SetPossessive()
                    .SetAmountOrigin(ExtraOriginOfAmount.SourceProficiencyBonus)
                    .SetFeatures(attackBonus)
                    .AddToDB(),
                ConditionDefinitionBuilder
                    .Create("ConditionDeadMasterUndeadChainsLevel")
                    .SetGuiPresentation(ChainsName, Category.Feature)
                    .SetSilent(Silent.WhenAddedOrRemoved)
                    .SetPossessive()
                    .SetAmountOrigin(ExtraOriginOfAmount.SourceClassLevel, WizardClass)
                    .SetFeatures(hpBonus, hpBonus)
                    .AddToDB())
            .AddToDB();

        Subclass = CharacterSubclassDefinitionBuilder
            .Create(WizardDeadMasterName)
            .SetGuiPresentation(Category.Subclass, SorcerousHauntedSoul)
            .AddFeaturesAtLevel(2,
                autoPreparedSpellsDeadMaster,
                deadMasterUndeadChains)
            .AddFeaturesAtLevel(6,
                targetReducedToZeroHpDeadMasterStarkHarvest)
            .AddFeaturesAtLevel(10,
                DamageAffinityGenericHardenToNecrotic)
            .AddFeaturesAtLevel(14,
                PowerCasterCommandUndead)
            .AddToDB();

        EnableCommandAllUndead();
    }

    internal override CharacterSubclassDefinition Subclass { get; }

    internal override FeatureDefinitionSubclassChoice SubclassChoice =>
        FeatureDefinitionSubclassChoices.SubclassChoiceWizardArcaneTraditions;

    private static void EnableCommandAllUndead()
    {
        if (!Main.Settings.EnableCommandAllUndead)
        {
            return;
        }

        var monsterDefinitions = DatabaseRepository.GetDatabase<MonsterDefinition>();

        foreach (var monsterDefinition in monsterDefinitions
                     .Where(x => x.CharacterFamily == Undead.Name))
        {
            monsterDefinition.fullyControlledWhenAllied = true;
        }
    }

    [NotNull]
    private static FeatureDefinitionAutoPreparedSpells.AutoPreparedSpellsGroup[] GetDeadSpellAutoPreparedGroups()
    {
        var createDeadSpellMonsters =
            new Dictionary<(int clazz, int spell),
                List<(MonsterDefinition monster, int number, AssetReferenceSprite icon, BaseDefinition[] attackSprites
                    )>>
            {
                {
                    (2, 1), new()
                    {
                        (Skeleton, 2, Sprites.SpellRaiseSkeleton, new BaseDefinition[] { Scimitar }),
                        (Skeleton_Archer, 2, Sprites.SpellRaiseSkeletonArcher,
                            new BaseDefinition[] { Shortbow, Shortsword })
                    }
                }, //CR 0.25 x2
                {
                    (3, 2), new()
                    {
                        (Ghoul, 1, Sprites.SpellRaiseGhoul,
                            new BaseDefinition[]
                            {
                                MonsterAttackDefinitions.Attack_Wildshape_GiantEagle_Talons,
                                MonsterAttackDefinitions.Attack_Wildshape_Wolf_Bite
                            })
                    }
                }, //CR 1
                {
                    (5, 3), new()
                    {
                        (Skeleton_Enforcer, 1, Sprites.SpellRaiseSkeletonEnforcer,
                            new BaseDefinition[] { Battleaxe, MonsterAttackDefinitions.Attack_Wildshape_Ape_Toss_Rock })
                    }
                }, //CR 2
                {
                    (7, 4), new()
                    {
                        (Skeleton_Knight, 1, Sprites.SpellRaiseSkeletonKnight, new BaseDefinition[] { Longsword }),
                        (Skeleton_Marksman, 1, Sprites.SpellRaiseSkeletonMarksman,
                            new BaseDefinition[] { Longbow, Shortsword })
                    }
                }, //CR 3
                {
                    (9, 5),
                    new() { (Ghost, 1, Sprites.SpellRaiseGhost, new BaseDefinition[] { Enchanted_Dagger_Souldrinker }) }
                }, //CR 4
                {
                    (11, 6),
                    new() { (Wight, 2, Sprites.SpellRaiseWight, new BaseDefinition[] { LongswordPlus2, LongbowPlus1 }) }
                }, //CR 3 x2
                {
                    (13, 7), new()
                    {
                        (WightLord, 1, Sprites.SpellRaiseWightLord,
                            new BaseDefinition[] { Enchanted_Longsword_Frostburn, Enchanted_Shortbow_Medusa })
                    }
                } //CR 6
            };

        var result = new List<FeatureDefinitionAutoPreparedSpells.AutoPreparedSpellsGroup>();

        foreach (var kvp in createDeadSpellMonsters)
        {
            var (clazz, spell) = kvp.Key;
            var monsters = kvp.Value;
            var spells = new List<SpellDefinition>();

            foreach (var (monsterDefinition, count, icon, attackSprites) in monsters)
            {
                var monster = MakeSummonedMonster(monsterDefinition, attackSprites);
                var title = Gui.Format("Spell/&SpellRaiseDeadFormatTitle",
                    monster.FormatTitle());
                var description = Gui.Format("Spell/&SpellRaiseDeadFormatDescription",
                    monster.FormatTitle(),
                    monster.FormatDescription());

                var createDeadSpell = SpellDefinitionBuilder
                    .Create($"CreateDead{monster.name}")
                    .SetGuiPresentation(title, description, icon)
                    .SetSchoolOfMagic(SchoolOfMagicDefinitions.SchoolNecromancy)
                    .SetSpellLevel(spell)
                    .SetMaterialComponent(MaterialComponentType.Mundane)
                    .SetVocalSpellSameType(VocalSpellSemeType.Debuff)
                    .SetUniqueInstance()
                    .SetCastingTime(ActivationTime.Action)
                    .SetEffectDescription(EffectDescriptionBuilder.Create()
                        .SetTargetingData(Side.All, RangeType.Distance, 4, TargetType.Position, count)
                        .SetDurationData(DurationType.UntilLongRest)
                        .SetParticleEffectParameters(VampiricTouch)
                        .SetEffectForms(EffectFormBuilder.Create()
                            .SetSummonCreatureForm(1, monster.Name)
                            .Build())
                        .Build())
                    .AddToDB();

                if (Main.Settings.EnableRegisteringUndeadSpells)
                {
                    Models.SpellsContext.RegisterSpell(createDeadSpell);
                }

                spells.Add(createDeadSpell);
            }

            result.Add(new FeatureDefinitionAutoPreparedSpells.AutoPreparedSpellsGroup
            {
                ClassLevel = clazz, SpellsList = spells
            });
        }

        return result.ToArray();
    }

    private static MonsterDefinition MakeSummonedMonster(
        MonsterDefinition monster,
        IReadOnlyList<BaseDefinition> attackSprites)
    {
        var modified = MonsterDefinitionBuilder
            .Create(monster, $"Risen{monster.Name}")
            .SetDefaultFaction(FactionDefinitions.Party)
            .SetBestiaryEntry(BestiaryDefinitions.BestiaryEntry.None)
            .SetDungeonMakerPresence(MonsterDefinition.DungeonMaker.None)
            .AddCreatureTags(CreateDeadTag)
            .SetFullyControlledWhenAllied(true)
            .SetDroppedLootDefinition(null)
            .AddToDB();

        if (attackSprites == null)
        {
            return modified;
        }

        for (var i = 0; i < attackSprites.Count; i++)
        {
            var attack = modified.AttackIterations.ElementAtOrDefault(i);

            if (attack != null)
            {
                attack.MonsterAttackDefinition.GuiPresentation.spriteReference =
                    attackSprites[i].GuiPresentation.SpriteReference;
            }
        }

        return modified;
    }

    private sealed class StarkHarvest : ITargetReducedToZeroHp
    {
        private readonly FeatureDefinition feature;

        public StarkHarvest(FeatureDefinition feature)
        {
            this.feature = feature;
        }

        public IEnumerator HandleCharacterReducedToZeroHp(
            GameLocationCharacter attacker,
            GameLocationCharacter downedCreature,
            RulesetAttackMode attackMode,
            RulesetEffect activeEffect)
        {
            if (activeEffect is not RulesetEffectSpell spellEffect)
            {
                yield break;
            }

            var characterFamily = downedCreature.RulesetCharacter.CharacterFamily;

            if (characterFamily == Construct.Name || characterFamily == Undead.Name)
            {
                yield break;
            }

            var usedSpecialFeatures = attacker.UsedSpecialFeatures;

            usedSpecialFeatures.TryAdd(feature.Name, 0);

            if (usedSpecialFeatures[feature.Name] > 0)
            {
                yield break;
            }

            usedSpecialFeatures[feature.Name]++;

            var rulesetAttacker = attacker.RulesetCharacter;
            var spell = spellEffect.SpellDefinition;
            var isNecromancy = spell.SchoolOfMagic == SchoolNecromancy;
            var healingReceived = (isNecromancy ? 3 : 2) * spell.SpellLevel;

            GameConsoleHelper.LogCharacterUsedFeature(rulesetAttacker, feature, indent: true);

            if (rulesetAttacker.MissingHitPoints > 0)
            {
                rulesetAttacker.ReceiveHealing(healingReceived, true, rulesetAttacker.Guid);
            }
            else if (rulesetAttacker.TemporaryHitPoints <= healingReceived)
            {
                rulesetAttacker.ReceiveTemporaryHitPoints(healingReceived, DurationType.Minute, 1,
                    TurnOccurenceType.EndOfTurn, rulesetAttacker.Guid);
            }
        }
    }
}
