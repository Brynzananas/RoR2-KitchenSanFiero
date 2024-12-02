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
using static CaeliImperiumPlugin.CaeliImperium;
using RoR2.Orbs;
using CaeliImperium.Buffs;
using static R2API.RecalculateStatsAPI;
using Rewired;
using UnityEngine.UIElements;

namespace CaeliImperium.Items
{
    internal static class RejectedDagger
    {
        internal static GameObject RejectedDaggerPrefab;
        internal static Sprite RejectedDaggerIcon;
        public static ItemDef RejectedDaggerItemDef;
        public static ConfigEntry<bool> RejectedDaggerEnable;
        public static ConfigEntry<bool> RejectedDaggerAIBlacklist;
        public static ConfigEntry<float> RejectedDaggerTier;
        public static ConfigEntry<bool> RejectedDaggerAltFunc;
        public static ConfigEntry<bool> RejectedDaggerTeamCount;
        public static ConfigEntry<bool> RejectedDaggerGlobalCount;
        public static ConfigEntry<float> RejectedDaggerDamageToAll;
        public static ConfigEntry<float> RejectedDaggerDamageToAllStack;
        /*public static ConfigEntry<int> RejectedDaggerMinEnemyCount;
        public static ConfigEntry<float> RejectedDaggerDamageToMin;
        public static ConfigEntry<float> RejectedDaggerChampionReduction;*/
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
            if (!RejectedDaggerEnable.Value)
            {
                return;
            }
            Item();

            AddLanguageTokens();
        }

