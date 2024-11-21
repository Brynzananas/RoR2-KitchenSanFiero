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
using static KitchenSanFieroPlugin.KitchenSanFiero;
using RiskOfOptions.Options;
using RiskOfOptions;
using BepInEx.Configuration;
using KitchenSanFiero.Buffs;
using RiskOfOptions.OptionConfigs;

namespace KitchenSanFiero.Elites
{
    public static class BrassModality
    {
        public static Color AffixBrassModalityColor = new Color(1f, 0.5f, 0.0f);
        public static EquipmentDef AffixBrassModalityEquipment;
        public static BuffDef AffixBrassModalityBuff;
        public static EliteDef AffixModalityElite;
        public static float healthMult = 4f;
        public static float damageMult = 2f;
        //public static GameObject DroneSupport = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/EliteHaunted/AffixHauntedWard.prefab").WaitForCompletion();
        public static float affixDropChance = 0.00025f;
        private static GameObject BrassModalityWard = PrefabAPI.InstantiateClone(Addressables.LoadAssetAsync<GameObject>("RoR2/Base/EliteHaunted/AffixHauntedWard.prefab").WaitForCompletion(), "BrassModalityWard");
        private static Material brassModalityMat = Addressables.LoadAssetAsync<Material>("RoR2/Base/Common/VFX/matOpaqueDustLarge_BrassContraption_opt.mat").WaitForCompletion();
        private static Texture2D eliteRamp = MainAssets.LoadAsset<Texture2D>("Assets/Textures/brass_modality_ramp.png");
        private static Sprite eliteIcon = MainAssets.LoadAsset<Sprite>("Assets/Icons/brass_modality_icon.png");
        // RoR2/Base/Common/ColorRamps/texRampWarbanner.png 
        public static ConfigEntry<bool> BrassModalityEnable;
        public static ConfigEntry<float> BrassModalityHealthMult;
        public static ConfigEntry<float> BrassModalityDamageMult;
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
            EliteRamp.AddRamp(AffixModalityElite, eliteRamp);
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
                                         4f,
                                         "Control the health multiplier of Brass Modality elite");
            BrassModalityDamageMult = Config.Bind<float>("Elite : Brass Modality",
                                         "Damage Multiplier",
                                         2f,
                                         "Control the damage multiplier of Brass Modality elite");
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
                                         true,
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
                                         10,
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
                if (damageInfo.attacker && !damageInfo.rejected && damageInfo.attacker.GetComponent<CharacterBody>().HasBuff(AffixBrassModalityBuff) && BrassModalityDoWound.Value)
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
        private static void CombatDirector_Init(On.RoR2.CombatDirector.orig_Init orig)
        {
            orig();
            if (EliteAPI.VanillaEliteTiers.Length > 2)
            {
                // HONOR
                CombatDirector.EliteTierDef targetTier = EliteAPI.VanillaEliteTiers[2];
                List<EliteDef> elites = targetTier.eliteTypes.ToList();
                AffixModalityElite.healthBoostCoefficient = BrassModalityHealthMult.Value / 1.6f;
                AffixModalityElite.damageBoostCoefficient = BrassModalityDamageMult.Value / 1.3f;
                elites.Add(AffixModalityElite);
                targetTier.eliteTypes = elites.ToArray();
            }
            if (EliteAPI.VanillaEliteTiers.Length > 1)
            {
                CombatDirector.EliteTierDef targetTier = EliteAPI.VanillaEliteTiers[1];
                List<EliteDef> elites = targetTier.eliteTypes.ToList();
                AffixModalityElite.healthBoostCoefficient = BrassModalityHealthMult.Value;
                AffixModalityElite.damageBoostCoefficient = BrassModalityDamageMult.Value;
                elites.Add(AffixModalityElite);
                targetTier.eliteTypes = elites.ToArray();
            }
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
            ContentAddition.AddEliteDef(AffixModalityElite);
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
            AffixBrassModalityEquipment.pickupModelPrefab = PrefabAPI.InstantiateClone(Addressables.LoadAssetAsync<GameObject>("RoR2/Base/EliteFire/PickupEliteFire.prefab").WaitForCompletion(), "PickupAffixBrassModality", false);
            foreach (Renderer componentsInChild in AffixBrassModalityEquipment.pickupModelPrefab.GetComponentsInChildren<Renderer>())
                componentsInChild.material = brassModalityMat;
            AffixBrassModalityEquipment.pickupIconSprite = Addressables.LoadAssetAsync<Sprite>("RoR2/Base/EliteIce/texAffixWhiteIcon.png").WaitForCompletion();
        }

        private static void SetupElite()
        {
            AffixModalityElite = ScriptableObject.CreateInstance<EliteDef>();
            AffixModalityElite.color = Color.white;//AffixBrassModalityColor;
            AffixModalityElite.eliteEquipmentDef = AffixBrassModalityEquipment;
            AffixModalityElite.modifierToken = "ELITE_MODIFIER_BRASSMODALITY";
            AffixModalityElite.name = "EliteBrassModality";
            AffixModalityElite.healthBoostCoefficient = BrassModalityHealthMult.Value;
            AffixModalityElite.damageBoostCoefficient = BrassModalityDamageMult.Value;
            AffixBrassModalityBuff.eliteDef = AffixModalityElite;
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