using R2API;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine.AddressableAssets;
using UnityEngine;
using static CaeliImperiumPlugin.CaeliImperium;
using RiskOfOptions.OptionConfigs;
using RiskOfOptions.Options;
using RiskOfOptions;
using System.Xml.Linq;
using BepInEx.Configuration;
using System.ComponentModel;

namespace CaeliImperium.Items
{
    internal static class PackOfDynamite
    {
        internal static GameObject ThunderThighsPrefab;
        internal static Sprite ThunderThighsIcon;
        public static ItemDef ThunderThighsItemDef;
        public static string name = "Pack Of Dynamite";
        public static ConfigEntry<bool> ThunderThigsEnable;
        public static ConfigEntry<bool> ThunderThigsEnableConfig;
        public static ConfigEntry<bool> ThunderThighsAIBlacklist;
        public static ConfigEntry<float> ThunderThighsTier;

        internal static void Init()
        {
            AddConfigs();
            string tier = "Assets/Icons/ChalkTier1.png";
            switch (ConfigFloat(ThunderThighsTier, ThunderThigsEnableConfig))
            {
                case 1:
                    tier = "Assets/Icons/ChalkTier1.png";
                    break;
                case 2:
                    tier = "Assets/Icons/ChalkTier2.png";
                    break;
                case 3:
                    tier = "Assets/Icons/ChalkTier3.png";
                    break;

            }
            ThunderThighsPrefab = MainAssets.LoadAsset<GameObject>("Assets/Models/Prefabs/Chalk.prefab");
            ThunderThighsIcon = MainAssets.LoadAsset<Sprite>(tier);
            Item();

            AddLanguageTokens();
            Buffs.RocketJumpBuff.Init();
        }
        public static void AddConfigs()
        {
            ThunderThigsEnable = Config.Bind<bool>("Item : " + name,
            "Activation",
                             true,
                             "Enable this item?");
            ThunderThigsEnableConfig = Config.Bind<bool>("Item : " + name,
                             "Config Activation",
                             false,
                             "Enable config?\nActivation option and |options under these brackets| are always taken in effect");
            ThunderThighsAIBlacklist = Config.Bind<bool>("Item : " + name,
            "AI Blacklist",
                             true,
                             "Blacklist this item from enemies?");
            ThunderThighsTier = Config.Bind<float>("Item : " + name,
            "Item tier",
            1f,
                                         "1: Common/White\n2: Rare/Green\n3: Legendary/Red");
            ModSettingsManager.AddOption(new CheckBoxOption(ThunderThigsEnable, new CheckBoxConfig() { restartRequired = true }));
            ModSettingsManager.AddOption(new CheckBoxOption(ThunderThigsEnableConfig));
            ModSettingsManager.AddOption(new CheckBoxOption(ThunderThighsAIBlacklist, new CheckBoxConfig() { restartRequired = true }));
            ModSettingsManager.AddOption(new StepSliderOption(ThunderThighsTier, new StepSliderConfig() { min = 1, max = 3, increment = 1f, restartRequired = true }));
        }
        private static void Item()
        {
            ThunderThighsItemDef = ScriptableObject.CreateInstance<ItemDef>();
            ThunderThighsItemDef.name = name.Replace(" ", "");
            ThunderThighsItemDef.nameToken = name.Replace(" ", "").ToUpper() + "_NAME";
            ThunderThighsItemDef.pickupToken = name.Replace(" ", "").ToUpper() + "_PICKUP";
            ThunderThighsItemDef.descriptionToken = name.Replace(" ", "").ToUpper() + "_DESC";
            ThunderThighsItemDef.loreToken = name.Replace(" ", "").ToUpper() + "_LORE";
            switch (ConfigFloat(ThunderThighsTier, ThunderThigsEnableConfig))
            {
                case 1:
                    ThunderThighsItemDef.deprecatedTier = ItemTier.Tier1;
                    break;
                case 2:
                    ThunderThighsItemDef.deprecatedTier = ItemTier.Tier2;
                    break;
                case 3:
                    ThunderThighsItemDef.deprecatedTier = ItemTier.Tier3;
                    break;

            }
            ThunderThighsItemDef.pickupIconSprite = ThunderThighsIcon;
            ThunderThighsItemDef.pickupModelPrefab = ThunderThighsPrefab;
            ThunderThighsItemDef.canRemove = true;
            ThunderThighsItemDef.hidden = false;
            ThunderThighsItemDef.requiredExpansion = CaeliImperiumExpansionDef;
            var tags = new List<ItemTag>() { ItemTag.Damage, ItemTag.EquipmentRelated };
            if (ConfigBool(ThunderThighsAIBlacklist, ThunderThigsEnableConfig))
            {
                tags.Add(ItemTag.AIBlacklist);
            }
            ThunderThighsItemDef.tags = tags.ToArray();
            var displayRules = new ItemDisplayRuleDict(null);
            ItemAPI.Add(new CustomItem(ThunderThighsItemDef, displayRules));
            On.RoR2.EquipmentSlot.OnEquipmentExecuted += Explode;
        }

