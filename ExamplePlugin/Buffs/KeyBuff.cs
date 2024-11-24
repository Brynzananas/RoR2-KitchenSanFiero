using CaeliImperium.Items;
using R2API;
using System;
using System.Collections.Generic;
using System.Text;
using static R2API.RecalculateStatsAPI;
using UnityEngine;
using RoR2;
using static ReignFromGreatBeyondPlugin.CaeliImperium;

namespace CaeliImperium.Buffs
{
    internal static class KeyBuff
    {
        public static BuffDef KeyBuffDef;
        internal static Sprite KeyIcon;
        
        internal static void Init()
        {


            KeyIcon = MainAssets.LoadAsset<Sprite>("Assets/Icons/Keybuff.png");

            Buff();

        }
        private static void Buff()
        {
            KeyBuffDef = ScriptableObject.CreateInstance<BuffDef>();
            KeyBuffDef.name = "ciKeyBuff";
            KeyBuffDef.buffColor = Color.white;
            KeyBuffDef.canStack = true;
            KeyBuffDef.isDebuff = false;
            KeyBuffDef.ignoreGrowthNectar = false;
            KeyBuffDef.iconSprite = KeyIcon;
            KeyBuffDef.isHidden = false;
            KeyBuffDef.isCooldown = false;
            ContentAddition.AddBuffDef(KeyBuffDef);
            GetStatCoefficients += Stats;
            //On.RoR2.CharacterBody.OnBuffFinalStackLost += CharacterBody_OnBuffFinalStackLost;
        }

        private static void Stats(CharacterBody sender, StatHookEventArgs args)
        {
            float buffCount = sender.GetBuffCount(KeyBuffDef);
                int itemCount = sender.inventory ? sender.inventory.GetItemCount(Keychain.KeychainItemDef) : 0;

            if (buffCount > 0)
            {
                args.critDamageMultAdd +=( Keychain.keychainCritDamagePerBuff.Value / 100)  * buffCount;
                args.critAdd += Keychain.keychainCritChancePerBuff.Value * buffCount;
            }
        }
    }
}
