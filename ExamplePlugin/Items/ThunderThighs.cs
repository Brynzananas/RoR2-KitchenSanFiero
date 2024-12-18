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
    internal static class ThunderThighs //: ItemBase<FirstItem>
    {
        internal static GameObject ThunderThighsPrefab;
        internal static Sprite ThunderThighsIcon;
        public static ItemDef ThunderThighsItemDef;
        public static string name = "RJ360 Prototype";
        public static ConfigEntry<bool> ThunderThigsEnable;
        public static ConfigEntry<bool> ThunderThigsEnableConfig;
        public static ConfigEntry<bool> ThunderThighsAIBlacklist;
        public static ConfigEntry<float> ThunderThighsTier;
        public static ConfigEntry<float> ThunderThighsDamage;
        public static ConfigEntry<float> ThunderThighsRadius;

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
                             false,
                             "Blacklist this item from enemies?");
            ThunderThighsTier = Config.Bind<float>("Item : " + name,
            "Item tier",
            3f,
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
            var tags = new List<ItemTag>() { ItemTag.Damage };
            if (ConfigBool(ThunderThighsAIBlacklist, ThunderThigsEnableConfig))
            {
                tags.Add(ItemTag.AIBlacklist);
            }
            ThunderThighsItemDef.tags = tags.ToArray();
            var displayRules = new ItemDisplayRuleDict(null);
            ItemAPI.Add(new CustomItem(ThunderThighsItemDef, displayRules));
            On.RoR2.CharacterBody.OnSkillActivated += Explode;
        }
        public static void RocketJump(CharacterBody self)
        {
            self.transform.position += self.transform.up.normalized;
            if (self.characterMotor.isAirControlForced == false)
            {
                self.characterMotor.isAirControlForced = true;
            }
            self.characterMotor.Jump(3, 3);
            if (self.characterMotor.jumpCount < 2)
            {
            self.AddBuff(Buffs.RocketJumpBuff.RocketJumpBuffDef);

            }
        }
        private static void Explode(On.RoR2.CharacterBody.orig_OnSkillActivated orig, CharacterBody self, GenericSkill skill)
        {
            orig(self, skill);
            if (self.skillLocator.utility && skill == self.skillLocator.utility)
            {
                int itemCount = self.inventory ? self.inventory.GetItemCount(ThunderThighsItemDef) : 0;
                if (itemCount > 0)
                {
                    if (self.GetBuffCount(Buffs.RocketJumpBuff.RocketJumpBuffDef) < itemCount && self.inputBank.jump.down) // && self.inputBank.aimDirection.y <= -0.75 | self.inputBank.aimDirection.y >= 0.75)
                    {
                        RocketJump(self);
                    }
                    float num = 12f;
                    float damageCoefficient = 0.6f;
                    float baseDamage = self.damage * itemCount * skill.baseRechargeInterval;
                    EffectManager.SpawnEffect(LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/OmniEffect/OmniExplosionVFXQuick"), new EffectData
                    {
                        origin = self.footPosition,
                        scale = num,
                        rotation = Util.QuaternionSafeLookRotation(self.transform.up)
                    }, true);
                    BlastAttack blastAttack = new BlastAttack();
                    blastAttack.position = self.footPosition;
                    blastAttack.baseDamage = baseDamage;
                    blastAttack.baseForce = 0f;
                    blastAttack.radius = num;
                    blastAttack.attacker = self.gameObject;
                    blastAttack.inflictor = null;
                    blastAttack.teamIndex = TeamComponent.GetObjectTeam(blastAttack.attacker);
                    blastAttack.crit = Util.CheckRoll(self.crit);
                    blastAttack.procChainMask = default;
                    blastAttack.procCoefficient = 0f;
                    blastAttack.damageColorIndex = DamageColorIndex.Item;
                    blastAttack.falloffModel = BlastAttack.FalloffModel.None;
                    blastAttack.damageType = DamageType.AOE;
                    blastAttack.Fire();
                }
            }
        }

        private static void AddLanguageTokens()
        {
            LanguageAPI.Add(name.Replace(" ", "").ToUpper() + "_NAME", name);
            LanguageAPI.Add(name.Replace(" ", "").ToUpper() + "_PICKUP", "Rocket jump!");
            LanguageAPI.Add(name.Replace(" ", "").ToUpper() + "_DESC", "On utility skill use, explode for some damage. Rocket jump by pressing utility skill while holding jump button");
            LanguageAPI.Add(name.Replace(" ", "").ToUpper() + "_LORE", "");
        }
    }
}
