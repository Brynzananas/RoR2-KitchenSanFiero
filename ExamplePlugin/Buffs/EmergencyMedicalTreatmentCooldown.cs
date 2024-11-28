using R2API;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using static CaeliImperiumPlugin.CaeliImperium;

namespace CaeliImperium.Buffs
{
    internal static class EmergencyMedicalTreatmentCooldownBuff
    {
        public static BuffDef EmergencyMedicalTreatmentCooldownBuffDef;
        internal static Sprite EmergencyMedicalTreatmentCooldownIcon;
        public static bool isCleanse = false;
        internal static void Init()
        {


            EmergencyMedicalTreatmentCooldownIcon = MainAssets.LoadAsset<Sprite>("Assets/Icons/HealShield2.png");

            Buff();

        }
        private static void Buff()
        {
            EmergencyMedicalTreatmentCooldownBuffDef = ScriptableObject.CreateInstance<BuffDef>();
            EmergencyMedicalTreatmentCooldownBuffDef.name = "ciEmergencyMedicalTreatmentCooldown";
            EmergencyMedicalTreatmentCooldownBuffDef.buffColor = Color.grey;
            EmergencyMedicalTreatmentCooldownBuffDef.canStack = false;
            EmergencyMedicalTreatmentCooldownBuffDef.isDebuff = false;
            EmergencyMedicalTreatmentCooldownBuffDef.ignoreGrowthNectar = true;
            EmergencyMedicalTreatmentCooldownBuffDef.isCooldown = false;
            EmergencyMedicalTreatmentCooldownBuffDef.iconSprite = EmergencyMedicalTreatmentCooldownIcon;
            EmergencyMedicalTreatmentCooldownBuffDef.isHidden = false;
            ContentAddition.AddBuffDef(EmergencyMedicalTreatmentCooldownBuffDef);
            //On.RoR2.CharacterBody.OnBuffFinalStackLost += CharacterBody_OnBuffFinalStackLost;
        }/*
        private static void CharacterBody_OnBuffFinalStackLost(On.RoR2.CharacterBody.orig_OnBuffFinalStackLost orig, CharacterBody self, BuffDef buffDef)
        {
            orig(self, buffDef);
            if (buffDef == EmergencyMedicalTreatmentCooldownBuffDef)
            {
                self.AddBuff(EmergencyMedicalTreatmentActiveBuff.EmergencyMedicalTreatmentActiveBuffDef);
            }
        }*/
    }
}
