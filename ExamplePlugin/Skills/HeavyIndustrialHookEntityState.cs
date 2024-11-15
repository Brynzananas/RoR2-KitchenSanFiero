using EntityStates;
using RoR2.Projectile;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using R2API;
using UnityEngine.AddressableAssets;
using EntityStates.Loader;

namespace KitchenSanFiero.Skills
{
    public class HeavyIndustrialHookEntityState : BaseSkillState
    {
        // Token: 0x06000F9D RID: 3997 RVA: 0x00041C64 File Offset: 0x0003FE64
        public override void OnEnter()
        {
            base.OnEnter();
            if (base.isAuthority)
            {
                Ray aimRay = base.GetAimRay();
                TrajectoryAimAssist.ApplyTrajectoryAimAssist(ref aimRay, projectilePrefab, base.gameObject, 1f);
                FireProjectileInfo fireProjectileInfo = new FireProjectileInfo
                {
                    position = aimRay.origin,
                    rotation = Quaternion.LookRotation(aimRay.direction),
                    crit = base.characterBody.RollCrit(),
                    damage = this.damageStat * HeavyIndustrialHookEntityState.damageCoefficient,
                    force = 0f,
                    damageColorIndex = DamageColorIndex.Default,
                    procChainMask = default(ProcChainMask),
                    projectilePrefab = projectilePrefab,
                    owner = base.gameObject
                };
                ProjectileManager.instance.FireProjectile(fireProjectileInfo);
            }
            EffectManager.SimpleMuzzleFlash(HeavyIndustrialHookEntityState.muzzleflashEffectPrefab, base.gameObject, "HeavyIndustrialHookFlash", false);
            Util.PlaySound(HeavyIndustrialHookEntityState.fireSoundString, base.gameObject);
            //this.PlayAnimation("Grapple", HeavyIndustrialHookEntityState.FireHookIntroStateHash);
        }

        // Token: 0x06000F9E RID: 3998 RVA: 0x00041D6E File Offset: 0x0003FF6E
        public void SetHookReference(GameObject hook)
        {
            this.hookInstance = hook;
            this.hookStickOnImpact = hook.GetComponent<ProjectileStickOnImpact>();
            this.hadHookInstance = true;
        }

        // Token: 0x06000F9F RID: 3999 RVA: 0x00041D8C File Offset: 0x0003FF8C
        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (this.hookStickOnImpact)
            {
                if (this.hookStickOnImpact.stuck && !this.isStuck)
                {
                    //this.PlayAnimation("Grapple", HeavyIndustrialHookEntityState.FireHookLoopStateHash);
                }
                this.isStuck = this.hookStickOnImpact.stuck;
            }
            if (base.isAuthority && !this.hookInstance && this.hadHookInstance)
            {
                this.outer.SetNextStateToMain();
            }
        }

        // Token: 0x06000FA0 RID: 4000 RVA: 0x00041E0A File Offset: 0x0004000A
        public override void OnExit()
        {
            //this.PlayAnimation("Grapple", HeavyIndustrialHookEntityState.FireHookExitStateHash);
            EffectManager.SimpleMuzzleFlash(HeavyIndustrialHookEntityState.muzzleflashEffectPrefab, base.gameObject, "HeavyIndustrialHookFlash", false);
            base.OnExit();
        }

        // Token: 0x06000FA1 RID: 4001 RVA: 0x0001C63E File Offset: 0x0001A83E
        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Pain;
        }

        // Token: 0x04001314 RID: 4884
        [SerializeField]
        public static GameObject projectilePrefab = PrefabAPI.InstantiateClone(Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Loader/LoaderYankHook.prefab").WaitForCompletion(), "HeavyIndustrialHookProjectile");

        // Token: 0x04001315 RID: 4885
        public static float damageCoefficient;

        // Token: 0x04001316 RID: 4886
        public static GameObject muzzleflashEffectPrefab = PrefabAPI.InstantiateClone(Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Loader/MuzzleflashLoader.prefab").WaitForCompletion(), "HeavyIndustrialHookFlash");

        // Token: 0x04001317 RID: 4887
        public static string fireSoundString;

        // Token: 0x04001318 RID: 4888
        public GameObject hookInstance;

        // Token: 0x04001319 RID: 4889
        protected ProjectileStickOnImpact hookStickOnImpact;

        // Token: 0x0400131A RID: 4890
        private bool isStuck;

        // Token: 0x0400131B RID: 4891
        private bool hadHookInstance;

        // Token: 0x0400131C RID: 4892
        private uint soundID;

        // Token: 0x0400131D RID: 4893
        private static int FireHookIntroStateHash = Animator.StringToHash("FireHookIntro");

        // Token: 0x0400131E RID: 4894
        private static int FireHookLoopStateHash = Animator.StringToHash("FireHookLoop");

        // Token: 0x0400131F RID: 4895
        private static int FireHookExitStateHash = Animator.StringToHash("FireHookExit");
    }
}
