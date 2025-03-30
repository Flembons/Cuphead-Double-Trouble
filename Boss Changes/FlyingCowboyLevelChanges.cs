using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DoubleBosses.Boss_Changes
{
    internal class FlyingCowboyLevelChanges
    {
        public static FlyingCowboyLevel flyingCowboyLevel;

        public void Init()
        {
            On.FlyingCowboyLevelCowboy.endVacuumPullPlayer += endVacuumPullPlayer;
            On.FlyingCowboyLevelCowboy.startVacuumPullPlayer += startVacuumPullPlayer;
        }

        public void startVacuumPullPlayer(On.FlyingCowboyLevelCowboy.orig_startVacuumPullPlayer orig, FlyingCowboyLevelCowboy self, bool immediateFullStrength)
        {
            self.endVacuumPullPlayer();
            LevelProperties.FlyingCowboy.Debris debris = self.properties.CurrentState.debris;
            foreach (AbstractPlayerController abstractPlayerController in PlayerManager.GetAllPlayers())
            {
                // If the player is on foot in this level, no need to do anything
                if (abstractPlayerController as LevelPlayerController)
                {
                    return;
                }

                PlanePlayerController planePlayerController = (PlanePlayerController)abstractPlayerController;
                if (!(planePlayerController == null))
                {
                    FlyingCowboyLevelCowboy.VacuumForce force = new FlyingCowboyLevelCowboy.VacuumForce(planePlayerController, self.vacuumDebrisDisappearTransform, debris.vacuumWindStrength * 0.5f, (!immediateFullStrength) ? debris.vacuumTimeToFullStrength : 0f);
                    planePlayerController.motor.AddForce(force);
                    if (planePlayerController.id == PlayerId.PlayerOne)
                    {
                        self.forcePlayer1 = force;
                    }
                    else if (planePlayerController.id == PlayerId.PlayerTwo)
                    {
                        self.forcePlayer2 = force;
                    }
                }
            }
        }

        public void endVacuumPullPlayer(On.FlyingCowboyLevelCowboy.orig_endVacuumPullPlayer orig, FlyingCowboyLevelCowboy self)
        {
            foreach (AbstractPlayerController abstractPlayerController in PlayerManager.GetAllPlayers())
            {
                // If the player is on foot and not a plane, simply return
                if (abstractPlayerController as LevelPlayerController)
                {
                    return;
                }
                PlanePlayerController planePlayerController = (PlanePlayerController)abstractPlayerController;
                if (!(planePlayerController == null) && !(planePlayerController.motor == null))
                {
                    planePlayerController.motor.RemoveForce(self.forcePlayer1);
                    planePlayerController.motor.RemoveForce(self.forcePlayer2);
                }
            }
            self.forcePlayer1 = null;
            self.forcePlayer2 = null;
        }
    }
}
