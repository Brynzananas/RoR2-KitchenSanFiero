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
    internal static class PainkillBuff

    {
        public static BuffDef PainkillBuffDef;
        internal static Sprite PainkillIcon;

        internal static void Init()
        {


            PainkillIcon = MainAssets.LoadAsset<Sprite>("Assets/Icons/Killswitchbuff.png");

            Buff();

        }
        private static void Buff()
        {
            PainkillBuffDef = ScriptableObject.CreateInstance<BuffDef>();
            PainkillBuffDef.name = "ksfPainkill";
            PainkillBuffDef.buffColor = Color.white;
            PainkillBuffDef.canStack = true;
            PainkillBuffDef.isDebuff = false;
            PainkillBuffDef.ignoreGrowthNectar = false;
            PainkillBuffDef.iconSprite = PainkillIcon;
            PainkillBuffDef.isHidden = false;
            PainkillBuffDef.isCooldown = false;
            ContentAddition.AddBuffDef(PainkillBuffDef);
            GetStatCoefficients += Stats;
        }

        private static void Stats(CharacterBody sender, StatHookEventArgs args)
        {
            int buffCount = sender ? sender.GetBuffCount(PainkillBuffDef) : 0;
            if (buffCount > 0)
            {
                args.armorAdd = buffCount * 1;
            }
        }
    }
}
