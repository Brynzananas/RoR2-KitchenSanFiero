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
using static UnityEngine.UIElements.ListViewDragger;
using static CaeliImperiumPlugin.CaeliImperium;
using CaeliImperium.Buffs;
using Object = UnityEngine.Object;
using JetBrains.Annotations;
using BepInEx.Configuration;
using RiskOfOptions.Options;
using RiskOfOptions;
using UnityEngine.XR;
using UnityEngine.Networking;
using CaeliImperium.Items;
using RiskOfOptions.OptionConfigs;
using System.Numerics;
using System.Xml.Linq;
using static RoR2.CombatDirector;

namespace CaeliImperium.Elites
{
    public static class Dredged
    {
        public static Color AffixDredgedColor = new Color(1f, 0.5f, 0.0f);
        public static EquipmentDef AffixDredgedEquipment;
        public static BuffDef AffixDredgedBuff;
        public static EliteDef AffixDredgedElite;
        public static CombatDirector.EliteTierDef AffixDredgedTier;
        public static float healthMult = 4f;
        public static float damageMult = 2f;
        public static float affixDropChance = 0.00025f;
        public static EliteTierDef[] CanAppearInEliteTiers = EliteAPI.GetCombatDirectorEliteTiers();
        private static GameObject DredgedWard = PrefabAPI.InstantiateClone(Addressables.LoadAssetAsync<GameObject>("RoR2/Base/EliteHaunted/AffixHauntedWard.prefab").WaitForCompletion(), "DredgedWard");
        private static Material dredgedMat = MainAssets.LoadAsset<Material>("Assets/Materials/dredged_ramp.mat");
        private static Texture2D eliteRamp = MainAssets.LoadAsset<Texture2D>("Assets/Textures/dredged_ramp.png");
        private static Sprite eliteIcon = MainAssets.LoadAsset<Sprite>("Assets/Icons/dredged_icon.png");

        public static GameObject DredgedMasterprefab;
        public static UnityEngine.Vector3 DredgedPosition;
        public static Inventory DredgedInventory;
        public static bool isRespawning = false;
        //public static int buffCOunt = 0;
        public static ConfigEntry<bool> DredgedEnableConfig;
        public static ConfigEntry<float> DredgedHealthMult;
        public static ConfigEntry<float> DredgedDamageMult;
        public static ConfigEntry<float> DredgedTier;
        public static ConfigEntry<bool> DredgedHonor;
        public static ConfigEntry<float> DredgedCostMult;
        public static ConfigEntry<int> DredgedLoopCount;
        public static ConfigEntry<int> DredgedStageCount;
        public static ConfigEntry<int> DredgedDamageReviveCount;
        public static ConfigEntry<float> DredgedDamageReviveMult;
        public static ConfigEntry<float> DredgedHealthReviveMult;
        public static ConfigEntry<float> DredgedRegenReviveMult;/*
        public static ConfigEntry<float> DredgedDamageReviveMultPlayer;
        public static ConfigEntry<float> DredgedHealthReviveMultPlayer;
        public static ConfigEntry<float> DredgedRegenReviveMultPlayer;*/
        public static ConfigEntry<bool> DredgedConversion;
        public static ConfigEntry<bool> DredgedEnable;

        // RoR2/Base/Common/ColorRamps/texRampWarbanner.png 

        public static void Init()
        {
            AddConfigs();
            if (!DredgedEnable.Value)
            {
                return;
            }
            DredgedWard.transform.GetChild(0).GetChild(0).GetComponent<MeshRenderer>().material = dredgedMat;
            AddLanguageTokens();
            SetupBuff();
            SetupEquipment();
            SetupElite();
            AddContent();
            //CreateEliteTier();

            //ColdHeart.Init();
            DeathCountBuff.Init();
            EliteRamp.AddRamp(AffixDredgedElite, eliteRamp);
            ContentAddition.AddEquipmentDef(AffixDredgedEquipment);
            On.RoR2.CharacterBody.OnBuffFirstStackGained += CharacterBody_OnBuffFirstStackGained;
            On.RoR2.CharacterBody.OnBuffFinalStackLost += CharacterBody_OnBuffFinalStackLost;
            On.RoR2.CombatDirector.Init += CombatDirector_Init;
            On.RoR2.CharacterBody.OnDeathStart += OnDeath;
            On.RoR2.CharacterBody.OnInventoryChanged += GainDio;
            //On.RoR2.CharacterBody.Start += OnRespawn;
            //On.RoR2.CharacterMaster.OnBodyDeath += OnDeathMaster;
            GetStatCoefficients += Stats;
        }

