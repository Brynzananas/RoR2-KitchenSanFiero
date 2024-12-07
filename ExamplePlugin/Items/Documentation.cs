using R2API;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using static CaeliImperiumPlugin.CaeliImperium;

namespace CaeliImperium.Items
{
    internal static class Documentation
    {
        internal static GameObject DocumentationPrefab;
        internal static Sprite DocumentationIcon;
        public static ItemDef DocumentationItemDef;


        internal static void Init()
        {
            DocumentationPrefab = MainAssets.LoadAsset<GameObject>("Assets/Models/Prefabs/Documentation.prefab");
            DocumentationIcon = MainAssets.LoadAsset<Sprite>("Assets/Icons/Documentation.png");

            Item();

            AddLanguageTokens();
        }
        private static void Item()
        {
            DocumentationItemDef = ScriptableObject.CreateInstance<ItemDef>();
            DocumentationItemDef.name = "CapturedPotentialDocumentation";
            DocumentationItemDef.nameToken = "CAPTUREDPOTENTIALDOCUMENTATION_NAME";
            DocumentationItemDef.pickupToken = "CAPTUREDPOTENTIALDOCUMENTATION_PICKUP";
            DocumentationItemDef.descriptionToken = "CAPTUREDPOTENTIALDOCUMENTATIONR_DESC";
            DocumentationItemDef.loreToken = "CAPTUREDPOTENTIALDOCUMENTATION_LORE";
            DocumentationItemDef.deprecatedTier = ItemTier.NoTier;
            DocumentationItemDef.pickupIconSprite = DocumentationIcon;
            DocumentationItemDef.pickupModelPrefab = DocumentationPrefab;
            DocumentationItemDef.canRemove = true;
            DocumentationItemDef.hidden = false;
            DocumentationItemDef.requiredExpansion = CaeliImperiumExpansionDef;
            var displayRules = new ItemDisplayRuleDict(null);
            ItemAPI.Add(new CustomItem(DocumentationItemDef, displayRules));
            On.RoR2.CharacterMaster.OnServerStageBegin += StageStart;
        }

        private static void StageStart(On.RoR2.CharacterMaster.orig_OnServerStageBegin orig, CharacterMaster self, Stage stage)
        {
            orig(self, stage);
            int itemCount = self.inventory ? self.inventory.GetItemCount(DocumentationItemDef) : 0;
            if (itemCount > 0)
            {
                self.inventory.RemoveItem(DocumentationItemDef, itemCount);
            }
        }

        private static void AddLanguageTokens()
        {
            LanguageAPI.Add("CAPTUREDPOTENTIALDOCUMENTATION_NAME", "Captured Potential Documentation");
            LanguageAPI.Add("CAPTUREDPOTENTIALDOCUMENTATION_PICKUP", "To <style=cIsUtility>switch equipments</style>, <style=cIsUtility>hold</style> the <style=cUserSetting>first key</style> <style=cStack>(E by default)</style> and <style=cUserSetting>press second key</style> <style=cStack>(Alpha 1 by default)</style> to <style=cIsUtility>switch equipment</style> to the <style=cIsUtility>next one</style> or <style=cUserSetting>third key</style> <style=cStack>(Alpha 3 by default)</style> to <style=cIsUtility>switch it</style> to the <style=cIsUtility>previous one</style>. You can <style=cUserSetting>unbind</style> the <style=cUserSetting>first key</style> to <style=cIsUtility>always switch equipments</style> on <style=cUserSetting>second</style> and <style=cUserSetting>third keys press</style>. You can also <style=cIsUtility>use</style> your <style=cUserSetting>Mouse Wheel</style> to <style=cIsUtility>switch equipments forth</style> and <style=cIsUtility>back</style>. <style=cDeath>Controller support is not implemented yet</style>. At the core uses console commands to switch equipments, <style=cUserSetting>\"EquipArrayIndexUp\"</style> to <style=cIsUtility>switch forward</style>, <style=cUserSetting>\"EquipArrayIndexDown\"</style> to <style=cIsUtility>switch backwards</style>");
            LanguageAPI.Add("CAPTUREDPOTENTIALDOCUMENTATION_DESC", "To <style=cIsUtility>switch equipments</style>, <style=cIsUtility>hold</style> the <style=cUserSetting>first key</style> <style=cStack>(E by default)</style> and <style=cUserSetting>press second key</style> <style=cStack>(Alpha 1 by default)</style> to <style=cIsUtility>switch equipment</style> to the <style=cIsUtility>next one</style> or <style=cUserSetting>third key</style> <style=cStack>(Alpha 3 by default)</style> to <style=cIsUtility>switch it</style> to the <style=cIsUtility>previous one</style>. You can <style=cUserSetting>unbind</style> the <style=cUserSetting>first key</style> to <style=cIsUtility>always switch equipments</style> on <style=cUserSetting>second</style> and <style=cUserSetting>third keys press</style>. You can also <style=cIsUtility>use</style> your <style=cUserSetting>Mouse Wheel</style> to <style=cIsUtility>switch equipments forth</style> and <style=cIsUtility>back</style>. <style=cDeath>Controller support is not implemented yet</style>. At the core uses console commands to switch equipments, <style=cUserSetting>\"EquipArrayIndexUp\"</style> to <style=cIsUtility>switch forward</style>, <style=cUserSetting>\"EquipArrayIndexDown\"</style> to <style=cIsUtility>switch backwards</style>");
            LanguageAPI.Add("CAPTUREDPOTENTIALDOCUMENTATION_LORE", "");
        }
    }
}
