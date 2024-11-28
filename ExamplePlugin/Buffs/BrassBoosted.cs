using CaeliImperium.Items;
using R2API;
using RoR2;
using RoR2.Audio;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using static CaeliImperiumPlugin.CaeliImperium;
using static R2API.RecalculateStatsAPI;

namespace CaeliImperium.Buffs
{
    internal static class BrassBoostedBuff
    {
        public static BuffDef BrassBoostedBuffDef;
        internal static Sprite BrassBoostedIcon;
        public static Color newColor = new Color(0.56f, 0.4f, 0f, 1f);
        internal static void Init()
        {


            BrassBoostedIcon = MainAssets.LoadAsset<Sprite>("Assets/Icons/BellBoosted.png");

            Buff();

        }
        private static void Buff()
        {
            BrassBoostedBuffDef = ScriptableObject.CreateInstance<BuffDef>();
            BrassBoostedBuffDef.name = "ciBellBoosted";
            BrassBoostedBuffDef.buffColor = Color.white;
            BrassBoostedBuffDef.canStack = false;
            BrassBoostedBuffDef.isDebuff = false;
            BrassBoostedBuffDef.ignoreGrowthNectar = false;
            BrassBoostedBuffDef.iconSprite = BrassBoostedIcon;
            BrassBoostedBuffDef.isHidden = false;
            BrassBoostedBuffDef.isCooldown = false;
            ContentAddition.AddBuffDef(BrassBoostedBuffDef);
            GetStatCoefficients += Stats;
            On.RoR2.CharacterBody.OnBuffFirstStackGained += CharacterBody_OnBuffFinalStackLost;
        }

        private static void Stats(CharacterBody sender, StatHookEventArgs args)
        {
            float buffCount = sender.GetBuffCount(BrassBoostedBuffDef);
            if (buffCount > 0)
            {
                int itemCount = sender.inventory ? sender.inventory.GetItemCount(BrassBell.BrassBellItemDef) : 0;
                args.damageMultAdd += (BrassBell.BrassBellDamageIncrease.Value / 100) + ((itemCount - 1) * BrassBell.BrassBellDamageIncreaseStack.Value / 100);
            }
        }
        
        private static void CharacterBody_OnBuffFinalStackLost(On.RoR2.CharacterBody.orig_OnBuffFirstStackGained orig, CharacterBody self, BuffDef buffDef)
        {
            orig(self, buffDef);
            if (buffDef == BrassBoostedBuffDef)
            {
                if (self.skillLocator.secondary.stock <= 0 && BrassBell.BrassBellIsReloadSecondary.Value)
                {
                self.skillLocator.secondary.AddOneStock();

                }
                if (self.skillLocator.utility.stock <= 0 && BrassBell.BrassBellIsReloadutility.Value)
                {
                    self.skillLocator.utility.AddOneStock();

                }
                if (self.skillLocator.special.stock <= 0 && BrassBell.BrassBellIsReloadSpecial.Value)
                {
                    self.skillLocator.special.AddOneStock();

                }
                if (BrassBell.BrassBellIsReloadSound.Value)
                {
                    EntitySoundManager.EmitSoundServer(BrassBell.BellSound.akId, self.gameObject);

                    //CreateSound();
                    //Util.PlaySound("item_proc_bandolierPickup_01", self.gameObject);
                }
                //self.AddTimedBuff(BrassTimerBuff.BrassTimerBuffDef, BrassBell.BrassBellCooldown.Value);
            }
        }
    }
}
