using R2API;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine.AddressableAssets;
using UnityEngine;
using static CaeliImperiumPlugin.CaeliImperium;
using static R2API.RecalculateStatsAPI;
using RiskOfOptions.OptionConfigs;
using RiskOfOptions.Options;
using RiskOfOptions;
using System.Xml.Linq;
using BepInEx.Configuration;
using System.ComponentModel;
using UnityEngine.Networking;

namespace CaeliImperium.Items
{
    internal static class HeroBandage
    {
        internal static GameObject ThunderThighsPrefab;
        internal static Sprite ThunderThighsIcon;
        public static ItemDef ThunderThighsItemDef;
        public static string name = "Hero Bandage";
        public static ConfigEntry<bool> ThunderThigsEnable;
        public static ConfigEntry<bool> ThunderThigsEnableConfig;
        public static ConfigEntry<bool> ThunderThighsAIBlacklist;
        public static ConfigEntry<float> ThunderThighsTier;
        public static ConfigEntry<float> ThunderThighsDamage;
        public static ConfigEntry<float> ThunderThighsRadius;

        internal static void Init()
        {
            AddConfigs();
            string tier = "Assets/Icons/ChalkTier1.png";
            switch (ConfigFloat(ThunderThighsTier, ThunderThigsEnableConfig))
            {
                case 1:
                    tier = "Assets/Icons/ChalkTier1.png";
                    break;
                case 2:
                    tier = "Assets/Icons/ChalkTier2.png";
                    break;
                case 3:
                    tier = "Assets/Icons/ChalkTier3.png";
                    break;

            }
            ThunderThighsPrefab = MainAssets.LoadAsset<GameObject>("Assets/Models/Prefabs/Chalk.prefab");
            ThunderThighsIcon = MainAssets.LoadAsset<Sprite>(tier);
            Item();

            AddLanguageTokens();
            Buffs.RocketJumpBuff.Init();
        }
        public static void AddConfigs()
        {
            ThunderThigsEnable = Config.Bind<bool>("Item : " + name,
            "Activation",
                             true,
                             "Enable this item?");
            ThunderThigsEnableConfig = Config.Bind<bool>("Item : " + name,
                             "Config Activation",
                             false,
                             "Enable config?\nActivation option and |options under these brackets| are always taken in effect");
            ThunderThighsAIBlacklist = Config.Bind<bool>("Item : " + name,
            "AI Blacklist",
                             false,
                             "Blacklist this item from enemies?");
            ThunderThighsTier = Config.Bind<float>("Item : " + name,
            "Item tier",
            3f,
                                         "1: Common/White\n2: Rare/Green\n3: Legendary/Red");
            ModSettingsManager.AddOption(new CheckBoxOption(ThunderThigsEnable, new CheckBoxConfig() { restartRequired = true }));
            ModSettingsManager.AddOption(new CheckBoxOption(ThunderThigsEnableConfig));
            ModSettingsManager.AddOption(new CheckBoxOption(ThunderThighsAIBlacklist, new CheckBoxConfig() { restartRequired = true }));
            ModSettingsManager.AddOption(new StepSliderOption(ThunderThighsTier, new StepSliderConfig() { min = 1, max = 3, increment = 1f, restartRequired = true }));
        }
        private static void Item()
        {
            ThunderThighsItemDef = ScriptableObject.CreateInstance<ItemDef>();
            ThunderThighsItemDef.name = name.Replace(" ", "");
            ThunderThighsItemDef.nameToken = name.Replace(" ", "").ToUpper() + "_NAME";
            ThunderThighsItemDef.pickupToken = name.Replace(" ", "").ToUpper() + "_PICKUP";
            ThunderThighsItemDef.descriptionToken = name.Replace(" ", "").ToUpper() + "_DESC";
            ThunderThighsItemDef.loreToken = name.Replace(" ", "").ToUpper() + "_LORE";
            switch (ConfigFloat(ThunderThighsTier, ThunderThigsEnableConfig))
            {
                case 1:
                    ThunderThighsItemDef.deprecatedTier = ItemTier.Tier1;
                    break;
                case 2:
                    ThunderThighsItemDef.deprecatedTier = ItemTier.Tier2;
                    break;
                case 3:
                    ThunderThighsItemDef.deprecatedTier = ItemTier.Tier3;
                    break;

            }
            ThunderThighsItemDef.pickupIconSprite = ThunderThighsIcon;
            ThunderThighsItemDef.pickupModelPrefab = ThunderThighsPrefab;
            ThunderThighsItemDef.canRemove = true;
            ThunderThighsItemDef.hidden = false;
            ThunderThighsItemDef.requiredExpansion = CaeliImperiumExpansionDef;
            var tags = new List<ItemTag>() { ItemTag.Damage };
            if (ConfigBool(ThunderThighsAIBlacklist, ThunderThigsEnableConfig))
            {
                tags.Add(ItemTag.AIBlacklist);
            }
            ThunderThighsItemDef.tags = tags.ToArray();
            var displayRules = new ItemDisplayRuleDict(null);
            ItemAPI.Add(new CustomItem(ThunderThighsItemDef, displayRules));
            //On.RoR2.CharacterBody.OnSkillActivated += Fire;
            On.RoR2.HealthComponent.TakeDamageProcess += Damage;
            On.RoR2.CharacterBody.OnInventoryChanged += Beeeeehaviour;
        }

