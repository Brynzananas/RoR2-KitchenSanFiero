using BepInEx.Configuration;
using R2API;
using RiskOfOptions.Options;
using RiskOfOptions;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking.Match;
using static KitchenSanFieroPlugin.KitchenSanFiero;
using static R2API.DotAPI;
using JetBrains.Annotations;

namespace KitchenSanFiero.Buffs
{
    internal static class IrradiatedBuff
    {
        public static BuffDef IrradiatedBuffDef;
        public static DotController.DotIndex IrradiatedDOTDef;
        public static CustomDotBehaviour behaviour;
        public static Color newColor = new Color(0f, 0.98f, 0.41f, 1f);
        internal static Sprite IrradiatedIcon;
        public static float timer2;
        public static ConfigEntry<float> IrradiatedRange;
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
            IrradiatedRange = Config.Bind<float>("Buff : Irradiated",
                             "Range",
                             0.4f,
                             "Control the range value");
            IrradiatedDamage = Config.Bind<float>("Buff : Irradiated",
                             "Damage",
                             0.1f,
                             "Control the damage value");
            IrradiatedTimer = Config.Bind<float>("Buff : Irradiated",
                 "Interval",
                 1f,
                 "Control the interval value in seconds");
            IrradiatedDuration = Config.Bind<float>("Buff : Irradiated",
                 "Duration",
                 5f,
                 "Control the duration value");
            IrradiatedMaxStack = Config.Bind<int>("Buff : Irradiated",
                 "Max stack",
                 5,
                 "Control the max stack value");
            ModSettingsManager.AddOption(new FloatFieldOption(IrradiatedRange));
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
            IrradiatedBuffDef.name = "ksfIrradiated";
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
                                InflictDotInfo dotInfo = new InflictDotInfo()
                                {
                                    attackerObject = body.gameObject,
                                    victimObject = characterBody.gameObject,
                                    totalDamage = body.maxHealth * IrradiatedDamage.Value,
                                    damageMultiplier = body.GetBuffCount(IrradiatedBuff.IrradiatedBuffDef),
                                    duration = IrradiatedDuration.Value,
                                    dotIndex = Buffs.IrradiatedBuff.IrradiatedDOTDef,
                                    maxStacksFromAttacker = (uint?)IrradiatedMaxStack.Value

                                };
                                StrengthenBurnUtils.CheckDotForUpgrade(body.inventory, ref dotInfo);
                                DotController.InflictDot(ref dotInfo);
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
