using R2API;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine.AddressableAssets;
using UnityEngine;
using static CaeliImperiumPlugin.CaeliImperium;
using System.ComponentModel;
using CaeliImperium.Buffs;
using UnityEngine.Diagnostics;
using BepInEx.Configuration;
using RiskOfOptions.OptionConfigs;
using RiskOfOptions.Options;
using RiskOfOptions;

namespace CaeliImperium.Items
{
    internal static class FragileGiftBox //: ItemBase<FirstItem>
    {
        internal static GameObject FragileGiftBoxPrefab;
        internal static Sprite FragileGiftBoxIcon;
        public static ItemDef FragileGiftBoxItemDef;
        public static ConfigEntry<bool> FragileGiftBoxEnable;
        public static ConfigEntry<bool> FragileGiftBoxEnableConfig;
        public static ConfigEntry<bool> FragileGiftBoxAIBlacklist;
        //public static ConfigEntry<float> FragileGiftBoxCurseMultiplier;
        //public static ConfigEntry<int> FragileGiftBoxCurseCleanseNextStage;
        public static ConfigEntry<float> FragileGiftBoxRewardChance;
        public static ConfigEntry<int> FragileGiftBoxReward;
        public static ConfigEntry<int> FragileGiftBoxRewardPerStack;
        public static ConfigEntry<float> FragileGiftBoxCurseChance;
        public static ConfigEntry<bool> FragileGiftBoxCurseIsElse;
        public static ConfigEntry<int> FragileGiftBoxWound;
        public static ConfigEntry<int> FragileGiftBoxWoundStack;
        public static ConfigEntry<float> FragileGiftBoxWoundDamage;
        public static ConfigEntry<float> FragileGiftBoxWoundDamageStack;
        public static ConfigEntry<float> FragileGiftBoxWoundClearChance;

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
            DeepWound.Init();
            AddLanguageTokens();
        }

