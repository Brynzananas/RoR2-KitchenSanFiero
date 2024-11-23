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
using static CaeliImperium.Equipment.Necronomicon;
using Object = UnityEngine.Object;
using UnityEngine.UIElements;
using UnityEngine.XR;
using static RoR2.MasterSpawnSlotController;
using static ReignFromGreatBeyondPlugin.CaeliImperium;
using UnityEngine.Networking;
using CaeliImperium.Items;
using BepInEx.Configuration;
using RiskOfOptions;
using RiskOfOptions.Options;
using System.IO;
using static RoR2.MasterCatalog;
using System.Collections;
using System.Runtime.Serialization.Formatters.Binary;
using RiskOfOptions.OptionConfigs;
using RoR2.Navigation;

namespace CaeliImperium.Elites
{
    public static class ArchNemesis
    {
        public static Color AffixArchNemesisColor = new Color(1f, 0.5f, 0.0f);
        public static EquipmentDef AffixArchNemesisEquipment;
        public static BuffDef AffixArchNemesisBuff;
        public static EliteDef AffixArchNemesisElite;
        public static float healthMult = 6f;
        public static float damageMult = 4f;
        public static float affixDropChance = 0f;
        private static GameObject ArchNemesisWard = MainAssets.LoadAsset<GameObject>("Assets/Models/Prefabs/archnemesishat.prefab");
        private static Material ArchNemesisMat = Addressables.LoadAssetAsync<Material>("RoR2/Base/Common/VFX/matOpaqueDustLarge_BrassContraption_opt.mat").WaitForCompletion();
        private static Texture2D eliteRamp = MainAssets.LoadAsset<Texture2D>("Assets/Textures/arch_nemesis_ramp.png");
        private static Sprite eliteIcon = MainAssets.LoadAsset<Sprite>("Assets/Icons/arch_nemesis_icon.png");
        public static bool ArchNemesisSpawning = false;
        public static GameObject ArchNemesisAllyMasterprefab;
        public static GameObject ArchNemesisToCopyMasterprefab;
        public static Inventory ArchNemesisAllyInventory;
        public static Vector3 ArchNemesisAllyPosition;
        //public static bool ArchNemesisDead = false;
       // public static bool isThereArchNemesis = false;
        public static ConfigEntry<bool> ArchNemesisEnable;
        public static ConfigEntry<float> ArchNemesisHealthMult;
        public static ConfigEntry<float> ArchNemesisDamageMult;
        public static ConfigEntry<int> ArchNemesisStageBegin;
        public static ConfigEntry<bool> ArchNemesisAIBlacklistItems;
        public static ConfigEntry<bool> ArchNemesisChampions;
        public static ConfigEntry<bool> ArchNemesisMithrix;
        public static ConfigEntry<bool> ArchNemesisVoidling;
        public static ConfigEntry<bool> ArchNemesisFalseSon;
        public static ConfigEntry<bool> ArchNemesisScavenger;
        public static ConfigEntry<bool> ArchNemesisLunarScavengers;
        public static ConfigEntry<float> ArchNemesisDropChance;

        // RoR2/Base/Common/ColorRamps/texRampWarbanner.png 

        public static void Init()
        {
            //ArchNemesisWard.transform.GetChild(0).GetChild(0).GetComponent<MeshRenderer>().material = ArchNemesisMat;
            AddConfigs();
            if (!ArchNemesisEnable.Value)
            {
                return;
            }
            AddLanguageTokens();
            SetupBuff();
            SetupEquipment();
            SetupElite();
            AddContent();
            CoolHat.Init();
            EliteRamp.AddRamp(AffixArchNemesisElite, eliteRamp);
            ContentAddition.AddEquipmentDef(AffixArchNemesisEquipment);
            On.RoR2.CharacterBody.OnBuffFirstStackGained += CharacterBody_OnBuffFirstStackGained;
            On.RoR2.CharacterBody.OnBuffFinalStackLost += CharacterBody_OnBuffFinalStackLost;
            GetStatCoefficients += Stats;
            RoR2.Stage.onStageStartGlobal += SpawnArchNemesis;
            //On.RoR2.CharacterBody.Start += SetArchNemesis;
            On.RoR2.CharacterBody.OnDeathStart += RemoveArchNemesis;
            Run.onRunStartGlobal += RespawnArchNemesis;
        }

