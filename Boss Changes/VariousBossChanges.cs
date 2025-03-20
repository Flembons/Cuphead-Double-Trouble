using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using UnityEngine;

namespace DoubleBosses
{
    public class VariousBossChanges
    {

        public static AirplaneLevel airplaneLevel;
        public static OldManLevel oldManLevel;
        public static SnowCultLevel snowCultLevel;

        public void Init()
        {
            On.SaltbakerLevel.phase_two_to_three_cr += phase_two_to_three_cr;

            On.OldManLevel.Start += Start;
            On.OldManLevel.phase_2_trans_cr += phase_2_trans_cr;
            On.OldManLevel.SetupStomach += SetupStomach;
            On.OldManLevel.scuba_gnomes_cr += scuba_gnomes_cr;
            On.OldManLevelSpitProjectile.Update += Update;
            On.OldManLevelTurretProjectile.spawn_sparkles_cr += spawn_sparkles_cr;

            On.SnowCultLevel.nextPattern_cr += nextPattern_cr;
            On.SnowCultLevel.to_phase_3_cr += to_phase_3_cr;

            On.FrogsLevelTall.Awake += Awake;
            On.FrogsLevelShort.Awake += Awake;
            On.FrogsLevelShort.StartMorph += StartMorph;
            On.FrogsLevelMorphed.Enable += Enable;

            On.AirplaneLevelPlayerPlane.Update += Update;
            On.AirplaneLevelBulldogPlane.intro_cr += intro_cr;
            On.AirplaneLevelTerrier.FixedUpdate += FixedUpdate;
            On.AirplaneLevelTerrier.setup_dogs_cr += setup_dogs_cr;
            On.AirplaneLevel.leader_cr += leader_cr;
            On.AirplaneLevelLeader.AniEvent_PawGrab += AniEvent_PawGrab;
            On.AirplaneLevelLeader.OnDamageTaken += OnDamageTaken;
        }

