using R2API;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using static ReignFromGreatBeyondPlugin.CaeliImperium;

namespace CaeliImperium.Buffs
{
    internal static class TalismanAttackerBuff
    {
        public static BuffDef TalismanAttackerBuffDef;
        internal static Sprite TalismanAttackerIcon;
        internal static void Init()
        {


            TalismanAttackerIcon = MainAssets.LoadAsset<Sprite>("Assets/Icons/TalismanReady.png");

            Buff();

        }
        private static void Buff()
        {
            TalismanAttackerBuffDef = ScriptableObject.CreateInstance<BuffDef>();
            TalismanAttackerBuffDef.name = "ciTalismanReady";
            TalismanAttackerBuffDef.buffColor = Color.white;
            TalismanAttackerBuffDef.canStack = true;
            TalismanAttackerBuffDef.isDebuff = false;
            TalismanAttackerBuffDef.ignoreGrowthNectar = true;
            TalismanAttackerBuffDef.iconSprite = TalismanAttackerIcon;
            TalismanAttackerBuffDef.isHidden = false;
            TalismanAttackerBuffDef.isCooldown = false;
            ContentAddition.AddBuffDef(TalismanAttackerBuffDef);
        }
    }
}
