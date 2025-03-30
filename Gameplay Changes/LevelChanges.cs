using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace DoubleBosses
{
    public class LevelChanges
    {
        public void Init()
        {
            On.Level.CheckIntros += CheckIntros;
            On.Level._OnLevelEnd += _OnLevelEnd;
            On.Level.zHack_OnWin += zHack_OnWin;
        }

        protected void zHack_OnWin(On.Level.orig_zHack_OnWin orig, Level self)
        {
            if (self.players[0] == null)
            {
                Level[] levels = (Level[])UnityEngine.Resources.FindObjectsOfTypeAll(typeof(Level));
                self.players = levels[1].players;
            }
            orig(self);
        }

        private void CheckIntros(On.Level.orig_CheckIntros orig, Level self)
        {
            if (self.players[0] == null)
            {
                Level[] levels = (Level[])UnityEngine.Resources.FindObjectsOfTypeAll(typeof(Level));
                self.players = levels[1].players; 
            }
            LevelPlayerAnimationController component = self.players[0].GetComponent<LevelPlayerAnimationController>();
            if (component != null)
            {
                if (self.players[0].stats.Loadout.charm == Charm.charm_chalice)
                {
                    if (self.players[1] != null && self.players[1].stats.isChalice && self.CurrentLevel != Levels.Devil && self.CurrentLevel != Levels.Saltbaker && (!Level.IsDicePalace || DicePalaceMainLevelGameInfo.IS_FIRST_ENTRY))
                    {
                        component.CookieFail();
                    }
                    if (self.players[0].stats.isChalice && (self.CurrentLevel == Levels.Devil || self.CurrentLevel == Levels.Saltbaker))
                    {
                        component.ScaredChalice(self.CurrentLevel == Levels.Devil);
                    }
                }
                else if (self.CurrentLevel != Levels.Devil && self.CurrentLevel != Levels.Saltbaker)
                {
                    if (self.player1HeldJump && !self.player1HeldSuper)
                    {
                        component.IsIntroB();
                    }
                    else if (!self.player1HeldJump && !self.player1HeldSuper && Rand.Bool())
                    {
                        component.IsIntroB();
                    }
                }
            }
            if (self.players.Length >= 2 && self.players[1] != null)
            {
                LevelPlayerAnimationController component2 = self.players[1].GetComponent<LevelPlayerAnimationController>();
                if (component2 != null)
                {
                    if (self.players[1].stats.Loadout.charm == Charm.charm_chalice)
                    {
                        if (self.players[0].stats.isChalice && self.CurrentLevel != Levels.Devil && self.CurrentLevel != Levels.Saltbaker && (!Level.IsDicePalace || DicePalaceMainLevelGameInfo.IS_FIRST_ENTRY))
                        {
                            component2.CookieFail();
                        }
                        if (self.players[1].stats.isChalice && (self.CurrentLevel == Levels.Devil || self.CurrentLevel == Levels.Saltbaker))
                        {
                            component2.ScaredChalice(self.CurrentLevel == Levels.Devil);
                        }
                    }
                    else if (PlayerManager.Multiplayer && self.CurrentLevel != Levels.Devil && self.CurrentLevel != Levels.Saltbaker)
                    {
                        if (self.player2HeldJump && !self.player2HeldSuper)
                        {
                            component2.IsIntroB();
                        }
                        else if (!self.player2HeldJump && !self.player2HeldSuper && Rand.Bool())
                        {
                            component2.IsIntroB();
                        }
                    }
                }
            }
        }

        private void _OnLevelEnd(On.Level.orig__OnLevelEnd orig, Level self)
        {
            self.Ending = true;
            self.OnLevelEnd();
            // instead of checking PlayerMode, I can check if PlayerOne has a LevelPlayerController or PlanePlayerController component
            GameObject[] player = GameObject.FindGameObjectsWithTag("Player");

            if (player[0].GetComponent<LevelPlayerController>() != null)
            {
                LevelPlayerWeaponManager[] wepManagers = (LevelPlayerWeaponManager[])UnityEngine.Resources.FindObjectsOfTypeAll(typeof(LevelPlayerWeaponManager));
                wepManagers[0].OnLevelEnd();
                PlayerManager.SetPlayerCanJoin(PlayerId.PlayerTwo, false, false);
                PlayerManager.ClearJoinPrompt();
            }
            else if (self.playerMode == PlayerMode.Plane)
            {
                PlanePlayerWeaponManager[] planeManagers = (PlanePlayerWeaponManager[])UnityEngine.Resources.FindObjectsOfTypeAll(typeof(PlanePlayerWeaponManager));
                planeManagers[0].OnLevelEnd();
                PlayerManager.SetPlayerCanJoin(PlayerId.PlayerTwo, false, false);
                PlayerManager.ClearJoinPrompt();
            }
        }
    }
}
