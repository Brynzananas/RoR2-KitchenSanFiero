using CaeliImperium.Items;
using R2API;
using RiskOfOptions.Options;
using RiskOfOptions;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.AddressableAssets;
using static CaeliImperiumPlugin.CaeliImperium;
using static R2API.RecalculateStatsAPI;
using BepInEx.Configuration;

namespace CaeliImperium.Buffs
{
    internal static class WoundedBuff
    {
        public static BuffDef WoundedBuffDef;
        internal static Sprite WoundedIcon;
        public static ConfigEntry<float> WoundedCurse;
        //public static ConfigEntry<int> WoundedCleanse;

        internal static void Init()
        {

            AddConfigs();
            WoundedIcon = MainAssets.LoadAsset<Sprite>("Assets/Icons/Wounded.png");

            Buff();

        }
        private static void AddConfigs()
        {
            WoundedCurse = Config.Bind<float>("Buff : Wounded",
                             "Damage increase",
                             10f,
                             "Control how much Wounded increases incoming damage in percentage per buff stack");
            /*WoundedCleanse = Config.Bind<int>("Buff : Wounded",
                             "Cleanse",
                             7,
                             "Control how much Wounded is cleansing per stage.\nSet it to -1 to cleanse all");*/
            ModSettingsManager.AddOption(new FloatFieldOption(WoundedCurse));
            //ModSettingsManager.AddOption(new IntFieldOption(WoundedCleanse));
        }
        private static void Buff()
        {
            WoundedBuffDef = ScriptableObject.CreateInstance<BuffDef>();
            WoundedBuffDef.name = "ciWounded";
            WoundedBuffDef.buffColor = Color.white;
            WoundedBuffDef.canStack = true;
            WoundedBuffDef.isDebuff = false;
            WoundedBuffDef.ignoreGrowthNectar = true;
            WoundedBuffDef.iconSprite = WoundedIcon;
            WoundedBuffDef.isHidden = false;
            ContentAddition.AddBuffDef(WoundedBuffDef);
            On.RoR2.HealthComponent.TakeDamage += IncreaseDamage;
            //GetStatCoefficients += Stats;
            //On.RoR2.CharacterMaster.OnServerStageBegin += StageStart;
        }

        private static void IncreaseDamage(On.RoR2.HealthComponent.orig_TakeDamage orig, HealthComponent self, DamageInfo damageInfo)
        {
            int buffCount = self.body ? self.body.GetBuffCount(WoundedBuffDef) : 0;
            if (self.body && damageInfo.attacker && !damageInfo.rejected && buffCount > 0)
            {
                damageInfo.damage *= 1 + (buffCount * WoundedCurse.Value / 100);
            }
            orig(self, damageInfo);
        }
        /*
private static void StageStart(On.RoR2.CharacterMaster.orig_OnServerStageBegin orig, CharacterMaster self, Stage stage)
{
   int buffCount = self.GetBody() ? self.GetBody().GetBuffCount(ShatteredHopeBuffDef) : 0;

   orig(self, stage);

   if (buffCount > 0)
   {
       if (WoundedCleanse.Value >= 0)
       {
               self.GetBody().SetBuffCount(ShatteredHopeBuffDef.buffIndex, buffCount - WoundedCleanse.Value);

       }

   }
}
*/
        /*
        private static void Stats(CharacterBody sender, StatHookEventArgs args)
        {
            float buffCount = sender.GetBuffCount(ShatteredHopeBuffDef);
            if (buffCount > 0)
            {
                args.baseCurseAdd += buffCount * (WoundedCurse.Value);
            }
        }*/
    }
}
