using R2API;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine.AddressableAssets;
using UnityEngine;
using static CaeliImperiumPlugin.CaeliImperium;
using RoR2.Stats;
using CaeliImperiumPlugin;
using BepInEx.Configuration;
using static R2API.RecalculateStatsAPI;
using RoR2.Items;
using MonoMod.Cil;
using UnityEngine.Diagnostics;
using RiskOfOptions.OptionConfigs;
using RiskOfOptions.Options;
using RiskOfOptions;
using CaeliImperium.Buffs;

namespace CaeliImperium.Items
{

    internal static class GuardianCrown
    {
        internal static GameObject GuardianCrownPrefab;
        internal static Sprite GuardianCrownIcon;
        public static ItemDef GuardianCrownItemDef;
        public static ConfigEntry<bool> GuardianCrownEnable;
        public static ConfigEntry<bool> GuardianCrownAIBlacklist;
        public static ConfigEntry<float> GuardianCrownTier;
        public static ConfigEntry<bool> GuardianCrownLuckAffected;
        public static ConfigEntry<float> GuardianCrownChance;
        public static ConfigEntry<float> GuardianCrownEnemyChance;
        public static ConfigEntry<float> GuardianCrownDistance;
        public static ConfigEntry<float> GuardianCrownDistanceMaxChance;
        public static ConfigEntry<float> GuardianCrownMaxChance;
        public static ConfigEntry<float> GuardianCrownBuffTime;
        public static ConfigEntry<float> GuardianCrownBuffTimeStack;
        public static string name = "Guardian Crown";
        internal static void Init()
        {
            AddConfigs();
            string tier = "Assets/Icons/GuardianAngelCrown.png";
            switch (GuardianCrownTier.Value)
            {
                case 1:
                    tier = "Assets/Icons/GuardianAngelCrownTier1.png";
                    break;
                case 2:
                    tier = "Assets/Icons/GuardianAngelCrown.png";
                    break;
                case 3:
                    tier = "Assets/Icons/GuardianAngelCrownTier3.png";
                    break;

            }
            GuardianCrownPrefab = MainAssets.LoadAsset<GameObject>("Assets/Models/Prefabs/GuardianAngel.prefab");
            GuardianCrownIcon = MainAssets.LoadAsset<Sprite>(tier);
            if (!GuardianCrownEnable.Value)
            {
                return;
            }
            Item();

            AddLanguageTokens();
        }

