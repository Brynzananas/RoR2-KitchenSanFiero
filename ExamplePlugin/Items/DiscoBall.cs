using R2API;
using RoR2;
using static R2API.RecalculateStatsAPI;
using UnityEngine.AddressableAssets;
using UnityEngine;
using static CaeliImperiumPlugin.CaeliImperium;
using BepInEx.Configuration;
using System;
using RiskOfOptions.OptionConfigs;
using RiskOfOptions.Options;
using RiskOfOptions;
using System.Collections.Generic;
using static CaeliImperium.Items.SkullGammaGun;
using UnityEngine.Networking;
using System.Linq;
using EmotesAPI;
using CaeliImperium.Buffs;
using System.Runtime.CompilerServices;
using EntityStates.AffixVoid;

namespace CaeliImperium.Items
{
    internal static class DiscoBall
    {
        internal static GameObject DiscoBallPrefab;
        internal static Sprite DiscoBallBarIcon;
        public static ItemDef DiscoBallItemDef;
        public static string[] emoteArray = new string[0];
        public static string[] Taunts = { "Taunt1", "Taunt2", "Taunt3", "Taunt4", "Taunt5", "Taunt6", "Taunt7", "Taunt8", "Taunt9" };
        public static ConfigEntry<bool> DiscoBallEnable;
        public static ConfigEntry<bool> DiscoBallEnableConfig;
        public static ConfigEntry<bool> DiscoBallAIBlacklist;
        public static ConfigEntry<float> DiscoBallTier;
        public static ConfigEntry<bool> DiscoBallPizzaTowerMode;
        public static ConfigEntry<float> DiscoBallInterval;
        public static ConfigEntry<float> DiscoBallChance;
        public static ConfigEntry<float> DiscoBallChancePlayer;
        //public static ConfigEntry<float> DiscoBallChanceStack;
        public static ConfigEntry<float> DiscoBallTime;
        public static ConfigEntry<float> DiscoBallTimePlayer;
        public static ConfigEntry<float> DiscoBallMaxChance;
        public static ConfigEntry<float> DiscoBallSlowdown;

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        internal static void Init()
        {
            if (!EmotesCompatibility.badassEmotesEnabled && !EmotesCompatibility.brynzaEmotesEnabled)
            {
                return;
            }
            AddConfigs();
            string tier = "Assets/Icons/DiscoBallTier3.png";
            switch (ConfigFloat(DiscoBallTier, DiscoBallEnableConfig))
            {
                case 1:
                    tier = "Assets/Icons/DiscoBallTier1.png";
                    break;
                case 2:
                    tier = "Assets/Icons/DiscoBallTier2.png";
                    break;
                case 3:
                    tier = "Assets/Icons/DiscoBallTier3.png";
                    break;

            }
            DiscoBallPrefab = MainAssets.LoadAsset<GameObject>("Assets/Models/Prefabs/DiscoBall.prefab");
            DiscoBallBarIcon = MainAssets.LoadAsset<Sprite>(tier);
            

            AddEmotes();
            if (!DiscoBallEnable.Value)
            {
                return;
            }
            Item();
            DanceFrenzyBuff.Init();
            AddLanguageTokens();
        }

        private static void AddEmotes()
        {
            if (EmotesCompatibility.brynzaEmotesEnabled)
            {
                var emoteList = emoteArray.ToList();
                emoteList.Add("FeastDance");
                emoteList.Add("CaramellDansen");
                emoteList.Add("BigShoeLmfao");
                emoteList.Add("Bailan las Rochas y las Chetas");
                emoteList.Add("DrLiveSeyWalk");
                emoteList.Add("KahootBonetrousle");
                //emoteList.Add("KiryuPose");
                //emoteList.Add("MajimaPose");
                emoteList.Add("MajimaDance");
                emoteList.Add("TomSpeen");
                emoteList.Add("YouSpeenMeRound");
                emoteList.Add("Popipo");
                emoteList.Add("DragonDreamFeet");
                emoteArray = emoteList.ToArray();
            }
            if (EmotesCompatibility.badassEmotesEnabled)
            {
                var emoteList = emoteArray.ToList();
                emoteList.Add("Extraterrestial");
                emoteList.Add("Droop");
                emoteList.Add("SeeTinh");
                emoteList.Add("PopLock");
                emoteList.Add("DanceMoves");
                emoteList.Add("ImDiamond");
                emoteList.Add("Frolic");
                emoteList.Add("SwayLead");
                emoteList.Add("BestMates");
                emoteList.Add("Crackdown");
                emoteList.Add("Distraction");
                emoteList.Add("GangnamStyle");
                emoteList.Add("FlamencoIntro");
                emoteList.Add("Security");
                emoteList.Add("PPMusic");
                emoteList.Add("Crossbounce");
                emoteList.Add("Popular Vibe");
                emoteList.Add("NinjaStyle");
                emoteList.Add("VSWORLD");
                emoteList.Add("Griddy");
                emoteList.Add("Floss");
                emoteList.Add("Penguin");
                emoteList.Add("ChugJug");
                emoteList.Add("IsDynamite");
                emoteList.Add("GangTorture");
                emoteList.Add("OldSchool");
                emoteList.Add("OrangeJustice");
                emoteList.Add("BlindingLightsIntro");
                emoteList.Add("SquatKickIntro");
                emoteList.Add("Breakneck");
                emoteList.Add("Dougie");
                emoteList.Add("MyWorld");
                emoteList.Add("Stuck");
                emoteList.Add("Summertime");
                emoteList.Add("FancyFeet");
                emoteList.Add("Shufflin");
                emoteList.Add("Toosie");
                emoteList.Add("BimBamBom");
                emoteList.Add("GetDown");
                emoteList.Add("ArkDance");
                emoteList.Add("Macarena");
                emoteList.Add("ElectroSwing");
                emoteList.Add("Horny");
                emoteList.Add("Fresh");
                emoteList.Add("Goopie");
                emoteList.Add("TakeTheL");
                emoteList.Add("Infectious");
                emoteList.Add("Rollie");
                emoteList.Add("NeverGonna");
                emoteList.Add("CaliforniaGirls");
            emoteArray = emoteList.ToArray();
            }
        }