        private static void AddConfigs()
        {
            ArchNemesisEnable = Config.Bind<bool>("Elite : Arch Nemesis",
                 "Activation",
                 true,
                 "Enable this elite?");
            ArchNemesisHealthMult = Config.Bind<float>("Elite : Arch Nemesis",
                                         "Health Multiplier",
                                         6f,
                                         "Control the health multiplier of Arch Nemesis elite");
            ArchNemesisDamageMult = Config.Bind<float>("Elite : Arch Nemesis",
                                         "Damage Multiplier",
                                         3f,
                                         "Control the damage multiplier of Arch Nemesis elite");
            ArchNemesisDropChance = Config.Bind<float>("Elite : Arch Nemesis",
                                         "Item drop chance",
                                         100f,
                                         "Control the chance Arch Nemesis elite drops its item");
            ArchNemesisStageBegin = Config.Bind<int>("Elite : Arch Nemesis",
                                         "Stage",
                                         2,
                                         "Control from which stage monsters can become an Arch Nemesis");
            ArchNemesisAIBlacklistItems = Config.Bind<bool>("Elite : Arch Nemesis",
                                         "AI Blacklisted items",
                                         false,
                                         "Can Arch Nemesis get AI Blacklisted items?");
            ArchNemesisChampions = Config.Bind<bool>("Elite : Arch Nemesis",
                                         "Champions",
                                         true,
                                         "Can champion enemies become an Arch Nemesis?");
            ArchNemesisMithrix = Config.Bind<bool>("Elite : Arch Nemesis",
                                         "Mithrix",
                                         false,
                                         "Can Mithrix become an Arch Nemesis?");
            ArchNemesisVoidling = Config.Bind<bool>("Elite : Arch Nemesis",
                                         "Voidling",
                                         false,
                                         "Can Voidling become an Arch Nemesis?");
            ArchNemesisFalseSon = Config.Bind<bool>("Elite : Arch Nemesis",
                                         "False Son",
                                         false,
                                         "Can False Son become an Arch Nemesis?");
            ArchNemesisScavenger = Config.Bind<bool>("Elite : Arch Nemesis",
                             "Scavenger",
                             false,
                             "Can Scavenger become am Arch Nemesis?");
            ArchNemesisLunarScavengers = Config.Bind<bool>("Elite : Arch Nemesis",
                             "Lunar Scavengers",
                             false,
                             "Can Lunar Scavengers become an Arch Nemesis?");
            ModSettingsManager.AddOption(new CheckBoxOption(ArchNemesisEnable, new CheckBoxConfig() { restartRequired = true }));
            ModSettingsManager.AddOption(new FloatFieldOption(ArchNemesisHealthMult));
            ModSettingsManager.AddOption(new FloatFieldOption(ArchNemesisDamageMult));
            ModSettingsManager.AddOption(new FloatFieldOption(ArchNemesisDropChance));
            ModSettingsManager.AddOption(new IntFieldOption(ArchNemesisStageBegin));
            ModSettingsManager.AddOption(new CheckBoxOption(ArchNemesisAIBlacklistItems));
            ModSettingsManager.AddOption(new CheckBoxOption(ArchNemesisChampions));
            ModSettingsManager.AddOption(new CheckBoxOption(ArchNemesisMithrix));
            ModSettingsManager.AddOption(new CheckBoxOption(ArchNemesisVoidling));
            ModSettingsManager.AddOption(new CheckBoxOption(ArchNemesisFalseSon));
            ModSettingsManager.AddOption(new CheckBoxOption(ArchNemesisScavenger));
            ModSettingsManager.AddOption(new CheckBoxOption(ArchNemesisLunarScavengers));
        }

