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
using static CaeliImperium.Items.SkullGammaGun;
using AK.Wwise;
using System.Linq;

namespace CaeliImperium.Items
{
    internal static class CursingEye
    //: ItemBase<FirstItem>
    {
        internal static GameObject CursingEyePrefab;
        internal static Sprite CursingEyeIcon;
        public static ItemDef CursingEyeItemDef;
        public static ConfigEntry<bool> CursingEyeEnable;
        public static ConfigEntry<bool> CursingEyeEnableConfig;
        public static ConfigEntry<bool> CursingEyeAIBlacklist;
        public static ConfigEntry<float> CursingEyeInterval;
        public static ConfigEntry<int> CursingEyeDebuff;
        public static ConfigEntry<int> CursingEyeDebuffStack;
        public static ConfigEntry<float> CursingEyeDuration;
        public static ConfigEntry<float> CursingEyeDurationStack;
        
        private static string name = "Cursing Eye";
        //public static ConfigEntry<bool> PainkillersOnItemPickupEffect;

        internal static void Init()
        {
            AddConfigs();
            string tier = "Assets/Icons/Painkillers.png";
            CursingEyePrefab = MainAssets.LoadAsset<GameObject>("Assets/Models/Prefabs/CursingEye.prefab");
            CursingEyeIcon = MainAssets.LoadAsset<Sprite>(tier);
            if (!CursingEyeEnable.Value)
            {
                return;
            }
            Item();
            AddLanguageTokens();
        }

        private static void BlacklistDebuffs()
        {
            
        }

        public static void AddConfigs()
        {
            CursingEyeEnable = Config.Bind<bool>("Item : " + name,
                             "Activation",
                             true,
                             "Enable this item?");
            CursingEyeEnableConfig = Config.Bind<bool>("Item : " + name,
                             "Config Activation",
                             false,
                             "Enable config?\nActivation option and |options under these brackets| are always taken in effect");
            CursingEyeAIBlacklist = Config.Bind<bool>("Item : " + name,
                             "AI Blacklist",
                             false,
                             "Blacklist this item from enemies?");
            CursingEyeInterval = Config.Bind<float>("Item : " + name,
                                         "Interval",
                                         3f,
                                         "Control the time required for this item to curse");
            CursingEyeDebuff = Config.Bind<int>("Item : " + name,
                                         "Debuff amount",
                                         1,
                                         "Control the amount of debuffs applied on curse");
            CursingEyeDebuffStack = Config.Bind<int>("Item : " + name,
                                         "Debuff amount stack",
                                         1,
                                         "Control the amount of debuffs applied per item stack on curse");
            CursingEyeDuration = Config.Bind<float>("Item : " + name,
                                         "Duration",
                                         10f,
                                         "Control the duration of debuffs in seconds");
            CursingEyeDurationStack = Config.Bind<float>("Item : " + name,
                                         "Duration stack",
                                         0f,
                                         "Control the duration of debuffs increase per item stack in seconds");
            ModSettingsManager.AddOption(new CheckBoxOption(CursingEyeEnable, new CheckBoxConfig() { restartRequired = true }));
            ModSettingsManager.AddOption(new CheckBoxOption(CursingEyeEnableConfig));
            ModSettingsManager.AddOption(new CheckBoxOption(CursingEyeAIBlacklist, new CheckBoxConfig() { restartRequired = true }));
            ModSettingsManager.AddOption(new FloatFieldOption(CursingEyeInterval));
            ModSettingsManager.AddOption(new FloatFieldOption(CursingEyeDuration));
            ModSettingsManager.AddOption(new FloatFieldOption(CursingEyeDurationStack));
            ModSettingsManager.AddOption(new IntFieldOption(CursingEyeDebuff));
            ModSettingsManager.AddOption(new IntFieldOption(CursingEyeDebuffStack));
        }

        private static void Item()
        {
            CursingEyeItemDef = ScriptableObject.CreateInstance<ItemDef>();
            CursingEyeItemDef.name = name.Replace(" ", "");
            CursingEyeItemDef.nameToken = name.Replace(" ", "").ToUpper() + "_NAME";
            CursingEyeItemDef.pickupToken = name.Replace(" ", "").ToUpper() + "_PICKUP";
            CursingEyeItemDef.descriptionToken = name.Replace(" ", "").ToUpper() + "_DESC";
            CursingEyeItemDef.loreToken = name.Replace(" ", "").ToUpper() + "_LORE";
            switch (SkullGammaGun.SkullGammaGunItemDef.tier)
            {
                case ItemTier.Tier1:
                    CursingEyeItemDef.deprecatedTier = ItemTier.VoidTier1;
                    break;
                case ItemTier.Tier2:
                    CursingEyeItemDef.deprecatedTier = ItemTier.VoidTier2;
                    break;
                case ItemTier.Tier3:
                    CursingEyeItemDef.deprecatedTier = ItemTier.VoidTier3;
                    break;

            }
            CursingEyeItemDef.pickupIconSprite = CursingEyeIcon;
            CursingEyeItemDef.pickupModelPrefab = CursingEyePrefab;
            CursingEyeItemDef.canRemove = false;
            CursingEyeItemDef.hidden = false;
            CursingEyeItemDef.requiredExpansion = CaeliImperiumExpansionDef;
            var tags = new List<ItemTag>() { ItemTag.Utility };
            if (ConfigBool(CursingEyeAIBlacklist, CursingEyeEnableConfig))
            {
                tags.Add(ItemTag.AIBlacklist);
            }
            CursingEyeItemDef.tags = tags.ToArray();
            ItemDisplayRuleDict rules = new ItemDisplayRuleDict();
            var displayRules = new ItemDisplayRuleDict(null);
            ItemAPI.Add(new CustomItem(CursingEyeItemDef, displayRules));
            On.RoR2.CharacterBody.OnInventoryChanged += Beeehaviour;
        }

        private static void Beeehaviour(On.RoR2.CharacterBody.orig_OnInventoryChanged orig, CharacterBody self)
        {
            if (NetworkServer.active)
            {
                self.AddItemBehavior<CursingEyeBehaviour>(self.inventory.GetItemCount(CursingEyeItemDef));
            }
            orig(self);

        }
        public class CursingEyeBehaviour : RoR2.CharacterBody.ItemBehavior
        {
            //[BaseItemBodyBehavior.ItemDefAssociationAttribute(useOnServer = true, useOnClient = false)]
            private BuffIndex[] debuffs = new BuffIndex[0];
            private float timer1 = ConfigFloat(CursingEyeInterval, CursingEyeEnableConfig);
            private bool isWorking;

            private static ItemDef GetItemDef()
            {
                return SkullGammaGunItemDef;
            }
            private void Awake()
            {
                base.enabled = false;
                
            }

            private void OnEnable()
            {
                //base.enabled = true;
                debuffs = BuffCatalog.debuffBuffIndices;
                
                var debuffsList = debuffs.ToList();
                debuffsList.Remove(RoR2Content.Buffs.PermanentCurse.buffIndex);
                debuffsList.Remove(RoR2Content.Buffs.NullifyStack.buffIndex);
                debuffsList.Remove(DLC2Content.Buffs.SoulCost.buffIndex);
                debuffsList.Remove(RoR2Content.Buffs.PulverizeBuildup.buffIndex);
                debuffsList.Remove(RoR2Content.Buffs.LunarDetonationCharge.buffIndex);
                if (OtherworldlyManuscript.OtherworldlyManuscriptEnable.Value)
                {
                debuffsList.Remove(Buffs.TalismanVictimBuff.TalismanVictimBuffDef.buffIndex);

                }
                debuffs = debuffsList.ToArray();
            }

            private void FixedUpdate()
            {
                if (!NetworkServer.active)
                {
                    return;
                }
                int stack = this.stack;
                if (stack > 0)
                {
                    Ray ray = new Ray(body.corePosition, body.inputBank.aimDirection);
                    RaycastHit raycastHit = new RaycastHit();
                    if (Physics.Raycast(body.corePosition, body.inputBank.aimDirection, out raycastHit, 30000f, LayerIndex.world.mask | LayerIndex.entityPrecise.mask, QueryTriggerInteraction.Ignore))
                    {
                        CharacterBody target = Util.HurtBoxColliderToBody(raycastHit.collider) ? Util.HurtBoxColliderToBody(raycastHit.collider) : null;
                        if (target && target.teamComponent.teamIndex != body.teamComponent.teamIndex)
                        {
                            timer1 -= Time.fixedDeltaTime;
                        }
                        else if (timer1 < ConfigFloat(CursingEyeInterval, CursingEyeEnableConfig))
                        {
                            timer1 += Time.fixedDeltaTime / 2;

                        }
                        
                        //Debug.Log(timer1);
                        if (timer1 < 0f)
                        {
                            //Debug.Log(raycastHit.collider);
                            //Debug.Log(target);
                            if (target)
                            {
                                for (int i = 0; i < StackInt(ConfigInt(CursingEyeDebuff, CursingEyeEnableConfig), ConfigInt(CursingEyeDebuffStack, CursingEyeEnableConfig), stack); i++)
                                {
                                    var index = UnityEngine.Random.Range(0, debuffs.Length);
                                    target.AddTimedBuff(debuffs[index], StackFloat(ConfigFloat(CursingEyeDuration, CursingEyeEnableConfig), ConfigFloat(CursingEyeDurationStack, CursingEyeEnableConfig), stack));
                                }
                                
                            }
                            
                            timer1 = ConfigFloat(CursingEyeInterval, CursingEyeEnableConfig); 
                        }
                    }
                    else if (timer1 < ConfigFloat(CursingEyeInterval, CursingEyeEnableConfig))
                    {
                        timer1 += Time.fixedDeltaTime / 2;
                    }
                    
                }
            }
        }
        private static void AddLanguageTokens()
        {
            string debuffStack = "";
            if (ConfigInt(CursingEyeDebuffStack, CursingEyeEnableConfig) > 0)
            {
                debuffStack = " <style=cStack>(+" + ConfigInt(CursingEyeDebuffStack, CursingEyeEnableConfig) + " per item stack)</style>";
            }
            string durationStack = "";
            if (ConfigFloat(CursingEyeDurationStack, CursingEyeEnableConfig) > 0)
            {
                durationStack = " <style=cStack>(+" + ConfigFloat(CursingEyeDurationStack, CursingEyeEnableConfig) + " per item stack)</style>";
            }
            LanguageAPI.Add(name.Replace(" ", "").ToUpper() + "_NAME", name);
            LanguageAPI.Add(name.Replace(" ", "").ToUpper() + "_PICKUP", "Your gaze curses enemies. <style=cIsVoid>Corrupts all Skull Gamma Gun</style>");
            LanguageAPI.Add(name.Replace(" ", "").ToUpper() + "_DESC", "Enemy under your look receives <style=cIsUtility>" + ConfigInt(CursingEyeDebuff, CursingEyeEnableConfig) + debuffStack + " random debuff</style> for " + ConfigFloat(CursingEyeDuration, CursingEyeEnableConfig) + durationStack + " seconds. <style=cIsVoid>Corrupts all Skull Gamma Gun</style>");
            LanguageAPI.Add(name.Replace(" ", "").ToUpper() + "_LORE", "");
        }
    }
}
