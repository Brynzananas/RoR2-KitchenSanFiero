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
using static CaeliImperiumPlugin.CaeliImperium;
using RiskOfOptions.Options;
using RiskOfOptions;
using BepInEx.Configuration;
using CaeliImperium.Buffs;
using RiskOfOptions.OptionConfigs;
using static RoR2.CombatDirector;
using System.Xml.Linq;

namespace CaeliImperium.Elites
{
    public static class BrassModality
    {
        public static Color AffixBrassModalityColor = new Color(1f, 0.5f, 0.0f);
        public static EquipmentDef AffixBrassModalityEquipment;
        public static BuffDef AffixBrassModalityBuff;
        public static EliteDef AffixBrassModalityElite;
        public static CombatDirector.EliteTierDef AffixBrassModalityTier;
        public static float healthMult = 4f;
        public static float damageMult = 2f;
        public static EliteTierDef[] CanAppearInEliteTiers = EliteAPI.GetCombatDirectorEliteTiers();
        //public static GameObject DroneSupport = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/EliteHaunted/AffixHauntedWard.prefab").WaitForCompletion();
        public static float affixDropChance = 0.00025f;
        private static GameObject BrassModalityWard = PrefabAPI.InstantiateClone(Addressables.LoadAssetAsync<GameObject>("RoR2/Base/EliteHaunted/AffixHauntedWard.prefab").WaitForCompletion(), "BrassModalityWard");
        private static Material brassModalityMat = MainAssets.LoadAsset<Material>("Assets/Materials/brass_modality_ramp.mat");
        private static Texture2D eliteRamp = MainAssets.LoadAsset<Texture2D>("Assets/Textures/brass_modality_ramp.png");
        private static Sprite eliteIcon = MainAssets.LoadAsset<Sprite>("Assets/Icons/brass_modality_icon.png");
        // RoR2/Base/Common/ColorRamps/texRampWarbanner.png 
        public static ConfigEntry<bool> BrassModalityEnable;
        public static ConfigEntry<float> BrassModalityHealthMult;
        public static ConfigEntry<float> BrassModalityDamageMult;
        public static ConfigEntry<float> BrassModalityTier;
        public static ConfigEntry<bool> BrassModalityHonor;
        public static ConfigEntry<float> BrassModalityCostMult;
        public static ConfigEntry<int> BrassModalityLoopCount;
        public static ConfigEntry<int> BrassModalityStageCount;
        public static ConfigEntry<float> BrassModalityDamageMultAddition;
        public static ConfigEntry<float> BrassModalityAttackSpeedReduction;
        public static ConfigEntry<float> BrassModalityArmor;
        public static ConfigEntry<float> BrassModalityDamageMultAdditionPlayer;
        public static ConfigEntry<float> BrassModalityAttackSpeedReductionPlayer;
        public static ConfigEntry<float> BrassModalityArmorPlayer;
        public static ConfigEntry<bool> BrassModalityDoWound;
        public static ConfigEntry<bool> BrassModalityDoWoundAmount;
        public static ConfigEntry<bool> BrassModalityDoWoundTime;
        public static ConfigEntry<float> BrassModalityFlatWoundTime;
        public static ConfigEntry<float> BrassModalityDamageWoundTime;
        public static ConfigEntry<int> BrassModalityFlatWoundAmount;
        public static ConfigEntry<float> BrassModalityDamageWoundAmount;

