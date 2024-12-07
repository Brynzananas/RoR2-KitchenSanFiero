using R2API;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine.AddressableAssets;
using UnityEngine;
using static CaeliImperiumPlugin.CaeliImperium;
using static R2API.RecalculateStatsAPI;
using BepInEx.Configuration;
using RiskOfOptions.OptionConfigs;
using RiskOfOptions.Options;
using RiskOfOptions;
using UnityEngine.Networking;
using CaeliImperium.Buffs;

namespace CaeliImperium.Items
{
    internal static class Medicine
        //: ItemBase<FirstItem>
    {
        internal static GameObject MedicinePrefab;
        internal static Sprite MedicineIcon;
        public static ItemDef MedicineItemDef;
        public static ConfigEntry<bool> PainkillersEnable;
        public static ConfigEntry<bool> PainkillersEnableConfig;
        public static ConfigEntry<bool> PainkillersAIBlacklist;
        public static ConfigEntry<float> PainkillersTier;
        //public static ConfigEntry<float> PainkillersArmorMult;
        public static ConfigEntry<float> PainkillersArmorAdd;
        public static ConfigEntry<float> PainkillersMaxHealthMult;
        public static ConfigEntry<float> PainkillersMaxHealthAdd;
        public static ConfigEntry<float> PainkillersRegenMult;
        public static ConfigEntry<float> PainkillersRegenAdd;
        public static ConfigEntry<float> PainkillersHealthMult;
        public static ConfigEntry<float> PainkillersHealthAdd;
        private static string name = "Medicine";
        //public static ConfigEntry<bool> PainkillersOnItemPickupEffect;

        internal static void Init()
        {
            AddConfigs();
            string tier = "Assets/Icons/Painkillers.png";
            switch (ConfigFloat(PainkillersTier, PainkillersEnableConfig))
            {
                case 1:
                    tier = "Assets/Icons/Painkillers.png";
                    break;
                case 2:
                    tier = "Assets/Icons/PainkillersTier2.png";
                    break;
                case 3:
                    tier = "Assets/Icons/PainkillersTier3.png";
                    break;

            }
            MedicinePrefab = MainAssets.LoadAsset<GameObject>("Assets/Models/Prefabs/PainKillers.prefab");
            MedicineIcon = MainAssets.LoadAsset<Sprite>(tier);
            if (!PainkillersEnable.Value)
            {
                return;
            }
            Item();
            //MedicineBuff.Init();
            AddLanguageTokens();
        }

        public static void AddConfigs()
        {
            PainkillersEnable = Config.Bind<bool>("Item : " + name,
                             "Activation",
                             true,
                             "Enable this item?");
            PainkillersEnableConfig = Config.Bind<bool>("Item : " + name,
                             "Config Activation",
                             false,
                             "Enable config?");
            PainkillersAIBlacklist = Config.Bind<bool>("Item : " + name,
                             "AI Blacklist",
                             false,
                             "Blacklist this item from enemies?");
            PainkillersTier = Config.Bind<float>("Item : " + name,
                                         "Item tier",
                                         1f,
                                         "1: Common/White\n2: Rare/Green\n3: Legendary/Red");
            PainkillersArmorAdd = Config.Bind<float>("Item : " + name,
                                         "Armor add",
                                         0f,
                                         "Control how much this item gives flat armor");
            /*PainkillersArmorMult = Config.Bind<float>("Item : Painkillers",
                                         "Armor multiply percentage",
                                         0f,
                                         "Control how much this item multiplies armor in percentage");*/
            PainkillersRegenAdd = Config.Bind<float>("Item : " + name,
                                         "Regen add",
                                         1f,
                                         "Control how much this item gives flat regeneration");
            PainkillersRegenMult = Config.Bind<float>("Item : " + name,
                                         "Regen multiply percentage",
                                         5f,
                                         "Control how much this item multiplies regeneration in percentage");
            PainkillersMaxHealthAdd = Config.Bind<float>("Item : " + name,
                                         "Max health add",
                                         0f,
                                         "Control how much this item gives flat maximum health");
            PainkillersMaxHealthMult = Config.Bind<float>("Item : " + name,
                                         "Max health multiply percentage",
                                         0f,
                                         "Control how much this item multiplies maximum health in percentage");
            PainkillersHealthAdd = Config.Bind<float>("Item : " + name,
                                         "Healing add",
                                         1f,
                                         "Control how much this item gives flat healing addition");
            PainkillersHealthMult = Config.Bind<float>("Item : " + name,
                                         "Healing increase percentage",
                                         5f,
                                         "Control how much this item increase healing in percentage");
            /*PainkillersOnItemPickupEffect = Config.Bind<bool>("Item : Painkillers",
                             "On item pickup effect",
                             true,
                             "Enable on pickup effect?");*/
            ModSettingsManager.AddOption(new CheckBoxOption(PainkillersEnable, new CheckBoxConfig() { restartRequired = true }));
            ModSettingsManager.AddOption(new CheckBoxOption(PainkillersAIBlacklist, new CheckBoxConfig() { restartRequired = true }));
            ModSettingsManager.AddOption(new StepSliderOption(PainkillersTier, new StepSliderConfig() { min = 1, max = 3, increment = 1f, restartRequired = true }));
            ModSettingsManager.AddOption(new FloatFieldOption(PainkillersArmorAdd));
            ModSettingsManager.AddOption(new FloatFieldOption(PainkillersRegenAdd));
            ModSettingsManager.AddOption(new FloatFieldOption(PainkillersRegenMult));
            ModSettingsManager.AddOption(new FloatFieldOption(PainkillersMaxHealthAdd));
            ModSettingsManager.AddOption(new FloatFieldOption(PainkillersMaxHealthMult));
            ModSettingsManager.AddOption(new FloatFieldOption(PainkillersHealthAdd));
            ModSettingsManager.AddOption(new FloatFieldOption(PainkillersHealthMult));
            //ModSettingsManager.AddOption(new CheckBoxOption(PainkillersOnItemPickupEffect));
        }

        private static void Item()
        {
            MedicineItemDef = ScriptableObject.CreateInstance<ItemDef>();
            MedicineItemDef.name = name.Replace(" ", "");
            MedicineItemDef.nameToken = name.Replace(" ", "").ToUpper() + "_NAME";
            MedicineItemDef.pickupToken = name.Replace(" ", "").ToUpper() + "_PICKUP";
            MedicineItemDef.descriptionToken = name.Replace(" ", "").ToUpper() + "_DESC";
            MedicineItemDef.loreToken = name.Replace(" ", "").ToUpper() + "_LORE";
            switch (ConfigFloat(PainkillersTier, PainkillersEnableConfig))
            {
                case 1:
                    MedicineItemDef.deprecatedTier = ItemTier.Tier1;
                    break;
                case 2:
                    MedicineItemDef.deprecatedTier = ItemTier.Tier2;
                    break;
                case 3:
                    MedicineItemDef.deprecatedTier = ItemTier.Tier3;
                    break;

            }
            MedicineItemDef.pickupIconSprite = MedicineIcon;
            MedicineItemDef.pickupModelPrefab = MedicinePrefab;
            MedicineItemDef.canRemove = true;
            MedicineItemDef.hidden = false;
            MedicineItemDef.requiredExpansion = CaeliImperiumExpansionDef;
            var tags = new List<ItemTag>() { ItemTag.Healing };
            if (PainkillersAIBlacklist.Value)
            {
                tags.Add(ItemTag.AIBlacklist);
            }
            MedicineItemDef.tags = tags.ToArray();
            ItemDisplayRuleDict rules = new ItemDisplayRuleDict();
            rules.Add("mdlCommandoDualies", new RoR2.ItemDisplayRule[]{
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = MedicinePrefab,
                    childName = "Pelvis",
localPos = new Vector3(-0.18914F, -0.04197F, 0.01054F),
localAngles = new Vector3(10.29398F, 110.1134F, 180F),
localScale = new Vector3(0.06126F, 0.06126F, 0.06126F)
                }
            });
            rules.Add("mdlHuntress", new RoR2.ItemDisplayRule[]{
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = MedicinePrefab,
                    childName = "Pelvis",
localPos = new Vector3(-0.24011F, 0.05098F, 0.0286F),
localAngles = new Vector3(337.8354F, 85.27686F, 187.3692F),
localScale = new Vector3(0.04147F, 0.04147F, 0.04147F)
                }
            });
            rules.Add("mdlBandit2", new RoR2.ItemDisplayRule[]{
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = MedicinePrefab,
                    childName = "Chest",
localPos = new Vector3(-0.13665F, 0.30019F, -0.225F),
localAngles = new Vector3(16.17203F, 94.40896F, 123.702F),
localScale = new Vector3(0.03756F, 0.03756F, 0.03756F)
                }
            });
            rules.Add("mdlToolbot", new RoR2.ItemDisplayRule[]{
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = MedicinePrefab,
                    childName = "Chest",
localPos = new Vector3(2.4724F, 1.79825F, -0.43137F),
localAngles = new Vector3(0F, 0F, 3.70976F),
localScale = new Vector3(0.40546F, 0.40546F, 0.40546F)
                }
            });
            rules.Add("mdlEngi", new RoR2.ItemDisplayRule[]{
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = MedicinePrefab,
                    childName = "Pelvis",
localPos = new Vector3(-0.2754F, 0.04958F, 0.01303F),
localAngles = new Vector3(5.42078F, 71.9818F, 174.0692F),
localScale = new Vector3(0.04987F, 0.04987F, 0.04987F)
                }
            });
            //rules.Add("mdlEngiTurrety", new RoR2.ItemDisplayRule[]{
            //    new RoR2.ItemDisplayRule
            //    {
            //        ruleType = ItemDisplayRuleType.ParentedPrefab,
            //        followerPrefab = PainkillersPrefab,
            //        childName = "Chest",
            //        localPos = new Vector3(0f, 0f, 0f),
            //        localAngles = new Vector3(0f, 0f, 0f),
            //        localScale = new Vector3(1f, 1f, 1f)
            //    }
            //});
            rules.Add("mdlMage", new RoR2.ItemDisplayRule[]{
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = MedicinePrefab,
                    childName = "Pelvis",
localPos = new Vector3(-0.21339F, -0.03018F, -0.05785F),
localAngles = new Vector3(0.24362F, 82.89925F, 209.5247F),
localScale = new Vector3(0.05072F, 0.05072F, 0.05072F)
                }
            });
            rules.Add("mdlMerc", new RoR2.ItemDisplayRule[]{
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = MedicinePrefab,
                    childName = "Pelvis",
localPos = new Vector3(0.21222F, 0.08415F, -0.01188F),
localAngles = new Vector3(348.6433F, 262.1263F, 187.7867F),
localScale = new Vector3(0.05652F, 0.05652F, 0.05652F)
                }
            });/*
            rules.Add("mdlTreebot", new RoR2.ItemDisplayRule[]{
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = PainkillersPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0f, 0f, 0f),
                    localAngles = new Vector3(0f, 0f, 0f),
                    localScale = new Vector3(1f, 1f, 1f)
                }
            });
            rules.Add("mdlLoader", new RoR2.ItemDisplayRule[]{
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = PainkillersPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0f, 0f, 0f),
                    localAngles = new Vector3(0f, 0f, 0f),
                    localScale = new Vector3(1f, 1f, 1f)
                }
            });
            rules.Add("mdlCroco", new RoR2.ItemDisplayRule[]{
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = PainkillersPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0f, 0f, 0f),
                    localAngles = new Vector3(0f, 0f, 0f),
                    localScale = new Vector3(1f, 1f, 1f)
                }
            });
            rules.Add("mdlCaptain", new RoR2.ItemDisplayRule[]{
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = PainkillersPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0f, 0f, 0f),
                    localAngles = new Vector3(0f, 0f, 0f),
                    localScale = new Vector3(1f, 1f, 1f)
                }
            });
            rules.Add("mdlRailGunner", new RoR2.ItemDisplayRule[]{
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = PainkillersPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0f, 0f, 0f),
                    localAngles = new Vector3(0f, 0f, 0f),
                    localScale = new Vector3(1f, 1f, 1f)
                }
            });
            rules.Add("mdlVoidSurvivor", new RoR2.ItemDisplayRule[]{
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = PainkillersPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0f, 0f, 0f),
                    localAngles = new Vector3(0f, 0f, 0f),
                    localScale = new Vector3(1f, 1f, 1f)
                }
            });
            rules.Add("mdlSeeker", new RoR2.ItemDisplayRule[]{
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = PainkillersPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0f, 0f, 0f),
                    localAngles = new Vector3(0f, 0f, 0f),
                    localScale = new Vector3(1f, 1f, 1f)
                }
            });
            rules.Add("mdlChef", new RoR2.ItemDisplayRule[]{
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = PainkillersPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0f, 0f, 0f),
                    localAngles = new Vector3(0f, 0f, 0f),
                    localScale = new Vector3(1f, 1f, 1f)
                }
            });
            rules.Add("mdlFalseSon", new RoR2.ItemDisplayRule[]{
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = PainkillersPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0f, 0f, 0f),
                    localAngles = new Vector3(0f, 0f, 0f),
                    localScale = new Vector3(1f, 1f, 1f)
                }
            });
            rules.Add("mdlScav", new RoR2.ItemDisplayRule[]{
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = PainkillersPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0f, 0f, 0f),
                    localAngles = new Vector3(0f, 0f, 0f),
                    localScale = new Vector3(1f, 1f, 1f)
                }
            });*/
            var displayRules = new ItemDisplayRuleDict(null);
            ItemAPI.Add(new CustomItem(MedicineItemDef, rules));
            //On.RoR2.CharacterBody.OnInventoryChanged += HealOnPickup;
            GetStatCoefficients += Stats;
            On.RoR2.HealthComponent.Heal += IncreasedHealing;
        }


        private static float IncreasedHealing(On.RoR2.HealthComponent.orig_Heal orig, HealthComponent self, float amount, ProcChainMask procChainMask, bool nonRegen)
        {
            int itemCount = self.body.inventory? self.body.inventory.GetItemCount(MedicineItemDef) : 0;
            if (itemCount > 0)
            {
                amount += itemCount * ConfigFloat(PainkillersHealthAdd, PainkillersEnableConfig);

                amount *= 1f + (itemCount * ConfigFloat(PainkillersHealthMult, PainkillersEnableConfig) / 100);
            }
            return orig(self, amount, procChainMask, nonRegen);
        }

        /*
        private static void HealOnPickup(On.RoR2.CharacterBody.orig_OnInventoryChanged orig, CharacterBody self)
        {

            orig(self);
            int itemCount = self.inventory ? self.inventory.GetItemCount(PainkillersItemDef) : 0;
            if (itemCount > 0)
            {
                Debug.Log((ItemIndex)self.inventory.itemAcquisitionOrder.ToArray().GetValue(self.inventory.itemAcquisitionOrder.ToArray().Length - 1));
                Debug.Log(Painkillers.PainkillersItemDef.itemIndex);
            }

            if (itemCount > 0 && (ItemIndex)self.inventory.itemAcquisitionOrder.ToArray().GetValue(self.inventory.itemAcquisitionOrder.ToArray().Length - 1) == Painkillers.PainkillersItemDef.itemIndex)
            {
                Chat.AddMessage("Heal");
                Util.CleanseBody(self, true, false, false, true, true, false);
                self.healthComponent.HealFraction(1, default);
                self.healthComponent.RechargeShieldFull();
                self.healthComponent.barrier += Math.Min(self.maxHealth - self.healthComponent.barrier, 0);
            }
        }*/

        private static void Stats(CharacterBody sender, StatHookEventArgs args)
        {
            int itemCount = sender.inventory ? sender.inventory.GetItemCount(MedicineItemDef) : 0;
            if (itemCount > 0)
            {
                args.armorAdd += itemCount * ConfigFloat(PainkillersArmorAdd, PainkillersEnableConfig);
                args.baseRegenAdd += itemCount * ConfigFloat(PainkillersRegenAdd, PainkillersEnableConfig);
                args.regenMultAdd += itemCount * ConfigFloat(PainkillersRegenMult, PainkillersEnableConfig) / 100;
                args.baseHealthAdd += itemCount * ConfigFloat(PainkillersMaxHealthAdd, PainkillersEnableConfig);
                args.healthMultAdd += itemCount * ConfigFloat(PainkillersMaxHealthMult, PainkillersEnableConfig) / 100;
            }

        }

        private static void AddLanguageTokens()
        {
            string armor = "";
            if (ConfigFloat(PainkillersArmorAdd, PainkillersEnableConfig) > 0)
            {
                armor = "Increases <style=cIsHealing>armor</style> by <style=cIsHealing>" + ConfigFloat(PainkillersArmorAdd, PainkillersEnableConfig) + "</style> <style=cStack>(+" + ConfigFloat(PainkillersArmorAdd, PainkillersEnableConfig) + " per item stack)</style>\n";
            }
            string regen = "";
            if (ConfigFloat(PainkillersRegenAdd, PainkillersEnableConfig) > 0)
            {
                regen = "Increases <style=cIsHealing>regeneration</style> by <style=cIsHealing>" + ConfigFloat(PainkillersRegenAdd, PainkillersEnableConfig) + "</style> <style=cStack>(+" + ConfigFloat(PainkillersRegenAdd, PainkillersEnableConfig) + " per item stack)</style>\n";
            }
            string maxHealh = "";
            if (ConfigFloat(PainkillersMaxHealthAdd, PainkillersEnableConfig) > 0)
            {
                maxHealh = "Increases <style=cIsHealth>max health</style> by <style=cIsHealth>" + ConfigFloat(PainkillersMaxHealthAdd, PainkillersEnableConfig) + "</style> <style=cStack>(+" + ConfigFloat(PainkillersMaxHealthAdd, PainkillersEnableConfig) + " per item stack)</style>\n";
            }
            string realing = "";
            if (ConfigFloat(PainkillersHealthAdd, PainkillersEnableConfig) > 0)
            {
                realing = "Increases <style=cIsHealing>healing</style> by <style=cIsHealing>" + ConfigFloat(PainkillersHealthAdd, PainkillersEnableConfig) + "</style> <style=cStack>(+" + ConfigFloat(PainkillersHealthAdd, PainkillersEnableConfig) + " per item stack)</style>\n";
            }
            string regenMult = "";
            if (ConfigFloat(PainkillersRegenMult, PainkillersEnableConfig) > 0)
            {
                regenMult = "Increases <style=cIsHealing>regeneration</style> by <style=cIsHealing>" + ConfigFloat(PainkillersRegenMult, PainkillersEnableConfig) + "%</style> <style=cStack>(+" + ConfigFloat(PainkillersRegenMult, PainkillersEnableConfig) + "% per item stack)</style>\n";
            }
            string maxHealthMult = "";
            if (ConfigFloat(PainkillersMaxHealthMult, PainkillersEnableConfig) > 0)
            {
                maxHealthMult = "Increases <style=cIsHealth>max health</style> by <style=cIsHealth>" + ConfigFloat(PainkillersMaxHealthMult, PainkillersEnableConfig) + "%</style> <style=cStack>(+" + ConfigFloat(PainkillersMaxHealthMult, PainkillersEnableConfig) + "% per item stack)</style>\n";
            }
            string healingMult = "";
            if (ConfigFloat(PainkillersHealthMult, PainkillersEnableConfig) > 0)
            {
                healingMult = "Increases <style=cIsHealing>healing</style> by <style=cIsHealing>" + ConfigFloat(PainkillersHealthMult, PainkillersEnableConfig) + "%</style> <style=cStack>(+" + ConfigFloat(PainkillersHealthMult, PainkillersEnableConfig) + "% per item stack)</style>\n";
            }
            LanguageAPI.Add(name.Replace(" ", "").ToUpper() + "_NAME", name);
            LanguageAPI.Add(name.Replace(" ", "").ToUpper() + "_PICKUP", "Slightly increases <style=cIsHealing>all health statistics</style>");
            LanguageAPI.Add(name.Replace(" ", "").ToUpper() + "_DESC", 
                armor +
                regen +
                maxHealh +
                realing +
                regenMult +
                maxHealthMult +
                healingMult);
            LanguageAPI.Add(name.Replace(" ", "").ToUpper() + "_LORE", "\n" +
                "\"May 9" +
                "\n" +
                "   Rain." +
                "\n" +
                "   Stayed out the window all day." +
                "\n" +
                "   Peaceful here - nothing to do." +
                "\n" +
                "   Still not allowed to go outside." +
                "\n" +
                "\n" +
                "May 10" +
                "\n" +
                "   Still raining." +
                "\n" +
                "   Talked with a doctor a little." +
                "\n" +
                "   Would they have saved me" +
                "\n" +
                "   if I didn't have a family to feed?" +
                "\n" +
                "   I know I\'m pathetic and weak." +
                "\n" +
                "   Not everyone can be strong." +
                "\n" +
                "\n" +
                "May 11" +
                "\n" +
                "   Rain again." +
                "\n" +
                "   The meds made me" +
                "\n" +
                "   feel sick today." +
                "\n" +
                "   If I\'m only better" +
                "\n" +
                "   when I\'m drugged," +
                "\n" +
                "   then who am I anyway?\"");
        }
    }
}
