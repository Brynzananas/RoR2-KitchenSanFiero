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
using AK.Wwise;
using On.RoR2.Items;
using static UnityEditor.Progress;
using HarmonyLib;

namespace CaeliImperium.Items
{
    internal static class TuskOfShifting
    //: ItemBase<FirstItem>
    {
        internal static GameObject TuskOfShiftingPrefab;
        internal static Sprite TuskOfShiftingIcon;
        public static ItemDef TuskOfShiftingItemDef;
        public static ConfigEntry<bool> TuskOfShiftingEnable;
        public static ConfigEntry<bool> TuskOfShiftingEnableConfig;
        public static ConfigEntry<bool> TuskOfShiftingAIBlacklist;
        public static ConfigEntry<float> TuskOfShiftingDamage;
        public static ConfigEntry<float> TuskOfShiftingDamageStack;
        public static ConfigEntry<float> TuskOfShiftingChance;
        public static ConfigEntry<float> TuskOfShiftingChanceStack;
        private static GameObject infestPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/EliteVoid/VoidInfestEffect.prefab").WaitForCompletion();
        private static string name = "Tusk Of Shifting";
        //public static ConfigEntry<bool> PainkillersOnItemPickupEffect;

        internal static void Init()
        {
            AddConfigs();
            string tier = "Assets/Icons/Painkillers.png";
            TuskOfShiftingPrefab = MainAssets.LoadAsset<GameObject>("Assets/Models/Prefabs/TuskOfShifting.prefab");
            TuskOfShiftingIcon = MainAssets.LoadAsset<Sprite>(tier);
            if (!TuskOfShiftingEnable.Value)
            {
                return;
            }
            Item();
            AddLanguageTokens();
        }

        public static void AddConfigs()
        {
            TuskOfShiftingEnable = Config.Bind<bool>("Item : " + name,
                             "Activation",
                             true,
                             "Enable this item?");
            TuskOfShiftingEnableConfig = Config.Bind<bool>("Item : " + name,
                             "Config Activation",
                             false,
                             "Enable config?\nActivation option and |options under these brackets| are always taken in effect");
            TuskOfShiftingAIBlacklist = Config.Bind<bool>("Item : " + name,
                             "AI Blacklist",
                             false,
                             "Blacklist this item from enemies?");
            TuskOfShiftingDamage = Config.Bind<float>("Item : " + name,
                             "Damage",
                             15f,
                             "Control the damage increase in percentage");
            TuskOfShiftingDamageStack = Config.Bind<float>("Item : " + name,
                             "Damage stack",
                             15f,
                             "Control the damage increase per item stack in percentage");
            TuskOfShiftingChance = Config.Bind<float>("Item : " + name,
                             "Voidtouch chance",
                             1f,
                             "Control the voidtouch chance on hit in percentage");
            TuskOfShiftingChanceStack = Config.Bind<float>("Item : " + name,
                             "Voidtouch chance stack",
                             1f,
                             "Control the voidtouch chance on hit increase per item stack in percentage");
            ModSettingsManager.AddOption(new CheckBoxOption(TuskOfShiftingEnable, new CheckBoxConfig() { restartRequired = true }));
            ModSettingsManager.AddOption(new CheckBoxOption(TuskOfShiftingEnableConfig));
            ModSettingsManager.AddOption(new CheckBoxOption(TuskOfShiftingAIBlacklist, new CheckBoxConfig() { restartRequired = true }));
        }

        private static void Item()
        {
            TuskOfShiftingItemDef = ScriptableObject.CreateInstance<ItemDef>();
            TuskOfShiftingItemDef.name = name.Replace(" ", "");
            TuskOfShiftingItemDef.nameToken = name.Replace(" ", "").ToUpper() + "_NAME";
            TuskOfShiftingItemDef.pickupToken = name.Replace(" ", "").ToUpper() + "_PICKUP";
            TuskOfShiftingItemDef.descriptionToken = name.Replace(" ", "").ToUpper() + "_DESC";
            TuskOfShiftingItemDef.loreToken = name.Replace(" ", "").ToUpper() + "_LORE";
            switch (ItemTier.Tier1)
            {
                case ItemTier.Tier1:
                    TuskOfShiftingItemDef.deprecatedTier = ItemTier.VoidTier1;
                    break;
                case ItemTier.Tier2:
                    TuskOfShiftingItemDef.deprecatedTier = ItemTier.VoidTier2;
                    break;
                case ItemTier.Tier3:
                    TuskOfShiftingItemDef.deprecatedTier = ItemTier.VoidTier3;
                    break;

            }
            TuskOfShiftingItemDef.pickupIconSprite = TuskOfShiftingIcon;
            TuskOfShiftingItemDef.pickupModelPrefab = TuskOfShiftingPrefab;
            TuskOfShiftingItemDef.canRemove = false;
            TuskOfShiftingItemDef.hidden = false;
            TuskOfShiftingItemDef.requiredExpansion = CaeliImperiumExpansionDef;
            var tags = new List<ItemTag>() { ItemTag.Damage };
            if (ConfigBool(TuskOfShiftingAIBlacklist, TuskOfShiftingEnableConfig))
            {
                tags.Add(ItemTag.AIBlacklist);
            }
            TuskOfShiftingItemDef.tags = tags.ToArray();
            ItemDisplayRuleDict rules = new ItemDisplayRuleDict();
            var displayRules = new ItemDisplayRuleDict(null);
            ItemAPI.Add(new CustomItem(TuskOfShiftingItemDef, displayRules));
            On.RoR2.GlobalEventManager.OnHitEnemy += Shift;
            On.RoR2.HealthComponent.TakeDamageProcess += Tusk;
            On.RoR2.Items.ContagiousItemManager.Init += VoidPair;
        }

        private static void VoidPair(ContagiousItemManager.orig_Init orig)
        {
            var pair = new ItemDef.Pair[]
                            {
            new ItemDef.Pair
            {
                itemDef1 = RoR2Content.Items.BossDamageBonus,
                itemDef2 = TuskOfShiftingItemDef,
            }
            };


        ItemCatalog.itemRelationships[DLC1Content.ItemRelationshipTypes.ContagiousItem] = ItemCatalog.itemRelationships[DLC1Content.ItemRelationshipTypes.ContagiousItem].AddRangeToArray(pair);

            orig();
        }

        private static void Tusk(On.RoR2.HealthComponent.orig_TakeDamageProcess orig, HealthComponent self, DamageInfo damageInfo)
        {
            if (damageInfo != null && self != null && damageInfo.attacker && self.body)
            {
                var attackerBody = damageInfo.attacker ? damageInfo.attacker.GetComponent<CharacterBody>() : null;
                int itemCount = 0;
                if (attackerBody != null)
                {
                    try
                    {
                        itemCount = attackerBody.inventory ? attackerBody.inventory.GetItemCount(TuskOfShiftingItemDef) : 0;

                    }
                    catch
                    {
                    }
                }

                if (itemCount > 0 && self.body.master && self.body.teamComponent.teamIndex == TeamIndex.Void)
                {
                    damageInfo.damage += damageInfo.damage * StackFloat(ConfigFloat(TuskOfShiftingDamage, TuskOfShiftingEnableConfig), ConfigFloat(TuskOfShiftingDamageStack, TuskOfShiftingEnableConfig), itemCount); 
                }
            }
            orig(self, damageInfo);
        }

        private static void Shift(On.RoR2.GlobalEventManager.orig_OnHitEnemy orig, GlobalEventManager self, DamageInfo damageInfo, GameObject victim)
        {
            orig(self, damageInfo, victim);
            if (damageInfo.attacker && victim)
            {
                CharacterBody attackerBody = damageInfo.attacker.GetComponent<CharacterBody>() ? damageInfo.attacker.GetComponent<CharacterBody>() : null;
                CharacterBody victimBody = victim.GetComponent<CharacterBody>() ? victim.GetComponent<CharacterBody>() : null;
                int itemCount = 0;
                if (victimBody != null)
                {
                    itemCount = attackerBody.inventory ? attackerBody.inventory.GetItemCount(TuskOfShiftingItemDef) : 0;
                }
                if (itemCount > 0 && attackerBody && victimBody && victimBody.master && victimBody.teamComponent.teamIndex != TeamIndex.Void && !victimBody.isPlayerControlled)
                {
                    if (Util.CheckRoll(itemCount * StackFloat(ConfigFloat(TuskOfShiftingChance, TuskOfShiftingEnableConfig), ConfigFloat(TuskOfShiftingChanceStack, TuskOfShiftingEnableConfig), itemCount), attackerBody.master))
                    {
                        victimBody.master.teamIndex = TeamIndex.Void;
                        victimBody.teamComponent.teamIndex = TeamIndex.Void;
                        victimBody.inventory.SetEquipmentIndex(DLC1Content.Equipment.EliteVoidEquipment.equipmentIndex);
                        EffectManager.SimpleImpactEffect(infestPrefab, damageInfo.position, Vector3.up, false);
                    }
                }
            }
        }

        private static void AddLanguageTokens()
        {
            string damageStack = "";
            if (ConfigFloat(TuskOfShiftingDamageStack, TuskOfShiftingEnableConfig) > 0)
            {
                damageStack = " <style=cStack>(+" + ConfigFloat(TuskOfShiftingDamageStack, TuskOfShiftingEnableConfig) + "% per item stack)</style>";
            }
            string chanceStack = "";
            if (ConfigFloat(TuskOfShiftingChanceStack, TuskOfShiftingEnableConfig) > 0)
            {
                chanceStack = " <style=cStack>(+" + ConfigFloat(TuskOfShiftingChanceStack, TuskOfShiftingEnableConfig) + "% per item stack)</style>";
            }
            LanguageAPI.Add(name.Replace(" ", "").ToUpper() + "_NAME", name);
            LanguageAPI.Add(name.Replace(" ", "").ToUpper() + "_PICKUP", "Deal more damage to void enemies. On hit enemies have a chance to be voidtouched. <style=cIsVoid>Corrupts all Armor-Piercing Rounds</style>");
            LanguageAPI.Add(name.Replace(" ", "").ToUpper() + "_DESC", "Deal <style=cIsDamage>" + ConfigFloat(TuskOfShiftingDamage, TuskOfShiftingEnableConfig) + "%</style>" + damageStack + " <style=cIsDamage>more damage</style> to enemies on <style=cArtifact>Void team</style>. <style=cArtifact>" + ConfigFloat(TuskOfShiftingChance, TuskOfShiftingEnableConfig) + "%</style>" + chanceStack + " chance to <style=cArtifact>voidtouch</style> an enemy on hit. <style=cIsVoid>Corrupts all Armor-Piercing Rounds</style>");
            LanguageAPI.Add(name.Replace(" ", "").ToUpper() + "_LORE", "");
        }
    }
}
