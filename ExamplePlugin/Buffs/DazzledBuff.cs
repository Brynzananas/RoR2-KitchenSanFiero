using CaeliImperium.Items;
using R2API;
using System;
using System.Collections.Generic;
using System.Text;
using static R2API.RecalculateStatsAPI;
using UnityEngine;
using RoR2;
using static CaeliImperiumPlugin.CaeliImperium;
using RiskOfOptions.Options;
using RiskOfOptions;
using BepInEx.Configuration;

namespace CaeliImperium.Buffs
{
    internal static class DazzledBuff
    {
        public static BuffDef DazzledBuffDef;
        internal static Sprite DazzledIcon;
        public static ConfigEntry<float> DazzledDamageIncrease;

        internal static void Init()
        {

            DazzledDamageIncrease = Config.Bind<float>("Buff : Dazzled",
                             "Damage increase",
                             50f,
                             "Control how much Dazzled increases incoming damage in percentage");
            ModSettingsManager.AddOption(new FloatFieldOption(DazzledDamageIncrease));
            DazzledIcon = MainAssets.LoadAsset<Sprite>("Assets/Icons/Dazzled.png");

            Buff();

        }
        private static void Buff()
        {
            DazzledBuffDef = ScriptableObject.CreateInstance<BuffDef>();
            DazzledBuffDef.name = "ciDazzled";
            DazzledBuffDef.buffColor = Color.white;
            DazzledBuffDef.canStack = true;
            DazzledBuffDef.isDebuff = true;
            DazzledBuffDef.ignoreGrowthNectar = false;
            DazzledBuffDef.iconSprite = DazzledIcon;
            DazzledBuffDef.isHidden = false;
            DazzledBuffDef.isCooldown = false;
            ContentAddition.AddBuffDef(DazzledBuffDef);
            On.RoR2.HealthComponent.TakeDamage += TakeDamageMore;
            //GetStatCoefficients += Stats;
            //On.RoR2.CharacterBody.OnBuffFinalStackLost += CharacterBody_OnBuffFinalStackLost;
        }

        private static void TakeDamageMore(On.RoR2.HealthComponent.orig_TakeDamage orig, HealthComponent self, DamageInfo damageInfo)
        {
            int buffCount = self.body ? self.body.GetBuffCount(DazzledBuffDef) : 0;
            if (self.body && damageInfo.attacker && !damageInfo.rejected && buffCount > 0)
            {
                damageInfo.damage *= 1 + (buffCount * DazzledDamageIncrease.Value / 100);
            }
            orig(self, damageInfo);
        }
        /*
private static void Stats(CharacterBody sender, StatHookEventArgs args)
{
   float buffCount = sender.GetBuffCount(DallzedBuffDef);

   if (buffCount > 0)
   {
       args.armorAdd -= buffCount * 50;
   }
}*/
    }
}
