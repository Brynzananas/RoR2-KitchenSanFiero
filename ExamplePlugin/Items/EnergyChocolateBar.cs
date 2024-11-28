using R2API;
using RoR2;
using static R2API.RecalculateStatsAPI;
using UnityEngine.AddressableAssets;
using UnityEngine;
using static CaeliImperiumPlugin.CaeliImperium;
using BepInEx.Configuration;
using System;
using RiskOfOptions.OptionConfigs;
using RiskOfOptions.Options;
using RiskOfOptions;
using System.Collections.Generic;

namespace CaeliImperium.Items
{
    internal static class EnergyChocolateBar //: ItemBase<FirstItem>
    {
        internal static GameObject EnergyChocolateBarPrefab;
        internal static Sprite EnergyChocolateBarIcon;
        public static ItemDef EnergyChocolateBarItemDef;
        public static ConfigEntry<bool> EnergyChocolateBarsEnable;
        public static ConfigEntry<bool> EnergyChocolateBarsAIBlacklist;
        public static ConfigEntry<float> EnergyChocolateBarTier;
        public static ConfigEntry<float> EnergyChocolateBuffStats;
        public static ConfigEntry<float> EnergyChocolateBuffStatsStack;
        public static ConfigEntry<bool> EnergyChocolateBarsLuck;
        public static ConfigEntry<float> EnergyChocolateStageConsumeChance;
        public static ConfigEntry<float> EnergyChocolateNextStage;

        internal static void Init()
        {
            AddConfigs();
            string tier = "Assets/Icons/EnergisedChocolateBarIcon.png";
            switch (EnergyChocolateBarTier.Value)
            {
                case 1:
                    tier = "Assets/Icons/EnergisedChocolateBarIcon.png";
                    break;
                case 2:
                    tier = "Assets/Icons/EnergisedChocolateBarIconTier2.png";
                    break;
                case 3:
                    tier = "Assets/Icons/EnergisedChocolateBarIconTier3.png";
                    break;

            }
            EnergyChocolateBarPrefab = MainAssets.LoadAsset<GameObject>("Assets/Models/Prefabs/Energychocolatebar.prefab");
            EnergyChocolateBarIcon = MainAssets.LoadAsset<Sprite>(tier);
            if (!EnergyChocolateBarsEnable.Value)
            {
                return;
            }
            Item();

            AddLanguageTokens();
            UsedEnergyChocolateBar.Init();
        }

        public static void AddConfigs()
        {
            EnergyChocolateBarsEnable = Config.Bind<bool>("Item : Energised Chocolate Bar",
                             "Activation",
                             true,
                             "Enable Energised Chocolate Bar item?");
            EnergyChocolateBarsAIBlacklist = Config.Bind<bool>("Item : Energised Chocolate Bar",
                                         "AI Blacklist",
                                         false,
                                         "Blacklist this item from enemies?");
            EnergyChocolateBarTier = Config.Bind<float>("Item : Energised Chocolate Bar",
                                         "Item tier",
                                         1f,
                                         "1: Common/White\n2: Rare/Green\n3: Legendary/Red");
            EnergyChocolateBuffStats = Config.Bind<float>("Item : Energised Chocolate Bar",
                                         "Statistics increase",
                                         10f,
                                         "Control how much this item gives all stats increase in percentage");
            EnergyChocolateBuffStatsStack = Config.Bind<float>("Item : Energised Chocolate Bar",
                                         "Statistics increase stack",
                                         10f,
                                         "Control how much this item gives all stats increas per item stacke in percentage");
            EnergyChocolateNextStage = Config.Bind<float>("Item : Energised Chocolate Bar",
                                         "Next stage behaviour",
                                         1f,
                                         "0: Do nothing\n1: Turn into consumed\n2: Turn into scrap");
            EnergyChocolateBarsLuck = Config.Bind<bool>("Item : Energised Chocolate Bar",
                                         "Luck",
                                         false,
                                         "Is consuming chance affected by luck?");
            EnergyChocolateStageConsumeChance = Config.Bind<float>("Item : Energised Chocolate Bar",
                                         "Consume chance",
                                         50f,
                                         "Control the chance of consuming one item upon the next stage");
            ModSettingsManager.AddOption(new CheckBoxOption(EnergyChocolateBarsEnable, new CheckBoxConfig() { restartRequired = true }));
            ModSettingsManager.AddOption(new CheckBoxOption(EnergyChocolateBarsAIBlacklist, new CheckBoxConfig() { restartRequired = true }));
            ModSettingsManager.AddOption(new StepSliderOption(EnergyChocolateBarTier, new StepSliderConfig() { min = 1, max = 3, increment = 1f, restartRequired = true }));
            ModSettingsManager.AddOption(new FloatFieldOption(EnergyChocolateBuffStats));
            ModSettingsManager.AddOption(new FloatFieldOption(EnergyChocolateBuffStatsStack));
            ModSettingsManager.AddOption(new StepSliderOption(EnergyChocolateNextStage, new StepSliderConfig() { min = 0, max = 2, increment = 1f }));
            ModSettingsManager.AddOption(new FloatFieldOption(EnergyChocolateStageConsumeChance));
            ModSettingsManager.AddOption(new CheckBoxOption(EnergyChocolateBarsLuck));
        }

