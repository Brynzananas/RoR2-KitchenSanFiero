using R2API;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using static ReignFromGreatBeyondPlugin.CaeliImperium;

namespace CaeliImperium.Buffs
{
    internal static class TalismanCooldownBuff
    {
        public static BuffDef TalismanCooldownBuffDef;
        internal static Sprite TalismanCooldownIcon;
        internal static void Init()
        {


            TalismanCooldownIcon = MainAssets.LoadAsset<Sprite>("Assets/Icons/TalismanReady.png");

            Buff();

        }
        private static void Buff()
        {
            TalismanCooldownBuffDef = ScriptableObject.CreateInstance<BuffDef>();
            TalismanCooldownBuffDef.name = "ciTalismanCooldwon";
            TalismanCooldownBuffDef.buffColor = Color.grey;
            TalismanCooldownBuffDef.canStack = true;
            TalismanCooldownBuffDef.isDebuff = true;
            TalismanCooldownBuffDef.ignoreGrowthNectar = true;
            TalismanCooldownBuffDef.iconSprite = TalismanCooldownIcon;
            TalismanCooldownBuffDef.isHidden = false;
            TalismanCooldownBuffDef.isCooldown = false;
            ContentAddition.AddBuffDef(TalismanCooldownBuffDef);
            //On.RoR2.CharacterBody.OnBuffFinalStackLost += CharacterBody_OnBuffFinalStackLost;
        }
        /*
        private static void CharacterBody_OnBuffFinalStackLost(On.RoR2.CharacterBody.orig_OnBuffFinalStackLost orig, CharacterBody self, BuffDef buffDef)
        {
            orig(self, buffDef);
            if (buffDef == TalismanCooldownBuffDef)
            {
                self.AddBuff(TalismanAttackerBuff.TalismanAttackerBuffDef);
            }
        }*/
    }
}
