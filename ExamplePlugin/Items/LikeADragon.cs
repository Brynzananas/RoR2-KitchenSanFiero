using BepInEx.Configuration;
using R2API;
using RiskOfOptions.OptionConfigs;
using RiskOfOptions.Options;
using RiskOfOptions;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using static ReignFromGreatBeyondPlugin.CaeliImperium;
using RoR2.Orbs;
using CaeliImperium.Buffs;
using static R2API.RecalculateStatsAPI;
using Rewired;

namespace CaeliImperium.Items
{
    internal static class LikeADragon
    {
        internal static GameObject LikeADragonPrefab;
        internal static Sprite LikeADragonIcon;
        public static ItemDef LikeADragonItemDef;
        public static ConfigEntry<bool> LikeADragonEnable;
        public static ConfigEntry<bool> LikeADragonAIBlacklist;
        public static ConfigEntry<float> LikeADragonTier;
        public static ConfigEntry<int> LikeADragonRoll;
        public static ConfigEntry<int> LikeADragonRollStack;
        public static ConfigEntry<float> LikeADragonRollChance;
        public static ConfigEntry<float> LikeADragonRollChanceStack;
        public static ConfigEntry<bool> LikeADragonLuck;
        public static ConfigEntry<float> LikeADragonDamage;
        public static ConfigEntry<float> LikeADragonDamageStack;
        public static string name = "Like a Dragon";

        internal static void Init()
        {
            AddConfigs();
            string tier = "Assets/Icons/LikeADragonTier1.png";
            switch (LikeADragonTier.Value)
            {
                case 1:
                    tier = "Assets/Icons/LikeADragonTier1.png";
                    break;
                case 2:
                    tier = "Assets/Icons/LikeADragonTier2.png";
                    break;
                case 3:
                    tier = "Assets/Icons/LikeADragonTier3.png";
                    break;

            }
            LikeADragonPrefab = MainAssets.LoadAsset<GameObject>("Assets/Models/Prefabs/BrassKnuckles.prefab");
            LikeADragonIcon = MainAssets.LoadAsset<Sprite>(tier);
            if (!LikeADragonEnable.Value)
            {
                return;
            }
            Item();

            AddLanguageTokens();
        }

        private static void AddConfigs()
        {
            LikeADragonEnable = Config.Bind<bool>("Item : " + name,
                             "Activation",
                             true,
                             "Enable this item?");
            LikeADragonAIBlacklist = Config.Bind<bool>("Item : " + name,
                             "AI Blacklist",
                             false,
                             "Blacklist this item from enemies?");
            LikeADragonTier = Config.Bind<float>("Item : " + name,
                                         "Item tier",
                                         2f,
                                         "1: Common/White\n2: Rare/Green\n3: Legendary/Red");
            LikeADragonRoll = Config.Bind<int>("Item : " + name,
                                         "Roll amount",
                                         3,
                                         "Control the roll amount for damage increase");
            LikeADragonRollStack = Config.Bind<int>("Item : " + name,
                                         "Roll amount stack",
                                         1,
                                         "Control the roll amount increase for damage increase per item stack");
            LikeADragonRollChance = Config.Bind<float>("Item : " + name,
                                         "Roll chance",
                                         20,
                                         "Control the roll chance for damage increase");
            LikeADragonRollChanceStack = Config.Bind<float>("Item : " + name,
                                         "Roll chance stack",
                                         0,
                                         "Control the roll chance increase for damage increase per item stack");
            LikeADragonLuck = Config.Bind<bool>("Item : " + name,
                             "Luck",
                             true,
                             "Does luck affects the chance?");
            LikeADragonDamage = Config.Bind<float>("Item : " + name,
                                         "Damage increase",
                                         20,
                                         "Control the damage increase in percentage");
            LikeADragonDamageStack = Config.Bind<float>("Item : " + name,
                                         "Damage increase stack",
                                         0,
                                         "Control the damage increase per item stack in percentage");
            ModSettingsManager.AddOption(new CheckBoxOption(LikeADragonEnable, new CheckBoxConfig() { restartRequired = true }));
            ModSettingsManager.AddOption(new CheckBoxOption(LikeADragonAIBlacklist, new CheckBoxConfig() { restartRequired = true }));
            ModSettingsManager.AddOption(new StepSliderOption(LikeADragonTier, new StepSliderConfig() { min = 1, max = 3, increment = 1f, restartRequired = true }));
            ModSettingsManager.AddOption(new IntFieldOption(LikeADragonRoll));
            ModSettingsManager.AddOption(new IntFieldOption(LikeADragonRollStack));
            ModSettingsManager.AddOption(new FloatFieldOption(LikeADragonRollChance));
            ModSettingsManager.AddOption(new FloatFieldOption(LikeADragonRollChanceStack));
            ModSettingsManager.AddOption(new CheckBoxOption(LikeADragonLuck));
            ModSettingsManager.AddOption(new FloatFieldOption(LikeADragonDamage));
            ModSettingsManager.AddOption(new FloatFieldOption(LikeADragonDamageStack));
        }

