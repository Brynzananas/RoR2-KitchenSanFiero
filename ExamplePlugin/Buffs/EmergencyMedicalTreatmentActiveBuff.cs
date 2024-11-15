using R2API;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using static KitchenSanFieroPlugin.KitchenSanFiero;

namespace KitchenSanFiero.Buffs
{
    internal static class EmergencyMedicalTreatmentActiveBuff
    {
        public static BuffDef EmergencyMedicalTreatmentActiveBuffDef;
        internal static Sprite EmergencyMedicalTreatmentActiveIcon;
        internal static void Init()
        {


            EmergencyMedicalTreatmentActiveIcon = MainAssets.LoadAsset<Sprite>("Assets/Icons/HealShield2.png");

            Buff();

        }
        private static void Buff()
        {
            EmergencyMedicalTreatmentActiveBuffDef = ScriptableObject.CreateInstance<BuffDef>();
            EmergencyMedicalTreatmentActiveBuffDef.name = "ksfEmergencyMedicalTreatmentActive";
            EmergencyMedicalTreatmentActiveBuffDef.buffColor = Color.white;
            EmergencyMedicalTreatmentActiveBuffDef.canStack = false;
            EmergencyMedicalTreatmentActiveBuffDef.isDebuff = false;
            EmergencyMedicalTreatmentActiveBuffDef.ignoreGrowthNectar = false;
            EmergencyMedicalTreatmentActiveBuffDef.isCooldown = false;
            EmergencyMedicalTreatmentActiveBuffDef.iconSprite = EmergencyMedicalTreatmentActiveIcon;
            EmergencyMedicalTreatmentActiveBuffDef.isHidden = false;
            ContentAddition.AddBuffDef(EmergencyMedicalTreatmentActiveBuffDef);
        }
    }
}
