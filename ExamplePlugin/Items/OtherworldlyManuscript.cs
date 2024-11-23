using BepInEx.Configuration;
using R2API;
using RiskOfOptions.OptionConfigs;
using RiskOfOptions.Options;
using RiskOfOptions;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UIElements;
using static ReignFromGreatBeyondPlugin.CaeliImperium;
using CaeliImperium.Buffs;
using UnityEngine.Networking;

namespace CaeliImperium.Items
{
    public static class OtherworldlyManuscript
    {


        internal static GameObject OtherworldlyManuscriptPrefab;
        internal static GameObject TalismanExplosionPrefab;
        internal static Sprite OtherworldlyManuscriptIcon;
        public static ItemDef OtherworldlyManuscriptItemDef;
        public static ConfigEntry<bool> OtherworldlyManuscriptEnable;
        public static ConfigEntry<bool> OtherworldlyManuscriptAIBlacklist;
        public static ConfigEntry<float> OtherworldlyManuscriptTier;
        public static ConfigEntry<float> OtherworldlyManuscriptTalismanedTimer;
        public static ConfigEntry<int> OtherworldlyManuscriptTalismanedCount;
        public static ConfigEntry<float> OtherworldlyManuscriptCooldown;
        public static ConfigEntry<float> OtherworldlyManuscriptDamageMultiplier;
        public static ConfigEntry<int> OtherworldlyManuscriptCurseAmount;
        private static GameObject TalismanPrefab;

        internal static void Init()
        {
            AddConfigs();
            string tier = "Assets/Icons/OtherworldlyManuscript.png";
            switch (OtherworldlyManuscriptTier.Value)
            {
                case 1:
                    tier = "Assets/Icons/OtherworldlyManuscriptTier1.png";
                    break;
                case 2:
                    tier = "Assets/Icons/OtherworldlyManuscript.png";
                    break;
                case 3:
                    tier = "Assets/Icons/OtherworldlyManuscriptTier3.png";
                    break;

            }
            OtherworldlyManuscriptPrefab = MainAssets.LoadAsset<GameObject>("Assets/Models/Prefabs/Talisman.prefab");
            TalismanExplosionPrefab = MainAssets.LoadAsset<GameObject>("Assets/VFX/TalismanExplosion.prefab");
            OtherworldlyManuscriptIcon = MainAssets.LoadAsset<Sprite>(tier);
            TalismanPrefab = MainAssets.LoadAsset<GameObject>("Assets/Models/Prefabs/TalismanWorldModel.prefab");
            if (!OtherworldlyManuscriptEnable.Value)
            {
                return;
            }
            Item();

            AddLanguageTokens();
            TalismanAttackerBuff.Init();
            TalismanCooldownBuff.Init();
            TalismanVictimBuff.Init();
        }

