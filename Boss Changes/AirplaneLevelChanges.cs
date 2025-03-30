using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace DoubleBosses.Boss_Changes
{
    internal class AirplaneLevelChanges
    {
        public static AirplaneLevel airplaneLevel;

        public void Init()
        {
            On.AirplaneLevelPlayerPlane.Update += Update;
            On.AirplaneLevelBulldogPlane.intro_cr += intro_cr;
            On.AirplaneLevelTerrier.FixedUpdate += FixedUpdate;
            On.AirplaneLevelTerrier.setup_dogs_cr += setup_dogs_cr;
            On.AirplaneLevel.leader_cr += leader_cr;
            On.AirplaneLevelLeader.AniEvent_PawGrab += AniEvent_PawGrab;
            On.AirplaneLevelLeader.OnDamageTaken += OnDamageTaken;
        }

        private void Update(On.AirplaneLevelPlayerPlane.orig_Update orig, AirplaneLevelPlayerPlane self)
        {
            if (Level.Current as AirplaneLevel && ((AirplaneLevel)Level.Current).Rotating)
            {
                if (self.playerInSuper[0])
                {
                    self.restorePlayerPos[0] = true;
                }
                if (self.playerInSuper[1])
                {
                    self.restorePlayerPos[1] = true;
                }
            }
            if (self.player1 == null)
            {
                self.player1 = PlayerManager.GetPlayer(PlayerId.PlayerOne);
                if (self.player1 != null)
                {
                    LevelPlayerWeaponManager component = self.player1.gameObject.GetComponent<LevelPlayerWeaponManager>();
                    component.OnSuperStart += self.StartP1Super;
                    component.OnSuperEnd += self.EndP1Super;
                    component.OnExStart += self.StartP1Super;
                    component.OnExEnd += self.EndP1Super;
                }
            }
            if (self.player2 == null)
            {
                self.player2 = PlayerManager.GetPlayer(PlayerId.PlayerTwo);
                if (self.player2 != null)
                {
                    LevelPlayerWeaponManager component2 = self.player2.gameObject.GetComponent<LevelPlayerWeaponManager>();
                    component2.OnSuperStart += self.StartP2Super;
                    component2.OnSuperEnd += self.EndP2Super;
                    component2.OnExStart += self.StartP2Super;
                    component2.OnExEnd += self.EndP2Super;
                }
            }
            if (self.player1 != null)
            {
                self.p1IsColliding = (self.player1.transform.parent == self.airplane1.transform);
                self.player1.transform.SetEulerAngles(null, null, new float?(0f));
            }
            else
            {
                self.p1IsColliding = false;
            }
            if (self.player2 != null)
            {
                self.p2IsColliding = (self.player2.transform.parent == self.airplane1.transform);
                self.player2.transform.SetEulerAngles(null, null, new float?(0f));
            }
            else
            {
                self.p2IsColliding = false;
            }
            self.autoTiltTime = Mathf.Clamp(self.autoTiltTime + CupheadTime.Delta * ((!self.autoX || !self.autoTilt) ? -1f : 3f), 0f, 1f);
            for (int i = 0; i < self.puffTimer.Length; i++)
            {
                self.puffTimer[i] -= CupheadTime.Delta;
                if (self.puffTimer[i] <= 0f)
                {
                    self.puffTimer[i] += ((i != 0) ? 0.8f : 1f);
                    Effect effect = self.planePuffFX.Create(self.planePuffPos[i].position);
                    effect.transform.SetEulerAngles(null, null, new float?((float)((i != 0) ? 30 : -30)));
                }
            }
        }

        private IEnumerator intro_cr(On.AirplaneLevelBulldogPlane.orig_intro_cr orig, AirplaneLevelBulldogPlane self)
        {
            self.leaderIntroBG.SetTrigger("Continue");
            YieldInstruction wait = new WaitForFixedUpdate();
            yield return self.bullDogPlane.WaitForAnimationToStart(self, "Intro", false);
            int target = Animator.StringToHash(self.bullDogPlane.GetLayerName(0) + ".Intro");

            if (Level.Current as AirplaneLevel)
            {
                airplaneLevel = (AirplaneLevel)Level.Current;
            }
            else
            {
                airplaneLevel = UnityEngine.Object.FindObjectOfType<AirplaneLevel>();
            }

            while (self.bullDogPlane.GetCurrentAnimatorStateInfo(0).fullPathHash == target)
            {
                float s = self.bullDogPlane.GetCurrentAnimatorStateInfo(0).normalizedTime;
                if (s > 0.7f && s < 0.95f)
                {
                    airplaneLevel.UpdateShadow(1f - Mathf.Sin(Mathf.InverseLerp(0.7f, 0.95f, s) * 3.14159274f) * 0.2f);
                }
                else
                {
                    airplaneLevel.UpdateShadow(1f);
                }
                yield return wait;
            }
            airplaneLevel.UpdateShadow(1f);
            yield return CupheadTime.WaitForSeconds(self, 0.35f);
            self.SFX_DOGFIGHT_BulldogPlane_Loop();
            self.SFX_DOGFIGHT_Intro_BulldogPlaneDecend();
            self.StartCoroutine(self.turret_cr());
            self.StartCoroutine(self.mainattack_cr());
            float t = 0f;
            float time = 0.8f;
            float endTime = 0.4f;
            float start = self.transform.position.y;
            self.StartCoroutine(self.scale_in_cr());
            while (t < time)
            {
                t += CupheadTime.FixedDelta;
                float val = EaseUtils.Ease(EaseUtils.EaseType.easeOutSine, 0f, 1f, t / time);
                self.transform.SetPosition(null, new float?(Mathf.Lerp(start, 156f, val)), null);
                yield return wait;
            }
            t = 0f;
            start = self.transform.position.y;
            while (t < endTime)
            {
                t += CupheadTime.FixedDelta;
                float val2 = EaseUtils.Ease(EaseUtils.EaseType.easeOutSine, 0f, 1f, t / endTime);
                self.transform.SetPosition(null, new float?(Mathf.Lerp(start, 256f, val2)), null);
                yield return wait;
            }
            self.transform.SetPosition(null, new float?(256f), null);
            if (self.state == AirplaneLevelBulldogPlane.State.Intro)
            {
                self.state = AirplaneLevelBulldogPlane.State.Main;
            }
            self.StartCoroutine(self.move_cr());
            yield return null;
            yield break;
        }

        private void FixedUpdate(On.AirplaneLevelTerrier.orig_FixedUpdate orig, AirplaneLevelTerrier self)
        {
            if (!self.IsDead)
            {
                self.smokeTimer += CupheadTime.FixedDelta * ((!self.introFinished) ? 0.2f : ((!self.gettingEaten) ? 1f : 0.1f));
                if (self.smokeTimer > self.smokeDelay)
                {
                    self.smokeTimer -= self.smokeDelay;
                    airplaneLevel.CreateSmokeFX(self.flame.transform.position, (!self.introFinished) ? (MathUtils.AngleToDirection(self.flame.transform.eulerAngles.z - 90f) * 300f) : Vector2.zero, self.hp < self.smokingThreshold, self.rends[self.currentAngle].sortingLayerID, (self.currentAngle > 4) ? 30 : -1);
                }
            }
        }

        private IEnumerator setup_dogs_cr(On.AirplaneLevelTerrier.orig_setup_dogs_cr orig, AirplaneLevelTerrier self)
        {
            self.rotationOffset = Vector3.zero;
            YieldInstruction wait = new WaitForFixedUpdate();
            int indexToPlay = self.index;
            self.transform.SetScale(new float?((!self.isClockwise) ? Mathf.Abs(self.transform.localScale.x) : (-Mathf.Abs(self.transform.localScale.x))), null, null);
            int num = self.index;
            if (num != 1)
            {
                if (num == 3)
                {
                    indexToPlay = ((!self.isClockwise) ? self.index : (indexToPlay = 1));
                }
            }
            else
            {
                indexToPlay = ((!self.isClockwise) ? self.index : (indexToPlay = 3));
            }
            self.animator.Play("Intro_" + indexToPlay);
            int flamePos = indexToPlay * 4;
            if (indexToPlay == 3)
            {
                flamePos = 4;
            }
            self.flame.transform.localPosition = self.flameOffset[flamePos];
            if (indexToPlay == 1)
            {
                self.flame.transform.localPosition = new Vector3(-self.flame.transform.localPosition.x, self.flame.transform.localPosition.y);
            }
            self.angle *= 0.0174532924f;
            self.loopSizeX = 675f;
            self.loopSizeY = 328.5f;
            self.rotationOffset.x = Mathf.Sin(self.angle) * self.loopSizeX;
            self.rotationOffset.y = Mathf.Cos(self.angle) * self.loopSizeY;
            Vector3 startPos = self.pivotPoint.position + new Vector3(self.pivotOffset.x, self.pivotOffset.y, 0) + new Vector3(self.rotationOffset.x, self.rotationOffset.y, 0) * 2f;
            Vector3 endPos = self.pivotPoint.position + new Vector3(self.pivotOffset.x, self.pivotOffset.y, 0) + new Vector3(self.rotationOffset.x, self.rotationOffset.y, 0);
            float t = 0f;
            float time = 0.5f;
            self.transform.position = startPos;
            while (t < time)
            {
                t += CupheadTime.FixedDelta;
                self.transform.position = Vector3.Lerp(startPos, endPos, t / time) + self.WobblePos();
                yield return wait;
            }
            self.animator.SetTrigger("ContinueIntro");
            t = 0f;
            while (t < 0.9f)
            {
                t += CupheadTime.FixedDelta;
                self.transform.position = endPos + self.WobblePos();
                self.wobbleModifier = Mathf.Lerp(1f, 0f, t / 0.9f);
                yield return wait;
            }
            self.wobbleModifier = 0f;
            self.animator.SetTrigger("EndIntro");
            self.rotationSpeed = 0f;
            self.ReadyToMove = true;
            yield return self.animator.WaitForAnimationToStart(self, "Idle", false);
            self.introFinished = true;
            airplaneLevel.terriersIntroFinished = true;
            yield break;
        }

        private IEnumerator leader_cr(On.AirplaneLevel.orig_leader_cr orig, AirplaneLevel self)
        {
            LevelProperties.Airplane.Plane p = self.properties.CurrentState.plane;
            if (self.terriers == null)
            {
                while (self.bulldogPlane.state != AirplaneLevelBulldogPlane.State.Main)
                {
                    yield return null;
                }
                self.bulldogPlane.BulldogDeath();
            }
            if (!self.secretPhaseActivated)
            {
                yield return CupheadTime.WaitForSeconds(self, 0.5f);
            }
            self.leader.gameObject.SetActive(true);
            self.leader.StartLeader();
            if (self.secretPhaseActivated)
            {
                self.leader.animator.Play("Intro", 0, 0.54f);
                yield return null;
            }
            else
            {
                self.airplane.AutoMoveToPos(new Vector3(0f, p.moveDownPhThree), true, true);
                self.canteenAnimator.ForceLook(new Vector3(0f, p.moveDownPhThree), 3);
                self.airplane.SetXRange(-225f, 225f);
                yield return CupheadTime.WaitForSeconds(self, 1f);
            }
            int target = Animator.StringToHash(self.leader.animator.GetLayerName(0) + ".Intro");
            while (self.leader.animator.GetCurrentAnimatorStateInfo(0).fullPathHash == target)
            {
                if (self.leader.animator.GetCurrentAnimatorStateInfo(0).normalizedTime > 0.75f)
                {
                    airplaneLevel.UpdateShadow(1f - Mathf.InverseLerp(0.75f, 1f, self.leader.animator.GetCurrentAnimatorStateInfo(0).normalizedTime) * 0.1f);
                }
                yield return null;
            }
            if (!self.secretPhaseActivated)
            {
                self.StartCoroutine(self.rotate_camera());
            }
            else
            {
                self.StartCoroutine(self.secret_phase_cr());
            }
            yield break;
        }

        private void AniEvent_PawGrab(On.AirplaneLevelLeader.orig_AniEvent_PawGrab orig, AirplaneLevelLeader self)
        {
            CupheadLevelCamera.Current.Shake(30f, 0.8f, false);
            airplaneLevel.MoveBoundsIn();
            airplaneLevel.BlurBGCamera();
        }

        private void OnDamageTaken(On.AirplaneLevelLeader.orig_OnDamageTaken orig, AirplaneLevelLeader self, DamageDealer.DamageInfo info)
        {
            if (airplaneLevel.Rotating)
            {
                return;
            }
            self.properties.DealDamage(info.damage);
            if (self.properties.CurrentHealth <= 0f && !self.isDead)
            {
                self.StopAllCoroutines();
                self.StartCoroutine(self.death_cr());
            }
        }
    }
}