        private static void Beeeeehaviour(On.RoR2.CharacterBody.orig_OnInventoryChanged orig, CharacterBody self)
        {
            if (NetworkServer.active)
            {
                self.AddItemBehavior<HeroBandageBehaviour>(self.inventory.GetItemCount(ThunderThighsItemDef));
            }
            orig(self);
        }

        private static void Damage(On.RoR2.HealthComponent.orig_TakeDamageProcess orig, HealthComponent self, DamageInfo damageInfo)
        {
            var attackerBody = damageInfo.attacker ? damageInfo.attacker.GetComponent<CharacterBody>() : null;
            if (attackerBody)
            {
                int itemCount = attackerBody.inventory ? attackerBody.inventory.GetItemCount(ThunderThighsItemDef) : 0;
                if (itemCount > 0 && self.body && damageInfo.attacker && attackerBody && !damageInfo.rejected && damageInfo.damageType.damageSource == DamageSource.Secondary)
                {
                    damageInfo.damage *= 1 + ((itemCount - 1) * 0.5f);
                }
            }
            
            orig(self, damageInfo);
        }
        /*
        private static void Fire(On.RoR2.CharacterBody.orig_OnSkillActivated orig, CharacterBody self, GenericSkill skill)
        {
            orig (self, skill);
            int itemCount = self.inventory ? self.inventory.GetItemCount(ThunderThighsItemDef) : 0;
            if (itemCount > 0 && self.skillLocator.secondary && skill == self.skillLocator.secondary && self.skillLocator.secondary.stock < 1)
            {
                self.skillLocator.secondary.AddOneStock();
            }
        }*/
        public class HeroBandageBehaviour : RoR2.CharacterBody.ItemBehavior
        {
            //[BaseItemBodyBehavior.ItemDefAssociationAttribute(useOnServer = true, useOnClient = false)]
            private float timer1;
            private bool isWorking;

            private static ItemDef GetItemDef()
            {
                return ThunderThighsItemDef;
            }
            private void Awake()
            {
                base.enabled = false;
            }

            private void OnEnable()
            {
                //base.enabled = true;
            }

            private void FixedUpdate()
            {
                if (body)
                {
                    if (stack > 0)
                    { 
                        if (body.inputBank.skill2.down)
                        {
                            
                            var secondarySkill = body.skillLocator.secondary;
                            //if (secondarySkill && secondarySkill.stock >= 1)
                            //{
                            //    secondarySkill.ExecuteIfReady();
                            //}
                            if (secondarySkill && secondarySkill.stock < 1)
                            {
                                secondarySkill.AddOneStock();
                            }
                        }
                        
                    }
                }
            }
        }

                private static void AddLanguageTokens()
        {
            LanguageAPI.Add(name.Replace(" ", "").ToUpper() + "_NAME", name);
            LanguageAPI.Add(name.Replace(" ", "").ToUpper() + "_PICKUP", "Kept you waiting huh");
            LanguageAPI.Add(name.Replace(" ", "").ToUpper() + "_DESC", "Balls");
            LanguageAPI.Add(name.Replace(" ", "").ToUpper() + "_LORE", "");
        }
    }
}
