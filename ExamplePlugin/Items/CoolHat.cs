using IL.RoR2.Items;
using CaeliImperium.Buffs;
using R2API;
using RoR2;
using RoR2.Items;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using static ReignFromGreatBeyondPlugin.CaeliImperium;
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
            CoolHatIcon = MainAssets.LoadAsset<Sprite>("Assets/Icons/BrassBellIcon.png");

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
            CoolHatItemDef.hidden = false;
            CoolHatItemDef.requiredExpansion = CaeliImperiumExpansionDef;
            var tags = new List<ItemTag>() { ItemTag.Damage, ItemTag.AIBlacklist, ItemTag.WorldUnique, ItemTag.RebirthBlacklist, ItemTag.BrotherBlacklist };
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
                }
                Stage.DontDestroyOnLoad(characterMaster);
                //Util.IsDontDestroyOnLoad(characterMaster.gameObject);    
                self.inventory.RemoveItem(CoolHatItemDef);
            }
        }

        public static void AddLanguageTokens()
        {
            LanguageAPI.Add("COOLHAT_NAME", "Cool Hat");
            LanguageAPI.Add("COOLHAT_PICKUP", "Turn Arch Nemesis to your side");
            LanguageAPI.Add("COOLHAT_DESC", "Turn Arch Nemesis to your side");
            LanguageAPI.Add("COOLHAT_LORE", "mmmm yummy");
        }
    }
}
