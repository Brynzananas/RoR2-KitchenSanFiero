﻿using BepInEx.Configuration;
using R2API;
using RiskOfOptions.OptionConfigs;
using RiskOfOptions.Options;
using RiskOfOptions;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using static KitchenSanFieroPlugin.KitchenSanFiero;
using RoR2.Orbs;
using KitchenSanFiero.Buffs;
using static R2API.RecalculateStatsAPI;
using Rewired;

namespace KitchenSanFiero.Items
{
    internal static class OpposingForce
    {
        internal static GameObject OpposingForcePrefab;
        internal static Sprite OpposingForceIcon;
        public static ItemDef OpposingForceItemDef;
        public static ConfigEntry<bool> OpposingForceEnable;
        public static ConfigEntry<bool> OpposingForceAIBlacklist;
        public static ConfigEntry<float> OpposingForceTier;
        public static ConfigEntry<float> OpposingForceArmorGain;
        public static ConfigEntry<float> OpposingForceArmorGainStack;
        public static ConfigEntry<float> OpposingForceDamage;
        public static ConfigEntry<float> OpposingForceDamageStack;
        public static string name = "Opposing Force";

        internal static void Init()
        {
            AddConfigs();
            string tier = "Assets/Icons/BrassBellIcon.png";
            switch (OpposingForceTier.Value)
            {
                case 1:
                    tier = "Assets/Icons/BrassBellIconTier1.png";
                    break;
                case 2:
                    tier = "Assets/Icons/BrassBellIcon.png";
                    break;
                case 3:
                    tier = "Assets/Icons/BrassBellIconTier3.png";
                    break;

            }
            OpposingForcePrefab = MainAssets.LoadAsset<GameObject>("Assets/Models/Prefabs/OpposingForceItem.prefab");
            OpposingForceIcon = MainAssets.LoadAsset<Sprite>(tier);
            if (!OpposingForceEnable.Value)
            {
                return;
            }
            Item();

            AddLanguageTokens();
        }

        private static void AddConfigs()
        {
            OpposingForceEnable = Config.Bind<bool>("Item : " + name,
                             "Activation",
                             true,
                             "Enable this item?");
            OpposingForceAIBlacklist = Config.Bind<bool>("Item : " + name,
                             "AI Blacklist",
                             true,
                             "Blacklist this item from enemies?");
            OpposingForceTier = Config.Bind<float>("Item : " + name,
                                         "Item tier",
                                         1f,
                                         "1: Common/White\n2: Rare/Green\n3: Legendary/Red");
            OpposingForceArmorGain = Config.Bind<float>("Item : " + name,
                                         "Armor",
                                         6f,
                                         "Control the armor gain");
            OpposingForceArmorGainStack = Config.Bind<float>("Item : " + name,
                                         "Armor stack",
                                         4f,
                                         "Control the armor gain per item stack");
            OpposingForceDamage = Config.Bind<float>("Item : " + name,
                                         "Damage",
                                         100f,
                                         "Control the final damage in percentage");
            OpposingForceDamageStack = Config.Bind<float>("Item : " + name,
                                         "Damage stack",
                                         100f,
                                         "Control the damage increase per item stack in percentage");
            ModSettingsManager.AddOption(new CheckBoxOption(OpposingForceEnable, new CheckBoxConfig() { restartRequired = true }));
            ModSettingsManager.AddOption(new CheckBoxOption(OpposingForceAIBlacklist, new CheckBoxConfig() { restartRequired = true }));
            ModSettingsManager.AddOption(new StepSliderOption(OpposingForceTier, new StepSliderConfig() { min = 1, max = 3, increment = 1f, restartRequired = true }));
            ModSettingsManager.AddOption(new StepSliderOption(OpposingForceArmorGain));
            ModSettingsManager.AddOption(new StepSliderOption(OpposingForceArmorGainStack));
            ModSettingsManager.AddOption(new StepSliderOption(OpposingForceDamage));
            ModSettingsManager.AddOption(new StepSliderOption(OpposingForceDamageStack));
        }