        public static void AddConfigs()
        {
            OtherworldlyManuscriptEnable = Config.Bind<bool>("Item : Otherworldly Manuscript",
                             "Activation",
                             true,
                             "Enable Otherworldly Manuscript item?");
            OtherworldlyManuscriptAIBlacklist = Config.Bind<bool>("Item : Otherworldly Manuscript",
                 "AI Blacklist",
                 true,
                 "Blacklist this item from enemies?");
            OtherworldlyManuscriptTier = Config.Bind<float>("Item : Otherworldly Manuscript",
                                         "Item tier",
                                         2f,
                                         "1: Common/White\n2: Rare/Green\n3: Legendary/Red");
            OtherworldlyManuscriptTalismanedTimer = Config.Bind<float>("Item : Otherworldly Manuscript",
                                         "Talismanned timer",
                                         5f,
                                         "Controll the timer of a talismanned debuff");
            OtherworldlyManuscriptTalismanedCount = Config.Bind<int>("Item : Otherworldly Manuscript",
                                         "Talismanned needed to activate item",
                                         7,
                                         "Controll how much talismanned debuff needed to proc this item");
            OtherworldlyManuscriptCooldown = Config.Bind<float>("Item : Otherworldly Manuscript",
                                         "Cooldown",
                                         10f,
                                         "Controll the cooldown of this item");
            OtherworldlyManuscriptDamageMultiplier = Config.Bind<float>("Item : Otherworldly Manuscript",
                                         "Damage Multiplier",
                                         1f,
                                         "Controll the damage multiplier of this item explosion");
            OtherworldlyManuscriptCurseAmount = Config.Bind<int>("Item : Otherworldly Manuscript",
                                         "Wound amount",
                                         1,
                                         "Controll how much Wounds the explosion applies per item stack");
            ModSettingsManager.AddOption(new CheckBoxOption(OtherworldlyManuscriptEnable, new CheckBoxConfig() { restartRequired = true }));
            ModSettingsManager.AddOption(new CheckBoxOption(OtherworldlyManuscriptAIBlacklist, new CheckBoxConfig() { restartRequired = true }));
            ModSettingsManager.AddOption(new StepSliderOption(OtherworldlyManuscriptTier, new StepSliderConfig() { min = 1, max = 3, increment = 1f, restartRequired = true }));
            ModSettingsManager.AddOption(new FloatFieldOption(OtherworldlyManuscriptTalismanedTimer));
            ModSettingsManager.AddOption(new IntFieldOption(OtherworldlyManuscriptTalismanedCount));
            ModSettingsManager.AddOption(new FloatFieldOption(OtherworldlyManuscriptCooldown));
            ModSettingsManager.AddOption(new FloatFieldOption(OtherworldlyManuscriptDamageMultiplier));
            ModSettingsManager.AddOption(new IntFieldOption(OtherworldlyManuscriptCurseAmount));
        }

