using R2API;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using static CaeliImperiumPlugin.CaeliImperium;

namespace CaeliImperium.Buffs
{
    internal static class TalismanVictimBuff
    {
        public static BuffDef TalismanVictimBuffDef;
        internal static Sprite TalismanVictimIcon;
        internal static void Init()
        {


            TalismanVictimIcon = MainAssets.LoadAsset<Sprite>("Assets/Icons/talismanbuff.png");

            Buff();

        }
        private static void Buff()
        {
            TalismanVictimBuffDef = ScriptableObject.CreateInstance<BuffDef>();
            TalismanVictimBuffDef.name = "ciTalismaned";
            TalismanVictimBuffDef.buffColor = Color.white;
            TalismanVictimBuffDef.canStack = true;
            TalismanVictimBuffDef.isDebuff = false;
            TalismanVictimBuffDef.ignoreGrowthNectar = true;
            TalismanVictimBuffDef.iconSprite = TalismanVictimIcon;
            TalismanVictimBuffDef.isHidden = true;
            TalismanVictimBuffDef.isCooldown = false;
            ContentAddition.AddBuffDef(TalismanVictimBuffDef);
        }
    }
}
