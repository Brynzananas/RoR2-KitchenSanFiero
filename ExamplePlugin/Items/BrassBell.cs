using BepInEx.Configuration;
using IL.RoR2.Items;
using CaeliImperium.Buffs;
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
using static CaeliImperiumPlugin.CaeliImperium;
using System.Runtime.CompilerServices;
using static CaeliImperium.Items.EmergencyMedicalTreatment;
using RoR2.Audio;
using static UnityEditorInternal.ReorderableList;

namespace CaeliImperium.Items
{
    public static class BrassBell
    {
        internal static GameObject BrassBellPrefab;
        internal static Sprite BrassBellIcon;
        public static ItemDef BrassBellItemDef;

        public static ConfigEntry<bool> BrassBellEnable;
        public static ConfigEntry<bool> BrassBellEnableConfig;
        public static ConfigEntry<bool> BrassBellAIBlacklist;
        public static ConfigEntry<float> BrassBellTier;
        public static ConfigEntry<float> BrassBellCooldown;
        public static ConfigEntry<float> BrassBellCooldownStack;
        public static ConfigEntry<float> BrassBellEffectTime;
        public static ConfigEntry<float> BrassBellEffectTimeStack;
        public static ConfigEntry<float> BrassBellDamageIncrease;
        public static ConfigEntry<float> BrassBellDamageIncreaseStack;
        public static ConfigEntry<bool> BrassBellIsReloadSecondary;
        public static ConfigEntry<bool> BrassBellIsReloadutility;
        public static ConfigEntry<bool> BrassBellIsReloadSpecial;
        public static ConfigEntry<bool> BrassBellIsReloadSound;
        public static NetworkSoundEventDef BellSound;
        public static string name = "Brass Bell";
        internal static void Init()
        {
            AddConfigs();
            Debug.Log(BrassBellTier.DefaultValue);
            string tier = "Assets/Icons/BrassBellIcon.png";
            switch (ConfigFloat(BrassBellTier, BrassBellEnableConfig))
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

            BrassBellPrefab = MainAssets.LoadAsset<GameObject>("Assets/Models/Prefabs/hotelbell.prefab");
            BrassBellIcon = MainAssets.LoadAsset<Sprite>(tier);
            if (!BrassBellEnable.Value)
            {
                return;
            }
            Item();
            CreateSound();
            AddLanguageTokens();
            BrassBoostedBuff.Init();
            BrassTimerBuff.Init();
        }
        public static void AddConfigs()
        {
            BrassBellEnable = Config.Bind<bool>("Item : " + name,
                 "Activation",
                 true,
                 "Enable this item?");//\nDefault value: " + BrassBellEnable.DefaultValue);
            BrassBellEnableConfig = Config.Bind<bool>("Item : " + name,
                 "Config activation",
                 false,
                 "Enable config?");

            BrassBellAIBlacklist = Config.Bind<bool>("Item : " + name,
                                         "AI Blacklist",
                                         false,
                                         "Blacklist this item from enemies?");//\nDefault value: " + BrassBellAIBlacklist.DefaultValue);
            BrassBellTier = Config.Bind<float>("Item : " + name,
                                         "Item tier",
                                         2f,
                                         "1: Common/White\n2: Rare/Green\n3: Legendary/Red");//\nDefault value: " + BrassBellTier.DefaultValue);
            BrassBellCooldown = Config.Bind<float>("Item : " + name,
                                         "Interval between effects",
                                         15f,
                                         "Control the interval for this item activation");//\nDefault value: " + BrassBellCooldown.DefaultValue);
            BrassBellCooldownStack = Config.Bind<float>("Item : " + name,
                                         "Interval reduction",
                                         0f,
                                         "Control the interval reduction per stack in percentage\nSet to 0 to disable stacking");//\nDefault value: " + BrassBellCooldownStack.DefaultValue);
            BrassBellEffectTime = Config.Bind<float>("Item : " + name,
                                         "Effect time",
                                         1f,
                                         "Control how long this item effect lasts in seconds");//\nDefault value: " + BrassBellEffectTime.DefaultValue);
            BrassBellEffectTimeStack = Config.Bind<float>("Item : " + name,
                                         "Effect time stack",
                                         0f,
                                         "Control addition effect duration in seconds\nSet it to 0 to disable stacking");//\nDefault value: " + BrassBellEffectTimeStack.DefaultValue);
            BrassBellDamageIncrease = Config.Bind<float>("Item : " + name,
                                         "Damage increase",
                                         80f,
                                         "Control the damage increase in percentage");//\nDefault value: " + BrassBellDamageIncrease.DefaultValue);
            BrassBellDamageIncreaseStack = Config.Bind<float>("Item : " + name,
                                         "Damage increase stack",
                                         80f,
                                         "Control the damage increase stack in percentage\nSet it to 0 to disable stacking");//\nDefault value: " + BrassBellDamageIncreaseStack.DefaultValue);
            BrassBellIsReloadSecondary = Config.Bind<bool>("Item : " + name,
                                         "Secondary skill reload",
                                         true,
                                         "Will this item reload secondary skill on activation?");//\nDefault value: " + BrassBellIsReloadSecondary.DefaultValue);
            BrassBellIsReloadutility = Config.Bind<bool>("Item : " + name,
                                         "Utility skill reload",
                                         false,
                                         "Will this item reload utility skill on activation?");//\nDefault value: " + BrassBellIsReloadutility.DefaultValue);
            BrassBellIsReloadSpecial = Config.Bind<bool>("Item : " + name,
                                         "Special skill reload",
                                          false,
                                         "Will this item reload special skill on activation?");//\nDefault value: " + BrassBellIsReloadSpecial.DefaultValue);
            BrassBellIsReloadSound = Config.Bind<bool>("Item : " + name,
                                         "|Sound|",
                                         false,
                                         "Play sound on activation?");//\nDefault value: " + BrassBellIsReloadSound.DefaultValue);

            ModSettingsManager.AddOption(new CheckBoxOption(BrassBellEnable, new CheckBoxConfig() { restartRequired = true }));
            ModSettingsManager.AddOption(new CheckBoxOption(BrassBellEnableConfig, new CheckBoxConfig() { restartRequired = true }));
            ModSettingsManager.AddOption(new CheckBoxOption(BrassBellAIBlacklist, new CheckBoxConfig() { restartRequired = true }));
            ModSettingsManager.AddOption(new StepSliderOption(BrassBellTier, new StepSliderConfig() { min = 1, max = 3, increment = 1f, restartRequired = true }));
            ModSettingsManager.AddOption(new FloatFieldOption(BrassBellCooldown));
            ModSettingsManager.AddOption(new FloatFieldOption(BrassBellCooldownStack));
            ModSettingsManager.AddOption(new FloatFieldOption(BrassBellEffectTime));
            ModSettingsManager.AddOption(new FloatFieldOption(BrassBellEffectTimeStack));
            ModSettingsManager.AddOption(new FloatFieldOption(BrassBellDamageIncrease));
            ModSettingsManager.AddOption(new FloatFieldOption(BrassBellDamageIncreaseStack));
            ModSettingsManager.AddOption(new CheckBoxOption(BrassBellIsReloadSecondary));
            ModSettingsManager.AddOption(new CheckBoxOption(BrassBellIsReloadutility));
            ModSettingsManager.AddOption(new CheckBoxOption(BrassBellIsReloadSpecial));
            //ModSettingsManager.AddOption(new CheckBoxOption(BrassBellIsReloadSound));
        }
        public static void Item()
        {

            BrassBellItemDef = ScriptableObject.CreateInstance<ItemDef>();
            BrassBellItemDef.name = name.Replace(" ", "");
            BrassBellItemDef.nameToken = name.Replace(" ", "").ToUpper() + "_NAME";
            BrassBellItemDef.pickupToken = name.Replace(" ", "").ToUpper() + "_PICKUP";
            BrassBellItemDef.descriptionToken = name.Replace(" ", "").ToUpper() + "_DESC";
            BrassBellItemDef.loreToken = name.Replace(" ", "").ToUpper() + "_LORE";
            switch (ConfigFloat(BrassBellTier, BrassBellEnableConfig))
            {
                case 1:
                    BrassBellItemDef.deprecatedTier = ItemTier.Tier1;
                    break;
                case 2:
                    BrassBellItemDef.deprecatedTier = ItemTier.Tier2;
                    break;
                case 3:
                    BrassBellItemDef.deprecatedTier = ItemTier.Tier3;
                    break;


            }
            BrassBellItemDef.pickupIconSprite = BrassBellIcon;
            BrassBellItemDef.pickupModelPrefab = BrassBellPrefab;
            BrassBellItemDef.canRemove = true;
            BrassBellItemDef.hidden = false;
            BrassBellItemDef.requiredExpansion = CaeliImperiumExpansionDef;
            var tags = new List<ItemTag>() { ItemTag.Damage };
            if (ConfigBool(BrassBellAIBlacklist, BrassBellEnableConfig))
            {
                tags.Add(ItemTag.AIBlacklist);
            }
            BrassBellItemDef.tags = tags.ToArray();
            ItemDisplayRuleDict rules = new ItemDisplayRuleDict();
            rules.Add("mdlCommandoDualies", new RoR2.ItemDisplayRule[]{
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = BrassBellPrefab,
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
                    followerPrefab = BrassBellPrefab,
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
                    followerPrefab = BrassBellPrefab,
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
                    followerPrefab = BrassBellPrefab,
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
                    followerPrefab = BrassBellPrefab,
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
                    followerPrefab = BrassBellPrefab,
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
                    followerPrefab = BrassBellPrefab,
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
                    followerPrefab = BrassBellPrefab,
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
                    followerPrefab = BrassBellPrefab,
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
                    followerPrefab = BrassBellPrefab,
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
                    followerPrefab = BrassBellPrefab,
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
                    followerPrefab = BrassBellPrefab,
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
                    followerPrefab = BrassBellPrefab,
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
                    followerPrefab = BrassBellPrefab,
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
                    followerPrefab = BrassBellPrefab,
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
                    followerPrefab = BrassBellPrefab,
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
                    followerPrefab = BrassBellPrefab,
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
                    followerPrefab = BrassBellPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0f, 0f, 0f),
                    localAngles = new Vector3(0f, 0f, 0f),
                    localScale = new Vector3(1f, 1f, 1f)
                }
            });
            var displayRules = new ItemDisplayRuleDict(null);
            ItemAPI.Add(new CustomItem(BrassBellItemDef, displayRules));
            On.RoR2.CharacterBody.OnInventoryChanged += AddBuff;
            //On.RoR2.CharacterBody.Start += ReAddBuff;
        }
        public class BrassBellBehaviour : RoR2.CharacterBody.ItemBehavior
        {
            public bool appliedRegen;
            private void Awake()
            {
                base.enabled = false;
                
            }
            public void FixedUpdate()
            {
                if (body)
                {
                    int count = stack;
                    if (count > 0 && !body.HasBuff(BrassBoostedBuff.BrassBoostedBuffDef) && !body.HasBuff(BrassTimerBuff.BrassTimerBuffDef))
                    {
                        /*
                        if (body.skillLocator.secondary.stock <= 0 && BrassBell.BrassBellIsReloadSecondary.Value)
                        {
                            body.skillLocator.secondary.AddOneStock();

                        }
                        if (body.skillLocator.utility.stock <= 0 && BrassBell.BrassBellIsReloadutility.Value)
                        {
                            body.skillLocator.utility.AddOneStock();

                        }
                        if (body.skillLocator.special.stock <= 0 && BrassBell.BrassBellIsReloadSpecial.Value)
                        {
                            body.skillLocator.secondary.AddOneStock();

                        }*/
                        float cooldown = ConfigFloat(BrassBellCooldown, BrassBellEnableConfig);
                        if (ConfigFloat(BrassBellCooldownStack, BrassBellEnableConfig) != 0)
                        {
                            for (int i = 0; i < count - 1; i++)
                            {
                                cooldown -= cooldown * (ConfigFloat(BrassBellCooldownStack, BrassBellEnableConfig) / 100);
                            }
                        }
                        
                        body.AddTimedBuff(BrassTimerBuff.BrassTimerBuffDef, cooldown);

                    }

                }
            }
            public void OnDisable()
            {
                if (body)
                {
                    body.RemoveBuff(BrassTimerBuff.BrassTimerBuffDef);
                    body.RemoveBuff(BrassBoostedBuff.BrassBoostedBuffDef);
                }
            }
        }/*
        private static void ReAddBuff(On.RoR2.CharacterBody.orig_Start orig, CharacterBody self)
        {
            orig(self);
            int itemCount = self.inventory ? self.inventory.GetItemCount(BrassBellItemDef) : 0;

            if (!self.HasBuff(BrassBoostedBuff.BrassBoostedBuffDef) && !self.HasBuff(BrassTimerBuff.BrassTimerBuffDef) && itemCount > 0)
            {
                self.AddTimedBuff(BrassTimerBuff.BrassTimerBuffDef, BrassBellCooldown.Value);
            }
        }*/
        private static void CreateSound()
        {
            BellSound = ScriptableObject.CreateInstance<NetworkSoundEventDef>();
            BellSound.eventName = "Play_bell";
            R2API.ContentAddition.AddNetworkSoundEventDef(BellSound);
        }
        private static void AddBuff(On.RoR2.CharacterBody.orig_OnInventoryChanged orig, CharacterBody self)
        {
            orig(self);
            int itemCount = self.inventory ? self.inventory.GetItemCount(BrassBellItemDef) : 0;
            self.AddItemBehavior<BrassBellBehaviour>(itemCount);
            /*
            if (!self.HasBuff(BrassBoostedBuff.BrassBoostedBuffDef) && !self.HasBuff(BrassTimerBuff.BrassTimerBuffDef) && itemCount > 0)
            {
                self.AddTimedBuff(BrassTimerBuff.BrassTimerBuffDef, BrassBellCooldown.Value);
            }
            if (itemCount <= 0 && (self.HasBuff(BrassBoostedBuff.BrassBoostedBuffDef) || self.HasBuff(BrassTimerBuff.BrassTimerBuffDef)))
            {
                self.SetBuffCount(BrassBoostedBuff.BrassBoostedBuffDef.buffIndex, 0);
                self.SetBuffCount(BrassTimerBuff.BrassTimerBuffDef.buffIndex, 0);
            }*/
        }

        public static void AddLanguageTokens()
        {
            string configSkills = ".";
            if (ConfigBool(BrassBellIsReloadSecondary, BrassBellEnableConfig) || ConfigBool(BrassBellIsReloadutility, BrassBellEnableConfig) || ConfigBool(BrassBellIsReloadSpecial, BrassBellEnableConfig))
            {
                configSkills += " Reload ";

                if (ConfigBool(BrassBellIsReloadSecondary, BrassBellEnableConfig))
                {
                    configSkills += "<style=cIsUtility>secondary</style>";
                }
                if (ConfigBool(BrassBellIsReloadutility, BrassBellEnableConfig) || ConfigBool(BrassBellIsReloadSpecial, BrassBellEnableConfig))
                {
                    configSkills += ",";
                }
                if (ConfigBool(BrassBellIsReloadutility, BrassBellEnableConfig))
                {
                    configSkills += " <style=cIsUtility>utility</style>";
                    
                }
                if (ConfigBool(BrassBellIsReloadSpecial, BrassBellEnableConfig))
                {
                    configSkills += ",";
                }
                if (ConfigBool(BrassBellIsReloadSpecial, BrassBellEnableConfig))
                {
                    configSkills += "<style=cIsUtility>special</style> ";
                }
                configSkills += " <style=cIsUtility>skills</style> on effect activation.";

            }
            string cooldownStack = "";
            if (ConfigFloat(BrassBellCooldownStack, BrassBellEnableConfig) != 0)
            {
                cooldownStack = " <style=cStack>(-" + ConfigFloat(BrassBellCooldownStack, BrassBellEnableConfig) + "% per item stack)</style>";
            }
            string damageStack = "";
            if (ConfigFloat(BrassBellDamageIncreaseStack, BrassBellEnableConfig) != 0)
            {
                damageStack = " <style=cStack>(+" + ConfigFloat(BrassBellDamageIncreaseStack, BrassBellEnableConfig) + "% per item stack)</style>";
            }
            string timeStack = "";
            if (ConfigFloat(BrassBellEffectTimeStack, BrassBellEnableConfig) != 0)
            {
                timeStack = " <style=cStack>(+" + ConfigFloat(BrassBellEffectTimeStack, BrassBellEnableConfig) + " per item stack)</style>";
            }
            LanguageAPI.Add(name.Replace(" ", "").ToUpper() + "_NAME", name);
            LanguageAPI.Add(name.Replace(" ", "").ToUpper() + "_PICKUP", "Every " + ConfigFloat(BrassBellCooldown, BrassBellEnableConfig) + "" + cooldownStack + " seconds increase your damage by <style=cIsDamage>" + ConfigFloat(BrassBellDamageIncrease, BrassBellEnableConfig) + "%</style>" + damageStack + " for " + ConfigFloat(BrassBellEffectTime, BrassBellEnableConfig) + "" + timeStack + " seconds" + configSkills);
            LanguageAPI.Add(name.Replace(" ", "").ToUpper() + "_DESC", "Every " + ConfigFloat(BrassBellCooldown, BrassBellEnableConfig) + "" + cooldownStack + " seconds increase your damage by <style=cIsDamage>" + ConfigFloat(BrassBellDamageIncrease, BrassBellEnableConfig) + "%</style>" + damageStack + " for " + ConfigFloat(BrassBellEffectTime, BrassBellEnableConfig) + "" + timeStack + " seconds" + configSkills);
            LanguageAPI.Add(name.Replace(" ", "").ToUpper() + "_LORE", "");
        }
    }
}
