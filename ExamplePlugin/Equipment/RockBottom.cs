using R2API;
using Rewired.Utils;
using RiskOfOptions.Options;
using RiskOfOptions;
using RoR2;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Assertions.Must;
using static CaeliImperiumPlugin.CaeliImperium;
using BepInEx.Configuration;
using RiskOfOptions.OptionConfigs;
using System.Linq;

namespace CaeliImperium.Equipment
{
    internal static class RockBottom
    {
        internal static GameObject RockBottomPrefab;
        internal static Sprite RockBottomiconIcon;
        public static EquipmentDef RockBottomEquipDef;
        public static ConfigEntry<bool> RockBottomEnable;
        public static ConfigEntry<bool> RockBotomEnableConfig;
        public static ConfigEntry<float> RockBottomCooldown;
        public static GameObject targetCrosshair = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Lightning/LightningIndicator.prefab").WaitForCompletion();
        private static string name = "Rock Bottom";

        internal static void Init()
        {
            AddConfigs();
            RockBottomPrefab = MainAssets.LoadAsset<GameObject>("Assets/Models/Prefabs/ForbiddenTome.prefab");
            RockBottomiconIcon = MainAssets.LoadAsset<Sprite>("Assets/Icons/NecronomiconIcon.png");
            if (!RockBottomEnable.Value)
            {
                return;
            }

            Item();

            AddLanguageTokens();
            Buffs.RockBottomBuff.Init();
        }

        private static void AddConfigs()
        {
            RockBottomEnable = Config.Bind<bool>("Equipment : " + name,
                                         "Activation",
                                         true,
                                         "Enable this equipment?");
            RockBotomEnableConfig = Config.Bind<bool>("Equipment : " + name,
                                         "Config Activation",
                                         false,
                                         "Enable this equipment?");
            RockBottomCooldown = Config.Bind<float>("Equipment : " + name,
                                         "Cooldown",
                                         30,
                                         "Control the cooldown time in seconds");
            ModSettingsManager.AddOption(new CheckBoxOption(RockBottomEnable, new CheckBoxConfig() { restartRequired = true }));
            ModSettingsManager.AddOption(new CheckBoxOption(RockBotomEnableConfig, new CheckBoxConfig() { restartRequired = true }));
            ModSettingsManager.AddOption(new FloatFieldOption(RockBottomCooldown));
        }

