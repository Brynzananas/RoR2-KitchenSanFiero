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
using static KitchenSanFieroPlugin.KitchenSanFiero;
using KitchenSanFiero.Buffs;
using Object = UnityEngine.Object;
using JetBrains.Annotations;
using BepInEx.Configuration;
using RiskOfOptions.Options;
using RiskOfOptions;
using UnityEngine.XR;
using UnityEngine.Networking;
using KitchenSanFiero.Items;
using RiskOfOptions.OptionConfigs;

namespace KitchenSanFiero.Elites
{
    public static class Dredged
    {
        public static Color AffixDredgedColor = new Color(1f, 0.5f, 0.0f);
        public static EquipmentDef AffixDredgedEquipment;
        public static BuffDef AffixDredgedBuff;
        public static EliteDef AffixDredgedElite;
        public static float healthMult = 4f;
        public static float damageMult = 2f;
        public static float affixDropChance = 0.00025f;
        private static GameObject DredgedWard = PrefabAPI.InstantiateClone(Addressables.LoadAssetAsync<GameObject>("RoR2/Base/EliteHaunted/AffixHauntedWard.prefab").WaitForCompletion(), "DredgedWard");
        private static Material dredgedMat = Addressables.LoadAssetAsync<Material>("RoR2/Base/Common/VFX/matOpaqueDustLarge_BrassContraption_opt.mat").WaitForCompletion();
        private static Texture2D eliteRamp = MainAssets.LoadAsset<Texture2D>("Assets/Textures/dredged_ramp.png");
        private static Sprite eliteIcon = MainAssets.LoadAsset<Sprite>("Assets/Icons/dredged_icon.png");

        public static GameObject DredgedMasterprefab;
        public static Vector3 DredgedPosition;
        public static Inventory DredgedInventory;
        public static bool isRespawning = false;
        //public static int buffCOunt = 0;
        public static ConfigEntry<float> DredgedHealthMult;
        public static ConfigEntry<float> DredgedDamageMult;
        public static ConfigEntry<int> DredgedDamageReviveCount;
        public static ConfigEntry<float> DredgedDamageReviveMult;
        public static ConfigEntry<float> DredgedHealthReviveMult;
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
            EliteRamp.AddRamp(AffixDredgedElite, eliteRamp);
            ContentAddition.AddEquipmentDef(AffixDredgedEquipment);
            On.RoR2.CharacterBody.OnBuffFirstStackGained += CharacterBody_OnBuffFirstStackGained;
            On.RoR2.CharacterBody.OnBuffFinalStackLost += CharacterBody_OnBuffFinalStackLost;
            On.RoR2.CombatDirector.Init += CombatDirector_Init;
            On.RoR2.CharacterBody.OnDeathStart += OnDeath;
            //On.RoR2.CharacterBody.Start += OnRespawn;
            On.RoR2.CharacterMaster.OnBodyDeath += OnDeathMaster;
            GetStatCoefficients += Stats;
        }

        

        private static void AddConfigs()
        {
            DredgedHealthMult = Config.Bind<float>("Elite : Dredged",
                                         "Health Multiplier",
                                         4f,
                                         "Control the health multiplier of Dredged elite");
            DredgedDamageMult = Config.Bind<float>("Elite : Dredged",
                                         "Damage Multiplier",
                                         2f,
                                         "Control the damage multiplier of Dredged elite");
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
            DredgedEnable = Config.Bind<bool>("Elite : Dredged",
                 "Activation",
                 true,
                 "Enable Dredged elite?");
            ModSettingsManager.AddOption(new CheckBoxOption(DredgedEnable, new CheckBoxConfig() { restartRequired = true }));
            ModSettingsManager.AddOption(new FloatFieldOption(DredgedHealthMult));
            ModSettingsManager.AddOption(new FloatFieldOption(DredgedDamageMult));
            ModSettingsManager.AddOption(new IntFieldOption(DredgedDamageReviveCount));
            ModSettingsManager.AddOption(new FloatFieldOption(DredgedHealthReviveMult));
            ModSettingsManager.AddOption(new FloatFieldOption(DredgedDamageReviveMult));
        }