        private static void GainDio(On.RoR2.CharacterBody.orig_OnInventoryChanged orig, CharacterBody self)
        {
            orig(self);
            if (self && self.isPlayerControlled && self.inventory && self.inventory.GetEquipmentIndex() == AffixDredgedEquipment.equipmentIndex && DredgedConversion.Value)
            {
                self.inventory.SetEquipmentIndex(EquipmentIndex.None);
                self.inventory.GiveItem(RoR2Content.Items.ExtraLife);
                //CharacterMasterNotificationQueue.SendTransformNotification(self.master, self.inventory.currentEquipmentIndex, AffixDredgedEquipment.equipmentIndex, CharacterMasterNotificationQueue.TransformationType.Default);
            }
        }

        private static void AddConfigs()
        {
            DredgedEnableConfig = Config.Bind<bool>("Elite : Dredged",
                 "Config Activation",
                 false,
                 "Enable config?");
            DredgedHealthMult = Config.Bind<float>("Elite : Dredged",
                                         "Health Multiplier",
                                         6f,
                                         "Control the health multiplier of Dredged elite");
            DredgedDamageMult = Config.Bind<float>("Elite : Dredged",
                                         "Damage Multiplier",
                                         3f,
                                         "Control the damage multiplier of Dredged elite");
            DredgedTier = Config.Bind<float>("Elite : Dredged",
                 "Tier",
                 2f,
                 "Control the tier of Dredged elite\n1: Default\n2: Appear from stage 2\n3: Appear from first loop");
            DredgedHonor = Config.Bind<bool>("Elite : Dredged",
                             "Honor",
                             true,
                             "Enable Honor variant?");
            DredgedCostMult = Config.Bind<float>("Elite : Dredged",
                             "Cost Multiplier",
                             1.2f,
                             "Control the cost multiplier of this elite\nWIP: Does not work");
            DredgedLoopCount = Config.Bind<int>("Elite : Dredged",
                                         "Loop count",
                                         1,
                                         "Control from which loop this elite appears\nWIP: Does not work");
            DredgedStageCount = Config.Bind<int>("Elite : Dredged",
                                         "Stage count",
                                         0,
                                         "Control from which stage this elite appears\nWIP: Does not work");
            DredgedDamageReviveCount = Config.Bind<int>("Elite : Dredged",
                                         "Maximum revive count",
                                         2,
                                         "Control the maximum amount Dredged elites can revive");
            DredgedHealthReviveMult = Config.Bind<float>("Elite : Dredged",
                                         "Health increase per revive",
                                         50f,
                                         "Control the health increase of Dredged elite per every revive in percentage");
            DredgedDamageReviveMult = Config.Bind<float>("Elite : Dredged",
                                         "Damage increase per revive",
                                         50f,
                                         "Control the damage increase of Dredged elite per every revive in percentage");
            DredgedRegenReviveMult = Config.Bind<float>("Elite : Dredged",
                                         "Regen increase per revive",
                                         50f,
                                         "Control the regen increase of Dredged elite per every revive in percentage");
            /*DredgedHealthReviveMultPlayer = Config.Bind<float>("Elite : Dredged",
                                         "Health increase for players",
                                         10f,
                                         "Control the health increase of Dredged players");
            DredgedDamageReviveMultPlayer = Config.Bind<float>("Elite : Dredged",
                                         "Damage increase for players",
                                         10f,
                                         "Control the damage increase of Dredged players");
            DredgedRegenReviveMultPlayer = Config.Bind<float>("Elite : Dredged",
                                         "Regen increase for players",
                                         10f,
                                         "Control the regen increase of Dredged players");*/
            DredgedConversion = Config.Bind<bool>("Elite : Dredged",
                 "Conversion",
                 true,
                 "Would picking up this affix convert it to Dios Best Friend?\nOtherwise it does nothing");
            DredgedEnable = Config.Bind<bool>("Elite : Dredged",
                 "Activation",
                 true,
                 "Enable Dredged elite?");
            ModSettingsManager.AddOption(new CheckBoxOption(DredgedEnable, new CheckBoxConfig() { restartRequired = true }));
            ModSettingsManager.AddOption(new CheckBoxOption(DredgedEnableConfig, new CheckBoxConfig() { restartRequired = true }));
            ModSettingsManager.AddOption(new FloatFieldOption(DredgedHealthMult));
            ModSettingsManager.AddOption(new FloatFieldOption(DredgedDamageMult));
            ModSettingsManager.AddOption(new StepSliderOption(DredgedTier, new StepSliderConfig() { min = 1, max = 3, increment = 1f, restartRequired = true }));
            ModSettingsManager.AddOption(new CheckBoxOption(DredgedHonor, new CheckBoxConfig() { restartRequired = true }));
            ModSettingsManager.AddOption(new FloatFieldOption(DredgedCostMult, new FloatFieldConfig() { restartRequired = true }));
            ModSettingsManager.AddOption(new IntFieldOption(DredgedLoopCount, new IntFieldConfig() { restartRequired = true }));
            ModSettingsManager.AddOption(new IntFieldOption(DredgedStageCount, new IntFieldConfig() { restartRequired = true }));
            ModSettingsManager.AddOption(new IntFieldOption(DredgedDamageReviveCount));
            ModSettingsManager.AddOption(new FloatFieldOption(DredgedHealthReviveMult));
            ModSettingsManager.AddOption(new FloatFieldOption(DredgedDamageReviveMult));
            ModSettingsManager.AddOption(new CheckBoxOption(DredgedConversion));
            //ModSettingsManager.AddOption(new FloatFieldOption(DredgedRegenReviveMult));
            //ModSettingsManager.AddOption(new FloatFieldOption(DredgedHealthReviveMultPlayer));
            //ModSettingsManager.AddOption(new FloatFieldOption(DredgedDamageReviveMultPlayer));
            //ModSettingsManager.AddOption(new FloatFieldOption(DredgedRegenReviveMultPlayer));
        }

