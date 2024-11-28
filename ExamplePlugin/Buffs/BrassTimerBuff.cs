using CaeliImperium.Items;
using R2API;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using static CaeliImperiumPlugin.CaeliImperium;
using CaeliImperium;
using UnityEngine.Networking;
using RoR2.Audio;

namespace CaeliImperium.Buffs
{
    internal static class BrassTimerBuff
    {
        public static BuffDef BrassTimerBuffDef;
        internal static Sprite EmergencyMedicalTreatmentCooldownIcon;
        
        internal static void Init()
        {


            EmergencyMedicalTreatmentCooldownIcon = MainAssets.LoadAsset<Sprite>("Assets/Icons/BellBoosted.png");

            Buff();

        }
        private static void Buff()
        {
            BrassTimerBuffDef = ScriptableObject.CreateInstance<BuffDef>();
            BrassTimerBuffDef.name = "ciBellTimer";
            BrassTimerBuffDef.buffColor = Color.grey;
            BrassTimerBuffDef.canStack = false;
            BrassTimerBuffDef.isDebuff = false;
            BrassTimerBuffDef.ignoreGrowthNectar = false;
            BrassTimerBuffDef.isCooldown = true;
            BrassTimerBuffDef.iconSprite = EmergencyMedicalTreatmentCooldownIcon;
            BrassTimerBuffDef.isHidden = true;
            ContentAddition.AddBuffDef(BrassTimerBuffDef);
            On.RoR2.CharacterBody.OnBuffFinalStackLost += CharacterBody_OnBuffFinalStackLost;
        }
        private static void CharacterBody_OnBuffFinalStackLost(On.RoR2.CharacterBody.orig_OnBuffFinalStackLost orig, CharacterBody self, BuffDef buffDef)
        {
            orig(self, buffDef);
            if (buffDef == BrassTimerBuffDef)
            {
                self.AddTimedBuff(BrassBoostedBuff.BrassBoostedBuffDef, BrassBell.BrassBellEffectTime.Value + BrassBell.BrassBellEffectTimeStack.Value);
                
            }
        }/*
        private static void CreateSound()
        {
            BellSound = ScriptableObject.CreateInstance<NetworkSoundEventDef>();
            BellSound.eventName = "Play_bell";
            R2API.ContentAddition.AddNetworkSoundEventDef(BellSound);
        }*/
    }
}
