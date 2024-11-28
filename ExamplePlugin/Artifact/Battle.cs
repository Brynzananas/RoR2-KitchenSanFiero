using BepInEx;
using BepInEx.Configuration;
using CaeliImperiumPlugin;
using R2API;
using RiskOfOptions;
using RiskOfOptions.Components.Misc;
using RiskOfOptions.Options;
using RoR2;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.XR;
using static CaeliImperiumPlugin.CaeliImperium;

namespace CaeliImperium.Artifact
{
    internal static class Battle
    {
        internal static Sprite BattleIcon;
        internal static Sprite BattleIconDisabled;
        public static ArtifactDef BattleArtifactDef;
        public static bool changeTeleport = false;
        public static bool timerSet = false;
        public static float difficulty = 0;
        public static bool itHappened = false;
        public static ConfigEntry<float> BattleInterval;
        public static ConfigEntry<float> BattleCreditsAddition;
        public static ConfigEntry<bool> BattleMidwayBool;
        public static ConfigEntry<float> BattleMidwayDifficulty;
        public static ConfigEntry<float> BattleMidwayCredits;
        public static ConfigEntry<float> BattleChargeOnKill;
        public static ConfigEntry<float> BattleZoneRange;
        public static ConfigEntry<float> BattleTime;
        public static ConfigEntry<float> BattleDrizzleDifficulty;
        public static ConfigEntry<float> BattleRainstormDifficulty;
        public static ConfigEntry<float> BattleMonsoonDifficulty;
        public static ConfigEntry<float> BattleHigherDifficulty;
        internal static void Init()
        {
            AddConfigs();
            BattleIcon = MainAssets.LoadAsset<Sprite>("Assets/Icons/BattleArtifactActivated.png");
            BattleIconDisabled = MainAssets.LoadAsset<Sprite>("Assets/Icons/BattleArtifactDeActivated.png");

            Artifact();

            AddLanguageTokens();
        }

        private static void AddConfigs()
        {
            BattleInterval = Config.Bind<float>("Artifact : Battle",
                             "Interval",
                             10f,
                             "Control the interval between monsters director credits addition");
            BattleCreditsAddition = Config.Bind<float>("Artifact : Battle",
                                         "Credits addition",
                                         20f,
                                         "Control the monsters director credits addition");
            BattleMidwayCredits = Config.Bind<float>("Artifact : Battle",
                                         "One time credits addition",
                                         2000f,
                                         "Control the one time monsters director credits addition midway teleport event");
            BattleChargeOnKill = Config.Bind<float>("Artifact : Battle",
                                         "Charge on kill multiplier",
                                         2f,
                                         "Control the multiplier for charge on kill");
            BattleZoneRange = Config.Bind<float>("Artifact : Battle",
                                         "Zone radius multiplier",
                                         2f,
                                         "Control the zone radius multiplier");
            BattleTime = Config.Bind<float>("Artifact : Battle",
                                         "Teleport time",
                                         240f,
                                         "Control the teleport time in seconds");
            BattleDrizzleDifficulty = Config.Bind<float>("Artifact : Battle",
                                         "Credits multiplier on Drizzle and lower",
                                         2f,
                                         "Control the credits multiplier for Drizzle and lower diffculties");
            BattleRainstormDifficulty = Config.Bind<float>("Artifact : Battle",
                                         "Credits multiplier on Rainstorm",
                                         3f,
                                         "Control the credits multiplier for Rainstorm");
            BattleMonsoonDifficulty = Config.Bind<float>("Artifact : Battle",
                                         "Credits multiplier on Monsoon",
                                         4f,
                                         "Control the credits multiplier for Monsoon");
            BattleHigherDifficulty = Config.Bind<float>("Artifact : Battle",
                                         "Credits multiplier on Eclipse and higher difficulties",
                                         4f,
                                         "Control the credits multiplier for Eclipse and higher difficulties");
            BattleMidwayBool = Config.Bind<bool>("Artifact : Battle",
                                         "Midway teleport event",
                                         true,
                                         "Do credits addition and difficulty scaling midway teleport event?");
            BattleMidwayDifficulty = Config.Bind<float>("Artifact : Battle",
                                         "Credits multiplier midway",
                                         2,
                                         "Control the credits multiplier midway teleporter event");
            ModSettingsManager.AddOption(new FloatFieldOption(BattleInterval));
            ModSettingsManager.AddOption(new FloatFieldOption(BattleCreditsAddition));
            ModSettingsManager.AddOption(new CheckBoxOption(BattleMidwayBool));
            ModSettingsManager.AddOption(new FloatFieldOption(BattleMidwayDifficulty));
            ModSettingsManager.AddOption(new FloatFieldOption(BattleMidwayCredits));
            ModSettingsManager.AddOption(new FloatFieldOption(BattleChargeOnKill));
            ModSettingsManager.AddOption(new FloatFieldOption(BattleZoneRange));
            ModSettingsManager.AddOption(new FloatFieldOption(BattleTime));
            ModSettingsManager.AddOption(new FloatFieldOption(BattleDrizzleDifficulty));
            ModSettingsManager.AddOption(new FloatFieldOption(BattleRainstormDifficulty));
            ModSettingsManager.AddOption(new FloatFieldOption(BattleMonsoonDifficulty));
            ModSettingsManager.AddOption(new FloatFieldOption(BattleHigherDifficulty));
        }