        private static void AddConfigs()
        {
            RejectedDaggerEnable = Config.Bind<bool>("Item : " + name,
                             "Enable",
                             true,
                             "Enable this item?");
            RejectedDaggerAIBlacklist = Config.Bind<bool>("Item : " + name,
                             "AI Blacklist",
                             false,
                             "Blacklist this item from enemies?");
            RejectedDaggerTier = Config.Bind<float>("Item : " + name,
                                         "Item tier",
                                         3f,
                                         "1: Common/White\n2: Rare/Green\n3: Legendary/Red");
            RejectedDaggerAltFunc = Config.Bind<bool>("Item : " + name,
                             "Alternative function",
                             false,
                             "Enable alternative function?\nInstead of damaging all enemies, damage the same enemy\nDamage division replaces with multiplication");
            RejectedDaggerTeamCount = Config.Bind<bool>("Item : " + name,
                             "Team count",
                             false,
                             "Count all enemy teams?");
            RejectedDaggerGlobalCount = Config.Bind<bool>("Item : " + name,
                             "Global count",
                             false,
                             "Count all enemies?");
            RejectedDaggerDamageToAll = Config.Bind<float>("Item : " + name,
                                         "Damage sharing to all",
                                         50f,
                                         "Control the final damage in percentage");
            RejectedDaggerDamageToAllStack = Config.Bind<float>("Item : " + name,
                                         "Damage sharing to all stack",
                                         50f,
                                         "Control the final damage increase per item stack in percentage");
            /*RejectedDaggerMinEnemyCount = Config.Bind<int>("Item : " + name,
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
                                         "Control the division applied to the damage increase if the enemy is a champion");*/
            ModSettingsManager.AddOption(new CheckBoxOption(RejectedDaggerEnable, new CheckBoxConfig() { restartRequired = true }));
            ModSettingsManager.AddOption(new CheckBoxOption(RejectedDaggerAIBlacklist, new CheckBoxConfig() { restartRequired = true }));
            ModSettingsManager.AddOption(new StepSliderOption(RejectedDaggerTier, new StepSliderConfig() { min = 1, max = 3, increment = 1f, restartRequired = true }));
            ModSettingsManager.AddOption(new CheckBoxOption(RejectedDaggerAltFunc));
            ModSettingsManager.AddOption(new CheckBoxOption(RejectedDaggerTeamCount));
            ModSettingsManager.AddOption(new CheckBoxOption(RejectedDaggerGlobalCount));
            ModSettingsManager.AddOption(new FloatFieldOption(RejectedDaggerDamageToAll));
            ModSettingsManager.AddOption(new FloatFieldOption(RejectedDaggerDamageToAllStack));
            /*ModSettingsManager.AddOption(new IntFieldOption(RejectedDaggerMinEnemyCount));
            ModSettingsManager.AddOption(new FloatFieldOption(RejectedDaggerDamageToMin));
            ModSettingsManager.AddOption(new FloatFieldOption(RejectedDaggerChampionReduction));*/
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
            RejectedDaggerItemDef.requiredExpansion = CaeliImperiumExpansionDef;
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
                count = body.inventory ? body.inventory.GetItemCount(RejectedDaggerItemDef) : 0;

            }
            if (count > 0)
            {
                if (damageInfo.attacker && !damageInfo.rejected)
                {                        
                    var victimBody = victim.GetComponent<CharacterBody>();
                    CharacterBody[] characterBodyArray = new CharacterBody[0];
                    int enemiesLeft = 0;
                    //Debug.Log("Victim: " + victim);
                    //Debug.Log("VictimBody:" + victimBody);
                    //Debug.Log("VictimTeam:" + victimBody.teamComponent.teamIndex);
                    //Debug.Log("VictimMaster: " + victimBody.master.masterIndex);

                    foreach (var characterBody in CharacterBody.readOnlyInstancesList)
                    {
                        //Debug.Log("CharacterBody: " + characterBody);
                        //Debug.Log("CharacterTeam: " + characterBody.teamComponent.teamIndex);
                        //Debug.Log("CharacterTeam " + characterBody.master.masterIndex);
                        
                        //Debug.Log("GlobalCount: " +  globalCount);
                        if (victimBody && characterBody && body.teamComponent.teamIndex != characterBody.teamComponent.teamIndex)// && victimBody.master.masterIndex == characterBody.master.masterIndex && victimBody != characterBody)
                        {
                            bool teamCount = true;
                            if (!RejectedDaggerTeamCount.Value)
                            {
                                teamCount = victimBody.teamComponent.teamIndex == characterBody.teamComponent.teamIndex;
                            }
                            bool globalCount = true;
                            if (!RejectedDaggerGlobalCount.Value)
                            {
                                globalCount = victimBody.master && characterBody.master && victimBody.master.masterIndex == characterBody.master.masterIndex;
                            }
                            if (globalCount && teamCount)
                            {
                                enemiesLeft++;
                                Array.Resize(ref characterBodyArray, enemiesLeft);
                                characterBodyArray.SetValue(characterBody, enemiesLeft - 1);
                            }
                            
                            

                        }
                    }
                    float damageFinal = damageInfo.damage;
                    //Debug.Log("1Damage: " + damageFinal);
                    if (RejectedDaggerAltFunc.Value)
                    {
                        damageFinal *= characterBodyArray.Length;
                        //Debug.Log("2Damage: " + damageFinal);
                        DamageInfo damageInfo2 = new DamageInfo
                        {
                            damage = damageFinal * (RejectedDaggerDamageToAll.Value / 100) + ((count - 1) * (RejectedDaggerDamageToAllStack.Value / 100)),
                            damageColorIndex = DamageColorIndex.Item,
                            damageType = DamageType.Silent,
                            attacker = attacker,
                            crit = damageInfo.crit,
                            force = Vector3.zero,
                            inflictor = null,
                            position = victimBody.transform.position,
                            procChainMask = damageInfo.procChainMask,
                            procCoefficient = 0f
                        };
                        victimBody.healthComponent.TakeDamage(damageInfo2);
                    }
                    else
                    {
                        damageFinal /= characterBodyArray.Length;
                        //Debug.Log("2Damage: " + damageFinal);
                        foreach (var characterBody in characterBodyArray)
                        {

                            DamageInfo damageInfo2 = new DamageInfo
                            {
                                damage = damageFinal * (RejectedDaggerDamageToAll.Value / 100) + ((count - 1) * (RejectedDaggerDamageToAllStack.Value / 100)),
                                damageColorIndex = DamageColorIndex.Item,
                                damageType = DamageType.Silent,
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
            string damage = "";
            string team = "";
            if (!RejectedDaggerTeamCount.Value)
            {
                team = " on the same team";
            }
            if (!RejectedDaggerGlobalCount.Value)
            {
                team = " of the same type";
            }
            if (RejectedDaggerAltFunc.Value)
            {
                damage = "On hit, <style=cIsDamage>damage</style> the same enemy for <style=cIsDamage>" + RejectedDaggerDamageToAll.Value + "%</style> <style=cStack>(+" + RejectedDaggerDamageToAll.Value + "% per item stack)</style> <style=cIsDamage>TOTAL damage</style>. Damage is multiplied by the number of all enemies" +team;
            }
            else
            {
                damage = "On hit, <style=cIsDamage>damage</style> all enemies" + team + " for <style=cIsDamage>" + RejectedDaggerDamageToAll.Value + "%</style> <style=cStack>(+" + RejectedDaggerDamageToAll.Value + "% per item stack)</style> <style=cIsDamage>TOTAL damage</style>. Damage is divided by the number of all enemies" + team;
            }
            LanguageAPI.Add(name.ToUpper().Replace(" ", "") + "_NAME", name);
            LanguageAPI.Add(name.ToUpper().Replace(" ", "") + "_PICKUP", damage);
            LanguageAPI.Add(name.ToUpper().Replace(" ", "") + "_DESC", damage);
            LanguageAPI.Add(name.ToUpper().Replace(" ", "") + "_LORE", "\"What have I done? Why did I killed him? Is this what I fight for? I can't continue this path anymore...\"");
        }
    }
}
