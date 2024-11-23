using R2API;
using RoR2;
using RoR2.Items;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Diagnostics;
using static R2API.RecalculateStatsAPI;
using Object = UnityEngine.Object;
using UnityEngine.UIElements;
using UnityEngine.XR;
using static RoR2.MasterSpawnSlotController;
using static ReignFromGreatBeyondPlugin.CaeliImperium;
using RiskOfOptions.Options;
using RiskOfOptions;
using BepInEx.Configuration;
using CaeliImperium.Buffs;
using Rewired;
using RiskOfOptions.OptionConfigs;

namespace CaeliImperium.Elites
{
    public static class Defender
    {
        //public static Color AffixBrassModalityColor = new Color(1f, 0.5f, 0.0f);
        public static EquipmentDef AffixDefenderEquipment;
        public static BuffDef AffixDefenderBuff;
        public static EliteDef AffixDefenderElite;
        public static float healthMult = 4f;
        public static float damageMult = 2f;
        //public static GameObject DroneSupport = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/EliteHaunted/AffixHauntedWard.prefab").WaitForCompletion();
        public static float affixDropChance = 0.00025f;
        private static GameObject DefenderWard = PrefabAPI.InstantiateClone(Addressables.LoadAssetAsync<GameObject>("RoR2/Base/EliteHaunted/AffixHauntedWard.prefab").WaitForCompletion(), "BrassModalityWard");
        private static Material DefenderMat = Addressables.LoadAssetAsync<Material>("RoR2/Base/Common/VFX/matOpaqueDustLarge_BrassContraption_opt.mat").WaitForCompletion();
        private static Texture2D eliteRamp = MainAssets.LoadAsset<Texture2D>("Assets/Textures/defender_ramp.png");
        private static Sprite eliteIcon = MainAssets.LoadAsset<Sprite>("Assets/Icons/guardian_elite_icon.png");
        // RoR2/Base/Common/ColorRamps/texRampWarbanner.png 
        public static ConfigEntry<float> DefenderHealthMult;
        public static ConfigEntry<float> DefenderDamageMult;
        public static ConfigEntry<float> DefenderStunChance;
        public static ConfigEntry<float> DefenderDazzledTime;
        public static ConfigEntry<bool> DefenderEnable;
        public static ConfigEntry<bool> DefenderEnableDamageAbsorb;
        public static ConfigEntry<float> DefenderArmor;
        public static string name = "Defender";
        public static void Init()
        {
            AddConfigs();
            if (!DefenderEnable.Value)
            {
                return;
            }
            DefenderWard.transform.GetChild(0).GetChild(0).GetComponent<MeshRenderer>().material = DefenderMat;
            AddLanguageTokens();
            SetupBuff();
            SetupEquipment();
            SetupElite();
            AddContent();
            EliteRamp.AddRamp(AffixDefenderElite, eliteRamp);
            ContentAddition.AddEquipmentDef(AffixDefenderEquipment);
            On.RoR2.CharacterBody.OnBuffFirstStackGained += CharacterBody_OnBuffFirstStackGained;
            On.RoR2.CharacterBody.OnBuffFinalStackLost += CharacterBody_OnBuffFinalStackLost;
            On.RoR2.CombatDirector.Init += CombatDirector_Init;
            On.RoR2.CharacterBody.OnSkillActivated += OnEnemySkillUse;
            On.RoR2.HealthComponent.TakeDamageProcess += AbsorbDamage;
            GetStatCoefficients += Stats;
        }

        

