using CaeliImperium.Items;
using R2API;
using System;
using System.Collections.Generic;
using System.Text;
using static R2API.RecalculateStatsAPI;
using UnityEngine;
using RoR2;
using static CaeliImperiumPlugin.CaeliImperium;
using System.Runtime.CompilerServices;

namespace CaeliImperium.Buffs
{
    internal static class KillswitchBuff

    {
        public static BuffDef KillSwitchBuffDef;
        internal static Sprite KillSwitchIcon;

        internal static void Init()
        {


            KillSwitchIcon = MainAssets.LoadAsset<Sprite>("Assets/Icons/Killswitchbuff.png");

            Buff();

        }
        private static void Buff()
        {
            KillSwitchBuffDef = ScriptableObject.CreateInstance<BuffDef>();
            KillSwitchBuffDef.name = "ciKillSwitch";
            KillSwitchBuffDef.buffColor = Color.white;
            KillSwitchBuffDef.canStack = true;
            KillSwitchBuffDef.isDebuff = false;
            KillSwitchBuffDef.ignoreGrowthNectar = true;
            KillSwitchBuffDef.iconSprite = KillSwitchIcon;
            KillSwitchBuffDef.isHidden = false;
            KillSwitchBuffDef.isCooldown = false;
            ContentAddition.AddBuffDef(KillSwitchBuffDef);
            //On.RoR2.CharacterBody.OnClientBuffsChanged += Killswitch;
            //On.RoR2.CharacterBody.OnBuffFinalStackLost += Killswitch2;
            //On.RoR2.CharacterBody.OnBuffFinalStackLost += CharacterBody_OnBuffFinalStackLost;
        }
        /*
        private static void Killswitch(On.RoR2.CharacterBody.orig_OnClientBuffsChanged orig, CharacterBody self)
        {
            orig(self);
            if (self && self.healthComponent.alive && self.GetBuffCount(KillSwitchBuffDef) >= 10 && MajesticHand.MajesticHandFunction.Value)
            {
                self.healthComponent.Suicide();
            }
        }

        private static void Killswitch2(On.RoR2.CharacterBody.orig_OnBuffFinalStackLost orig, CharacterBody self, BuffDef buffDef)
        {
            orig(self, buffDef);
            if (buffDef == KillSwitchBuffDef && !MajesticHand.MajesticHandFunction.Value)
            {
                if (self && self.healthComponent.alive)
                {
                    self.healthComponent.Suicide();
                }
            }
        }*/
        
    }
}
