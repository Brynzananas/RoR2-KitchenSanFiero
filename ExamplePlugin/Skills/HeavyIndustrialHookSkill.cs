using EntityStates;
using R2API;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine.AddressableAssets;
using UnityEngine;
using RoR2.Skills;
using RoR2;
using EntityStates.Loader;
using static KitchenSanFieroPlugin.KitchenSanFiero;
using static RoR2.MasterSpawnSlotController;

namespace KitchenSanFiero.Skills
{
    internal static class HeavyIndustrialHookSkill
    {
        //public static SkillDef HeavyIndustrialHookSkillDef;
        internal static void Init()
        {
            Skill();
        }
        public static void Skill()
        {
            // First we must load our survivor's Body prefab. For this tutorial, we are making a skill for Commando
            // If you would like to load a different survivor, you can find the key for their Body prefab at the following link
            // https://xiaoxiao921.github.io/GithubActionCacheTest/assetPathsDump.html
            GameObject commandoBodyPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Commando/CommandoBody.prefab").WaitForCompletion();

            // We use LanguageAPI to add strings to the game, in the form of tokens
            // Please note that it is instead recommended that you use a language file.
            // More info in https://risk-of-thunder.github.io/R2Wiki/Mod-Creation/Assets/Localization/
            LanguageAPI.Add("COMMANDO_PRIMARY_SIMPLEBULLET_NAME", "Simple Bullet Attack");
            LanguageAPI.Add("COMMANDO_PRIMARY_SIMPLEBULLET_DESCRIPTION", $"Fire a bullet from your right pistol for <style=cIsDamage>300% damage</style>.");

            // Now we must create a SkillDef
            SkillDef HeavyIndustrialHookSkillDef = ScriptableObject.CreateInstance<SkillDef>();

            //Check step 2 for the code of the CustomSkillsTutorial.MyEntityStates.SimpleBulletAttack class
            HeavyIndustrialHookSkillDef.activationState = new SerializableEntityStateType(typeof(HeavyIndustrialHookEntityState));
            HeavyIndustrialHookSkillDef.activationStateMachineName = "Weapon";
            HeavyIndustrialHookSkillDef.baseMaxStock = 1;
            HeavyIndustrialHookSkillDef.baseRechargeInterval = 0f;
            HeavyIndustrialHookSkillDef.beginSkillCooldownOnSkillEnd = true;
            HeavyIndustrialHookSkillDef.canceledFromSprinting = false;
            HeavyIndustrialHookSkillDef.cancelSprintingOnActivation = true;
            HeavyIndustrialHookSkillDef.fullRestockOnAssign = true;
            HeavyIndustrialHookSkillDef.interruptPriority = InterruptPriority.Any;
            HeavyIndustrialHookSkillDef.isCombatSkill = true;
            HeavyIndustrialHookSkillDef.mustKeyPress = false;
            HeavyIndustrialHookSkillDef.rechargeStock = 1;
            HeavyIndustrialHookSkillDef.requiredStock = 1;
            HeavyIndustrialHookSkillDef.stockToConsume = 1;
            // For the skill icon, you will have to load a Sprite from your own AssetBundle
            HeavyIndustrialHookSkillDef.icon = MainAssets.LoadAsset<Sprite>("Assets/Materials/Item/ForbiddenTome/ForbiddenTome.png");
            HeavyIndustrialHookSkillDef.skillDescriptionToken = "COMMANDO_PRIMARY_SIMPLEBULLET_DESCRIPTION";
            HeavyIndustrialHookSkillDef.skillName = "COMMANDO_PRIMARY_SIMPLEBULLET_NAME";
            HeavyIndustrialHookSkillDef.skillNameToken = "COMMANDO_PRIMARY_SIMPLEBULLET_NAME";

            // This adds our skilldef. If you don't do this, the skill will not work.
            ContentAddition.AddSkillDef(HeavyIndustrialHookSkillDef);

            // Now we add our skill to one of the survivor's skill families
            // You can change component.primary to component.secondary, component.utility and component.special
            SkillLocator skillLocator = commandoBodyPrefab.GetComponent<SkillLocator>();
            SkillFamily skillFamily = skillLocator.primary.skillFamily;

            // If this is an alternate skill, use this code.
            // Here, we add our skill as a variant to the existing Skill Family.
            Array.Resize(ref skillFamily.variants, skillFamily.variants.Length + 1);
            skillFamily.variants[skillFamily.variants.Length - 1] = new SkillFamily.Variant
            {
                skillDef = HeavyIndustrialHookSkillDef,
                unlockableName = "",
                viewableNode = new ViewablesCatalog.Node(HeavyIndustrialHookSkillDef.skillNameToken, false, null)
            };
            //On.RoR2.CharacterBody.Start += HookHook;
        }
        /*
        private static void HookHook(On.RoR2.CharacterBody.orig_Start orig, CharacterBody self)
        {
            orig(self);
            EntityStateMachine[] components = self.gameObject.GetComponents<EntityStateMachine>();
            bool hasHook = false;
            for (int i = 0; i < components.Length; i++)
            {
                if (components[i].customName == "Hook")
                {
                    hasHook = true;
                }
            }
            if (!hasHook)
            {
                self.gameObject.AddComponent<EntityStateMachine>().customName += "Hook";
            }
        }*/
    }
}