        private static void Stats(CharacterBody sender, StatHookEventArgs args)
        {
            if (sender.HasBuff(AffixDredgedBuff) && sender.GetBuffCount(DeathCountBuff.DeathCountBuffDef) > 0 && !sender.isPlayerControlled)
            {
                args.healthMultAdd += sender.GetBuffCount(DeathCountBuff.DeathCountBuffDef) * ConfigFloat(DredgedHealthReviveMult, DredgedEnableConfig) / 100;
                args.damageMultAdd += sender.GetBuffCount(DeathCountBuff.DeathCountBuffDef) * ConfigFloat(DredgedDamageReviveMult, DredgedEnableConfig) / 100;
                args.regenMultAdd += sender.GetBuffCount(DeathCountBuff.DeathCountBuffDef) * ConfigFloat(DredgedRegenReviveMult, DredgedEnableConfig) / 100;
            }/*
            else if(sender.HasBuff(AffixDredgedBuff) && sender.isPlayerControlled)
            {
                args.healthMultAdd += DredgedHealthReviveMultPlayer.Value / 100;
                args.damageMultAdd += DredgedDamageReviveMultPlayer.Value / 100;
                args.regenMultAdd += DredgedRegenReviveMultPlayer.Value / 100;
            }*/
        }
        //Пошло это нахуй все блять, я ебал нахуй. Я не буду делать эти ебанные Айл хуки замудренные
        /*
        public static void DredgedRespawn(CharacterMaster self)
        {
            
            CharacterMasterNotificationQueue.SendTransformNotification(self, RoR2Content.Items.ExtraLife.itemIndex, ColdHeart.ColdHeartItemDef.itemIndex, CharacterMasterNotificationQueue.TransformationType.Default);
            UnityEngine.Vector3 vector = self.deathFootPosition;
            if (self.killedByUnsafeArea)
            {
                vector = (TeleportHelper.FindSafeTeleportDestination(self.deathFootPosition, self.bodyPrefab.GetComponent<CharacterBody>(), RoR2Application.rng) ?? self.deathFootPosition);
            }
            self.Respawn(vector, UnityEngine.Quaternion.Euler(0f, UnityEngine.Random.Range(0f, 360f), 0f), true);
            //body.AddBuff(DeathCountBuff.DeathCountBuffDef);

            self.GetBody().AddTimedBuff(RoR2Content.Buffs.Immune, 1f);
            if (!self.GetBody().isPlayerControlled)
            {
                self.GetBody().AddBuff(DeathCountBuff.DeathCountBuffDef);
            }
            if (NetworkServer.active)
            {
                self.inventory.SetEquipmentDisabled(false);
            }
            GameObject gameObject = LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/HippoRezEffect");
            if (self.bodyInstanceObject)
            {
                foreach (EntityStateMachine entityStateMachine in self.bodyInstanceObject.GetComponents<EntityStateMachine>())
                {
                    entityStateMachine.initialStateType = entityStateMachine.mainStateType;
                }
                if (gameObject)
                {
                    EffectManager.SpawnEffect(gameObject, new EffectData
                    {
                        origin = vector,
                        rotation = self.bodyInstanceObject.transform.rotation
                    }, true);
                }
            }
        }
        private static void OnDeathMaster(On.RoR2.CharacterMaster.orig_OnBodyDeath orig, CharacterMaster self, CharacterBody body)
        {

            orig(self, body);

            if (NetworkServer.active)
            {
                if (body.HasBuff(AffixDredgedBuff) && body.GetBuffCount(DeathCountBuff.DeathCountBuffDef) < DredgedDamageReviveCount.Value && !body.isPlayerControlled)
                {
                    
                    
                    //self.destroyOnBodyDeath = false;
                    //DredgedRespawn(self);

                    if (self.IsInvoking("RespawnExtraLife"))
                    {
                        self.CancelInvoke("RespawnExtraLife");
                        self.inventory.GiveItem(RoR2Content.Items.ExtraLife, 1);

                    }
                    if (self.IsInvoking("PlayExtraLifeSFX"))
                    {
                        self.CancelInvoke("PlayExtraLifeSFX");

                    }
                    if (self.IsInvoking("RespawnExtraLifeVoid"))
                    {
                        self.CancelInvoke("RespawnExtraLifeVoid");
                        self.inventory.GiveItem(DLC1Content.Items.ExtraLifeVoid, 1);

                    }
                    if (self.IsInvoking("PlayExtraLifeVoidSFX"))
                    {
                        self.CancelInvoke("PlayExtraLifeVoidSFX");

                    }
                    if (self.IsInvoking("RespawnExtraLifeShrine"))
                    {
                        self.CancelInvoke("RespawnExtraLifeShrine");
                        self.GetBody().AddBuff(DLC2Content.Buffs.ExtraLifeBuff);
                    }
                    //UnityEngine.Vector3 vector = self.deathFootPosition;
                    self.RespawnExtraLife();
                }
                else if (body.HasBuff(AffixDredgedBuff) && body.isPlayerControlled && body.inventory.GetEquipmentIndex() == AffixDredgedEquipment.equipmentIndex)
                {
                    if (body.inventory.GetEquipmentIndex() == AffixDredgedEquipment.equipmentIndex)
                    {

                        //DredgedRespawn(self);
                        body.inventory.GiveItem(ColdHeart.ColdHeartItemDef, 1);
                        body.inventory.SetEquipmentIndex(EquipmentIndex.None);
                        
                        if (self.IsInvoking("RespawnExtraLife"))
                        {
                            self.CancelInvoke("RespawnExtraLife");
                            self.inventory.GiveItem(RoR2Content.Items.ExtraLife, 1);

                        }
                        if (self.IsInvoking("PlayExtraLifeSFX"))
                        {
                            self.CancelInvoke("PlayExtraLifeSFX");

                        }
                        if (self.IsInvoking("RespawnExtraLifeVoid"))
                        {
                            self.CancelInvoke("RespawnExtraLifeVoid");
                            self.inventory.GiveItem(DLC1Content.Items.ExtraLifeVoid, 1);

                        }
                        if (self.IsInvoking("PlayExtraLifeVoidSFX"))
                        {
                            self.CancelInvoke("PlayExtraLifeVoidSFX");

                        }
                        if (self.IsInvoking("RespawnExtraLifeShrine"))
                        {
                            self.CancelInvoke("RespawnExtraLifeShrine");
                            self.GetBody().AddBuff(DLC2Content.Buffs.ExtraLifeBuff);
                        }
                        //UnityEngine.Vector3 vector = self.deathFootPosition;
                        self.RespawnExtraLife();
                    }
                }
            }

            

        }*/
        