        private static void Explode(On.RoR2.EquipmentSlot.orig_OnEquipmentExecuted orig, EquipmentSlot self)
        {
            orig(self);
            int itemCount = self.inventory ? self.inventory.GetItemCount(ThunderThighsItemDef) : 0;
            if (itemCount > 0)
            {
                List<CharacterBody> enemyList = new List<CharacterBody>();
                foreach (var characterBody in CharacterBody.readOnlyInstancesList)
                {
                    if (characterBody.master && characterBody.teamComponent.teamIndex != self.teamComponent.teamIndex)
                    {
                        enemyList.Add(characterBody);
                    }
                }
                if (enemyList.Count > 0)
                {
                    for (int i = 0; i < itemCount; i++)
                    {
                        int enemyIndex = UnityEngine.Random.Range(0, enemyList.Count);
                        CharacterBody enemyToExplode = enemyList[enemyIndex];
                        float num = 12f;
                        float damageCoefficient = 0.6f;
                        float baseDamage = self.characterBody.damage * 10;
                        EffectManager.SpawnEffect(LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/OmniEffect/OmniExplosionVFXQuick"), new EffectData
                        {
                            origin = enemyToExplode.corePosition,
                            scale = num,
                            rotation = Util.QuaternionSafeLookRotation(self.characterBody.transform.up)
                        }, true);
                        BlastAttack blastAttack = new BlastAttack();
                        blastAttack.position = enemyToExplode.corePosition;
                        blastAttack.baseDamage = baseDamage;
                        blastAttack.baseForce = 0f;
                        blastAttack.radius = num;
                        blastAttack.attacker = self.characterBody.gameObject;
                        blastAttack.inflictor = null;
                        blastAttack.teamIndex = TeamComponent.GetObjectTeam(blastAttack.attacker);
                        blastAttack.crit = Util.CheckRoll(self.characterBody.crit);
                        blastAttack.procChainMask = default;
                        blastAttack.procCoefficient = 1f;
                        blastAttack.damageColorIndex = DamageColorIndex.Item;
                        blastAttack.falloffModel = BlastAttack.FalloffModel.None;
                        blastAttack.damageType = DamageType.AOE;
                        blastAttack.Fire();
                    }
                }
                
            }
        }

        private static void AddLanguageTokens()
        {
            LanguageAPI.Add(name.Replace(" ", "").ToUpper() + "_NAME", name);
            LanguageAPI.Add(name.Replace(" ", "").ToUpper() + "_PICKUP", "");
            LanguageAPI.Add(name.Replace(" ", "").ToUpper() + "_DESC", "");
            LanguageAPI.Add(name.Replace(" ", "").ToUpper() + "_LORE", "");
        }
    }
}
