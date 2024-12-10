using EmotesAPI;
using R2API;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using static CaeliImperiumPlugin.CaeliImperium;
using static CaeliImperium.Items.DiscoBall;
using static R2API.RecalculateStatsAPI;
using MonoMod.Cil;
using UnityEngine.AddressableAssets;
using RoR2.Skills;
using CaeliImperium.Items;
namespace CaeliImperium.Buffs
{
    internal static class DanceFrenzyBuff
    {
        public static BuffDef DanceFrenzyBuffDef;
        internal static Sprite DanceFrenzyTimerIcon;
        public static SkillDef skillDef = Addressables.LoadAssetAsync<SkillDef>("RoR2/DLC2/Common/DisabledSkill.asset").WaitForCompletion();
        internal static void Init()
        {


            DanceFrenzyTimerIcon = MainAssets.LoadAsset<Sprite>("Assets/Icons/Killswitchbuff.png");

            Buff();

        }
        private static void Buff()
        {
            DanceFrenzyBuffDef = ScriptableObject.CreateInstance<BuffDef>();
            DanceFrenzyBuffDef.name = "ciDanceFrenzy";
            DanceFrenzyBuffDef.buffColor = Color.black;
            DanceFrenzyBuffDef.canStack = false;
            DanceFrenzyBuffDef.isDebuff = true;
            DanceFrenzyBuffDef.ignoreGrowthNectar = true;
            DanceFrenzyBuffDef.iconSprite = DanceFrenzyTimerIcon;
            DanceFrenzyBuffDef.isHidden = true;
            DanceFrenzyBuffDef.isCooldown = false;
            ContentAddition.AddBuffDef(DanceFrenzyBuffDef);
            On.RoR2.CharacterBody.FixedUpdate += DANCE;
            //IL.RoR2.CharacterBody.HandleDisableAllSkillsDebuff += (il) =>
            //{
            //    ILCursor c = new ILCursor(il);
            //    c.GotoNext(
            //        x => x.MatchLdarg(0),
            //        x => x.MatchLdsfld<BuffDef>("DisableAllSkills"),
            //        x => x.MatchCall<bool>("HasBuff(class RoR2.BuffDef)"),
            //        x => x.MatchStloc(0),
            //        x => x.MatchLdcR4(0.9f)
            //        );
            //    c.Index += 4;
            //};
            //On.RoR2.CharacterBody.HandleDisableAllSkillsDebuff += ThisIsAwful;
            On.RoR2.CharacterBody.OnBuffFinalStackLost += STOPDANCING;
            On.RoR2.CharacterBody.OnBuffFirstStackGained += STOPATTACKING;
            GetStatCoefficients += Slowdown;
        }

        private static void STOPATTACKING(On.RoR2.CharacterBody.orig_OnBuffFirstStackGained orig, CharacterBody self, BuffDef buffDef)
        {
            orig(self, buffDef);
            if (buffDef == DanceFrenzyBuffDef)
            {
                if (self.skillLocator.primary)// && self.skillLocator.primary.skillName != skillDef.skillName)
                {
                    self.skillLocator.primary.SetSkillOverride(self, skillDef, GenericSkill.SkillOverridePriority.Contextual);
                }
                if (self.skillLocator.secondary)// && self.skillLocator.secondary.skillName != skillDef.skillName)
                {
                    self.skillLocator.secondary.SetSkillOverride(self, skillDef, GenericSkill.SkillOverridePriority.Contextual);
                }
                if (self.skillLocator.utility)// && self.skillLocator.utility.skillName != skillDef.skillName)
                {
                    self.skillLocator.utility.SetSkillOverride(self, skillDef, GenericSkill.SkillOverridePriority.Contextual);
                }
                if (self.skillLocator.special)// && self.skillLocator.special.skillName != skillDef.skillName)
                {
                    self.skillLocator.special.SetSkillOverride(self, skillDef, GenericSkill.SkillOverridePriority.Contextual);
                }
            }
        }