        private static void Item()
        {
            RockBottomEquipDef = ScriptableObject.CreateInstance<EquipmentDef>();
            RockBottomEquipDef.name = name.Replace(" ", "");
            RockBottomEquipDef.nameToken = name.Replace(" ", "").ToUpper() + "_NAME";
            RockBottomEquipDef.pickupToken = name.Replace(" ", "").ToUpper() + "_PICKUP";
            RockBottomEquipDef.descriptionToken = name.Replace(" ", "").ToUpper() + "_DESC";
            RockBottomEquipDef.loreToken = name.Replace(" ", "").ToUpper() + "_LORE";
            RockBottomEquipDef.pickupIconSprite = RockBottomiconIcon;
            RockBottomEquipDef.pickupModelPrefab = RockBottomPrefab;
            RockBottomEquipDef.appearsInMultiPlayer = true;
            RockBottomEquipDef.appearsInSinglePlayer = true;
            RockBottomEquipDef.cooldown = ConfigFloat(RockBottomCooldown, RockBotomEnableConfig);
            RockBottomEquipDef.requiredExpansion = CaeliImperiumExpansionDef;
            RockBottomEquipDef.canDrop = true;
            ItemDisplayRuleDict rules = new ItemDisplayRuleDict();
            rules.Add("mdlCommandoDualies", new RoR2.ItemDisplayRule[]{
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = RockBottomPrefab,
                    childName = "ThighL",
localPos = new Vector3(0.12686F, 0.09934F, 0.03536F),
localAngles = new Vector3(359.926F, 318.8314F, 175.6739F),
localScale = new Vector3(0.57554F, 0.57554F, 0.57554F)
                }
            });
            rules.Add("mdlHuntress", new RoR2.ItemDisplayRule[]{
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = RockBottomPrefab,
                    childName = "ThighL",
localPos = new Vector3(0.14283F, 0.22244F, 0.12951F),
localAngles = new Vector3(344.0844F, 332.0223F, 166.9739F),
localScale = new Vector3(0.73059F, 0.73059F, 0.73059F)
                }
            });
            rules.Add("mdlBandit2", new RoR2.ItemDisplayRule[]{
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = RockBottomPrefab,
                    childName = "Pelvis",
localPos = new Vector3(0.21239F, -0.04734F, -0.02518F),
localAngles = new Vector3(12.90182F, 346.7058F, 169.4931F),
localScale = new Vector3(0.51855F, 0.51855F, 0.51855F)
                }
            });
            rules.Add("mdlToolbot", new RoR2.ItemDisplayRule[]{
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = RockBottomPrefab,
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
                    followerPrefab = RockBottomPrefab,
                    childName = "Pelvis",
localPos = new Vector3(0.29162F, 0.11391F, 0.01788F),
localAngles = new Vector3(358.4242F, 340.0651F, 168.5703F),
localScale = new Vector3(0.40316F, 0.40316F, 0.40316F)
                }
            });
            rules.Add("mdlEngiTurrety", new RoR2.ItemDisplayRule[]{
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = RockBottomPrefab,
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
                    followerPrefab = RockBottomPrefab,
                    childName = "LowerArmR",
localPos = new Vector3(-0.03398F, 0.16557F, -0.09688F),
localAngles = new Vector3(359.6599F, 310.7145F, 353.4925F),
localScale = new Vector3(0.46805F, 0.46805F, 0.46805F)
                }
            });
            rules.Add("mdlMerc", new RoR2.ItemDisplayRule[]{
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = RockBottomPrefab,
                    childName = "ThighL",
localPos = new Vector3(0.11285F, 0.13791F, -0.01707F),
localAngles = new Vector3(353.9679F, 4.27201F, 167.4407F),
localScale = new Vector3(0.29592F, 0.29592F, 0.29592F)
                }
            });/*
            rules.Add("mdlTreebot", new RoR2.ItemDisplayRule[]{
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = NecronomiconPrefab,
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
                    followerPrefab = NecronomiconPrefab,
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
                    followerPrefab = NecronomiconPrefab,
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
                    followerPrefab = NecronomiconPrefab,
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
                    followerPrefab = NecronomiconPrefab,
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
                    followerPrefab = NecronomiconPrefab,
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
                    followerPrefab = NecronomiconPrefab,
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
                    followerPrefab = NecronomiconPrefab,
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
                    followerPrefab = NecronomiconPrefab,
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
                    followerPrefab = NecronomiconPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0f, 0f, 0f),
                    localAngles = new Vector3(0f, 0f, 0f),
                    localScale = new Vector3(1f, 1f, 1f)
                }
            });*/
            var displayRules = new ItemDisplayRuleDict(null);
            ItemAPI.Add(new CustomEquipment(RockBottomEquipDef, displayRules));
            On.RoR2.EquipmentSlot.PerformEquipmentAction += PerformAction;
            On.RoR2.EquipmentSlot.UpdateTargets += PickTarget;
        }

        private static void PickTarget(On.RoR2.EquipmentSlot.orig_UpdateTargets orig, EquipmentSlot self, EquipmentIndex targetingEquipmentIndex, bool userShouldAnticipateTarget)
        {
            orig(self, targetingEquipmentIndex, userShouldAnticipateTarget);
            bool flag = targetingEquipmentIndex == RockBottomEquipDef.equipmentIndex;
            if (flag)
            {
                self.ConfigureTargetFinderForEnemies();
                HurtBox source = null;
                source = self.targetFinder.GetResults().FirstOrDefault<HurtBox>();
                self.currentTarget = new EquipmentSlot.UserTargetInfo(source);
            }
            else
            {
                self.currentTarget = default(EquipmentSlot.UserTargetInfo);
            }
            bool flag6 = self.currentTarget.transformToIndicateAt;
            if (flag6)
            {
                if (flag)
                {
                    self.targetIndicator.visualizerPrefab = targetCrosshair;
                }
            }
            self.targetIndicator.active = flag6;
            self.targetIndicator.targetTransform = (flag6 ? self.currentTarget.transformToIndicateAt : null);
            
        }

        private static bool PerformAction(On.RoR2.EquipmentSlot.orig_PerformEquipmentAction orig, EquipmentSlot self, EquipmentDef equipmentDef)
        {
            if (equipmentDef == RockBottomEquipDef)
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
           slot.UpdateTargets(RockBottomEquipDef.equipmentIndex, true);
            HurtBox hurtBox = slot.currentTarget.hurtBox;
            if (hurtBox != null)
            {
                slot.characterBody.AddTimedBuff(Buffs.RockBottomBuff.RockBottomBuffDef, 10);
                Util.HurtBoxColliderToBody(hurtBox.collider).AddTimedBuff(Buffs.RockBottomBuff.RockBottomBuffDef, 10);
                return true;
            }
            return false;
        }




        private static void AddLanguageTokens()
        {
            LanguageAPI.Add(name.Replace(" ", "").ToUpper() + "_NAME", name);
            LanguageAPI.Add(name.Replace(" ", "").ToUpper() + "_PICKUP", "Battle");
            LanguageAPI.Add(name.Replace(" ", "").ToUpper() + "_DESC", "");
            LanguageAPI.Add(name.Replace(" ", "").ToUpper() + "_LORE", "");
        }
    }
}
