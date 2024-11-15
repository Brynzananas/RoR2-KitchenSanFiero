using R2API;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using static KitchenSanFieroPlugin.KitchenSanFiero;

namespace KitchenSanFiero.Items
{
    internal static class UsedEnergyChocolateBar //: ItemBase<FirstItem>
    {
        internal static GameObject UsedEnergyChocolateBarPrefab;
        internal static Sprite UsedEnergyChocolateBarIcon;
        public static ItemDef UsedEnergyChocolateBarItemDef;


        internal static void Init()
        {
            UsedEnergyChocolateBarPrefab = MainAssets.LoadAsset<GameObject>("Assets/Models/Prefabs/Item/Energychocolatebar.prefab");
            UsedEnergyChocolateBarIcon = MainAssets.LoadAsset<Sprite>("Assets/Icons/EnergisedChocolateBarIcon.png");

            Item();

            AddLanguageTokens();
        }
        private static void Item()
        {
            UsedEnergyChocolateBarItemDef = ScriptableObject.CreateInstance<ItemDef>();
            UsedEnergyChocolateBarItemDef.name = "UsedEnergisedChocolateBar";
            UsedEnergyChocolateBarItemDef.nameToken = "USEDENERGYCHOCOLATEBAR_NAME";
            UsedEnergyChocolateBarItemDef.pickupToken = "USEDENERGYCHOCOLATEBAR_PICKUP";
            UsedEnergyChocolateBarItemDef.descriptionToken = "USEDENERGYCHOCOLATEBAR_DESC";
            UsedEnergyChocolateBarItemDef.loreToken = "USEDENERGYCHOCOLATEBAR_LORE";
            UsedEnergyChocolateBarItemDef.deprecatedTier = ItemTier.NoTier;
            UsedEnergyChocolateBarItemDef.pickupIconSprite = UsedEnergyChocolateBarIcon;
            UsedEnergyChocolateBarItemDef.pickupModelPrefab = UsedEnergyChocolateBarPrefab;
            UsedEnergyChocolateBarItemDef.canRemove = false;
            UsedEnergyChocolateBarItemDef.hidden = false;
            var displayRules = new ItemDisplayRuleDict(null);
            ItemAPI.Add(new CustomItem(UsedEnergyChocolateBarItemDef, displayRules));
        }
        private static void AddLanguageTokens()
        {
            LanguageAPI.Add("USEDENERGYCHOCOLATEBAR_NAME", "Consumed Energised Chocolate Bar");
            LanguageAPI.Add("USEDENERGYCHOCOLATEBAR_PICKUP", "So tasty...");
            LanguageAPI.Add("USEDENERGYCHOCOLATEBAR_DESC", "So tasty...");
            LanguageAPI.Add("USEDENERGYCHOCOLATEBAR_LORE", "");
        }
    }
}
