using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using UnityEngine;

namespace DoubleBosses
{
    public class RumRunnersChanges
    {
        public static RumRunnersLevel rumRunnersLevel;

        public void Init()
        {
            On.RumRunnersLevel.Start += Start;
            On.RumRunnersLevelAnteater.animationEvent_LeftDirt += animationEvent_LeftDirt;
            On.RumRunnersLevelAnteater.animationEvent_RightDirt += animationEvent_RightDirt;
            On.RumRunnersLevelAnteater.animationEvent_MiddleBridgeDestroy += animationEvent_MiddleBridgeDestroy;
            On.RumRunnersLevelAnteater.animationEvent_UpperBridgeDestroy += animationEvent_UpperBridgeDestroy;
            On.RumRunnersLevelAnteater.animationEvent_BridgeShatter += animationEvent_BridgeShatter;
            On.RumRunnersLevelBarrel.LevelInit += LevelInit;
            On.RumRunnersLevelBarrel.Die += Die;
        }

        protected void Start(On.RumRunnersLevel.orig_Start orig, RumRunnersLevel self)
        {
            rumRunnersLevel = self;
            orig(self);
        }

        private void animationEvent_LeftDirt(On.RumRunnersLevelAnteater.orig_animationEvent_LeftDirt orig, RumRunnersLevelAnteater self)
        {
            CupheadLevelCamera.Current.Shake(20f, 0.3f, true);
            rumRunnersLevel.FullscreenDirt(1, new float?(-640f + UnityEngine.Random.Range(100f, 200f)), -1f, -1f);
        }

        private void animationEvent_RightDirt(On.RumRunnersLevelAnteater.orig_animationEvent_RightDirt orig, RumRunnersLevelAnteater self)
        {
            CupheadLevelCamera.Current.Shake(20f, 0.3f, true);
            rumRunnersLevel.FullscreenDirt(1, new float?(640f - UnityEngine.Random.Range(100f, 200f)), -1f, -1f);
        }

        private void animationEvent_MiddleBridgeDestroy(On.RumRunnersLevelAnteater.orig_animationEvent_MiddleBridgeDestroy orig, RumRunnersLevelAnteater self)
        {
            rumRunnersLevel.DestroyMiddleBridge();
        }

        private void animationEvent_UpperBridgeDestroy(On.RumRunnersLevelAnteater.orig_animationEvent_UpperBridgeDestroy orig, RumRunnersLevelAnteater self)
        {
            rumRunnersLevel.DestroyUpperBridge();
        }

        private void animationEvent_BridgeShatter(On.RumRunnersLevelAnteater.orig_animationEvent_BridgeShatter orig, RumRunnersLevelAnteater self)
        {
            rumRunnersLevel.ShatterBridges();
        }

        public void LevelInit(On.RumRunnersLevelBarrel.orig_LevelInit orig, RumRunnersLevelBarrel self, LevelProperties.RumRunners properties)
        {
            self.properties = properties;
            rumRunnersLevel.OnUpperBridgeDestroy += self.onUpperBridgeDestroy;
        }

        public void Die(On.RumRunnersLevelBarrel.orig_Die orig, RumRunnersLevelBarrel self, bool immediate, bool spawnShrapnel = true)
        {
            rumRunnersLevel.OnUpperBridgeDestroy -= self.onUpperBridgeDestroy;
            self.StopAllCoroutines();
            if (immediate)
            {
                UnityEngine.Object.Destroy(self.gameObject);
                return;
            }
            if (self.isCop)
            {
                self.StartCoroutine(self.copDeath_cr());
            }
            else
            {
                if (self.transform.position.x * self.facingDirection < 960f)
                {
                    Effect effect = self.deathPoof.Create(self.transform.position);
                    if (!spawnShrapnel)
                    {
                        effect.GetComponent<Animator>().Play("Poof", 0, 0.0833333358f);
                    }
                    self.SFX_RUMRUN_BarrelExplode();
                    if (spawnShrapnel)
                    {
                        float num = UnityEngine.Random.Range(0f, 6.28318548f);
                        for (int i = 0; i < 2; i++)
                        {
                            for (int j = 0; j < 4; j++)
                            {
                                float f = num + 6.28318548f * (float)j / 4f;
                                Vector3 b = new Vector3(Mathf.Cos(f) * 50f, Mathf.Sin(f) * 50f);
                                Effect effect2 = self.deathShrapnel.Create(self.transform.position + b);
                                effect2.animator.SetInteger("Effect", j);
                                effect2.animator.SetBool("Parry", self._canParry);
                                if (i > 0)
                                {
                                    SpriteRenderer component = effect2.GetComponent<SpriteRenderer>();
                                    component.sortingLayerName = "Background";
                                    component.sortingOrder = 95;
                                    component.color = new Color(0.7f, 0.7f, 0.7f, 1f);
                                    effect2.transform.SetScale(new float?(0.75f), new float?(0.75f), null);
                                }
                                SpriteDeathParts component2 = effect2.GetComponent<SpriteDeathParts>();
                                if (b.x > 0f)
                                {
                                    component2.SetVelocityX(0f, component2.VelocityXMax);
                                }
                                else
                                {
                                    component2.SetVelocityX(component2.VelocityXMin, 0f);
                                }
                            }
                        }
                    }
                }
                if (!spawnShrapnel)
                {
                    self.GetComponent<Collider2D>().enabled = false;
                    self.runSpeed = 0f;
                    self.StartCoroutine(self.destroy_with_delay_cr());
                }
                else
                {
                    UnityEngine.Object.Destroy(self.gameObject);
                }
            }
        }
    }
}
