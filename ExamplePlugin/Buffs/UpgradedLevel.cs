using R2API;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UIElements;
using static CaeliImperiumPlugin.CaeliImperium;
using static R2API.RecalculateStatsAPI;

namespace CaeliImperium.Buffs
{
    internal static class UpgradedLevelBuff
    {
        public static BuffDef UpgradedLevelBuffDef;
        internal static Sprite UpgradedLevelIcon;
        internal static void Init()
        {


            UpgradedLevelIcon = MainAssets.LoadAsset<Sprite>("Assets/Icons/PainkillersIcon.png");

            Buff();

        }
        private static void Buff()
        {
            UpgradedLevelBuffDef = ScriptableObject.CreateInstance<BuffDef>();
            UpgradedLevelBuffDef.name = "UpgradedLevel";
            UpgradedLevelBuffDef.buffColor = Color.red;
            UpgradedLevelBuffDef.canStack = true;
            UpgradedLevelBuffDef.isDebuff = false;
            UpgradedLevelBuffDef.ignoreGrowthNectar = true;
            UpgradedLevelBuffDef.iconSprite = UpgradedLevelIcon;
            UpgradedLevelBuffDef.isHidden = true;
            UpgradedLevelBuffDef.isCooldown = false;
            ContentAddition.AddBuffDef(UpgradedLevelBuffDef);
        }
    }
}
