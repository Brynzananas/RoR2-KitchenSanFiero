using BepInEx.Configuration;
using R2API;
using RiskOfOptions.OptionConfigs;
using RiskOfOptions.Options;
using RiskOfOptions;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using static KitchenSanFieroPlugin.KitchenSanFiero;
using RoR2.Orbs;
using KitchenSanFiero.Buffs;
using static R2API.RecalculateStatsAPI;
using Rewired;
using UnityEngine.UIElements;
using static R2API.DamageAPI;

namespace KitchenSanFiero.Items
{
    internal static class RejectedDagger
    {
        internal static GameObject RejectedDaggerPrefab;
        internal static Sprite RejectedDaggerIcon;
        public static ItemDef RejectedDaggerItemDef;
        public static ConfigEntry<bool> RejectedDaggerAIBlacklist;
        public static ConfigEntry<float> RejectedDaggerTier;
        public static ConfigEntry<float> RejectedDaggerDamageToAll;
        public static ConfigEntry<int> RejectedDaggerMinEnemyCount;
        public static ConfigEntry<float> RejectedDaggerDamageToMin;
        public static ConfigEntry<float> RejectedDaggerChampionReduction;
        public static string name = "Rejected Dagger";

        internal static void Init()
        {
            AddConfigs();
            string tier = "Assets/Icons/RejectedDagger.png";
            switch (RejectedDaggerTier.Value)
            {
                case 1:
                    tier = "Assets/Icons/RejectedDaggerTier1.png";
                    break;
                case 2:
                    tier = "Assets/Icons/RejectedDaggerTier2.png";
                    break;
                case 3:
                    tier = "Assets/Icons/RejectedDagger.png";
                    break;

            }
            RejectedDaggerPrefab = MainAssets.LoadAsset<GameObject>("Assets/Models/Prefabs/CritBuffOnEliteKill.prefab");
            RejectedDaggerIcon = MainAssets.LoadAsset<Sprite>(tier);

            Item();

            AddLanguageTokens();
        }

        private static void AddConfigs()
        {
            RejectedDaggerAIBlacklist = Config.Bind<bool>("Item : " + name,
                             "AI Blacklist",
                             false,
                             "Blacklist this item from enemies?");
            RejectedDaggerTier = Config.Bind<float>("Item : " + name,
                                         "Item tier",
                                         3f,
                                         "1: Common/White\n2: Rare/Green\n3: Legendary/Red");
            RejectedDaggerDamageToAll = Config.Bind<float>("Item : " + name,
                                         "Damage sharing to all",
                                         0.5f,
                                         "Control the damage sharing multiplier to all enemies of the same type");
            RejectedDaggerMinEnemyCount = Config.Bind<int>("Item : " + name,
                                         "Enemy count",
                                         5,
                                         "Control the count of enemies left required to activate the single damage bonus");
            RejectedDaggerDamageToMin = Config.Bind<float>("Item : " + name,
                                         "Damage increase",
                                         45f,
                                         "Control the base damage increase when enemy count is true");
            RejectedDaggerChampionReduction = Config.Bind<float>("Item : " + name,
                                         "Damage to champion reduction",
                                         2f,
                                         "Control the division applied to the damage increase if the enemy is a champion");
            ModSettingsManager.AddOption(new CheckBoxOption(RejectedDaggerAIBlacklist, new CheckBoxConfig() { restartRequired = true }));
            ModSettingsManager.AddOption(new StepSliderOption(RejectedDaggerTier, new StepSliderConfig() { min = 1, max = 3, increment = 1f, restartRequired = true }));
            ModSettingsManager.AddOption(new FloatFieldOption(RejectedDaggerDamageToAll));
            ModSettingsManager.AddOption(new IntFieldOption(RejectedDaggerMinEnemyCount));
            ModSettingsManager.AddOption(new FloatFieldOption(RejectedDaggerDamageToMin));
            ModSettingsManager.AddOption(new FloatFieldOption(RejectedDaggerChampionReduction));
        }

