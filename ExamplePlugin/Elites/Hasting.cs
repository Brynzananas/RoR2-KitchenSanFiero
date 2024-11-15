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
using static KitchenSanFieroPlugin.KitchenSanFiero;
using RiskOfOptions.Options;
using RiskOfOptions;
using BepInEx.Configuration;
using RiskOfOptions.OptionConfigs;

namespace KitchenSanFiero.Elites
{
    public class Hasting : BaseItemBodyBehavior
    {
        public static Color AffixHastingColor = new Color(1f, 0.5f, 0.0f);
        public static EquipmentDef AffixHastingEquipment;
        public static BuffDef AffixHastingBuff;
        public static EliteDef AffixHastingElite;
        public static float healthMult = 4f;
        public static float damageMult = 2f;
        public static float affixDropChance = 0.00025f;
        private static GameObject HastingWard = PrefabAPI.InstantiateClone(Addressables.LoadAssetAsync<GameObject>("RoR2/Base/EliteHaunted/AffixHauntedWard.prefab").WaitForCompletion(), "HastingWard");
        private static Material hastingMat = Addressables.LoadAssetAsync<Material>("RoR2/Base/WardOnLevel/matWarbannerBuffRing.mat").WaitForCompletion();
        private static Texture2D eliteRamp = MainAssets.LoadAsset<Texture2D>("Assets/Textures/hasting_ramp.png");
        private static Sprite eliteIcon = MainAssets.LoadAsset<Sprite>("Assets/Icons/hasting_icon.png");
        // RoR2/Base/Common/ColorRamps/texRampWarbanner.png 
        public static ConfigEntry<float> HastingHealthMult;
        public static ConfigEntry<float> HastingDamageMult;
        public static ConfigEntry<float> HastingSpeedMult;
        public static ConfigEntry<float> HastingAttackSpeedMult;
        public static ConfigEntry<float> HastingCooldownReductionMult;
        public static ConfigEntry<float> HastingSpeedMultAddition;
        public static ConfigEntry<float> HastingAttackSpeedMultAddition;
        public static ConfigEntry<bool> HastingEnable;

        public Hasting()
        {
            this.AddConfigs();
            if (!HastingEnable.Value)
            {
                return;
            }
            HastingWard.transform.GetChild(0).GetChild(0).GetComponent<MeshRenderer>().material = hastingMat;
            this.AddLanguageTokens();
            this.SetupBuff();
            this.SetupEquipment();
            this.SetupElite();
            this.AddContent();
            EliteRamp.AddRamp(AffixHastingElite, eliteRamp);
            ContentAddition.AddEquipmentDef(AffixHastingEquipment);
            On.RoR2.CharacterBody.OnBuffFirstStackGained += CharacterBody_OnBuffFirstStackGained;
            On.RoR2.CharacterBody.OnBuffFinalStackLost += CharacterBody_OnBuffFinalStackLost;
            On.RoR2.CombatDirector.Init += CombatDirector_Init;
            //On.RoR2.CharacterBody.FixedUpdate += LessHPMoreSpeed;
            //On.RoR2.CharacterBody.OnTakeDamageServer += OnDamageTake;
            GetStatCoefficients += Stats;
        }
        private void AddConfigs()
        {
            HastingHealthMult = Config.Bind<float>("Elite : Hasting",
                                         "Health Multiplier",
                                         4f,
                                         "Control the health multiplier of Hasting elite");
            HastingDamageMult = Config.Bind<float>("Elite : Hasting",
                                         "Damage Multiplier",
                                         2f,
                                         "Control the damage multiplier of Dredged elite");
            HastingSpeedMult = Config.Bind<float>("Elite : Hasting",
                                         "Base Speed Multiplier",
                                         2f,
                                         "Control the base speed multiplier of Hasting elite");
            HastingAttackSpeedMult = Config.Bind<float>("Elite : Hasting",
                                         "Base Attack Speed Multiplier",
                                         2f,
                                         "Control the base attack speed multiplier of Dredged elite");
            HastingCooldownReductionMult = Config.Bind<float>("Elite : Hasting",
                                         "Base Cooldown Reduction Multiplier",
                                         0.5f,
                                         "Control the cooldow multiplier of Dredged elite\nDon't touch this if you don't know how it works");
            HastingSpeedMultAddition = Config.Bind<float>("Elite : Hasting",
                                         "Extra Speed Multiplier",
                                         1f,
                                         "Control the speed multiplier of Hasting elite on low health");
            HastingAttackSpeedMultAddition = Config.Bind<float>("Elite : Hasting",
                                         "Extra Attack Speed Multiplier",
                                         1f,
                                         "Control the attack speed multiplier of Hasting elite on low health");
            HastingEnable = Config.Bind<bool>("Elite : Hasting",
                 "Activation",
                 true,
                 "Enable Hasting elite?");
            ModSettingsManager.AddOption(new CheckBoxOption(HastingEnable, new CheckBoxConfig() { restartRequired = true }));
            ModSettingsManager.AddOption(new FloatFieldOption(HastingHealthMult));
            ModSettingsManager.AddOption(new FloatFieldOption(HastingDamageMult));
            ModSettingsManager.AddOption(new FloatFieldOption(HastingSpeedMult));
            ModSettingsManager.AddOption(new FloatFieldOption(HastingAttackSpeedMult));
            ModSettingsManager.AddOption(new FloatFieldOption(HastingCooldownReductionMult));
            ModSettingsManager.AddOption(new FloatFieldOption(HastingAttackSpeedMultAddition));
            ModSettingsManager.AddOption(new FloatFieldOption(HastingSpeedMultAddition));
        }