        private static void RespawnArchNemesis(Run run)
        {
            //var path = System.IO.Path.Combine(SavesDirectory, "Prefab.txt");
            if (File.Exists(System.IO.Path.Combine(SavesDirectory, "Prefab.txt")))
            {
                File.WriteAllText(System.IO.Path.Combine(SavesDirectory, "IsDefeated.txt"), "False");
                //isThereArchNemesis = true;

            }
            /*
            var path = System.IO.Path.Combine(SavesDirectory, "Prefab");
            var path1 = System.IO.Path.Combine(SavesDirectory, "StageName");
            var path2 = System.IO.Path.Combine(SavesDirectory, "Inventory");
            var path3 = System.IO.Path.Combine(SavesDirectory, "IfLoop");
            if (File.Exists(path))
            {
                isThereArchNemesis = true ;
            archNemesisMasterPrefab = GetMasterPrefab(FindMasterIndex(File.ReadAllText(path).Replace("(Clone)", "")));

            Debug.Log("KitchenSanFieroLog: Found current Arch Nemesis: " + archNemesisMasterPrefab);
                ArchNemesisStageName = File.ReadAllText(path1);

                Debug.Log("KitchenSanFieroLog: Arch Nemesis will appear on the stage: " + ArchNemesisStageName);
                if (File.ReadAllText(path3) == "True")
                {
                ifLoop = true;

                Debug.Log("KitchenSanFieroLog: Arch Nemesis will appear only after loop");

                } else
                {
                    ifLoop= false;
                }
                archNemesisInventory = File.ReadAllText(path2);
                Debug.Log("KitchenSanFieroLog: Arch Nemesis inventory:");
                Debug.Log(archNemesisInventory);
                //Debug.Log("KitchenSanFieroLog: Arch Nemesis will appear with this inventory: " + archNemesisInventory);

            }
            else
            {
                isThereArchNemesis= false ;
                Debug.Log("KitchenSanFieroLog: Found no Arch Nemesis, keep it up!");
            }*/

        }

        private static void RemoveArchNemesis(On.RoR2.CharacterBody.orig_OnDeathStart orig, CharacterBody self)
        {
            orig(self);
            /*if (isThereArchNemesis)
            {
            archNemesisMasterPrefab = GetMasterPrefab(FindMasterIndex(File.ReadAllText(System.IO.Path.Combine(SavesDirectory, "Prefab.txt")).Replace("(Clone)", "")));
            isThereArchNemesis && MasterCatalog.GetMasterPrefab(self.master.masterIndex) == archNemesisMasterPrefab
            }*/
            if (self.HasBuff(AffixArchNemesisBuff))
            {
                //ArchNemesisAllyMasterprefab = MasterCatalog.GetMasterPrefab(self.master.masterIndex);
                ArchNemesisAllyPosition = self.transform.position;
                //ArchNemesisAllyInventory = self.inventory;
                if (Util.CheckRoll(ArchNemesisDropChance.Value))
                {
                PickupDropletController.CreatePickupDroplet(PickupCatalog.FindPickupIndex(CoolHat.CoolHatItemDef.itemIndex), self.transform.position, self.transform.rotation.eulerAngles * 20f);

                }

                //archNemesisInventory = null;
                //ArchNemesisDead = true;
                //File.Delete(System.IO.Path.Combine(SavesDirectory, "IsDefeated.txt"));
                if (File.ReadAllText(System.IO.Path.Combine(SavesDirectory, "IsDefeated.txt")) == "False")
                {
                    File.WriteAllText(System.IO.Path.Combine(SavesDirectory, "IsDefeated.txt"), "True");
                }
                
                //File.Delete(System.IO.Path.Combine(SavesDirectory, "Prefab.txt"));
                //File.Delete(System.IO.Path.Combine(SavesDirectory, "Stage.txt"));
                //File.Delete(System.IO.Path.Combine(SavesDirectory, "Team.txt"));
            }
        }
        /*
        private static void SetArchNemesis(On.RoR2.CharacterBody.orig_Start orig, CharacterBody self)
        {
            orig(self);
            if (ArchNemesisSpawning)
            {
                archNemesisMasterPrefab = GetMasterPrefab(FindMasterIndex(File.ReadAllText(System.IO.Path.Combine(SavesDirectory, "Prefab.txt")).Replace("(Clone)", "")));
                if (MasterCatalog.GetMasterPrefab(self.master.masterIndex) == archNemesisMasterPrefab && self.teamComponent.teamIndex != TeamIndex.Player && MasterCatalog.GetMasterPrefab(self.master.masterIndex) != null)
                {
                    self.inventory.SetEquipmentIndex(AffixArchNemesisEquipment.equipmentIndex);
                    ArchNemesisSpawning = false;
                }
            }
        }*/

