using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using SolastaUnfinishedBusiness.Builders;
using SolastaUnfinishedBusiness.Builders.Features;
using SolastaUnfinishedBusiness.CustomInterfaces;
using static RuleDefinitions;
using static SolastaUnfinishedBusiness.Builders.Features.AutoPreparedSpellsGroupBuilder;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper.CharacterSubclassDefinitions;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper.FeatureDefinitionAbilityCheckAffinitys;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper.FeatureDefinitionAutoPreparedSpellss;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper.FeatureDefinitionBonusCantripss;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper.FeatureDefinitionCampAffinitys;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper.FeatureDefinitionFeatureSets;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper.FeatureDefinitionHealingModifiers;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper.FeatureDefinitionMagicAffinitys;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper.FeatureDefinitionPowers;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper.SpellDefinitions;


namespace SolastaUnfinishedBusiness.Subclasses;

internal sealed class OathOfTheArmathor : AbstractSubclass
{
    internal const string Name = "OathOfTheArmathor";

    internal OathOfTheArmathor()
    {
        var autoPreparedSpellsOathOfTheArmathor = FeatureDefinitionAutoPreparedSpellsBuilder
            .Create("AutoPreparedSpellsOathOfTheArmathor")
            .SetGuiPresentation("OathSpells", Category.Feature)
            .SetPreparedSpellGroups(
                BuildSpellGroup(2, Bless, BurningHands, CureWounds, FaerieFire, MagicMissile, ProtectionFromEvilGood, Shield, ShieldOfFaith),
                BuildSpellGroup(3, Blur, FlameBlade),
                BuildSpellGroup(5, ProtectionFromEnergy, DispelMagic),
                BuildSpellGroup(7, FireShield, DeathWard),
                BuildSpellGroup(9, HoldMonster, GreaterRestoration))
            .SetSpellcastingClass(CharacterClassDefinitions.Cleric)
            .AddToDB();

        Subclass = CharacterSubclassDefinitionBuilder
            .Create("OathOfTheArmathor")
            .SetGuiPresentation(Category.Subclass, DomainSun)
            .AddFeaturesAtLevel(3,
                AbilityCheckAffinityDomainInsightDivineEye,
                // AutoPreparedSpellsDomainBattle,
                // AutoPreparedSpellsDomainLife,
                // AutoPreparedSpellsDomainSun,
                autoPreparedSpellsOathOfTheArmathor,
                // AutoPreparedSpellsOathOfDevotion,
                BonusCantripsDomainSun,
                CampAffinityDomainOblivionPeacefulRest,
                FeatureSetDomainInsightDivineLore,
                FeatureSetDomainLawCommandingPresence,
                FeatureSetDomainLawUnyieldingEnforcer,
                HealingModifierDomainLifeDiscipleOfLife,
                MagicAffinityDomainSunHolyRadiance,
                PowerOathOfDevotionSacredWeapon,
                PowerOathOfDevotionTurnUnholy)
            .AddFeaturesAtLevel(6,
                HealingModifierDomainLifeBlessedHealer,
                PowerDomainBattleHeraldOfBattle)
            .AddFeaturesAtLevel(7,
                PowerOathOfDevotionAuraDevotion,
                PowerOathOfMotherlandVolcanicAura)
            .AddToDB();
    }

    internal override CharacterSubclassDefinition Subclass { get; }

    internal override FeatureDefinitionSubclassChoice SubclassChoice =>
        FeatureDefinitionSubclassChoices.SubclassChoicePaladinSacredOaths;

    private sealed class PaladinHolder : IClassHoldingFeature
    {
        // allows Illuminating Strike damage to scale with barbarian level
        public CharacterClassDefinition Class => CharacterClassDefinitions.Paladin;
    }
}