        public static void AddConfigs()
        {
            GuardianCrownEnable = Config.Bind<bool>("Item : " + name,
                             "Activation",
                             true,
                             "Enable " + name + " item?");
            GuardianCrownAIBlacklist = Config.Bind<bool>("Item : " + name,
                             "AI Blacklist",
                             true,
                             "Blacklist this item from enemies?");
            GuardianCrownTier = Config.Bind<float>("Item : " + name,
                                         "Item tier",
                                         2f,
                                         "1: Common/White\n2: Rare/Green\n3: Legendary/Red");
            GuardianCrownLuckAffected = Config.Bind<bool>("Item : " + name,
                                         "Luck",
                                         false,
                                         "Is luck affected?");
            GuardianCrownChance = Config.Bind<float>("Item : " + name,
                                         "Chance for players",
                                         20,
                                         "Change the chance of enemies being stunned on their attack");
            GuardianCrownEnemyChance = Config.Bind<float>("Item : " + name,
                                         "Chance for enemies",
                                         5,
                                         "Change the chance of players being stunned on their attack");
            GuardianCrownDistance = Config.Bind<float>("Item : " + name,
                                         "Distance",
                                         30,
                                         "Change the distance for chance increase");
            GuardianCrownDistanceMaxChance = Config.Bind<float>("Item : " + name,
                                         "Distance chance",
                                         50,
                                         "Change the distance maximum chance increase");
            GuardianCrownMaxChance = Config.Bind<float>("Item : " + name,
                                         "Max chance",
                                         70,
                                         "Change the maximum stun chance in percentage");
            GuardianCrownBuffTime = Config.Bind<float>("Item : " + name,
                                         "Dazzled time",
                                         5,
                                         "Change the time of the Dazzled debuff in seconds");
            GuardianCrownBuffTimeStack = Config.Bind<float>("Item : " + name,
                                         "Dazzled time stack",
                                         0,
                                         "Change the time increase of the Dazzled debuff per item stack in seconds");

            ModSettingsManager.AddOption(new CheckBoxOption(GuardianCrownEnable, new CheckBoxConfig() { restartRequired = true }));
            ModSettingsManager.AddOption(new CheckBoxOption(GuardianCrownAIBlacklist, new CheckBoxConfig() { restartRequired = true }));
            ModSettingsManager.AddOption(new StepSliderOption(GuardianCrownTier, new StepSliderConfig() { min = 1, max = 3, increment = 1f, restartRequired = true }));
            ModSettingsManager.AddOption(new CheckBoxOption(GuardianCrownLuckAffected));
            ModSettingsManager.AddOption(new FloatFieldOption(GuardianCrownChance));
            ModSettingsManager.AddOption(new FloatFieldOption(GuardianCrownEnemyChance));
            ModSettingsManager.AddOption(new FloatFieldOption(GuardianCrownDistance));
            ModSettingsManager.AddOption(new FloatFieldOption(GuardianCrownDistanceMaxChance));
            ModSettingsManager.AddOption(new FloatFieldOption(GuardianCrownMaxChance));
            ModSettingsManager.AddOption(new FloatFieldOption(GuardianCrownBuffTime));
            ModSettingsManager.AddOption(new FloatFieldOption(GuardianCrownBuffTimeStack));
        }

        private static void Item()
        {

            GuardianCrownItemDef = ScriptableObject.CreateInstance<ItemDef>();
            GuardianCrownItemDef.name = name.Replace(" ", "");
            GuardianCrownItemDef.nameToken = name.Replace(" ", "").ToUpper() + "_NAME";
            GuardianCrownItemDef.pickupToken = name.Replace(" ", "").ToUpper() + "_PICKUP";
            GuardianCrownItemDef.descriptionToken = name.Replace(" ", "").ToUpper() + "_DESC";
            GuardianCrownItemDef.loreToken = name.Replace(" ", "").ToUpper() + "_LORE";
            switch (GuardianCrownTier.Value)
            {
                case 1:
                    GuardianCrownItemDef.deprecatedTier = ItemTier.Tier1;
                    break;
                case 2:
                    GuardianCrownItemDef.deprecatedTier = ItemTier.Tier2;
                    break;
                case 3:
                    GuardianCrownItemDef.deprecatedTier = ItemTier.Tier3;
                    break;

            }
            GuardianCrownItemDef.pickupIconSprite = GuardianCrownIcon;
            GuardianCrownItemDef.pickupModelPrefab = GuardianCrownPrefab;
            GuardianCrownItemDef.canRemove = true;
            GuardianCrownItemDef.hidden = false;
            GuardianCrownItemDef.requiredExpansion = CaeliImperiumExpansionDef;
            var tags = new List<ItemTag>() { ItemTag.Utility, ItemTag.Damage };
            if (GuardianCrownAIBlacklist.Value)
            {
                tags.Add(ItemTag.AIBlacklist);
            }
            GuardianCrownItemDef.tags = tags.ToArray();
            ItemDisplayRuleDict rules = new ItemDisplayRuleDict();
            rules.Add("mdlCommandoDualies", new RoR2.ItemDisplayRule[]{
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = GuardianCrownPrefab,
                    childName = "Head",
localPos = new Vector3(-0.00191F, 0.44798F, -0.12129F),
localAngles = new Vector3(11.43523F, 180F, 1.76892F),
localScale = new Vector3(0.32039F, 0.32039F, 0.32039F)
                }
            });
            rules.Add("mdlHuntress", new RoR2.ItemDisplayRule[]{
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = GuardianCrownPrefab,
                    childName = "Head",
localPos = new Vector3(0F, 0.37404F, -0.05873F),
localAngles = new Vector3(0F, 180F, 0F),
localScale = new Vector3(0.25391F, 0.25391F, 0.25391F)
                }
            });
            rules.Add("mdlBandit2", new RoR2.ItemDisplayRule[]{
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = GuardianCrownPrefab,
                    childName = "MainWeapon",
localPos = new Vector3(-0.13764F, 0.7148F, -0.00004F),
localAngles = new Vector3(90F, 270F, 0F),
localScale = new Vector3(0.05917F, 0.05917F, 0.05917F)
                }
            });
            rules.Add("mdlToolbot", new RoR2.ItemDisplayRule[]{
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = GuardianCrownPrefab,
                    childName = "HeadCenter",
localPos = new Vector3(0F, 1.35609F, 1.98573F),
localAngles = new Vector3(55.67212F, 0F, 0F),
localScale = new Vector3(1F, 1F, 1F)
                }
            });
            rules.Add("mdlEngi", new RoR2.ItemDisplayRule[]{
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = GuardianCrownPrefab,
                    childName = "Chest",
localPos = new Vector3(0F, 0.39093F, 0.17181F),
localAngles = new Vector3(29.92985F, 180F, 180F),
localScale = new Vector3(0.16227F, 0.16227F, 0.16227F)
                }
            });
            //rules.Add("mdlEngiTurrety", new RoR2.ItemDisplayRule[]{
            //    new RoR2.ItemDisplayRule
            //    {
            //        ruleType = ItemDisplayRuleType.ParentedPrefab,
            //        followerPrefab = GuardianCrownPrefab,
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
                    followerPrefab = GuardianCrownPrefab,
                    childName = "Head",
