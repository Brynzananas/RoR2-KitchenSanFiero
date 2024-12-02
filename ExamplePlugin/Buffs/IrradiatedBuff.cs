using BepInEx.Configuration;
using R2API;
using RiskOfOptions.Options;
using RiskOfOptions;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using static CaeliImperiumPlugin.CaeliImperium;
using static R2API.DotAPI;
using JetBrains.Annotations;

namespace CaeliImperium.Buffs
{
    internal static class IrradiatedBuff
    {
        public static BuffDef IrradiatedBuffDef;
        public static DotController.DotIndex IrradiatedDOTDef;
        public static CustomDotBehaviour behaviour;
        public static Color newColor = new Color(0f, 0.98f, 0.41f, 1f);
        internal static Sprite IrradiatedIcon;
        public static float timer2;
        public static ConfigEntry<bool> IrradiatedDoDamage;
        public static ConfigEntry<bool> IrradiatedDoDebuff;
        public static ConfigEntry<bool> IrradiatedCanStack;
        public static ConfigEntry<float> IrradiatedRange;
        public static ConfigEntry<float> IrradiatedUsualDamage;
        public static ConfigEntry<float> IrradiatedDamage;
        public static ConfigEntry<float> IrradiatedTimer;
        public static ConfigEntry<float> IrradiatedDuration;
        public static ConfigEntry<int> IrradiatedMaxStack;
        internal static void Init()
        {


            IrradiatedIcon = MainAssets.LoadAsset<Sprite>("Assets/Icons/Irradiated.png");
            AddConfigs();
            behaviour = DOTBehaviour;
            Buff();
            DOT();

        }

        private static void AddConfigs()
        {
            IrradiatedDoDamage = Config.Bind<bool>("Buff : Irradiated",
                             "Do damage",
                             true,
                             "Does this DOT damages nearby allies?");
            IrradiatedDoDebuff = Config.Bind<bool>("Buff : Irradiated",
                             "Do DOT?",
                             false,
                             "Does this DOT shares this DOT with nearby allies?");
            IrradiatedCanStack = Config.Bind<bool>("Buff : Irradiated",
                             "Can stack?",
                             false,
                             "Can this DOT stack, increase all its damage values?");
            IrradiatedRange = Config.Bind<float>("Buff : Irradiated",
                             "Range",
                             3.6f,
                             "Control the range value");
            IrradiatedUsualDamage = Config.Bind<float>("Buff : Irradiated",
                             "Damage",
                             1f,
                             "Control the damage value in percentage");
            IrradiatedDamage = Config.Bind<float>("Buff : Irradiated",
                             "DOT damage",
                             1f,
                             "Control the DOT damage value in percentage");
            IrradiatedTimer = Config.Bind<float>("Buff : Irradiated",
                 "Interval",
                 1f,
                 "Control the DOT interval value in seconds");
            IrradiatedDuration = Config.Bind<float>("Buff : Irradiated",
                 "Duration",
                 5f,
                 "Control the DOT duration value");
            IrradiatedMaxStack = Config.Bind<int>("Buff : Irradiated",
                 "Max stack",
                 5,
                 "Control the DOT max stack value");
            ModSettingsManager.AddOption(new FloatFieldOption(IrradiatedRange));
            ModSettingsManager.AddOption(new FloatFieldOption(IrradiatedUsualDamage));
            ModSettingsManager.AddOption(new FloatFieldOption(IrradiatedDamage));
            ModSettingsManager.AddOption(new FloatFieldOption(IrradiatedTimer));
            ModSettingsManager.AddOption(new FloatFieldOption(IrradiatedDuration));
            ModSettingsManager.AddOption(new IntFieldOption(IrradiatedMaxStack));
        }

        private static void DOTBehaviour(DotController self, DotController.DotStack dotStack)
        {
            dotStack.dotIndex = IrradiatedDOTDef;
            dotStack.attackerObject = null;
            dotStack.attackerTeam = TeamIndex.Neutral;
            dotStack.timer = 0f;
            dotStack.damage = 0f;
            dotStack.damageType = default(DamageTypeCombo);
        }

        private static void Buff()
        {
            IrradiatedBuffDef = ScriptableObject.CreateInstance<BuffDef>();
            IrradiatedBuffDef.name = "ciIrradiated";
            IrradiatedBuffDef.buffColor = Color.white;
            IrradiatedBuffDef.canStack = true;
            IrradiatedBuffDef.isDebuff = false;
            IrradiatedBuffDef.ignoreGrowthNectar = true;
            IrradiatedBuffDef.iconSprite = IrradiatedIcon;
            IrradiatedBuffDef.isHidden = false;
            IrradiatedBuffDef.isCooldown = false;
            ContentAddition.AddBuffDef(IrradiatedBuffDef);
            //On.RoR2.CharacterBody.FixedUpdate += IrradiateNeaby;
            On.RoR2.CharacterBody.OnBuffFirstStackGained += IrradiateBehaviourInitialisation;
            On.RoR2.CharacterBody.OnBuffFinalStackLost += IrradiateBehaviourDeletion;
        }