        public static void Init()
        {
            AddConfigs();
            if (!BrassModalityEnable.Value)
            {
                return;
            }
            BrassModalityWard.transform.GetChild(0).GetChild(0).GetComponent<MeshRenderer>().material = brassModalityMat;
            AddLanguageTokens();
            SetupBuff();
            SetupEquipment();
            SetupElite();
            AddContent();
            //CreateEliteTier();

            EliteRamp.AddRamp(AffixBrassModalityElite, eliteRamp);
            ContentAddition.AddEquipmentDef(AffixBrassModalityEquipment);
            On.RoR2.CharacterBody.OnBuffFirstStackGained += CharacterBody_OnBuffFirstStackGained;
            On.RoR2.CharacterBody.OnBuffFinalStackLost += CharacterBody_OnBuffFinalStackLost;
            On.RoR2.CombatDirector.Init += CombatDirector_Init;
            On.RoR2.GlobalEventManager.OnHitEnemy += WoundThem;
            GetStatCoefficients += Stats;
        }
        private static void AddConfigs()
        {
            BrassModalityHealthMult = Config.Bind<float>("Elite : Brass Modality",
                                         "Health Multiplier",
                                         6f,
                                         "Control the health multiplier of Brass Modality elite");
            BrassModalityDamageMult = Config.Bind<float>("Elite : Brass Modality",
                                         "Damage Multiplier",
                                         3f,
                                         "Control the damage multiplier of Brass Modality elite");
            BrassModalityTier = Config.Bind<float>("Elite : Brass Modality",
                                         "Tier",
                                         2f,
                                         "Control the tier of Brass Modality elite\n1: Default\n2: Appear from stage 2\n3: Appear from first loop");
            BrassModalityHonor = Config.Bind<bool>("Elite : Brass Modality",
                                         "Honor",
                                         false,
                                         "Enable Honor variant?");
            BrassModalityCostMult = Config.Bind<float>("Elite : Brass Modality",
                             "Cost Multiplier",
                             1.2f,
                             "Control the cost multiplier of this elite\nWIP: Does not work");
            BrassModalityLoopCount = Config.Bind<int>("Elite : Brass Modality",
                                         "Loop count",
                                         0,
                                         "Control from which loop this elite appears\nWIP: Does not work");
            BrassModalityStageCount = Config.Bind<int>("Elite : Brass Modality",
                                         "Stage count",
                                         2,
                                         "Control from which stage this elite appears\nWIP: Does not work"); ;
            BrassModalityDamageMultAddition = Config.Bind<float>("Elite : Brass Modality",
                                         "Additional damage multiplier",
                                         3f,
                                         "Control the additional damage multiplier of Brass Modality elite");
            BrassModalityAttackSpeedReduction = Config.Bind<float>("Elite : Brass Modality",
                                         "Attack speed reduction",
                                         2f,
                                         "Control the attack speed reduction of Brass Modality elite");
            BrassModalityArmor = Config.Bind<float>("Elite : Brass Modality",
                                         "Armor",
                                         100f,
                                         "Control the armor addition of Brass Modality elite");
            BrassModalityDamageMultAdditionPlayer = Config.Bind<float>("Elite : Brass Modality",
                                         "Additional damage multiplier for player user",
                                         1.5f,
                                         "Control the additional damage multiplier of Brass Modality elite for players");
            BrassModalityAttackSpeedReductionPlayer = Config.Bind<float>("Elite : Brass Modality",
                                         "Attack speed reduction for player users",
                                         1.5f,
                                         "Control the attack speed reduction of Brass Modality elite for players");
            BrassModalityArmorPlayer = Config.Bind<float>("Elite : Brass Modality",
                                         "Armor for player users",
                                         20f,
                                         "Control the armor addition of Brass Modality elite for players");
            BrassModalityDoWound = Config.Bind<bool>("Elite : Brass Modality",
                                         "Wound",
                                         true,
                                         "Do Wound on hit?");
            BrassModalityDoWoundAmount = Config.Bind<bool>("Elite : Brass Modality",
                                         "Wound amount function",
                                         true,
                                         "Enable: Applies flat amount of Wound on hit\nDisable: Applies scaled from damage amount of Wound on Hit");
            BrassModalityDoWoundTime = Config.Bind<bool>("Elite : Brass Modality",
                                         "Wound time function",
                                         false,
                                         "Enable: Wound lasts flat amount of time\nDisable: Wound lasts based on damage");
            BrassModalityFlatWoundTime = Config.Bind<float>("Elite : Brass Modality",
                                         "Flat Wound time",
                                         10f,
                                         "Control the duration of a Wounded debuff in seconds");
            BrassModalityDamageWoundTime = Config.Bind<float>("Elite : Brass Modality",
                                         "Damage to Wound time",
                                         0.1f,
                                         "Control the multiplier upon converting damage to Wound time");
            BrassModalityFlatWoundAmount = Config.Bind<int>("Elite : Brass Modality",
                                         "Flat Wound amount",
                                         2,
                                         "Control the amount of Wound per hit");
            BrassModalityDamageWoundAmount = Config.Bind<float>("Elite : Brass Modality",
                                         "Damage to Wound amount",
                                         0.1f,
                                         "Control the multiplier upon converting damage to Wound amount");
            BrassModalityEnable = Config.Bind<bool>("Elite : Brass Modality",
                             "Activation",
                             true,
                             "Enable this elite?");
            ModSettingsManager.AddOption(new CheckBoxOption(BrassModalityEnable, new CheckBoxConfig() { restartRequired = true }));
            ModSettingsManager.AddOption(new FloatFieldOption(BrassModalityHealthMult));
            ModSettingsManager.AddOption(new FloatFieldOption(BrassModalityDamageMult));
            ModSettingsManager.AddOption(new StepSliderOption(BrassModalityTier, new StepSliderConfig() { min = 1, max = 3, increment = 1f, restartRequired = true }));
            ModSettingsManager.AddOption(new CheckBoxOption(BrassModalityHonor, new CheckBoxConfig() { restartRequired = true }));
            ModSettingsManager.AddOption(new FloatFieldOption(BrassModalityCostMult, new FloatFieldConfig() { restartRequired = true }));
            ModSettingsManager.AddOption(new IntFieldOption(BrassModalityLoopCount, new IntFieldConfig() { restartRequired = true }));
            ModSettingsManager.AddOption(new IntFieldOption(BrassModalityStageCount, new IntFieldConfig() { restartRequired = true }));
            ModSettingsManager.AddOption(new FloatFieldOption(BrassModalityDamageMultAddition));
            ModSettingsManager.AddOption(new FloatFieldOption(BrassModalityAttackSpeedReduction));
            ModSettingsManager.AddOption(new FloatFieldOption(BrassModalityArmor));
            ModSettingsManager.AddOption(new FloatFieldOption(BrassModalityDamageMultAdditionPlayer));
            ModSettingsManager.AddOption(new FloatFieldOption(BrassModalityAttackSpeedReductionPlayer));
            ModSettingsManager.AddOption(new FloatFieldOption(BrassModalityArmorPlayer));
            ModSettingsManager.AddOption(new CheckBoxOption(BrassModalityDoWound));
            ModSettingsManager.AddOption(new CheckBoxOption(BrassModalityDoWoundTime));
            ModSettingsManager.AddOption(new CheckBoxOption(BrassModalityDoWoundAmount));
            ModSettingsManager.AddOption(new FloatFieldOption(BrassModalityDamageWoundTime));
            ModSettingsManager.AddOption(new FloatFieldOption(BrassModalityDamageWoundAmount));
            ModSettingsManager.AddOption(new FloatFieldOption(BrassModalityFlatWoundTime));
            ModSettingsManager.AddOption(new IntFieldOption(BrassModalityFlatWoundAmount));
        }
        private static void WoundThem(On.RoR2.GlobalEventManager.orig_OnHitEnemy orig, GlobalEventManager self, DamageInfo damageInfo, GameObject victim)
        {
            orig(self, damageInfo, victim);
            if (BrassModalityDoWound.Value)
            {
                if (damageInfo.attacker && victim.GetComponent<CharacterBody>() && !damageInfo.rejected && damageInfo.attacker.GetComponent<CharacterBody>().HasBuff(AffixBrassModalityBuff) && BrassModalityDoWound.Value)
                {
                    float timer = 0;
                    if (BrassModalityDoWoundTime.Value)
                    {
                        timer = BrassModalityFlatWoundTime.Value * damageInfo.procCoefficient;
                    }
                    else
                    {
                        timer = damageInfo.damage * BrassModalityDamageWoundTime.Value * damageInfo.procCoefficient;
                    }
                    if (BrassModalityDoWoundAmount.Value)
                    {
                        for (int i = 0; i < BrassModalityFlatWoundAmount.Value; i++)
                        {
                            victim.GetComponent<CharacterBody>().AddTimedBuff(WoundedBuff.WoundedBuffDef, timer);

                        }
                    }
                    else
                    {
                        for (int i = 0; i < Math.Ceiling(damageInfo.damage * BrassModalityDamageWoundAmount.Value); i++)
                        {
                            victim.GetComponent<CharacterBody>().AddTimedBuff(WoundedBuff.WoundedBuffDef, timer);

                        }
                    }

                    }
                }
                
            }
            
        

