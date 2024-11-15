using KitchenSanFiero.Items;
using R2API;
using System;
using System.Collections.Generic;
using System.Text;
using static R2API.RecalculateStatsAPI;
using UnityEngine;
using RoR2;
using static KitchenSanFieroPlugin.KitchenSanFiero;
using System.Runtime.CompilerServices;

namespace KitchenSanFiero.Buffs
{
    internal static class SmokingBuff

    {
        public static BuffDef SmokingBuffDef;
        internal static Sprite SmokingIcon;

        internal static void Init()
        {


            SmokingIcon = MainAssets.LoadAsset<Sprite>("Assets/Icons/Killswitchbuff.png");

            Buff();

        }
        private static void Buff()
        {
            SmokingBuffDef = ScriptableObject.CreateInstance<BuffDef>();
            SmokingBuffDef.name = "ksfSmoking";
            SmokingBuffDef.buffColor = Color.white;
            SmokingBuffDef.canStack = true;
            SmokingBuffDef.isDebuff = true;
            SmokingBuffDef.ignoreGrowthNectar = true;
            SmokingBuffDef.iconSprite = SmokingIcon;
            SmokingBuffDef.isHidden = false;
            SmokingBuffDef.isCooldown = false;
            ContentAddition.AddBuffDef(SmokingBuffDef);
        }
    }
}
