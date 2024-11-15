using R2API;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine.AddressableAssets;
using UnityEngine;
using static KitchenSanFieroPlugin.KitchenSanFiero;
using static R2API.RecalculateStatsAPI;
using BepInEx.Configuration;
using RiskOfOptions.OptionConfigs;
using RiskOfOptions.Options;
using RiskOfOptions;
using UnityEngine.Networking;
using KitchenSanFiero.Buffs;

namespace KitchenSanFiero.Items
{
    internal static class Painkillers //: ItemBase<FirstItem>
    {
        internal static GameObject PainkillersPrefab;
        internal static Sprite PainkillersIcon;
        public static ItemDef PainkillersItemDef;
        public static ConfigEntry<bool> PainkillersEnable;
        public static ConfigEntry<bool> PainkillersAIBlacklist;
        public static ConfigEntry<float> PainkillersTier;
        public static ConfigEntry<float> PainkillersArmor;
        public static ConfigEntry<bool> PainkillersOnItemPickupEffect;

        internal static void Init()
        {
            AddConfigs();
            string tier = "Assets/Icons/Painkillers.png";
            switch (PainkillersTier.Value)
            {
                case 1:
                    tier = "Assets/Icons/Painkillers.png";
                    break;
                case 2:
                    tier = "Assets/Icons/PainkillersTier2.png";
                    break;
                case 3:
                    tier = "Assets/Icons/PainkillersTier3.png";
                    break;

            }
            PainkillersPrefab = MainAssets.LoadAsset<GameObject>("Assets/Models/Prefabs/PainKillers.prefab");
            PainkillersIcon = MainAssets.LoadAsset<Sprite>(tier);
            if (!PainkillersEnable.Value)
            {
                return;
            }
            Item();

            AddLanguageTokens();
        }

        public static void AddConfigs()
        {
            PainkillersEnable = Config.Bind<bool>("Item : Painkillers",
                             "Activation",
                             true,
                             "Enable Painkillers item?");
            PainkillersAIBlacklist = Config.Bind<bool>("Item : Painkillers",
                             "AI Blacklist",
                             false,
                             "Blacklist this item from enemies?");
            PainkillersTier = Config.Bind<float>("Item : Painkillers",
                                         "Item tier",
                                         1f,
                                         "1: Common/White\n2: Rare/Green\n3: Legendary/Red");
            PainkillersArmor = Config.Bind<float>("Item : Painkillers",
                                         "Armor gain",
                                         2.5f,
                                         "How much this item gives armor?");
            PainkillersOnItemPickupEffect = Config.Bind<bool>("Item : Painkillers",
                             "On item pickup effect",
                             true,
                             "Enable on pickup effect?");
            ModSettingsManager.AddOption(new CheckBoxOption(PainkillersEnable, new CheckBoxConfig() { restartRequired = true }));
            ModSettingsManager.AddOption(new CheckBoxOption(PainkillersAIBlacklist, new CheckBoxConfig() { restartRequired = true }));
            ModSettingsManager.AddOption(new StepSliderOption(PainkillersTier, new StepSliderConfig() { min = 1, max = 3, increment = 1f, restartRequired = true }));
            ModSettingsManager.AddOption(new FloatFieldOption(PainkillersArmor));
            ModSettingsManager.AddOption(new CheckBoxOption(PainkillersOnItemPickupEffect));
        }

