using R2API;
using RoR2;
using RoR2.Audio;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using static CaeliImperiumPlugin.CaeliImperium;
using static CaeliImperium.Equipment.EnforcerHand;
using static RoR2.MasterSpawnSlotController;

namespace CaeliImperium.Buffs
{
    internal static class ParryNextDamageBuff
    {
        public static BuffDef ParryNextDamageBuffDef;
        internal static Sprite ParryNextDamageIcon;
        public static NetworkSoundEventDef ShieldBlockSound;
        internal static void Init()
        {

            CreateSound();
            ParryNextDamageIcon = MainAssets.LoadAsset<Sprite>("Assets/Icons/ReflectDamage.png");

            Buff();

        }
        private static void Buff()
        {
            ParryNextDamageBuffDef = ScriptableObject.CreateInstance<BuffDef>();
            ParryNextDamageBuffDef.name = "ciParryNextDamage";
            ParryNextDamageBuffDef.buffColor = Color.white;
            ParryNextDamageBuffDef.canStack = false;
            ParryNextDamageBuffDef.isDebuff = false;
            ParryNextDamageBuffDef.ignoreGrowthNectar = false;
            ParryNextDamageBuffDef.iconSprite = ParryNextDamageIcon;
            ParryNextDamageBuffDef.isHidden = false;
            ParryNextDamageBuffDef.isCooldown = false;
            ContentAddition.AddBuffDef(ParryNextDamageBuffDef);
            On.RoR2.HealthComponent.TakeDamageProcess += Parry;
        }
        private static void CreateSound()
        {
            ShieldBlockSound = ScriptableObject.CreateInstance<NetworkSoundEventDef>();
            ShieldBlockSound.eventName = "Play_block";
            R2API.ContentAddition.AddNetworkSoundEventDef(ShieldBlockSound);
        }
        private static void Parry(On.RoR2.HealthComponent.orig_TakeDamageProcess orig, HealthComponent self, DamageInfo damageInfo)
        {
            if (damageInfo != null && self != null && self.body.HasBuff(ParryNextDamageBuffDef))
            {
            var attackerToStun = damageInfo.attacker ? damageInfo.attacker.GetComponent<CharacterBody>() : null;
                if (attackerToStun != null)
                {
                    Vector3 targetDir = attackerToStun.corePosition - self.body.corePosition;
                    float angle = Vector3.Angle(targetDir, self.body.inputBank.aimDirection);
                    //float dist = Vector3.Distance(attackerToStun.corePosition, self.body.corePosition);
                    if (angle < EnforcerHandViewAngle.Value / 2)
                    {
                    SetStateOnHurt component = attackerToStun.GetComponent<SetStateOnHurt>();
                        /*
                    EffectData effectData = new EffectData
                    {
                        origin = damageInfo.position,
                        rotation = Util.QuaternionSafeLookRotation((damageInfo.force != Vector3.zero) ? damageInfo.force : UnityEngine.Random.onUnitSphere)
                    };
                        //EffectManager.SpawnEffect(HealthComponent.AssetReferences.bearEffectPrefab, effectData, true);
                        /*damageInfo.damage = 0;
                        damageInfo.rejected = true;
                        float damage = damageInfo.damage;
                        Vector3 position2 = characterBody.transform.position;
                        DamageInfo damageInfo2 = new DamageInfo
                        {
                            damage = damage * stack,
                            damageColorIndex = DamageColorIndex.Item,
                            damageType = DamageType.Generic,
                            attacker = body.gameObject,
                            crit = Util.CheckRoll(body.crit),
                            force = Vector3.zero,
                            inflictor = null,
                            position = position2,
                            procChainMask = default,
                            procCoefficient = 1f
                        };*/
                        if (self.body != attackerToStun && attackerToStun)
                        {
                            var newDamage = damageInfo;
                            newDamage.damage *= EnforcerHandReflectDamageMultiplier.Value;
                            newDamage.damage += (attackerToStun.maxHealth * EnforcerHandMaxHealthDamageMulyiplier.Value / 100) + (self.body.damageFromRecalculateStats * EnforcerHandTotalDamageMultiplier.Value / 100);
                            attackerToStun.healthComponent.TakeDamage(newDamage);
                            for (int i = 0; i < EnforcerHandWoundedCount.Value; i++)
                            {
                            attackerToStun.AddBuff(WoundedBuff.WoundedBuffDef);

                            }
                            if (!attackerToStun.isChampion && EnforcerHandDoStun.Value)
                            {
                                if (component.hasEffectiveAuthority)
                                {
                                    component.SetStunInternal(1f);
                                    EffectManager.SimpleImpactEffect(LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/ImpactEffects/ImpactStunGrenade"), attackerToStun.corePosition, attackerToStun.corePosition, true);
                                }
                                else
                                {
                                    component.CallRpcSetStun(1f);
                                    EffectManager.SimpleImpactEffect(LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/ImpactEffects/ImpactStunGrenade"), attackerToStun.corePosition, attackerToStun.corePosition, true);

                                }
                            }
                        }
                        damageInfo.rejected = true;
                        EntitySoundManager.EmitSoundServer(ShieldBlockSound.akId, self.gameObject);
                        //Util.PlaySound("Play_item_proc_negateAttack", self.gameObject);
                        self.body.inventory.DeductActiveEquipmentCooldown(EnforcerHandCooldownDeduction.Value);
                        self.body.SetBuffCount(ParryNextDamageBuffDef.buffIndex, 0);
                        self.body.AddTimedBuff(RoR2Content.Buffs.Immune, EnforcerHandImmunity.Value);
                    //Debug.Log(damageInfo);
                    //Debug.Log(damageInfo.attacker);
                    }

                }

            }
            orig(self, damageInfo);
        }
    }
}
