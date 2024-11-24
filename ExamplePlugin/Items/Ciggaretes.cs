using RoR2;
using R2API;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine.AddressableAssets;
using UnityEngine;
using System.Reflection;
using UnityEngine.Diagnostics;
using static UnityEngine.RemoteConfigSettingsHelper;
using static ReignFromGreatBeyondPlugin.CaeliImperium;
using BepInEx.Configuration;
using RiskOfOptions.OptionConfigs;
using RiskOfOptions.Options;
using RiskOfOptions;
using CaeliImperium.Buffs;
using UnityEngine.UIElements;
using static R2API.RecalculateStatsAPI;

namespace CaeliImperium.Items
{
    internal static class Ciggaretes //: ItemBase<FirstItem>
    {
        internal static GameObject CiggaretesPrefab;
        internal static Sprite CiggaretesIcon;
        public static ItemDef CiggaretesItemDef;
        public static ConfigEntry<bool> PackOfCiggaretesEnable;
        public static ConfigEntry<bool> PackOfCiggaretesAIBlacklist;
        public static ConfigEntry<float> PackOfCiggaretesTier;
        public static ConfigEntry<bool> PackOfCiggaretesRework;
        public static ConfigEntry<float> PackOfCiggaretesRegen;
        public static ConfigEntry<float> PackOfCiggaretesRegenStack;
        public static ConfigEntry<float> PackOfCiggaretesRegenPercentage;
        public static ConfigEntry<float> PackOfCiggaretesRegenPercentageStack;
        public static ConfigEntry<float> PackOfCiggaretesConversion;
        public static ConfigEntry<float> PackOfCiggaretesTotalDamage;
        public static ConfigEntry<float> PackOfCiggaretesDamage;
        public static ConfigEntry<float> PackOfCiggaretesDuration;
       // public static ConfigEntry<float> PackOfCiggaretesIgniteTankNerf;
        public static ConfigEntry<bool> PackOfCiggaretesSmokeOnHit;
        public static ConfigEntry<bool> PackOfCiggaretesIgniteOnHit;
        public static ConfigEntry<float> PackOfCiggaretesTotalDamageOld;
        public static ConfigEntry<float> PackOfCiggaretesDamageOld;
        public static ConfigEntry<float> PackOfCiggaretesDurationOld;
        //public static ConfigEntry<float> PackOfCiggaretesIgniteTankNerfOld;
        private static GameObject CiggaretePrefab;

        internal static void Init()
        {
            AddConfigs();
            string tier = "Assets/Icons/PackOfCiggaretsIcon.png";
            switch (PackOfCiggaretesTier.Value)
            {
                case 1:
                    tier = "Assets/Icons/PackOfCiggaretsIcon.png";
                    break;
                case 2:
                    tier = "Assets/Icons/PackOfCiggaretsIconTier2.png";
                    break;
                case 3:
                    tier = "Assets/Icons/PackOfCiggaretsIconTier3.png";
                    break;

            }
            CiggaretesPrefab = MainAssets.LoadAsset<GameObject>("Assets/Models/Prefabs/Ciggaretes.prefab");
            CiggaretePrefab = MainAssets.LoadAsset<GameObject>("Assets/Models/Prefabs/CigWorldModel.prefab");
            CiggaretesIcon = MainAssets.LoadAsset<Sprite>(tier);
            if (!PackOfCiggaretesEnable.Value)
            {
                return;
            }
            Item();

            AddLanguageTokens();
            SmokingBuff.Init();
        }