        private static void Item()
        {
            EnergyChocolateBarItemDef = ScriptableObject.CreateInstance<ItemDef>();
            EnergyChocolateBarItemDef.name = "EnergisedChocolateBar";
            EnergyChocolateBarItemDef.nameToken = "ENERGYCHOCOLATEBAR_NAME";
            EnergyChocolateBarItemDef.pickupToken = "ENERGYCHOCOLATEBAR_PICKUP";
            EnergyChocolateBarItemDef.descriptionToken = "ENERGYCHOCOLATEBAR_DESC";
            EnergyChocolateBarItemDef.loreToken = "ENERGYCHOCOLATEBAR_LORE";
            switch (EnergyChocolateBarTier.Value)
            {
                case 1:
                    EnergyChocolateBarItemDef.deprecatedTier = ItemTier.Tier1;
                    break;
                case 2:
                    EnergyChocolateBarItemDef.deprecatedTier = ItemTier.Tier2;
                    break;
                case 3:
                    EnergyChocolateBarItemDef.deprecatedTier = ItemTier.Tier3;
                    break;

            }
            EnergyChocolateBarItemDef.pickupIconSprite = EnergyChocolateBarIcon;
            EnergyChocolateBarItemDef.pickupModelPrefab = EnergyChocolateBarPrefab;
            EnergyChocolateBarItemDef.canRemove = true;
            EnergyChocolateBarItemDef.hidden = false;
            EnergyChocolateBarItemDef.requiredExpansion = CaeliImperiumExpansionDef;
            var tags = new List<ItemTag>() { ItemTag.Damage, ItemTag.Healing};
            if (EnergyChocolateBarsAIBlacklist.Value)
            {
                tags.Add(ItemTag.AIBlacklist);
            }
            EnergyChocolateBarItemDef.tags = tags.ToArray();
            ItemDisplayRuleDict rules = new ItemDisplayRuleDict();
            rules.Add("mdlCommandoDualies", new RoR2.ItemDisplayRule[]{
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = EnergyChocolateBarPrefab,
                    childName = "Head",
localPos = new Vector3(-0.14437F, 0.08411F, 0.21978F),
localAngles = new Vector3(67.14931F, 197.7514F, 47.91053F),
localScale = new Vector3(0.28959F, 0.28959F, 0.28959F)
                }
            });
            rules.Add("mdlHuntress", new RoR2.ItemDisplayRule[]{
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = EnergyChocolateBarPrefab,
                    childName = "Head",
localPos = new Vector3(-0.03193F, 0.12892F, 0.20428F),
localAngles = new Vector3(84.18024F, 215.9874F, 63.58562F),
localScale = new Vector3(0.18503F, 0.18503F, 0.18503F)
                }
            });
            rules.Add("mdlBandit2", new RoR2.ItemDisplayRule[]{
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = EnergyChocolateBarPrefab,
                    childName = "Head",
localPos = new Vector3(-0.10461F, -0.08352F, 0.1266F),
localAngles = new Vector3(42.75136F, 202.3632F, 43.04484F),
localScale = new Vector3(0.15394F, 0.15394F, 0.15394F)
                }
            });
            rules.Add("mdlToolbot", new RoR2.ItemDisplayRule[]{
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = EnergyChocolateBarPrefab,
                    childName = "Head",
localPos = new Vector3(0.677F, 3.11327F, -1.48611F),
localAngles = new Vector3(41.71579F, 88.20041F, 122.4081F),
localScale = new Vector3(1F, 1F, 1F)
                }
            });
            rules.Add("mdlEngi", new RoR2.ItemDisplayRule[]{
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = EnergyChocolateBarPrefab,
                    childName = "Chest",
localPos = new Vector3(-0.09458F, 0.39751F, 0.26715F),
localAngles = new Vector3(41.9376F, 240.1781F, 70.90697F),
localScale = new Vector3(0.19937F, 0.19937F, 0.19937F)
                }
            });
            rules.Add("mdlEngiTurrety", new RoR2.ItemDisplayRule[]{
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = EnergyChocolateBarPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0f, 0f, 0f),
                    localAngles = new Vector3(0f, 0f, 0f),
                    localScale = new Vector3(1f, 1f, 1f)
                }
            });
            rules.Add("mdlMage", new RoR2.ItemDisplayRule[]{
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = EnergyChocolateBarPrefab,
                    childName = "Head",
localPos = new Vector3(-0.05581F, -0.05835F, 0.14325F),
localAngles = new Vector3(51.0438F, 184.2134F, 29.81714F),
localScale = new Vector3(0.15306F, 0.15306F, 0.15306F)
                }
            });
            rules.Add("mdlMerc", new RoR2.ItemDisplayRule[]{
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = EnergyChocolateBarPrefab,
                    childName = "Head",
localPos = new Vector3(-0.07584F, -0.00039F, 0.18758F),
localAngles = new Vector3(26.50391F, 235.7366F, 61.81181F),
localScale = new Vector3(0.21935F, 0.21935F, 0.21935F)
                }
            });
            rules.Add("mdlTreebot", new RoR2.ItemDisplayRule[]{
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = EnergyChocolateBarPrefab,
                    childName = "PlatformBase",
localPos = new Vector3(-0.31788F, 1.4358F, 0.35937F),
localAngles = new Vector3(311.4557F, 117.9195F, 196.9814F),
localScale = new Vector3(1F, 1F, 1F)
                }
            });
            rules.Add("mdlLoader", new RoR2.ItemDisplayRule[]{
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = EnergyChocolateBarPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0f, 0f, 0f),
                    localAngles = new Vector3(0f, 0f, 0f),
                    localScale = new Vector3(1f, 1f, 1f)
                }
            });
            rules.Add("mdlCroco", new RoR2.ItemDisplayRule[]{
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = EnergyChocolateBarPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0f, 0f, 0f),
                    localAngles = new Vector3(0f, 0f, 0f),
                    localScale = new Vector3(1f, 1f, 1f)
                }
            });
            rules.Add("mdlCaptain", new RoR2.ItemDisplayRule[]{
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = EnergyChocolateBarPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0f, 0f, 0f),
                    localAngles = new Vector3(0f, 0f, 0f),
                    localScale = new Vector3(1f, 1f, 1f)
                }
            });
            rules.Add("mdlRailGunner", new RoR2.ItemDisplayRule[]{
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = EnergyChocolateBarPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0f, 0f, 0f),
                    localAngles = new Vector3(0f, 0f, 0f),
                    localScale = new Vector3(1f, 1f, 1f)
                }
            });
            rules.Add("mdlVoidSurvivor", new RoR2.ItemDisplayRule[]{
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = EnergyChocolateBarPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0f, 0f, 0f),
                    localAngles = new Vector3(0f, 0f, 0f),
                    localScale = new Vector3(1f, 1f, 1f)
                }
            });
            rules.Add("mdlSeeker", new RoR2.ItemDisplayRule[]{
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = EnergyChocolateBarPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0f, 0f, 0f),
                    localAngles = new Vector3(0f, 0f, 0f),
                    localScale = new Vector3(1f, 1f, 1f)
                }
            });
            rules.Add("mdlChef", new RoR2.ItemDisplayRule[]{
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = EnergyChocolateBarPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0f, 0f, 0f),
                    localAngles = new Vector3(0f, 0f, 0f),
                    localScale = new Vector3(1f, 1f, 1f)
                }
            });
            rules.Add("mdlFalseSon", new RoR2.ItemDisplayRule[]{
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = EnergyChocolateBarPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0f, 0f, 0f),
                    localAngles = new Vector3(0f, 0f, 0f),
                    localScale = new Vector3(1f, 1f, 1f)
                }
            });
            rules.Add("mdlScav", new RoR2.ItemDisplayRule[]{
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = EnergyChocolateBarPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0f, 0f, 0f),
                    localAngles = new Vector3(0f, 0f, 0f),
                    localScale = new Vector3(1f, 1f, 1f)
                }
            });
            var displayRules = new ItemDisplayRuleDict(null);
            ItemAPI.Add(new CustomItem(EnergyChocolateBarItemDef, rules));
            GetStatCoefficients += Stats;
            On.RoR2.CharacterMaster.OnServerStageBegin += StageStart;
        }

        private static void StageStart(On.RoR2.CharacterMaster.orig_OnServerStageBegin orig, CharacterMaster self, Stage stage)
        {
            orig(self, stage);
            int itemCount = self.inventory ? self.inventory.GetItemCount(EnergyChocolateBarItemDef) : 0;
            if (itemCount > 0 && EnergyChocolateNextStage.Value != 0)
                
            {
                int itemsToConsume = 0;
                float luckDown = self.luck;
                if (!EnergyChocolateBarsLuck.Value)
                {
                    luckDown = 0;
                }
                for (int i = 0; i < itemCount; i++)
                {

                    if (!Util.CheckRoll(EnergyChocolateStageConsumeChance.Value, -luckDown))
                    {
                        itemsToConsume++;
                    }
                }
                switch (EnergyChocolateNextStage.Value)
                {
                    case 0:
                        break;
                    case 1:
                        self.inventory.GiveItem(UsedEnergyChocolateBar.ConsumedEnergyChocolateBarItemDef, itemsToConsume);
                        self.inventory.RemoveItem(EnergyChocolateBarItemDef, itemsToConsume);
                        CharacterMasterNotificationQueue.SendTransformNotification(self, EnergyChocolateBarItemDef.itemIndex, UsedEnergyChocolateBar.ConsumedEnergyChocolateBarItemDef.itemIndex, CharacterMasterNotificationQueue.TransformationType.Default);
                        break;
                    case 2:
                        self.inventory.GiveItem(RoR2Content.Items.ScrapWhite, itemsToConsume);
                        self.inventory.RemoveItem(EnergyChocolateBarItemDef, itemsToConsume);
                        CharacterMasterNotificationQueue.SendTransformNotification(self, EnergyChocolateBarItemDef.itemIndex, RoR2Content.Items.ScrapWhite.itemIndex, CharacterMasterNotificationQueue.TransformationType.Default);
                        break;
                }

            }
        }

        private static void Stats(CharacterBody sender, StatHookEventArgs args)
        {
            int statIncrease = sender.inventory ? sender.inventory.GetItemCount(EnergyChocolateBarItemDef) : 0;// * (EnergyChocolateBuffStats.Value / 100f) + 1;
            if (statIncrease > 0)
            {
            args.healthMultAdd += (EnergyChocolateBuffStats.Value / 100f) + ((statIncrease - 1) * (EnergyChocolateBuffStatsStack.Value / 100f));
            args.baseAttackSpeedAdd += (EnergyChocolateBuffStats.Value / 100f) + ((statIncrease - 1) * (EnergyChocolateBuffStatsStack.Value / 100f));
                args.damageMultAdd += (EnergyChocolateBuffStats.Value / 100f) + ((statIncrease - 1) * (EnergyChocolateBuffStatsStack.Value / 100f));
                args.moveSpeedMultAdd += (EnergyChocolateBuffStats.Value / 100f) + ((statIncrease - 1) * (EnergyChocolateBuffStatsStack.Value / 100f));
                args.armorAdd += EnergyChocolateBuffStats.Value + ((statIncrease - 1) * (EnergyChocolateBuffStatsStack.Value));
                args.regenMultAdd += (EnergyChocolateBuffStats.Value / 10f) + ((statIncrease - 1) * (EnergyChocolateBuffStatsStack.Value / 10f));
                args.critAdd += EnergyChocolateBuffStats.Value + ((statIncrease - 1) * (EnergyChocolateBuffStatsStack.Value));
            }
            
        }

        private static void AddLanguageTokens()
        {
            string nextStageBehaviour = "";
            if (EnergyChocolateNextStage.Value != 0)
            {
                if (EnergyChocolateNextStage.Value == 1)
                {
                    nextStageBehaviour = " Consume on next stage with " + EnergyChocolateStageConsumeChance.Value + "% chance";
                }
                if (EnergyChocolateNextStage.Value == 2)
                {
                    nextStageBehaviour = " Scrap on next stage with " + EnergyChocolateStageConsumeChance.Value + "% chance";
                }
            }
            else
            {
                nextStageBehaviour = "";
            }
            LanguageAPI.Add("ENERGYCHOCOLATEBAR_NAME", "Energised Chocolate Bar");
            LanguageAPI.Add("ENERGYCHOCOLATEBAR_PICKUP", "Gain " + EnergyChocolateBuffStats.Value + "%<style=cStack>(+" + EnergyChocolateBuffStatsStack.Value + "% per item stack)</style> to all <style=cIsDamage>all statistics</style>." + nextStageBehaviour);
            LanguageAPI.Add("ENERGYCHOCOLATEBAR_DESC", "Gain " + EnergyChocolateBuffStats.Value + "%<style=cStack>(+" + EnergyChocolateBuffStatsStack.Value + "% per item stack)</style> <style=cIsDamage>all stat bonus</style>." + nextStageBehaviour);
            LanguageAPI.Add("ENERGYCHOCOLATEBAR_LORE", "mmmm yummy");
        }
    }
}
