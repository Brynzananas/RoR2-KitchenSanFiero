using CaeliImperium.Buffs;
using R2API;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using static CaeliImperiumPlugin.CaeliImperium;
using BepInEx.Configuration;
using static BepInEx.Configuration.ConfigFile;
using BepInEx;
using RiskOfOptions.OptionConfigs;
using RiskOfOptions.Options;
using RiskOfOptions;
using BepInEx.Bootstrap;
using CaeliImperiumPlugin;
using static R2API.RecalculateStatsAPI;
using RoR2.Audio;

namespace CaeliImperium.Items
{
    public static class EmergencyMedicalTreatment
    {
        internal static GameObject EmergencyMedicalTreatmentPrefab;
        internal static Sprite EmergencyMedicalTreatmentIcon;
        public static ItemDef EmergencyMedicalTreatmentItemDef;
        public static ConfigEntry<bool> EmergencyMedicalTreatmentEnable;
        public static ConfigEntry<bool> EmergencyMedicalTreatmentEnableConfig;
        public static ConfigEntry<bool> EmergencyMedicalTreatmentAIBlacklist;
        public static ConfigEntry<float> EmergencyMedicalTreatmentTier;
        public static ConfigEntry<float> EmergencyMedicalTreatmentRegen;
        public static ConfigEntry<float> EmergencyMedicalTreatmentRegenStack;
        public static ConfigEntry<float> EmergencyMedicalTreatmentHealth;
        //public static ConfigEntry<bool> EmergencyMedicalTreatmentCooldownType;
        public static ConfigEntry<float> EmergencyMedicalTreatmentStartCooldown;
        //public static ConfigEntry<float> EmergencyMedicalTreatmentFlatCooldownReduction;
        public static ConfigEntry<float> EmergencyMedicalTreatmentPercentageCooldownReduction;
        //public static ConfigEntry<int> EmergencyMedicalTreatmentMaxStackForCooldown;
        public static ConfigEntry<float> EmergencyMedicalTreatmentInvicibilityBase;
        public static ConfigEntry<float> EmergencyMedicalTreatmentInvicibilityPerStack;
        public static ConfigEntry<float> EmergencyMedicalTreatmentBarrierBase;
        public static ConfigEntry<float> EmergencyMedicalTreatmentBarrierPerStack;
        private static NetworkSoundEventDef healSound;

