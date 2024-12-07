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

namespace CaeliImperium.Equipment
{
    internal static class Necronomicon
    {
        internal static GameObject NecronomiconPrefab;
        internal static Sprite NecronomiconIcon;
        public static EquipmentDef NecronomiconEquipDef;
        public static GameObject NecronomiconMasterprefab;
        public static Inventory NecronomiconInventory;
        public static Vector3 NecronomiconPosition;
        public static ConfigEntry<bool> NecronomiconEnable;
        public static ConfigEntry<bool> NecronomiconEnableConfig;
        public static ConfigEntry<float> NecronomiconCooldown;
        public static ConfigEntry<int> NecronomiconSpawnAmount;
        public static ConfigEntry<int> NecronomiconHealthBoost;
        public static ConfigEntry<int> NecronomiconDamageBoost;
        public static ConfigEntry<int> NecronomiconHealthDrain;
        public static ConfigEntry<bool> NecronomiconDoInventoryCopy;
        public static ConfigEntry<int> NecronomiconSpawnAmountNoPlayer;
        public static ConfigEntry<int> NecronomiconHealthBoostNoPlayer;
        public static ConfigEntry<int> NecronomiconDamageBoostNoPlayer;
        public static ConfigEntry<int> NecronomiconHealthDrainNoPlayer;
        public static ConfigEntry<bool> NecronomiconDoInventoryCopyNoPlayer;

        internal static void Init()
        {
            AddConfigs();
            NecronomiconPrefab = MainAssets.LoadAsset<GameObject>("Assets/Models/Prefabs/ForbiddenTome.prefab");
            NecronomiconIcon = MainAssets.LoadAsset<Sprite>("Assets/Icons/NecronomiconIcon.png");
            if (!NecronomiconEnable.Value)
            {
                return;
            }

            Item();

            AddLanguageTokens();
        }

        private static void AddConfigs()
        {
            NecronomiconEnable = Config.Bind<bool>("Equipment : Necronomicon",
                                         "Activation",
                                         true,
                                         "Enable this equipment?");
            NecronomiconEnableConfig = Config.Bind<bool>("Equipment : Necronomicon",
                                         "Config Activation",
                                         false,
                                         "Enable this equipment?");
            NecronomiconCooldown = Config.Bind<float>("Equipment : Necronomicon",
                                         "Cooldown",
                                         30,
                                         "Control the cooldown time in seconds");
            NecronomiconSpawnAmount = Config.Bind<int>("Equipment : Necronomicon",
                                         "Spawn amount",
                                         5,
                                         "Control the spawn amount");
            NecronomiconHealthBoost = Config.Bind<int>("Equipment : Necronomicon",
                                         "Health boost",
                                         20,
                                         "Control the health boost in 10%");
            NecronomiconDamageBoost = Config.Bind<int>("Equipment : Necronomicon",
                                         "Damage boost",
                                         20,
                                         "Control the damage boost in 10%");
            NecronomiconHealthDrain = Config.Bind<int>("Equipment : Necronomicon",
                                         "Ghost duration",
                                         30,
                                         "Control how long ghosts last in seconds");
            NecronomiconDoInventoryCopy = Config.Bind<bool>("Equipment : Necronomicon",
                                         "Inventory copy",
                                         true,
                                         "Do spawned monsters retain their items?");
            NecronomiconSpawnAmountNoPlayer = Config.Bind<int>("Equipment : Necronomicon",
                                         "Spawn amount for non player users",
                                         5,
                                         "Control the spawn amount for non player users");
            NecronomiconHealthBoostNoPlayer = Config.Bind<int>("Equipment : Necronomicon",
                                         "Health boost for non player users",
                                         20,
                                         "Control the health boost in 10% for non player users");
            NecronomiconDamageBoostNoPlayer = Config.Bind<int>("Equipment : Necronomicon",
                                         "Damage boost for non player users",
                                         20,
                                         "Control the damage boost in 10% for non player users");
            NecronomiconHealthDrainNoPlayer = Config.Bind<int>("Equipment : Necronomicon",
                                         "Ghost duration for non player users",
                                         20,
                                         "Control how long ghosts last in seconds for non player users");
            NecronomiconDoInventoryCopyNoPlayer = Config.Bind<bool>("Equipment : Necronomicon",
                                         "Inventory copy for non players users",
                                         true,
                                         "Do spawned monsters retain their items for non player users?");
            ModSettingsManager.AddOption(new CheckBoxOption(NecronomiconEnable, new CheckBoxConfig() { restartRequired = true }));
            ModSettingsManager.AddOption(new CheckBoxOption(NecronomiconEnableConfig, new CheckBoxConfig() { restartRequired = true }));
            ModSettingsManager.AddOption(new FloatFieldOption(NecronomiconCooldown));
            ModSettingsManager.AddOption(new IntFieldOption(NecronomiconSpawnAmount));
            ModSettingsManager.AddOption(new IntFieldOption(NecronomiconHealthBoost));
            ModSettingsManager.AddOption(new IntFieldOption(NecronomiconDamageBoost));
            ModSettingsManager.AddOption(new IntFieldOption(NecronomiconHealthDrain));
            ModSettingsManager.AddOption(new CheckBoxOption(NecronomiconDoInventoryCopy));
            ModSettingsManager.AddOption(new IntFieldOption(NecronomiconSpawnAmountNoPlayer));
            ModSettingsManager.AddOption(new IntFieldOption(NecronomiconHealthBoostNoPlayer));
            ModSettingsManager.AddOption(new IntFieldOption(NecronomiconDamageBoostNoPlayer));
            ModSettingsManager.AddOption(new IntFieldOption(NecronomiconHealthDrainNoPlayer));
            ModSettingsManager.AddOption(new CheckBoxOption(NecronomiconDoInventoryCopyNoPlayer));
        }

