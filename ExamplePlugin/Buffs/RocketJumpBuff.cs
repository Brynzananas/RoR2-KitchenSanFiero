using EmotesAPI;
using R2API;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using static CaeliImperiumPlugin.CaeliImperium;
using MonoMod.Cil;
using UnityEngine.AddressableAssets;
using RoR2.Skills;
using CaeliImperium.Items;
using static CaeliImperium.Items.ThunderThighs;
namespace CaeliImperium.Buffs
{
    internal static class RocketJumpBuff
    {
        public static BuffDef RocketJumpBuffDef;
        internal static Sprite RocketJumprIcon;
        internal static void Init()
        {


            RocketJumprIcon = MainAssets.LoadAsset<Sprite>("Assets/Icons/Killswitchbuff.png");

            Buff();

        }
        private static void Buff()
        {
            RocketJumpBuffDef = ScriptableObject.CreateInstance<BuffDef>();
            RocketJumpBuffDef.name = "ciRocketJump";
            RocketJumpBuffDef.buffColor = Color.black;
            RocketJumpBuffDef.canStack = true;
            RocketJumpBuffDef.isDebuff = false;
            RocketJumpBuffDef.ignoreGrowthNectar = false;
            RocketJumpBuffDef.iconSprite = RocketJumprIcon;
            RocketJumpBuffDef.isHidden = false;
            RocketJumpBuffDef.isCooldown = false;
            ContentAddition.AddBuffDef(RocketJumpBuffDef);
            On.RoR2.CharacterBody.FixedUpdate += Fly;
            On.RoR2.GlobalEventManager.OnCharacterHitGroundServer += Unfly;
        }

        private static void Unfly(On.RoR2.GlobalEventManager.orig_OnCharacterHitGroundServer orig, GlobalEventManager self, CharacterBody characterBody, CharacterMotor.HitGroundInfo hitGroundInfo)
        {
            orig(self, characterBody, hitGroundInfo);
            if (characterBody.HasBuff(RocketJumpBuffDef))
            {
                int itemCount = characterBody.inventory ? characterBody.inventory.GetItemCount(ThunderThighsItemDef) : 0;
                float num = 12f * characterBody.GetBuffCount(RocketJumpBuffDef);
                float damageCoefficient = 0.6f;
                float baseDamage = characterBody.damage * itemCount * characterBody.GetBuffCount(RocketJumpBuffDef) * 10;
                EffectManager.SpawnEffect(LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/OmniEffect/OmniExplosionVFXQuick"), new EffectData
                {
                    origin = characterBody.footPosition,
                    scale = num,
                    rotation = Util.QuaternionSafeLookRotation(characterBody.transform.up)
                }, true);
                BlastAttack blastAttack = new BlastAttack();
                blastAttack.position = characterBody.footPosition;
                blastAttack.baseDamage = baseDamage;
                blastAttack.baseForce = 0f;
                blastAttack.radius = num;
                blastAttack.attacker = characterBody.gameObject;
                blastAttack.inflictor = null;
                blastAttack.teamIndex = TeamComponent.GetObjectTeam(blastAttack.attacker);
                blastAttack.crit = Util.CheckRoll(characterBody.crit);
                blastAttack.procChainMask = default;
                blastAttack.procCoefficient = 0f;
                blastAttack.damageColorIndex = DamageColorIndex.Item;
                blastAttack.falloffModel = BlastAttack.FalloffModel.None;
                blastAttack.damageType = DamageType.AOE;
                blastAttack.Fire();
                characterBody.SetBuffCount(RocketJumpBuffDef.buffIndex, 0);
            }
        }

        private static void Fly(On.RoR2.CharacterBody.orig_FixedUpdate orig, CharacterBody self)
        {
            orig(self);
            if (self.HasBuff(RocketJumpBuffDef))
            {
                //if (self.inputBank.rawMoveData != Vector2.zero)
                //{
                //    Chat.AddMessage("true");
                //}
                int buffCount = self.GetBuffCount(RocketJumpBuffDef);
                Vector3 forward = Vector3.zero;
                if (self.inputBank && self.characterDirection)
                {
                    self.characterDirection.moveVector = self.inputBank.moveVector;
                    forward = self.characterDirection.forward;
                }
                if (self.characterMotor)
                {
                self.characterMotor.rootMotion += forward * self.moveSpeed * 0.03f * buffCount;

                }
                int itemCount = self.inventory ? self.inventory.GetItemCount(ThunderThighsItemDef) : 0;
                if (self.GetBuffCount(RocketJumpBuffDef) < itemCount && self.inputBank.jump.justPressed)
                {
                    RocketJump(self);
                    float num = 12f;// * buffCount;
                    float damageCoefficient = 0.6f;
                    float baseDamage = self.damage * itemCount;// * buffCount * 10;
                    EffectManager.SpawnEffect(LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/OmniEffect/OmniExplosionVFXQuick"), new EffectData
                    {
                        origin = self.footPosition,
                        scale = num,
                        rotation = Util.QuaternionSafeLookRotation(self.transform.up)
                    }, true);
                    BlastAttack blastAttack = new BlastAttack();
                    blastAttack.position = self.footPosition;
                    blastAttack.baseDamage = baseDamage;
                    blastAttack.baseForce = 0f;
                    blastAttack.radius = num;
                    blastAttack.attacker = self.gameObject;
                    blastAttack.inflictor = null;
                    blastAttack.teamIndex = TeamComponent.GetObjectTeam(blastAttack.attacker);
                    blastAttack.crit = Util.CheckRoll(self.crit);
                    blastAttack.procChainMask = default;
                    blastAttack.procCoefficient = 0f;
                    blastAttack.damageColorIndex = DamageColorIndex.Item;
                    blastAttack.falloffModel = BlastAttack.FalloffModel.None;
                    blastAttack.damageType = DamageType.AOE;
                    blastAttack.Fire();
                }
            }
        }
    }
}