localPos = new Vector3(0F, 0.2463F, 0.00001F),
localAngles = new Vector3(0F, 180F, 0F),
localScale = new Vector3(0.25623F, 0.25623F, 0.25623F)
                }
            });
            rules.Add("mdlMerc", new RoR2.ItemDisplayRule[]{
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = GuardianCrownPrefab,
                    childName = "Head",
localPos = new Vector3(0F, 0.33108F, -0.00001F),
localAngles = new Vector3(0F, 180F, 0F),
localScale = new Vector3(0.23871F, 0.23871F, 0.23871F)
                }
            });/*
            rules.Add("mdlTreebot", new RoR2.ItemDisplayRule[]{
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = GuardianCrownPrefab,
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
                    followerPrefab = GuardianCrownPrefab,
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
                    followerPrefab = GuardianCrownPrefab,
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
                    followerPrefab = GuardianCrownPrefab,
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
                    followerPrefab = GuardianCrownPrefab,
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
                    followerPrefab = GuardianCrownPrefab,
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
                    followerPrefab = GuardianCrownPrefab,
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
                    followerPrefab = GuardianCrownPrefab,
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
                    followerPrefab = GuardianCrownPrefab,
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
                    followerPrefab = GuardianCrownPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0f, 0f, 0f),
                    localAngles = new Vector3(0f, 0f, 0f),
                    localScale = new Vector3(1f, 1f, 1f)
                }
            });*/
            var displayRules = new ItemDisplayRuleDict(null);
            ItemAPI.Add(new CustomItem(GuardianCrownItemDef, rules));
            On.RoR2.CharacterBody.OnSkillActivated += OnEnemySkillUse;
        }

        private static void OnEnemySkillUse(On.RoR2.CharacterBody.orig_OnSkillActivated orig, CharacterBody self, GenericSkill skill)
        {

            //var query = self.master.inventory.GetItemCount(ForbiddenTomeItemDef);
            //Chat.AddMessage(playerCharacterMasterController.ToString());
            //int itemCount = self.inventory.GetItemCount(ForbiddenTomeItemDef);
            /*int itemCountPlayer = Util.GetItemCountForTeam(TeamIndex.Player, GuardianCrownItemDef.itemIndex, true);
            int itemCountMonsters = Util.GetItemCountForTeam(TeamIndex.Monster, GuardianCrownItemDef.itemIndex, true) + 
                Util.GetItemCountForTeam(TeamIndex.Lunar, GuardianCrownItemDef.itemIndex, true) + 
                Util.GetItemCountForTeam(TeamIndex.Void, GuardianCrownItemDef.itemIndex, true) +
                Util.GetItemCountForTeam(TeamIndex.Neutral, GuardianCrownItemDef.itemIndex, true);
            if (itemCountPlayer > 0)
            {
                if (self.teamComponent.teamIndex != TeamIndex.Player)
                {*/
            float stunChance = 0;
            float allLuck = 0;
            foreach (var characterBody in CharacterBody.readOnlyInstancesList)
            {
                if (characterBody && characterBody.teamComponent.teamIndex != self.teamComponent.teamIndex && characterBody.inventory)
                {
                    int itemCount = characterBody.inventory ? characterBody.inventory.GetItemCount(GuardianCrownItemDef) : 0;
                    if (itemCount > 0)
                    {
                        float dist = Vector3.Distance(characterBody.corePosition, self.corePosition);
                        if (characterBody.teamComponent.teamIndex == TeamIndex.Player)
                        {
                            stunChance += GuardianCrownChance.Value;
                        }
                        else
                        {
                            stunChance += GuardianCrownEnemyChance.Value;
                        }
                        if (dist < GuardianCrownDistance.Value)
                        {
                            stunChance += GuardianCrownDistanceMaxChance.Value - (dist * (GuardianCrownDistanceMaxChance.Value/dist));
                        }
                        if (GuardianCrownLuckAffected.Value)
                        {
                        allLuck += Math.Max(characterBody.master.luck, -1);
                        }
                        stunChance *= itemCount;

                    }
                }
                // }

                //}
                

            }
            if (stunChance > 0)
            {
bool roll = false;
                int buffCount = self.GetBuffCount(Buffs.DazzledBuff.DazzledBuffDef) + 1;
                roll = Util.CheckRoll(ConvertAmplificationPercentageIntoReductionPercentage(stunChance / buffCount, GuardianCrownMaxChance.Value), allLuck);
            if (roll)
            {
                if (true)
                {
                    SetStateOnHurt component = self.GetComponent<SetStateOnHurt>();
                    if (component.hasEffectiveAuthority)
                    {
                            //self.AddTimedBuff(DLC2Content.Buffs.DisableAllSkills, 0.2f);
                            if (!self.isChampion)
                            {
                                component.SetStunInternal(0.2f);
                            }
                        //SetStateOnHurt.SetStunOnObject(self.gameObject, 0.2f);
                        EffectManager.SimpleImpactEffect(LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/ImpactEffects/ImpactStunGrenade"), self.corePosition, self.corePosition, true);
                    }
                    else
                    {
                            if (!self.isChampion)
                            {
                                component.SetStunInternal(0.2f);
                            }
                            //SetStateOnHurt.SetStunOnObject(self.gameObject, 0.2f);
                            EffectManager.SimpleImpactEffect(LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/ImpactEffects/ImpactStunGrenade"), self.corePosition, self.corePosition, true);

                    }
                    float timerCount = (Util.GetItemCountGlobal(GuardianCrownItemDef.itemIndex, true) - Util.GetItemCountForTeam(self.teamComponent.teamIndex, GuardianCrownItemDef.itemIndex, true) - 1) * GuardianCrownBuffTimeStack.Value;
                        self.AddTimedBuff(Buffs.DazzledBuff.DazzledBuffDef, GuardianCrownBuffTime.Value + timerCount);
                    return;
                }
            }
            }
            orig(self, skill);

            /*
            if (itemCountMonsters > 0)
            {
                if (self.teamComponent.teamIndex == TeamIndex.Player)
                {
                    bool roll = false;
                    if (ForbiddenTomeLuckAffected.Value)
                    {
                        roll = Util.CheckRoll(Util.ConvertAmplificationPercentageIntoReductionPercentage(itemCountMonsters * ForbiddenTomeEnemyChance.Value), self.master);
                    }
                    else
                    {
                        roll = Util.CheckRoll(Util.ConvertAmplificationPercentageIntoReductionPercentage(itemCountMonsters * ForbiddenTomeEnemyChance.Value));
                    }
                        ;
                    //Chat.AddMessage(roll.ToString());
                    //Chat.AddMessage((Util.ConvertAmplificationPercentageIntoReductionPercentage(itemCount * ForbiddenTomeChance.Value), playerCharacterMasterController.master).ToString());
                    if (roll)
                    {
                        if (!self.isChampion)
                        {
                            if (component.hasEffectiveAuthority)
                            {
                                //self.AddTimedBuff(DLC2Content.Buffs.DisableAllSkills, 0.2f);
                                component.SetStunInternal(0.2f);
                                //SetStateOnHurt.SetStunOnObject(self.gameObject, 0.2f);
                                EffectManager.SimpleImpactEffect(LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/ImpactEffects/ImpactStunGrenade"), self.corePosition, self.corePosition, true);
                            }
                            else
                            {
                                //self.AddTimedBuff(DLC2Content.Buffs.DisableAllSkills, 0.2f);
                                component.CallRpcSetStun(0.2f);
                                //SetStateOnHurt.SetStunOnObject(self.gameObject, 0.2f);
                                EffectManager.SimpleImpactEffect(LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/ImpactEffects/ImpactStunGrenade"), self.corePosition, self.corePosition, true);

                            }
                        }


                    }
                }
            }*/
        }



        /*
        using (IEnumerator<CharacterMaster> enumerator = CharacterMaster.readOnlyInstancesList.GetEnumerator())

        {
            int itemCount = enumerator.Current.inventory.GetItemCount(ForbiddenTome.ForbiddenTomeItemDef);
            if (itemCount > 0)
            {
                Chat.AddMessage(itemCount.ToString());
               if (Util.CheckRoll(Util.ConvertAmplificationPercentageIntoReductionPercentage(itemCount * 20), enumerator.Current))
                {
                    Chat.AddMessage("attack");
                    EffectManager.SimpleImpactEffect(LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/ImpactEffects/ImpactStunGrenade"), self.corePosition, self.corePosition, true);
                    SetStateOnHurt.SetStunOnObject(self.gameObject, 0.2f);
                } 
            }
        }*/




        private static void AddLanguageTokens()
        {
            LanguageAPI.Add(name.Replace(" ", "").ToUpper() + "_NAME", name);
            LanguageAPI.Add(name.Replace(" ", "").ToUpper() + "_PICKUP", "On enemy attack, stun them with " + GuardianCrownChance.Value + "% <style=cStack>(+ " + GuardianCrownChance.Value + "% per item stack hyperbollicaly)</style> chance and apply a debuff, that increases <style=cIsDamage>incoming damage</style> by <style=cIsDamage>" + DazzledBuff.DazzledDamageIncrease.Value +"%</style>.");
            LanguageAPI.Add(name.Replace(" ", "").ToUpper() + "_DESC", "On enemy attack, stun them with " + GuardianCrownChance.Value + "% <style=cStack>(+ " + GuardianCrownChance.Value + "% per item stack hyperbollicaly)</style> chance and apply a debuff, that increases <style=cIsDamage>incoming damage</style> by <style=cIsDamage>" + DazzledBuff.DazzledDamageIncrease.Value + "%</style>.");
            LanguageAPI.Add(name.Replace(" ", "").ToUpper() + "_LORE", "To protect, to support, to relief, this crown will help. Let it be your honor to protect the weak. Let it be your duty to protect this world");
        }

    }
}