        public static void AddConfigs()
        {
            PackOfCiggaretesEnable = Config.Bind<bool>("Item : Pack of Siggaretes",
                             "Activation",
                             true,
                             "Enable Pack of Siggaretes item?");
            PackOfCiggaretesAIBlacklist = Config.Bind<bool>("Item : Pack of Siggaretes",
                                         "AI Blacklist",
                                         false,
                                         "Blacklist this item from enemies?");
            PackOfCiggaretesTier = Config.Bind<float>("Item : Pack of Siggaretes",
                                         "Item tier",
                                         1f,
                                         "1: Common/White\n2: Uncommon/Green\n3: Rare/Red");
            PackOfCiggaretesRework = Config.Bind<bool>("Item : Pack of Siggaretes",
                                         "Rework",
                                         true,
                                         "Enable item rework?");
            PackOfCiggaretesRegen = Config.Bind<float>("Item : Pack of Siggaretes",
                                         "Negative regen",
                                         3f,
                                         "Control the negative regen value");
            PackOfCiggaretesRegenStack = Config.Bind<float>("Item : Pack of Siggaretes",
                                         "Negative regen stack",
                                         1f,
                                         "Control the negative regen value per item stack");
            PackOfCiggaretesRegenPercentage = Config.Bind<float>("Item : Pack of Siggaretes",
                                         "Negative regen percentage",
                                         0.3f,
                                         "Control the negative regen max health percentage");
            PackOfCiggaretesRegenPercentageStack = Config.Bind<float>("Item : Pack of Siggaretes",
                                         "Negative regen percentage stack",
                                         0.1f,
                                         "Control the negative regen max health percentage per item stack");
            PackOfCiggaretesDamage = Config.Bind<float>("Item : Pack of Siggaretes",
                                         "Burn damage multiplier",
                                         60f,
                                         "Change the burn damage multiplier in percentage");
            PackOfCiggaretesConversion = Config.Bind<float>("Item : Pack of Siggaretes",
                                         "Conversion",
                                         400f,
                                         "Control the damage required for the conversion in percentage");
            PackOfCiggaretesTotalDamage = Config.Bind<float>("Item : Pack of Siggaretes",
                                         "Burn total damage",
                                         50f,
                                         "Change the burn total damage in percentage");
            PackOfCiggaretesDuration = Config.Bind<float>("Item : Pack of Siggaretes",
                                         "Duration",
                                         5f,
                                         "Change the burn duration");
            /*PackOfCiggaretesIgniteTankNerf = Config.Bind<float>("Item : Pack of Siggaretes",
                                         "Ignition tank nerf",
                                         0f,
                                         "Control damage increase from Ignition Tank burn upgrade in percentage\nSet it to zero to use standart value");*/
            PackOfCiggaretesSmokeOnHit = Config.Bind<bool>("Item : Pack of Siggaretes",
                                         "Smoke on hit",
                                         false,
                                         "Smoke enemies on hit?\nPS: Old function");
            PackOfCiggaretesIgniteOnHit = Config.Bind<bool>("Item : Pack of Siggaretes",
                                         "Ihnite on hit",
                                         false,
                                         "Ignite enemies on hit?\nPS: Oldest function");
            PackOfCiggaretesDamageOld = Config.Bind<float>("Item : Pack of Siggaretes",
                                         "Burn damage multiplier (Old)",
                                         60f,
                                         "Change the burn damage multiplier in percentage");
            PackOfCiggaretesTotalDamageOld = Config.Bind<float>("Item : Pack of Siggaretes",
                                         "Burn total damage (Old)",
                                         50f,
                                         "Change the burn total damage in percentage");
            PackOfCiggaretesDurationOld = Config.Bind<float>("Item : Pack of Siggaretes",
                                         "Duration (Old)",
                                         5f,
                                         "Change the burn duration");
            /*PackOfCiggaretesIgniteTankNerfOld = Config.Bind<float>("Item : Pack of Siggaretes",
                                         "Ignition tank nerf (Old)",
                                         150f,
                                         "Control damage increase from Ignition Tank burn upgrade in percentage\nSet it to zero to use standart value");*/
            ModSettingsManager.AddOption(new CheckBoxOption(PackOfCiggaretesEnable, new CheckBoxConfig() { restartRequired = true }));
            ModSettingsManager.AddOption(new CheckBoxOption(PackOfCiggaretesAIBlacklist, new CheckBoxConfig() { restartRequired = true }));
            ModSettingsManager.AddOption(new StepSliderOption(PackOfCiggaretesTier, new StepSliderConfig() { min = 1, max = 3, increment = 1f, restartRequired = true }));
            ModSettingsManager.AddOption(new CheckBoxOption(PackOfCiggaretesRework));
            ModSettingsManager.AddOption(new FloatFieldOption(PackOfCiggaretesRegen));
            ModSettingsManager.AddOption(new FloatFieldOption(PackOfCiggaretesRegenStack));
            ModSettingsManager.AddOption(new FloatFieldOption(PackOfCiggaretesRegenPercentage));
            ModSettingsManager.AddOption(new FloatFieldOption(PackOfCiggaretesRegenPercentageStack));
            ModSettingsManager.AddOption(new CheckBoxOption(PackOfCiggaretesSmokeOnHit));
            ModSettingsManager.AddOption(new FloatFieldOption(PackOfCiggaretesConversion));
            ModSettingsManager.AddOption(new FloatFieldOption(PackOfCiggaretesTotalDamage));
            ModSettingsManager.AddOption(new FloatFieldOption(PackOfCiggaretesDamage));
            ModSettingsManager.AddOption(new FloatFieldOption(PackOfCiggaretesDuration));
            ModSettingsManager.AddOption(new CheckBoxOption(PackOfCiggaretesIgniteOnHit));
            ModSettingsManager.AddOption(new FloatFieldOption(PackOfCiggaretesTotalDamageOld));
            ModSettingsManager.AddOption(new FloatFieldOption(PackOfCiggaretesDamageOld));
            ModSettingsManager.AddOption(new FloatFieldOption(PackOfCiggaretesDurationOld));
        }

