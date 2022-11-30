using System;
using SolastaUnfinishedBusiness.Api.Extensions;
using SolastaUnfinishedBusiness.Api.Helpers;
using SolastaUnfinishedBusiness.Builders;
using SolastaUnfinishedBusiness.Builders.Features;
using SolastaUnfinishedBusiness.CustomBehaviors;
using SolastaUnfinishedBusiness.CustomInterfaces;
using SolastaUnfinishedBusiness.CustomUI;
using SolastaUnfinishedBusiness.Models;
using SolastaUnfinishedBusiness.Properties;
using SolastaUnfinishedBusiness.Spells;
using UnityEngine.AddressableAssets;
using static RuleDefinitions;
using static RuleDefinitions.EffectIncrementMethod;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper.FeatureDefinitionAutoPreparedSpellss;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper.FeatureDefinitionHealingModifiers;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper.FeatureDefinitionPowers;

namespace SolastaUnfinishedBusiness.Classes.Armathor.Subclasses;

public static class ArmathorAlinor
{
    public static CharacterSubclassDefinition Build()
    {

        return CharacterSubclassDefinitionBuilder
            .Create("ArmathorAlinor")
            .SetGuiPresentation(Category.Subclass, CharacterSubclassDefinitions.OathOfDevotion)
            .AddFeaturesAtLevel(1,
                PowerOathOfDevotionTurnUnholy)
            .AddFeaturesAtLevel(3,
                AutoPreparedSpellsOathOfDevotion,
                PowerOathOfDevotionSacredWeapon)
            .AddFeaturesAtLevel(6,
                HealingModifierDomainLifeBlessedHealer,
                PowerDomainBattleHeraldOfBattle)
            .AddFeaturesAtLevel(7,
                PowerOathOfDevotionAuraDevotion)
            .AddFeaturesAtLevel(10,
                PowerClericDivineInterventionPaladin)
            .AddToDB();
    }
}
