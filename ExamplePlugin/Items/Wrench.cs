using BepInEx.Configuration;
using IL.RoR2.Items;
using CaeliImperium.Buffs;
using R2API;
using RiskOfOptions.OptionConfigs;
using RiskOfOptions.Options;
using RiskOfOptions;
using RoR2;
using RoR2.Items;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using static CaeliImperiumPlugin.CaeliImperium;

namespace CaeliImperium.Items
{
    public static class Wrench
    {
        internal static GameObject WrenchPrefab;
        internal static Sprite WrenchIcon;
        public static ItemDef WrenchItemDef;

        public static ConfigEntry<bool> BrassBellAIBlacklist;
        public static ConfigEntry<float> BrassBellTier;

        internal static void Init()
        {
            AddConfigs();

            string tier = "Assets/Icons/BrassBellIcon.png";
            switch (BrassBellTier.Value)
            {
                case 1:
                    tier = "Assets/Icons/BrassBellIconTier1.png";
                    break;
                case 2:
                    tier = "Assets/Icons/BrassBellIcon.png";
                    break;
                case 3:
                    tier = "Assets/Icons/BrassBellIconTier3.png";
                    break;

            }
            WrenchPrefab = MainAssets.LoadAsset<GameObject>("Assets/Models/Prefabs/BrassBell.prefab");
            WrenchIcon = MainAssets.LoadAsset<Sprite>(tier);
            Item();

            AddLanguageTokens();
            RepairBuff.Init();
            UpgradedBuff.Init();
            UpgradedLevelBuff.Init();
        }
        public static void AddConfigs()
        {

            BrassBellAIBlacklist = Config.Bind<bool>("Item : Wrench",
                                         "AI Blacklist",
                                         false,
                                         "Blacklist this item from enemies?");
            BrassBellTier = Config.Bind<float>("Item : Wrench",
                                         "Item tier",
                                         2f,
                                         "1: Common/White\n2: Rare/Green\n3: Legendary/Red");

            ModSettingsManager.AddOption(new CheckBoxOption(BrassBellAIBlacklist, new CheckBoxConfig() { restartRequired = true }));
            ModSettingsManager.AddOption(new StepSliderOption(BrassBellTier, new StepSliderConfig() { min = 1, max = 3, increment = 1f, restartRequired = true }));
        }
        public static void Item()
        {

            WrenchItemDef = ScriptableObject.CreateInstance<ItemDef>();
            WrenchItemDef.name = "Wrench";
            WrenchItemDef.nameToken = "WRENCH_NAME";
            WrenchItemDef.pickupToken = "WRENCH_PICKUP";
            WrenchItemDef.descriptionToken = "WRENCH_DESC";
            WrenchItemDef.loreToken = "WRENCH_LORE";
            switch (BrassBellTier.Value)
            {
                case 1:
                    WrenchItemDef.deprecatedTier = ItemTier.Tier1;
                    break;
                case 2:
                    WrenchItemDef.deprecatedTier = ItemTier.Tier2;
                    break;
                case 3:
                    WrenchItemDef.deprecatedTier = ItemTier.Tier3;
                    break;

            }
            WrenchItemDef.pickupIconSprite = WrenchIcon;
            WrenchItemDef.pickupModelPrefab = WrenchPrefab;
            WrenchItemDef.canRemove = true;
            WrenchItemDef.hidden = false;
            var tags = new List<ItemTag>() { ItemTag.Damage };
            if (BrassBellAIBlacklist.Value)
            {
                tags.Add(ItemTag.AIBlacklist);
            }
            WrenchItemDef.tags = tags.ToArray();
            var displayRules = new ItemDisplayRuleDict(null);
            ItemAPI.Add(new CustomItem(WrenchItemDef, displayRules));
            On.RoR2.GlobalEventManager.OnHitAll += WrenchThem;
        }

        private static void WrenchThem(On.RoR2.GlobalEventManager.orig_OnHitAll orig, GlobalEventManager self, DamageInfo damageInfo, GameObject hitObject)
        {
            orig(self, damageInfo, hitObject);
            var body = damageInfo.attacker.GetComponent<CharacterBody>();
            var count = body.inventory ? body.inventory.GetItemCount(WrenchItemDef) : 0;
            if (count > 0)
            {
                Debug.Log(hitObject);
                var victimBody = hitObject.GetComponent<CharacterBody>();
                
                if (victimBody == null)
                {
                    victimBody = Util.HurtBoxColliderToBody(hitObject.GetComponent<Collider>());
                }
                if (victimBody && damageInfo.attacker && victimBody.teamComponent.teamIndex == body.teamComponent.teamIndex)
                {
                    victimBody.healthComponent.Heal(damageInfo.damage / 2 * count, default);
                    if (!victimBody.HasBuff(UpgradedBuff.UpgradedBuffDef))
                    {
                        victimBody.AddTimedBuff(RepairBuff.RepairBuffDef, 10f);
                        
                    EffectManager.SimpleImpactEffect(HealthComponent.AssetReferences.crowbarImpactEffectPrefab, damageInfo.position, -damageInfo.force, true);
                        if (victimBody.GetBuffCount(RepairBuff.RepairBuffDef) > 5)
                        {
                            victimBody.SetBuffCount(RepairBuff.RepairBuffDef.buffIndex, 0);
                            victimBody.AddTimedBuff(UpgradedBuff.UpgradedBuffDef, 10f);
                            victimBody.SetBuffCount(UpgradedLevelBuff.UpgradedLevelBuffDef.buffIndex, count);

                        }
                    }
                }
            }

        }

        public static void AddLanguageTokens()
        {
            LanguageAPI.Add("WRENCH_NAME", "Brass Bell");
            LanguageAPI.Add("WRENCH_PICKUP", "tf2");
            LanguageAPI.Add("WRENCH_DESC", "tf2");
            LanguageAPI.Add("WRENCH_LORE", "mmmm yummy");
        }
    }
}
