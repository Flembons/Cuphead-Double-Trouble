using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using UnityEngine;

namespace DoubleBosses.Boss_Changes
{
    internal class SaltbakerChanges
    {
        public static SaltbakerLevel saltbakerLevel;

        public void Init()
        {
            On.SaltbakerLevel.phase_two_to_three_cr += phase_two_to_three_cr;
            On.SaltbakerLevelSaltbaker.intro_cr += intro_cr;
            On.SaltbakerLevelSaltbaker.AniEvent_SpawnJumpers += AniEvent_SpawnJumpers;
            On.SaltbakerLevelSaltbaker.AniEvent_KillFires += AniEvent_KillFires;
            On.SaltbakerLevelSaltbaker.AniEvent_HandsClosed += AniEvent_HandsClosed;
            On.SaltbakerLevelPillarHandler.Update += Update;
        }
        private IEnumerator phase_two_to_three_cr(On.SaltbakerLevel.orig_phase_two_to_three_cr orig, SaltbakerLevel self)
        {
            SaltbakerLevel[] array = UnityEngine.Resources.FindObjectsOfTypeAll<SaltbakerLevel>();
            YieldInstruction wait = new WaitForFixedUpdate();

            // When there are multiple Saltbakers, only one of these white transitions would fade out,
            // leaving the screen completely white. self loop ensures every transition fades out and the fight can continue
            for (int i = 0; i < array.Length; i++)
            {
                if (array[i].transitionFader.gameObject.activeSelf)
                {
                    float t = 0f;
                    while (t < 1f)
                    {
                        t += CupheadTime.FixedDelta;
                        array[i].transitionFader.color = new Color(1f, 1f, 1f, Mathf.InverseLerp(1f, 0f, t));
                        yield return wait;
                    }
                }
            }
            self.saltbakerBouncer.gameObject.SetActive(true);
            self.saltbakerBouncer.StartBouncer(new Vector3(0f, 700f));
            self.transitionFader.gameObject.SetActive(false);
            yield break;
        }

        public IEnumerator intro_cr(On.SaltbakerLevelSaltbaker.orig_intro_cr orig, SaltbakerLevelSaltbaker self)
        {
            if (Level.Current as SaltbakerLevel)
            {
                saltbakerLevel = (SaltbakerLevel)Level.Current;
            }
            else
            {
                saltbakerLevel = UnityEngine.Object.FindObjectOfType<SaltbakerLevel>();
                Level.Current = saltbakerLevel;
            }

            yield return CupheadTime.WaitForSeconds(self, 1.5f);
            saltbakerLevel.SpawnSwoopers();
            yield return CupheadTime.WaitForSeconds(self, 1.5f);
            self.currentAttack = ((!self.animator.GetBool("IntroCubes")) ? SaltbakerLevelSaltbaker.State.Limes : SaltbakerLevelSaltbaker.State.Sugarcubes);
            self.prevAttack = self.currentAttack;
            self.AniEvent_StartProjectiles();
            self.attackCoroutines.Add(self.StartCoroutine(self.pattern_cr()));
            yield break;
        }

        public void AniEvent_SpawnJumpers(On.SaltbakerLevelSaltbaker.orig_AniEvent_SpawnJumpers orig, SaltbakerLevelSaltbaker self)
        {
            saltbakerLevel.SpawnJumpers();
        }

        public void AniEvent_KillFires(On.SaltbakerLevelSaltbaker.orig_AniEvent_KillFires orig, SaltbakerLevelSaltbaker self)
        {
            saltbakerLevel.KillFires();
        }

        public void AniEvent_HandsClosed(On.SaltbakerLevelSaltbaker.orig_AniEvent_HandsClosed orig, SaltbakerLevelSaltbaker self)
        {
            Level.Current = saltbakerLevel;
            self.ClearPhaseOneObjects();
            // change here
            saltbakerLevel.ClearFires();
            foreach (AbstractPlayerController abstractPlayerController in PlayerManager.GetAllPlayers())
            {
                LevelPlayerController levelPlayerController = (LevelPlayerController)abstractPlayerController;
                if (levelPlayerController != null)
                {
                    levelPlayerController.weaponManager.InterruptSuper();
                }
            }
            foreach (AbstractPlayerController abstractPlayerController2 in PlayerManager.GetAllPlayers())
            {
                LevelPlayerController levelPlayerController2 = (LevelPlayerController)abstractPlayerController2;
                if (levelPlayerController2 != null)
                {
                    levelPlayerController2.DisableInput();
                    levelPlayerController2.motor.ClearBufferedInput();
                    Level.Current.SetBounds(new int?(10780), new int?(-9220), new int?(446), new int?(370));
                    levelPlayerController2.transform.position = self.playerDefrostPositions[(int)levelPlayerController2.id].position + Vector3.left * 10000f;
                }
            }
            self.transitionCamera.SetActive(true);
            PlayerManager.SetPlayerCanJoin(PlayerId.PlayerTwo, false, false);
            LevelPauseGUI.OnUnpauseEvent += self.SuppressPlayerJoin;
            // Change here
            if (saltbakerLevel.playerLost)
            {
                self.animator.speed = 0f;
            }
            else
            {
                self.StartCoroutine(self.scroll_bg_cr());
            }
        }

        public IEnumerator scroll_bg_cr(On.SaltbakerLevelSaltbaker.orig_scroll_bg_cr orig, SaltbakerLevelSaltbaker self)
        {
            yield return CupheadTime.WaitForSeconds(self, self.transitionDelayAfterHandsClose);
            SaltbakerLevel level = saltbakerLevel;
            float t = 0f;
            YieldInstruction wait = new WaitForFixedUpdate();
            CupheadLevelCamera.Current.Shake(8f, self.transitionDuration, false);
            Vector3 shadowOffset = self.shadow.transform.position - self.table.transform.position;
            while (t < self.transitionDuration)
            {
                level.yScrollPos = EaseUtils.EaseInOut(EaseUtils.EaseType.easeInSine, EaseUtils.EaseType.easeOutBack, 0f, 1f, Mathf.InverseLerp(0f, self.transitionDuration, t));
                self.shadow.transform.position = shadowOffset + self.table.transform.position + Vector3.up * level.yScrollPos * 1500f;
                t += CupheadTime.FixedDelta;
                yield return wait;
            }
            level.yScrollPos = 1f;
            yield break;
        }

        public void Update(On.SaltbakerLevelPillarHandler.orig_Update orig, SaltbakerLevelPillarHandler self)
        {
            self.platforms.RemoveAll((GameObject g) => g == null);
            foreach (GameObject gameObject in self.platforms)
            {
                if (gameObject.transform.position.y < (float)Level.Current.Ground - 400f)
                {
                    UnityEngine.Object.Destroy(gameObject.gameObject);
                }
                else
                {
                    gameObject.transform.position += Vector3.down * self.GetPlatformFallSpeed() * CupheadTime.Delta;
                }
            }

            foreach (AbstractPlayerController abstractPlayerController in PlayerManager.GetAllPlayers())
            {
                // this cast in this for loop throws an error every frame if the player is a plane in Saltbaker's level
                // simply return is the player is a plane
                if (abstractPlayerController as PlanePlayerController)
                {
                    return;
                }

                LevelPlayerController levelPlayerController = (LevelPlayerController)abstractPlayerController;
                if (levelPlayerController)
                {
                    levelPlayerController.animationController.spriteRenderer.sortingOrder = ((levelPlayerController.motor.MoveDirection.y <= 0) ? 10 : 510);
                }
            }
        }

    }
}
