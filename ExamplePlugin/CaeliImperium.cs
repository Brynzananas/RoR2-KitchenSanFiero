using BepInEx;
using BepInEx.Configuration;
using R2API;
using RiskOfOptions;
using RoR2;
using System.Reflection;
using UnityEngine;
using UnityEngine.AddressableAssets;
using CaeliImperium.Buffs;
using CaeliImperium.Items;
using CaeliImperium.Equipment;
using CaeliImperium.Elites;
using System;
using CaeliImperium.Artifact;
using System.IO;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Linq;
using System.Security.Permissions;
using static CaeliImperium.Items.CapturedPotential;
using R2API.Utils;
using static CaeliImperium.Elites.ArchNemesis;
using RoR2.ExpansionManagement;
using RoR2.Audio;
using static RoR2.CombatDirector;
using ProperSave;
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
[assembly: HG.Reflection.SearchableAttribute.OptIn]
[assembly: HG.Reflection.SearchableAttribute.OptInAttribute]
namespace CaeliImperiumPlugin
{
    
    [BepInDependency(ItemAPI.PluginGUID, BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency(LanguageAPI.PluginGUID, BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency(EliteAPI.PluginGUID, BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency(RecalculateStatsAPI.PluginGUID, BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency(SoundAPI.PluginGUID, BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency(DotAPI.PluginGUID, BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency(ProperSavePlugin.GUID, BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency(CommandHelper.PluginGUID, BepInDependency.DependencyFlags.HardDependency)]
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.EveryoneNeedSameModVersion)]
    [System.Serializable]
    
    public class CaeliImperium : BaseUnityPlugin
    {
        public const string PluginGUID = PluginAuthor + PluginName;
        public const string PluginAuthor = "Brynzananas";
        public const string PluginName = "CaeliImperium";
        public const string PluginVersion = "0.7.4";
        public static string SavesDirectory { get; } = System.IO.Path.Combine(Application.persistentDataPath, "ArchNemesis");
        public static ExpansionDef CaeliImperiumExpansionDef = ScriptableObject.CreateInstance<ExpansionDef>();
        public static AssetBundle MainAssets;
        public static ConfigFile Config;
        public static EliteTierDef[] CanAppearInEliteTiers = EliteAPI.GetCombatDirectorEliteTiers();
        public static UnityEngine.Vector3[] deadPositionArray = new Vector3[420];
        public static Inventory[] deadInventoryArray = new Inventory[420];
        public static GameObject[] deadMasterPrefabArray = new GameObject[420];
        public static GameObject deadMasterprefab;
        public static GameObject MithrixBody = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Brother/BrotherMaster.prefab").WaitForCompletion();
        public static GameObject VoidlingBody = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/VoidRaidCrab/VoidRaidCrabMaster.prefab").WaitForCompletion();
        public static GameObject FalseSonBody = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC2/FalseSonBoss/FalseSonBossMaster.prefab").WaitForCompletion();
        public static GameObject ScavengerBody = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Scav/ScavMaster.prefab").WaitForCompletion();
        public static GameObject LunarScavenger1Body = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/ScavLunar/ScavLunar1Master.prefab").WaitForCompletion();
        public static GameObject LunarScavenger2Body = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/ScavLunar/ScavLunar2Master.prefab").WaitForCompletion();
        public static GameObject LunarScavenger3Body = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/ScavLunar/ScavLunar3Master.prefab").WaitForCompletion();
        public static GameObject LunarScavenger4Body = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/ScavLunar/ScavLunar4Master.prefab").WaitForCompletion();
        public static UnityEngine.Vector3 deadPosition;
        public static Inventory deadInventory; 
        
        
        public static Dictionary<string, string> ShaderLookup = new Dictionary<string, string>()
        {
            {"fake ror/hopoo games/deferred/hgstandard", "shaders/deferred/hgstandard"},
            {"fake ror/hopoo games/fx/hgcloud intersection remap", "shaders/fx/hgintersectioncloudremap" },
            {"fake ror/hopoo games/fx/hgcloud remap", "shaders/fx/hgcloudremap" },
            {"fake ror/hopoo games/fx/hgdistortion", "shaders/fx/hgdistortion" },
            {"fake ror/hopoo games/deferred/hgsnow topped", "shaders/deferred/hgsnowtopped" },
            {"fake ror/hopoo games/fx/hgsolid parallax", "shaders/fx/hgsolidparallax" }
        };
        public static Dictionary<string, string> ShaderLookup3 = new Dictionary<string, string>()
        {
            {"stubbed ro r2/base/shaders/hg standard", "shaders/deferred/hgstandard"},
            {"stubbed ro r2/base/shaders/hg intersection cloud remap", "shaders/fx/hgintersectioncloudremap" },
            {"stubbed ro r2/base/shaders/hg cloud remap", "shaders/fx/hgcloudremap" },
            {"stubbed ro r2/base/shaders/hg distortion", "shaders/fx/hgdistortion" },
            {"stubbed ro r2/base/shaders/hg snow topped", "shaders/deferred/hgsnowtopped" },
            {"stubbed ro r2/base/shaders/hg solid parallax", "shaders/fx/hgsolidparallax" }
        };
        public static Dictionary<string, string> ShaderLookup4 = new Dictionary<string, string>()
        {
            {"stubbedror2/base/shaders/hgstandard", "shaders/deferred/hgstandard"},
            {"stubbedror2/base/shaders/hgintersectioncloudremap", "shaders/fx/hgintersectioncloudremap" },
            {"stubbedror2/base/shaders/hgcloudremap", "shaders/fx/hgcloudremap" },
            {"stubbedror2/base/shaders/hgdistortion", "shaders/fx/hgdistortion" },
            {"stubbedror2/base/shaders/hgsnowtopped", "shaders/deferred/hgsnowtopped" },
            {"stubbedror2/base/shaders/hgsolidparallax", "shaders/fx/hgsolidparallax" }
        };
        public static Dictionary<string, string> ShaderLookup2 = new Dictionary<string, string>() // Strings of stubbed vs. real shaders
        {
            {"stubbed hopoo games/deferred/standard", "shaders/deferred/hgstandard"},
            {"stubbed hopoo games/fx/cloud intersection remap", "shaders/fx/hgintersectioncloudremap"},
            {"stubbed hopoo games/fx/cloud remap", "shaders/fx/hgcloudremap"},
            {"stubbed hopoo games/fx/opaque cloud remap", "shaders/fx/hgopaquecloudremap"},
            {"stubbed hopoo games/fx/distortion", "shaders/fx/hgdistortion"}
        };
        public static List<Material> SwappedMaterials = new List<Material>();
        bool restartRequired = true;
        
        public static BepInEx.Logging.ManualLogSource ModLogger;
        //public static CombatDirector.EliteTierDef[] CaeliImperiumEliteTier = new CombatDirector.EliteTierDef[4];
            

        public void Awake()
        {

            Config = new ConfigFile(Paths.ConfigPath + "\\BrynzananasCaeliImperium.cfg", true);

            ModLogger = Logger;
            


            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("CaeliImperium.assets"))
            {
                MainAssets = AssetBundle.LoadFromStream(stream);
            }
            
            foreach (Material material in MainAssets.LoadAllAssets<Material>())
            {
                if (!material.shader.name.StartsWith("StubbedRoR2"))
                {
                    continue;
                }
                var replacementShader = Resources.Load<Shader>(ShaderLookup4[material.shader.name.ToLower()]);
                if (replacementShader)
                {
                    material.shader = replacementShader;
                }
            }
            //ShaderConversion(MainAssets);
            GenerateExpansionDef();
            using (var bankStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("CaeliImperium.KSFsounds.bnk"))
            {
                var bytes = new byte[bankStream.Length];
                bankStream.Read(bytes, 0, bytes.Length);
                SoundAPI.SoundBanks.Add(bytes);
            }
            ModSettingsManager.SetModIcon(MainAssets.LoadAsset<Sprite>("Assets/Icons/ModIcon.png"));
            On.RoR2.CharacterBody.OnDeathStart += BufferDeaths;
            On.RoR2.CharacterBody.HandleOnKillEffectsServer += ifItKillsPlayer;
            Stage.onStageStartGlobal += ResetArrays;
            //VoidLunarTier.Init();
            WoundedBuff.Init();
            IrradiatedBuff.Init();
            DazzledBuff.Init();
            PackOfCiggaretes.Init();
            GuardianCrown.Init();
            Painkillers.Init();
            EnergyChocolateBar.Init();
            FragileGiftBox.Init();
            Necronomicon.Init();
            new Hasting();
            BrassModality.Init();
            Dredged.Init();
            EmergencyMedicalTreatment.Init();
            EnforcerHand.Init();
            CapturedPotential.Init();
            Battle.Init();
            SkullGammaGun.Init();
            BrassBell.Init();
            OtherworldlyManuscript.Init();
            ArchNemesis.Init();
            Keychain.Init();
            MajesticHand.Init();
            Defender.Init();
            RejectedDagger.Init();
            LikeADragon.Init();
            OpposingForce.Init();
            //CreateEliteTiers();
        }
        //public static void ShaderConversion(AssetBundle assets)
        //{
        //    var materialAssets = assets.LoadAllAssets<Material>().Where(material => material.shader.name.StartsWith("Fake RoR"));

        //    foreach (Material material in materialAssets)
        //    {
        //        var replacementShader = LegacyResourcesAPI.Load<Shader>(ShaderLookup2[material.shader.name.ToLowerInvariant()]);
        //        if (replacementShader)
        //        {
        //            material.shader = replacementShader;
        //            SwappedMaterials.Add(material);
        //        }
        //    }
        //}

            

        private void Update()
        {
            if (!PauseManager.isPaused && CapturedPotential.CapturedPotentialEnable.Value && NetworkUser.readOnlyLocalPlayersList.Count > 0 && NetworkUser.readOnlyLocalPlayersList[0].master?.inventory && NetworkUser.readOnlyLocalPlayersList[0].master?.inventory.GetItemCount(CapturedPotentialItemDef) > 0)
            {

                //var equipArray = GetOrCreateComponent(body.master).equipArray;
                bool key1 = true;
                if (CapturedPotentialKey1.Value.MainKey != KeyCode.None)
                {
                    key1 = Input.GetKey(CapturedPotentialKey1.Value.MainKey);
                }
                bool mouseWheel = false;
                if (CapturedPotentialWheel.Value)
                {
                    mouseWheel = true;
                }
                  
                if (key1 && ((Input.mouseScrollDelta == Vector2.up && mouseWheel) || (Input.GetKeyUp(CapturedPotentialKey2.Value.MainKey) || Input.GetKeyUp(CapturedPotentialKey4.Value.MainKey))))
                {
                    RoR2.Console.instance.SubmitCmd(NetworkUser.readOnlyLocalPlayersList.FirstOrDefault(), "EquipArrayIndexUp");
                    if (CapturedPotentialSound.Value)
                    {
                    EntitySoundManager.EmitSoundServer(EquipArrayUpSound.akId, NetworkUser.readOnlyLocalPlayersList[0].master?.GetBody().gameObject);

                    }
                    
                }
                if (key1 && ((Input.mouseScrollDelta == Vector2.down && mouseWheel) || (Input.GetKeyUp(CapturedPotentialKey3.Value.MainKey) || Input.GetKeyUp(CapturedPotentialKey5.Value.MainKey))))
                {
                    RoR2.Console.instance.SubmitCmd(NetworkUser.readOnlyLocalPlayersList.FirstOrDefault(), "EquipArrayIndexDown");
                    if (CapturedPotentialSound.Value)
                    {
                    EntitySoundManager.EmitSoundServer(EquipArrayDownSound.akId, NetworkUser.readOnlyLocalPlayersList[0].master?.GetBody().gameObject);

                    }
                }
            }
        }
        /*private static void CreateEliteTiers()
        {
            CaeliImperiumEliteTier =
            {
                new CombatDirector.EliteTierDef()
                {
                    costMultiplier = CombatDirector.baseEliteCostMultiplier * Defender.DefenderCostMult.Value,
                    eliteTypes = new EliteDef[0],// {Defender.AffixDefenderElite},
                    isAvailable = ((SpawnCard.EliteRules rules) => Run.instance.loopClearCount >= Defender.DefenderLoopCount.Value && rules == SpawnCard.EliteRules.Default && Run.instance.stageClearCount >= Defender.DefenderStageCount.Value),

                },
                new CombatDirector.EliteTierDef()
                {
                    costMultiplier = CombatDirector.baseEliteCostMultiplier * BrassModality.BrassModalityCostMult.Value,
                    eliteTypes = new EliteDef[0],// {Defender.AffixDefenderElite},
                    isAvailable = ((SpawnCard.EliteRules rules) => Run.instance.loopClearCount >= BrassModality.BrassModalityLoopCount.Value && rules == SpawnCard.EliteRules.Default && Run.instance.stageClearCount >= BrassModality.BrassModalityStageCount.Value),
                },
                new CombatDirector.EliteTierDef()
                {
                    costMultiplier = CombatDirector.baseEliteCostMultiplier * Dredged.DredgedCostMult.Value,
                    eliteTypes = new EliteDef[0],// {Defender.AffixDefenderElite},
                    isAvailable = ((SpawnCard.EliteRules rules) => Run.instance.loopClearCount >= Dredged.DredgedLoopCount.Value && rules == SpawnCard.EliteRules.Default && Run.instance.stageClearCount >= Dredged.DredgedStageCount.Value),
                },
                new CombatDirector.EliteTierDef()
                {
                    costMultiplier = CombatDirector.baseEliteCostMultiplier * Hasting.HastingCostMult.Value,
                    eliteTypes = new EliteDef[0],// {Defender.AffixDefenderElite},
                    isAvailable = ((SpawnCard.EliteRules rules) => Run.instance.loopClearCount >= Hasting.HastingLoopCount.Value && rules == SpawnCard.EliteRules.Default && Run.instance.stageClearCount >= Hasting.HastingStageCount.Value),
                }
            };
            foreach(CombatDirector.EliteTierDef eliteTierDef in CaeliImperiumEliteTier)
            {
            EliteAPI.AddCustomEliteTier(eliteTierDef);

            }
        }*/

        public void GenerateExpansionDef()
        {
            LanguageAPI.Add("CAELI_IMPERIUM_EXPANSION_DEF_NAME", "Caeli Imperium");
            LanguageAPI.Add("CAELI_IMPERIUM_EXPANSION_DEF_DESCRIPTION", "Brynzananas content mod");

            CaeliImperiumExpansionDef.descriptionToken = "CAELI_IMPERIUM_EXPANSION_DEF_DESCRIPTION";
            CaeliImperiumExpansionDef.nameToken = "CAELI_IMPERIUM_EXPANSION_DEF_NAME";
            CaeliImperiumExpansionDef.iconSprite = MainAssets.LoadAsset<Sprite>("Assets/Icons/ModIcon.png");
            CaeliImperiumExpansionDef.disabledIconSprite = MainAssets.LoadAsset<Sprite>("Assets/Icons/ModIconDisabled.png");

            R2API.ContentAddition.AddExpansionDef(CaeliImperiumExpansionDef);
        }


        public static class ProperSaveCompatibility
        {
            private static bool? _enabled;

            public static bool enabled
            {
                get
                {
                    if (_enabled == null)
                    {
                        _enabled = BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("com.KingEnderBrine.ProperSave");
                    }
                    return (bool)_enabled;
                }
            }

            [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
            public static void SomeMethodThatRequireTheDependencyToBeHere()
            {
                // stuff that require the dependency to be loaded
            }
        }/*
        public static event Action FinishedLoadingCompatability;

        public static void FinishedLoading()
        {
            FinishedLoadingCompatability.Invoke();
        }*/
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float ConvertAmplificationPercentageIntoReductionPercentage(float amplificationPercentage, float maxChance)
        {
            return (1f - maxChance / (maxChance + amplificationPercentage)) * maxChance;
        }
        private void ResetArrays(Stage stage)
        {
            Array.Clear(deadPositionArray, 0, deadPositionArray.Length);
            Array.Clear(deadInventoryArray, 0, deadInventoryArray.Length);
            Array.Clear(deadMasterPrefabArray, 0, deadMasterPrefabArray.Length);
        }
        
        //public void OnEnable()
        //{
        //    //MainAssets ??= AssetBundle.LoadFromFile(System.IO.Path.Combine(System.IO.Path.GetDirectoryName(Info.Location), "KitchenSanFiero"));
        //    base.StartCoroutine(MainAssets.UpgradeStubbedShadersAsync());
        //}
        private void ifItKillsPlayer(On.RoR2.CharacterBody.orig_HandleOnKillEffectsServer orig, CharacterBody self, DamageReport damageReport)
        {
            orig(self, damageReport);
            if (self && damageReport.victimBody && damageReport.victimBody.isPlayerControlled && damageReport.victimBody.teamComponent.teamIndex == TeamIndex.Player && self.inventory.GetEquipmentIndex() != ArchNemesis.AffixArchNemesisEquipment.equipmentIndex && Stage.instance.sceneDef.stageOrder >= ArchNemesisStageBegin.Value)
            {
                if (self.isChampion && !ArchNemesisChampions.Value)
                {
                    //Debug.Log("Death by champion");
                    return;
                }
                if (self.master.masterIndex == MithrixBody.GetComponent<CharacterMaster>().masterIndex && !ArchNemesisMithrix.Value)
                {
                    //Debug.Log("Death by Mithrix");
                    return;
                }
                if (self.master.masterIndex == VoidlingBody.GetComponent<CharacterMaster>().masterIndex && ArchNemesisVoidling.Value)
                {
                    //Debug.Log("Death by Voidling");
                    return;
                }
                if (self.master.masterIndex == FalseSonBody.GetComponent<CharacterMaster>().masterIndex && ArchNemesisFalseSon.Value)
                {
                    //Debug.Log("Death by False Son");
                    return;
                }
                if (self.master.masterIndex == ScavengerBody.GetComponent<CharacterMaster>().masterIndex && ArchNemesisScavenger.Value)
                {
                    //Debug.Log("Death by Scavenger");
                    return;
                }
                if ((self.master.masterIndex == LunarScavenger1Body.GetComponent<CharacterMaster>().masterIndex ||
                    self.master.masterIndex == LunarScavenger2Body.GetComponent<CharacterMaster>().masterIndex ||
                    self.master.masterIndex == LunarScavenger3Body.GetComponent<CharacterMaster>().masterIndex ||
                    self.master.masterIndex == LunarScavenger4Body.GetComponent<CharacterMaster>().masterIndex) && ArchNemesisLunarScavengers.Value)
                {
                    //Debug.Log("Death by Lunar Scavengers");
                    return;
                }
                //archNemesisMasterPrefab = MasterCatalog.GetMasterPrefab(self.master.masterIndex);
                //archNemesisInventory = damageReport.victimBody.inventory;
                //ArchNemesisStageName = RoR2.Stage.instance.sceneDef.cachedName;
                //bool ifLoopTemp = false;
                //if (Stage.instance.sceneDef.stageOrder >= 6)
                //{
                //    ifLoopTemp = true;
                //}
                //else
                //{
                //    ifLoopTemp = false;
                //}
                try
                {
                    Directory.CreateDirectory(SavesDirectory);
                    
                    var path = System.IO.Path.Combine(SavesDirectory, "Prefab.txt");
                    var path1 = System.IO.Path.Combine(SavesDirectory, "Stage.txt");
                    var path2 = System.IO.Path.Combine(SavesDirectory, "Inventory.txt");
                    var path3 = System.IO.Path.Combine(SavesDirectory, "Team.txt");
                    var path4 = System.IO.Path.Combine(SavesDirectory, "IsDefeated.txt");
                    //Convert.ToByte(damageReport.victimBody.inventory.itemAcquisitionOrder);
                    /*BinaryFormatter formatter = new BinaryFormatter();
                    byte[] byteArray;
                    using (MemoryStream stream = new MemoryStream())
                    {
                        formatter.Serialize(stream, damageReport.victimBody.inventory.ToJson());
                        byteArray = stream.ToArray();
                    }*/
                    /*
                    for (int i = 0; i < damageReport.victimBody.inventory.itemAcquisitionOrder.ToArray().Length; i++)
                    {
                        result += 1;
                    }
                    string[] result = new string[0];
                    var toWrite = damageReport.victimBody.inventory;
                    ;
                    Array.Resize(ref result, damageReport.victimBody.inventory.itemAcquisitionOrder.ToArray().Length);
                    for (int i = 0; i < damageReport.victimBody.inventory.itemAcquisitionOrder.ToArray().Length - 1; i++)
                    {
                        result.SetValue(damageReport.victimBody.inventory.itemAcquisitionOrder.ToArray().GetValue(i), i);
                    }
                    string.Join(",", result);
                    //string result = damageReport.victimBody.inventory.itemAcquisitionOrder.ToArray().Join("," result);*/
                    string result = string.Concat(damageReport.victimBody.inventory.itemStacks.Select(x => x.ToString()));
                    int x = 0;
                    int y = 0;
                    while (y < result.Length)
                    {
                        string toParse = result[y].ToString();
                        if (int.Parse(toParse) > 0)
                        {
                            x++;
                        }
                        y++;

                    }
                    string[] itemsArray = new string[x*2];
                    int j = 0;
                    for (int i = 0; i < result.Length; i++)
                    {
                        string toParse = result[i].ToString();
                        if (int.Parse(toParse) > 0)
                        {

                            itemsArray[j] = ItemCatalog.GetItemDef((ItemIndex)i).ToString().Replace(" (RoR2.ItemDef)", "");
                            j++;
                            itemsArray[j] = result[i].ToString();
                            j++;
                        }
                        
                    }
                    File.WriteAllText(path, self.master.name.ToString().Replace("(Clone)", ""));
                    File.WriteAllText(path2, string.Join(",", itemsArray));
                    File.WriteAllText(path1, RoR2.Stage.instance.sceneDef.stageOrder.ToString());
                    File.WriteAllText(path3, ((int)self.teamComponent.teamIndex).ToString());
                    File.WriteAllText(path4, "False");
                }
                catch (Exception e)
                {
                    Debug.LogError("Failed to save Arch Nemesis");
                    Debug.LogError(e);
                }
            }
            
        }/*
        public static object RawDeserializeEx(byte[] rawdatas, Type anytype)
        {
            int rawsize = Marshal.SizeOf(anytype);
            if (rawsize > rawdatas.Length)
                return null;
            GCHandle handle = GCHandle.Alloc(rawdatas, GCHandleType.Pinned);
            IntPtr buffer = handle.AddrOfPinnedObject();
            object retobj = Marshal.PtrToStructure(buffer, anytype);
            handle.Free();
            return retobj;
        }

        public static byte[] RawSerializeEx(object anything)
        {
            int rawsize = Marshal.SizeOf(anything);
            byte[] rawdatas = new byte[rawsize];
            GCHandle handle = GCHandle.Alloc(rawdatas, GCHandleType.Pinned);
            IntPtr buffer = handle.AddrOfPinnedObject();
            Marshal.StructureToPtr(anything, buffer, false);
            handle.Free();
            return rawdatas;
        }*/
        private void BufferDeaths(On.RoR2.CharacterBody.orig_OnDeathStart orig, CharacterBody self)
        {
            orig(self);

            if (self && !self.isPlayerControlled && self.inventory && self.inventory.GetItemCount(RoR2Content.Items.Ghost) <= 0 && self.inventory.GetEquipmentIndex() != Dredged.AffixDredgedEquipment.equipmentIndex)
            {

                deadPosition = self.transform.position;
                deadPositionArray.SetValue(deadPosition, deadPositionArray.Length - 1);
                Array.Copy(deadPositionArray, 1, deadPositionArray, 0, deadPositionArray.Length - 1);
                deadMasterprefab = MasterCatalog.GetMasterPrefab(self.master.masterIndex);
                deadMasterPrefabArray.SetValue(deadMasterprefab, deadMasterPrefabArray.Length - 1);
                Array.Copy(deadMasterPrefabArray, 1, deadMasterPrefabArray, 0, deadMasterPrefabArray.Length - 1);
                deadInventory = self.inventory;
                deadInventoryArray.SetValue(deadInventory, deadInventoryArray.Length - 1);
                Array.Copy(deadInventoryArray, 1, deadInventoryArray, 0, deadInventoryArray.Length - 1);
            }
        }
    }
}
