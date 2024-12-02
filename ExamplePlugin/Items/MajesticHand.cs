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
using System.Numerics;
using UnityEngine.Networking;

namespace CaeliImperium.Items
{
    internal static class MajesticHand
    {
        internal static GameObject MajesticHandPrefab;
        internal static Sprite MajesticHandIcon;
        public static ItemDef MajesticHandItemDef;
        public static ConfigEntry<bool> MajesticHandEnable;
        public static ConfigEntry<bool> MajesticHandAIBlacklist;
        public static ConfigEntry<float> MajesticHandTier;
        public static ConfigEntry<float> MajesticHandChance;
        public static ConfigEntry<float> MajesticHandMaxChance;
        public static ConfigEntry<float> MajesticHandDoDamage;
        public static ConfigEntry<float> MajesticHandDoDamageStack;
        //public static ConfigEntry<bool> MajesticHandFunction;
        public static string name = "Majestic Hand";

        internal static void Init()
        {
            AddConfigs();
            string tier = "Assets/Icons/MajesticHandTier3.png";
            switch (MajesticHandTier.Value)
            {
                case 1:
                    tier = "Assets/Icons/MajesticHandTier1.png";
                    break;
                case 2:
                    tier = "Assets/Icons/MajesticHandTier2.png";
                    break;
                case 3:
                    tier = "Assets/Icons/MajesticHandTier3.png";
                    break;

            }
            MajesticHandPrefab = MainAssets.LoadAsset<GameObject>("Assets/Models/Prefabs/revengeitem.prefab");
            MajesticHandIcon = MainAssets.LoadAsset<Sprite>(tier);
            if (!MajesticHandEnable.Value)
            {
                return;
            }
            Item();

            AddLanguageTokens();
            KillswitchBuff.Init();
        }

        private static void AddConfigs()
        {
            MajesticHandEnable = Config.Bind<bool>("Item : " + name,
                             "Activation",
                             true,
                             "Enable this item?");
            MajesticHandAIBlacklist = Config.Bind<bool>("Item : " + name,
                             "AI Blacklist",
                             true,
                             "Blacklist this item from enemies?");
            MajesticHandTier = Config.Bind<float>("Item : " + name,
                                         "Item tier",
                                         2f,
                                         "1: Common/White\n2: Rare/Green\n3: Legendary/Red");
            MajesticHandChance = Config.Bind<float>("Item : " + name,
                                         "Death chance",
                                         5f,
                                         "Control the death chance in percerntage\nSet to 0 to disable this effect");
            MajesticHandMaxChance = Config.Bind<float>("Item : " + name,
                                         "Max chance",
                                         15f,
                                         "Control the maximum death chance in percerntage");
            MajesticHandDoDamage = Config.Bind<float>("Item : " + name,
                             "Damage increase",
                             12f,
                             "Control how much damage increases based on the targets remaining health percentage in percentage\nSet to 0 to disable this effect");
            MajesticHandDoDamageStack = Config.Bind<float>("Item : " + name,
                             "Damage increase stack",
                             12f,
                             "Control how much damage increases based on the targets remaining health percentage per item stack in percentage");
            //MajesticHandFunction = Config.Bind<bool>("Item : " + name,
            //                             "Alternative Killswitch function",
            //                             false,
            //                             "No timer. Kill initializes on Killswitch buff count cap");
            ModSettingsManager.AddOption(new CheckBoxOption(MajesticHandEnable, new CheckBoxConfig() { restartRequired = true }));
            ModSettingsManager.AddOption(new CheckBoxOption(MajesticHandAIBlacklist, new CheckBoxConfig() { restartRequired = true }));
            ModSettingsManager.AddOption(new StepSliderOption(MajesticHandTier, new StepSliderConfig() { min = 1, max = 3, increment = 1f, restartRequired = true }));
            ModSettingsManager.AddOption(new FloatFieldOption(MajesticHandChance));
            ModSettingsManager.AddOption(new FloatFieldOption(MajesticHandMaxChance));
            ModSettingsManager.AddOption(new FloatFieldOption(MajesticHandDoDamage));
            ModSettingsManager.AddOption(new FloatFieldOption(MajesticHandDoDamageStack));
            //ModSettingsManager.AddOption(new CheckBoxOption(MajesticHandFunction, new CheckBoxConfig() { restartRequired = true }));
        }