        private static void AddConfigs()
        {
            DiscoBallEnable = Config.Bind<bool>("Item : Disco Ball",
                             "Activation",
                             true,
                             "Enable this item?");
            DiscoBallEnableConfig = Config.Bind<bool>("Item : Disco Ball",
                             "Config Activation",
                             false,
                             "Enable config?");
            DiscoBallAIBlacklist = Config.Bind<bool>("Item : Disco Ball",
                             "AI Blacklist",
                             true,
                             "Blacklist this item from enemies?" +
                             "\nSIKE" +
                             "\nFALSE BY DEFAULT" +
                             "\nHAHAHAHAHAHAHAHAHAHAHAHAHAHAHAHAHAHAHAHAHAHAHAHAHAHAHAHAHAHAHAHAHAHAHAHAHAHAHAHAHAHAHAHAHAHAHAHAHAHAHAHAHAHAHAHAHAHAHAHAHAHAHHAHAHAHAHAHAHAHAHAHAHAHAHAHAHAHAHAHAHAHAHAHAHAHAHAHAHAHAHAHAHAHAHAHAHAHAHAHAHAHAHAHAHAHAHAHAHAHAHAHAHAHAHAHAHAHAHAHAHAHAHAHAHAHAHAHAHAHAHAHAHAHAHAHAHAHAHAHAHAHAHAHAHAHAHAHAHAHAHAHAHAHAHAHAHAHAHAHAHAHAHAHAHAHAHAHAHAHAHAHAHAHAHAHAHAHAHAHAHAHAHAHAHAHAHAHAHAHAHAHAHAHAHAHAHAHAHAHAHAHAHAHHAHAHAHAHAHAHAHAHAHAHAHAHAHAHAHAHAHAHAHAHAHAHAHAHAHAHAHAHAHAHAHAHAHAHAHAHAHAHAHAHAHAHAHAHAHAHAHAHAHAHAHAHAHAHAHAHAHAHAHAHAHAHAHAHAHAHAHAHAHAHAHAHAHAHAHAHAHAHA" +
                             "\n" +
                             "\n" +
                             "\n...sorry...it works as usual");
            DiscoBallTier = Config.Bind<float>("Item : Disco Ball",
                                         "Item tier",
                                         2f,
                                         "1: Common/White\n2: Rare/Green\n3: Legendary/Red");
            DiscoBallEnableConfig = Config.Bind<bool>("Item : Disco Ball",
                             "Config Activation",
                             false,
                             "Enable config?");
            DiscoBallPizzaTowerMode = Config.Bind<bool>("Item : Disco Ball",
                             "|Pizza Tower mode|",
                             false,
                             "Replaces dances with Pizza Tower taunts");
            DiscoBallInterval = Config.Bind<float>("Item : Disco Ball",
                                         "Interval",
                                         20f,
                                         "Control the interval of rolling the effect");
            DiscoBallChance = Config.Bind<float>("Item : Disco Ball",
                                         "Chance",
                                         10f,
                                         "Control the chance of making enemies dance in percentage");
            DiscoBallChancePlayer = Config.Bind<float>("Item : Disco Ball",
                                         "Chance for players",
                                         2f,
                                         "Control the chance of making players dance in percentage");
            //DiscoBallChanceStack = Config.Bind<bool>("Item : Disco Ball",
            //                             "Chance stack",
            //                             true,
            //                             "Enable chance stacking?");
            DiscoBallMaxChance = Config.Bind<float>("Item : Disco Ball",
                                         "Max Chance",
                                         70f,
                                         "Control the maximum chance of making enemies dance in percentage");
            DiscoBallTime = Config.Bind<float>("Item : Disco Ball",
                                         "Time",
                                         12f,
                                         "Control the time of enemy dancing in seconds");
            DiscoBallTimePlayer = Config.Bind<float>("Item : Disco Ball",
                                         "Time for players",
                                         7f,
                                         "Control the time of player dancing in seconds");
            //DiscoBallTimeStack = Config.Bind<bool>("Item : Disco Ball",
            //                             "Time stack",
            //                             false,
            //                             "Enable time stacking?");
            DiscoBallSlowdown = Config.Bind<float>("Item : Disco Ball",
                                         "Slowdown",
                                         70f,
                                         "Control the slowdown of dancing enemies in percentage");
            
            ModSettingsManager.AddOption(new CheckBoxOption(DiscoBallEnable, new CheckBoxConfig() { restartRequired = true }));
            ModSettingsManager.AddOption(new CheckBoxOption(DiscoBallEnableConfig, new CheckBoxConfig() { restartRequired = true }));
            ModSettingsManager.AddOption(new CheckBoxOption(DiscoBallAIBlacklist, new CheckBoxConfig() { restartRequired = true }));
            ModSettingsManager.AddOption(new StepSliderOption(DiscoBallTier, new StepSliderConfig() { min = 1, max = 3, increment = 1f, restartRequired = true }));
            ModSettingsManager.AddOption(new CheckBoxOption(DiscoBallPizzaTowerMode));
            ModSettingsManager.AddOption(new FloatFieldOption(DiscoBallInterval));
            ModSettingsManager.AddOption(new FloatFieldOption(DiscoBallChance));
            ModSettingsManager.AddOption(new FloatFieldOption(DiscoBallChancePlayer));
            ModSettingsManager.AddOption(new FloatFieldOption(DiscoBallMaxChance));
            ModSettingsManager.AddOption(new FloatFieldOption(DiscoBallTime));
            ModSettingsManager.AddOption(new FloatFieldOption(DiscoBallTimePlayer));
            ModSettingsManager.AddOption(new FloatFieldOption(DiscoBallSlowdown));
        }