        private IEnumerator phase_two_to_three_cr(On.SaltbakerLevel.orig_phase_two_to_three_cr orig, SaltbakerLevel self)
        {
            SaltbakerLevel[] array = UnityEngine.Resources.FindObjectsOfTypeAll<SaltbakerLevel>();
            YieldInstruction wait = new WaitForFixedUpdate();
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

        private IEnumerator to_phase_3_cr(On.SnowCultLevel.orig_to_phase_3_cr orig, SnowCultLevel self)
        {
            self.yeti.ForceOutroToStart();
            while (self.yeti.state != SnowCultLevelYeti.States.Idle || self.yeti.inBallForm)
            {
                yield return null;
            }
            self.cultists.SetTrigger("Summon");
            self.yeti.OnDeath();
            self.jackFrost.Intro();
            yield return CupheadTime.WaitForSeconds(self, self.properties.CurrentState.yeti.timeToPlatforms);
            self.jackFrost.CreatePlatforms();
            self.StartCoroutine(self.SFX_SNOWCULT_IcePlatformAppear_cr());
            self.StartCoroutine(self.SFX_SNOWCULT_P2_to_P3_Transition_cr());
            for (int i = 0; i < 5; i++)
            {
                self.jackFrost.CreateAscendingPlatform(i);
                if (i < 4)
                {
                    yield return CupheadTime.WaitForSeconds(self, 0.2f);
                }
            }
            AbstractPlayerController player = PlayerManager.GetPlayer(PlayerId.PlayerOne);
            AbstractPlayerController player2 = PlayerManager.GetPlayer(PlayerId.PlayerTwo);
            LevelPlayerMotor p1Motor = player.GetComponent<LevelPlayerMotor>();
            LevelPlayerMotor p2Motor = null;
            bool hasStarted = false;
            while (!hasStarted)
            {
                if (player2 != null && !player2.IsDead)
                {
                    if (p2Motor == null)
                    {
                        p2Motor = player2.GetComponent<LevelPlayerMotor>();
                    }
                    if ((player.transform.position.y > -80f && p1Motor.Grounded) || (player2.transform.position.y > -80f && p2Motor.Grounded))
                    {
                        hasStarted = true;
                    }
                }
                else if (player.transform.position.y > -80f && p1Motor.Grounded)
                {
                    hasStarted = true;
                }
                yield return null;
            }

            GameObject[] array = GameObject.FindGameObjectsWithTag("MainCamera");
            Vector3 cameraEndPos = new Vector3(0f, 950f, 0f);
            float time = self.properties.CurrentState.yeti.timeForCameraMove;
            array[array.Length - 1].GetComponent<CupheadLevelCamera>().ChangeVerticalBounds(1290, 675);
            self.pit.SetActive(true);
            float cameraStartPos = array[array.Length - 1].GetComponent<CupheadLevelCamera>().transform.position.y;
            self.StartCoroutine(array[array.Length - 1].GetComponent<CupheadLevelCamera>().slide_camera_cr(cameraEndPos, time));
            time = 0f;
            time = 0f;
            while (time < 0.5f)
            {
                time = Mathf.InverseLerp(cameraStartPos, cameraEndPos.y, array[array.Length - 1].GetComponent<CupheadLevelCamera>().transform.position.y);
                yield return null;
            }
            Level.Current.SetBounds(new int?(640), new int?(640), new int?(1290), new int?(675));
            while (time < 0.75f)
            {
                time = Mathf.InverseLerp(cameraStartPos, cameraEndPos.y, array[array.Length - 1].GetComponent<CupheadLevelCamera>().transform.position.y);
                yield return null;
            }
            self.jackFrost.StartPhase3();
            self.pit.transform.parent = null;
            while (time < 0.95f)
            {
                time = Mathf.InverseLerp(cameraStartPos, cameraEndPos.y, array[array.Length - 1].GetComponent<CupheadLevelCamera>().transform.position.y);
                self.pit.transform.localPosition = array[array.Length - 1].GetComponent<CupheadLevelCamera>().transform.position + Vector3.down * 500f;
                yield return null;
            }
            self.pit.transform.localPosition = cameraEndPos + Vector3.down * 500f;
            yield break;
        }

        private IEnumerator nextPattern_cr(On.SnowCultLevel.orig_nextPattern_cr orig, SnowCultLevel self)
        {
            if (Level.Current as SnowCultLevel)
            {
            }
            else
            {
                Level.Current = UnityEngine.Resources.FindObjectsOfTypeAll<SnowCultLevel>()[0];
            }
            while (self.wizard != null && (self.wizard.Turning() || self.wizard.dead))
            {
                yield return null;
            }
            LevelProperties.SnowCult.Pattern p = self.properties.CurrentState.NextPattern;
            if (self.firstAttack)
            {
                while (p != LevelProperties.SnowCult.Pattern.Quad)
                {
                    p = self.properties.CurrentState.NextPattern;
                }
                self.firstAttack = false;
            }
            switch (p)
            {
                case LevelProperties.SnowCult.Pattern.Switch:
                    yield return self.StartCoroutine(self.switch_cr());
                    goto IL_2E6;
                case LevelProperties.SnowCult.Pattern.Eye:
                    yield return self.StartCoroutine(self.eye_attack_cr());
                    goto IL_2E6;
                case LevelProperties.SnowCult.Pattern.Shard:
                    yield return self.StartCoroutine(self.shard_attack_cr());
                    goto IL_2E6;
                case LevelProperties.SnowCult.Pattern.Mouth:
                    yield return self.StartCoroutine(self.mouth_shot_cr());
                    goto IL_2E6;
                case LevelProperties.SnowCult.Pattern.Quad:
                    yield return self.StartCoroutine(self.quad_cr());
                    goto IL_2E6;
                case LevelProperties.SnowCult.Pattern.Block:
                    yield return self.StartCoroutine(self.ice_block_cr());
                    goto IL_2E6;
                case LevelProperties.SnowCult.Pattern.SeriesShot:
                    yield return self.StartCoroutine(self.series_shot_cr());
                    goto IL_2E6;
                case LevelProperties.SnowCult.Pattern.Yeti:
                    goto IL_2E6;
            }
            yield return CupheadTime.WaitForSeconds(self, 1f);
        IL_2E6:
            yield break;
        }

        protected void Awake(On.FrogsLevelTall.orig_Awake orig, FrogsLevelTall self)
        {
            orig(self);
            var tallFrogs = Resources.FindObjectsOfTypeAll<GameObject>().Where(obj => obj.name == "Frog_Tall").ToArray();
            if (DoubleBossesManager.db_tallFrogCheck == 1)
            {
                tallFrogs[0].name = "Frog_Tall_0";
                tallFrogs[tallFrogs.Length - 1].name = "Frog_Tall_1";
                DoubleBossesManager.db_tallFrogCheck = 0;
            }
            else
            {
                DoubleBossesManager.db_tallFrogCheck++;
            }
        }

        protected void Awake(On.FrogsLevelShort.orig_Awake orig, FrogsLevelShort self)
        {
            orig(self);
            var shortFrogs = Resources.FindObjectsOfTypeAll<GameObject>().Where(obj => obj.name == "Frog_Short").ToArray();
            var morphedFrogs = Resources.FindObjectsOfTypeAll<GameObject>().Where(obj => obj.name == "Frog_Morphed").ToArray();
            if (DoubleBossesManager.db_shortFrogCheck == 1)
            {
                shortFrogs[0].name = "Frog_Short_0";
                shortFrogs[shortFrogs.Length -1].name = "Frog_Short_1";
                morphedFrogs[0].name = "Frog_Morphed_0";
                morphedFrogs[morphedFrogs.Length - 1].name = "Frog_Morphed_1";
                DoubleBossesManager.db_shortFrogCheck = 0;
            }
            else
            {
                DoubleBossesManager.db_shortFrogCheck++;
            }
        }

        public void StartMorph(On.FrogsLevelShort.orig_StartMorph orig, FrogsLevelShort self)
        {
            self.StopAllCoroutines();
            self.animator.Play("Idle");
            self.state = FrogsLevelShort.State.Morphing;

            // before rolling, make sure the current frog is the correct frog associated with self short frog
            var tallFrogs = Resources.FindObjectsOfTypeAll<GameObject>().Where(obj => obj.name.StartsWith("Frog_Tall")).ToArray();
            if (tallFrogs.Length > 1)
            {
                var morphedFrogs = Resources.FindObjectsOfTypeAll<GameObject>().Where(obj => obj.name.StartsWith("Frog_Morphed")).ToArray();
                if (self.name.Equals("Frog_Short_0"))
                {
                    FrogsLevelTall.Current = tallFrogs[0].GetComponent<FrogsLevelTall>();
                    FrogsLevelMorphed.Current = morphedFrogs[0].GetComponent<FrogsLevelMorphed>();
                }
                else
                {
                    FrogsLevelTall.Current = tallFrogs[1].GetComponent<FrogsLevelTall>();
                    FrogsLevelMorphed.Current = morphedFrogs[1].GetComponent<FrogsLevelMorphed>();
                }
            }
            self.StartCoroutine(self.morphRoll_cr());
        }

        public void Enable(On.FrogsLevelMorphed.orig_Enable orig, FrogsLevelMorphed self, bool demonTriggered)
        {
            FrogsLevel.FINAL_FORM = false;
            orig(self, demonTriggered);
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