        private static void IrradiateBehaviourDeletion(On.RoR2.CharacterBody.orig_OnBuffFinalStackLost orig, CharacterBody self, BuffDef buffDef)
        {
            orig(self, buffDef);
            if (buffDef == IrradiatedBuffDef)
            {
                if (self.gameObject.GetComponent<IrradiatedBuff.IrradiateComponent>())
                {
                    UnityEngine.Object.Destroy(self.gameObject.GetComponent<IrradiatedBuff.IrradiateComponent>());
                }
            }
        }

        public class IrradiateComponent : MonoBehaviour
        {
            public CharacterBody body;
            public float timer;
            public void FixedUpdate()
            {
                timer += Time.fixedDeltaTime;
                if (timer > IrradiatedTimer.Value)
                {
                    if (body && body.HasBuff(IrradiatedBuffDef))
                    {
                        foreach (var characterBody in CharacterBody.readOnlyInstancesList)
                        {
                            float dist = Vector3.Distance(characterBody.corePosition, body.corePosition);

                            if (dist < IrradiatedRange.Value && body.teamComponent.teamIndex == characterBody.teamComponent.teamIndex && body != characterBody)
                            {
                                float buffCount = 1;
                                if (IrradiatedCanStack.Value)
                                {
                                    buffCount = body.GetBuffCount(IrradiatedBuff.IrradiatedBuffDef);
                                }
                                if (IrradiatedDoDamage.Value)
                                {
                                    DamageInfo damageInfo2 = new DamageInfo
                                    {
                                        damage = characterBody.maxHealth * (IrradiatedUsualDamage.Value / 100) * buffCount,
                                        damageColorIndex = DamageColorIndex.Item,
                                        damageType = DamageType.BypassArmor,
                                        attacker = null,
                                        crit = false,
                                        force = Vector3.zero,
                                        inflictor = null,
                                        position = characterBody.transform.position,
                                        procChainMask = default,
                                        procCoefficient = 0f
                                    };
                                    characterBody.healthComponent.TakeDamage(damageInfo2);
                                }

                                if (IrradiatedDoDebuff.Value)
                                {
                                    InflictDotInfo dotInfo = new InflictDotInfo()
                                    {
                                        attackerObject = null,
                                        victimObject = characterBody.gameObject,
                                        totalDamage = body.maxHealth * (IrradiatedDamage.Value / 100),
                                        damageMultiplier = buffCount,
                                        duration = IrradiatedDuration.Value,
                                        dotIndex = Buffs.IrradiatedBuff.IrradiatedDOTDef,
                                        maxStacksFromAttacker = (uint?)IrradiatedMaxStack.Value

                                    };
                                    DotController.InflictDot(ref dotInfo);
                                }

                                
                            }

                        }
                    }

                    timer = 0f;
                }

            }
        }

        private static void IrradiateBehaviourInitialisation(On.RoR2.CharacterBody.orig_OnBuffFirstStackGained orig, CharacterBody self, BuffDef buffDef)
        {
            orig(self, buffDef);
            if (buffDef == IrradiatedBuffDef)
            {
                if (!self.gameObject.GetComponent<IrradiatedBuff.IrradiateComponent>())
                {
                    IrradiatedBuff.IrradiateComponent component = self.gameObject.AddComponent<IrradiatedBuff.IrradiateComponent>();
                    component.body = self;
                }
            }
            

        }
        /*
        private static void IrradiateNeaby(On.RoR2.CharacterBody.orig_FixedUpdate orig, CharacterBody self)
        {
            orig(self);
            if (self.GetBuffCount(IrradiatedBuff.IrradiatedBuffDef) > 0)
            {
                foreach (var characterBody in CharacterBody.readOnlyInstancesList)
                {
                    float dist = Vector3.Distance(characterBody.corePosition, self.corePosition);
                    timer2 += Time.fixedDeltaTime;
                    if (timer2 > IrradiatedTimer.Value * 2)
                    {
                        if (dist < IrradiatedRange.Value && self.teamComponent.teamIndex == characterBody.teamComponent.teamIndex && self != characterBody)
                        {
                            InflictDotInfo dotInfo = new InflictDotInfo()
                            {
                                attackerObject = self.gameObject,
                                victimObject = characterBody.gameObject,
                                totalDamage = self.maxHealth * IrradiatedDamage.Value,
                                damageMultiplier = self.GetBuffCount(IrradiatedBuff.IrradiatedBuffDef),
                                duration = 5f,
                                dotIndex = Buffs.IrradiatedBuff.IrradiatedDOTDef,
                                maxStacksFromAttacker = (uint?)IrradiatedMaxStack.Value

                            };
                            StrengthenBurnUtils.CheckDotForUpgrade(self.inventory, ref dotInfo);
                            DotController.InflictDot(ref dotInfo);
                        }
                        timer2 = 0f;
                    }

                }
            }
        }
        */
        private static void DOT()
        {
            IrradiatedDOTDef = DotAPI.RegisterDotDef(new DotController.DotDef
            {
                resetTimerOnAdd = true,
                interval = 0.5f,
                damageCoefficient = 0.5f,
                damageColorIndex = DamageColorIndex.Poison,
                associatedBuff = IrradiatedBuffDef
            }, (CustomDotBehaviour)behaviour, (CustomDotVisual)null

                );


        }
    }
}