        private void Stats(CharacterBody sender, StatHookEventArgs args)
        {
            
                if (sender.HasBuff(AffixHastingBuff))
                {
                    float combinedHealthFraction = sender.healthComponent.combinedHealthFraction;
                    
                        args.attackSpeedMultAdd =  (HastingAttackSpeedMult.Value - 1) + ((1f - combinedHealthFraction) * HastingAttackSpeedMultAddition.Value);
                        args.moveSpeedMultAdd =  (HastingSpeedMult.Value - 1) + ((1f - combinedHealthFraction) * HastingSpeedMultAddition.Value);
                args.cooldownMultAdd = -HastingCooldownReductionMult.Value;
                    

               }
            
        }
        /*
        private void OnDamageTake(On.RoR2.CharacterBody.orig_OnTakeDamageServer orig, CharacterBody self, DamageReport damageReport)
        {
            orig(self, damageReport);
            if (self.HasBuff(AffixHastingBuff))
            {
                float combinedHealth = self.healthComponent.combinedHealth;
                float combinedHealthFraction = self.healthComponent.combinedHealthFraction;
                Debug.Log("CombinedHealth: " +  combinedHealth);
                Debug.Log("CombinedHealthFraction: " + combinedHealthFraction);
            }
        }
        /*
        
        private void LessHPMoreSpeed(On.RoR2.CharacterBody.orig_FixedUpdate orig, CharacterBody self)
        {
            orig(self);
            if (self.HasBuff(AffixHastingBuff))
            {
                float speedBuff = 1f;
                float test = 1f;
                test = self.healthComponent.combinedHealthFraction;
                if (self.healthComponent)
                {
                    Debug.Log(test);
                self.baseAttackSpeed *= speedBuff;
                self.baseAttackSpeed *= speedBuff;
                }
                
            }
        }
        */
        private void CombatDirector_Init(On.RoR2.CombatDirector.orig_Init orig)
        {
            orig();
            if (EliteAPI.VanillaEliteTiers.Length > 2)
            {
                // HONOR
                CombatDirector.EliteTierDef targetTier = EliteAPI.VanillaEliteTiers[2];
                List<EliteDef> elites = targetTier.eliteTypes.ToList();
                AffixHastingElite.healthBoostCoefficient = HastingHealthMult.Value / 1.6f;
                AffixHastingElite.damageBoostCoefficient = HastingDamageMult.Value / 1.3f;
                elites.Add(AffixHastingElite);
                targetTier.eliteTypes = elites.ToArray();
            }
            if (EliteAPI.VanillaEliteTiers.Length > 1)
            {
                CombatDirector.EliteTierDef targetTier = EliteAPI.VanillaEliteTiers[1];
                List<EliteDef> elites = targetTier.eliteTypes.ToList();
                AffixHastingElite.healthBoostCoefficient = HastingHealthMult.Value;
                AffixHastingElite.damageBoostCoefficient = HastingDamageMult.Value;
                elites.Add(AffixHastingElite);
                targetTier.eliteTypes = elites.ToArray();
            }
        }

        private void CharacterBody_OnBuffFirstStackGained(
           On.RoR2.CharacterBody.orig_OnBuffFirstStackGained orig,
           CharacterBody self,
           BuffDef buffDef
           )
        {
            orig(self, buffDef);
            if (buffDef == AffixHastingBuff)
            {
                GameObject gameObject = Object.Instantiate<GameObject>(HastingWard);
                //self.baseMoveSpeed *= 2f;
                //self.baseAttackSpeed *= 2f;
                //BuffWard component = gameObject.GetComponent<BuffWard>();
                //gameObject.GetComponent<TeamFilter>().teamIndex = self.teamComponent.teamIndex;
                //component.buffDef = RoR2Content.Buffs.Warbanner;
                //component.Networkradius = 25f + self.radius;
                //gameObject.GetComponent<NetworkedBodyAttachment>().AttachToGameObjectAndSpawn(self.gameObject);
            }
        }