        private static void AddConfigs()
        {
            DefenderHealthMult = Config.Bind<float>("Elite : " + name,
                                         "Health Multiplier",
                                         4f,
                                         "Control the health multiplier of this elite");
            DefenderDamageMult = Config.Bind<float>("Elite : " + name,
                                         "Damage Multiplier",
                                         2f,
                                         "Control the damage multiplier of this elite");
            DefenderArmor = Config.Bind<float>("Elite : " + name,
                                         "Armor",
                                         500f,
                                         "Control the armor of this elite");
            DefenderStunChance = Config.Bind<float>("Elite : " + name,
                                         "Stun chance",
                                         2f,
                                         "Control the stun chance on enemy skill use");
            DefenderDazzledTime = Config.Bind<float>("Elite : " + name,
                                         "Dazzled time",
                                         5f,
                                         "Control the Dazzled time");

            DefenderEnable = Config.Bind<bool>("Elite : " + name,
                 "Activation",
                 true,
                 "Enable this elite?");
            DefenderEnableDamageAbsorb = Config.Bind<bool>("Elite : " + name,
                 "Damage absorb",
                 true,
                 "Enable damage absorbing?");
            ModSettingsManager.AddOption(new CheckBoxOption(DefenderEnable, new CheckBoxConfig() { restartRequired = true }));
            ModSettingsManager.AddOption(new FloatFieldOption(DefenderHealthMult));
            ModSettingsManager.AddOption(new FloatFieldOption(DefenderDamageMult));
            ModSettingsManager.AddOption(new FloatFieldOption(DefenderArmor));
            ModSettingsManager.AddOption(new FloatFieldOption(DefenderStunChance));
            ModSettingsManager.AddOption(new FloatFieldOption(DefenderDazzledTime));
            ModSettingsManager.AddOption(new CheckBoxOption(DefenderEnableDamageAbsorb));
        }
        private static void CombatDirector_Init(On.RoR2.CombatDirector.orig_Init orig)
        {
            orig();
            if (EliteAPI.VanillaEliteTiers.Length > 2)
            {
                // HONOR
                CombatDirector.EliteTierDef targetTier = EliteAPI.VanillaEliteTiers[2];
                List<EliteDef> elites = targetTier.eliteTypes.ToList();
                AffixDefenderElite.healthBoostCoefficient = DefenderHealthMult.Value / 1.6f;
                AffixDefenderElite.damageBoostCoefficient = DefenderDamageMult.Value / 1.3f;
                elites.Add(AffixDefenderElite);
                targetTier.eliteTypes = elites.ToArray();
            }
            if (EliteAPI.VanillaEliteTiers.Length > 1)
            {
                CombatDirector.EliteTierDef targetTier = EliteAPI.VanillaEliteTiers[1];
                List<EliteDef> elites = targetTier.eliteTypes.ToList();
                AffixDefenderElite.healthBoostCoefficient = DefenderHealthMult.Value;
                AffixDefenderElite.damageBoostCoefficient = DefenderDamageMult.Value;
                elites.Add(AffixDefenderElite);
                targetTier.eliteTypes = elites.ToArray();
            }
        }
        private static void Stats(CharacterBody sender, StatHookEventArgs args)
        {
            if (sender.HasBuff(AffixDefenderBuff))
            {
                args.armorAdd += DefenderArmor.Value;
            }
        }

