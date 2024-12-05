using R2API;
using RoR2;
using static R2API.RecalculateStatsAPI;
using UnityEngine.AddressableAssets;
using UnityEngine;
using static CaeliImperiumPlugin.CaeliImperium;
using static CaeliImperium.Items.FragileGiftBox;
using BepInEx.Configuration;
using System;
using RiskOfOptions.OptionConfigs;
using RiskOfOptions.Options;
using RiskOfOptions;
using System.Collections.Generic;

namespace CaeliImperium.Items
{
    internal static class DeepWound
    {
        internal static GameObject DeepWoundPrefab;
        internal static Sprite DeepWoundBarIcon;
        public static ItemDef DeepWoundItemDef;

        internal static void Init()
        {
            AddConfigs();
            string tier = "Assets/Icons/EnergisedChocolateBarIcon.png";
            DeepWoundPrefab = MainAssets.LoadAsset<GameObject>("Assets/Models/Prefabs/Energychocolatebar.prefab");
            DeepWoundBarIcon = MainAssets.LoadAsset<Sprite>(tier);
            Item();

            AddLanguageTokens();
            UsedEnergyChocolateBar.Init();
        }

        private static void Item()
        {
            DeepWoundItemDef = ScriptableObject.CreateInstance<ItemDef>();
            DeepWoundItemDef.name = "DeepWound";
            DeepWoundItemDef.nameToken = "DEEPWOUND_NAME";
            DeepWoundItemDef.pickupToken = "DEEPWOUND_PICKUP";
            DeepWoundItemDef.descriptionToken = "DEEPWOUND_DESC";
            DeepWoundItemDef.loreToken = "DEEPWOUND_LORE";
            DeepWoundItemDef.deprecatedTier = ItemTier.NoTier;
            DeepWoundItemDef.pickupIconSprite = DeepWoundBarIcon;
            DeepWoundItemDef.pickupModelPrefab = DeepWoundPrefab;
            DeepWoundItemDef.canRemove = false;
            DeepWoundItemDef.hidden = false;
            DeepWoundItemDef.requiredExpansion = CaeliImperiumExpansionDef;
            var tags = new List<ItemTag>() {ItemTag.CannotSteal};
            DeepWoundItemDef.tags = tags.ToArray();
            var displayRules = new ItemDisplayRuleDict(null);
            ItemAPI.Add(new CustomItem(DeepWoundItemDef, displayRules));
            On.RoR2.CharacterMaster.OnServerStageBegin += StageStart;
        }
        private static void IncreaseDamage(On.RoR2.HealthComponent.orig_TakeDamage orig, HealthComponent self, DamageInfo damageInfo)
        {
            int buffCount = self.body.inventory ? self.body.inventory.GetItemCount(DeepWoundItemDef) : 0;
            if (self.body && damageInfo.attacker && !damageInfo.rejected && buffCount > 0)
            {
                damageInfo.damage *= 1 + ((ConfigFloat(FragileGiftBoxWoundDamage, FragileGiftBoxEnableConfig) + ((buffCount - 1) * ConfigFloat(FragileGiftBoxWoundDamageStack, FragileGiftBoxEnableConfig)) / 100));
            }
            orig(self, damageInfo);
        }
        private static void StageStart(On.RoR2.CharacterMaster.orig_OnServerStageBegin orig, CharacterMaster self, Stage stage)
        {
            orig(self, stage);
            int itemCount = self.inventory ? self.inventory.GetItemCount(DeepWoundItemDef) : 0;
            if (itemCount > 0)

            {
                int itemsToConsume = 0;
                //float luckDown = self.luck;
                //if (!EnergyChocolateBarsLuck.Value)
                //{
                //    luckDown = 0;
                //}
                for (int i = 0; i < itemCount; i++)
                {
                    //Debug.Log(Util.CheckRoll(EnergyChocolateStageConsumeChance.Value, -luckDown));
                    if (Util.CheckRoll(ConfigFloat(FragileGiftBoxWoundClearChance, FragileGiftBoxEnableConfig)))
                    {
                        itemsToConsume++;
                    }
                }
                //Debug.Log(itemsToConsume);
                if (itemsToConsume > 0)
                {
                    self.inventory.RemoveItem(DeepWoundItemDef, itemsToConsume);
                }


            }
        }

        private static void AddLanguageTokens()
        {
            LanguageAPI.Add("DEEPWOUND_NAME", "Deep Wound");
            LanguageAPI.Add("DEEPWOUND_PICKUP", "");
            LanguageAPI.Add("DEEPWOUND_DESC", "");
            LanguageAPI.Add("DEEPWOUND_LORE", "");
        }
    }
}