        private static void Stats(CharacterBody sender, StatHookEventArgs args)
        {
            if (sender.HasBuff(AffixDredgedBuff) && sender.GetBuffCount(DeathCountBuff.DeathCountBuffDef) > 0)
            {
                args.healthMultAdd += sender.GetBuffCount(DeathCountBuff.DeathCountBuffDef) * DredgedHealthReviveMult.Value / 100;
                args.damageMultAdd += sender.GetBuffCount(DeathCountBuff.DeathCountBuffDef) * DredgedDamageReviveMult.Value / 100;
            }
        }
        public static void DredgedRespawn(CharacterMaster self)
        {
            CharacterMasterNotificationQueue.SendTransformNotification(self, RoR2Content.Items.ExtraLife.itemIndex, ColdHeart.ColdHeartDef.itemIndex, CharacterMasterNotificationQueue.TransformationType.Default);
            Vector3 vector = self.deathFootPosition;
            if (self.killedByUnsafeArea)
            {
                vector = (TeleportHelper.FindSafeTeleportDestination(self.deathFootPosition, self.bodyPrefab.GetComponent<CharacterBody>(), RoR2Application.rng) ?? self.deathFootPosition);
            }
            self.Respawn(vector, Quaternion.Euler(0f, UnityEngine.Random.Range(0f, 360f), 0f), true);
            //body.AddBuff(DeathCountBuff.DeathCountBuffDef);

            self.GetBody().AddTimedBuff(RoR2Content.Buffs.Immune, 1f);
            
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
                {/*
                    if (self.IsInvoking("RespawnExtraLife"))
                    {
                    self.CancelInvoke("RespawnExtraLife");

                    }
                    if (self.IsInvoking("PlayExtraLifeSFX"))
                    {
                        self.CancelInvoke("PlayExtraLifeSFX");

                    }
                    if (self.IsInvoking("RespawnExtraLifeVoid"))
                    {
                        self.CancelInvoke("RespawnExtraLifeVoid");

                    }
                    if (self.IsInvoking("PlayExtraLifeVoidSFX"))
                    {
                        self.CancelInvoke("PlayExtraLifeVoidSFX");

                    }
                    if (self.IsInvoking("RespawnExtraLifeShrine"))
                    {
                        self.CancelInvoke("RespawnExtraLifeShrine");

                    }
                    */
                    //self.Invoke("RespawnExtraLife", 0.5f);
                    //self.Invoke("PlayExtraLifeSFX", 0f);
                    //DredgedRespawn(self, body);
                    
                }else if (body.HasBuff(AffixDredgedBuff) && body.isPlayerControlled && body.inventory.GetEquipmentIndex() == AffixDredgedEquipment.equipmentIndex)
                {
                    if (body.inventory.GetEquipmentIndex() == AffixDredgedEquipment.equipmentIndex)
                    {
                        DredgedRespawn(self); //DredgedRespawn(self, body);
                        body.inventory.GiveItem(ColdHeart.ColdHeartDef, 1);
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

                    }
                }
            }

        }
        private static void OnDeath(On.RoR2.CharacterBody.orig_OnDeathStart orig, CharacterBody self)
        {
            orig(self);
            if (self.HasBuff(AffixDredgedBuff) && self.GetBuffCount(DeathCountBuff.DeathCountBuffDef) < DredgedDamageReviveCount.Value && !self.isPlayerControlled)
            {
                int buffCount = self.GetBuffCount(DeathCountBuff.DeathCountBuffDef) + 1;
                
                DredgedMasterprefab = (GameObject)deadMasterPrefabArray.GetValue(deadMasterPrefabArray.Length - 2);
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
                Array.Reverse(deadInventoryArray);
                if (DredgedMasterprefab != null && DredgedPosition != null)
                {
                    var summon = new MasterSummon
                    {
                        masterPrefab = DredgedMasterprefab,
                        position = DredgedPosition,
                        rotation = Quaternion.identity,
                        teamIndexOverride = new TeamIndex?(self.GetComponent<CharacterBody>().teamComponent.teamIndex),
                        useAmbientLevel = true,
                        summonerBodyObject = self.gameObject,
                        inventoryToCopy = DredgedInventory,
                        ignoreTeamMemberLimit = true,

                    };
                    CharacterMaster characterMaster = summon.Perform();

                    if (characterMaster)
                    {
                        characterMaster.GetBody().SetBuffCount(DeathCountBuff.DeathCountBuffDef.buffIndex, buffCount);
                    }
                }
                else
                {
                    Debug.Log("Dredged: failed to revive");
                }
                //isRespawning = true;
            }else
            if (self.HasBuff(AffixDredgedBuff) && self.isPlayerControlled && self)
            {

            }
        }/*
        private static void OnRespawn(On.RoR2.CharacterBody.orig_Start orig, CharacterBody self)
        {
            orig(self);
            if (isRespawning)
            {
                //self.inventory.GiveEquipmentString("AffixDredged");
                for (int i = 0; i < buffCOunt; i++)
                {
                    self.AddBuff(DeathCountBuff.DeathCountBuffDef);
                }
                isRespawning = false;
            }
        }
        */
        private static void CombatDirector_Init(On.RoR2.CombatDirector.orig_Init orig)
        {
            orig();
            if (EliteAPI.VanillaEliteTiers.Length > 2)
            {
                // HONOR
                CombatDirector.EliteTierDef targetTier = EliteAPI.VanillaEliteTiers[2];
                List<EliteDef> elites = targetTier.eliteTypes.ToList();
                AffixDredgedElite.healthBoostCoefficient = DredgedHealthMult.Value / 1.6f;
                AffixDredgedElite.damageBoostCoefficient = DredgedDamageMult.Value / 1.3f;
                elites.Add(AffixDredgedElite);
                targetTier.eliteTypes = elites.ToArray();
            }
            if (EliteAPI.VanillaEliteTiers.Length > 1)
            {
                CombatDirector.EliteTierDef targetTier = EliteAPI.VanillaEliteTiers[1];
                List<EliteDef> elites = targetTier.eliteTypes.ToList();
                AffixDredgedElite.healthBoostCoefficient = DredgedHealthMult.Value;
                AffixDredgedElite.damageBoostCoefficient = DredgedDamageMult.Value;
                elites.Add(AffixDredgedElite);
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
            if (buffDef == AffixDredgedBuff)
            {
                self.AddTimedBuff(DeathCountBuff.DeathCountBuffDef, 1);
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
            AffixDredgedEquipment.pickupModelPrefab = PrefabAPI.InstantiateClone(Addressables.LoadAssetAsync<GameObject>("RoR2/Base/EliteFire/PickupEliteFire.prefab").WaitForCompletion(), "PickupAffixDredged", false);
            foreach (Renderer componentsInChild in AffixDredgedEquipment.pickupModelPrefab.GetComponentsInChildren<Renderer>())
                componentsInChild.material = dredgedMat;
            AffixDredgedEquipment.pickupIconSprite = Addressables.LoadAssetAsync<Sprite>("RoR2/Base/EliteIce/texAffixWhiteIcon.png").WaitForCompletion();

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
            AffixDredgedElite.healthBoostCoefficient = healthMult;
            AffixDredgedElite.damageBoostCoefficient = damageMult;
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