        private static void Item()
        {
            OtherworldlyManuscriptItemDef = ScriptableObject.CreateInstance<ItemDef>();
            OtherworldlyManuscriptItemDef.name = "OtherworldlyManuscript";
            OtherworldlyManuscriptItemDef.nameToken = "OTHERWORLDLYMANUSCRIPT_NAME";
            OtherworldlyManuscriptItemDef.pickupToken = "OTHERWORLDLYMANUSCRIPT_PICKUP";
            OtherworldlyManuscriptItemDef.descriptionToken = "OTHERWORLDLYMANUSCRIPT_DESC";
            OtherworldlyManuscriptItemDef.loreToken = "OTHERWORLDLYMANUSCRIPT_LORE";
            switch (OtherworldlyManuscriptTier.Value)
            {
                case 1:
                    OtherworldlyManuscriptItemDef.deprecatedTier = ItemTier.Tier1;
                    break;
                case 2:
                    OtherworldlyManuscriptItemDef.deprecatedTier = ItemTier.Tier2;
                    break;
                case 3:
                    OtherworldlyManuscriptItemDef.deprecatedTier = ItemTier.Tier3;
                    break;

            }
            OtherworldlyManuscriptItemDef.pickupIconSprite = OtherworldlyManuscriptIcon;
            OtherworldlyManuscriptItemDef.pickupModelPrefab = OtherworldlyManuscriptPrefab;
            OtherworldlyManuscriptItemDef.canRemove = true;
            OtherworldlyManuscriptItemDef.hidden = false;
            OtherworldlyManuscriptItemDef.requiredExpansion = CaeliImperiumExpansionDef;
            var tags = new List<ItemTag>() { ItemTag.Damage };
            if (OtherworldlyManuscriptAIBlacklist.Value)
            {
                tags.Add(ItemTag.AIBlacklist);
            }
            OtherworldlyManuscriptItemDef.tags = tags.ToArray();
            ItemDisplayRuleDict rules = new ItemDisplayRuleDict();
            rules.Add("mdlCommandoDualies", new RoR2.ItemDisplayRule[]{
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = TalismanPrefab,
                    childName = "Chest",
localPos = new Vector3(0F, 0.18069F, 0.20983F),
localAngles = new Vector3(0F, 0F, 0F),
localScale = new Vector3(0.0337F, 0.0337F, 0.0337F)
                }
            });
            rules.Add("mdlHuntress", new RoR2.ItemDisplayRule[]{
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = TalismanPrefab,
                    childName = "Chest",
localPos = new Vector3(0.00001F, 0.17083F, 0.16833F),
localAngles = new Vector3(327.056F, 0F, 0F),
localScale = new Vector3(0.035F, 0.035F, 0.035F)
                }
            });
            rules.Add("mdlBandit2", new RoR2.ItemDisplayRule[]{
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = TalismanPrefab,
                    childName = "Chest",
localPos = new Vector3(0F, 0.24915F, 0.15171F),
localAngles = new Vector3(337.9442F, 0F, 0F),
localScale = new Vector3(0.06695F, 0.06695F, 0.06695F)
                }
            });
            rules.Add("mdlToolbot", new RoR2.ItemDisplayRule[]{
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = TalismanPrefab,
                    childName = "Chest",
localPos = new Vector3(1.0727F, 1.18914F, 3.26753F),
localAngles = new Vector3(0F, 0F, 0F),
localScale = new Vector3(0.238F, 0.238F, 0.25784F)
                }
            });
            rules.Add("mdlEngi", new RoR2.ItemDisplayRule[]{
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = TalismanPrefab,
                    childName = "Chest",
localPos = new Vector3(0F, 0.08648F, 0.27481F),
localAngles = new Vector3(25.31622F, 0F, 0F),
localScale = new Vector3(0.06543F, 0.06543F, 0.06543F)
                }
            });
            rules.Add("mdlEngiTurret", new RoR2.ItemDisplayRule[]{
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = TalismanPrefab,
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
                    followerPrefab = TalismanPrefab,
                    childName = "Chest",
localPos = new Vector3(0F, 0.13571F, 0.13325F),
localAngles = new Vector3(331.5589F, 0F, 0F),
localScale = new Vector3(0.05483F, 0.05483F, 0.05483F)
                }
            });
            rules.Add("mdlMerc", new RoR2.ItemDisplayRule[]{
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = TalismanPrefab,
                    childName = "Chest",
localPos = new Vector3(0F, 0.08189F, 0.21372F),
localAngles = new Vector3(350.2855F, 0F, 0F),
localScale = new Vector3(0.0526F, 0.0526F, 0.0526F)
                }
            });
            rules.Add("mdlTreebot", new RoR2.ItemDisplayRule[]{
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = TalismanPrefab,
                    childName = "Eye",
localPos = new Vector3(0F, 0.74757F, 0.00002F),
localAngles = new Vector3(270F, 0F, 0F),
localScale = new Vector3(0.18672F, 0.18672F, 0.18672F)
                }
            });
            rules.Add("mdlLoader", new RoR2.ItemDisplayRule[]{
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = TalismanPrefab,
                    childName = "Chest",
localPos = new Vector3(0F, 0.17583F, 0.18136F),
localAngles = new Vector3(346.8075F, 0F, 0F),
localScale = new Vector3(0.07196F, 0.07196F, 0.07196F)
                }
            });
            rules.Add("mdlCroco", new RoR2.ItemDisplayRule[]{
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = TalismanPrefab,
                    childName = "Chest",
localPos = new Vector3(0F, -0.16351F, -2.3261F),
localAngles = new Vector3(0F, 0F, 0F),
localScale = new Vector3(0.49266F, 0.49266F, 0.49266F)
                }
            });
            rules.Add("mdlCaptain", new RoR2.ItemDisplayRule[]{
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = TalismanPrefab,
                    childName = "Chest",
localPos = new Vector3(0.00001F, 0.18152F, 0.19409F),
localAngles = new Vector3(350.7645F, 0F, 0F),
localScale = new Vector3(0.08584F, 0.08584F, 0.08584F)
                }
            });
            rules.Add("mdlRailGunner", new RoR2.ItemDisplayRule[]{
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = TalismanPrefab,
                    childName = "Chest",
localPos = new Vector3(-0.00001F, 0.02366F, 0.1653F),
localAngles = new Vector3(0F, 0F, 0F),
localScale = new Vector3(0.06648F, 0.06648F, 0.07401F)
                }
            });
            rules.Add("mdlVoidSurvivor", new RoR2.ItemDisplayRule[]{
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = TalismanPrefab,
                    childName = "Chest",
localPos = new Vector3(0.00001F, 0.00099F, 0.20308F),
localAngles = new Vector3(8.06209F, 0F, 0F),
localScale = new Vector3(0.05532F, 0.05532F, 0.05532F)
                }
            });
            rules.Add("mdlSeeker", new RoR2.ItemDisplayRule[]{
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = TalismanPrefab,
                    childName = "Chest",
localPos = new Vector3(0F, 0.14973F, 0.06963F),
localAngles = new Vector3(329.1524F, 0F, 0F),
localScale = new Vector3(0.06904F, 0.06904F, 0.06904F)
                }
            });
            rules.Add("mdlChef", new RoR2.ItemDisplayRule[]{
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = TalismanPrefab,
                    childName = "Base",
localPos = new Vector3(-0.02953F, 0F, 0F),
localAngles = new Vector3(0F, 90F, 0F),
localScale = new Vector3(0.16626F, 0.16626F, 0.16626F)
                }
            });
            rules.Add("mdlFalseSon", new RoR2.ItemDisplayRule[]{
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = TalismanPrefab,
                    childName = "Chest",
localPos = new Vector3(0F, 0F, 0.26073F),
localAngles = new Vector3(0F, 0F, 0F),
localScale = new Vector3(0.06222F, 0.06222F, 0.06441F)
                }
            });
            rules.Add("mdlScav", new RoR2.ItemDisplayRule[]{
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = TalismanPrefab,
                    childName = "Chest",
localPos = new Vector3(0F, 5.33682F, -5.91645F),
localAngles = new Vector3(42.51031F, 0F, 0F),
localScale = new Vector3(1.44492F, 1.44492F, 1.44492F)
                }
            });
            var displayRules = new ItemDisplayRuleDict(null);
            ItemAPI.Add(new CustomItem(OtherworldlyManuscriptItemDef, rules));
            On.RoR2.GlobalEventManager.OnHitEnemy += GlobalEventManager_OnHitEnemy;
            On.RoR2.CharacterBody.OnInventoryChanged += GainBuff;
            //On.RoR2.CharacterBody.Start += ReAddBuff;
        }
        public class OtherworldlyManuscriptBehaviour : RoR2.CharacterBody.ItemBehavior
        {
            private void Awake()
            {
                base.enabled = false;
            }
            public void FixedUpdate()
            {
                if (body)
                {
                    int count = stack;
                    if (count > 0 && !body.HasBuff(TalismanAttackerBuff.TalismanAttackerBuffDef) && !body.HasBuff(TalismanCooldownBuff.TalismanCooldownBuffDef))
                    {
                body.AddBuff(TalismanAttackerBuff.TalismanAttackerBuffDef);

                    }
                }
            }
            public void OnDisable()
            {
                if (body)
                {
                    body.RemoveBuff(TalismanAttackerBuff.TalismanAttackerBuffDef);
                    body.RemoveBuff(TalismanCooldownBuff.TalismanCooldownBuffDef);
                }
            }
        }
        /*
        private static void ReAddBuff(On.RoR2.CharacterBody.orig_Start orig, CharacterBody self)
        {
            orig(self);
            int itemCount = self.inventory ? self.inventory.GetItemCount(OtherworldlyManuscriptItemDef) : 0;

            if (!self.HasBuff(TalismanAttackerBuff.TalismanAttackerBuffDef) && !self.HasBuff(TalismanCooldownBuff.TalismanCooldownBuffDef) && itemCount > 0)
            {
                self.AddBuff(TalismanAttackerBuff.TalismanAttackerBuffDef);
            }
        }*/

        private static void GainBuff(On.RoR2.CharacterBody.orig_OnInventoryChanged orig, CharacterBody self)
        {
            orig(self);
            if (NetworkServer.active)
            {
int count = self.inventory ? self.inventory.GetItemCount(OtherworldlyManuscriptItemDef) : 0;
            self.AddItemBehavior<OtherworldlyManuscriptBehaviour>(count);
            }
            
            /*if (count > 0 && !self.HasBuff(Buffs.TalismanAttackerBuff.TalismanAttackerBuffDef) && !self.HasBuff(Buffs.TalismanCooldownBuff.TalismanCooldownBuffDef))
            {
                self.AddBuff(Buffs.TalismanAttackerBuff.TalismanAttackerBuffDef);
            }*/
        }
        
        private static void GlobalEventManager_OnHitEnemy(On.RoR2.GlobalEventManager.orig_OnHitEnemy orig, GlobalEventManager self, DamageInfo damageInfo, GameObject victim)
        {
            orig(self, damageInfo, victim);
            var attacker = damageInfo.attacker;
            var body = attacker ? attacker.GetComponent<CharacterBody>() : null;
            int count = body.inventory ? body.inventory.GetItemCount(OtherworldlyManuscriptItemDef) : 0 ;
            if (count > 0 && (body ? body.HasBuff(Buffs.TalismanAttackerBuff.TalismanAttackerBuffDef) : false))
            {
                if (damageInfo.attacker)
                {
                    var victimBody = victim.GetComponent<CharacterBody>();
                    victimBody.AddTimedBuff(Buffs.TalismanVictimBuff.TalismanVictimBuffDef, OtherworldlyManuscriptTalismanedTimer.Value);
                    if (victimBody.GetBuffCount(Buffs.TalismanVictimBuff.TalismanVictimBuffDef) >= OtherworldlyManuscriptTalismanedCount.Value)
                    {
                        victimBody.SetBuffCount(Buffs.TalismanVictimBuff.TalismanVictimBuffDef.buffIndex, 0);
                        body.SetBuffCount(Buffs.TalismanAttackerBuff.TalismanAttackerBuffDef.buffIndex, 0);
                        body.AddTimedBuff(Buffs.TalismanCooldownBuff.TalismanCooldownBuffDef, OtherworldlyManuscriptCooldown.Value);
                        float damage = Util.OnHitProcDamage(damageInfo.damage, body.damage, count * OtherworldlyManuscriptDamageMultiplier.Value);
                        Vector3 position2 = damageInfo.position;
                        ProcChainMask procChainMask5 = damageInfo.procChainMask;
                        procChainMask5.AddProc(ProcType.Rings);
                        DamageInfo damageInfo2 = new DamageInfo
                        {
                            damage = damage,
                            damageColorIndex = DamageColorIndex.Item,
                            damageType = DamageType.Generic,
                            attacker = attacker,
                            crit = damageInfo.crit,
                            force = Vector3.zero,
                            inflictor = null,
                            position = position2,
                            procChainMask = procChainMask5,
                            procCoefficient = 1f
                        };

                        for (int i = 0; i < count * OtherworldlyManuscriptCurseAmount.Value; i++)
                        {
                        victimBody.AddBuff(WoundedBuff.WoundedBuffDef);
                        }
                        
                        EffectManager.SimpleImpactEffect(LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/ImpactEffects/IceRingExplosion"), position2, Vector3.up, true);
                        victimBody.healthComponent.TakeDamage(damageInfo2);
                    }
                }
            }
        }

        private static void AddLanguageTokens()
        {
            LanguageAPI.Add("OTHERWORLDLYMANUSCRIPT_NAME", "Otherworldly Manuscript");
            LanguageAPI.Add("OTHERWORLDLYMANUSCRIPT_PICKUP", "Hitting enemies continually " + OtherworldlyManuscriptTalismanedCount.Value + " times in a span of " + OtherworldlyManuscriptTalismanedTimer.Value + " seconds will make them explode for " + OtherworldlyManuscriptDamageMultiplier.Value * 100 + "% (+" + OtherworldlyManuscriptDamageMultiplier.Value * 100 + "% per item stack) TOTAL damage and applies " + OtherworldlyManuscriptCurseAmount.Value + " (+" + OtherworldlyManuscriptCurseAmount.Value + " per item stack) amount of Wounds");
            LanguageAPI.Add("OTHERWORLDLYMANUSCRIPT_DESC", "Hitting enemies continually " + OtherworldlyManuscriptTalismanedCount.Value + " times in a span of " + OtherworldlyManuscriptTalismanedTimer.Value + " seconds will make them explode for " + OtherworldlyManuscriptDamageMultiplier.Value * 100 + "% (+" + OtherworldlyManuscriptDamageMultiplier.Value * 100 + "% per item stack) TOTAL damage and applies " + OtherworldlyManuscriptCurseAmount.Value + " (+" + OtherworldlyManuscriptCurseAmount.Value + " per item stack) amount of Wounds");
            LanguageAPI.Add("OTHERWORLDLYMANUSCRIPT_LORE", "None Sols");
        }
    }

}