        private static void OnDeath(On.RoR2.CharacterBody.orig_OnDeathStart orig, CharacterBody self)
        {
            orig(self);
            if (self.HasBuff(AffixDredgedBuff) && self.GetBuffCount(DeathCountBuff.DeathCountBuffDef) < ConfigInt(DredgedDamageReviveCount, DredgedEnableConfig) && !self.isPlayerControlled)
            {
                int buffCount = self.GetBuffCount(DeathCountBuff.DeathCountBuffDef) + 1;

                /*DredgedMasterprefab = (GameObject)deadMasterPrefabArray.GetValue(deadMasterPrefabArray.Length - 2);
                Array.Reverse(deadMasterPrefabArray);
                Array.Copy(deadMasterPrefabArray, 1, deadMasterPrefabArray, 0, deadMasterPrefabArray.Length - 1);
                Array.Reverse(deadMasterPrefabArray);
                DredgedPosition = (Vector3)deadPositionArray.GetValue(deadPositionArray.Length - 2);
                Array.Reverse(deadPositionArray);
                Array.Copy(deadPositionArray, 1, deadPositionArray, 0, deadPositionArray.Length - 1);
                Array.Reverse(deadPositionArray);
                DredgedInventory = (Inventory)deadInventoryArray.GetValue(deadInventoryArray.Length - 2);
                Array.Reverse(deadInventoryArray);
                Array.Copy(deadInventoryArray, 1, deadInventoryArray, 0, deadInventoryArray.Length - 1);
                Array.Reverse(deadInventoryArray);*/
                var vector = self.master.deathFootPosition;
                if (self.master.killedByUnsafeArea)
                {
                    vector = (TeleportHelper.FindSafeTeleportDestination(self.master.deathFootPosition, self, RoR2Application.rng) ?? self.master.deathFootPosition);
                }
                
                try
                {

                    if (self)
                    {
                        var summon = new MasterSummon
                        {
                            masterPrefab = MasterCatalog.GetMasterPrefab(self.master.masterIndex),
                            position = vector,
                            rotation = UnityEngine.Quaternion.identity,
                            teamIndexOverride = new TeamIndex?(self.GetComponent<CharacterBody>().teamComponent.teamIndex),
                            useAmbientLevel = true,
                            summonerBodyObject = self.master.minionOwnership ? self.master.minionOwnership.gameObject : null,
                            inventoryToCopy = self.inventory,
                            ignoreTeamMemberLimit = true,

                        };
                        CharacterMaster characterMaster = summon.Perform();

                        if (characterMaster)
                        {
                            characterMaster.GetBody().SetBuffCount(DeathCountBuff.DeathCountBuffDef.buffIndex, buffCount);
                        }
                    }

                }
                catch (Exception e)
                {

                }
                
                //isRespawning = true;
            }
        }
        private static void CreateEliteTier()
        {
            
            var baseEliteTierDefs = EliteAPI.GetCombatDirectorEliteTiers();
            var indexToInsertAt = Array.FindIndex(baseEliteTierDefs, x => x.costMultiplier >= AffixDredgedTier.costMultiplier);
            EliteAPI.AddCustomEliteTier(AffixDredgedTier);
            EliteAPI.Add(new CustomElite(AffixDredgedElite, CanAppearInEliteTiers));
        }
        private static void CombatDirector_Init(On.RoR2.CombatDirector.orig_Init orig)
        {
            orig();
            //if (EliteAPI.VanillaEliteTiers.Length > 2)
            //{
            //    // HONOR
            if (ConfigBool(DredgedHonor, DredgedEnableConfig))
            {
                CombatDirector.EliteTierDef targetTier2 = EliteAPI.VanillaEliteTiers[2];
                List<EliteDef> elites2 = targetTier2.eliteTypes.ToList();
                AffixDredgedElite.healthBoostCoefficient = ConfigFloat(DredgedHealthMult, DredgedEnableConfig) / 1.6f;
                AffixDredgedElite.damageBoostCoefficient = ConfigFloat(DredgedDamageMult, DredgedEnableConfig) / 1.3f;
                elites2.Add(AffixDredgedElite);
                targetTier2.eliteTypes = elites2.ToArray();
            }
            
            //}
            //if (EliteAPI.VanillaEliteTiers.Length > 1)
            //{
            //var eliteTier = new CombatDirector.EliteTierDef()
            //{
            //    costMultiplier = CombatDirector.baseEliteCostMultiplier * Dredged.DredgedCostMult.Value,
            //    eliteTypes = new EliteDef[] { AffixDredgedElite },
            //    canSelectWithoutAvailableEliteDef = false,
            //    isAvailable = ((SpawnCard.EliteRules rules) => Run.instance.loopClearCount >= Dredged.DredgedLoopCount.Value && rules == SpawnCard.EliteRules.Default && Run.instance.stageClearCount >= Dredged.DredgedStageCount.Value),
            //};
            //var targetTiers = CombatDirector.eliteTiers;
            //targetTiers.ToList().Add(eliteTier);
            //targetTiers.ToArray();
            //CombatDirector.eliteTiers = targetTiers;
            //Array.Resize(ref CombatDirector.eliteTiers, CombatDirector.eliteTiers.Length + 1);
            //CombatDirector.eliteTiers[CombatDirector.eliteTiers.Length - 1] = AffixDredgedTier;
            EliteDef index = RoR2Content.Elites.Ice;
            switch (ConfigFloat(DredgedTier, DredgedEnableConfig))
            {
                case 1: index = RoR2Content.Elites.Ice; break;
                case 2: index = DLC2Content.Elites.Aurelionite; break;
                case 3: index = RoR2Content.Elites.Poison; break;

            }
            foreach (EliteTierDef eliteIndex in EliteAPI.VanillaEliteTiers)
            {
                if (eliteIndex.eliteTypes.Contains(index))
                {
                    CombatDirector.EliteTierDef targetTier = eliteIndex;
                    List<EliteDef> elites = targetTier.eliteTypes.ToList();
                    AffixDredgedElite.healthBoostCoefficient = ConfigFloat(DredgedHealthMult, DredgedEnableConfig);
                    AffixDredgedElite.damageBoostCoefficient = ConfigFloat(DredgedDamageMult, DredgedEnableConfig);
                    elites.Add(AffixDredgedElite);
                    targetTier.eliteTypes = elites.ToArray();
                }
            }
            //}
        }

