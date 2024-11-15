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
    internal static class HasCardBuff

    {
        public static BuffDef HasCardBuffDef;
        internal static Sprite HasCardIcon;

        internal static void Init()
        {


            HasCardIcon = MainAssets.LoadAsset<Sprite>("Assets/Icons/Killswitchbuff.png");

            Buff();

        }
        private static void Buff()
        {
            HasCardBuffDef = ScriptableObject.CreateInstance<BuffDef>();
            HasCardBuffDef.name = "ksfHasCard";
            HasCardBuffDef.buffColor = Color.white;
            HasCardBuffDef.canStack = false;
            HasCardBuffDef.isDebuff = false;
            HasCardBuffDef.ignoreGrowthNectar = true;
            HasCardBuffDef.iconSprite = HasCardIcon;
            HasCardBuffDef.isHidden = false;
            HasCardBuffDef.isCooldown = false;
            ContentAddition.AddBuffDef(HasCardBuffDef);
        }
    }
}
