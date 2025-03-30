using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace DoubleBosses.Boss_Changes
{
    internal class SnowCultChanges
    {
        public static SnowCultLevel snowCultLevel;

        public void Init()
        {
            On.SnowCultLevel.nextPattern_cr += nextPattern_cr;
            On.SnowCultLevel.to_phase_3_cr += to_phase_3_cr;
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
    }
}