        public static void AddConfigs()
        {
            FragileGiftBoxEnable = Config.Bind<bool>("Item : Fragile Gift Box",
                             "Activation",
                             true,
                             "Enable Fragile Gift Box item?");
            FragileGiftBoxEnableConfig = Config.Bind<bool>("Item : Fragile Gift Box",
                             "Activation",
                             false,
                             "Enable config?");
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
                                         "Control the chance of getting items from chests");
            FragileGiftBoxReward = Config.Bind<int>("Item : Fragile Gift Box",
                                         "Items addition",
                                         2,
                                         "Control the amount of additional items");
            FragileGiftBoxRewardPerStack = Config.Bind<int>("Item : Fragile Gift Box",
                                         "Items addition stack",
                                         1,
                                         "Control the amount of additional items per item stack");
            FragileGiftBoxCurseChance = Config.Bind<float>("Item : Fragile Gift Box",
                                         "Wound chance",
                                         100f,
                                         "Control the chance of getting a Deep Wound from opening chests");
            FragileGiftBoxWound = Config.Bind<int>("Item : Fragile Gift Box",
                                         "Wound",
                                         2,
                                         "Control the amount of Wound");
            FragileGiftBoxWoundStack = Config.Bind<int>("Item : Fragile Gift Box",
                                         "Wound stack",
                                         1,
                                         "Control the amount of Wound per item stack");
            FragileGiftBoxCurseIsElse = Config.Bind<bool>("Item : Fragile Gift Box",
                                         "Wound on failure",
                                         false,
                                         "Would reward failure give Deep Wounded?");
            FragileGiftBoxWoundDamage = Config.Bind<float>("Item : Fragile Gift Box",
                                         "Wound damage",
                                         10f,
                                         "Control the incoming damage increase of Deep Wound");
            FragileGiftBoxWoundDamageStack = Config.Bind<float>("Item : Fragile Gift Box",
                                         "Wound damage stack",
                                         10f,
                                         "Control the incoming damage increase of Deep Wound per Deep Wound stack");
            FragileGiftBoxWoundClearChance = Config.Bind<float>("Item : Fragile Gift Box",
                                         "Wound clear chance",
                                         35f,
                                         "Control the chance of clearing Deep Wound in percentage");
            ModSettingsManager.AddOption(new CheckBoxOption(FragileGiftBoxEnable, new CheckBoxConfig() { restartRequired = true }));
            ModSettingsManager.AddOption(new CheckBoxOption(FragileGiftBoxAIBlacklist, new CheckBoxConfig() { restartRequired = true }));
            //ModSettingsManager.AddOption(new FloatFieldOption(FragileGiftBoxCurseMultiplier));
            //ModSettingsManager.AddOption(new IntFieldOption(FragileGiftBoxCurseCleanseNextStage));
            ModSettingsManager.AddOption(new IntFieldOption(FragileGiftBoxReward));
            ModSettingsManager.AddOption(new IntFieldOption(FragileGiftBoxRewardPerStack));
            ModSettingsManager.AddOption(new FloatFieldOption(FragileGiftBoxRewardChance));
            ModSettingsManager.AddOption(new FloatFieldOption(FragileGiftBoxCurseChance));
            ModSettingsManager.AddOption(new IntFieldOption(FragileGiftBoxWound));
            ModSettingsManager.AddOption(new IntFieldOption(FragileGiftBoxWoundStack));
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
            FragileGiftBoxItemDef.requiredExpansion = CaeliImperiumExpansionDef;
            var displayRules = new ItemDisplayRuleDict(null);
            ItemAPI.Add(new CustomItem(FragileGiftBoxItemDef, displayRules));
            On.RoR2.PurchaseInteraction.OnInteractionBegin += OnChestOpen;
        }

        private static void OnChestOpen(On.RoR2.PurchaseInteraction.orig_OnInteractionBegin orig, PurchaseInteraction self, Interactor activator)
        {
            orig(self, activator);
            int itemCount = activator.GetComponent<CharacterBody>() ? activator.GetComponent<CharacterBody>().inventory.GetItemCount(FragileGiftBoxItemDef) : 0;
            if (itemCount > 0)
            {
                var activatorBody = activator.GetComponent<CharacterBody>().inventory;
                if (self.GetComponent<ChestBehavior>())
                {
                    if (Util.CheckRoll(ConfigFloat(FragileGiftBoxRewardChance, FragileGiftBoxEnableConfig)))
                    {
                    self.GetComponent<ChestBehavior>().dropCount += ConfigInt(FragileGiftBoxReward, FragileGiftBoxEnableConfig) + ((itemCount - 1) * ConfigInt(FragileGiftBoxRewardPerStack, FragileGiftBoxEnableConfig));
                    }
                    else
                    {
                        self.GetComponent<ChestBehavior>().dropCount = 0;
                        if (ConfigBool(FragileGiftBoxCurseIsElse, FragileGiftBoxEnableConfig))
                        {
                            activatorBody.GiveItem(DeepWound.DeepWoundItemDef, ConfigInt(FragileGiftBoxWound, FragileGiftBoxEnableConfig) + ((itemCount - 1) * ConfigInt(FragileGiftBoxWoundStack, FragileGiftBoxEnableConfig)));
                            //for (var i = 0; i < ConfigInt(FragileGiftBoxWound, FragileGiftBoxEnableConfig) + ((itemCount - 1) * ConfigInt(FragileGiftBoxWoundStack, FragileGiftBoxEnableConfig)); i++)
                            //{
                            //    activator.GetComponent<CharacterBody>().AddBuff(WoundedBuff.WoundedBuffDef);
                            //}


                        }
                    }
                    if (Util.CheckRoll(ConfigFloat(FragileGiftBoxCurseChance, FragileGiftBoxEnableConfig)))
                    {
                        activatorBody.GiveItem(DeepWound.DeepWoundItemDef, ConfigInt(FragileGiftBoxWound, FragileGiftBoxEnableConfig) + ((itemCount - 1) * ConfigInt(FragileGiftBoxWoundStack, FragileGiftBoxEnableConfig)));
                        //for (var i = 0; i < ConfigInt(FragileGiftBoxWound, FragileGiftBoxEnableConfig) + ((itemCount - 1) * ConfigInt(FragileGiftBoxWoundStack, FragileGiftBoxEnableConfig)); i++)
                        //{
                        //    activator.GetComponent<CharacterBody>().AddBuff(WoundedBuff.WoundedBuffDef);
                        //}
                    }

                }
                if (self.GetComponent<RouletteChestController>())
                {
                    if (Util.CheckRoll(ConfigFloat(FragileGiftBoxRewardChance, FragileGiftBoxEnableConfig)))
                    {
                        self.GetComponent<RouletteChestController>().dropCount += ConfigInt(FragileGiftBoxReward, FragileGiftBoxEnableConfig) + ((itemCount - 1) * ConfigInt(FragileGiftBoxRewardPerStack, FragileGiftBoxEnableConfig));
                    }
                    else
                    {
                        self.GetComponent<RouletteChestController>().dropCount = 0;
                        if (ConfigBool(FragileGiftBoxCurseIsElse, FragileGiftBoxEnableConfig))
                        {
                            activatorBody.GiveItem(DeepWound.DeepWoundItemDef, ConfigInt(FragileGiftBoxWound, FragileGiftBoxEnableConfig) + ((itemCount - 1) * ConfigInt(FragileGiftBoxWoundStack, FragileGiftBoxEnableConfig)));
                            //for (var i = 0; i < ConfigInt(FragileGiftBoxWound, FragileGiftBoxEnableConfig) + ((itemCount - 1) * ConfigInt(FragileGiftBoxWoundStack, FragileGiftBoxEnableConfig)); i++)
                            //{
                            //    activator.GetComponent<CharacterBody>().AddBuff(WoundedBuff.WoundedBuffDef);
                            //}

                        }
                    }

                    if (Util.CheckRoll(ConfigFloat(FragileGiftBoxCurseChance, FragileGiftBoxEnableConfig)))
                    {
                        activatorBody.GiveItem(DeepWound.DeepWoundItemDef, ConfigInt(FragileGiftBoxWound, FragileGiftBoxEnableConfig) + ((itemCount - 1) * ConfigInt(FragileGiftBoxWoundStack, FragileGiftBoxEnableConfig)));
                        //for (var i = 0; i < ConfigInt(FragileGiftBoxWound, FragileGiftBoxEnableConfig) + ((itemCount - 1) * ConfigInt(FragileGiftBoxWoundStack, FragileGiftBoxEnableConfig)); i++)
                        //{
                        //    activator.GetComponent<CharacterBody>().AddBuff(WoundedBuff.WoundedBuffDef);
                        //}
                    }
                }

            }
        }

        private static void AddLanguageTokens()
        {
            string rewardStack = "";
            if (ConfigInt(FragileGiftBoxRewardPerStack, FragileGiftBoxEnableConfig) != 0)
            {
                rewardStack = " <style=cStack>(+" + ConfigInt(FragileGiftBoxRewardPerStack, FragileGiftBoxEnableConfig) + " per item stack)</style>";
            }
            string rewardChance = "";
            if (ConfigFloat(FragileGiftBoxRewardChance, FragileGiftBoxEnableConfig) != 0)
            {
                rewardChance = ConfigFloat(FragileGiftBoxRewardChance, FragileGiftBoxEnableConfig) + "% chance to receive all or nothing. ";
            }
            string woundStack = "";
            if (ConfigInt(FragileGiftBoxWoundStack, FragileGiftBoxEnableConfig) != 0)
            {
                woundStack = " <style=cStack(+" + ConfigInt(FragileGiftBoxWoundStack, FragileGiftBoxEnableConfig) + " per item stack)</style>";
            }
            LanguageAPI.Add("FRAGILEGIFTBOX_NAME", "Fragile Gift Box");
            LanguageAPI.Add("FRAGILEGIFTBOX_PICKUP", "Get +" + ConfigInt(FragileGiftBoxReward, FragileGiftBoxEnableConfig) + rewardStack + " more items from chests. " + rewardChance + "Get " + ConfigInt(FragileGiftBoxWound, FragileGiftBoxEnableConfig) + woundStack + " <style=cIsDamage>Deep Wound</style> on chest opening");
            LanguageAPI.Add("FRAGILEGIFTBOX_DESC", "Get +" + ConfigInt(FragileGiftBoxReward, FragileGiftBoxEnableConfig) + rewardStack + " more items from chests. " + rewardChance + "Get " + ConfigInt(FragileGiftBoxWound, FragileGiftBoxEnableConfig) + woundStack + " <style=cIsDamage>Deep Wound</style> on chest opening");
            LanguageAPI.Add("FRAGILEGIFTBOX_LORE", "I never though you are also a cook brother"/*"\"Pick\"" +
                "\n" +
                "He picks the left one. The other one gives him what appears to be a gift box, made out of glass. The receiver looks at him badly, takes the box, takes the breath and crushes it, covering his arms in blood. Inside it there was a blue cupcake, he takes it, examines it and proceeds to consume it. His eyes fills with joy and pleasure." +
                "\n" +
                "\"Such a delicious meal. I never thought you are also a cook, brother... May I ask you what's also on your right hand?\"" +
                "\n" +
                "The \"brother\" then serrves him another box of it. He immediately took and smashed the box. On this time, it appears to be nothing inside of it."*/);
        }

    }

}