        private static void Item()
        {
            RejectedDaggerItemDef = ScriptableObject.CreateInstance<ItemDef>();
            RejectedDaggerItemDef.name = name.Replace(" ", "");
            RejectedDaggerItemDef.nameToken = name.ToUpper().Replace(" ", "") + "_NAME";
            RejectedDaggerItemDef.pickupToken = name.ToUpper().Replace(" ", "") + "_PICKUP";
            RejectedDaggerItemDef.descriptionToken = name.ToUpper().Replace(" ", "") + "_DESC";
            RejectedDaggerItemDef.loreToken = name.ToUpper().Replace(" ", "") + "_LORE";
            switch (RejectedDaggerTier.Value)
            {
                case 1:
                    RejectedDaggerItemDef.deprecatedTier = ItemTier.Tier1;
                    break;
                case 2:
                    RejectedDaggerItemDef.deprecatedTier = ItemTier.Tier2;
                    break;
                case 3:
                    RejectedDaggerItemDef.deprecatedTier = ItemTier.Tier3;
                    break;

            }
            RejectedDaggerItemDef.pickupIconSprite = RejectedDaggerIcon;
            RejectedDaggerItemDef.pickupModelPrefab = RejectedDaggerPrefab;
            RejectedDaggerItemDef.canRemove = true;
            RejectedDaggerItemDef.hidden = false;
            var tags = new List<ItemTag>() { ItemTag.Damage };
            if (RejectedDaggerAIBlacklist.Value)
            {
                tags.Add(ItemTag.AIBlacklist);
            }
            RejectedDaggerItemDef.tags = tags.ToArray();
            var displayRules = new ItemDisplayRuleDict(null);
            ItemAPI.Add(new CustomItem(RejectedDaggerItemDef, displayRules));
            On.RoR2.GlobalEventManager.OnHitEnemy += ShareDamage;
            //On.RoR2.GlobalEventManager.ProcessHitEnemy += ShareDamage;
        }
        /*
        private static void ShareDamage(On.RoR2.GlobalEventManager.orig_ProcessHitEnemy orig, GlobalEventManager self, DamageInfo damageInfo, GameObject victim)
        {
            var attacker = damageInfo.attacker;
            var body = attacker ? attacker.GetComponent<CharacterBody>() : null;
            int count = 0;
            if (body != null)
            {
                count = body.inventory.GetItemCount(RejectedDaggerItemDef);

            }
            if (count > 0)
            {
                if (damageInfo.attacker && !damageInfo.rejected)
                {
                    var victimBody = victim.GetComponent<CharacterBody>();
                    int enemiesLeft = 0;
                    //Debug.Log(victim);
                    //Debug.Log(victimBody);
                    //Debug.Log(victimBody.teamComponent.teamIndex);
                    //Debug.Log(victimBody.master.masterIndex);
                    foreach (var characterBody in CharacterBody.readOnlyInstancesList)
                    {
                        //Debug.Log(characterBody);
                        //Debug.Log(characterBody.teamComponent.teamIndex);
                        //Debug.Log(characterBody.master.masterIndex);
                        if (victimBody && characterBody && victimBody.teamComponent.teamIndex == characterBody.teamComponent.teamIndex && victimBody.master.masterIndex == characterBody.master.masterIndex && victimBody != characterBody)
                        {
                            enemiesLeft++;
                            var newDamage = damageInfo;
                            newDamage.damage *= RejectedDaggerDamageToAll.Value / 100 * count;
                            newDamage.position = characterBody.transform.position;
                            newDamage.procCoefficient *= 0.1f;
                            newDamage.damageColorIndex = DamageColorIndex.Nearby;
                            EffectManager.SimpleImpactEffect(HealthComponent.AssetReferences.crowbarImpactEffectPrefab, newDamage.position, -newDamage.force, true);
                            characterBody.healthComponent.TakeDamage(newDamage);

                        }
                    }
                    if (enemiesLeft < RejectedDaggerMinEnemyCount.Value)
                    {
                        var newDamage = damageInfo;
                        float damageIncrease = RejectedDaggerDamageToMin.Value;

                        if (victimBody.isChampion)
                        {
                            damageIncrease /= RejectedDaggerChampionReduction.Value;
                        }
                        if (enemiesLeft <= 0)
                        {
                            enemiesLeft = 1;
                        }
                        newDamage.damage *= damageIncrease * (count / (enemiesLeft)) / 100;
                        newDamage.damageColorIndex = DamageColorIndex.Nearby;
                        victim.GetComponent<HealthComponent>().TakeDamage(newDamage);
                    }
                }
            }
            orig(self, damageInfo, victim);
        }*/
        
