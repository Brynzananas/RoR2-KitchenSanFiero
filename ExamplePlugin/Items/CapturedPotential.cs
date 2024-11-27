using BepInEx.Configuration;
using R2API;
using RiskOfOptions.OptionConfigs;
using RiskOfOptions.Options;
using RiskOfOptions;
using RoR2;
using RoR2.Orbs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using static ReignFromGreatBeyondPlugin.CaeliImperium;
using static Rewired.InputMapper;
using UnityEngine.Networking;
using HG;
using CaeliImperium.Elites;
using static CaeliImperium.Artifact.Battle;
using UnityEngine.PlayerLoop;
using R2API.Networking.Interfaces;
using TMPro;
using UnityEngine.AddressableAssets;
using System.Collections;
using CaeliImperium.Buffs;
using System.Threading;
using R2API.Utils;
using ProperSave;
using System.Runtime.Serialization;
using Rewired;
using static RoR2.MasterSpawnSlotController;
using RoR2.Audio;

namespace CaeliImperium.Items
{
    internal static class CapturedPotential //: ItemBase<FirstItem>
    {
        internal static GameObject CapturedPotentialPrefab;
        internal static Sprite CapturedPotentialIcon;
        public static ItemDef CapturedPotentialItemDef;
        public static ConfigEntry<bool> CapturedPotentialEnable;
        public static ConfigEntry<float> CapturedPotentialTier;
        public static ConfigEntry<int> CapturedPotentialEquipSlots;
        public static ConfigEntry<int> CapturedPotentialEquipSlotsStack;
        public static ConfigEntry<bool> CapturedPotentialAffixes;
        public static ConfigEntry<bool> CapturedPotentialCard;
        public static ConfigEntry<bool> CapturedPotentialWheel;
        public static ConfigEntry<KeyboardShortcut> CapturedPotentialKey1;
        public static ConfigEntry<KeyboardShortcut> CapturedPotentialKey2;
        public static ConfigEntry<KeyboardShortcut> CapturedPotentialKey3;
        public static ConfigEntry<KeyboardShortcut> CapturedPotentialKey4;
        public static ConfigEntry<KeyboardShortcut> CapturedPotentialKey5;
        private static NetworkSoundEventDef EquipArrayUpSound;
        private static NetworkSoundEventDef EquipArrayDownSound;

        /*public static EquipmentIndex[] equipArray = new EquipmentIndex[0];

public static int itemCountBefore = 0;
public static bool hasIfritAffix = false;
public static bool hasColdAffix = false;
public static bool hasPoisonAffix = false;
public static bool hasBlueAffix = false;
public static bool hasHauntedAffix = false;
public static bool hasEchoAffix = false;
public static bool hasLunarAffix = false;
public static bool hasVoidAffix = false;
public static bool hasHealAffix = false;
public static bool hasCard = false;
public static bool hasBeadAffix = false;
public static bool hasAuroAffix = false;
*/
        internal static void Init()
        {
            AddConfigs();

            string tier = "Assets/Icons/BrassBellIcon.png";
            switch (CapturedPotentialTier.Value)
            {
                case 1:
                    tier = "Assets/Icons/CapturedPotentialTier1.png";
                    break;
                case 2:
                    tier = "Assets/Icons/CapturedPotential.png";
                    break;
                case 3:
                    tier = "Assets/Icons/CapturedPotentialTier3.png";
                    break;

            }
            CapturedPotentialPrefab = MainAssets.LoadAsset<GameObject>("Assets/Models/Prefabs/QuantumDeck.prefab");
            CapturedPotentialIcon = MainAssets.LoadAsset<Sprite>(tier);
            if (!CapturedPotentialEnable.Value)
            {
                return;
            }

            Item();
            CreateSound();
            AddLanguageTokens();
        }