        private static void Item()
        {
            MajesticHandItemDef = ScriptableObject.CreateInstance<ItemDef>();
            MajesticHandItemDef.name = name.Replace(" ", "");
            MajesticHandItemDef.nameToken = name.Replace(" ", "").ToUpper() + "_NAME";
            MajesticHandItemDef.pickupToken = name.Replace(" ", "").ToUpper() + "_PICKUP";
            MajesticHandItemDef.descriptionToken = name.Replace(" ", "").ToUpper() + "_DESC";
            MajesticHandItemDef.loreToken = name.Replace(" ", "").ToUpper() + "_LORE";
            switch (MajesticHandTier.Value)
            {
                case 1:
                    MajesticHandItemDef.deprecatedTier = ItemTier.Tier1;
                    break;
                case 2:
                    MajesticHandItemDef.deprecatedTier = ItemTier.Tier2;
                    break;
                case 3:
                    MajesticHandItemDef.deprecatedTier = ItemTier.Tier3;
                    break;

            }
            MajesticHandItemDef.pickupIconSprite = MajesticHandIcon;
            MajesticHandItemDef.pickupModelPrefab = MajesticHandPrefab;
            MajesticHandItemDef.canRemove = true;
            MajesticHandItemDef.hidden = false;
            var tags = new List<ItemTag>() { ItemTag.Damage };
            if (MajesticHandAIBlacklist.Value)
            {
                tags.Add(ItemTag.AIBlacklist);
            }
            MajesticHandItemDef.tags = tags.ToArray();
            MajesticHandItemDef.requiredExpansion = CaeliImperiumExpansionDef;
            var displayRules = new ItemDisplayRuleDict(null);
            ItemAPI.Add(new CustomItem(MajesticHandItemDef, displayRules));
            On.RoR2.CharacterBody.Start += Die;
            //On.RoR2.GlobalEventManager.OnHitEnemy += KillswitchChance;
            On.RoR2.HealthComponent.TakeDamageProcess += IncreaseDamage;
        }

        private static void IncreaseDamage(On.RoR2.HealthComponent.orig_TakeDamageProcess orig, HealthComponent self, DamageInfo damageInfo)
        {
            if (MajesticHandDoDamage.Value > 0 && damageInfo != null && self != null && damageInfo.attacker && !damageInfo.rejected)
            {
                var attackerBody = damageInfo.attacker.GetComponent<CharacterBody>();
                int itemCount = 0;
                try
                {
                itemCount = attackerBody.inventory ? attackerBody.inventory.GetItemCount(MajesticHandItemDef) : 0;

                }
                catch (Exception ex)
                {

                }
                if (itemCount > 0)
                {
                    damageInfo.damage *= 1 + ((1 - self.combinedHealthFraction) * ((MajesticHandDoDamage.Value / 100) + ((itemCount - 1) * (MajesticHandDoDamageStack.Value / 100))));
                }
            }
            orig(self, damageInfo);
        }