        private static void Item()
        {
            PainkillersItemDef = ScriptableObject.CreateInstance<ItemDef>();
            PainkillersItemDef.name = "Painkillers";
            PainkillersItemDef.nameToken = "PAINKILLERS_NAME";
            PainkillersItemDef.pickupToken = "PAINKILLERS_PICKUP";
            PainkillersItemDef.descriptionToken = "PAINKILLERS_DESC";
            PainkillersItemDef.loreToken = "PAINKILLERS_LORE";
            switch (PainkillersTier.Value)
            {
                case 1:
                    PainkillersItemDef.deprecatedTier = ItemTier.Tier1;
                    break;
                case 2:
                    PainkillersItemDef.deprecatedTier = ItemTier.Tier2;
                    break;
                case 3:
                    PainkillersItemDef.deprecatedTier = ItemTier.Tier3;
                    break;

            }
            PainkillersItemDef.pickupIconSprite = PainkillersIcon;
            PainkillersItemDef.pickupModelPrefab = PainkillersPrefab;
            PainkillersItemDef.canRemove = true;
            PainkillersItemDef.hidden = false;
            var tags = new List<ItemTag>() { ItemTag.Healing };
            if (PainkillersAIBlacklist.Value)
            {
                tags.Add(ItemTag.AIBlacklist);
            }
            PainkillersItemDef.tags = tags.ToArray();
            ItemDisplayRuleDict rules = new ItemDisplayRuleDict();
            rules.Add("mdlCommandoDualies", new RoR2.ItemDisplayRule[]{
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = PainkillersPrefab,
                    childName = "Pelvis",
localPos = new Vector3(-0.18914F, -0.04197F, 0.01054F),
localAngles = new Vector3(10.29398F, 110.1134F, 180F),
localScale = new Vector3(0.06126F, 0.06126F, 0.06126F)
                }
            });
            rules.Add("mdlHuntress", new RoR2.ItemDisplayRule[]{
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = PainkillersPrefab,
                    childName = "Pelvis",
localPos = new Vector3(-0.24011F, 0.05098F, 0.0286F),
localAngles = new Vector3(337.8354F, 85.27686F, 187.3692F),
localScale = new Vector3(0.04147F, 0.04147F, 0.04147F)
                }
            });
            rules.Add("mdlBandit2", new RoR2.ItemDisplayRule[]{
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = PainkillersPrefab,
                    childName = "Chest",
localPos = new Vector3(-0.13665F, 0.30019F, -0.225F),
localAngles = new Vector3(16.17203F, 94.40896F, 123.702F),
localScale = new Vector3(0.03756F, 0.03756F, 0.03756F)
                }
            });
            rules.Add("mdlToolbot", new RoR2.ItemDisplayRule[]{
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = PainkillersPrefab,
                    childName = "Chest",
localPos = new Vector3(2.4724F, 1.79825F, -0.43137F),
localAngles = new Vector3(0F, 0F, 3.70976F),
localScale = new Vector3(0.40546F, 0.40546F, 0.40546F)
                }
            });
            rules.Add("mdlEngi", new RoR2.ItemDisplayRule[]{
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = PainkillersPrefab,
                    childName = "Pelvis",
localPos = new Vector3(-0.2754F, 0.04958F, 0.01303F),
localAngles = new Vector3(5.42078F, 71.9818F, 174.0692F),
localScale = new Vector3(0.04987F, 0.04987F, 0.04987F)
                }
            });
            rules.Add("mdlEngiTurrety", new RoR2.ItemDisplayRule[]{
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = PainkillersPrefab,
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
                    followerPrefab = PainkillersPrefab,
                    childName = "Pelvis",
localPos = new Vector3(-0.21339F, -0.03018F, -0.05785F),
localAngles = new Vector3(0.24362F, 82.89925F, 209.5247F),
localScale = new Vector3(0.05072F, 0.05072F, 0.05072F)
                }
            });
            rules.Add("mdlMerc", new RoR2.ItemDisplayRule[]{
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = PainkillersPrefab,
                    childName = "Pelvis",
localPos = new Vector3(0.21222F, 0.08415F, -0.01188F),
localAngles = new Vector3(348.6433F, 262.1263F, 187.7867F),
localScale = new Vector3(0.05652F, 0.05652F, 0.05652F)
                }
            });
            rules.Add("mdlTreebot", new RoR2.ItemDisplayRule[]{
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = PainkillersPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0f, 0f, 0f),
                    localAngles = new Vector3(0f, 0f, 0f),
                    localScale = new Vector3(1f, 1f, 1f)
                }
            });
            rules.Add("mdlLoader", new RoR2.ItemDisplayRule[]{
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = PainkillersPrefab,
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
                    followerPrefab = PainkillersPrefab,
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
                    followerPrefab = PainkillersPrefab,
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
                    followerPrefab = PainkillersPrefab,
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
                    followerPrefab = PainkillersPrefab,
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
                    followerPrefab = PainkillersPrefab,
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
                    followerPrefab = PainkillersPrefab,
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
                    followerPrefab = PainkillersPrefab,
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
                    followerPrefab = PainkillersPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0f, 0f, 0f),
                    localAngles = new Vector3(0f, 0f, 0f),
                    localScale = new Vector3(1f, 1f, 1f)
                }
            });
            var displayRules = new ItemDisplayRuleDict(null);
            ItemAPI.Add(new CustomItem(PainkillersItemDef, displayRules));
            //On.RoR2.CharacterBody.OnInventoryChanged += HealOnPickup;
            GetStatCoefficients += Stats;
        }

        /*
        private static void HealOnPickup(On.RoR2.CharacterBody.orig_OnInventoryChanged orig, CharacterBody self)
        {

            orig(self);
            int itemCount = self.inventory ? self.inventory.GetItemCount(PainkillersItemDef) : 0;
            if (itemCount > 0)
            {
                Debug.Log((ItemIndex)self.inventory.itemAcquisitionOrder.ToArray().GetValue(self.inventory.itemAcquisitionOrder.ToArray().Length - 1));
                Debug.Log(Painkillers.PainkillersItemDef.itemIndex);
            }

            if (itemCount > 0 && (ItemIndex)self.inventory.itemAcquisitionOrder.ToArray().GetValue(self.inventory.itemAcquisitionOrder.ToArray().Length - 1) == Painkillers.PainkillersItemDef.itemIndex)
            {
                Chat.AddMessage("Heal");
                Util.CleanseBody(self, true, false, false, true, true, false);
                self.healthComponent.HealFraction(1, default);
                self.healthComponent.RechargeShieldFull();
                self.healthComponent.barrier += Math.Min(self.maxHealth - self.healthComponent.barrier, 0);
            }
        }*/

        private static void Stats(CharacterBody sender, StatHookEventArgs args)
        {
            int itemCount = sender.inventory ? sender.inventory.GetItemCount(PainkillersItemDef) : 0;
            if (itemCount > 0)
            {
                args.armorAdd += itemCount * PainkillersArmor.Value;
                args.baseRegenAdd += itemCount * 1;
            }

        }

        private static void AddLanguageTokens()
        {
            LanguageAPI.Add("PAINKILLERS_NAME", "Painkillers");
            LanguageAPI.Add("PAINKILLERS_PICKUP", "<style=cIsUtility>Gain " + PainkillersArmor.Value + " armor</style> and 1 regen <style=cStack>per item stack<style=cStack>");
            LanguageAPI.Add("PAINKILLERS_DESC", "<style=cIsUtility>Gain " + PainkillersArmor.Value + " armor</style> and 1 regen <style=cStack>per item stack<style=cStack>");
            LanguageAPI.Add("PAINKILLERS_LORE", "mmmm yummy");
        }
    }
}