        internal static void Init()
        {
            AddConfigs();
            string tier = "Assets/Icons/EmergencyMedicalTreatmentIcon.png";
            switch (ConfigFloat(EmergencyMedicalTreatmentTier, EmergencyMedicalTreatmentEnableConfig))
            {
                case 1:
                    tier = "Assets/Icons/EmergencyMedicalTreatmentIconTier1.png";
                    break;
                case 2:
                    tier = "Assets/Icons/EmergencyMedicalTreatmentIconTier2.png";
                    break;
                case 3:
                    tier = "Assets/Icons/EmergencyMedicalTreatmentIcon.png";
                    break;

            }
            EmergencyMedicalTreatmentPrefab = MainAssets.LoadAsset<GameObject>("Assets/Models/Prefabs/EmergencyMedicalTreatment.prefab");
            EmergencyMedicalTreatmentIcon = MainAssets.LoadAsset<Sprite>(tier);
            if (!EmergencyMedicalTreatmentEnable.Value)
            {
                return;
            }
            Item();
            CreateSound();
            AddLanguageTokens();
            EmergencyMedicalTreatmentActiveBuff.Init();
            EmergencyMedicalTreatmentCooldownBuff.Init();
        }
        private static void AddConfigs()
        {
            EmergencyMedicalTreatmentEnable = Config.Bind<bool>("Item : Emergency Medical Treatment",
                 "Activation",
                 true,
                 "Enable Emergency Medical Treatment item?");
            EmergencyMedicalTreatmentEnableConfig = Config.Bind<bool>("Item : Emergency Medical Treatment",
                 "Config Activation",
                 false,
                 "Enable config?");
            EmergencyMedicalTreatmentAIBlacklist = Config.Bind<bool>("Item : Emergency Medical Treatment",
                                         "AI Blacklist",
                                         false,
                                         "Blacklist this item from enemies?");
            EmergencyMedicalTreatmentTier = Config.Bind<float>("Item : Emergency Medical Treatment",
                                         "Item tier",
                                         3f,
                                         "1: Common/White\n2: Rare/Green\n3: Legendary/Red");
            EmergencyMedicalTreatmentRegen = Config.Bind<float>("Item : Emergency Medical Treatment",
                                         "Regen",
                                         5f,
                                         "Control the regen increase based on max health in percentage");
            EmergencyMedicalTreatmentRegenStack = Config.Bind<float>("Item : Emergency Medical Treatment",
                                         "Regen stack",
                                         2.5f,
                                         "Control the regen increase based on max health per item stack in percentage");
            EmergencyMedicalTreatmentHealth = Config.Bind<float>("Item : Emergency Medical Treatment",
                                         "Health threshold",
                                         25f,
                                         "Control on which health percentage the item activates");
            //EmergencyMedicalTreatmentCooldownType = Config.Bind<bool>("Item : Emergency Medical Treatment",
            //                             "Cooldown reduction type upon stacking",
            //                             false,
            //                             "Enable: Flat cooldown reduction\nDisable: Percentage cooldown reduction");
            EmergencyMedicalTreatmentStartCooldown = Config.Bind<float>("Item : Emergency Medical Treatment",
                                         "Cooldown",
                                         260f,
                                         "Control the cooldown in seconds");
            //EmergencyMedicalTreatmentFlatCooldownReduction = Config.Bind<float>("Item : Emergency Medical Treatment",
            //                             "Cooldown flat reduction",
            //                             20f,
            //                             "Control how much cooldown is substructed from a start cooldown per stack in seconds");
            EmergencyMedicalTreatmentPercentageCooldownReduction = Config.Bind<float>("Item : Emergency Medical Treatment",
                                         "Cooldown percentage reduction",
                                         15f,
                                         "Control how much cooldown is reduced in percentage");
            //EmergencyMedicalTreatmentMaxStackForCooldown = Config.Bind<int>("Item : Emergency Medical Treatment",
            //                             "Max cooldown reductions",
            //                             10,
            //                             "Control the maximum amount of item stacks before they stop substructing cooldown");
            EmergencyMedicalTreatmentInvicibilityPerStack = Config.Bind<float>("Item : Emergency Medical Treatment",
                                         "Invicibility seconds per item stack",
                                         0f,
                                         "Control invicibility time per item stack in seconds");
            EmergencyMedicalTreatmentBarrierPerStack = Config.Bind<float>("Item : Emergency Medical Treatment",
                             "Barrier per item stack",
                             20f,
                             "Control how much you gain barrier per item stack");
            EmergencyMedicalTreatmentInvicibilityBase = Config.Bind<float>("Item : Emergency Medical Treatment",
                                         "Invicibility seconds",
                                         1f,
                                         "Control base invicibility time in seconds");
            EmergencyMedicalTreatmentBarrierBase = Config.Bind<float>("Item : Emergency Medical Treatment",
                             "Barrier",
                             20f,
                             "Control how much you gain barrier");
            ModSettingsManager.AddOption(new CheckBoxOption(EmergencyMedicalTreatmentEnable, new CheckBoxConfig() { restartRequired = true }));
            ModSettingsManager.AddOption(new CheckBoxOption(EmergencyMedicalTreatmentAIBlacklist, new CheckBoxConfig() { restartRequired = true }));
            ModSettingsManager.AddOption(new StepSliderOption(EmergencyMedicalTreatmentTier, new StepSliderConfig() { min = 1, max = 3, increment = 1f, restartRequired = true }));
            ModSettingsManager.AddOption(new FloatFieldOption(EmergencyMedicalTreatmentRegen));
            ModSettingsManager.AddOption(new FloatFieldOption(EmergencyMedicalTreatmentRegenStack));
            ModSettingsManager.AddOption(new FloatFieldOption(EmergencyMedicalTreatmentHealth));
            //ModSettingsManager.AddOption(new CheckBoxOption(EmergencyMedicalTreatmentCooldownType));
            ModSettingsManager.AddOption(new FloatFieldOption(EmergencyMedicalTreatmentStartCooldown));
            //ModSettingsManager.AddOption(new FloatFieldOption(EmergencyMedicalTreatmentFlatCooldownReduction));
            ModSettingsManager.AddOption(new FloatFieldOption(EmergencyMedicalTreatmentPercentageCooldownReduction));
            //ModSettingsManager.AddOption(new IntFieldOption(EmergencyMedicalTreatmentMaxStackForCooldown));
            ModSettingsManager.AddOption(new FloatFieldOption(EmergencyMedicalTreatmentInvicibilityBase));
            ModSettingsManager.AddOption(new FloatFieldOption(EmergencyMedicalTreatmentBarrierBase));
            ModSettingsManager.AddOption(new FloatFieldOption(EmergencyMedicalTreatmentInvicibilityPerStack));
            ModSettingsManager.AddOption(new FloatFieldOption(EmergencyMedicalTreatmentBarrierPerStack));
        }
        private static void Item()
        {
            EmergencyMedicalTreatmentItemDef = ScriptableObject.CreateInstance<ItemDef>();
            EmergencyMedicalTreatmentItemDef.name = "EmergencyMedicalTreatment";
            EmergencyMedicalTreatmentItemDef.nameToken = "EMERGENCYMEDICALTREATMENT_NAME";
            EmergencyMedicalTreatmentItemDef.pickupToken = "EMERGENCYMEDICALTREATMENT_PICKUP";
            EmergencyMedicalTreatmentItemDef.descriptionToken = "EMERGENCYMEDICALTREATMENT_DESC";
            EmergencyMedicalTreatmentItemDef.loreToken = "EMERGENCYMEDICALTREATMENT_LORE";
            switch (ConfigFloat(EmergencyMedicalTreatmentTier, EmergencyMedicalTreatmentEnableConfig))
            {
                case 1:
                    EmergencyMedicalTreatmentItemDef.deprecatedTier = ItemTier.Tier1;
                    break;
                case 2:
                    EmergencyMedicalTreatmentItemDef.deprecatedTier = ItemTier.Tier2;
                    break;
                case 3:
                    EmergencyMedicalTreatmentItemDef.deprecatedTier = ItemTier.Tier3;
                    break;

            }
            EmergencyMedicalTreatmentItemDef.pickupIconSprite = EmergencyMedicalTreatmentIcon;
            EmergencyMedicalTreatmentItemDef.pickupModelPrefab = EmergencyMedicalTreatmentPrefab;
            EmergencyMedicalTreatmentItemDef.canRemove = true;
            EmergencyMedicalTreatmentItemDef.hidden = false;
            EmergencyMedicalTreatmentItemDef.requiredExpansion = CaeliImperiumExpansionDef;
            var tags = new List<ItemTag>() { ItemTag.Healing, ItemTag.LowHealth };
            if (ConfigBool(EmergencyMedicalTreatmentAIBlacklist, EmergencyMedicalTreatmentEnableConfig))
            {
                tags.Add(ItemTag.AIBlacklist);
            }
            EmergencyMedicalTreatmentItemDef.tags = tags.ToArray();
            ItemDisplayRuleDict rules = new ItemDisplayRuleDict();
            rules.Add("mdlCommandoDualies", new RoR2.ItemDisplayRule[]{
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = EmergencyMedicalTreatmentPrefab,
                    childName = "ThighR",
localPos = new Vector3(-0.11457F, 0.11879F, 0.00996F),
localAngles = new Vector3(3.64506F, 89.63889F, 179.9771F),
localScale = new Vector3(0.14067F, 0.14067F, 0.14067F)
                }
            });
            rules.Add("mdlHuntress", new RoR2.ItemDisplayRule[]{
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = EmergencyMedicalTreatmentPrefab,
                    childName = "ThighR",
localPos = new Vector3(-0.09062F, 0.12277F, 0.05849F),
localAngles = new Vector3(6.8942F, 77.55903F, 171.8056F),
localScale = new Vector3(0.1128F, 0.1128F, 0.1128F)
                }
            });
            rules.Add("mdlBandit2", new RoR2.ItemDisplayRule[]{
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = EmergencyMedicalTreatmentPrefab,
                    childName = "Chest",
localPos = new Vector3(0F, 0.2083F, -0.1688F),
localAngles = new Vector3(0F, 0F, 0F),
localScale = new Vector3(0.11889F, 0.11889F, 0.13623F)
                }
            });
            rules.Add("mdlToolbot", new RoR2.ItemDisplayRule[]{
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = EmergencyMedicalTreatmentPrefab,
                    childName = "Chest",
localPos = new Vector3(-0.62166F, 0.51806F, -1.65629F),
localAngles = new Vector3(359.7561F, 352.608F, 91.54978F),
localScale = new Vector3(1F, 1F, 1F)
                }
            });
            rules.Add("mdlEngi", new RoR2.ItemDisplayRule[]{
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = EmergencyMedicalTreatmentPrefab,
                   childName = "Chest",
localPos = new Vector3(-0.05002F, 0.15491F, -0.31989F),
localAngles = new Vector3(349.4762F, 0F, 0F),
localScale = new Vector3(0.0958F, 0.0958F, 0.0958F)
                }
            });
            //rules.Add("mdlEngiTurrety", new RoR2.ItemDisplayRule[]{
            //    new RoR2.ItemDisplayRule
            //    {
            //        ruleType = ItemDisplayRuleType.ParentedPrefab,
            //        followerPrefab = EmergencyMedicalTreatmentPrefab,
            //        childName = "Chest",
            //        localPos = new Vector3(0f, 0f, 0f),
            //        localAngles = new Vector3(0f, 0f, 0f),
            //        localScale = new Vector3(1f, 1f, 1f)
            //    }
            //});
            rules.Add("mdlMage", new RoR2.ItemDisplayRule[]{
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = EmergencyMedicalTreatmentPrefab,
                    childName = "Chest",
localPos = new Vector3(-0.10853F, 0.0579F, -0.29707F),
localAngles = new Vector3(0F, 0F, 0F),
localScale = new Vector3(0.13405F, 0.13405F, 0.13405F)
                }
            });
            rules.Add("mdlMerc", new RoR2.ItemDisplayRule[]{
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = EmergencyMedicalTreatmentPrefab,
                    childName = "Chest",
localPos = new Vector3(-0.20172F, 0.15582F, -0.14041F),
localAngles = new Vector3(7.86398F, 30.33241F, 339.6068F),
localScale = new Vector3(0.05425F, 0.05425F, 0.05425F)
                }
            });/*
            rules.Add("mdlTreebot", new RoR2.ItemDisplayRule[]{
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = EmergencyMedicalTreatmentPrefab,
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
                    followerPrefab = EmergencyMedicalTreatmentPrefab,
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
                    followerPrefab = EmergencyMedicalTreatmentPrefab,
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
                    followerPrefab = EmergencyMedicalTreatmentPrefab,
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
                    followerPrefab = EmergencyMedicalTreatmentPrefab,
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
                    followerPrefab = EmergencyMedicalTreatmentPrefab,
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
                    followerPrefab = EmergencyMedicalTreatmentPrefab,
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
                    followerPrefab = EmergencyMedicalTreatmentPrefab,
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
                    followerPrefab = EmergencyMedicalTreatmentPrefab,
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
                    followerPrefab = EmergencyMedicalTreatmentPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0f, 0f, 0f),
                    localAngles = new Vector3(0f, 0f, 0f),
                    localScale = new Vector3(1f, 1f, 1f)
                }
            });*/
            var displayRules = new ItemDisplayRuleDict(null);
            ItemAPI.Add(new CustomItem(EmergencyMedicalTreatmentItemDef, displayRules));
            On.RoR2.CharacterBody.OnInventoryChanged += GainBuff;
            On.RoR2.HealthComponent.TakeDamageProcess += OnHitHeal;
            //On.RoR2.CharacterBody.Start += ReAddBuff;
            GetStatCoefficients += Stats;
        }
        private static void CreateSound()
        {
            healSound = ScriptableObject.CreateInstance<NetworkSoundEventDef>();
            healSound.eventName = "Play_medshot";
            R2API.ContentAddition.AddNetworkSoundEventDef(healSound);
        }
        private static void OnHitHeal(On.RoR2.HealthComponent.orig_TakeDamageProcess orig, HealthComponent self, DamageInfo damageInfo)
        {
            if (NetworkServer.active && damageInfo!=null && self!=null && self.body && damageInfo.damage > 0f)
            {
                if (self.body.GetBuffCount(Buffs.EmergencyMedicalTreatmentActiveBuff.EmergencyMedicalTreatmentActiveBuffDef) > 0 && self.health - damageInfo.damage <= self.body.maxHealth * (ConfigFloat(EmergencyMedicalTreatmentHealth, EmergencyMedicalTreatmentEnableConfig) / 100))
                {
                    var itemCount = self.body.inventory.GetItemCount(EmergencyMedicalTreatmentItemDef);
                    float cooldown = ConfigFloat(EmergencyMedicalTreatmentStartCooldown, EmergencyMedicalTreatmentEnableConfig);
                    //if (EmergencyMedicalTreatmentCooldownType.Value)
                    //{
                    //    for (int i = 0; i < itemCount && i < EmergencyMedicalTreatmentMaxStackForCooldown.Value; i++)
                    //    {
                    //        cooldown -= EmergencyMedicalTreatmentFlatCooldownReduction.Value;
                    //    }
                    //}
                    //else
                    //{
                        for (int i = 0; i < itemCount; i++)
                        {
                            cooldown -= cooldown * (ConfigFloat(EmergencyMedicalTreatmentPercentageCooldownReduction, EmergencyMedicalTreatmentEnableConfig) / 100);
                        }
                    //}
                    self.body.SetBuffCount(Buffs.EmergencyMedicalTreatmentActiveBuff.EmergencyMedicalTreatmentActiveBuffDef.buffIndex, 0);
                    self.body.AddTimedBuff(Buffs.EmergencyMedicalTreatmentCooldownBuff.EmergencyMedicalTreatmentCooldownBuffDef, ConfigFloat(EmergencyMedicalTreatmentStartCooldown, EmergencyMedicalTreatmentEnableConfig));
                    self.body.AddTimedBuff(RoR2Content.Buffs.Immune, ConfigFloat(EmergencyMedicalTreatmentInvicibilityBase, EmergencyMedicalTreatmentEnableConfig) + ((itemCount - 1) * ConfigFloat(EmergencyMedicalTreatmentInvicibilityPerStack, EmergencyMedicalTreatmentEnableConfig)));// itemCount * EmergencyMedicalTreatmentInvicibilityPerStack.Value);
                    Util.CleanseBody(self.body, true, false, false, true, true, true);
                    self.barrier += ConfigFloat(EmergencyMedicalTreatmentBarrierBase, EmergencyMedicalTreatmentEnableConfig) + ((itemCount - 1) * ConfigFloat(EmergencyMedicalTreatmentBarrierPerStack, EmergencyMedicalTreatmentEnableConfig));
                    self.HealFraction(1 - self.combinedHealthFraction, default(ProcChainMask));
                    self.RechargeShieldFull();
                    damageInfo.rejected = true;
                    EffectData effectData = new EffectData
                    {
                        origin = self.transform.position
                    };
                    effectData.SetNetworkedObjectReference(self.gameObject);
                    EffectManager.SpawnEffect(LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/HealingPotionEffect"), effectData, true);
                    EntitySoundManager.EmitSoundServer(healSound.akId, self.gameObject);
                }
            }
            orig(self, damageInfo);
        }

        private static void Stats(CharacterBody sender, StatHookEventArgs args)
        {
            int count = sender.inventory ? sender.inventory.GetItemCount(EmergencyMedicalTreatmentItemDef) : 0;
            if (count > 0)
            {
                args.baseRegenAdd += sender.maxHealth * (ConfigFloat(EmergencyMedicalTreatmentRegen, EmergencyMedicalTreatmentEnableConfig) / 100) + ((count - 1) * ConfigFloat(EmergencyMedicalTreatmentRegenStack, EmergencyMedicalTreatmentEnableConfig));
            }
        }

        public class EmergencyMedicalTreatmentBehaviour : RoR2.CharacterBody.ItemBehavior
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
                    if (count > 0 && !body.HasBuff(EmergencyMedicalTreatmentActiveBuff.EmergencyMedicalTreatmentActiveBuffDef) && !body.HasBuff(EmergencyMedicalTreatmentCooldownBuff.EmergencyMedicalTreatmentCooldownBuffDef))
                    {
                        body.AddBuff(EmergencyMedicalTreatmentActiveBuff.EmergencyMedicalTreatmentActiveBuffDef);

                    }

                }
            }
            public void OnDisable()
            {
                if (body)
                {
                    body.RemoveBuff(EmergencyMedicalTreatmentCooldownBuff.EmergencyMedicalTreatmentCooldownBuffDef);
                    body.RemoveBuff(EmergencyMedicalTreatmentActiveBuff.EmergencyMedicalTreatmentActiveBuffDef);
                }
            }
        }
        /*
        private static void ReAddBuff(On.RoR2.CharacterBody.orig_Start orig, CharacterBody self)
        {
            orig(self);
            int itemCount = self.inventory ? self.inventory.GetItemCount(EmergencyMedicalTreatmentItemDef) : 0;

            if (!self.HasBuff(EmergencyMedicalTreatmentActiveBuff.EmergencyMedicalTreatmentActiveBuffDef) && !self.HasBuff(EmergencyMedicalTreatmentCooldownBuff.EmergencyMedicalTreatmentCooldownBuffDef) && itemCount > 0)
            {
                self.AddBuff(EmergencyMedicalTreatmentActiveBuff.EmergencyMedicalTreatmentActiveBuffDef);
            }
        }
        */
        private static void GainBuff(On.RoR2.CharacterBody.orig_OnInventoryChanged orig, CharacterBody self)
        {
            orig(self);
            if (NetworkServer.active)
            {
                int itemCount = self.inventory ? self.inventory.GetItemCount(EmergencyMedicalTreatmentItemDef) : 0;
                self.AddItemBehavior<EmergencyMedicalTreatmentBehaviour>(itemCount);
            }
            /*
            if (self.inventory.GetItemCount(EmergencyMedicalTreatmentItemDef) > 0 && !self.HasBuff(Buffs.EmergencyMedicalTreatmentActiveBuff.EmergencyMedicalTreatmentActiveBuffDef) && !self.HasBuff(Buffs.EmergencyMedicalTreatmentCooldownBuff.EmergencyMedicalTreatmentCooldownBuffDef))
            {
                self.AddBuff(Buffs.EmergencyMedicalTreatmentActiveBuff.EmergencyMedicalTreatmentActiveBuffDef);
            }
            if (self.inventory.GetItemCount(EmergencyMedicalTreatmentItemDef) <= 0 && (self.HasBuff(Buffs.EmergencyMedicalTreatmentActiveBuff.EmergencyMedicalTreatmentActiveBuffDef) || self.HasBuff(Buffs.EmergencyMedicalTreatmentCooldownBuff.EmergencyMedicalTreatmentCooldownBuffDef)))
            {
                self.SetBuffCount(EmergencyMedicalTreatmentCooldownBuff.EmergencyMedicalTreatmentCooldownBuffDef.buffIndex, 0);
                self.SetBuffCount(EmergencyMedicalTreatmentActiveBuff.EmergencyMedicalTreatmentActiveBuffDef.buffIndex, 0);
            }*/
        }
        /*
        private static void OnHitHeal(On.RoR2.HealthComponent.orig_UpdateLastHitTime orig, HealthComponent self, float damageValue, Vector3 damagePosition, bool damageIsSilent, GameObject attacker)
        {
            if (NetworkServer.active && self.body && damageValue > 0f)
            {
                if (self.body.GetBuffCount(Buffs.EmergencyMedicalTreatmentActiveBuff.EmergencyMedicalTreatmentActiveBuffDef) > 0 && self.isHealthLow)
                {
                    var itemCount = self.body.inventory.GetItemCount(EmergencyMedicalTreatmentItemDef);
                    float cooldownReduction = 0;
                    for (int i = 0; i < itemCount && i < EmergencyMedicalTreatmentMaxStackForCooldown.Value; i++)
                    {
                        cooldownReduction += EmergencyMedicalTreatmentCooldownReduction.Value;
                    }
                    self.body.RemoveBuff(Buffs.EmergencyMedicalTreatmentActiveBuff.EmergencyMedicalTreatmentActiveBuffDef);
                    self.body.AddTimedBuff(Buffs.EmergencyMedicalTreatmentCooldownBuff.EmergencyMedicalTreatmentCooldownBuffDef, EmergencyMedicalTreatmentStartCooldown.Value - cooldownReduction);
                    self.body.AddTimedBuff(RoR2Content.Buffs.Immune, itemCount * EmergencyMedicalTreatmentInvicibilityMultiplier.Value);
                    Util.CleanseBody(self.body, true, false, false, true, true, true);
                    self.barrier += itemCount * EmergencyMedicalTreatmentBarrierMultiplier.Value;
                    self.HealFraction(0.75f, default(ProcChainMask));
                    self.RechargeShieldFull();
                    damageValue = 0;
                    EffectData effectData = new EffectData
                    {
                        origin = self.transform.position
                    };
                    effectData.SetNetworkedObjectReference(self.gameObject);
                    EffectManager.SpawnEffect(LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/HealingPotionEffect"), effectData, true);
                }
            }
            orig(self, damageValue, damagePosition, damageIsSilent, attacker);

        }*/

        private static void AddLanguageTokens()
        {
            string stackType = "";
            //if (EmergencyMedicalTreatmentCooldownType.Value)
            //{
            //    stackType = " <style=cStack>(-" + EmergencyMedicalTreatmentFlatCooldownReduction.Value + "seconds per item stack)</style>";
            //}
            //else
            //{
                stackType = " <style=cStack>(-" + ConfigFloat(EmergencyMedicalTreatmentPercentageCooldownReduction, EmergencyMedicalTreatmentEnableConfig) + "% per item stack)</style>";
            //}
            LanguageAPI.Add("EMERGENCYMEDICALTREATMENT_NAME", "Emergency Medical Treatment");
            LanguageAPI.Add("EMERGENCYMEDICALTREATMENT_PICKUP", "Taking damage to bellow <style=cIsHealth>" + ConfigFloat(EmergencyMedicalTreatmentHealth, EmergencyMedicalTreatmentEnableConfig) + "% health</style>, <style=cIsHealing>fully heal</style>. Recharges in " + ConfigFloat(EmergencyMedicalTreatmentStartCooldown, EmergencyMedicalTreatmentEnableConfig) + stackType + " seconds. Increase <style=cIsHealing>regen</style> by <style=cIsHealing>" + ConfigFloat(EmergencyMedicalTreatmentRegen, EmergencyMedicalTreatmentEnableConfig) + "%</style> <style=cStack>(+" + ConfigFloat(EmergencyMedicalTreatmentRegenStack, EmergencyMedicalTreatmentEnableConfig) + "% per item stack)</style> of the <style=cIsHealing>maximum health</style>");
            LanguageAPI.Add("EMERGENCYMEDICALTREATMENT_DESC", "Taking damage to bellow <style=cIsHealth>" + ConfigFloat(EmergencyMedicalTreatmentHealth, EmergencyMedicalTreatmentEnableConfig) + "% health</style>, <style=cIsHealing>fully heal</style>. Recharges in " + ConfigFloat(EmergencyMedicalTreatmentStartCooldown, EmergencyMedicalTreatmentEnableConfig) + stackType + " seconds. Increase <style=cIsHealing>regen</style> by <style=cIsHealing>" + ConfigFloat(EmergencyMedicalTreatmentRegen, EmergencyMedicalTreatmentEnableConfig) + "%</style> <style=cStack>(+" + ConfigFloat(EmergencyMedicalTreatmentRegenStack, EmergencyMedicalTreatmentEnableConfig) + "% per item stack)</style> of the <style=cIsHealing>maximum health</style>");
            LanguageAPI.Add("EMERGENCYMEDICALTREATMENT_LORE", "");
        }

       
    }
}
