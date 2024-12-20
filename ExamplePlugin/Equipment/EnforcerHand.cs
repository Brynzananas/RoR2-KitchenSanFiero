﻿using BepInEx.Configuration;
using CaeliImperium.Elites;
using CaeliImperium.Items;
using R2API;
using RiskOfOptions.Options;
using RiskOfOptions;
using RoR2;
using RoR2.Audio;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using UnityEngine;
using static CaeliImperiumPlugin.CaeliImperium;
using CaeliImperium.Buffs;
using RiskOfOptions.OptionConfigs;

namespace CaeliImperium.Equipment
{
    public static class EnforcerHand
    {
        internal static GameObject EnforcerHandPrefab;
        internal static Sprite EnforcerHandiconIcon;
        static EquipmentDef EnforcerHandEquipDef;
        static GameObject NecronomiconMasterprefab;
        static Vector3 NecronomiconPosition;
        static bool isRespawning = false;
        public static NetworkSoundEventDef ShielRaiseSound;
        public static ConfigEntry<bool> EnforcerHandEnableConfig;
        public static ConfigEntry<float> EnforcerHandTimeWindow;
        public static ConfigEntry<float> EnforcerHandReflectDamageMultiplier;
        public static ConfigEntry<float> EnforcerHandTotalDamageMultiplier;
        public static ConfigEntry<float> EnforcerHandMaxHealthDamageMulyiplier;
        public static ConfigEntry<int> EnforcerHandWoundedCount;
        public static ConfigEntry<float> EnforcerHandWoundedTime;
        public static ConfigEntry<bool> EnforcerHandDoStun;
        public static ConfigEntry<bool> EnforcerHandEnable;
        public static ConfigEntry<float> EnforcerHandCooldownDeduction;
        public static ConfigEntry<float> EnforcerHandCooldown;
        public static ConfigEntry<float> EnforcerHandImmunity;
        public static ConfigEntry<float> EnforcerHandViewAngle;