        private static void Item()
        {
            DiscoBallItemDef = ScriptableObject.CreateInstance<ItemDef>();
            DiscoBallItemDef.name = "DiscoBall";
            DiscoBallItemDef.nameToken = "DISCOBALL_NAME";
            DiscoBallItemDef.pickupToken = "DISCOBALL_PICKUP";
            DiscoBallItemDef.descriptionToken = "DISCOBALL_DESC";
            DiscoBallItemDef.loreToken = "DISCOBALL_LORE";
            switch (ConfigFloat(DiscoBallTier, DiscoBallEnableConfig))
            {
                case 1:
                    DiscoBallItemDef.deprecatedTier = ItemTier.Tier1;
                    break;
                case 2:
                    DiscoBallItemDef.deprecatedTier = ItemTier.Tier2;
                    break;
                case 3:
                    DiscoBallItemDef.deprecatedTier = ItemTier.Tier3;
                    break;


            }
            DiscoBallItemDef.pickupIconSprite = DiscoBallBarIcon;
            DiscoBallItemDef.pickupModelPrefab = DiscoBallPrefab;
            DiscoBallItemDef.canRemove = true;
            DiscoBallItemDef.hidden = false;
            DiscoBallItemDef.requiredExpansion = CaeliImperiumExpansionDef;
            var tags = new List<ItemTag>() { ItemTag.Utility };
            if (ConfigBool(DiscoBallAIBlacklist, DiscoBallEnableConfig))
            {
                tags.Add(ItemTag.AIBlacklist);
            }
            DiscoBallItemDef.tags = tags.ToArray();
            var displayRules = new ItemDisplayRuleDict(null);
            ItemAPI.Add(new CustomItem(DiscoBallItemDef, displayRules));
            Run.onRunStartGlobal += OnStart;
            //On.RoR2.CharacterBody.OnInventoryChanged += ItemBehaviour;
        }

        private static void OnStart(Run run)
        {
            if (NetworkServer.active)
            {
                run.gameObject.AddComponent<DiscoBallComponent>();
            }
        }

        public class DiscoBallComponent : MonoBehaviour
        {