        private static void AddConfigs()
        {
            CapturedPotentialEnable = Config.Bind<bool>("Item : Captured Potential",
                             "Activation",
                             true,
                             "Enable Captured Potential item?");
            CapturedPotentialTier = Config.Bind<float>("Item : Captured Potential",
                                         "Item tier",
                                         2f,
                                         "1: Common/White\n2: Rare/Green\n3: Legendary/Red");
            CapturedPotentialEquipSlots = Config.Bind<int>("Item : Captured Potential",
                                         "Equipment slots",
                                         2,
                                         "Control how much this item gives equipment slots");
            CapturedPotentialEquipSlotsStack = Config.Bind<int>("Item : Captured Potential",
                                         "Equipment slots stacking",
                                         1,
                                         "Control how much this item gives equipment slots per next stacks");
            CapturedPotentialAffixes = Config.Bind<bool>("Item : Captured Potential",
                                         "Affixes compatibility",
                                         true,
                                         "Enable working affixes in inventory");
            CapturedPotentialCard = Config.Bind<bool>("Item : Captured Potential",
                                         "Credit Card compatibility",
                                         true,
                                         "Enable working Credit Card in inventory");
            CapturedPotentialWheel = Config.Bind<bool>("Item : Captured Potential",
                                         "Mouse wheel",
                                         true,
                                         "Enable Mouse Wheel as equipment switchng input?");
            CapturedPotentialKey1 = Config.Bind<KeyboardShortcut>("Item : Captured Potential",
                                         "Key 1",
                                         new KeyboardShortcut(KeyCode.E),
                                         "Key to start switching equipments\nUnbind it to always switch equipments on input");
            CapturedPotentialKey2 = Config.Bind<KeyboardShortcut>("Item : Captured Potential",
                                         "Key 2",
                                         new KeyboardShortcut(KeyCode.Alpha1),
                                         "Key to switch equipment up");
            CapturedPotentialKey3 = Config.Bind<KeyboardShortcut>("Item : Captured Potential",
                                         "Key 3",
                                         new KeyboardShortcut(KeyCode.Alpha3),
                                         "Key to switch equipment down");
            CapturedPotentialKey4 = Config.Bind<KeyboardShortcut>("Item : Captured Potential",
                                         "Key 4",
                                         new KeyboardShortcut(KeyCode.Joystick1Button4),
                                         "Key to switch equipment up for game controllers\nWIP: Does not work");
            CapturedPotentialKey5 = Config.Bind<KeyboardShortcut>("Item : Captured Potential",
                                         "Key 5",
                                         new KeyboardShortcut(KeyCode.Joystick1Button6),
                                         "Key to switch equipment down for game controllers\nWIP: Does not work");
            ModSettingsManager.AddOption(new CheckBoxOption(CapturedPotentialEnable, new CheckBoxConfig() { restartRequired = true }));
            ModSettingsManager.AddOption(new StepSliderOption(CapturedPotentialTier, new StepSliderConfig() { min = 1, max = 3, increment = 1f, restartRequired = true }));
            ModSettingsManager.AddOption(new IntFieldOption(CapturedPotentialEquipSlots));
            ModSettingsManager.AddOption(new IntFieldOption(CapturedPotentialEquipSlotsStack));
            ModSettingsManager.AddOption(new CheckBoxOption(CapturedPotentialCard));
            ModSettingsManager.AddOption(new CheckBoxOption(CapturedPotentialAffixes));
            ModSettingsManager.AddOption(new CheckBoxOption(CapturedPotentialWheel));
            ModSettingsManager.AddOption(new KeyBindOption(CapturedPotentialKey1));
            ModSettingsManager.AddOption(new KeyBindOption(CapturedPotentialKey2));
            ModSettingsManager.AddOption(new KeyBindOption(CapturedPotentialKey3));
            ModSettingsManager.AddOption(new KeyBindOption(CapturedPotentialKey4));
            ModSettingsManager.AddOption(new KeyBindOption(CapturedPotentialKey5));
        }
        private static void CreateSound()
        {
            EquipArrayUpSound = ScriptableObject.CreateInstance<NetworkSoundEventDef>();
            EquipArrayUpSound.eventName = "Play_equip_up";
            R2API.ContentAddition.AddNetworkSoundEventDef(EquipArrayUpSound);
            EquipArrayDownSound = ScriptableObject.CreateInstance<NetworkSoundEventDef>();
            EquipArrayDownSound.eventName = "Play_equip_down";
            R2API.ContentAddition.AddNetworkSoundEventDef(EquipArrayDownSound);
        }
        private static void Item()
        {
            CapturedPotentialItemDef = ScriptableObject.CreateInstance<ItemDef>();
            CapturedPotentialItemDef.name = "CapturedPotential";
            CapturedPotentialItemDef.nameToken = "CAPTUREDPOTENTIAL_NAME";
            CapturedPotentialItemDef.pickupToken = "CAPTUREDPOTENTIAL_PICKUP";
            CapturedPotentialItemDef.descriptionToken = "CAPTUREDPOTENTIAL_DESC";
            CapturedPotentialItemDef.loreToken = "CAPTUREDPOTENTIAL_LORE";
            switch (CapturedPotentialTier.Value)
            {
                case 1:
                    CapturedPotentialItemDef.deprecatedTier = ItemTier.Tier1;
                    break;
                case 2:
                    CapturedPotentialItemDef.deprecatedTier = ItemTier.Tier2;
                    break;
                case 3:
                    CapturedPotentialItemDef.deprecatedTier = ItemTier.Tier3;
                    break;

            }
            CapturedPotentialItemDef.pickupIconSprite = CapturedPotentialIcon;
            CapturedPotentialItemDef.pickupModelPrefab = CapturedPotentialPrefab;
            CapturedPotentialItemDef.canRemove = true;
            CapturedPotentialItemDef.hidden = false;
            CapturedPotentialItemDef.requiredExpansion = CaeliImperiumExpansionDef;
            var tags = new List<ItemTag>() { ItemTag.Utility, ItemTag.AIBlacklist, ItemTag.BrotherBlacklist, ItemTag.EquipmentRelated };
            CapturedPotentialItemDef.tags = tags.ToArray();
            ItemDisplayRuleDict rules = new ItemDisplayRuleDict();
            rules.Add("mdlCommandoDualies", new RoR2.ItemDisplayRule[]{
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = CapturedPotentialPrefab,
                    childName = "Head",
localPos = new Vector3(0F, 0.21647F, 0.01699F),
localAngles = new Vector3(0F, 0F, 0F),
localScale = new Vector3(0.2276F, 0.2276F, 0.2276F)
                }
            });
            rules.Add("mdlHuntress", new RoR2.ItemDisplayRule[]{
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = CapturedPotentialPrefab,
                    childName = "Head",
localPos = new Vector3(0F, 0.14004F, -0.00001F),
localAngles = new Vector3(0F, 0F, 0F),
localScale = new Vector3(0.20722F, 0.20722F, 0.20722F)
                }
            });
            rules.Add("mdlBandit2", new RoR2.ItemDisplayRule[]{
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = CapturedPotentialPrefab,
                    childName = "SideWeapon",
localPos = new Vector3(-0.00871F, -0.09839F, 0.0979F),
localAngles = new Vector3(331.6818F, 338.2739F, 310.0308F),
localScale = new Vector3(0.06386F, 0.06386F, 0.06386F)
                }
            });
            rules.Add("mdlToolbot", new RoR2.ItemDisplayRule[]{
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = CapturedPotentialPrefab,
                    childName = "Base",
localPos = new Vector3(0F, 0F, -0.74925F),
localAngles = new Vector3(0F, 331.2403F, 0F),
localScale = new Vector3(1.40124F, 1.40124F, 1.40124F)
                }
            });
            rules.Add("mdlEngi", new RoR2.ItemDisplayRule[]{
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = CapturedPotentialPrefab,
                    childName = "Chest",
localPos = new Vector3(0F, 0.54213F, 0.02324F),
localAngles = new Vector3(28.7177F, 0F, 0F),
localScale = new Vector3(0.22816F, 0.22816F, 0.22816F)
                }
            });
            rules.Add("mdlEngiTurret", new RoR2.ItemDisplayRule[]{
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = CapturedPotentialPrefab,
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
                    followerPrefab = CapturedPotentialPrefab,
                    childName = "Head",
localPos = new Vector3(0F, 0.06614F, -0.02483F),
localAngles = new Vector3(21.02913F, 0F, 0F),
localScale = new Vector3(0.14696F, 0.14696F, 0.19114F)
                }
            });
            rules.Add("mdlMerc", new RoR2.ItemDisplayRule[]{
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = CapturedPotentialPrefab,
                    childName = "Head",
localPos = new Vector3(0F, 0.13605F, 0.03262F),
localAngles = new Vector3(354.4589F, 141.4038F, 4.4072F),
localScale = new Vector3(0.16594F, 0.16594F, 0.16594F)
                }
            });
            rules.Add("mdlTreebot", new RoR2.ItemDisplayRule[]{
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = CapturedPotentialPrefab,
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
                    followerPrefab = CapturedPotentialPrefab,
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
                    followerPrefab = CapturedPotentialPrefab,
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
                    followerPrefab = CapturedPotentialPrefab,
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
                    followerPrefab = CapturedPotentialPrefab,
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
                    followerPrefab = CapturedPotentialPrefab,
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
                    followerPrefab = CapturedPotentialPrefab,
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
                    followerPrefab = CapturedPotentialPrefab,
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
                    followerPrefab = CapturedPotentialPrefab,
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
                    followerPrefab = CapturedPotentialPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0f, 0f, 0f),
                    localAngles = new Vector3(0f, 0f, 0f),
                    localScale = new Vector3(1f, 1f, 1f)
                }
            });
            var displayRules = new ItemDisplayRuleDict(null);
            ItemAPI.Add(new CustomItem(CapturedPotentialItemDef, rules));
            //On.RoR2.CharacterBody.Update += KeyInputUpdate;
            //On.RoR2.CharacterBody.FixedUpdate += KeyInput;
            On.RoR2.CharacterBody.OnInventoryChanged += ChangeArraySize;
            //On.RoR2.EquipmentDef.AttemptGrant += FillEmptySlots;
            On.RoR2.PurchaseInteraction.OnInteractionBegin += CardCompatibility;
                ProperSave.SaveFile.OnGatherSaveData += SaveFile_OnGatherSaveData;
                ProperSave.Loading.OnLoadingEnded += Loading_OnLoadingStarted;
            
        }

        private static void Loading_OnLoadingStarted(SaveFile file)
        {
            string ComponentDictKey = "KitchenSanFiero_CapturedPotentialInventory";
            List<CapturedPotentialSaveStructure> CapturedPotentialStructures = file.GetModdedData<List<CapturedPotentialSaveStructure>>(ComponentDictKey);
            foreach (CapturedPotentialSaveStructure ComponentsList in CapturedPotentialStructures)
            {
                NetworkUserId NUID = ComponentsList.userID.Load();
                CharacterMaster master = NetworkUser.readOnlyInstancesList.FirstOrDefault(Nuser => Nuser.id.Equals(NUID)).master;
                GameObject masterObject = NetworkUser.readOnlyInstancesList.FirstOrDefault(Nuser => Nuser.id.Equals(NUID)).masterObject;
                EquipmentIndex[] equipmentIndexes = new EquipmentIndex[0];
                int number = 0;
                Debug.Log(master);
                
                foreach (EquipmentIndex equip in ComponentsList.EquipInventory)
                {
                    //if (equip != EquipmentIndex.None)
                    //{
                    //    PickupDropletController.CreatePickupDroplet(PickupCatalog.FindPickupIndex(equip), master.GetBody().transform.position, master.GetBody().transform.rotation.eulerAngles * 20f);

                    //}
                    Array.Resize(ref equipmentIndexes, number + 1);
                    equipmentIndexes.SetValue(equip, number);
                    number++;
                    //equipmentIndexes.Add(EquipmentCatalog.GetEquipmentDef(EquipmentCatalog.FindEquipmentIndex(equip)));

                    
                }
                Debug.Log(equipmentIndexes.Length);
                Debug.Log(equipmentIndexes.GetValue(0));
                //if (master.GetBody().masterObject.GetComponent<CapturedPotentialComponent>())
                //{
                //    CapturedPotentialComponent temp = master.GetBody().masterObject.GetComponent<CapturedPotentialComponent>();
                //    temp.equipArray = equipmentIndexes;
                //    temp.master = master;
                //}
                //else
                //{
                    CapturedPotentialComponent temp = masterObject.AddComponent<CapturedPotentialComponent>();
                    temp.master = master;
                    temp.equipArray = equipmentIndexes;
                //}
            }
        }

        private static void SaveFile_OnGatherSaveData(Dictionary<string, object> dictionary)
        {
            string ComponentDictKey = "KitchenSanFiero_CapturedPotentialInventory";

            List<CapturedPotentialComponent> equipInventory = CharacterMaster.instancesList
            .Select(master => master.GetBody().masterObject.GetComponent<CapturedPotentialComponent>())
            .Where(tracker => tracker != null)
            .ToList();

            List<CapturedPotentialSaveStructure> ComponentSaveListList = new List<CapturedPotentialSaveStructure>();

            foreach (CapturedPotentialComponent component in equipInventory)
            {
                EquipmentIndex[] ComponentEquipList = new EquipmentIndex[component.equipArray.Length];
                int number = 0;
                foreach (EquipmentIndex ED in component.equipArray)
                {
                    ComponentEquipList.SetValue(ED, number);
                number++;

                }

                ComponentSaveListList.Add(new CapturedPotentialSaveStructure
                {
                    userID = new ProperSave.Data.UserIDData(component.master.playerCharacterMasterController.networkUser.id),
                    EquipInventory = ComponentEquipList
                });
            }

            dictionary.Add(ComponentDictKey, ComponentSaveListList);
        }
        public struct CapturedPotentialSaveStructure
        {
            [DataMember(Name = "UserID")]
            public ProperSave.Data.UserIDData userID;
            [DataMember(Name = "EquipInventory")]
            public EquipmentIndex[] EquipInventory;
        }
        /*
private static void FillEmptySlots(On.RoR2.EquipmentDef.orig_AttemptGrant orig, ref PickupDef.GrantContext context)
{
   if (context.body.masterObject.GetComponent<CapturedPotentialComponent>() && context.body.masterObject.GetComponent<CapturedPotentialComponent>().equipArray.Length > 0 && context.body.masterObject.GetComponent<CapturedPotentialComponent>().equipArray.Contains(EquipmentIndex.None))
   {
       bool found = false;
       for (int i = 0; i < context.body.masterObject.GetComponent<CapturedPotentialComponent>().equipArray.Length && !found; i++)
       {
           if (context.body.masterObject.GetComponent<CapturedPotentialComponent>().equipArray[i] == EquipmentIndex.None)
           {
               context.body.masterObject.GetComponent<CapturedPotentialComponent>().equipArray[i] = context.body.inventory.currentEquipmentIndex;
               context.body.inventory.SetEquipmentIndex(EquipmentIndex.None);
               found = true;
           }

       }
   }
   orig(ref context);
}
*/
        [ConCommand(commandName = "EquipArrayIndexUp", flags = ConVarFlags.ExecuteOnServer)]
        private static void EquipArrayIndexUp(ConCommandArgs args)
        {
            //Chat.AddMessage("up");

            CharacterMaster networkIdentity = args.senderBody.masterObject.GetComponent<CapturedPotentialComponent>().master;

            if (networkIdentity && args.senderMasterObject.GetComponent<CapturedPotentialComponent>())
            {
                //Chat.AddMessage("true");
                int itemCount = args.senderBody.inventory ? args.senderBody.inventory.GetItemCount(CapturedPotentialItemDef) : 0;
                if (itemCount > 0)
                {
                    var equipArray = args.senderMasterObject.GetComponent<CapturedPotentialComponent>().equipArray;
                    EquipmentIndex portal = (EquipmentIndex)args.senderMasterObject.GetComponent<CapturedPotentialComponent>().equipArray.GetValue(args.senderMasterObject.GetComponent<CapturedPotentialComponent>().equipArray.Length - args.senderMasterObject.GetComponent<CapturedPotentialComponent>().equipArray.Length);
                    Array.Copy(args.senderMasterObject.GetComponent<CapturedPotentialComponent>().equipArray, 1, args.senderMasterObject.GetComponent<CapturedPotentialComponent>().equipArray, 0, args.senderMasterObject.GetComponent<CapturedPotentialComponent>().equipArray.Length - 1);
                    args.senderMasterObject.GetComponent<CapturedPotentialComponent>().equipArray.SetValue(networkIdentity.inventory.GetEquipmentIndex(), args.senderMasterObject.GetComponent<CapturedPotentialComponent>().equipArray.Length - 1);
                    networkIdentity.inventory.SetEquipmentIndex(portal);
                    EntitySoundManager.EmitSoundServer(EquipArrayUpSound.akId, args.senderBody.gameObject);
                }
            }
        }
        [ConCommand(commandName = "EquipArrayIndexDown", flags = ConVarFlags.ExecuteOnServer)]
        private static void EquipArrayIndexDown(ConCommandArgs args)
        {
            CharacterMaster networkIdentity = args.senderBody.masterObject.GetComponent<CapturedPotentialComponent>().master ;
            if (networkIdentity && args.senderMasterObject.GetComponent<CapturedPotentialComponent>())
            {
                int itemCount = args.senderBody.inventory ? args.senderBody.inventory.GetItemCount(CapturedPotentialItemDef) : 0;
                if (itemCount > 0)
                {
                    var equipArray = args.senderMasterObject.GetComponent<CapturedPotentialComponent>().equipArray;
                        Array.Reverse(args.senderMasterObject.GetComponent<CapturedPotentialComponent>().equipArray);
                        EquipmentIndex portal = (EquipmentIndex)args.senderMasterObject.GetComponent<CapturedPotentialComponent>().equipArray.GetValue(args.senderMasterObject.GetComponent<CapturedPotentialComponent>().equipArray.Length - args.senderMasterObject.GetComponent<CapturedPotentialComponent>().equipArray.Length);
                    Array.Copy(args.senderMasterObject.GetComponent<CapturedPotentialComponent>().equipArray, 1, args.senderMasterObject.GetComponent<CapturedPotentialComponent>().equipArray, 0, args.senderMasterObject.GetComponent<CapturedPotentialComponent>().equipArray.Length - 1);
                    args.senderMasterObject.GetComponent<CapturedPotentialComponent>().equipArray.SetValue(networkIdentity.inventory.GetEquipmentIndex(), args.senderMasterObject.GetComponent<CapturedPotentialComponent>().equipArray.Length - 1);
                    networkIdentity.inventory.SetEquipmentIndex(portal);
                    Array.Reverse(args.senderMasterObject.GetComponent<CapturedPotentialComponent>().equipArray);
                    EntitySoundManager.EmitSoundServer(EquipArrayDownSound.akId, args.senderBody.gameObject);
                }
            }
        }

        private static void CardCompatibility(On.RoR2.PurchaseInteraction.orig_OnInteractionBegin orig, PurchaseInteraction self, Interactor activator)
        {
            
            CharacterMaster activatorMaster = activator.gameObject.GetComponent<CharacterBody>().master;
            //Debug.Log("ActivatorMaster " + activatorMaster);
            int moneyCost = self.cost;
            //Debug.Log("MoneyCost " + moneyCost);
            if (CapturedPotentialCard.Value && activatorMaster && activatorMaster.hasBody && activatorMaster.inventory && activatorMaster.inventory.currentEquipmentIndex != DLC1Content.Equipment.MultiShopCard.equipmentIndex && activatorMaster.GetBody().masterObject.GetComponent<CapturedPotentialComponent>() && activatorMaster.GetBody().masterObject.GetComponent<CapturedPotentialComponent>().equipArray.Contains(DLC1Content.Equipment.MultiShopCard.equipmentIndex))
            {
                //Debug.Log("true");
                CharacterBody body = activatorMaster.GetBody();
                //Debug.Log("CharacterBody " +  body);
                int cardamount = 0;
                    if (moneyCost > 0)
                    {
                    foreach (EquipmentIndex equipIndex in activatorMaster.GetBody().masterObject.GetComponent<CapturedPotentialComponent>().equipArray)
                    {
                        if (equipIndex == DLC1Content.Equipment.MultiShopCard.equipmentIndex)
                        {
                            cardamount++;
                        }
                    }
                    GoldOrb goldOrb = new GoldOrb();
                        Orb orb = goldOrb;
                        GameObject purchasedObject = self.gameObject;
                        //Debug.Log("PurchasedObject "+ purchasedObject);
                        Vector3? vector;
                        if (purchasedObject == null)
                        {
                            vector = null;
                        }
                        else
                        {
                            Transform transform = purchasedObject.transform;
                            vector = ((transform != null) ? new Vector3?(transform.position) : null);
                        }
                        orb.origin = (vector ?? body.corePosition);
                        goldOrb.target = body.mainHurtBox;
                        goldOrb.goldAmount = (uint)(0.1f * (float)moneyCost * cardamount);
                        OrbManager.instance.AddOrb(goldOrb);
                    }
                    GameObject purchasedObject2 = self.gameObject;
                    //Debug.Log("PurchasedObject2 "+  purchasedObject2);
                    ShopTerminalBehavior shopTerminalBehavior = (purchasedObject2 != null) ? purchasedObject2.GetComponent<ShopTerminalBehavior>() : null;
                    //Debug.Log("ShopTerminalBehaviour " +  shopTerminalBehavior);
                    if (shopTerminalBehavior && shopTerminalBehavior.serverMultiShopController)
                    {
                        //Debug.Log("NotClosing");
                        shopTerminalBehavior.serverMultiShopController.SetCloseOnTerminalPurchase(purchasedObject2.GetComponent<PurchaseInteraction>(), false);
                    }
                }
            
            orig(self, activator);
        }
        

        private static void ChangeArraySize(On.RoR2.CharacterBody.orig_OnInventoryChanged orig, CharacterBody self)
        {
            orig(self);
                int itemCount = self.inventory ? self.inventory.GetItemCount(CapturedPotentialItemDef) : 0;
            if (itemCount > 0 && !self.masterObject.GetComponent<CapturedPotentialComponent>())
            {
                //Debug.Log("add");
                CapturedPotentialComponent component = self.masterObject.AddComponent<CapturedPotentialComponent>();
                //component.body = self;
                component.equipArray = new EquipmentIndex[0];
                component.master = self.master;
            }
            //self.AddItemBehavior<CapturedPotentialBehaviour>(itemCount);

            /*if (itemCount == 0 && self.masterObject.GetComponent<CapturedPotentialComponent>() && self.masterObject.GetComponent<CapturedPotentialComponent>().equipArray != null)
            {
                for (int i = 0; i < self.GetComponent<CapturedPotentialComponent>().equipArray.Length - itemCount; i++)
                {
                    EquipmentIndex pickupIndex = (EquipmentIndex)self.GetComponent<CapturedPotentialComponent>().equipArray.GetValue(self.GetComponent<CapturedPotentialComponent>().equipArray.Length - (i + 1));
                    if (pickupIndex != EquipmentIndex.None)
                    {
                        UnityEngine.Vector3 RandomVector = UnityEngine.Random.onUnitSphere * 3;
                        PickupDropletController.CreatePickupDroplet(PickupCatalog.FindPickupIndex(pickupIndex), self.transform.position, (self.transform.up * 10) + RandomVector);

                    }

                }
            }*/
            
            /*if (self.masterObject.GetComponent<CapturedPotentialComponent>())
            {
            Debug.Log(self.masterObject.GetComponent<CapturedPotentialComponent>().body);

            Debug.Log(self.masterObject.GetComponent<CapturedPotentialComponent>().equipArray.Length);
            }*/
            

        }
        /*
        public class CapturedPotentialScroll : NetworkBehaviour
        {
            private void Update()
            {
                if (!PauseManager.isPaused)
                {

                    //var equipArray = GetOrCreateComponent(body.master).equipArray;

                    if (Input.GetKey(KeyCode.E) && Input.mouseScrollDelta == Vector2.up)
                    {
                        Chat.AddMessage("Send");

                        //NetworkWriter networkWriter = new NetworkWriter();
                        RoR2.Console.instance.SubmitCmd(NetworkUser.readOnlyLocalPlayersList.FirstOrDefault(), "EquipArrayIndexUp");
                        //SendCommandInternal(networkWriter, 0, "EquipArrayIndexUp");
                    }
                    if (Input.GetKey(KeyCode.E) && Input.mouseScrollDelta == Vector2.down)
                    {
                        RoR2.Console.instance.SubmitCmd(NetworkUser.readOnlyLocalPlayersList.FirstOrDefault(), "EquipArrayIndexDown");
                        //NetworkWriter networkWriter = new NetworkWriter();
                        //SendCommandInternal(networkWriter, 0, "EquipArrayIndexDown");
                    }
                }
            }
        }*/
        public class CapturedPotentialComponent : MonoBehaviour
        {
            [DataMember(Name = "EquipInventory")]
            public EquipmentIndex[] equipArray = new EquipmentIndex[0];
            //public CharacterBody body;
            [IgnoreDataMember]
            public CharacterMaster master;
            public void Update()
            {
                /*
                if (syncIds.Count > 0)
                {
                    NetworkInstanceId syncId = syncIds.Dequeue();
                    GameObject supposedChimera = RoR2.Util.FindNetworkObject(syncId);
                    if (supposedChimera)
                    {
                        CharBody = supposedChimera.GetComponent<RoR2.CharacterBody>();
                        if (Input.GetKey(KeyCode.E) && Input.mouseScrollDelta == Vector2.up)
                        {
                            Chat.AddMessage("up");
                            CharacterBody networkIdentity = CharBody;//NetworkUser.readOnlyLocalPlayersList[0].master?.GetBody();
                            Debug.Log(networkIdentity);
                            Debug.Log(networkIdentity.masterObject.GetComponent<CapturedPotentialComponent>());
                            if (networkIdentity && GetOrCreateComponent(networkIdentity.master))
                            {
                                Chat.AddMessage("true");
                                int itemCount = networkIdentity.inventory ? networkIdentity.inventory.GetItemCount(CapturedPotential.CapturedPotentialItemDef) : 0;
                                if (itemCount > 0)
                                {
                                    var equipArray = GetOrCreateComponent(networkIdentity.master).equipArray;
                                    EquipmentIndex portal = (EquipmentIndex)equipArray.GetValue(equipArray.Length - equipArray.Length);
                                    Array.Copy(equipArray, 1, equipArray, 0, equipArray.Length - 1);
                                    equipArray.SetValue(networkIdentity.inventory.GetEquipmentIndex(), equipArray.Length - 1);
                                    networkIdentity.inventory.SetEquipmentIndex(portal);
                                }
                            }

                        }
                        if (Input.GetKey(KeyCode.E) && Input.mouseScrollDelta == Vector2.down)
                        {
                            Chat.AddMessage("down");
                            CharacterBody networkIdentity = CharBody;//NetworkUser.readOnlyLocalPlayersList[0].master?.GetBody();

                            if (networkIdentity && GetOrCreateComponent(networkIdentity.master))
                            {
                                int itemCount = networkIdentity.inventory ? networkIdentity.inventory.GetItemCount(CapturedPotential.CapturedPotentialItemDef) : 0;
                                if (itemCount > 0)
                                {
                                    var equipArray = GetOrCreateComponent(networkIdentity.master).equipArray;
                                    Array.Reverse(equipArray);
                                    EquipmentIndex portal = (EquipmentIndex)equipArray.GetValue(equipArray.Length - equipArray.Length);
                                    Array.Copy(equipArray, 1, equipArray, 0, equipArray.Length - 1);
                                    equipArray.SetValue(networkIdentity.inventory.GetEquipmentIndex(), equipArray.Length - 1);
                                    networkIdentity.inventory.SetEquipmentIndex(portal);
                                    Array.Reverse(equipArray);
                                }
                            }

                        }
                    }
                    else
                    {
                        syncIds.Enqueue(syncId);
                    }
                    
                }*/
                
            }
            public void Awake()
            {
                //body = gameObject.GetComponent<CharacterBody>();
               //equipArray = new EquipmentIndex[0];
            }
            public void OnEnable()
            {
                //body = gameObject.GetComponent<CharacterBody>();
                //equipArray = new EquipmentIndex[0];
            }
            private void FixedUpdate()
            {

                if (!NetworkServer.active)
                {
                    return;
                }
                
                    if (master.GetBody())
                    {
                        int itemCount = master.inventory ? master.inventory.GetItemCount(CapturedPotentialItemDef) : 0;
                        if (itemCount > 0)
                        {
                            itemCount = CapturedPotentialEquipSlots.Value + ((master.inventory.GetItemCount(CapturedPotentialItemDef) - 1) * CapturedPotentialEquipSlotsStack.Value);
                        }
                        //var equipArray = body.masterObject.GetComponent<CapturedPotentialComponent>().equipArray;
                        if (itemCount < equipArray.Length)
                        {
                            for (int i = 0; i < (equipArray.Length - itemCount); i++)
                            {
                                EquipmentIndex pickupIndex = (EquipmentIndex)equipArray.GetValue(equipArray.Length - (i + 1));
                                if (pickupIndex != EquipmentIndex.None)
                                {
                                    PickupDropletController.CreatePickupDroplet(PickupCatalog.FindPickupIndex(pickupIndex), master.GetBody().transform.position, master.GetBody().transform.rotation.eulerAngles * 20f);

                                }

                            }
                            Array.Resize<EquipmentIndex>(ref equipArray, itemCount);

                        }
                        if (itemCount > equipArray.Length)
                        {
                            int toChange = equipArray.Length;
                            Array.Resize<EquipmentIndex>(ref equipArray, itemCount);
                            for (int i = 0; i < (itemCount - toChange); i++)
                            {
                                equipArray.SetValue(EquipmentIndex.None, equipArray.Length - (i + 1));

                            }



                        }
                        var body = master.GetBody();
                        //timer += Time.fixedDeltaTime;
                        if (CapturedPotentialAffixes.Value)
                        {
                            {
                                foreach (EquipmentIndex equipIndex in equipArray)
                                {
                                    if (EquipmentCatalog.GetEquipmentDef(equipIndex) && EquipmentCatalog.GetEquipmentDef(equipIndex).passiveBuffDef && !body.HasBuff(EquipmentCatalog.GetEquipmentDef(equipIndex).passiveBuffDef))
                                    {
                                        body.AddTimedBuff(EquipmentCatalog.GetEquipmentDef(equipIndex).passiveBuffDef, 1);
                                    }
                                }
                            }
                            /*

                            if (equipArray.Contains<EquipmentIndex>(RoR2Content.Equipment.AffixRed.equipmentIndex) && !body.HasBuff(RoR2Content.Buffs.AffixRed))
                            {
                                body.AddTimedBuff(RoR2Content.Buffs.AffixRed.buffIndex, 1);
                            }
                            if (equipArray.Contains<EquipmentIndex>(RoR2Content.Equipment.AffixWhite.equipmentIndex) && !body.HasBuff(RoR2Content.Buffs.AffixWhite))
                            {
                                body.AddTimedBuff(RoR2Content.Buffs.AffixWhite.buffIndex, 1);
                            }
                            if (equipArray.Contains<EquipmentIndex>(RoR2Content.Equipment.AffixPoison.equipmentIndex) && !body.HasBuff(RoR2Content.Buffs.AffixPoison))
                            {
                                body.AddTimedBuff(RoR2Content.Buffs.AffixPoison.buffIndex, 1);
                            }
                            if (equipArray.Contains<EquipmentIndex>(RoR2Content.Equipment.AffixBlue.equipmentIndex) && !body.HasBuff(RoR2Content.Buffs.AffixBlue))
                            {
                                body.AddTimedBuff(RoR2Content.Buffs.AffixBlue.buffIndex, 1);
                            }
                            if (equipArray.Contains<EquipmentIndex>(RoR2Content.Equipment.AffixHaunted.equipmentIndex) && !body.HasBuff(RoR2Content.Buffs.AffixHaunted))
                            {
                                body.AddTimedBuff(RoR2Content.Buffs.AffixHaunted.buffIndex, 1);
                            }
                            if (equipArray.Contains<EquipmentIndex>(RoR2Content.Equipment.AffixLunar.equipmentIndex) && !body.HasBuff(RoR2Content.Buffs.AffixLunar))
                            {
                                body.AddTimedBuff(RoR2Content.Buffs.AffixLunar.buffIndex, 1);
                            }
                            if (equipArray.Contains<EquipmentIndex>(DLC1Content.Equipment.EliteVoidEquipment.equipmentIndex) && !body.HasBuff(DLC1Content.Buffs.EliteVoid))
                            {
                                body.AddTimedBuff(DLC1Content.Buffs.EliteVoid.buffIndex, 1);
                            }
                            if (equipArray.Contains<EquipmentIndex>(DLC1Content.Elites.Earth.eliteEquipmentDef.equipmentIndex) && !body.HasBuff(DLC1Content.Buffs.EliteEarth))
                            {
                                body.AddTimedBuff(DLC1Content.Buffs.EliteEarth.buffIndex, 1);
                            }
                            if (equipArray.Contains<EquipmentIndex>(DLC2Content.Equipment.EliteBeadEquipment.equipmentIndex) && !body.HasBuff(DLC2Content.Buffs.EliteBead.buffIndex))
                            {
                                body.AddTimedBuff(DLC2Content.Buffs.EliteBead.buffIndex, 1);
                            }
                            if (equipArray.Contains<EquipmentIndex>(DLC2Content.Equipment.EliteAurelioniteEquipment.equipmentIndex) && !body.HasBuff(DLC2Content.Buffs.EliteAurelionite.buffIndex))
                            {
                                body.AddTimedBuff(DLC2Content.Buffs.EliteAurelionite.buffIndex, 1);
                            }
                            if (equipArray.Contains<EquipmentIndex>(BrassModality.AffixBrassModalityEquipment.equipmentIndex) && !body.HasBuff(BrassModality.AffixBrassModalityBuff.buffIndex))
                            {
                                body.AddTimedBuff(BrassModality.AffixBrassModalityBuff.buffIndex, 1);
                            }
                            if (equipArray.Contains<EquipmentIndex>(Dredged.AffixDredgedEquipment.equipmentIndex) && !body.HasBuff(Dredged.AffixDredgedBuff.buffIndex))
                            {
                                body.AddTimedBuff(Dredged.AffixDredgedBuff.buffIndex, 1);
                            }
                            if (equipArray.Contains<EquipmentIndex>(Hasting.AffixHastingEquipment.equipmentIndex) && !body.HasBuff(Hasting.AffixHastingBuff.buffIndex))
                            {
                                body.AddTimedBuff(Hasting.AffixHastingBuff.buffIndex, 1);
                            }
                            if (equipArray.Contains<EquipmentIndex>(Defender.AffixDefenderEquipment.equipmentIndex) && !body.HasBuff(Defender.AffixDefenderBuff.buffIndex))
                            {
                                body.AddTimedBuff(Defender.AffixDefenderBuff.buffIndex, 1);
                            }*/
                        }
                    }
                
                

            }/*
            public class AssignOwner : INetMessage
            {
                private NetworkInstanceId ownerNetId;
                private NetworkInstanceId minionNetId;

                public AssignOwner()
                {
                }

                public AssignOwner(NetworkInstanceId ownerNetId, NetworkInstanceId minionNetId)
                {
                    this.ownerNetId = ownerNetId;
                    this.minionNetId = minionNetId;
                }

                public void Deserialize(NetworkReader reader)
                {
                    ownerNetId = reader.ReadNetworkId();
                    minionNetId = reader.ReadNetworkId();
                }

                public void OnReceived()
                {
                    if (NetworkServer.active) return;
                    GameObject owner = RoR2.Util.FindNetworkObject(ownerNetId);
                    if (!owner) return;

                    CapturedPotentialComponent lcComponent = CapturedPotentialComponent.GetOrCreateComponent(owner);
                    lcComponent.syncIds.Enqueue(minionNetId);
                }

                public void Serialize(NetworkWriter writer)
                {
                    writer.Write(ownerNetId);
                    writer.Write(minionNetId);
                }
            }*/
            
            public static CapturedPotentialComponent GetOrCreateComponent(RoR2.CharacterMaster master)
            {
                return GetOrCreateComponent(master.gameObject);
            }
            public static CapturedPotentialComponent GetOrCreateComponent(GameObject masterObject)
            {
                CapturedPotentialComponent thisComponent = masterObject.GetComponent<CapturedPotentialComponent>();
                if (!thisComponent) thisComponent = masterObject.AddComponent<CapturedPotentialComponent>();
                return thisComponent;
            }
        }
        
        
        public class CapturedPotentialBehaviour : RoR2.CharacterBody.ItemBehavior
        {
            //public EquipmentIndex[] equipArray = new EquipmentIndex[0];
            public float testTimer = 0;
            public bool hasCard;

            private static ItemDef GetItemDef()
            {
                return CapturedPotentialItemDef;
            }
            private void Awake()
            {
                // This is important because it prevents OnEnable from getting called
                // before `this.body` has been assigned a value. If you skip this, you
                // need to null check for it in OnEnable if you need it.
                base.enabled = false;
            }

            private void OnEnable()
            {/*
                if (body)
                {
                    if (!body.masterObject.GetComponent<CapturedPotentialComponent>())
                    {
                        body.masterObject.AddComponent<CapturedPotentialComponent>();
                        body.masterObject.GetComponent<CapturedPotentialComponent>().body = body;
                        body.masterObject.GetComponent<CapturedPotentialComponent>().equipArray = new EquipmentIndex[0];

                    }
                }*/
                // Any initialisation logic, null check for `this.body` as necessary
                // `this.stack` is still unassigned at this point so use `this.body.inventory`
            }/*
            private void UpTheIndex()
            {
                Chat.AddMessage("up true");
                EquipmentIndex portal = (EquipmentIndex)GetOrCreateComponent(body.master).equipArray.GetValue(GetOrCreateComponent(body.master).equipArray.Length - GetOrCreateComponent(body.master).equipArray.Length);
                Array.Copy(GetOrCreateComponent(body.master).equipArray, 1, GetOrCreateComponent(body.master).equipArray, 0, GetOrCreateComponent(body.master).equipArray.Length - 1);
                GetOrCreateComponent(body.master).equipArray.SetValue(body.inventory.GetEquipmentIndex(), GetOrCreateComponent(body.master).equipArray.Length - 1);
                body.inventory.SetEquipmentIndex(portal);
                upInput = false;
            }
            private void DownTheIndex()
            {
                Chat.AddMessage("down true");
                Array.Reverse(GetOrCreateComponent(body.master).equipArray);
                EquipmentIndex portal = (EquipmentIndex)GetOrCreateComponent(body.master).equipArray.GetValue(GetOrCreateComponent(body.master).equipArray.Length - GetOrCreateComponent(body.master).equipArray.Length);
                Array.Copy(GetOrCreateComponent(body.master).equipArray, 1, GetOrCreateComponent(body.master).equipArray, 0, GetOrCreateComponent(body.master).equipArray.Length - 1);
                GetOrCreateComponent(body.master).equipArray.SetValue(body.inventory.GetEquipmentIndex(), GetOrCreateComponent(body.master).equipArray.Length - 1);
                body.inventory.SetEquipmentIndex(portal);
                Array.Reverse(GetOrCreateComponent(body.master).equipArray);
                downInput = false;
            }*/
            private void Update()
            {
                if (body.hasEffectiveAuthority && !PauseManager.isPaused && stack > 0)
                {

                    //var equipArray = GetOrCreateComponent(body.master).equipArray;
                    /*
                    if (Input.GetKey(KeyCode.E) && Input.mouseScrollDelta == Vector2.up)
                    {
                        Chat.AddMessage("up");
                        CharacterBody networkIdentity = body.masterObject.GetComponent<CapturedPotentialComponent>().CharBody;//CapturedPotentialComponent.GetOrCreateComponent(body.master).CharBody;//NetworkUser.readOnlyLocalPlayersList[0].master?.GetBody();
                        Debug.Log(networkIdentity);
                        Debug.Log(networkIdentity.masterObject.GetComponent<CapturedPotentialComponent>());
                        if (networkIdentity)
                        {
                            Chat.AddMessage("true");
                            int itemCount = stack;
                            if (itemCount > 0)
                            {
                                var equipArray = body.masterObject.GetComponent<CapturedPotentialComponent>().equipArray;
                                EquipmentIndex portal = (EquipmentIndex)body.masterObject.GetComponent<CapturedPotentialComponent>().equipArray.GetValue(body.masterObject.GetComponent<CapturedPotentialComponent>().equipArray.Length - body.masterObject.GetComponent<CapturedPotentialComponent>().equipArray.Length);
                                Array.Copy(body.masterObject.GetComponent<CapturedPotentialComponent>().equipArray, 1, body.masterObject.GetComponent<CapturedPotentialComponent>().equipArray, 0, body.masterObject.GetComponent<CapturedPotentialComponent>().equipArray.Length - 1);
                                body.masterObject.GetComponent<CapturedPotentialComponent>().equipArray.SetValue(networkIdentity.inventory.GetEquipmentIndex(), body.masterObject.GetComponent<CapturedPotentialComponent>().equipArray.Length - 1);
                                networkIdentity.inventory.SetEquipmentIndex(portal);
                            }
                        }

                    }
                    if (Input.GetKey(KeyCode.E) && Input.mouseScrollDelta == Vector2.down)
                    {
                        Chat.AddMessage("down");
                        CharacterBody networkIdentity = body.masterObject.GetComponent<CapturedPotentialComponent>().CharBody;// CapturedPotentialComponent.GetOrCreateComponent(body.master).CharBody;//NetworkUser.readOnlyLocalPlayersList[0].master?.GetBody();

                        if (networkIdentity)
                        {
                            int itemCount = stack;
                            if (itemCount > 0)
                            {
                                var equipArray = body.masterObject.GetComponent<CapturedPotentialComponent>().equipArray;
                                Array.Reverse(body.masterObject.GetComponent<CapturedPotentialComponent>().equipArray);
                                EquipmentIndex portal = (EquipmentIndex)body.masterObject.GetComponent<CapturedPotentialComponent>().equipArray.GetValue(body.masterObject.GetComponent<CapturedPotentialComponent>().equipArray.Length - body.masterObject.GetComponent<CapturedPotentialComponent>().equipArray.Length);
                                Array.Copy(body.masterObject.GetComponent<CapturedPotentialComponent>().equipArray, 1, body.masterObject.GetComponent<CapturedPotentialComponent>().equipArray, 0, body.masterObject.GetComponent<CapturedPotentialComponent>().equipArray.Length - 1);
                                body.masterObject.GetComponent<CapturedPotentialComponent>().equipArray.SetValue(networkIdentity.inventory.GetEquipmentIndex(), body.masterObject.GetComponent<CapturedPotentialComponent>().equipArray.Length - 1);
                                networkIdentity.inventory.SetEquipmentIndex(portal);
                                Array.Reverse(body.masterObject.GetComponent<CapturedPotentialComponent>().equipArray);
                            }
                        }

                    }*/

                }
            }
            private void FixedUpdate()
            {
                if (!NetworkServer.active)
                {
                    return;
                }
                if (body)
                {
                    testTimer += Time.deltaTime;
                    if (testTimer > 1f)
                    {
                        testTimer = 0f;
                        if (body.masterObject.GetComponent<CapturedPotentialComponent>())
                        {
                            //Debug.Log(body.masterObject.GetComponent<CapturedPotentialComponent>().body);
                        }
                    }
                }
                /*
                if (!NetworkServer.active)
                {
                    return;
                }
                if (body)
                {
                    int itemCount = this.stack;
                    var equipArray = body.masterObject.GetComponent<CapturedPotentialComponent>().equipArray;
                    //testTimer += Time.fixedDeltaTime;
                    if (testTimer > 1)
                    {
                    Debug.Log(equipArray.Length);
                        testTimer = 0;
                    }
                    if (itemCount < body.masterObject.GetComponent<CapturedPotentialComponent>().equipArray.Length)
                    {
                        for (int i = 0; i < (body.masterObject.GetComponent<CapturedPotentialComponent>().equipArray.Length - itemCount); i++)
                        {
                            EquipmentIndex pickupIndex = (EquipmentIndex)body.masterObject.GetComponent<CapturedPotentialComponent>().equipArray.GetValue(body.masterObject.GetComponent<CapturedPotentialComponent>().equipArray.Length - (i + 1));
                            PickupDropletController.CreatePickupDroplet(PickupCatalog.FindPickupIndex(pickupIndex), body.transform.position, body.transform.rotation.eulerAngles * 20f);

                        }
                        Array.Resize<EquipmentIndex>(ref body.masterObject.GetComponent<CapturedPotentialComponent>().equipArray, itemCount);

                    }
                    if (itemCount > body.masterObject.GetComponent<CapturedPotentialComponent>().equipArray.Length)
                    {
                        int toChange = body.masterObject.GetComponent<CapturedPotentialComponent>().equipArray.Length;
                        Array.Resize<EquipmentIndex>(ref body.masterObject.GetComponent<CapturedPotentialComponent>().equipArray, itemCount);
                        for (int i = 0; i < (itemCount - toChange); i++)
                        {
                            body.masterObject.GetComponent<CapturedPotentialComponent>().equipArray.SetValue(EquipmentIndex.None, body.masterObject.GetComponent<CapturedPotentialComponent>().equipArray.Length - (i + 1));

                        }



                    }
                    
                    if (CapturedPotentialAffixes.Value)
                    {
                        if (equipArray.Contains<EquipmentIndex>(RoR2Content.Equipment.AffixRed.equipmentIndex) && !body.HasBuff(RoR2Content.Buffs.AffixRed))
                        {
                            body.AddTimedBuff(RoR2Content.Buffs.AffixRed.buffIndex, 1);
                        }
                        if (equipArray.Contains<EquipmentIndex>(RoR2Content.Equipment.AffixWhite.equipmentIndex) && !body.HasBuff(RoR2Content.Buffs.AffixWhite))
                        {
                            body.AddTimedBuff(RoR2Content.Buffs.AffixWhite.buffIndex, 1);
                        }
                        if (equipArray.Contains<EquipmentIndex>(RoR2Content.Equipment.AffixPoison.equipmentIndex) && !body.HasBuff(RoR2Content.Buffs.AffixPoison))
                        {
                            body.AddTimedBuff(RoR2Content.Buffs.AffixPoison.buffIndex, 1);
                        }
                        if (equipArray.Contains<EquipmentIndex>(RoR2Content.Equipment.AffixBlue.equipmentIndex) && !body.HasBuff(RoR2Content.Buffs.AffixBlue))
                        {
                            body.AddTimedBuff(RoR2Content.Buffs.AffixBlue.buffIndex, 1);
                        }
                        if (equipArray.Contains<EquipmentIndex>(RoR2Content.Equipment.AffixHaunted.equipmentIndex) && !body.HasBuff(RoR2Content.Buffs.AffixHaunted))
                        {
                            body.AddTimedBuff(RoR2Content.Buffs.AffixHaunted.buffIndex, 1);
                        }
                        if (equipArray.Contains<EquipmentIndex>(RoR2Content.Equipment.AffixLunar.equipmentIndex) && !body.HasBuff(RoR2Content.Buffs.AffixLunar))
                        {
                            body.AddTimedBuff(RoR2Content.Buffs.AffixLunar.buffIndex, 1);
                        }
                        if (equipArray.Contains<EquipmentIndex>(DLC1Content.Equipment.EliteVoidEquipment.equipmentIndex) && !body.HasBuff(DLC1Content.Buffs.EliteVoid))
                        {
                            body.AddTimedBuff(DLC1Content.Buffs.EliteVoid.buffIndex, 1);
                        }
                        if (equipArray.Contains<EquipmentIndex>(DLC1Content.Elites.Earth.eliteEquipmentDef.equipmentIndex) && !body.HasBuff(DLC1Content.Buffs.EliteEarth))
                        {
                            body.AddTimedBuff(DLC1Content.Buffs.EliteEarth.buffIndex, 1);
                        }
                        if (equipArray.Contains<EquipmentIndex>(DLC2Content.Equipment.EliteBeadEquipment.equipmentIndex) && !body.HasBuff(DLC2Content.Buffs.EliteBead.buffIndex))
                        {
                            body.AddTimedBuff(DLC2Content.Buffs.EliteBead.buffIndex, 1);
                        }
                        if (equipArray.Contains<EquipmentIndex>(DLC2Content.Equipment.EliteAurelioniteEquipment.equipmentIndex) && !body.HasBuff(DLC2Content.Buffs.EliteAurelionite.buffIndex))
                        {
                            body.AddTimedBuff(DLC2Content.Buffs.EliteAurelionite.buffIndex, 1);
                        }
                        if (equipArray.Contains<EquipmentIndex>(BrassModality.AffixBrassModalityEquipment.equipmentIndex) && !body.HasBuff(BrassModality.AffixBrassModalityBuff.buffIndex))
                        {
                            body.AddTimedBuff(BrassModality.AffixBrassModalityBuff.buffIndex, 1);
                        }
                        if (equipArray.Contains<EquipmentIndex>(Dredged.AffixDredgedEquipment.equipmentIndex) && !body.HasBuff(Dredged.AffixDredgedBuff.buffIndex))
                        {
                            body.AddTimedBuff(Dredged.AffixDredgedBuff.buffIndex, 1);
                        }
                        if (equipArray.Contains<EquipmentIndex>(Hasting.AffixHastingEquipment.equipmentIndex) && !body.HasBuff(Hasting.AffixHastingBuff.buffIndex))
                        {
                            body.AddTimedBuff(Hasting.AffixHastingBuff.buffIndex, 1);
                        }
                        if (equipArray.Contains<EquipmentIndex>(Defender.AffixDefenderEquipment.equipmentIndex) && !body.HasBuff(Defender.AffixDefenderBuff.buffIndex))
                        {
                            body.AddTimedBuff(Defender.AffixDefenderBuff.buffIndex, 1);
                        }
                    }



                    if (equipArray.Contains<EquipmentIndex>(DLC1Content.Equipment.MultiShopCard.equipmentIndex) && !body.HasBuff(Buffs.HasCardBuff.HasCardBuffDef) && CapturedPotentialCard.Value)
                    {
                        body.AddTimedBuff(HasCardBuff.HasCardBuffDef, 1);
                    }
                }
                
            }

            private void OnDisable()
            {/*
                
                if (body && body.GetComponent<CapturedPotentialComponent>())
                {
var equipArray = body.masterObject.GetComponent<CapturedPotentialComponent>().equipArray;
                Array.Clear(body.masterObject.GetComponent<CapturedPotentialComponent>().equipArray, -1, body.masterObject.GetComponent<CapturedPotentialComponent>().equipArray.Length);
                Array.Resize<EquipmentIndex>(ref body.masterObject.GetComponent<CapturedPotentialComponent>().equipArray, 0);
                }
                */
            }

        }
        private static void AddLanguageTokens()
        {
            LanguageAPI.Add("CAPTUREDPOTENTIAL_NAME", "Captured Potential");
            LanguageAPI.Add("CAPTUREDPOTENTIAL_PICKUP", "Gain <style=cIsUtility>+" + CapturedPotentialEquipSlots.Value + "</style> <style=cStack>(+" + CapturedPotentialEquipSlotsStack.Value + " per item stack)</style> <style=cIsUtility>equipment slots</style>");
            LanguageAPI.Add("CAPTUREDPOTENTIAL_DESC", "Gain <style=cIsUtility>+" + CapturedPotentialEquipSlots.Value + "</style> <style=cStack>(+" + CapturedPotentialEquipSlotsStack.Value + " per item stack)</style> <style=cIsUtility>equipment slots</style>");
            LanguageAPI.Add("CAPTUREDPOTENTIAL_LORE", "<style=cMono>//--ATTEMPT № 45123--//</style>" +
                "\n" +
                "Void: 67.23%" +
                "\n" +
                "Pressure: 921%" +
                "\n" +
                "Status: <style=cDeath>FAILURE</style>" +
                "\n" +
                "\n" +
                "<style=cMono>//--ATTEMPT № 45123--//</style>" +
                "\n" +
                "Void: 67.23%" +
                "\n" +
                "Pressure: 922%" +
                "\n" +
                "Status: <style=cDeath>FAILURE</style>" +
                "\n" +
                "\n" +
                "<style=cMono>//--ATTEMPT № 45123--//</style>" +
                "\n" +
                "Void: 67.23%" +
                "\n" +
                "Pressure: 923%" +
                "\n" +
                "<style=cDeath>FAILURE</style>" +
                "\n" +
                "\n" +
                "<style=cMono>//--ATTEMPT № 45123--//</style>" +
                "\n" +
                "Void: 67.23%" +
                "\n" +
                "Pressure: 924%" +
                "\n" +
                "Status: <style=cDeath>FAILURE</style>" +
                "\n" +
                "\n" +
                "<style=cMono>//--ATTEMPT № 45123--//</style>" +
                "\n" +
                "Void: 67.23%" +
                "\n" +
                "Pressure: 925%" +
                "\n" +
                "Status: <style=cArtifact>「SUCC?SS』</style>" +
                "\n" +
                "\n" +
                "<style=cMono>//--ATTEMPT № 45124--//</style>" +
                "\n" +
                "<style=cArtifact>「Vo?d』</style>: 45.452%" +
                "\n" +
                "Pressure: 926%" + 
                "\n" +
                "<style=cArtifact>Stat?s: 「??IL?RE』</style>" +
                "\n" +
                "\n" +
                "<style=cMono>//--ATTEMPT № 45125--//</style>" +
                "\n" +
                "<style=cArtifact>「Vo?d』: ?3.0?%</style>" +
                "\n" +
                "<style=cArtifact>「P??ssu??』</style>: 927%" +
                "\n" +
                "<style=cArtifact>??a??s: 「??I????』</style>"/* +
                "\n" +
                "<style=cArtifact>/  /  -  -  ?  P  ?  ?  L  ?  ?     ?     ?  ?  E   ?  ?  -  -  /  /</style>" +
                "\n" +
                "<style=cArtifact>「?A?S』: 「??.E%』</style>" +
                "\n" +
                "<style=cArtifact>「?H??E?L?』: 「??P%』</style>" +
                "\n" +
                "<style=cArtifact>「??????』: 「??ME???』</style>"*/);
        }
    }
}