        public static void Artifact()
        {
            BattleArtifactDef = ScriptableObject.CreateInstance<ArtifactDef>();
            BattleArtifactDef.cachedName = "ARTIFACT_BATTLE";
            BattleArtifactDef.nameToken = "ARTIFACT_BATTLE_NAME";
            BattleArtifactDef.descriptionToken = "ARTIFACT_BATTLE_DESCRIPTION";
            BattleArtifactDef.smallIconSelectedSprite = BattleIcon;
            BattleArtifactDef.smallIconDeselectedSprite = BattleIconDisabled;
            ContentAddition.AddArtifactDef(BattleArtifactDef);
            Run.onRunStartGlobal += OnStart;
            //On.RoR2.Stage.FixedUpdate += TeleportStuffPleaseWork;
            TeleporterInteraction.onTeleporterBeginChargingGlobal += onTeleporterBeginChargingGlobal;
            //On.RoR2.Stage.FixedUpdate += ChageTeleport;
            //On.RoR2.CharacterBody.FixedUpdate += ChangeTeleport;
            TeleporterInteraction.onTeleporterChargedGlobal += OnTeleportFinish;
            On.RoR2.CharacterBody.OnDeathStart += AddCharge;
            Stage.onStageStartGlobal += ResetChange;
        }

        private static void ResetChange(Stage stage)
        {
            if (NetworkServer.active && RunArtifactManager.instance.IsArtifactEnabled(BattleArtifactDef))
            {
if (changeTeleport)
            {
                changeTeleport = false;

            }
            if (itHappened)
            {
                itHappened = false;

            }
            }
            
        }

        public class BattleComponent : MonoBehaviour
        {

            public float timer;
            public void FixedUpdate()
            {
                if (NetworkServer.active && RunArtifactManager.instance.IsArtifactEnabled(BattleArtifactDef) && changeTeleport)
                {
                    var teleport = TeleporterInteraction.instance;

                    timer += Time.fixedDeltaTime;
                    if (timer >= BattleInterval.Value)
                    {



                        teleport.bonusDirector.monsterCredit += BattleCreditsAddition.Value;
                        //Debug.Log("MonsterCredit " + teleport.bonusDirector.monsterCredit);
                        timer = 0f;
                    }

                    if (teleport.holdoutZoneController.charge > 0.5 && !itHappened && BattleMidwayBool.Value)
                    {
                        teleport.bonusDirector.monsterCredit += BattleMidwayCredits.Value;
                        teleport.bonusDirector.creditMultiplier *= BattleMidwayCredits.Value;

                        itHappened = true;
                    }
                }
            }
        }
        /*
private static void ChageTeleport(On.RoR2.Stage.orig_FixedUpdate orig, Stage self)
{
   orig(self);

   if (NetworkServer.active && RunArtifactManager.instance.IsArtifactEnabled(BattleArtifactDef) && changeTeleport)
   {
       var teleport = TeleporterInteraction.instance;

       timer += Time.fixedDeltaTime;
       if (timer >= 20f)
       {



           teleport.bonusDirector.monsterCredit += 50f;
           Debug.Log("MonsterCredit " + teleport.bonusDirector.monsterCredit);
           timer = 0f;
       }

       if (teleport.holdoutZoneController.charge > 5 && !itHappened)
       {
           Chat.AddMessage("HalvFile");
           teleport.bonusDirector.monsterCredit += 1000f;
           teleport.bonusDirector.creditMultiplier += 2f;
           teleport.holdoutZoneController.chargeRadiusDelta = 50f;
           teleport.holdoutZoneController.radiusSmoothTime = 4f;

           itHappened = true;
       }
   }
}
*/

