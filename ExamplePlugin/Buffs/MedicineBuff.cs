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
    internal static class MedicineBuff
    {
        public static BuffDef MedicineBuffDef;
        internal static Sprite BrassBoostedIcon;
        public static Color newColor = new Color(0.56f, 0.4f, 0f, 1f);
        internal static void Init()
        {


            BrassBoostedIcon = MainAssets.LoadAsset<Sprite>("Assets/Icons/BellBoosted.png");

            Buff();

        }
        private static void Buff()
        {
            MedicineBuffDef = ScriptableObject.CreateInstance<BuffDef>();
            MedicineBuffDef.name = "ciMedicine";
            MedicineBuffDef.buffColor = Color.white;
            MedicineBuffDef.canStack = false;
            MedicineBuffDef.isDebuff = false;
            MedicineBuffDef.ignoreGrowthNectar = false;
            MedicineBuffDef.iconSprite = BrassBoostedIcon;
            MedicineBuffDef.isHidden = false;
            MedicineBuffDef.isCooldown = false;
            ContentAddition.AddBuffDef(MedicineBuffDef);
            //GetStatCoefficients += Stats;
        }

        //private static void Stats(CharacterBody sender, StatHookEventArgs args)
        //{
        //    float buffCount = sender.GetBuffCount(MedicineBuffDef);
        //    if (buffCount > 0)
        //    {
        //        //int itemCount = sender.inventory ? sender.inventory.GetItemCount(Medicine.MedicineItemDef) : 0;
        //        args.baseRegenAdd += sender.maxHealth / 10;
        //    }
        //}
    }
}