        private static void SpawnArchNemesis(Stage stage)
        {
            if (NetworkServer.active && File.Exists(System.IO.Path.Combine(SavesDirectory, "Prefab.txt")))
            {
                int ArchNemesisStageCount = int.Parse(File.ReadAllText(System.IO.Path.Combine(SavesDirectory, "Stage.txt")));
                string isDefeated = File.ReadAllText(System.IO.Path.Combine(SavesDirectory, "IsDefeated.txt"));
                if (stage.sceneDef.stageOrder >= ArchNemesisStageCount && isDefeated == "False")
                {

                TeamIndex ArchNemesisTeam = (TeamIndex)int.Parse(File.ReadAllText(System.IO.Path.Combine(SavesDirectory, "Team.txt")));

                    //Chat.AddMessage("There is an Arch Nemesis here");
                    GameObject archNemesisMasterPrefab = GetMasterPrefab(FindMasterIndex(File.ReadAllText(System.IO.Path.Combine(SavesDirectory, "Prefab.txt")).Replace("(Clone)", "")));
                    NodeGraph nodeGraph = SceneInfo.instance.GetNodeGraph(MapNodeGroup.GraphType.Ground);
                    List<NodeGraph.NodeIndex> source = nodeGraph.FindNodesInRange(stage.transform.position, 50f, 200f, HullMask.Human);
                    Vector3 vector;
                    nodeGraph.GetNodePosition(source.First<NodeGraph.NodeIndex>(), out vector);
                    var summon = new MasterSummon
                    {

                        masterPrefab = archNemesisMasterPrefab,
                        position = vector,
                        rotation = Quaternion.identity,
                        teamIndexOverride = ArchNemesisTeam,
                        useAmbientLevel = true,
                        //summonerBodyObject = slot.gameObject,
                        //inventoryToCopy = NecronomiconInventory,
                        ignoreTeamMemberLimit = true,

                    };
                    CharacterMaster characterMaster = summon.Perform();

                    if (characterMaster)
                    {
                        characterMaster.inventory.SetEquipmentIndex(AffixArchNemesisEquipment.equipmentIndex);

                    }
                }/*
                if (!ArchNemesisDead && stage.sceneDef.cachedName == ArchNemesisStageName && stage.sceneDef.cachedName != null)
                {

                    if (File.ReadAllText(System.IO.Path.Combine(SavesDirectory, "IfLoop")) == "True")
                    {
                        ifLoop = true;

                    }
                    else
                    {
                        ifLoop = false;
                    }
                    bool IsLoop = stage.sceneDef.stageOrder >= 6;
                    if (IsLoop || !ifLoop)
                    {
                        ArchNemesisSpawning = true;

                    }
                }*/
            }
        }

        private static void Stats(CharacterBody sender, StatHookEventArgs args)
        {

            if (sender.HasBuff(AffixArchNemesisBuff))
            {
                //args.damageMultAdd = 1f;
                args.attackSpeedMultAdd += 1f;
                args.critAdd += 25f;
                args.armorAdd += 100f;
                args.moveSpeedMultAdd += 0.5f;
                args.cooldownMultAdd += -0.25f;

            }

        }