        private void CharacterBody_OnBuffFinalStackLost(
      On.RoR2.CharacterBody.orig_OnBuffFinalStackLost orig,
      CharacterBody self, BuffDef buffDef)
        {
            orig(self, buffDef);
            if (buffDef == AffixHastingBuff)
            {
                //BuffWard buffWard = self.gameObject.GetComponentInChildren<BuffWard>();
                //Object.Destroy(buffWard);
            }
        }

        private void AddContent()
        {
            ItemDisplay itemDisplays = new ItemDisplay();
            ContentAddition.AddEliteDef(AffixHastingElite);
            ContentAddition.AddBuffDef(AffixHastingBuff);
        }

        private void SetupBuff()
        {
            AffixHastingBuff = ScriptableObject.CreateInstance<BuffDef>();
            AffixHastingBuff.name = "EliteHastingBuff";
            AffixHastingBuff.canStack = false;
            AffixHastingBuff.isCooldown = false;
            AffixHastingBuff.isDebuff = false;
            AffixHastingBuff.buffColor = Color.white; // AffixHastingColor;
            AffixHastingBuff.iconSprite = eliteIcon;
        }

        private void SetupEquipment()
        {
            AffixHastingEquipment = ScriptableObject.CreateInstance<EquipmentDef>();
            AffixHastingEquipment.appearsInMultiPlayer = true;
            AffixHastingEquipment.appearsInSinglePlayer = true;
            AffixHastingEquipment.canBeRandomlyTriggered = false;
            AffixHastingEquipment.canDrop = false;
            AffixHastingEquipment.colorIndex = ColorCatalog.ColorIndex.Equipment;
            AffixHastingEquipment.cooldown = 0.0f;
            AffixHastingEquipment.isLunar = false;
            AffixHastingEquipment.isBoss = false;
            AffixHastingEquipment.passiveBuffDef = AffixHastingBuff;
            AffixHastingEquipment.dropOnDeathChance = affixDropChance;
            AffixHastingEquipment.enigmaCompatible = false;
            AffixHastingEquipment.pickupModelPrefab = PrefabAPI.InstantiateClone(Addressables.LoadAssetAsync<GameObject>("RoR2/Base/EliteFire/PickupEliteFire.prefab").WaitForCompletion(), "PickupAffixHasting", false);
            foreach (Renderer componentsInChild in AffixHastingEquipment.pickupModelPrefab.GetComponentsInChildren<Renderer>())
                componentsInChild.material = hastingMat;
            AffixHastingEquipment.pickupIconSprite = Addressables.LoadAssetAsync<Sprite>("RoR2/Base/EliteIce/texAffixWhiteIcon.png").WaitForCompletion();
            
            AffixHastingEquipment.nameToken = "EQUIPMENT_AFFIX_HASTING_NAME";
            AffixHastingEquipment.descriptionToken = "EQUIPMENT_AFFIX_HASTING_DESC";
            AffixHastingEquipment.pickupToken = "EQUIPMENT_AFFIX_HASTING_PICKUP";
            AffixHastingEquipment.loreToken = "EQUIPMENT_AFFIX_HASTING_LORE";
            AffixHastingEquipment.name = "AffixHasting";
        }

        private void SetupElite()
        {
            AffixHastingElite = ScriptableObject.CreateInstance<EliteDef>();
            AffixHastingElite.color = Color.white;//AffixHastingColor;
            AffixHastingElite.eliteEquipmentDef = AffixHastingEquipment;
            AffixHastingElite.modifierToken = "ELITE_MODIFIER_HASTING";
            AffixHastingElite.name = "EliteHasting";
            AffixHastingElite.healthBoostCoefficient = HastingHealthMult.Value;
            AffixHastingElite.damageBoostCoefficient = HastingDamageMult.Value;
            AffixHastingBuff.eliteDef = AffixHastingElite;
        }

        private void AddLanguageTokens()
        {
            LanguageAPI.Add("ELITE_MODIFIER_HASTING", "Hasting {0}");
            LanguageAPI.Add("EQUIPMENT_AFFIX_HASTING_NAME", "Haste Aspect");
            LanguageAPI.Add("EQUIPMENT_AFFIX_HASTING_DESC", "Become an aspect of Haste");
            LanguageAPI.Add("EQUIPMENT_AFFIX_HASTING_PICKUP", "Become an aspect of Haste");
            LanguageAPI.Add("EQUIPMENT_AFFIX_HASTING_LORE", "Hasting Aspect Lore");
        }
    }
}