        private static void Stats(CharacterBody sender, StatHookEventArgs args)
        {

            if (sender.HasBuff(AffixBrassModalityBuff))
            {
                if (sender.teamComponent.teamIndex == TeamIndex.Player && sender.isPlayerControlled)
                {
                    args.damageMultAdd += BrassModalityDamageMultAdditionPlayer.Value - 1;
                    args.attackSpeedReductionMultAdd += BrassModalityAttackSpeedReductionPlayer.Value - 1;
                    args.armorAdd += BrassModalityArmorPlayer.Value;
                }
                else
                {
                    args.damageMultAdd += BrassModalityDamageMultAddition.Value - 1;
                    args.attackSpeedReductionMultAdd += BrassModalityAttackSpeedReduction.Value - 1;
                    args.armorAdd += BrassModalityArmor.Value;
                }


            }

        }
        private static void CreateEliteTier()
        {
            AffixBrassModalityTier = new CombatDirector.EliteTierDef()
            {
                costMultiplier = CombatDirector.baseEliteCostMultiplier * BrassModality.BrassModalityCostMult.Value,
                eliteTypes = new EliteDef[]{AffixBrassModalityElite},
                canSelectWithoutAvailableEliteDef = false,
                isAvailable = (SpawnCard.EliteRules rules) => Run.instance.loopClearCount >= BrassModality.BrassModalityLoopCount.Value && rules == SpawnCard.EliteRules.Default && Run.instance.stageClearCount >= BrassModality.BrassModalityStageCount.Value,
            };

            var baseEliteTierDefs = EliteAPI.GetCombatDirectorEliteTiers();
            var indexToInsertAt = Array.FindIndex(baseEliteTierDefs, x => x.costMultiplier >= AffixBrassModalityTier.costMultiplier);
            EliteAPI.AddCustomEliteTier(AffixBrassModalityTier);
            EliteAPI.Add(new CustomElite(AffixBrassModalityElite, CanAppearInEliteTiers));
        }
        private static void CombatDirector_Init(On.RoR2.CombatDirector.orig_Init orig)
        {
            orig();
            //if (EliteAPI.VanillaEliteTiers.Length > 2)
            //{
            //    // HONOR
            if (BrassModalityHonor.Value)
            {
                CombatDirector.EliteTierDef targetTier2 = EliteAPI.VanillaEliteTiers[2];
                List<EliteDef> elites2 = targetTier2.eliteTypes.ToList();
                AffixBrassModalityElite.healthBoostCoefficient = BrassModalityHealthMult.Value / 1.6f;
                AffixBrassModalityElite.damageBoostCoefficient = BrassModalityDamageMult.Value / 1.3f;
                elites2.Add(AffixBrassModalityElite);
                targetTier2.eliteTypes = elites2.ToArray();
            }
                
            //}
            //if (EliteAPI.VanillaEliteTiers.Length > 1)
            //{
            //var eliteTier = new CombatDirector.EliteTierDef()
            //{
            //    costMultiplier = CombatDirector.baseEliteCostMultiplier * BrassModality.BrassModalityCostMult.Value,
            //    eliteTypes = new EliteDef[] { AffixBrassModalityElite },
            //    canSelectWithoutAvailableEliteDef = false,
            //    isAvailable = (SpawnCard.EliteRules rules) => Run.instance.loopClearCount >= BrassModality.BrassModalityLoopCount.Value && rules == SpawnCard.EliteRules.Default && Run.instance.stageClearCount >= BrassModality.BrassModalityStageCount.Value,
            //};
            //var targetTiers = CombatDirector.eliteTiers;
            //targetTiers.ToList().Add(eliteTier);
            //targetTiers.ToArray();
            //CombatDirector.eliteTiers = targetTiers;
            int index = 1;
            switch (BrassModalityTier.Value)
            {
                case 1: index = 1; break;
                case 2: index = 3; break;
                case 3: index = 4; break;

            }
            CombatDirector.EliteTierDef targetTier = EliteAPI.VanillaEliteTiers[index];
            List<EliteDef> elites = targetTier.eliteTypes.ToList();

            AffixBrassModalityElite.healthBoostCoefficient = BrassModalityHealthMult.Value;
            AffixBrassModalityElite.damageBoostCoefficient = BrassModalityDamageMult.Value;
            elites.Add(AffixBrassModalityElite);
            targetTier.eliteTypes = elites.ToArray();
            //}
        }
        private static void CharacterBody_OnBuffFirstStackGained(
           On.RoR2.CharacterBody.orig_OnBuffFirstStackGained orig,
           CharacterBody self,
           BuffDef buffDef
           )
        {
            orig(self, buffDef);
            if (buffDef == AffixBrassModalityBuff)
            {
                GameObject gameObject = Object.Instantiate<GameObject>(BrassModalityWard);
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
            if (buffDef == AffixBrassModalityBuff)
            {
                //BuffWard buffWard = self.gameObject.GetComponentInChildren<BuffWard>();
                //Object.Destroy(buffWard);
            }
        }

        private static void AddContent()
        {
            ItemDisplay itemDisplays = new ItemDisplay();
            ContentAddition.AddEliteDef(AffixBrassModalityElite);
            ContentAddition.AddBuffDef(AffixBrassModalityBuff);
        }

        private static void SetupBuff()
        {
            AffixBrassModalityBuff = ScriptableObject.CreateInstance<BuffDef>();
            AffixBrassModalityBuff.name = "EliteBrassModalityBuff";
            AffixBrassModalityBuff.canStack = false;
            AffixBrassModalityBuff.isCooldown = false;
            AffixBrassModalityBuff.isDebuff = false;
            AffixBrassModalityBuff.buffColor = Color.white; // AffixBrassModalityColor;
            AffixBrassModalityBuff.iconSprite = eliteIcon;
        }

        private static void SetupEquipment()
        {
            AffixBrassModalityEquipment = ScriptableObject.CreateInstance<EquipmentDef>();
            AffixBrassModalityEquipment.name = "AffixBrassModality";
            AffixBrassModalityEquipment.nameToken = "EQUIPMENT_AFFIX_BRASSMODALITY_NAME";
            AffixBrassModalityEquipment.pickupToken = "EQUIPMENT_AFFIX_BRASSMODALITY_PICKUP";
            AffixBrassModalityEquipment.descriptionToken = "EQUIPMENT_AFFIX_BRASSMODALITY_DESC";
            AffixBrassModalityEquipment.loreToken = "EQUIPMENT_AFFIX_BRASSMODALITY_LORE";
            AffixBrassModalityEquipment.appearsInMultiPlayer = true;
            AffixBrassModalityEquipment.appearsInSinglePlayer = true;
            AffixBrassModalityEquipment.canBeRandomlyTriggered = false;
            AffixBrassModalityEquipment.canDrop = false;
            AffixBrassModalityEquipment.colorIndex = ColorCatalog.ColorIndex.Equipment;
            AffixBrassModalityEquipment.cooldown = 0.0f;
            AffixBrassModalityEquipment.isLunar = false;
            AffixBrassModalityEquipment.isBoss = false;
            AffixBrassModalityEquipment.passiveBuffDef = AffixBrassModalityBuff;
            AffixBrassModalityEquipment.dropOnDeathChance = affixDropChance;
            AffixBrassModalityEquipment.enigmaCompatible = false;
            AffixBrassModalityEquipment.requiredExpansion = CaeliImperiumExpansionDef;
            AffixBrassModalityEquipment.pickupModelPrefab = PrefabAPI.InstantiateClone(MainAssets.LoadAsset<GameObject>("Assets/Models/Prefabs/AffixModel.prefab"), "PickupAffixBrassModality", false);
            foreach (Renderer componentsInChild in AffixBrassModalityEquipment.pickupModelPrefab.GetComponentsInChildren<Renderer>())
                componentsInChild.material = brassModalityMat;
            AffixBrassModalityEquipment.pickupIconSprite = MainAssets.LoadAsset<Sprite>("Assets/Icons/BrassModalityAffix.png");
        }

        private static void SetupElite()
        {
            AffixBrassModalityElite = ScriptableObject.CreateInstance<EliteDef>();
            AffixBrassModalityElite.color = Color.white;//AffixBrassModalityColor;
            AffixBrassModalityElite.eliteEquipmentDef = AffixBrassModalityEquipment;
            AffixBrassModalityElite.modifierToken = "ELITE_MODIFIER_BRASSMODALITY";
            AffixBrassModalityElite.name = "EliteBrassModality";
            AffixBrassModalityElite.healthBoostCoefficient = BrassModalityHealthMult.Value;
            AffixBrassModalityElite.damageBoostCoefficient = BrassModalityDamageMult.Value;
            AffixBrassModalityBuff.eliteDef = AffixBrassModalityElite;
        }

        private static void AddLanguageTokens()
        {
            LanguageAPI.Add("ELITE_MODIFIER_BRASSMODALITY", "Brass Modality {0}");
            LanguageAPI.Add("EQUIPMENT_AFFIX_BRASSMODALITY_NAME", "Brass Modality Aspect");
            LanguageAPI.Add("EQUIPMENT_AFFIX_BRASSMODALITY_PICKUP", "Brass Modality Pickup");
            LanguageAPI.Add("EQUIPMENT_AFFIX_BRASSMODALITY_DESC", "Brass Modality Description");
            LanguageAPI.Add("EQUIPMENT_AFFIX_BRASSMODALITY_LORE", "Brass Modality Lore");
        }
    }
}
