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
using static CaeliImperiumPlugin.CaeliImperium;
using RoR2.Orbs;
using CaeliImperium.Buffs;
using static R2API.RecalculateStatsAPI;
using Rewired;

namespace CaeliImperium.Items
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
        public static ConfigEntry<float> OpposingForcePower;
        public static ConfigEntry<float> OpposingForceProc;
        public static string name = "Opposing Force";

        internal static void Init()
        {
            AddConfigs();
            string tier = "Assets/Icons/OpposingForceTier1.png";
            switch (OpposingForceTier.Value)
            {
                case 1:
                    tier = "Assets/Icons/OpposingForceTier1.png";
                    break;
                case 2:
                    tier = "Assets/Icons/OpposingForceTier2.png";
                    break;
                case 3:
                    tier = "Assets/Icons/OpposingForceTier3.png";
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
                                         5f,
                                         "Control the armor gain");
            OpposingForceArmorGainStack = Config.Bind<float>("Item : " + name,
                                         "Armor stack",
                                         5f,
                                         "Control the armor gain per item stack");
            OpposingForceDamage = Config.Bind<float>("Item : " + name,
                                         "Damage",
                                         400f,
                                         "Control the damage in percentage");
            OpposingForceDamageStack = Config.Bind<float>("Item : " + name,
                                         "Damage stack",
                                         400f,
                                         "Control the damage increase per item stack in percentage");
            OpposingForcePower = Config.Bind<float>("Item : " + name,
                                         "Power",
                                         1.5f,
                                         "Control the power of reflected damage");
            OpposingForceProc = Config.Bind<float>("Item : " + name,
                                         "Proc",
                                         0.5f,
                                         "Control the proc of reflected damage");
            ModSettingsManager.AddOption(new CheckBoxOption(OpposingForceEnable, new CheckBoxConfig() { restartRequired = true }));
            ModSettingsManager.AddOption(new CheckBoxOption(OpposingForceAIBlacklist, new CheckBoxConfig() { restartRequired = true }));
            ModSettingsManager.AddOption(new StepSliderOption(OpposingForceTier, new StepSliderConfig() { min = 1, max = 3, increment = 1f, restartRequired = true }));
            ModSettingsManager.AddOption(new StepSliderOption(OpposingForceArmorGain));
            ModSettingsManager.AddOption(new StepSliderOption(OpposingForceArmorGainStack));
            ModSettingsManager.AddOption(new StepSliderOption(OpposingForceDamage));
            ModSettingsManager.AddOption(new StepSliderOption(OpposingForceDamageStack));
            ModSettingsManager.AddOption(new StepSliderOption(OpposingForcePower));
            ModSettingsManager.AddOption(new StepSliderOption(OpposingForceProc));
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
            OpposingForceItemDef.requiredExpansion = CaeliImperiumExpansionDef;
            var tags = new List<ItemTag>() { ItemTag.Damage, ItemTag.BrotherBlacklist };
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
                
                CharacterBody victimBody = victim.GetComponent<CharacterBody>() ? victim.GetComponent<CharacterBody>() : null;
                int itemCount = 0;
                if (victimBody != null)
                {
                itemCount = victimBody.inventory ? victimBody.inventory.GetItemCount(OpposingForceItemDef) : 0;
                }
                if (itemCount > 0 && victimBody && victimBody.armor > 0)
                {                   
                    CharacterBody attackerBody = damageInfo.attacker.GetComponent<CharacterBody>();

                    DamageInfo damageInfo2 = new DamageInfo
                    {
                        damage = (float)Math.Pow(damageInfo.damage, OpposingForcePower.Value) * (1 - (100 / (100 + victimBody.armor))) * (OpposingForceDamage.Value / 100) + ((itemCount - 1) * OpposingForceDamageStack.Value / 100),
                        damageColorIndex = DamageColorIndex.Item,
                        damageType = DamageType.Silent,
                        attacker = victim,
                        crit = Util.CheckRoll(victimBody.crit),
                        force = Vector3.zero,
                        inflictor = null,
                        position = attackerBody.transform.position,
                        procChainMask = damageInfo.procChainMask,
                        procCoefficient = OpposingForceProc.Value
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
            LanguageAPI.Add(name.ToUpper().Replace(" ", "") + "_NAME", name);
            LanguageAPI.Add(name.ToUpper().Replace(" ", "") + "_PICKUP", "Reflect <style=cIsDamage>" + OpposingForceDamage.Value + "%</style> <style=cStack>(+" + OpposingForceDamageStack.Value +"% per item stack)</style> <style=cIsDamage>incoming damage</style> multiplied to the <style=cIsHealing>armor</style> percentage. Gain <style=cIsHealing>+" + OpposingForceArmorGain.Value + "</style> <style=cStack>(+" + OpposingForceArmorGainStack.Value + " per item stack)</style> <style=cIsHealing>armor</style>");
            LanguageAPI.Add(name.ToUpper().Replace(" ", "") + "_DESC", "Reflect <style=cIsDamage>" + OpposingForceDamage.Value + "%</style> <style=cStack>(+" + OpposingForceDamageStack.Value + "% per item stack)</style> <style=cIsDamage>incoming damage</style> multiplied to the <style=cIsHealing>armor</style> percentage. Gain <style=cIsHealing>+" + OpposingForceArmorGain.Value + "</style> <style=cStack>(+" + OpposingForceArmorGainStack.Value + " per item stack)</style> <style=cIsHealing>armor</style>");
            LanguageAPI.Add(name.ToUpper().Replace(" ", "") + "_LORE", "");
        }
    }
}