        private static void Item()
        {
            CiggaretesItemDef = ScriptableObject.CreateInstance<ItemDef>();
            CiggaretesItemDef.name = "PackOfCiggarets";
            CiggaretesItemDef.nameToken = "CIGGARETS_NAME";
            CiggaretesItemDef.pickupToken = "CIGGARETS_PICKUP";
            CiggaretesItemDef.descriptionToken = "CIGGARETS_DESC";
            CiggaretesItemDef.loreToken = "CIGGARETS_LORE";
            switch (PackOfCiggaretesTier.Value)
            {
                case 1: CiggaretesItemDef.deprecatedTier = ItemTier.Tier1;
                    break;
                    case 2: CiggaretesItemDef.deprecatedTier = ItemTier.Tier2;
                    break;
                    case 3: CiggaretesItemDef.deprecatedTier = ItemTier.Tier3;
                    break;

            }
            CiggaretesItemDef.pickupIconSprite = CiggaretesIcon;
            CiggaretesItemDef.pickupModelPrefab = CiggaretesPrefab;
            CiggaretesItemDef.canRemove = true;
            CiggaretesItemDef.hidden = false;
            CiggaretesItemDef.requiredExpansion = CaeliImperiumExpansionDef;
            var tags = new List<ItemTag>() { ItemTag.Damage };
            if (PackOfCiggaretesAIBlacklist.Value)
            {
                tags.Add(ItemTag.AIBlacklist);
            }
            CiggaretesItemDef.tags = tags.ToArray();
            ItemDisplayRuleDict rules = new ItemDisplayRuleDict();
            rules.Add("mdlCommandoDualies", new RoR2.ItemDisplayRule[]{
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = CiggaretePrefab,
                    childName = "Head",
localPos = new Vector3(0.09397F, 0.13065F, 0.23453F),
localAngles = new Vector3(0F, 33.54984F, 0F),
localScale = new Vector3(0.02104F, 0.02104F, 0.02104F)
                }
            });
            rules.Add("mdlHuntress", new RoR2.ItemDisplayRule[]{
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = CiggaretePrefab,
                    childName = "Head",
localPos = new Vector3(0.04652F, 0.13839F, 0.16507F),
localAngles = new Vector3(15.21544F, 57.41241F, 0F),
localScale = new Vector3(0.01098F, 0.01098F, 0.01098F)
                }
            });
            rules.Add("mdlBandit2", new RoR2.ItemDisplayRule[]{
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = CiggaretePrefab,
                    childName = "Head",
localPos = new Vector3(0.09868F, -0.04745F, 0.10878F),
localAngles = new Vector3(16.02018F, 58.21046F, 0F),
localScale = new Vector3(0.01116F, 0.01116F, 0.01116F)
                }
            });
            rules.Add("mdlToolbot", new RoR2.ItemDisplayRule[]{
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = CiggaretePrefab,
                    childName = "HeadCenter",
localPos = new Vector3(0.50409F, 2.03132F, -1.64039F),
localAngles = new Vector3(304.5827F, 180F, 0F),
localScale = new Vector3(0.30673F, 0.30673F, 0.30673F)
                }
            });
            rules.Add("mdlEngi", new RoR2.ItemDisplayRule[]{
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = CiggaretePrefab,
                    childName = "Chest",
localPos = new Vector3(0.10162F, 0.46094F, 0.19986F),
localAngles = new Vector3(0F, 27.9533F, 0F),
localScale = new Vector3(0.01343F, 0.01343F, 0.01343F)
                }
            });
            rules.Add("mdlEngiTurrety", new RoR2.ItemDisplayRule[]{
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = CiggaretePrefab,
                    childName = "Chest",
                    localPos = new Vector3(0f, 0f, 0f),
                    localAngles = new Vector3(0f, 0f, 0f),
                    localScale = new Vector3(1f, 1f, 1f)
                }
            });
            rules.Add("mdlMage", new RoR2.ItemDisplayRule[]{
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = CiggaretePrefab,
                    childName = "Head",
localPos = new Vector3(0.04105F, -0.03665F, 0.1544F),
localAngles = new Vector3(14.97686F, 23.49881F, 0F),
localScale = new Vector3(0.01036F, 0.01036F, 0.01036F)
                }
            });
            rules.Add("mdlMerc", new RoR2.ItemDisplayRule[]{
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = CiggaretePrefab,
                    childName = "Head",
localPos = new Vector3(0.05895F, 0.02939F, 0.15884F),
localAngles = new Vector3(27.06619F, 46.10017F, 0F),
localScale = new Vector3(0.01019F, 0.01019F, 0.01019F)
                }
            });
            rules.Add("mdlTreebot", new RoR2.ItemDisplayRule[]{
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = CiggaretePrefab,
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
                    followerPrefab = CiggaretePrefab,
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
                    followerPrefab = CiggaretePrefab,
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
                    followerPrefab = CiggaretePrefab,
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
                    followerPrefab = CiggaretePrefab,
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
                    followerPrefab = CiggaretePrefab,
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
                    followerPrefab = CiggaretePrefab,
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
                    followerPrefab = CiggaretePrefab,
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
                    followerPrefab = CiggaretePrefab,
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
                    followerPrefab = CiggaretePrefab,
                    childName = "Chest",
                    localPos = new Vector3(0f, 0f, 0f),
                    localAngles = new Vector3(0f, 0f, 0f),
                    localScale = new Vector3(1f, 1f, 1f)
                }
            });
            var displayRules = new ItemDisplayRuleDict(null);
            ItemAPI.Add(new CustomItem(CiggaretesItemDef, rules));
            On.RoR2.GlobalEventManager.OnHitEnemy += GlobalEventManager_OnHitEnemy;
            GetStatCoefficients += Stats;
        }

        private static void Stats(CharacterBody sender, StatHookEventArgs args)
        {
            if (PackOfCiggaretesRework.Value)
            {
                int itemCount = Util.GetItemCountGlobal(CiggaretesItemDef.itemIndex, true) - Util.GetItemCountForTeam(sender.teamComponent.teamIndex, CiggaretesItemDef.itemIndex, true);
                if (itemCount > 0)
                {
                    args.baseRegenAdd -= PackOfCiggaretesRegen.Value + ((itemCount - 1) * PackOfCiggaretesRegenStack.Value) + (sender.maxHealth * ((PackOfCiggaretesRegenPercentage.Value / 100) + ((itemCount - 1) * (PackOfCiggaretesRegenPercentageStack.Value / 100))));
                }
            }
            
        }

        private static void GlobalEventManager_OnHitEnemy(On.RoR2.GlobalEventManager.orig_OnHitEnemy orig, GlobalEventManager self, DamageInfo damageInfo, GameObject victim)
        {
            orig(self, damageInfo, victim);
            if (PackOfCiggaretesSmokeOnHit.Value || PackOfCiggaretesIgniteOnHit.Value)
            {
                var attacker = damageInfo.attacker;
                var body = attacker ? attacker.GetComponent<CharacterBody>() : null;
                int count = 0;
                if (body != null)
                {
                    count = body.inventory.GetItemCount(CiggaretesItemDef);

                }
                if (count > 0)
                {
                    if (damageInfo.attacker && !damageInfo.rejected)
                    {
                        CharacterBody victimBody = victim.GetComponent<CharacterBody>();
                        if (PackOfCiggaretesSmokeOnHit.Value)
                        {
                            if (Util.CheckRoll(damageInfo.procCoefficient * 100))
                            {
                                victimBody.AddBuff(SmokingBuff.SmokingBuffDef);

                            }
                            if (damageInfo.damage / body.damage >= PackOfCiggaretesConversion.Value / 100)
                            {
                                InflictDotInfo dotInfo = new InflictDotInfo()
                                {
                                    attackerObject = damageInfo.attacker,
                                    victimObject = victim,
                                    totalDamage = damageInfo.damage * (PackOfCiggaretesTotalDamage.Value / 100) * victimBody.GetBuffCount(SmokingBuff.SmokingBuffDef),
                                    damageMultiplier = PackOfCiggaretesDamage.Value / 100,
                                    duration = PackOfCiggaretesDuration.Value,
                                    dotIndex = DotController.DotIndex.Burn,
                                    maxStacksFromAttacker = 69
                                };
                                StrengthenBurnUtils.CheckDotForUpgrade(body.inventory, ref dotInfo);
                                DotController.InflictDot(ref dotInfo);
                                victimBody.SetBuffCount(SmokingBuff.SmokingBuffDef.buffIndex, 0);
                            }
                        }


                        if (PackOfCiggaretesIgniteOnHit.Value)
                        {
                            /*if (body.inventory.GetItemCount(DLC1Content.Items.StrengthenBurn.itemIndex) > 0)
                                                {

                                                    float tankAmount = body.inventory.GetItemCount(DLC1Content.Items.StrengthenBurn.itemIndex) * 2f;
                                                    InflictDotInfo dotInfo = new InflictDotInfo()
                                                    {
                                                        attackerObject = damageInfo.attacker,
                                                        victimObject = victim,
                                                        totalDamage = damageInfo.damage * (PackOfCiggaretesTotalDamage.Value / 100),
                                                        damageMultiplier = PackOfCiggaretesDamage.Value * tankAmount * damageInfo.procCoefficient,
                                                        duration = PackOfCiggaretesDuration.Value,
                                                        dotIndex = DotController.DotIndex.StrongerBurn,
                                                        maxStacksFromAttacker = (uint?)count
                                                    };
                                                    DotController.InflictDot(ref dotInfo);
                                                }
                                                else
                                                {*/
                            InflictDotInfo dotInfo = new InflictDotInfo()
                            {
                                attackerObject = damageInfo.attacker,
                                victimObject = victim,
                                totalDamage = damageInfo.damage * PackOfCiggaretesTotalDamage.Value,
                                damageMultiplier = PackOfCiggaretesDamage.Value * damageInfo.procCoefficient,
                                duration = PackOfCiggaretesDuration.Value,
                                dotIndex = DotController.DotIndex.Burn,
                                maxStacksFromAttacker = (uint?)count
                            };
                            StrengthenBurnUtils.CheckDotForUpgrade(body.inventory, ref dotInfo);
                            DotController.InflictDot(ref dotInfo);

                            //}
                        }

                    }

                    //var InventoryCount = GetCount(body);



                }
            }
            
        }
        /*
        private void Update()
        {
            // This if statement checks if the player has currently pressed F2.
            if (Input.GetKeyDown(KeyCode.F2))
            {
                Chat.AddMessage();
            }
        }
        */
        /*private static void GlobalEventManager_OnHitEnemy(DamageReport report)
{







   // If a character was killed by the world, we shouldn't do anything.
   if (!report.attacker || !report.attackerBody)
   {
       return;
   }

   var attackerCharacterBody = report.attackerBody;

   // We need an inventory to do check for our item
   if (attackerCharacterBody.inventory)
   {
       // Store the amount of our item we have
       var garbCount = attackerCharacterBody.inventory.GetItemCount(myItemDef.itemIndex);
       if (garbCount > 0 &&
           // Roll for our 50% chance.
           Util.CheckRoll(50, attackerCharacterBody.master))
       {
           // Since we passed all checks, we now give our attacker the cloaked buff.
           // Note how we are scaling the buff duration depending on the number of the custom item in our inventory.
           attackerCharacterBody.AddTimedBuff(RoR2Content.Buffs.Cloak, 3 + garbCount);
       }
   }
}*/
        private static void AddLanguageTokens()
        {
            string smoke = "";
            if (PackOfCiggaretesSmokeOnHit.Value)
            {
                smoke = ". On hit <style=cIsUtility>Smoke</style> enemies <style=cStack>(+1 per item stack)</style>. Hits that deal <style=cIsDamage>more than " + PackOfCiggaretesConversion.Value + "% damage</style> convert all <style=cIsUtility>Smoke</style> to <style=cIsDamage>Burn</style>";
            }
            LanguageAPI.Add("CIGGARETS_NAME", "Pack of Ciggaretes");
            LanguageAPI.Add("CIGGARETS_PICKUP", "Enemies gain <style=cDeath>+" + PackOfCiggaretesRegen.Value + " hp/s</style> <style=cStack>(+" + PackOfCiggaretesRegenStack.Value + " hp/s per item stack)</style> and <style=cDeath>" + PackOfCiggaretesRegenPercentage.Value + "%</style> <style=cStack>(+" + PackOfCiggaretesRegenPercentageStack.Value + "% per item stack)</style> <style=cDeath>health drain</style>" + smoke);
            LanguageAPI.Add("CIGGARETS_DESC", "Enemies gain <style=cDeath>+" + PackOfCiggaretesRegen.Value + " hp/s</style> <style=cStack>(+" + PackOfCiggaretesRegenStack.Value + " hp/s per item stack)</style> and <style=cDeath>" + PackOfCiggaretesRegenPercentage.Value + "%</style> <style=cStack>(+" + PackOfCiggaretesRegenPercentageStack.Value + "% per item stack)</style> <style=cDeath>health drain</style>" + smoke);
            LanguageAPI.Add("CIGGARETS_LORE", "");
        }

    }

}
