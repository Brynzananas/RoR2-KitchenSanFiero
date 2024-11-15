using R2API;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine.AddressableAssets;
using UnityEngine;
using static KitchenSanFieroPlugin.KitchenSanFiero;
using System.ComponentModel;
using KitchenSanFiero.Buffs;
using UnityEngine.Diagnostics;
using BepInEx.Configuration;
using RiskOfOptions.OptionConfigs;
using RiskOfOptions.Options;
using RiskOfOptions;

namespace KitchenSanFiero.Items
{
    internal static class FragileGiftBox //: ItemBase<FirstItem>
    {
        internal static GameObject FragileGiftBoxPrefab;
        internal static Sprite FragileGiftBoxIcon;
        public static ItemDef FragileGiftBoxItemDef;
        public static ConfigEntry<bool> FragileGiftBoxEnable;
        public static ConfigEntry<bool> FragileGiftBoxAIBlacklist;
        //public static ConfigEntry<float> FragileGiftBoxCurseMultiplier;
        //public static ConfigEntry<int> FragileGiftBoxCurseCleanseNextStage;
        public static ConfigEntry<float> FragileGiftBoxRewardChance;
        public static ConfigEntry<float> FragileGiftBoxCurseChance;
        public static ConfigEntry<bool> FragileGiftBoxCurseIsElse;

        internal static void Init()
        {
            AddConfigs();

            FragileGiftBoxPrefab = MainAssets.LoadAsset<GameObject>("Assets/Models/Prefabs/FragileGiftBox.prefab");
            FragileGiftBoxIcon = MainAssets.LoadAsset<Sprite>("Assets/Icons/FragileGiftBoxIcon.png");
            if (!FragileGiftBoxEnable.Value)
            {
                return;
            }
            Item();

            AddLanguageTokens();
        }

        public static void AddConfigs()
        {
            FragileGiftBoxEnable = Config.Bind<bool>("Item : Fragile Gift Box",
                             "Activation",
                             true,
                             "Enable Fragile Gift Box item?");
            FragileGiftBoxAIBlacklist = Config.Bind<bool>("Item : Fragile Gift Box",
                                         "AI Blacklist",
                                         true,
                                         "Blacklist this item from enemies?");
            /*FragileGiftBoxCurseMultiplier = Config.Bind<float>("Item : Fragile Gift Box",
                                         "Curse multiplier",
                                         1f,
                                         "Control curse multiplier");
            FragileGiftBoxCurseCleanseNextStage = Config.Bind<int>("Item : Fragile Gift Box",
                                         "Curse cleanse",
                                         7,
                                         "How much curse is cleansed upon next stage?\nSet -1 to cleanse all");*/
            FragileGiftBoxRewardChance = Config.Bind<float>("Item : Fragile Gift Box",
                                         "Reward chance",
                                         50f,
                                         "Control the chance of getting utems from chests");
            FragileGiftBoxCurseChance = Config.Bind<float>("Item : Fragile Gift Box",
                                         "Curse chance",
                                         100f,
                                         "Control the chance of getting a curse from opening chests");
            FragileGiftBoxCurseIsElse = Config.Bind<bool>("Item : Fragile Gift Box",
                                         "Curse on failure",
                                         false,
                                         "Set true to get curse on reward roll failure");
            ModSettingsManager.AddOption(new CheckBoxOption(FragileGiftBoxEnable, new CheckBoxConfig() { restartRequired = true }));
            ModSettingsManager.AddOption(new CheckBoxOption(FragileGiftBoxAIBlacklist, new CheckBoxConfig() { restartRequired = true }));
            //ModSettingsManager.AddOption(new FloatFieldOption(FragileGiftBoxCurseMultiplier));
            //ModSettingsManager.AddOption(new IntFieldOption(FragileGiftBoxCurseCleanseNextStage));
            ModSettingsManager.AddOption(new FloatFieldOption(FragileGiftBoxRewardChance));
            ModSettingsManager.AddOption(new FloatFieldOption(FragileGiftBoxCurseChance));
            ModSettingsManager.AddOption(new CheckBoxOption(FragileGiftBoxCurseIsElse));
        }