        private static void Item()
        {
            OpposingForceItemDef = ScriptableObject.CreateInstance<ItemDef>();
            OpposingForceItemDef.name = name.Replace(" ", "");
            OpposingForceItemDef.nameToken = name.ToUpper().Replace(" ", "") + "_NAME";
            OpposingForceItemDef.pickupToken = name.ToUpper().Replace(" ", "") + "_PICKUP";
            OpposingForceItemDef.descriptionToken = name.ToUpper().Replace(" ", "") + "_DESC";
            OpposingForceItemDef.loreToken = name.ToUpper().Replace(" ", "") + "_LORE";
            switch (OpposingForceTier.Value)
            {
                case 1:
                    OpposingForceItemDef.deprecatedTier = ItemTier.Tier1;
                    break;
                case 2:
                    OpposingForceItemDef.deprecatedTier = ItemTier.Tier2;
                    break;
                case 3:
                    OpposingForceItemDef.deprecatedTier = ItemTier.Tier3;
                    break;

            }
            OpposingForceItemDef.pickupIconSprite = OpposingForceIcon;
            OpposingForceItemDef.pickupModelPrefab = OpposingForcePrefab;
            OpposingForceItemDef.canRemove = true;
            OpposingForceItemDef.hidden = false;
            var tags = new List<ItemTag>() { ItemTag.Damage };
            if (OpposingForceAIBlacklist.Value)
            {
                tags.Add(ItemTag.AIBlacklist);
            }
            OpposingForceItemDef.tags = tags.ToArray();
            var displayRules = new ItemDisplayRuleDict(null);
            ItemAPI.Add(new CustomItem(OpposingForceItemDef, displayRules));
            On.RoR2.GlobalEventManager.OnHitEnemy += ReturnDamage;
            GetStatCoefficients += Stats;
        }

        private static void ReturnDamage(On.RoR2.GlobalEventManager.orig_OnHitEnemy orig, GlobalEventManager self, DamageInfo damageInfo, GameObject victim)
        {
            orig(self, damageInfo, victim);
            if (damageInfo.attacker && victim)
            {
                
                CharacterBody victimBody = victim.GetComponent<CharacterBody>();
                int itemCount = victimBody.inventory ? victimBody.inventory.GetItemCount(OpposingForceItemDef) : 0;
                if (itemCount > 0 && victimBody.armor > 0)
                {                   
                    CharacterBody attackerBody = damageInfo.attacker.GetComponent<CharacterBody>();

                    DamageInfo damageInfo2 = new DamageInfo
                    {
                        damage = damageInfo.damage * (1 - (100 / (100 + victimBody.armor))) * (OpposingForceDamage.Value / 100) * ((itemCount - 1) / 100),
                        damageColorIndex = DamageColorIndex.Item,
                        damageType = DamageType.Silent,
                        attacker = victim,
                        crit = Util.CheckRoll(victimBody.crit),
                        force = Vector3.zero,
                        inflictor = null,
                        position = attackerBody.transform.position,
                        procChainMask = damageInfo.procChainMask,
                        procCoefficient = 0f
                    };
                    attackerBody.healthComponent.TakeDamage(damageInfo2);
                }
            }
        }

        private static void Stats(CharacterBody sender, StatHookEventArgs args)
        {
            int itemCount = sender.inventory ? sender.inventory.GetItemCount(OpposingForceItemDef) : 0;
            if (itemCount > 0)
            {
                args.armorAdd += OpposingForceArmorGain.Value + ((itemCount - 1) * OpposingForceArmorGainStack.Value);
            }
        }

        private static void AddLanguageTokens()
        {
            LanguageAPI.Add(name.ToUpper().Replace(" ", "") + "_NAME", name.Replace(" ", ""));
            LanguageAPI.Add(name.ToUpper().Replace(" ", "") + "_PICKUP", "");
            LanguageAPI.Add(name.ToUpper().Replace(" ", "") + "_DESC", "");
            LanguageAPI.Add(name.ToUpper().Replace(" ", "") + "_LORE", "mmmm yummy");
        }
    }
}
