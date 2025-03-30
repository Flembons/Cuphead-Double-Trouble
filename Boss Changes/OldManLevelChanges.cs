using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace DoubleBosses.Boss_Changes
{
    internal class OldManLevelChanges
    {
        public static OldManLevel oldManLevel;

        public void Init()
        {
            On.OldManLevel.Start += Start;
            On.OldManLevel.phase_2_trans_cr += phase_2_trans_cr;
            On.OldManLevel.SetupStomach += SetupStomach;
            On.OldManLevel.scuba_gnomes_cr += scuba_gnomes_cr;
            On.OldManLevelSpitProjectile.Update += Update;
            On.OldManLevelTurretProjectile.spawn_sparkles_cr += spawn_sparkles_cr;
        }

        private IEnumerator phase_2_trans_cr(On.OldManLevel.orig_phase_2_trans_cr orig, OldManLevel self)
        {
            self.oldMan.EndPhase1();
            self.ClearFX(self.sparkleFXPool);
            self.ClearFX(self.smokeFXPool);
            yield return self.oldMan.animator.WaitForAnimationToStart(self, "Phase_Trans", false);
            self.oldMan.StopAllCoroutines();
            yield return null;
            foreach (OldManLevelSpikeFloor oldManLevelSpikeFloor in self.spikes)
            {
                oldManLevelSpikeFloor.Exit();
            }
            self.platformManager.EndPhase();
            while (self.oldMan.animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 0.684210539f)
            {
                yield return null;
            }

            Level[] levels = GameObject.FindObjectsOfType<Level>();
            for (int i = 0; i < levels.Length; i++)
            {
                levels[i].SetBounds(new int?(1249), new int?(331), null, null);
            }
            if (CupheadLevelCamera.Current.bounds == null)
            {
                CupheadLevelCamera.Current.bounds = new Level.Bounds(1002, 85, 360, 360);
            }

            GameObject[] array = GameObject.FindGameObjectsWithTag("MainCamera");
            array[array.Length - 1].GetComponent<CupheadLevelCamera>().ChangeHorizontalBounds(1002, 85);
            Vector3 cameraEndPos = new Vector3(-460f, 0f, 0f);
            self.StartCoroutine(array[array.Length - 1].GetComponent<CupheadLevelCamera>().slide_camera_cr(cameraEndPos, 3f));
            self.StartCoroutine(self.move_clouds_cr(3f));
            yield return CupheadTime.WaitForSeconds(self, 2f);
            self.oldMan.OnPhase2();
            yield return CupheadTime.WaitForSeconds(self, 2f);
            self.bleachers.SetActive(true);
            yield return null;
            yield break;
        }

        protected void Start(On.OldManLevel.orig_Start orig, OldManLevel self)
        {
            orig(self);
            if (Level.Current as OldManLevel)
            {
                oldManLevel = (OldManLevel)Level.Current;
            }
            else
            {
                oldManLevel = UnityEngine.Object.FindObjectOfType<OldManLevel>();
            }
        }
        private void SetupStomach(On.OldManLevel.orig_SetupStomach orig, OldManLevel self)
        {
            var stomachs = Resources.FindObjectsOfTypeAll<GameObject>().Where(obj => obj.name == "Stomach");
            foreach (var stomach in stomachs)
            {
                stomach.transform.GetChild(1).GetComponent<ParallaxLayer>().Start();
                stomach.transform.GetChild(3).GetComponent<ParallaxLayer>().Start();
                stomach.transform.GetChild(4).GetComponent<ParallaxLayer>().Start();
            }

            Level[] array = GameObject.FindObjectsOfType<Level>();

            for (int i = 0; i < array.Length; i++)
            {
                array[i].SetBounds(new int?(1249), new int?(331), null, null);
            }
            self.gnomeLeader.gameObject.SetActive(true);
            self.phaseTransitionTrigger.gameObject.SetActive(false);
            self.mountainBG.SetActive(false);
            self.stomachBG.SetActive(true);
            self.sockPuppet.FinishPuppet();
            AbstractPlayerController player = PlayerManager.GetPlayer(PlayerId.PlayerOne);
            if (!player.IsDead)
            {
                player.gameObject.SetActive(true);
                LevelPlayerMotor component = player.GetComponent<LevelPlayerMotor>();
                component.ClearBufferedInput();
                component.ForceLooking(new Trilean2(1, 1));
                player.GetComponent<LevelPlayerAnimationController>().ResetMoveX();
                component.OnRevive(self.gnomeLeader.platformPositions[1].position + Vector3.up * 1000f);
                component.CancelReviveBounce();
                component.EnableInput();
            }
            AbstractPlayerController player2 = PlayerManager.GetPlayer(PlayerId.PlayerTwo);
            if (player2 != null && !player2.IsDead)
            {
                player2.gameObject.SetActive(true);
                LevelPlayerMotor component2 = player2.GetComponent<LevelPlayerMotor>();
                component2 = player2.GetComponent<LevelPlayerMotor>();
                component2.ClearBufferedInput();
                component2.ForceLooking(new Trilean2(1, 1));
                player2.GetComponent<LevelPlayerAnimationController>().ResetMoveX();
                component2.OnRevive(self.gnomeLeader.platformPositions[3].position + Vector3.up * 1000f);
                component2.CancelReviveBounce();
                component2.EnableInput();
            }
            self.SFX_StomachLoop();
        }

        private IEnumerator scuba_gnomes_cr(On.OldManLevel.orig_scuba_gnomes_cr orig, OldManLevel self)
        {
            bool onLeft = Rand.Bool();
            LevelProperties.OldMan.ScubaGnomes p = self.properties.CurrentState.scubaGnomes;
            PatternString scubaTypeString = new PatternString(p.scubaTypeString, true, true);
            PatternString spawnDelayString = new PatternString(p.spawnDelayString, true, true);
            PatternString dartParryableString = new PatternString(p.dartParryableString, true);
            float offset = 50f;

            GameObject[] array = GameObject.FindGameObjectsWithTag("MainCamera");
            for (; ; )
            {
                float xPos = (!onLeft) ? (array[array.Length - 1].GetComponent<CupheadLevelCamera>().Bounds.xMax - offset) : (array[array.Length - 1].GetComponent<CupheadLevelCamera>().Bounds.xMin + offset);
                OldManLevelScubaGnome scubaGnome = self.scubaGnomePrefab.Spawn<OldManLevelScubaGnome>();
                scubaGnome.Init(new Vector3(xPos, array[array.Length - 1].GetComponent<CupheadLevelCamera>().Bounds.yMin), PlayerManager.GetNext(), scubaTypeString.PopLetter() == 'A', onLeft, dartParryableString.PopLetter() == 'P', p, self.gnomeLeader);
                yield return CupheadTime.WaitForSeconds(self, spawnDelayString.PopFloat());
                onLeft = !onLeft;
                yield return null;
            }
            yield break;
        }

        protected void Update(On.OldManLevelSpitProjectile.orig_Update orig, OldManLevelSpitProjectile self)
        {
            Vector3 position = self.transform.position;
            if (self.lifetime == 0f)
            {
                self.lastPosition = (self.startPosition = position);
            }
            if (self.DestroyDistance > 0f && Vector3.Distance(self.startPosition, position) >= self.DestroyDistance)
            {
                self.OnDieDistance();
            }
            self.distance += Vector3.Distance(self.lastPosition, position);
            self.lastPosition = position;
            if (self.DestroyLifetime > 0f && self.lifetime >= self.DestroyLifetime)
            {
                self.OnDieLifetime();
            }
            self.lifetime += Time.deltaTime;
            if (self.DestroyedAfterLeavingScreen)
            {
                bool flag = CupheadLevelCamera.Current.ContainsPoint(position, new Vector2(150f, self._setYPadding));
                if (self.hasBeenRendered && !flag)
                {
                    UnityEngine.Object.Destroy(self.gameObject);
                }
                if (!self.hasBeenRendered)
                {
                    self.hasBeenRendered = flag;
                }
            }
            self.smokeTimer -= CupheadTime.Delta;
            if (self.smokeTimer <= 0f)
            {
                self.smokeTimer += self.smokeDelay;
                oldManLevel.CreateFX(self.transform.position, false, self.CanParry);
            }
        }
        private IEnumerator spawn_sparkles_cr(On.OldManLevelTurretProjectile.orig_spawn_sparkles_cr orig, OldManLevelTurretProjectile self)
        {
            self.sparkleAngle = (float)UnityEngine.Random.Range(0, 360);
            for (; ; )
            {
                yield return CupheadTime.WaitForSeconds(self, self.sparkleSpawnDelay);
                Vector2 direction = MathUtils.AngleToDirection(self.sparkleAngle);
                Vector3 offset = new Vector3(direction.x, direction.y, 0) * self.sparkleDistanceRange.RandomFloat();
                oldManLevel.CreateFX(self.transform.position + offset, true, self.CanParry);
                self.sparkleAngle = (self.sparkleAngle + self.sparkleAngleShiftRange.RandomFloat()) % 360f;
                yield return null;
            }
            yield break;
        }
    }
}
