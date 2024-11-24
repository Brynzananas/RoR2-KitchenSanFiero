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
    internal static class Keychain
    {
        internal static GameObject KeychainPrefab;
        internal static Sprite KeychainIcon;
        public static ItemDef KeychainItemDef;
        public static ConfigEntry<bool> KeychainEnable;
        public static ConfigEntry<bool> KeychainAIBlacklist;
        public static ConfigEntry<float> keychainTier;
        public static ConfigEntry<float> keychainInitialCritIncrease;
        public static ConfigEntry<float> keychainInitialCritIncreaseStack;
        public static ConfigEntry<float> keychainChancePerItemStack;
        public static ConfigEntry<float> keychainChance;
        public static ConfigEntry<int> KeychainDoElite;
        public static ConfigEntry<int> KeychainDoChampion;
        public static ConfigEntry<float> keychainCritChancePerBuff;
        public static ConfigEntry<float> keychainCritDamagePerBuff;
        public static string name = "Keychain";

        internal static void Init()
        {
            AddConfigs();
            string tier = "Assets/Icons/KeyChainTier1.png";
            switch (keychainTier.Value)
            {
                case 1:
                    tier = "Assets/Icons/KeyChainTier1.png";
                    break;
                case 2:
                    tier = "Assets/Icons/KeyChainTier2.png";
                    break;
                case 3:
                    tier = "Assets/Icons/KeyChainTier3.png";
                    break;

            }
            KeychainPrefab = MainAssets.LoadAsset<GameObject>("Assets/Models/Prefabs/Keychains.prefab");
            KeychainIcon = MainAssets.LoadAsset<Sprite>(tier);

            Item();

            AddLanguageTokens();
            KeyBuff.Init();
        }

        private static void AddConfigs()
        {
            KeychainEnable = Config.Bind<bool>("Item : " + name,
                             "Activation",
                             false,
                             "Enable this item?");
            KeychainAIBlacklist = Config.Bind<bool>("Item : " + name,
                             "AI Blacklist",
                             false,
                             "Blacklist this item from enemies?");
            keychainTier = Config.Bind<float>("Item : " + name,
                                         "Item tier",
                                         2f,
                                         "1: Common/White\n2: Rare/Green\n3: Legendary/Red");
            keychainInitialCritIncrease = Config.Bind<float>("Item : " + name,
                                         "Crit chance addition",
                                         5f,
                                         "Control how much this item gives crit chance");
            keychainInitialCritIncreaseStack = Config.Bind<float>("Item : " + name,
                                         "Crit chance addition stack",
                                         0f,
                                         "Control how much this item gives crit chance per item stack");
            keychainChance = Config.Bind<float>("Item : " + name,
                                         "On kill buff chance",
                                         5f,
                                         "Control the chance of getting a buff on enemy kill in percentage");
            keychainChancePerItemStack = Config.Bind<float>("Item : " + name,
                                         "On kill buff chance stack",
                                         5f,
                                         "Control the chance of getting a buff on enemy kill per item stack in percentage");
            KeychainDoElite = Config.Bind<int>("Item : " + name,
                             "Elite kill",
                             1,
                             "Control how many buffs you get per for an elite kill");
            KeychainDoChampion = Config.Bind<int>("Item : " + name,
                             "Champion kill",
                             3,
                             "Control how many buffs you get per for a champion kill");
            keychainCritChancePerBuff = Config.Bind<float>("Item : " + name,
                                         "Buff crit chance increase",
                                         2.5f,
                                         "Control the crit chance increase per every buff stack");
            keychainCritDamagePerBuff = Config.Bind<float>("Item : " + name,
                                         "Buff crit damage increase",
                                         5f,
                                         "Control the crit damage percentage increase per every buff stack");
            ModSettingsManager.AddOption(new CheckBoxOption(KeychainEnable, new CheckBoxConfig() { restartRequired = true }));
            ModSettingsManager.AddOption(new CheckBoxOption(KeychainAIBlacklist, new CheckBoxConfig() { restartRequired = true }));
            ModSettingsManager.AddOption(new StepSliderOption(keychainTier, new StepSliderConfig() { min = 1, max = 3, increment = 1f, restartRequired = true }));
        }

        private static void Item()
        {
            KeychainItemDef = ScriptableObject.CreateInstance<ItemDef>();
            KeychainItemDef.name = name.Replace(" ", "");
            KeychainItemDef.nameToken = name.ToUpper().Replace(" ", "") + "_NAME";
            KeychainItemDef.pickupToken = name.ToUpper().Replace(" ", "") + "_PICKUP";
            KeychainItemDef.descriptionToken = name.ToUpper().Replace(" ", "") + "_DESC";
            KeychainItemDef.loreToken = name.ToUpper().Replace(" ", "") + "_LORE";
            switch (keychainTier.Value)
            {
                case 1:
                    KeychainItemDef.deprecatedTier = ItemTier.Tier1;
                    break;
                case 2:
                    KeychainItemDef.deprecatedTier = ItemTier.Tier2;
                    break;
                case 3:
                    KeychainItemDef.deprecatedTier = ItemTier.Tier3;
                    break;

            }
            KeychainItemDef.pickupIconSprite = KeychainIcon;
            KeychainItemDef.pickupModelPrefab = KeychainPrefab;
            KeychainItemDef.canRemove = true;
            KeychainItemDef.hidden = false;
            var tags = new List<ItemTag>() { ItemTag.Damage };
            if (KeychainAIBlacklist.Value)
            {
                tags.Add(ItemTag.AIBlacklist);
            }
            KeychainItemDef.tags = tags.ToArray();
            KeychainItemDef.requiredExpansion = CaeliImperiumExpansionDef;
            var displayRules = new ItemDisplayRuleDict(null);
            ItemAPI.Add(new CustomItem(KeychainItemDef, displayRules));
            On.RoR2.CharacterBody.HandleOnKillEffectsServer += OnKillElite;
            GetStatCoefficients += Stats;
        }

        private static void Stats(CharacterBody sender, StatHookEventArgs args)
        {
            int itemCount = sender.inventory ? sender.inventory.GetItemCount(KeychainItemDef) : 0;
            if (itemCount > 0)
            {
                args.critAdd += keychainInitialCritIncrease.Value + ((itemCount - 1) * keychainInitialCritIncreaseStack.Value);
            }
        }

        private static void OnKillElite(On.RoR2.CharacterBody.orig_HandleOnKillEffectsServer orig, CharacterBody self, DamageReport damageReport)
        {
            orig(self, damageReport);
            if (self && damageReport.victim)
            {
                int itemCount = self.inventory ? self.inventory.GetItemCount(KeychainItemDef) : 0;

                if (itemCount > 0)
                {

                    
                    float rollChance = keychainChance.Value + ((itemCount - 1) * keychainChancePerItemStack.Value);
                    int superRoll = (int)Math.Floor((float)(rollChance / 100));
                    if (damageReport.victim.body.isElite)
                    {
                        superRoll += KeychainDoElite.Value;
                    }
                    if (damageReport.victim.body.isChampion)
                    {
                        superRoll += KeychainDoChampion.Value;
                    }
                    for (int i = 0; i < superRoll; i++)
                    {
                        self.AddBuff(KeyBuff.KeyBuffDef);
                    }
                    if (Util.CheckRoll(rollChance - (superRoll * 100), self.master))
                    {
                        self.AddBuff(KeyBuff.KeyBuffDef);
                    }
                }


            }
        }

        private static void AddLanguageTokens()
        {
            LanguageAPI.Add(name.ToUpper().Replace(" ", "") + "_NAME", name.Replace(" ", ""));
            LanguageAPI.Add(name.ToUpper().Replace(" ", "") + "_PICKUP", keychainChance.Value + "% <style=cStack>(+" + keychainChancePerItemStack.Value + "% per item stack)</style> to gain a <style=cIsDamage>buff</style> on enemy kill, that increases <style=cIsDamage>>crit chance</style> by <style=cIsDamage>" + keychainCritChancePerBuff.Value + "%</style> <style=cStack>(+" + keychainCritChancePerBuff.Value + "% per buff stack)</style> and <style=cIsDamage>crit damage<style> by <style=cIsDamage>" + keychainCritDamagePerBuff.Value + "%</style> <style=cStack>(+" + keychainCritDamagePerBuff.Value + "% per buff stack)</style>");
            LanguageAPI.Add(name.ToUpper().Replace(" ", "") + "_DESC", keychainChance.Value + "% <style=cStack>(+" + keychainChancePerItemStack.Value + "% per item stack)</style> to gain a <style=cIsDamage>buff</style> on enemy kill, that increases <style=cIsDamage>>crit chance</style> by <style=cIsDamage>" + keychainCritChancePerBuff.Value + "%</style> <style=cStack>(+" + keychainCritChancePerBuff.Value + "% per buff stack)</style> and <style=cIsDamage>crit damage<style> by <style=cIsDamage>" + keychainCritDamagePerBuff.Value + "%</style> <style=cStack>(+" + keychainCritDamagePerBuff.Value + "% per buff stack)</style>");
            LanguageAPI.Add(name.ToUpper().Replace(" ", "") + "_LORE", "\"What the hell this keychain is for?\"");
        }
    }
}