        private static void AddCharge(On.RoR2.CharacterBody.orig_OnDeathStart orig, CharacterBody self)
        {
            orig(self);
            if (NetworkServer.active && RunArtifactManager.instance.IsArtifactEnabled(BattleArtifactDef) && changeTeleport)
            {
                var teleport = TeleporterInteraction.instance;
                teleport.holdoutZoneController.charge += 1 / teleport.holdoutZoneController.baseChargeDuration * BattleChargeOnKill.Value ;//0.5f / 100;
            }
        }

        private static void OnTeleportFinish(TeleporterInteraction interaction)
        {
            if (NetworkServer.active && RunArtifactManager.instance.IsArtifactEnabled(BattleArtifactDef))
            {
                itHappened = false;

                changeTeleport = false;
            }
        }

        private static void onTeleporterBeginChargingGlobal(TeleporterInteraction interaction)
        {
            if (NetworkServer.active && RunArtifactManager.instance.IsArtifactEnabled(BattleArtifactDef))
            {
                var teleport = TeleporterInteraction.instance;
                ;/*
                Debug.Log("CreditMultiplier" + teleport.bonusDirector.creditMultiplier);
                Debug.Log("MoneyIntervals " + teleport.bonusDirector.moneyWaveIntervals);
                Debug.Log("MoneyWaves" + teleport.bonusDirector.moneyWaves);
                Debug.Log("MonsterCredit " + teleport.bonusDirector.monsterCredit);
                Debug.Log("GoldRewardCoof " + teleport.bonusDirector.goldRewardCoefficient);
                Debug.Log("ExpRewardCCoof " + teleport.bonusDirector.expRewardCoefficient);
                Debug.Log("CombatSquad " + teleport.bonusDirector.combatSquad);
                Debug.Log("MonsterSpawnTimer " + teleport.bonusDirector.monsterSpawnTimer);
                Debug.Log("BaseChargeDuration " + teleport.holdoutZoneController.baseChargeDuration);
                Debug.Log("DischargeRate" + teleport.holdoutZoneController.dischargeRate);*/
                changeTeleport = true;
                itHappened = false;
                teleport.holdoutZoneController.chargeRadiusDelta = BattleZoneRange.Value * 100;
                teleport.holdoutZoneController.baseChargeDuration = BattleTime.Value;
                teleport.bonusDirector.creditMultiplier += difficulty;
;
            }
        }

        private static void OnStart(Run run)
        {
                changeTeleport = false;

            if (NetworkServer.active && RunArtifactManager.instance.IsArtifactEnabled(BattleArtifactDef))
            {
                run.gameObject.AddComponent<BattleComponent>();
                //Chat.AddMessage("My Artifact has been enabled!");
                if (Run.instance.selectedDifficulty <= DifficultyIndex.Easy)
                {
                    difficulty = BattleDrizzleDifficulty.Value;
                }
                if (Run.instance.selectedDifficulty == DifficultyIndex.Normal)
                {
                    difficulty = BattleRainstormDifficulty.Value;
                }
                if (Run.instance.selectedDifficulty == DifficultyIndex.Hard)
                {
                    difficulty = BattleMonsoonDifficulty.Value;
                }
                if (Run.instance.selectedDifficulty > DifficultyIndex.Hard)
                {
                    difficulty = BattleHigherDifficulty.Value;
                }

            }

        }

        public static void AddLanguageTokens()
        {
            LanguageAPI.Add("ARTIFACT_BATTLE_NAME", "Artifact of Battle");
            LanguageAPI.Add("ARTIFACT_BATTLE_DESCRIPTION", "Increases teleporter event zone and completion time. Kills recharge teleporter.\n\"In an endless fight he could not win, his music was still electric\"");//"Multiply teleporter radius by " + BattleZoneRange.Value + " and set its completion time to " + BattleTime.Value + " seconds. Every " + BattleInterval.Value + " seconds monsters gain " + BattleCreditsAddition.Value + " director credits. Midway teleporter multiply credits multiplier by " + BattleMidwayDifficulty.Value + " and monsters gain " + BattleMidwayCredits.Value + " director credits one time. Kills gain " + BattleChargeOnKill.Value + "% teleporter charge");
        }
    }
}
