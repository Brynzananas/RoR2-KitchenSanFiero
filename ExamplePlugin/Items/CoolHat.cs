﻿using IL.RoR2.Items;
using CaeliImperium.Buffs;
using R2API;
using RoR2;
using RoR2.Items;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using static CaeliImperiumPlugin.CaeliImperium;
using static RoR2.MasterSpawnSlotController;
using static CaeliImperium.Elites.ArchNemesis;
using UnityEngine.Diagnostics;
using static RoR2.MasterCatalog;
using System.IO;

namespace CaeliImperium.Items
{
    public static class CoolHat
    {
        internal static GameObject CoolHatPrefab;
        internal static Sprite CoolHatIcon;
        public static ItemDef CoolHatItemDef;
        public static bool isSpawned = false;

        internal static void Init()
        {
            CoolHatPrefab = MainAssets.LoadAsset<GameObject>("Assets/Models/Prefabs/archnemesishat.prefab");
            CoolHatIcon = MainAssets.LoadAsset<Sprite>("Assets/Icons/ArchNemesisHat.png");

            Item();

            AddLanguageTokens();
        }
        public static void Item()
        {

            CoolHatItemDef = ScriptableObject.CreateInstance<ItemDef>();
            CoolHatItemDef.name = "CoolHat";
            CoolHatItemDef.nameToken = "COOLHAT_NAME";
            CoolHatItemDef.pickupToken = "COOLHAT_PICKUP";
            CoolHatItemDef.descriptionToken = "COOLHAT_DESC";
            CoolHatItemDef.loreToken = "COOLHAT_LORE";
            CoolHatItemDef.deprecatedTier = ItemTier.Boss;
            CoolHatItemDef.pickupIconSprite = CoolHatIcon;
            CoolHatItemDef.pickupModelPrefab = CoolHatPrefab;
            CoolHatItemDef.canRemove = false;
            CoolHatItemDef.hidden = true;
            CoolHatItemDef.requiredExpansion = CaeliImperiumExpansionDef;
            var tags = new List<ItemTag>() { ItemTag.AIBlacklist, ItemTag.WorldUnique, ItemTag.RebirthBlacklist, ItemTag.BrotherBlacklist };
            CoolHatItemDef.tags = tags.ToArray();
            var displayRules = new ItemDisplayRuleDict(null);
            ItemAPI.Add(new CustomItem(CoolHatItemDef, displayRules));
            On.RoR2.CharacterBody.OnInventoryChanged += AddBuff;
        }

        private static void AddBuff(On.RoR2.CharacterBody.orig_OnInventoryChanged orig, CharacterBody self)
        {
            orig(self);
            int itemCount = self.inventory ? self.inventory.GetItemCount(CoolHatItemDef) : 0;
            if (itemCount > 0)
            {
                GameObject archNemesisMasterPrefab = GetMasterPrefab(FindMasterIndex(File.ReadAllText(System.IO.Path.Combine(SavesDirectory, "Prefab.txt")).Replace("(Clone)", "")));
                var summon = new MasterSummon
                {

                    masterPrefab = archNemesisMasterPrefab,
                    position = ArchNemesisAllyPosition,
                    rotation = Quaternion.identity,
                    teamIndexOverride = new TeamIndex?(self.GetComponent<CharacterBody>().teamComponent.teamIndex),
                    useAmbientLevel = true,
                    summonerBodyObject = self.gameObject,
                    ignoreTeamMemberLimit = true,

                };
                CharacterMaster characterMaster = summon.Perform();

                if (characterMaster)
                {
                    characterMaster.inventory.SetEquipmentIndex(AffixArchNemesisEquipment.equipmentIndex);
                    characterMaster.inventory.GiveItem(RoR2Content.Items.MinionLeash);
                    characterMaster.inventory.GiveItem(RoR2Content.Items.TeleportWhenOob);
                    string archNemesisInventory = File.ReadAllText(System.IO.Path.Combine(SavesDirectory, "Inventory.txt"));
                    if (archNemesisInventory != null)
                    {
                        string[] archNemesisInventoryArray = archNemesisInventory.Split(",");
                        Vector3 position = characterMaster.transform.position + 2f * Vector3.forward;
                        for (int i = 0; i < archNemesisInventoryArray.Length; i++, i++)
                        {
                            try
                            {
                                string toParse = archNemesisInventoryArray[i + 1];
                                int itemCount2 = int.Parse(toParse);

                                if (!ItemCatalog.GetItemDef(ItemCatalog.FindItemIndex(archNemesisInventoryArray[i])).ContainsTag(ItemTag.AIBlacklist) || ArchNemesisAIBlacklistItems.Value)
                                {
                                    characterMaster.inventory.GiveItemString(archNemesisInventoryArray[i], itemCount2);

                                }

                            }
                            catch (Exception e)
                            {
                                Debug.LogError(e);
                            }

                        }
                    }
                }
                Stage.DontDestroyOnLoad(characterMaster);
                //Util.IsDontDestroyOnLoad(characterMaster.gameObject);    
                self.inventory.RemoveItem(CoolHatItemDef);
            }
        }

        public static void AddLanguageTokens()
        {
            LanguageAPI.Add("COOLHAT_NAME", "Cool Hat");
            LanguageAPI.Add("COOLHAT_PICKUP", "Turn <color=#ed1010>Arch Nemesis</color> to your side");
            LanguageAPI.Add("COOLHAT_DESC", "Turn <color=#ed1010>Arch Nemesis</color> to your side");
            LanguageAPI.Add("COOLHAT_LORE", "\"Here's to you, Nicola and Bart\nRest forever here in our hearts\nThe last and final moment is yours\nThat agony is your triumph\"");
        }
    }
}