        private static void Item()
        {
            NecronomiconEquipDef = ScriptableObject.CreateInstance<EquipmentDef>();
            NecronomiconEquipDef.name = "Necronomicon";
            NecronomiconEquipDef.nameToken = "NECRONOMICON_NAME";
            NecronomiconEquipDef.pickupToken = "NECRONOMICON_PICKUP";
            NecronomiconEquipDef.descriptionToken = "NECRONOMICON_DESC";
            NecronomiconEquipDef.loreToken = "NECRONOMICON_LORE";
            NecronomiconEquipDef.pickupIconSprite = NecronomiconIcon;
            NecronomiconEquipDef.pickupModelPrefab = NecronomiconPrefab;
            NecronomiconEquipDef.appearsInMultiPlayer = true;
            NecronomiconEquipDef.appearsInSinglePlayer = true;
            NecronomiconEquipDef.cooldown = ConfigFloat(NecronomiconCooldown, NecronomiconEnableConfig);
            NecronomiconEquipDef.requiredExpansion = CaeliImperiumExpansionDef;
            NecronomiconEquipDef.canDrop = true;
            ItemDisplayRuleDict rules = new ItemDisplayRuleDict();
            rules.Add("mdlCommandoDualies", new RoR2.ItemDisplayRule[]{
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = NecronomiconPrefab,
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
                    followerPrefab = NecronomiconPrefab,
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
                    followerPrefab = NecronomiconPrefab,
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
                    followerPrefab = NecronomiconPrefab,
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
                    followerPrefab = NecronomiconPrefab,
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
                    followerPrefab = NecronomiconPrefab,
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
                    followerPrefab = NecronomiconPrefab,
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
                    followerPrefab = NecronomiconPrefab,
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
            ItemAPI.Add(new CustomEquipment(NecronomiconEquipDef, rules));
            On.RoR2.EquipmentSlot.PerformEquipmentAction += PerformAction;
        }
        private static bool PerformAction(On.RoR2.EquipmentSlot.orig_PerformEquipmentAction orig, EquipmentSlot self, EquipmentDef equipmentDef)
        {
            if (equipmentDef == NecronomiconEquipDef)
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
            bool isNull = false;
            if (slot.characterBody.isPlayerControlled)
            {
                for (int i = 0; i < ConfigInt(NecronomiconSpawnAmount, NecronomiconEnableConfig) && !isNull; i++)
                {

                    try
                    {
                        NecronomiconMasterprefab = (GameObject)deadMasterPrefabArray.GetValue(deadMasterPrefabArray.Length - 2);
                        Array.Reverse(deadMasterPrefabArray);
                        Array.Copy(deadMasterPrefabArray, 1, deadMasterPrefabArray, 0, deadMasterPrefabArray.Length - 1);
                        Array.Reverse(deadMasterPrefabArray);
                        NecronomiconPosition = (Vector3)deadPositionArray.GetValue(deadPositionArray.Length - 2);
                        Array.Reverse(deadPositionArray);
                        Array.Copy(deadPositionArray, 1, deadPositionArray, 0, deadPositionArray.Length - 1);
                        Array.Reverse(deadPositionArray);
                        NecronomiconInventory = (Inventory)deadInventoryArray.GetValue(deadInventoryArray.Length - 2);
                        Array.Reverse(deadInventoryArray);
                        Array.Copy(deadInventoryArray, 1, deadInventoryArray, 0, deadInventoryArray.Length - 1);
                        Array.Reverse(deadInventoryArray);


                        if (NecronomiconMasterprefab != null && NecronomiconPosition != null)
                        {
                            var summon = new MasterSummon
                            {

                                masterPrefab = NecronomiconMasterprefab,
                                position = NecronomiconPosition,
                                rotation = Quaternion.identity,
                                teamIndexOverride = new TeamIndex?(slot.GetComponent<CharacterBody>().teamComponent.teamIndex),
                                useAmbientLevel = true,
                                summonerBodyObject = slot.gameObject,
                                //inventoryToCopy = NecronomiconInventory,
                                ignoreTeamMemberLimit = true,

                            };
                            CharacterMaster characterMaster = summon.Perform();
                            if (characterMaster)
                            {
                                if (NecronomiconInventory != null && NecronomiconDoInventoryCopy.Value)
                                {
                                    characterMaster.inventory.AddItemsFrom(NecronomiconInventory);

                                }
                                characterMaster.inventory.GiveItem(RoR2Content.Items.Ghost, 1);
                                characterMaster.inventory.GiveItem(RoR2Content.Items.HealthDecay, ConfigInt(NecronomiconHealthDrain, NecronomiconEnableConfig));
                                characterMaster.inventory.GiveItem(RoR2Content.Items.BoostHp, ConfigInt(NecronomiconHealthBoost, NecronomiconEnableConfig));
                                characterMaster.inventory.GiveItem(RoR2Content.Items.BoostDamage, ConfigInt(NecronomiconDamageBoost, NecronomiconEnableConfig));
                            }


                        }
                    }
                    catch (Exception e)
                    {
                        Debug.LogError("Necronomicon: failed to revive");
                        Debug.LogError(e);
                    }
                    

                }
            }
            else
            {
                for (int i = 0; i < ConfigInt(NecronomiconSpawnAmountNoPlayer, NecronomiconEnableConfig); i++)
                {

                    try
                    {
                        NecronomiconMasterprefab = (GameObject)deadMasterPrefabArray.GetValue(deadMasterPrefabArray.Length - 2);
                        Array.Reverse(deadMasterPrefabArray);
                        Array.Copy(deadMasterPrefabArray, 1, deadMasterPrefabArray, 0, deadMasterPrefabArray.Length - 1);
                        Array.Reverse(deadMasterPrefabArray);
                        NecronomiconPosition = (Vector3)deadPositionArray.GetValue(deadPositionArray.Length - 2);
                        Array.Reverse(deadPositionArray);
                        Array.Copy(deadPositionArray, 1, deadPositionArray, 0, deadPositionArray.Length - 1);
                        Array.Reverse(deadPositionArray);
                        NecronomiconInventory = (Inventory)deadInventoryArray.GetValue(deadInventoryArray.Length - 2);
                        Array.Reverse(deadInventoryArray);
                        Array.Copy(deadInventoryArray, 1, deadInventoryArray, 0, deadInventoryArray.Length - 1);
                        Array.Reverse(deadInventoryArray);

                        if (NecronomiconMasterprefab != null && NecronomiconPosition != null)
                        {
                            var summon = new MasterSummon
                            {

                                masterPrefab = NecronomiconMasterprefab,
                                position = NecronomiconPosition,
                                rotation = Quaternion.identity,
                                teamIndexOverride = new TeamIndex?(slot.GetComponent<CharacterBody>().teamComponent.teamIndex),
                                useAmbientLevel = true,
                                summonerBodyObject = slot.gameObject,
                                inventoryToCopy = NecronomiconInventory,
                                ignoreTeamMemberLimit = false,

                            };
                            CharacterMaster characterMaster = summon.Perform();

                            if (characterMaster)
                            {
                                if (NecronomiconInventory != null && NecronomiconDoInventoryCopyNoPlayer.Value)
                                {
                                    characterMaster.inventory.AddItemsFrom(NecronomiconInventory);

                                }
                                characterMaster.inventory.GiveItem(RoR2Content.Items.Ghost, 1);
                                characterMaster.inventory.GiveItem(RoR2Content.Items.HealthDecay, ConfigInt(NecronomiconHealthDrainNoPlayer, NecronomiconEnableConfig));
                                characterMaster.inventory.GiveItem(RoR2Content.Items.BoostHp, ConfigInt(NecronomiconHealthBoostNoPlayer, NecronomiconEnableConfig));
                                characterMaster.inventory.GiveItem(RoR2Content.Items.BoostDamage, ConfigInt(NecronomiconDamageBoostNoPlayer, NecronomiconEnableConfig));
                            }




                        }
                    }
                    catch (Exception e)
                    {
                        Debug.LogError("Necronomicon: failed to revive");
                        Debug.LogError(e);
                    }


                }
            }
            return true;
        }




        private static void AddLanguageTokens()
        {
            LanguageAPI.Add("NECRONOMICON_NAME", "Necronomicon");
            LanguageAPI.Add("NECRONOMICON_PICKUP", "On use revive " + ConfigInt(NecronomiconSpawnAmount, NecronomiconEnableConfig) + " last dead monsters to <style=cIsDamage>fight on your side</style>. Spawned monsters have <style=cIsHealth>" + ConfigInt(NecronomiconHealthBoost, NecronomiconEnableConfig) * 10 + "% more health</style> and <style=cIsDamage>" + ConfigInt(NecronomiconDamageBoost, NecronomiconEnableConfig) * 10 +  "% more damage</style>. They live for " + ConfigInt(NecronomiconHealthDrain, NecronomiconEnableConfig) + " seconds and cannot be revived again");
            LanguageAPI.Add("NECRONOMICON_DESC", "On use revive " + ConfigInt(NecronomiconSpawnAmount, NecronomiconEnableConfig) + " last dead monsters to <style=cIsDamage>fight on your side</style>. Spawned monsters have <style=cIsHealth>" + ConfigInt(NecronomiconHealthBoost, NecronomiconEnableConfig) * 10 + "% more health</style> and <style=cIsDamage>" + ConfigInt(NecronomiconDamageBoost, NecronomiconEnableConfig) * 10 + "% more damage</style>. They live for " + ConfigInt(NecronomiconHealthDrain, NecronomiconEnableConfig) + " seconds and cannot be revived again");
            LanguageAPI.Add("NECRONOMICON_LORE", "");
        }
    }
}
