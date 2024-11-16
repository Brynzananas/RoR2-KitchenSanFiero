using BepInEx.Configuration;
using IL.RoR2.Items;
using KitchenSanFiero.Buffs;
using R2API;
using RiskOfOptions.OptionConfigs;
using RiskOfOptions.Options;
using RiskOfOptions;
using RoR2;
using RoR2.Items;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using static KitchenSanFieroPlugin.KitchenSanFiero;
using static R2API.RecalculateStatsAPI;
using KitchenSanFiero.Elites;

namespace KitchenSanFiero.Items
{
    public static class ColdHeart
    {
        internal static GameObject ColdHeartPrefab;
        internal static Sprite ColdHeartIcon;
        public static ItemDef ColdHeartDef;
        public static ConfigEntry<float> ColdHeartDamage;
        public static ConfigEntry<float> ColdHeartHealth;
        public static ConfigEntry<bool> ColdHeartRegenerate;
        internal static void Init()
        {
            AddConfigs();

            string tier = "Assets/Icons/BrassBellIcon.png";
            ColdHeartPrefab = MainAssets.LoadAsset<GameObject>("Assets/Models/Prefabs/ColdHeart.prefab");
            ColdHeartIcon = MainAssets.LoadAsset<Sprite>(tier);
            Item();

            AddLanguageTokens();
        }
        public static void AddConfigs()
        {
            ColdHeartDamage = Config.Bind<float>("Item : Cold Heart",
                                         "Damage increase",
                                         20f,
                                         "Control the damage increase in percentage");
            ColdHeartHealth = Config.Bind<float>("Item : Cold Heart",
                                         "Health increase",
                                         20f,
                                         "Control the health increase in percentage");
            ColdHeartRegenerate = Config.Bind<bool>("Item : Cold Heart",
                                         "Regenerate",
                                         false,
                                         "Control if this item turns into The Dredged affix");

            ModSettingsManager.AddOption(new FloatFieldOption(ColdHeartDamage));
            ModSettingsManager.AddOption(new FloatFieldOption(ColdHeartHealth));
            ModSettingsManager.AddOption(new CheckBoxOption(ColdHeartRegenerate));
        }
        public static void Item()
        {

            ColdHeartDef = ScriptableObject.CreateInstance<ItemDef>();
            ColdHeartDef.name = "ColdHeart";
            ColdHeartDef.nameToken = "COLDHEART_NAME";
            ColdHeartDef.pickupToken = "COLDHEART_PICKUP";
            ColdHeartDef.descriptionToken = "COLDHEART_DESC";
            ColdHeartDef.loreToken = "COLDHEART_LORE";
            ColdHeartDef.deprecatedTier = ItemTier.Boss;
            ColdHeartDef.pickupIconSprite = ColdHeartIcon;
            ColdHeartDef.pickupModelPrefab = ColdHeartPrefab;
            ColdHeartDef.canRemove = false;
            ColdHeartDef.hidden = false;
            var tags = new List<ItemTag>() { ItemTag.WorldUnique, ItemTag.CannotDuplicate, ItemTag.CannotSteal};
            ColdHeartDef.tags = tags.ToArray();
            ItemDisplayRuleDict rules = new ItemDisplayRuleDict();
            rules.Add("mdlCommandoDualies", new RoR2.ItemDisplayRule[]{
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ColdHeartPrefab,
                    childName = "Pelvis",
localPos = new Vector3(-0.16116F, 0.01811F, 0.05452F),
localAngles = new Vector3(0F, 0F, 201.6437F),
localScale = new Vector3(0.04981F, 0.04981F, 0.04981F)
                }
            });
            rules.Add("mdlHuntress", new RoR2.ItemDisplayRule[]{
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ColdHeartPrefab,
                    childName = "Head",
localPos = new Vector3(0F, -0.00528F, 0.0624F),
localAngles = new Vector3(347.1176F, 0F, 0F),
localScale = new Vector3(0.05116F, 0.05369F, 0.05369F)
                }
            });
            rules.Add("mdlBandit2", new RoR2.ItemDisplayRule[]{
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ColdHeartPrefab,
                    childName = "Head",
localPos = new Vector3(-0.06211F, -0.07434F, 0.07202F),
localAngles = new Vector3(9.13731F, 313.1105F, 349.6901F),
localScale = new Vector3(0.02397F, 0.02397F, 0.02397F)
                }
            });
            rules.Add("mdlToolbot", new RoR2.ItemDisplayRule[]{
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ColdHeartPrefab,
                    childName = "Chest",
localPos = new Vector3(0F, -0.43046F, 2.33586F),
localAngles = new Vector3(0F, 0F, 0F),
localScale = new Vector3(1F, 1F, 1F)
                }
            });
            rules.Add("mdlEngi", new RoR2.ItemDisplayRule[]{
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ColdHeartPrefab,
                    childName = "Chest",
localPos = new Vector3(0F, 0.13773F, 0.21466F),
localAngles = new Vector3(0F, 0F, 0F),
localScale = new Vector3(0.06118F, 0.06118F, 0.06118F)
                }
            });
            rules.Add("mdlEngiTurret", new RoR2.ItemDisplayRule[]{
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ColdHeartPrefab,
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
                    followerPrefab = ColdHeartPrefab,
                    childName = "Head",
localPos = new Vector3(0F, -0.09482F, 0.04086F),
localAngles = new Vector3(348.9679F, 0F, 0F),
localScale = new Vector3(0.02953F, 0.02953F, 0.02953F)
                }
            });
            rules.Add("mdlMerc", new RoR2.ItemDisplayRule[]{
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ColdHeartPrefab,
                    childName = "Head",
localPos = new Vector3(0.08917F, 0.01914F, 0.06002F),
localAngles = new Vector3(0F, 90F, 0F),
localScale = new Vector3(0.03624F, 0.03624F, 0.03624F)
                }
            });
            rules.Add("mdlTreebot", new RoR2.ItemDisplayRule[]{
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ColdHeartPrefab,
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
                    followerPrefab = ColdHeartPrefab,
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
                    followerPrefab = ColdHeartPrefab,
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
                    followerPrefab = ColdHeartPrefab,
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
                    followerPrefab = ColdHeartPrefab,
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
                    followerPrefab = ColdHeartPrefab,
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
                    followerPrefab = ColdHeartPrefab,
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
                    followerPrefab = ColdHeartPrefab,
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
                    followerPrefab = ColdHeartPrefab,
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
                    followerPrefab = ColdHeartPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0f, 0f, 0f),
                    localAngles = new Vector3(0f, 0f, 0f),
                    localScale = new Vector3(1f, 1f, 1f)
                }
            });
            var displayRules = new ItemDisplayRuleDict(null);
            ItemAPI.Add(new CustomItem(ColdHeartDef, displayRules));
            GetStatCoefficients += Stats;
            On.RoR2.CharacterMaster.OnServerStageBegin += StageStart;
        }

        private static void StageStart(On.RoR2.CharacterMaster.orig_OnServerStageBegin orig, CharacterMaster self, Stage stage)
        {
            orig(self, stage);
            if (self && self.inventory && self.inventory.GetItemCount(ColdHeartDef) > 0)
            {
                int itemCount = self.inventory.GetItemCount(ColdHeartDef);
                for (int i = 0; i < itemCount; i++)
                {
                    PickupDropletController.CreatePickupDroplet(PickupCatalog.FindPickupIndex(Dredged.AffixDredgedEquipment.equipmentIndex), self.GetBody().transform.position, self.GetBody().transform.rotation.eulerAngles * 20f);
                    self.inventory.RemoveItem(ColdHeartDef);
                }
            }
        }

        private static void Stats(CharacterBody sender, StatHookEventArgs args)
        {
            int itemCount = sender.inventory ? sender.inventory.GetItemCount(ColdHeartDef) : 0;
            if (itemCount > 0)
            {
                args.damageMultAdd += ColdHeartDamage.Value / 100;
                args.healthMultAdd += ColdHeartHealth.Value / 100;
            }
        }

        public static void AddLanguageTokens()
        {
            LanguageAPI.Add("COLDHEART_NAME", "Cold Heart");
            LanguageAPI.Add("COLDHEART_PICKUP", "It's an eternity here...");
            LanguageAPI.Add("COLDHEART_DESC", "It's an eternity here...");
            LanguageAPI.Add("COLDHEART_LORE", "It's an eternity here...");
        }
    }
}