        private static void CharacterBody_OnBuffFirstStackGained(
           On.RoR2.CharacterBody.orig_OnBuffFirstStackGained orig,
           CharacterBody self,
           BuffDef buffDef
           )
        {
            orig(self, buffDef);
            if (buffDef == AffixDredgedBuff)
            {
                //self.AddTimedBuff(DeathCountBuff.DeathCountBuffDef, 1);
                GameObject gameObject = Object.Instantiate<GameObject>(DredgedWard);

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
            if (buffDef == AffixDredgedBuff)
            {
                //BuffWard buffWard = self.gameObject.GetComponentInChildren<BuffWard>();
                //Object.Destroy(buffWard);
            }
        }

        private static void AddContent()
        {
            ItemDisplay itemDisplays = new ItemDisplay();
            ContentAddition.AddEliteDef(AffixDredgedElite);
            ContentAddition.AddBuffDef(AffixDredgedBuff);
        }

        private static void SetupBuff()
        {
            AffixDredgedBuff = ScriptableObject.CreateInstance<BuffDef>();
            AffixDredgedBuff.name = "EliteDredgedBuff";
            AffixDredgedBuff.canStack = false;
            AffixDredgedBuff.isCooldown = false;
            AffixDredgedBuff.isDebuff = false;
            AffixDredgedBuff.buffColor = Color.white; //AffixDredgedColor;
            AffixDredgedBuff.iconSprite = eliteIcon;
        }

        private static void SetupEquipment()
        {
            AffixDredgedEquipment = ScriptableObject.CreateInstance<EquipmentDef>();
            AffixDredgedEquipment.appearsInMultiPlayer = true;
            AffixDredgedEquipment.appearsInSinglePlayer = true;
            AffixDredgedEquipment.canBeRandomlyTriggered = false;
            AffixDredgedEquipment.canDrop = false;
            AffixDredgedEquipment.colorIndex = ColorCatalog.ColorIndex.Equipment;
            AffixDredgedEquipment.cooldown = 0.0f;
            AffixDredgedEquipment.isLunar = false;
            AffixDredgedEquipment.isBoss = false;
            AffixDredgedEquipment.passiveBuffDef = AffixDredgedBuff;
            AffixDredgedEquipment.dropOnDeathChance = affixDropChance;
            AffixDredgedEquipment.enigmaCompatible = false;
            AffixDredgedEquipment.requiredExpansion = CaeliImperiumExpansionDef;
            AffixDredgedEquipment.pickupModelPrefab = PrefabAPI.InstantiateClone(MainAssets.LoadAsset<GameObject>("Assets/Models/Prefabs/AffixModel.prefab"), "PickupAffixDredged", false);
            foreach (Renderer componentsInChild in AffixDredgedEquipment.pickupModelPrefab.GetComponentsInChildren<Renderer>())
                componentsInChild.material = dredgedMat;
            AffixDredgedEquipment.pickupIconSprite = MainAssets.LoadAsset<Sprite>("Assets/Icons/DredgedAffix.png");

            AffixDredgedEquipment.nameToken = "EQUIPMENT_AFFIX_DREDGED_NAME";
            AffixDredgedEquipment.descriptionToken = "EQUIPMENT_AFFIX_DREDGED_DESC";
            AffixDredgedEquipment.pickupToken = "EQUIPMENT_AFFIX_DREDGED_PICKUP";
            AffixDredgedEquipment.loreToken = "EQUIPMENT_AFFIX_DREDGED_LORE";
            AffixDredgedEquipment.name = "AffixDredged";
        }

        private static void SetupElite()
        {
            AffixDredgedElite = ScriptableObject.CreateInstance<EliteDef>();
            AffixDredgedElite.color = Color.white;//AffixDredgedColor;
            AffixDredgedElite.eliteEquipmentDef = AffixDredgedEquipment;
            AffixDredgedElite.modifierToken = "ELITE_MODIFIER_DREDGED";
            AffixDredgedElite.name = "EliteDredged";
            AffixDredgedElite.healthBoostCoefficient = ConfigFloat(DredgedHealthMult, DredgedEnableConfig);
            AffixDredgedElite.damageBoostCoefficient = ConfigFloat(DredgedDamageMult, DredgedEnableConfig);
            AffixDredgedBuff.eliteDef = AffixDredgedElite;
        }

        private static void AddLanguageTokens()
        {
            LanguageAPI.Add("ELITE_MODIFIER_DREDGED", "Dredged {0}");
            LanguageAPI.Add("EQUIPMENT_AFFIX_DREDGED_NAME", "Dredge Aspect");
            LanguageAPI.Add("EQUIPMENT_AFFIX_DREDGED_DESC", "Become an aspect of The Dredged");
            LanguageAPI.Add("EQUIPMENT_AFFIX_DREDGED_PICKUP", "Become an aspect of The Dredged");
            LanguageAPI.Add("EQUIPMENT_AFFIX_DREDGED_LORE", "some lore stuff bla bla bla DeadBolt bla bla bla");
        }
    }
}