        private static void AbsorbDamage(On.RoR2.HealthComponent.orig_TakeDamageProcess orig, HealthComponent self, DamageInfo damageInfo)
        {
            if (DefenderEnableDamageAbsorb.Value && damageInfo.attacker && !self.body.HasBuff(AffixDefenderBuff))
            {
                int eliteCount = 0;
                CharacterBody[] eliteArray = new CharacterBody[0];
                foreach (var characterBody in CharacterBody.readOnlyInstancesList)
                {
                    if (characterBody.HasBuff(AffixDefenderBuff) && characterBody.teamComponent.teamIndex == self.body.teamComponent.teamIndex)
                    {
                        eliteCount++;
                        Array.Resize(ref eliteArray, eliteCount);
                        eliteArray.SetValue(characterBody, eliteCount - 1);
                    }
                }
                if (eliteCount > 0)
                {
                    
                    damageInfo.rejected = true;

                    foreach (var elite in eliteArray)
                    {
                        DamageInfo damageInfo2 = new DamageInfo
                        {
                            damage = damageInfo.damage / eliteCount,
                            damageColorIndex = damageInfo.damageColorIndex,
                            damageType = damageInfo.damageType,
                            attacker = damageInfo.attacker,
                            crit = damageInfo.crit,
                            force = Vector3.zero,
                            inflictor = damageInfo.inflictor,
                            position = elite.transform.position,
                            procChainMask = damageInfo.procChainMask,
                            procCoefficient = damageInfo.procCoefficient,
                        };
                        elite.healthComponent.TakeDamage(damageInfo2);
                    }
                }
            }

            orig(self, damageInfo);
        }
        private static void OnEnemySkillUse(On.RoR2.CharacterBody.orig_OnSkillActivated orig, CharacterBody self, GenericSkill skill)
        {
            float stunChance = 0;
            foreach (var characterBody in CharacterBody.readOnlyInstancesList)
            {
                if (characterBody && characterBody.teamComponent.teamIndex != self.teamComponent.teamIndex)
                {
                    if (characterBody.HasBuff(AffixDefenderBuff))
                    {
                        stunChance += DefenderStunChance.Value;

                    }
                }
            }
            if (stunChance > 0)
            {
                bool roll = false;
                int buffCount = self.GetBuffCount(DazzledBuff.DazzledBuffDef) + 1;
                    roll = Util.CheckRoll(Util.ConvertAmplificationPercentageIntoReductionPercentage(stunChance / buffCount));
                if (roll)
                {
                    if (!self.isChampion)
                    {
                        SetStateOnHurt component = self.GetComponent<SetStateOnHurt>();
                        if (component.hasEffectiveAuthority)
                        {
                            component.SetStunInternal(0.2f);
                            EffectManager.SimpleImpactEffect(LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/ImpactEffects/ImpactStunGrenade"), self.corePosition, self.corePosition, true);
                        }
                        else
                        {
                            component.CallRpcSetStun(0.2f);
                            EffectManager.SimpleImpactEffect(LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/ImpactEffects/ImpactStunGrenade"), self.corePosition, self.corePosition, true);

                        }
                        self.AddTimedBuff(DazzledBuff.DazzledBuffDef, DefenderDazzledTime.Value);
                    }
                }
            }
            orig(self, skill);
        }
        private static void CharacterBody_OnBuffFirstStackGained(
           On.RoR2.CharacterBody.orig_OnBuffFirstStackGained orig,
           CharacterBody self,
           BuffDef buffDef
           )
        {
            orig(self, buffDef);
            if (buffDef == AffixDefenderBuff)
            {
                GameObject gameObject = Object.Instantiate<GameObject>(DefenderWard);
                Vector3 position = self.transform.position + 2f * Vector3.forward;
                //self.baseMoveSpeed *= 2f;
                //self.baseAttackSpeed *= 2f;
                //BuffWard component = gameObject.GetComponent<BuffWard>();
                //gameObject.GetComponent<TeamFilter>().teamIndex = self.teamComponent.teamIndex;
                //component.buffDef = RoR2Content.Buffs.Warbanner;
                //component.Networkradius = 25f + self.radius;
                //gameObject.GetComponent<NetworkedBodyAttachment>().AttachToGameObjectAndSpawn(self.gameObject);
            }
        }

        private static void CharacterBody_OnBuffFinalStackLost(
      On.RoR2.CharacterBody.orig_OnBuffFinalStackLost orig,
      CharacterBody self, BuffDef buffDef)
        {
            orig(self, buffDef);
            if (buffDef == AffixDefenderBuff)
            {
                //BuffWard buffWard = self.gameObject.GetComponentInChildren<BuffWard>();
                //Object.Destroy(buffWard);
            }
        }

        private static void AddContent()
        {
            ItemDisplay itemDisplays = new ItemDisplay();
            ContentAddition.AddEliteDef(AffixDefenderElite);
            ContentAddition.AddBuffDef(AffixDefenderBuff);
        }

        private static void SetupBuff()
        {
            AffixDefenderBuff = ScriptableObject.CreateInstance<BuffDef>();
            AffixDefenderBuff.name = "Elite" + name.Replace(" ", "") + "Buff";
            AffixDefenderBuff.canStack = false;
            AffixDefenderBuff.isCooldown = false;
            AffixDefenderBuff.isDebuff = false;
            AffixDefenderBuff.buffColor = Color.white; // AffixBrassModalityColor;
            AffixDefenderBuff.iconSprite = eliteIcon;
        }

        private static void SetupEquipment()
        {
            AffixDefenderEquipment = ScriptableObject.CreateInstance<EquipmentDef>();
            AffixDefenderEquipment.name = "Affix" + name.Replace(" ", "");
            AffixDefenderEquipment.nameToken = "EQUIPMENT_AFFIX_" + name.ToUpper().Replace(" ", "") + "_NAME";
            AffixDefenderEquipment.pickupToken = "EQUIPMENT_AFFIX_" + name.ToUpper().Replace(" ", "") + "_PICKUP";
            AffixDefenderEquipment.descriptionToken = "EQUIPMENT_AFFIX_" + name.ToUpper().Replace(" ", "") + "_DESC";
            AffixDefenderEquipment.loreToken = "EQUIPMENT_AFFIX_" + name.ToUpper().Replace(" ", "") + "_LORE";
            AffixDefenderEquipment.appearsInMultiPlayer = true;
            AffixDefenderEquipment.appearsInSinglePlayer = true;
            AffixDefenderEquipment.canBeRandomlyTriggered = false;
            AffixDefenderEquipment.canDrop = false;
            AffixDefenderEquipment.colorIndex = ColorCatalog.ColorIndex.Equipment;
            AffixDefenderEquipment.cooldown = 0.0f;
            AffixDefenderEquipment.isLunar = false;
            AffixDefenderEquipment.isBoss = false;
            AffixDefenderEquipment.passiveBuffDef = AffixDefenderBuff;
            AffixDefenderEquipment.dropOnDeathChance = affixDropChance;
            AffixDefenderEquipment.enigmaCompatible = false;
            AffixDefenderEquipment.requiredExpansion = CaeliImperiumExpansionDef;
            AffixDefenderEquipment.pickupModelPrefab = MainAssets.LoadAsset<GameObject>("Assets/Models/Prefabs/AffixDefender.prefab");
            AffixDefenderEquipment.pickupIconSprite = Addressables.LoadAssetAsync<Sprite>("RoR2/Base/EliteIce/texAffixWhiteIcon.png").WaitForCompletion();
        }

        private static void SetupElite()
        {
            AffixDefenderElite = ScriptableObject.CreateInstance<EliteDef>();
            AffixDefenderElite.color = Color.white;//AffixBrassModalityColor;
            AffixDefenderElite.eliteEquipmentDef = AffixDefenderEquipment;
            AffixDefenderElite.modifierToken = "ELITE_MODIFIER_" + name.ToUpper().Replace(" ", "");
            AffixDefenderElite.name = "Elite" + name.Replace(" ", "");
            AffixDefenderElite.healthBoostCoefficient = DefenderHealthMult.Value;
            AffixDefenderElite.damageBoostCoefficient = DefenderDamageMult.Value;
            AffixDefenderBuff.eliteDef = AffixDefenderElite;
        }

        private static void AddLanguageTokens()
        {
            LanguageAPI.Add("ELITE_MODIFIER_" + name.ToUpper().Replace(" ", ""), name + " {0}");
            LanguageAPI.Add("EQUIPMENT_AFFIX_" + name.ToUpper().Replace(" ", "") + "_NAME", name + " Aspect");
            LanguageAPI.Add("EQUIPMENT_AFFIX_" + name.ToUpper().Replace(" ", "") + "_PICKUP", name + " Pickup");
            LanguageAPI.Add("EQUIPMENT_AFFIX_" + name.ToUpper().Replace(" ", "") + "_DESC", name + " Description");
            LanguageAPI.Add("EQUIPMENT_AFFIX_" + name.ToUpper().Replace(" ", "") + "_LORE", name + " Modality Lore");
        }
    }
}