            public float timer;
            public void FixedUpdate()
            {
                if (NetworkServer.active)
                {

                    timer += Time.fixedDeltaTime;
                    if (timer > ConfigFloat(DiscoBallInterval, DiscoBallEnableConfig))
                    {
                        if (Util.GetItemCountGlobal(DiscoBallItemDef.itemIndex, true) > 0)
                        {
                            foreach(var characterBody in CharacterBody.readOnlyInstancesList)
                            {
                                int itemCount = Util.GetItemCountGlobal(DiscoBallItemDef.itemIndex, true) - Util.GetItemCountForTeam(characterBody.teamComponent.teamIndex, DiscoBallItemDef.itemIndex, true);
                                float chance = itemCount * ConvertAmplificationPercentageIntoReductionPercentage(ConfigFloat(DiscoBallChance, DiscoBallEnableConfig), ConfigFloat(DiscoBallMaxChance, DiscoBallEnableConfig));
                                if (characterBody.isPlayerControlled && characterBody.teamComponent.teamIndex == TeamIndex.Player)
                                {
                                    chance = itemCount * ConvertAmplificationPercentageIntoReductionPercentage(ConfigFloat(DiscoBallChancePlayer, DiscoBallEnableConfig), ConfigFloat(DiscoBallMaxChance, DiscoBallEnableConfig));
                                }
                                float time = ConfigFloat(DiscoBallTime, DiscoBallEnableConfig);
                                if (characterBody.isPlayerControlled && characterBody.teamComponent.teamIndex >= TeamIndex.Player)
                                {
                                    time = ConfigFloat(DiscoBallTimePlayer, DiscoBallEnableConfig);
                                }
                                if (characterBody.master && Util.CheckRoll(chance))
                                {
                                    characterBody.AddTimedBuff(DanceFrenzyBuff.DanceFrenzyBuffDef, time);
                                    //characterBody.AddTimedBuff(DLC2Content.Buffs.DisableAllSkills, ConfigFloat(DiscoBallTime, DiscoBallEnableConfig));
                                }
                                
                            }
                        }
                        timer = 0f;
                    }
                }
            }
        }
        //private static void ItemBehaviour(On.RoR2.CharacterBody.orig_OnInventoryChanged orig, CharacterBody self)
        //{
        //    if (NetworkServer.active)
        //    {
        //        self.AddItemBehavior<DiscoBallBehaviour>(self.inventory.GetItemCount(DiscoBallItemDef));
        //    }
        //    orig(self);
        //}

        //public class DiscoBallBehaviour : RoR2.CharacterBody.ItemBehavior
        //{
        //    public float timer;
        //    public void FixedUpdate()
        //    {
        //        if (!NetworkServer.active)
        //        {
        //            return;
        //        }
        //        int stack = this.stack;
        //        if (stack > 0)
        //        {
        //            timer += Time.fixedDeltaTime;
        //            if (timer > 10)
        //            {
        //                foreach (var characterBody in CharacterBody.readOnlyInstancesList)
        //                {
        //                    if (characterBody.teamComponent.teamIndex != body.teamComponent.teamIndex)
        //                    {
        //                        if (Util.CheckRoll(ConvertAmplificationPercentageIntoReductionPercentage(stack * 10, 70), body.master))
        //                            characterBody.AddTimedBuff(DanceFrenzyBuff.DanceFrenzyBuffDef, 10);
        //                    }
        //                }
        //                timer = 0;
        //            }
        //        }
        //    }
        //}

            private static void AddLanguageTokens()
        {
            LanguageAPI.Add("DISCOBALL_NAME", "Disco Ball");
            LanguageAPI.Add("DISCOBALL_PICKUP", "Every " + ConfigFloat(DiscoBallInterval, DiscoBallEnableConfig) + " seconds enemies have a " + ConfigFloat(DiscoBallChance, DiscoBallEnableConfig) + "% <style=cStack>(+" + ConfigFloat(DiscoBallChance, DiscoBallEnableConfig) + "% per item stack hyperbollicaly)</style> to fall into a dance frenzy for " + ConfigFloat(DiscoBallTime, DiscoBallEnableConfig) + " seconds. Dancing enemies <style=cDeath>can't use</style> their <style=cIsUtility>skills</style> and slowed down up to " + ConfigFloat(DiscoBallSlowdown, DiscoBallEnableConfig));
            LanguageAPI.Add("DISCOBALL_DESC", "Every " + ConfigFloat(DiscoBallInterval, DiscoBallEnableConfig) + " seconds enemies have a " + ConfigFloat(DiscoBallChance, DiscoBallEnableConfig) + "% <style=cStack>(+" + ConfigFloat(DiscoBallChance, DiscoBallEnableConfig) + "% per item stack hyperbollicaly)</style> to fall into a dance frenzy for " + ConfigFloat(DiscoBallTime, DiscoBallEnableConfig) + " seconds. Dancing enemies <style=cDeath>can't use</style> their <style=cIsUtility>skills</style> and slowed down up to " + ConfigFloat(DiscoBallSlowdown, DiscoBallEnableConfig));
            LanguageAPI.Add("DISCOBALL_LORE", "Sigma sigma on the wall, who is skibidiest of them all");
        }
    }
}