        private static void Item()
        {
            FragileGiftBoxItemDef = ScriptableObject.CreateInstance<ItemDef>();
            FragileGiftBoxItemDef.name = "FragileGiftBox";
            FragileGiftBoxItemDef.nameToken = "FRAGILEGIFTBOX_NAME";
            FragileGiftBoxItemDef.pickupToken = "FRAGILEGIFTBOX_PICKUP";
            FragileGiftBoxItemDef.descriptionToken = "FRAGILEGIFTBOX_DESC";
            FragileGiftBoxItemDef.loreToken = "FRAGILEGIFTBOX_LORE";
            FragileGiftBoxItemDef.deprecatedTier = ItemTier.Lunar;
            FragileGiftBoxItemDef.pickupIconSprite = FragileGiftBoxIcon;
            FragileGiftBoxItemDef.pickupModelPrefab = FragileGiftBoxPrefab;
            FragileGiftBoxItemDef.canRemove = true;
            FragileGiftBoxItemDef.hidden = false;
            var displayRules = new ItemDisplayRuleDict(null);
            ItemAPI.Add(new CustomItem(FragileGiftBoxItemDef, displayRules));
            On.RoR2.PurchaseInteraction.OnInteractionBegin += OnChestOpen;
        }

        private static void OnChestOpen(On.RoR2.PurchaseInteraction.orig_OnInteractionBegin orig, PurchaseInteraction self, Interactor activator)
        {
            orig(self, activator);
            int itemCount = activator.GetComponent<CharacterBody>().inventory.GetItemCount(FragileGiftBoxItemDef);
            int buffCount = activator.GetComponent<CharacterBody>().GetBuffCount(RoR2Content.Buffs.PermanentCurse);
            //int num2 = 0;
            //float num3 = 1f;
            if (itemCount > 0)
            {
                if (self.GetComponent<ChestBehavior>())
                {
                    if (Util.CheckRoll(FragileGiftBoxRewardChance.Value))
                    {
                    self.GetComponent<ChestBehavior>().dropCount += itemCount;
                    }
                    else
                    {
                        self.GetComponent<ChestBehavior>().dropCount = 0;
                        if (FragileGiftBoxCurseIsElse.Value)
                        {
                            for (var i = 0; i < itemCount; i++)
                            {
                                activator.GetComponent<CharacterBody>().AddBuff(WoundedBuff.WoundedBuffDef);

                            }


                        }
                    }
                    if (Util.CheckRoll(FragileGiftBoxCurseChance.Value))
                    {
                    for (var i = 0; i < itemCount; i++)
                    {
                        activator.GetComponent<CharacterBody>().AddBuff(WoundedBuff.WoundedBuffDef);
                    }
                    }

                }
                if (self.GetComponent<RouletteChestController>())
                {
                    if (Util.CheckRoll(FragileGiftBoxRewardChance.Value))
                    {
                        self.GetComponent<RouletteChestController>().dropCount += itemCount;
                    }
                    else
                    {
                        self.GetComponent<RouletteChestController>().dropCount = 0;
                        if (FragileGiftBoxCurseIsElse.Value)
                        {
                            activator.GetComponent<CharacterBody>().AddBuff(WoundedBuff.WoundedBuffDef);

                        }
                    }

                    if (Util.CheckRoll(FragileGiftBoxCurseChance.Value))
                    {
                        for (var i = 0; i < itemCount; i++)
                        {
                            activator.GetComponent<CharacterBody>().AddBuff(WoundedBuff.WoundedBuffDef);
                        }
                    }
                }

            }
        }

        private static void AddLanguageTokens()
        {
            LanguageAPI.Add("FRAGILEGIFTBOX_NAME", "Fragile Gift Box");
            LanguageAPI.Add("FRAGILEGIFTBOX_PICKUP", "Gain more items from chests. 50% chance to receive all or nothing. Get permanent damage on chest opening");
            LanguageAPI.Add("FRAGILEGIFTBOX_DESC", "Gain more items from chests. 50% chance to receive all or nothing. Get permanent damage on chest opening");
            LanguageAPI.Add("FRAGILEGIFTBOX_LORE", "I never thought you are also a cook, brother");
        }

    }

}