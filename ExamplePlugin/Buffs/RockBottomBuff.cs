using EmotesAPI;
using R2API;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using static CaeliImperiumPlugin.CaeliImperium;
using MonoMod.Cil;
using UnityEngine.AddressableAssets;
using RoR2.Skills;
using CaeliImperium.Items;
namespace CaeliImperium.Buffs
{
    internal static class RockBottomBuff
    {
        public static BuffDef RockBottomBuffDef;
        internal static Sprite RockBottomTimerIcon;
        internal static void Init()
        {


            RockBottomTimerIcon = MainAssets.LoadAsset<Sprite>("Assets/Icons/Killswitchbuff.png");

            Buff();

        }
        private static void Buff()
        {
            RockBottomBuffDef = ScriptableObject.CreateInstance<BuffDef>();
            RockBottomBuffDef.name = "ciRockBottom";
            RockBottomBuffDef.buffColor = Color.black;
            RockBottomBuffDef.canStack = false;
            RockBottomBuffDef.isDebuff = false;
            RockBottomBuffDef.ignoreGrowthNectar = true;
            RockBottomBuffDef.iconSprite = RockBottomTimerIcon;
            RockBottomBuffDef.isHidden = false;
            RockBottomBuffDef.isCooldown = false;
            ContentAddition.AddBuffDef(RockBottomBuffDef);
            On.RoR2.HealthComponent.TakeDamageProcess += Round;
            On.RoR2.CharacterBody.HandleOnKillEffectsServer += Over;
        }

        private static void Over(On.RoR2.CharacterBody.orig_HandleOnKillEffectsServer orig, CharacterBody self, DamageReport damageReport)
        {
            if (self && damageReport.attacker)
            {
                var attackerBody = damageReport.attacker ? damageReport.attacker.GetComponent<CharacterBody>() : null;
                bool victimHasBuff = self.HasBuff(RockBottomBuffDef);
                bool attackerHasBuff = attackerBody ? attackerBody.HasBuff(RockBottomBuffDef) : false;
                if (victimHasBuff && attackerHasBuff)
                {
                    self.RemoveBuff(RockBottomBuffDef);
                    attackerBody.RemoveBuff(RockBottomBuffDef);
                }
            }
            orig(self, damageReport);

        }

        private static void Round(On.RoR2.HealthComponent.orig_TakeDamageProcess orig, HealthComponent self, DamageInfo damageInfo)
        {
            if (damageInfo != null && self != null && damageInfo.attacker && self.body && !damageInfo.rejected)
            {
                var attackerBody = damageInfo.attacker ? damageInfo.attacker.GetComponent<CharacterBody>() : null;
                bool victimHasBuff = self.body.HasBuff(RockBottomBuffDef);
                bool attackerHasBuff = attackerBody ? attackerBody.HasBuff(RockBottomBuffDef) : false;
                if (victimHasBuff && attackerHasBuff)
                {
                    damageInfo.damage *= 2;
                }
                else if (victimHasBuff || attackerHasBuff)
                {
                    damageInfo.rejected = true;
                }
            }
            orig(self, damageInfo); 
        }
    }
}