        public static void Init()
        {
            AddConfigs();

            EnforcerHandPrefab = MainAssets.LoadAsset<GameObject>("Assets/Models/Prefabs/EnforcerHand.prefab");
            EnforcerHandiconIcon = MainAssets.LoadAsset<Sprite>("Assets/Icons/EnforcerHand.png");
            if (!EnforcerHandEnable.Value)
            {
                return;
            }
            Item();
            CreateSound();
            AddLanguageTokens();
            ParryNextDamageBuff.Init();
        }
        public static void AddConfigs()
        {
            EnforcerHandEnableConfig = Config.Bind<bool>("Equipment : Enforcer Hand",
                                         "Config Activation",
                                         false,
                                         "Enable config?");
            EnforcerHandTimeWindow = Config.Bind<float>("Equipment : Enforcer Hand",
                                         "Time window",
                                         1f,
                                         "Control the time window of the parry");
            EnforcerHandReflectDamageMultiplier = Config.Bind<float>("Equipment : Enforcer Hand",
                                         "Reflect damage multiplier",
                                         2f,
                                         "Control the reflect damage multiplier upon the parry");
            EnforcerHandTotalDamageMultiplier = Config.Bind<float>("Equipment : Enforcer Hand",
                                         "Total damage percentage",
                                         400f,
                                         "Control the total damage percentage upon the parry");
            EnforcerHandMaxHealthDamageMulyiplier = Config.Bind<float>("Equipment : Enforcer Hand",
                                         "Max health damage percentage",
                                         5f,
                                         "Control the max health damage percentage upon the parry");
            EnforcerHandCooldownDeduction = Config.Bind<float>("Equipment : Enforcer Hand",
                                         "Cooldown decuction",
                                         15f,
                                         "Control the equipment cooldown deduction upon the parry in seconds");
            EnforcerHandDoStun = Config.Bind<bool>("Equipment : Enforcer Hand",
                                         "Stun",
                                         true,
                                         "Stun the enemy upon the parry?");
            EnforcerHandEnable = Config.Bind<bool>("Equipment : Enforcer Hand",
                                         "Activation",
                                         true,
                                         "Enable Enforcer Hand equipment?");
            EnforcerHandWoundedCount = Config.Bind<int>("Equipment : Enforcer Hand",
                                         "Wound amount",
                                         1,
                                         "Control the amount of the Wounded debuff applied upon the parry");
            EnforcerHandWoundedTime = Config.Bind<float>("Equipment : Enforcer Hand",
                                         "Wound time",
                                         10,
                                         "Control the time of the Wounded debuff applied upon the parry in seconds\nSet it to 0 to make it permanent");
            EnforcerHandCooldown = Config.Bind<float>("Equipment : Enforcer Hand",
                             "Cooldown",
                             15f,
                             "Control the equipment cooldown");
            EnforcerHandImmunity = Config.Bind<float>("Equipment : Enforcer Hand",
                             "Immunity",
                             0f,
                             "Control the immunity time after the parry in seconds");
            EnforcerHandViewAngle = Config.Bind<float>("Equipment : Enforcer Hand",
                             "View Angle",
                             70f,
                             "Control the view angle parry");
            ModSettingsManager.AddOption(new CheckBoxOption(EnforcerHandEnable, new CheckBoxConfig() { restartRequired = true }));
            ModSettingsManager.AddOption(new CheckBoxOption(EnforcerHandEnableConfig, new CheckBoxConfig() { restartRequired = true }));
            ModSettingsManager.AddOption(new FloatFieldOption(EnforcerHandCooldown, new FloatFieldConfig() { restartRequired = true }));
            ModSettingsManager.AddOption(new FloatFieldOption(EnforcerHandViewAngle));
            ModSettingsManager.AddOption(new FloatFieldOption(EnforcerHandTimeWindow));
            ModSettingsManager.AddOption(new FloatFieldOption(EnforcerHandReflectDamageMultiplier));
            ModSettingsManager.AddOption(new FloatFieldOption(EnforcerHandMaxHealthDamageMulyiplier));
            ModSettingsManager.AddOption(new FloatFieldOption(EnforcerHandTotalDamageMultiplier));
            ModSettingsManager.AddOption(new IntFieldOption(EnforcerHandWoundedCount));
            ModSettingsManager.AddOption(new FloatFieldOption(EnforcerHandWoundedTime));
            ModSettingsManager.AddOption(new CheckBoxOption(EnforcerHandDoStun));
            ModSettingsManager.AddOption(new FloatFieldOption(EnforcerHandCooldownDeduction));
            ModSettingsManager.AddOption(new FloatFieldOption(EnforcerHandImmunity));
        }
        public static void Item()
        {
            EnforcerHandEquipDef = ScriptableObject.CreateInstance<EquipmentDef>();
            EnforcerHandEquipDef.name = "EnforcerHand";
            EnforcerHandEquipDef.nameToken = "ENFORCERHAND_NAME";
            EnforcerHandEquipDef.pickupToken = "ENFORCERHAND_PICKUP";
            EnforcerHandEquipDef.descriptionToken = "ENFORCERHAND_DESC";
            EnforcerHandEquipDef.loreToken = "ENFORCERHAND_LORE";
            EnforcerHandEquipDef.pickupIconSprite = EnforcerHandiconIcon;
            EnforcerHandEquipDef.pickupModelPrefab = EnforcerHandPrefab;
            EnforcerHandEquipDef.appearsInMultiPlayer = true;
            EnforcerHandEquipDef.appearsInSinglePlayer = true;
            EnforcerHandEquipDef.canBeRandomlyTriggered = false;
            EnforcerHandEquipDef.canDrop = true;
            EnforcerHandEquipDef.requiredExpansion = CaeliImperiumExpansionDef;
            EnforcerHandEquipDef.cooldown = ConfigFloat(EnforcerHandCooldown, EnforcerHandEnableConfig);
            ItemDisplayRuleDict rules = new ItemDisplayRuleDict();
            rules.Add("mdlCommandoDualies", new RoR2.ItemDisplayRule[]{
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = EnforcerHandPrefab,
                    childName = "Chest",
localPos = new Vector3(-0.10889F, 0.33533F, 0.00002F),
localAngles = new Vector3(0F, 0F, 0F),
localScale = new Vector3(0.30991F, 0.30991F, 0.30991F)
                }
            });
            rules.Add("mdlHuntress", new RoR2.ItemDisplayRule[]{
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = EnforcerHandPrefab,
                    childName = "Chest",
localPos = new Vector3(-0.07711F, 0.23497F, -0.03041F),
localAngles = new Vector3(0F, 326.6053F, 0F),
localScale = new Vector3(0.32663F, 0.32663F, 0.32663F)
                }
            });
            rules.Add("mdlBandit2", new RoR2.ItemDisplayRule[]{
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = EnforcerHandPrefab,
                    childName = "Chest",
localPos = new Vector3(-0.07881F, 0.25489F, -0.07988F),
localAngles = new Vector3(0F, 302.05F, 0F),
localScale = new Vector3(0.32271F, 0.32271F, 0.32271F)
                }
            });
            rules.Add("mdlToolbot", new RoR2.ItemDisplayRule[]{
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = EnforcerHandPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0f, 0f, 0f),
                    localAngles = new Vector3(0f, 0f, 0f),
                    localScale = new Vector3(1f, 1f, 1f)
                }
            });
            rules.Add("mdlEngi", new RoR2.ItemDisplayRule[]{
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = EnforcerHandPrefab,
                    childName = "Chest",
localPos = new Vector3(-0.27198F, 0.352F, 0.00002F),
localAngles = new Vector3(0F, 0F, 19.02883F),
localScale = new Vector3(0.359F, 0.359F, 0.359F)
                }
            });/*
            rules.Add("mdlEngiTurrety", new RoR2.ItemDisplayRule[]{
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = EnforcerHandPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0f, 0f, 0f),
                    localAngles = new Vector3(0f, 0f, 0f),
                    localScale = new Vector3(1f, 1f, 1f)
                }
            });*/
            rules.Add("mdlMage", new RoR2.ItemDisplayRule[]{
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = EnforcerHandPrefab,
                    childName = "Chest",
localPos = new Vector3(-0.10535F, 0.22892F, -0.02295F),
localAngles = new Vector3(359.4844F, 334.8223F, 357.4088F),
localScale = new Vector3(0.30409F, 0.30409F, 0.30409F)
                }
            });
            rules.Add("mdlMerc", new RoR2.ItemDisplayRule[]{
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = EnforcerHandPrefab,
                    childName = "Chest",
localPos = new Vector3(-0.08653F, 0.17649F, -0.10837F),
localAngles = new Vector3(0F, 0F, 0F),
localScale = new Vector3(0.40697F, 0.40697F, 0.40697F)
                }
            });/*
            rules.Add("mdlTreebot", new RoR2.ItemDisplayRule[]{
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = EnforcerHandPrefab,
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
                    followerPrefab = EnforcerHandPrefab,
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
                    followerPrefab = EnforcerHandPrefab,
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
                    followerPrefab = EnforcerHandPrefab,
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
                    followerPrefab = EnforcerHandPrefab,
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
                    followerPrefab = EnforcerHandPrefab,
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
                    followerPrefab = EnforcerHandPrefab,
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
                    followerPrefab = EnforcerHandPrefab,
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
                    followerPrefab = EnforcerHandPrefab,
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
                    followerPrefab = EnforcerHandPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0f, 0f, 0f),
                    localAngles = new Vector3(0f, 0f, 0f),
                    localScale = new Vector3(1f, 1f, 1f)
                }
            });*/
            var displayRules = new ItemDisplayRuleDict(null);
            ItemAPI.Add(new CustomEquipment(EnforcerHandEquipDef, rules));
            On.RoR2.EquipmentSlot.PerformEquipmentAction += PerformAction;
        }

        

        public static bool PerformAction(On.RoR2.EquipmentSlot.orig_PerformEquipmentAction orig, EquipmentSlot self, EquipmentDef equipmentDef)
        {
            if (equipmentDef == EnforcerHandEquipDef)
            {
                
                return OnUse(self);
            }
            else
            {
                return orig(self, equipmentDef);
            }

        }
        public static bool OnUse(EquipmentSlot slot)
        {
            EntitySoundManager.EmitSoundServer(ShielRaiseSound.akId, slot.characterBody.gameObject);
            slot.characterBody.AddTimedBuff(Buffs.ParryNextDamageBuff.ParryNextDamageBuffDef, ConfigFloat(EnforcerHandTimeWindow, EnforcerHandEnableConfig));
            return true;
        }
        private static void CreateSound()
        {
            ShielRaiseSound = ScriptableObject.CreateInstance<NetworkSoundEventDef>();
            ShielRaiseSound.eventName = "Play_wEnforcer_ShieldActivate";
            R2API.ContentAddition.AddNetworkSoundEventDef(ShielRaiseSound);
        }
        

        /*
        public static void Parry(On.RoR2.CharacterBody.orig_OnTakeDamageServer orig, CharacterBody self, DamageReport damageReport)
        {
            orig(self, damageReport);
            var attackerToStun = damageReport.attackerBody;
            
            SetStateOnHurt component = attackerToStun.GetComponent<SetStateOnHurt>();
            if (self.equipmentSlot == EnforcerHandEquipDef && self.HasBuff(RoR2Content.Buffs.HiddenInvincibility))
            {
                /*
                if (component.hasEffectiveAuthority)
                {
                    component.SetStunInternal(1f);
                    EffectManager.SimpleImpactEffect(LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/ImpactEffects/ImpactStunGrenade"), attackerToStun.corePosition, attackerToStun.corePosition, true);
                }
                else
                {
                    component.CallRpcSetStun(1f);
                    EffectManager.SimpleImpactEffect(LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/ImpactEffects/ImpactStunGrenade"), attackerToStun.corePosition, attackerToStun.corePosition, true);

                }
                

                SetStateOnHurt.SetStunOnObject(attackerToStun.gameObject, 1f);
                Util.PlaySound("Play_item_proc_negateAttack", self.gameObject);
                self.GetComponent<CharacterBody>().inventory.DeductActiveEquipmentCooldown(10);
            }
            else if (self.GetComponent<CharacterBody>().equipmentSlot == EnforcerHandEquipDef)
            {
                self.GetComponent<CharacterBody>().inventory.DeductActiveEquipmentCooldown(2);
            }
            Debug.Log(damageReport);
            Debug.Log(damageReport.attacker);
            Debug.Log(damageReport.attackerBody);
            Debug.Log(damageReport.attackerOwnerMaster);
        }
        */
        public static void AddLanguageTokens()
        {
            LanguageAPI.Add("ENFORCERHAND_NAME", "Enforcer Hand");
            LanguageAPI.Add("ENFORCERHAND_PICKUP", "Appries incoming attack on use");
            LanguageAPI.Add("ENFORCERHAND_DESC", "<style=cIsDamage>Parry</style> an <style=cIsDamage>incoming attack</style> back to the attacker in a " + ConfigFloat(EnforcerHandTimeWindow, EnforcerHandEnableConfig) + "seconds time window on use. Deals <style=cIsDamage>" + ConfigFloat(EnforcerHandReflectDamageMultiplier, EnforcerHandEnableConfig) + "x reflected damage</style>, <style=cIsDamage>" + ConfigFloat(EnforcerHandTotalDamageMultiplier, EnforcerHandEnableConfig) + "% TOTAL damage</style> and <style=cIsHealth>" + ConfigFloat(EnforcerHandMaxHealthDamageMulyiplier, EnforcerHandEnableConfig) + "%</style> attackers <style=cIsHealth>max health</style>. Applies " + ConfigInt(EnforcerHandWoundedCount, EnforcerHandEnableConfig) + " amount of Wound for " + ConfigFloat(EnforcerHandWoundedTime, EnforcerHandEnableConfig) + " seconds");
            LanguageAPI.Add("ENFORCERHAND_LORE", "<style=cMono>//--SURVIVOR №17 STATUS--//</style>" +
                "\n" +
                "CLASS: Enforcer" +
                "\n" +
                "SURVIVOR ID: 12A s8 17" +
                "\n" +
                "SURVIVOR NAME: JOHN DOE" +
                "\n" +
                "SURVIVOR LOCATION: <style=cDeath>ERROR.</style> LATEST DATA: o63,283 i45,818 SKY MEADOWS" +
                "\n" +
                "BEACON ID: 12A o3 4" +
                "\n" +
                "BEACON LOCATION: <style=cDeath>ERROR.</style> LATEST DATA: o59,356 i43,284 SKY MEADOWS" +
                "\n" +
                "HEALTH STATUS: ALIVE. MISSING LEFT ARM. HEAVY BLOOD LOSS. FOREIGN OBJECTS DETECTED. SIGNS OF CORRUPTION" +
                "\n" +
                "NOTE: \"Teleporter has been completed\". EDITED 12 HOURS AGO");
        }
    }
}