        private static void Slowdown(CharacterBody sender, StatHookEventArgs args)
        {
            int buffCount = sender.GetBuffCount(DanceFrenzyBuffDef);

            if (buffCount > 0)
            {
                args.moveSpeedReductionMultAdd += 1 + (ConfigFloat(DiscoBallSlowdown, DiscoBallEnableConfig) / 100);
            }
        }

        //private static void ThisIsAwful(On.RoR2.CharacterBody.orig_HandleDisableAllSkillsDebuff orig, CharacterBody self)
        //{
        //    orig(self);
        //    if (self.HasBuff(RoR2.DLC2Content.Buffs.DisableAllSkills) || self.HasBuff(DLC2Content.Buffs.DisableAllSkills))
        //    {
        //        self.allSkillsDisabled = true;
        //    }
        //    else
        //    {
        //        self.allSkillsDisabled = false;
        //    }
        //}

        private static void STOPDANCING(On.RoR2.CharacterBody.orig_OnBuffFinalStackLost orig, CharacterBody self, BuffDef buffDef)
        {
            orig(self, buffDef);
            if (buffDef == DanceFrenzyBuffDef)
            {
                if (BoneMapper.characterBodiesToBoneMappers[self] && BoneMapper.characterBodiesToBoneMappers[self].currentClipName != "none")
                {
                    CustomEmotesAPI.PlayAnimation("none", BoneMapper.characterBodiesToBoneMappers[self]);
                }
                if (self.skillLocator.primary)
                {
                    self.skillLocator.primary.UnsetSkillOverride(self, skillDef, GenericSkill.SkillOverridePriority.Contextual);
                }
                if (self.skillLocator.secondary)
                {
                    self.skillLocator.secondary.UnsetSkillOverride(self, skillDef, GenericSkill.SkillOverridePriority.Contextual);
                }
                if (self.skillLocator.utility)
                {
                    self.skillLocator.utility.UnsetSkillOverride(self, skillDef, GenericSkill.SkillOverridePriority.Contextual);
                }
                if (self.skillLocator.special)
                {
                    self.skillLocator.special.UnsetSkillOverride(self, skillDef, GenericSkill.SkillOverridePriority.Contextual);
                }
            }
        }

        private static void DANCE(On.RoR2.CharacterBody.orig_FixedUpdate orig, CharacterBody self)
        {
            orig(self);
            if (self.GetBuffCount(DanceFrenzyBuffDef) > 0)
            {
                if (BoneMapper.characterBodiesToBoneMappers[self] && BoneMapper.characterBodiesToBoneMappers[self].currentClipName == "none")
                {
                    var emoteArray2 = emoteArray;
                    if (DiscoBallPizzaTowerMode.Value && ModCompatability.EmotesCompatibility.brynzaEmotesEnabled)
                    {
                        emoteArray2 = Taunts;
                    }
                    var emoteIndex = UnityEngine.Random.Range(0, emoteArray2.Length);
                    CustomEmotesAPI.PlayAnimation(emoteArray2[emoteIndex], BoneMapper.characterBodiesToBoneMappers[self]);
                }/*
                if (self.skillLocator.primary)// && self.skillLocator.primary.skillName != skillDef.skillName)
                {
                    self.skillLocator.primary.SetSkillOverride(self, skillDef, GenericSkill.SkillOverridePriority.Contextual);
                }
                if (self.skillLocator.secondary)// && self.skillLocator.secondary.skillName != skillDef.skillName)
                {
                    self.skillLocator.secondary.SetSkillOverride(self, skillDef, GenericSkill.SkillOverridePriority.Contextual);
                }
                if (self.skillLocator.utility)// && self.skillLocator.utility.skillName != skillDef.skillName)
                {
                    self.skillLocator.utility.SetSkillOverride(self, skillDef, GenericSkill.SkillOverridePriority.Contextual);
                }
                if (self.skillLocator.special)//l && self.skillLocator.special.skillName != skillDef.skillName)
                {
                    self.skillLocator.special.SetSkillOverride(self, skillDef, GenericSkill.SkillOverridePriority.Contextual);
                }*/
                //if (!self.allSkillsDisabled)
                //{
                //    self.allSkillsDisabled = true;
                //}
            }
        }
    }
}
