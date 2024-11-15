using R2API;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using static KitchenSanFieroPlugin.KitchenSanFiero;

namespace KitchenSanFiero.Buffs
{
    internal static class DeathCountBuff
    {
        public static BuffDef DeathCountBuffDef;
        internal static Sprite RessurectedTimerIcon;
        internal static void Init()
        {


            RessurectedTimerIcon = MainAssets.LoadAsset<Sprite>("Assets/Icons/PainkillersIcon.png");

            Buff();

        }
        private static void Buff()
        {
            DeathCountBuffDef = ScriptableObject.CreateInstance<BuffDef>();
            DeathCountBuffDef.name = "ksfDeathCount";
            DeathCountBuffDef.buffColor = Color.black;
            DeathCountBuffDef.canStack = true;
            DeathCountBuffDef.isDebuff = false;
            DeathCountBuffDef.ignoreGrowthNectar = true;
            DeathCountBuffDef.iconSprite = RessurectedTimerIcon;
            DeathCountBuffDef.isHidden = true;
            DeathCountBuffDef.isCooldown = false;
            ContentAddition.AddBuffDef(DeathCountBuffDef);
        }
    }
}
