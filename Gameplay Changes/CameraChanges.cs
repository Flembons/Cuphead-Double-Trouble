using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace DoubleBosses
{
    /*
     * This class fixes the parallax layers that are used in certain fights. The parallax layers are based on the current camera object,
     * and when a second scene is spawned in, that camera object is set to the second scene's camera instead of the first scene's. The changes
     * here fix that issue and cause the parallax layers to work properly
     */

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

            // If this is called on the second camera in the scene, bounds will not be defined, so simply return
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
            // At the beginning of the Glumstone fight, set the parallax layer's camnera to the correct level camera
            // This needs to be done to fix the camera scrolling during the phase 2 transition
            if (SceneLoader.CurrentLevel == Levels.OldMan)
            {
                GameObject[] array = GameObject.FindGameObjectsWithTag("MainCamera");
                self._camera = array[array.Length - 1].GetComponent<CupheadLevelCamera>();
            }
            // otherwise, set the camera to the current camera as normal
            else
            {
                self._camera = CupheadLevelCamera.Current;
            }
            self._startPosition = self.transform.position;
            self._cameraStartPosition = self._camera.transform.position;
        }

        private void LateUpdate(On.ParallaxLayer.orig_LateUpdate orig, ParallaxLayer self)
        {
            // Return if this ParallaxLayer's camera object is not set properly
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