        private static void Item()
        {
            LikeADragonItemDef = ScriptableObject.CreateInstance<ItemDef>();
            LikeADragonItemDef.name = name.Replace(" ", "");
            LikeADragonItemDef.nameToken = name.ToUpper().Replace(" ", "") + "_NAME";
            LikeADragonItemDef.pickupToken = name.ToUpper().Replace(" ", "") + "_PICKUP";
            LikeADragonItemDef.descriptionToken = name.ToUpper().Replace(" ", "") + "_DESC";
            LikeADragonItemDef.loreToken = name.ToUpper().Replace(" ", "") + "_LORE";
            switch (LikeADragonTier.Value)
            {
                case 1:
                    LikeADragonItemDef.deprecatedTier = ItemTier.Tier1;
                    break;
                case 2:
                    LikeADragonItemDef.deprecatedTier = ItemTier.Tier2;
                    break;
                case 3:
                    LikeADragonItemDef.deprecatedTier = ItemTier.Tier3;
                    break;

            }
            LikeADragonItemDef.pickupIconSprite = LikeADragonIcon;
            LikeADragonItemDef.pickupModelPrefab = LikeADragonPrefab;
            LikeADragonItemDef.canRemove = true;
            LikeADragonItemDef.hidden = false;
            LikeADragonItemDef.requiredExpansion = CaeliImperiumExpansionDef;
            var tags = new List<ItemTag>() { ItemTag.Damage };
            if (LikeADragonAIBlacklist.Value)
            {
                tags.Add(ItemTag.AIBlacklist);
            }
            LikeADragonItemDef.tags = tags.ToArray();
            var displayRules = new ItemDisplayRuleDict(null);
            ItemAPI.Add(new CustomItem(LikeADragonItemDef, displayRules));
            On.RoR2.HealthComponent.TakeDamageProcess += DragonIt;
        }

        private static void DragonIt(On.RoR2.HealthComponent.orig_TakeDamageProcess orig, HealthComponent self, DamageInfo damageInfo)
        {
            if (damageInfo.attacker && !damageInfo.rejected)
            {
                var attackerBody = damageInfo.attacker.GetComponent<CharacterBody>();
                int itemCount = attackerBody.inventory ? attackerBody.inventory.GetItemCount(LikeADragonItemDef) : 0;
                if (itemCount > 0)
                {
                    for (int i = 0; i < LikeADragonRoll.Value + ((itemCount - 1) * LikeADragonRollStack.Value); i++)
                    {
                        float luck = attackerBody.master.luck;
                        if (!LikeADragonLuck.Value)
                        {
                            luck = 0;
                        }
                        if (Util.CheckRoll(LikeADragonRollChance.Value + ((itemCount - 1) * LikeADragonRollChanceStack.Value), luck))
                        {
                        damageInfo.damage += damageInfo.damage * LikeADragonDamage.Value + ((itemCount - 1) * LikeADragonDamageStack.Value);
                        }
                    }
                }
            }
            orig(self, damageInfo);
        }

        private static void AddLanguageTokens()
        {
            LanguageAPI.Add(name.ToUpper().Replace(" ", "") + "_NAME", name);
            LanguageAPI.Add(name.ToUpper().Replace(" ", "") + "_PICKUP", "");
            LanguageAPI.Add(name.ToUpper().Replace(" ", "") + "_DESC", "");
            LanguageAPI.Add(name.ToUpper().Replace(" ", "") + "_LORE", "mmmm yummy");
        }
    }
}
