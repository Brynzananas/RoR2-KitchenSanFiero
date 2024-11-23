using R2API;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using static ReignFromGreatBeyondPlugin.CaeliImperium;

namespace CaeliImperium.Items
{
    internal static class UsedEnergyChocolateBar //: ItemBase<FirstItem>
    {
        internal static GameObject UsedEnergyChocolateBarPrefab;
        internal static Sprite UsedEnergyChocolateBarIcon;
        public static ItemDef ConsumedEnergyChocolateBarItemDef;


        internal static void Init()
        {
            UsedEnergyChocolateBarPrefab = MainAssets.LoadAsset<GameObject>("Assets/Models/Prefabs/Item/Energychocolatebar.prefab");
            UsedEnergyChocolateBarIcon = MainAssets.LoadAsset<Sprite>("Assets/Icons/EnergisedChocolateBarIconConsumed.png");

            Item();

            AddLanguageTokens();
        }
        private static void Item()
        {
            ConsumedEnergyChocolateBarItemDef = ScriptableObject.CreateInstance<ItemDef>();
            ConsumedEnergyChocolateBarItemDef.name = "ConsumedEnergisedChocolateBar";
            ConsumedEnergyChocolateBarItemDef.nameToken = "CONSUMEDENERGYCHOCOLATEBAR_NAME";
            ConsumedEnergyChocolateBarItemDef.pickupToken = "CONSUMEDENERGYCHOCOLATEBAR_PICKUP";
            ConsumedEnergyChocolateBarItemDef.descriptionToken = "CONSUMEDENERGYCHOCOLATEBAR_DESC";
            ConsumedEnergyChocolateBarItemDef.loreToken = "CONSUMEDENERGYCHOCOLATEBAR_LORE";
            ConsumedEnergyChocolateBarItemDef.deprecatedTier = ItemTier.NoTier;
            ConsumedEnergyChocolateBarItemDef.pickupIconSprite = UsedEnergyChocolateBarIcon;
            ConsumedEnergyChocolateBarItemDef.pickupModelPrefab = UsedEnergyChocolateBarPrefab;
            ConsumedEnergyChocolateBarItemDef.canRemove = false;
            ConsumedEnergyChocolateBarItemDef.hidden = false;
            ConsumedEnergyChocolateBarItemDef.requiredExpansion = CaeliImperiumExpansionDef;
            var displayRules = new ItemDisplayRuleDict(null);
            ItemAPI.Add(new CustomItem(ConsumedEnergyChocolateBarItemDef, displayRules));
        }
        private static void AddLanguageTokens()
        {
            LanguageAPI.Add("CONSUMEDENERGYCHOCOLATEBAR_NAME", "Consumed Energised Chocolate Bar");
            LanguageAPI.Add("CONSUMEDENERGYCHOCOLATEBAR_PICKUP", "So tasty...");
            LanguageAPI.Add("CONSUMEDENERGYCHOCOLATEBAR_DESC", "So tasty...");
            LanguageAPI.Add("CONSUMEDENERGYCHOCOLATEBAR_LORE", "");
        }
    }
}