        private static void CharacterBody_OnBuffFirstStackGained(
           On.RoR2.CharacterBody.orig_OnBuffFirstStackGained orig,
           CharacterBody self,
           BuffDef buffDef
           )
        {
            orig(self, buffDef);
            if (buffDef == AffixArchNemesisBuff)
            {
                //Chat.AddMessage("Arch Nemesis has been spawned");
                string archNemesisInventory = File.ReadAllText(System.IO.Path.Combine(SavesDirectory, "Inventory.txt"));
                if (archNemesisInventory != null)
                {
                    string[] archNemesisInventoryArray = archNemesisInventory.Split(",");
                    Vector3 position = self.transform.position + 2f * Vector3.forward;
                    for (int i = 0; i < archNemesisInventoryArray.Length; i++, i++)
                    {
                        try
                        {
                            string toParse = archNemesisInventoryArray[i + 1];
                            int itemCount = int.Parse(toParse);
                            if (!ItemCatalog.GetItemDef(ItemCatalog.FindItemIndex(archNemesisInventoryArray[i])).ContainsTag(ItemTag.AIBlacklist) && ArchNemesisAIBlacklistItems.Value)
                            {
                                self.inventory.GiveItemString(archNemesisInventoryArray[i], itemCount);

                            }

                        }
                        catch (Exception e)
                        {
                            Debug.LogError(e);
                        }
                        
                    }
                }
                
                /*
                for (int i = 0; i < archNemesisInventory.Length; i++)
                {

                    self.inventory.GiveItem(ItemIndex.Count, 5);
                }*/
                //self.inventory.AddItemsFrom(archNemesisInventory);
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
            if (buffDef == AffixArchNemesisBuff)
            {
                //BuffWard buffWard = self.gameObject.GetComponentInChildren<BuffWard>();
                //Object.Destroy(buffWard);
            }
        }

        private static void AddContent()
        {
            ItemDisplay itemDisplays = new ItemDisplay();
            ContentAddition.AddEliteDef(AffixArchNemesisElite);
            ContentAddition.AddBuffDef(AffixArchNemesisBuff);
        }

        private static void SetupBuff()
        {
            AffixArchNemesisBuff = ScriptableObject.CreateInstance<BuffDef>();
            AffixArchNemesisBuff.name = "EliteArchNemesisBuff";
            AffixArchNemesisBuff.canStack = false;
            AffixArchNemesisBuff.isCooldown = false;
            AffixArchNemesisBuff.isDebuff = false;
            AffixArchNemesisBuff.buffColor = Color.white; // AffixArchNemesisColor;
            AffixArchNemesisBuff.iconSprite = eliteIcon;
        }

        private static void SetupEquipment()
        {
            AffixArchNemesisEquipment = ScriptableObject.CreateInstance<EquipmentDef>();
            AffixArchNemesisEquipment.name = "AffixArchNemesis";
            AffixArchNemesisEquipment.nameToken = "EQUIPMENT_AFFIX_ARCHNEMESIS_NAME";
            AffixArchNemesisEquipment.pickupToken = "EQUIPMENT_AFFIX_ARCHNEMESIS_PICKUP";
            AffixArchNemesisEquipment.descriptionToken = "EQUIPMENT_AFFIX_ARCHNEMESIS_DESC";
            AffixArchNemesisEquipment.loreToken = "EQUIPMENT_AFFIX_ARCHNEMESIS_LORE";
            AffixArchNemesisEquipment.appearsInMultiPlayer = true;
            AffixArchNemesisEquipment.appearsInSinglePlayer = true;
            AffixArchNemesisEquipment.canBeRandomlyTriggered = false;
            AffixArchNemesisEquipment.canDrop = false;
            AffixArchNemesisEquipment.colorIndex = ColorCatalog.ColorIndex.Equipment;
            AffixArchNemesisEquipment.cooldown = 0.0f;
            AffixArchNemesisEquipment.isLunar = false;
            AffixArchNemesisEquipment.isBoss = false;
            AffixArchNemesisEquipment.passiveBuffDef = AffixArchNemesisBuff;
            AffixArchNemesisEquipment.dropOnDeathChance = affixDropChance;
            AffixArchNemesisEquipment.enigmaCompatible = false;
            AffixArchNemesisEquipment.requiredExpansion = CaeliImperiumExpansionDef;
            AffixArchNemesisEquipment.pickupModelPrefab = MainAssets.LoadAsset<GameObject>("Assets/Models/Prefabs/AffixArchNemesisModel.prefab"); ;
            AffixArchNemesisEquipment.pickupIconSprite = Addressables.LoadAssetAsync<Sprite>("RoR2/Base/EliteIce/texAffixWhiteIcon.png").WaitForCompletion();
            ItemDisplayRuleDict rules = new ItemDisplayRuleDict();
            rules.Add("mdlCommandoDualies", new RoR2.ItemDisplayRule[]{
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ArchNemesisWard,
                    childName = "Head",
localPos = new Vector3(0F, 0.18532F, 0.00523F),
localAngles = new Vector3(337.5209F, 0F, 0F),
localScale = new Vector3(0.32262F, 0.32262F, 0.32262F)
                }
            });
            rules.Add("mdlHuntress", new RoR2.ItemDisplayRule[]{
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ArchNemesisWard,
                    childName = "Head",
localPos = new Vector3(0F, 0.20189F, -0.0512F),
localAngles = new Vector3(327.8554F, 0F, 0F),
localScale = new Vector3(0.22833F, 0.22833F, 0.22833F)
                }
            });
            rules.Add("mdlBandit2", new RoR2.ItemDisplayRule[]{
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ArchNemesisWard,
                    childName = "Hat",
localPos = new Vector3(-0.00005F, 0.02792F, -0.00736F),
localAngles = new Vector3(334.5102F, 0F, 0F),
localScale = new Vector3(0.19204F, 0.19204F, 0.19204F)
                }
            });
            rules.Add("mdlToolbot", new RoR2.ItemDisplayRule[]{
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ArchNemesisWard,
                    childName = "HeadCenter",
localPos = new Vector3(0F, 0.08135F, 0.25116F),
localAngles = new Vector3(67.9741F, 0F, 0F),
localScale = new Vector3(3.32953F, 3.32953F, 3.32953F)
                }
            });
            rules.Add("mdlEngi", new RoR2.ItemDisplayRule[]{
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ArchNemesisWard,
                    childName = "Chest",
localPos = new Vector3(0F, 0.61285F, 0.03658F),
localAngles = new Vector3(14.10453F, 0F, 0F),
localScale = new Vector3(0.30164F, 0.30164F, 0.30164F)
                }
            });
            rules.Add("mdlEngiTurrety", new RoR2.ItemDisplayRule[]{
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ArchNemesisWard,
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
                    followerPrefab = ArchNemesisWard,
                    childName = "Head",
localPos = new Vector3(-0.0005F, 0.09323F, -0.08117F),
localAngles = new Vector3(343.4258F, 359.9102F, 0.02939F),
localScale = new Vector3(0.2379F, 0.2379F, 0.2379F)
                }
            });
            rules.Add("mdlMerc", new RoR2.ItemDisplayRule[]{
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ArchNemesisWard,
                    childName = "Head",
localPos = new Vector3(0F, 0.14434F, 0.0382F),
localAngles = new Vector3(353.2025F, 0F, 0F),
localScale = new Vector3(0.23293F, 0.23293F, 0.23293F)
                }
            });
            rules.Add("mdlTreebot", new RoR2.ItemDisplayRule[]{
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ArchNemesisWard,
                    childName = "Chest",
                    localPos = new Vector3(0f, 0f, 0f),
                    localAngles = new Vector3(0f, 0f, 0f),
                    localScale = new Vector3(1f, 1f, 1f)
                }
            });
            rules.Add("mdlLoader", new RoR2.ItemDisplayRule[]{
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ArchNemesisWard,
                    childName = "Chest",
                    localPos = new Vector3(0f, 0f, 0f),
                    localAngles = new Vector3(0f, 0f, 0f),
                    localScale = new Vector3(1f, 1f, 1f)
                }
            });
            rules.Add("mdlCroco", new RoR2.ItemDisplayRule[]{
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ArchNemesisWard,
                    childName = "Chest",
                    localPos = new Vector3(0f, 0f, 0f),
                    localAngles = new Vector3(0f, 0f, 0f),
                    localScale = new Vector3(1f, 1f, 1f)
                }
            });
            rules.Add("mdlCaptain", new RoR2.ItemDisplayRule[]{
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ArchNemesisWard,
                    childName = "Chest",
                    localPos = new Vector3(0f, 0f, 0f),
                    localAngles = new Vector3(0f, 0f, 0f),
                    localScale = new Vector3(1f, 1f, 1f)
                }
            });
            rules.Add("mdlRailGunner", new RoR2.ItemDisplayRule[]{
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ArchNemesisWard,
                    childName = "Chest",
                    localPos = new Vector3(0f, 0f, 0f),
                    localAngles = new Vector3(0f, 0f, 0f),
                    localScale = new Vector3(1f, 1f, 1f)
                }
            });
            rules.Add("mdlVoidSurvivor", new RoR2.ItemDisplayRule[]{
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ArchNemesisWard,
                    childName = "Chest",
                    localPos = new Vector3(0f, 0f, 0f),
                    localAngles = new Vector3(0f, 0f, 0f),
                    localScale = new Vector3(1f, 1f, 1f)
                }
            });
            rules.Add("mdlSeeker", new RoR2.ItemDisplayRule[]{
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ArchNemesisWard,
                    childName = "Chest",
                    localPos = new Vector3(0f, 0f, 0f),
                    localAngles = new Vector3(0f, 0f, 0f),
                    localScale = new Vector3(1f, 1f, 1f)
                }
            });
            rules.Add("mdlChef", new RoR2.ItemDisplayRule[]{
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ArchNemesisWard,
                    childName = "Chest",
                    localPos = new Vector3(0f, 0f, 0f),
                    localAngles = new Vector3(0f, 0f, 0f),
                    localScale = new Vector3(1f, 1f, 1f)
                }
            });
            rules.Add("mdlFalseSon", new RoR2.ItemDisplayRule[]{
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ArchNemesisWard,
                    childName = "Chest",
                    localPos = new Vector3(0f, 0f, 0f),
                    localAngles = new Vector3(0f, 0f, 0f),
                    localScale = new Vector3(1f, 1f, 1f)
                }
            });
            rules.Add("mdlScav", new RoR2.ItemDisplayRule[]{
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ArchNemesisWard,
                    childName = "Chest",
                    localPos = new Vector3(0f, 0f, 0f),
                    localAngles = new Vector3(0f, 0f, 0f),
                    localScale = new Vector3(1f, 1f, 1f)
                }
            });
            var displayRules = new ItemDisplayRuleDict(null);
            ItemAPI.Add(new CustomEquipment(AffixArchNemesisEquipment, displayRules));
        }

        private static void SetupElite()
        {
            AffixArchNemesisElite = ScriptableObject.CreateInstance<EliteDef>();
            AffixArchNemesisElite.color = Color.white;//AffixArchNemesisColor;
            AffixArchNemesisElite.eliteEquipmentDef = AffixArchNemesisEquipment;
            AffixArchNemesisElite.modifierToken = "ELITE_MODIFIER_ARCHNEMESIS";
            AffixArchNemesisElite.name = "EliteArchNemesis";
            AffixArchNemesisElite.healthBoostCoefficient = ArchNemesisHealthMult.Value;
            AffixArchNemesisElite.damageBoostCoefficient = ArchNemesisDamageMult.Value;
            AffixArchNemesisBuff.eliteDef = AffixArchNemesisElite;
        }

        private static void AddLanguageTokens()
        {
            LanguageAPI.Add("ELITE_MODIFIER_ARCHNEMESIS", "Arch Nemesis {0}");
            LanguageAPI.Add("EQUIPMENT_AFFIX_ARCHNEMESIS_NAME", "Arch Nemesis Aspect");
            LanguageAPI.Add("EQUIPMENT_AFFIX_ARCHNEMESIS_PICKUP", "Arch Nemesis Pickup");
            LanguageAPI.Add("EQUIPMENT_AFFIX_ARCHNEMESIS_DESC", "Arch Nemesis Description");
            LanguageAPI.Add("EQUIPMENT_AFFIX_ARCHNEMESIS_LORE", "Arch Nemesis Lore");
        }
    }
}