        private static void ShareDamage(On.RoR2.GlobalEventManager.orig_OnHitEnemy orig, GlobalEventManager self, DamageInfo damageInfo, GameObject victim)
        {
            var attacker = damageInfo.attacker;
            var body = attacker ? attacker.GetComponent<CharacterBody>() : null;
            int count = 0;
            if (body != null)
            {
                count = body.inventory.GetItemCount(RejectedDaggerItemDef);

            }
            if (count > 0)
            {
                if (damageInfo.attacker && !damageInfo.rejected)
                {                        
                    var victimBody = victim.GetComponent<CharacterBody>();
                    CharacterBody[] characterBodyArray = new CharacterBody[0];
                    int enemiesLeft = 0;
                    //Debug.Log(victim);
                    //Debug.Log(victimBody);
                    //Debug.Log(victimBody.teamComponent.teamIndex);
                    //Debug.Log(victimBody.master.masterIndex);

                    foreach (var characterBody in CharacterBody.readOnlyInstancesList)
                    {
                        //Debug.Log(characterBody);
                        //Debug.Log(characterBody.teamComponent.teamIndex);
                        //Debug.Log(characterBody.master.masterIndex);
                        if (victimBody && characterBody && victimBody.teamComponent.teamIndex == characterBody.teamComponent.teamIndex)// && victimBody.master.masterIndex == characterBody.master.masterIndex && victimBody != characterBody)
                        {

                            enemiesLeft++;
                            Array.Resize(ref characterBodyArray, enemiesLeft);
                            characterBodyArray.SetValue(characterBody, enemiesLeft - 1);
                            

                        }
                    }
                    foreach (var characterBody in characterBodyArray)
                    {
                        DamageInfo damageInfo2 = new DamageInfo
                        {
                            damage = damageInfo.damage / characterBodyArray.Length * RejectedDaggerDamageToAll.Value * count,
                            damageColorIndex = DamageColorIndex.Item,
                            damageType = DamageType.Generic,
                            attacker = attacker,
                            crit = damageInfo.crit,
                            force = Vector3.zero,
                            inflictor = null,
                            position = characterBody.transform.position,
                            procChainMask = damageInfo.procChainMask,
                            procCoefficient = 0f
                        };
                        //EffectManager.SimpleImpactEffect(HealthComponent.AssetReferences.crowbarImpactEffectPrefab, newDamage.position, -newDamage.force, true);
                        characterBody.healthComponent.TakeDamage(damageInfo2);
                    }
                    
                    /*
                    if (enemiesLeft < RejectedDaggerMinEnemyCount.Value)
                    {
                        var newDamage = damageInfo;
                        float damageIncrease = RejectedDaggerDamageToMin.Value;

                        if (victimBody.isChampion)
                        {
                            damageIncrease /= RejectedDaggerChampionReduction.Value;
                        }
                        if (enemiesLeft <= 0)
                        {
                            enemiesLeft = 1;
                        }
                        DamageInfo damageInfo2 = new DamageInfo
                        {
                            damage = damageInfo.damage * damageIncrease * (count / (enemiesLeft)) / 100,
                            damageColorIndex = DamageColorIndex.Item,
                            damageType = DamageType.Generic,
                            attacker = attacker,
                            crit = damageInfo.crit,
                            force = Vector3.zero,
                            inflictor = null,
                            position = victim.transform.position,
                            procChainMask = damageInfo.procChainMask,
                            procCoefficient = 0f
                        };
                        victim.GetComponent<HealthComponent>().TakeDamage(newDamage);
                    }*/
                }
            }
            orig(self, damageInfo, victim);

        }

        private static void AddLanguageTokens()
        {
            LanguageAPI.Add(name.ToUpper().Replace(" ", "") + "_NAME", name);
            LanguageAPI.Add(name.ToUpper().Replace(" ", "") + "_PICKUP", "Share 10% (+10% per item stack) between all monsters of the same type. Deal more damage on less monsters");
            LanguageAPI.Add(name.ToUpper().Replace(" ", "") + "_DESC", "Share 10% (+10% per item stack) between all monsters of the same type. Deal more damage on less monsters");
            LanguageAPI.Add(name.ToUpper().Replace(" ", "") + "_LORE", "mmmm yummy");
        }
    }
}
