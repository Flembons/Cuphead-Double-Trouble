using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace DoubleBosses
{
    public class CameraChanges
    {
        public void Init()
        {
            On.CupheadLevelCamera.Update += Update;
            On.ParallaxLayer.Start += Start;
            On.ParallaxLayer.LateUpdate += LateUpdate;
        }

        private void Update(On.CupheadLevelCamera.orig_Update orig, CupheadLevelCamera self)
        {
            if (PlayerManager.Count <= 0)
            {
                return;
            }
            if (self.bounds == null)
            {
                return;
            }
            self.UpdateBounds();
            Vector3 position = self._position;
            CupheadLevelCamera.Mode mode = self.mode;
            switch (mode)
            {
                case CupheadLevelCamera.Mode.Lerp:
                    break;
                case CupheadLevelCamera.Mode.TrapBox:
                    self.UpdateModeTrapBox();
                    goto IL_A4;
                case CupheadLevelCamera.Mode.Relative:
                    self.UpdateModeRelative();
                    goto IL_A4;
                case CupheadLevelCamera.Mode.Platforming:
                    self.UpdatePlatforming();
                    goto IL_A4;
                case CupheadLevelCamera.Mode.Path:
                    self.UpdatePath();
                    goto IL_A4;
                case CupheadLevelCamera.Mode.RelativeRook:
                    self.UpdateModeRelativeRook();
                    goto IL_A4;
                case CupheadLevelCamera.Mode.RelativeRumRunners:
                    self.UpdateModeRelativeRumRunners();
                    goto IL_A4;
                default:
                    if (mode == CupheadLevelCamera.Mode.Static)
                    {
                        goto IL_A4;
                    }
                    break;
            }
            self.UpdateModeLerp();
        IL_A4:
            Vector3 position2 = self._position;
            if (self.Width * 2f > (float)self.bounds.Width)
            {
                position2.x = Mathf.Lerp(position.x, 0f, CupheadTime.Delta * 10f);
            }
            if (self.Height * 2f > (float)self.bounds.Height)
            {
                position2.y = Mathf.Lerp(position.y, 0f, CupheadTime.Delta * 10f);
            }
            self._position = position2;
            self.Move();
        }

        protected virtual void Start(On.ParallaxLayer.orig_Start orig, ParallaxLayer self)
        {
            if (SceneLoader.CurrentLevel == Levels.OldMan)
            {
                GameObject[] array = GameObject.FindGameObjectsWithTag("MainCamera");
                self._camera = array[array.Length - 1].GetComponent<CupheadLevelCamera>();
            }
            else
            {
                self._camera = CupheadLevelCamera.Current;
            }
            self._startPosition = self.transform.position;
            self._cameraStartPosition = self._camera.transform.position;
        }
        private void LateUpdate(On.ParallaxLayer.orig_LateUpdate orig, ParallaxLayer self)
        {
            if (self._camera == null)
            {
                return;
            }
            switch (self.type)
            {
                case ParallaxLayer.Type.MinMax:
                    self.UpdateMinMax();
                    break;
                default:
                    self.UpdateComparative();
                    break;
                case ParallaxLayer.Type.Centered:
                    self.UpdateCentered();
                    break;
            }
        }
    }
}