        /*
private static void KillswitchChance(On.RoR2.GlobalEventManager.orig_OnHitEnemy orig, GlobalEventManager self, DamageInfo damageInfo, GameObject victim)
{
   orig(self, damageInfo, victim);
   var attacker = damageInfo.attacker;
   var body = attacker ? attacker.GetComponent<CharacterBody>() : null;
   int count = 0;
   if (body != null)
   {
       count = body.inventory.GetItemCount(MajesticHandItemDef);

   }
   if (count > 0)
   {
       if (damageInfo.attacker && !damageInfo.rejected)
       {
           if (MajesticHandFunction.Value)
           {
               if (Util.CheckRoll(Util.ConvertAmplificationPercentageIntoReductionPercentage(count * 4) / 4, attacker.GetComponent<CharacterMaster>().luck / 4))
               {

                   victim.GetComponent<CharacterBody>().AddBuff(KillswitchBuff.KillSwitchBuffDef);

               }
           }
           else
           {
               int superRoll = (int)Math.Floor((float)(count / 10));
               for (int i = 0; i < superRoll ; i++)
               {
                       victim.GetComponent<CharacterBody>().RemoveBuff(KillswitchBuff.KillSwitchBuffDef);
               }
               if (Util.CheckRoll(count * 10 - (superRoll * 100), attacker.GetComponent<CharacterMaster>()))
               {
                   victim.GetComponent<CharacterBody>().RemoveBuff(KillswitchBuff.KillSwitchBuffDef);
               }

           }

       }
   }
}
public class KillswitchComponent : MonoBehaviour
{
   public CharacterBody body;
   public float timer = 0;
   public void Awake()
   {
       //body = gameObject.GetComponent<CharacterBody>();
       //timer = 0;
   }
   public void FixedUpdate()
   {
       if (body)
       {
           int buffCount = body.GetBuffCount(KillswitchBuff.KillSwitchBuffDef);
           if (buffCount > 0)
           {
               timer += Time.fixedDeltaTime;
               if (timer > 1f)
               {
                   body.RemoveBuff(KillswitchBuff.KillSwitchBuffDef);
                   timer = 0;
               }
           }
       }
   }
}*/
        private static void Die(On.RoR2.CharacterBody.orig_Start orig, CharacterBody self)
        {
            orig(self);
            if (MajesticHandChance.Value > 0 && self)
            {
                //if (!self.GetComponent<KillswitchComponent>())
                //{
                //    self.gameObject.AddComponent<KillswitchComponent>();
                //    self.gameObject.AddComponent<KillswitchComponent>().body = self;
                //}
                //float finalChance = 0;
                //int finalTime = 61;
                //float allLuck = 0;
                //int itemCountMonster = self.inventory ? self.inventory.GetItemCount(MajesticHandItemDef) : 0;
                int teamItemCount = Util.GetItemCountGlobal(MajesticHandItemDef.itemIndex, true) - Util.GetItemCountForTeam(self.teamComponent.teamIndex, MajesticHandItemDef.itemIndex, true);
                if (teamItemCount > 0)
                {
                    if (Util.CheckRoll(ConvertAmplificationPercentageIntoReductionPercentage(teamItemCount * MajesticHandChance.Value, MajesticHandMaxChance.Value)))
                    {
                        EffectManager.SpawnEffect(OrbStorageUtility.Get("Prefabs/Effects/ImpactEffects/LightningStrikeImpact"), new EffectData
                        {
                            origin = self.transform.position,
                        }, true);
                        /*UnityEngine.Vector3 vector = UnityEngine.Vector3.zero;
                        uint difficultyScaledCost = (uint)Run.instance.GetDifficultyScaledCost((int)25f);
                        float deadMoney = self.master.money / difficultyScaledCost;
                        GameObject gameObject11 = UnityEngine.Object.Instantiate<GameObject>(LegacyResourcesAPI.Load<GameObject>("Prefabs/NetworkedObjects/BonusMoneyPack"), vector, UnityEngine.Random.rotation);
                        TeamFilter component13 = gameObject11.GetComponent<TeamFilter>();
                        if (component13)
                        {
                            component13.teamIndex = TeamIndex.Player;
                        }
                        for (int i = 0; i < deadMoney; i++)
                        {
                        NetworkServer.Spawn(gameObject11);
                        }*/
                        self.master.TrueKill();
                        //self.healthComponent.Suicide();
                    }
                }
                /*(foreach (var characterBody in CharacterBody.readOnlyInstancesList)
                {
                    if (MajesticHandFunction.Value)
                    {
                        if (itemCountMonster > 0)
                        {
                            if (characterBody.teamComponent.teamIndex != self.teamComponent.teamIndex && Util.CheckRoll(Util.ConvertAmplificationPercentageIntoReductionPercentage(itemCountMonster * 2), self.master))
                            {
                                characterBody.AddBuff(KillswitchBuff.KillSwitchBuffDef);
                                

                            }
                        }
                    }
                    else
                    {
                        if (itemCountMonster > 0)
                        {
                            if(characterBody.teamComponent.teamIndex != self.teamComponent.teamIndex)
                            {
                                if (!characterBody.HasBuff(KillswitchBuff.KillSwitchBuffDef))
                                {
                                    int finalTime2 = finalTime - 1;
                                    if (self.isElite)
                                    {
                                        finalTime2 *= 2;
                                    }
                                    if (self.isChampion || self.isLocalPlayer)
                                    {
                                        finalTime2 *= 5;
                                    }
                                    for (int i = 0; i < finalTime; i++)
                                    {
                                        characterBody.AddBuff(KillswitchBuff.KillSwitchBuffDef);
                                    }
                                }
                                else
                                {
                                    int superRoll = (int)Math.Floor((float)(itemCountMonster / 10));
                                    for (int i = 0; i < superRoll; i++)
                                    {
                                        characterBody.RemoveBuff(KillswitchBuff.KillSwitchBuffDef);
                                    }
                                    if (Util.CheckRoll(itemCountMonster * 10 - (superRoll * 100), self.master))
                                    {
                                        characterBody.RemoveBuff(KillswitchBuff.KillSwitchBuffDef);
                                    }
                                }
                            }
                        }
                    }

                    int itemCount = characterBody.inventory ? characterBody.inventory.GetItemCount(MajesticHandItemDef) : 0;
                    if (itemCount > 0)
                    {
                        if (characterBody.teamComponent.teamIndex != self.teamComponent.teamIndex)
                        {
                            finalChance += 5 * itemCount;
                            finalTime -= 1 * itemCount;
                            allLuck += self.master.luck;
                        }
                    }
                }
                if (finalChance > 0 && Util.CheckRoll(Util.ConvertAmplificationPercentageIntoReductionPercentage(finalChance), allLuck)){
                            self.healthComponent.Suicide();

                }
                if (!MajesticHandFunction.Value && finalChance > 0)
                {
                    if (self.isElite)
                    {
                        finalTime *= 2;
                    }
                    if (self.isChampion || self.isLocalPlayer)
                    {
                        finalTime *= 5;
                    }
                    for (int i = 0; i < finalTime; i++)
                    {
                        self.AddBuff(KillswitchBuff.KillSwitchBuffDef);
                    }
                }
            }8/
            
            /*
            int itemCountPlayers = Util.GetItemCountForTeam(TeamIndex.Player, InvisibleHandItemDef.itemIndex, true);
            if (itemCountPlayers > 0)
            {
                if (Util.CheckRoll(Util.ConvertAmplificationPercentageIntoReductionPercentage(itemCountPlayers * 10)))
                {
                self.healthComponent.Suicide();

                }*/
            }
        }

        private static void AddLanguageTokens()
        {
            string death = "";
            if (MajesticHandChance.Value > 0)
            {
                death += "Monsters have <style=cDeath>" + MajesticHandChance.Value + "%</style> <style=cStack>(+" + MajesticHandChance.Value + "% per item stack hyperbollicaly)</style> to <style=cDeath>die</style> on spawn up to " + MajesticHandMaxChance.Value + "%. ";
            }
            if (MajesticHandDoDamage.Value > 0)
            {
                death += "Deal <style=cIsDamage>more damage</style> the <style=cIsHealth>weaker</style> the enemy is up to <style=cIsDamage>" + MajesticHandDoDamage.Value +"%</style> <style=cStack>(+" + MajesticHandDoDamageStack.Value + "% per item stack)</style>";
            }
            LanguageAPI.Add(name.Replace(" ", "").ToUpper() + "_NAME", name);
            LanguageAPI.Add(name.Replace(" ", "").ToUpper() + "_PICKUP", death);
            LanguageAPI.Add(name.Replace(" ", "").ToUpper() + "_DESC", death);
            LanguageAPI.Add(name.Replace(" ", "").ToUpper() + "_LORE", "\"If we kill the god, it would be necessary to replace him.\"");
        }
    }
}