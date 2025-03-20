using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace DoubleBosses
{
    public class LevelPauseGUIChanges
    {
        public void Init()
        {
            On.LevelPauseGUI.Init_bool_OptionsGUI_AchievementsGUI_RestartTowerConfirmGUI += Init;
            On.LevelPauseGUI.Update += Update;
            On.LevelPauseGUI.Select += Select;
            On.LevelPauseGUI.Restart += Restart;
        }

        public void Init(On.LevelPauseGUI.orig_Init_bool_OptionsGUI_AchievementsGUI_RestartTowerConfirmGUI orig, LevelPauseGUI self, bool checkIfDead, OptionsGUI options, AchievementsGUI achievements, RestartTowerConfirmGUI restartTowerConfirm)
        {
            orig(self, checkIfDead, options, achievements, restartTowerConfirm);
            if (!SceneLoader.SceneName.StartsWith("scene_level") || SceneLoader.CurrentLevel == Levels.House)
            {
                self.menuItems[2].gameObject.SetActive(true);
                self.updateRotateControlsToggleVisualValue();
            }
        }

        protected void Update(On.LevelPauseGUI.orig_Update orig, LevelPauseGUI self)
        {
            if (self.state != AbstractPauseGUI.State.Animating)
            {
                self.menuItems[2].text = "MOD OPTIONS";
                self.menuItems[1].text = "RETRY";
            }
            if (SceneLoader.SceneName.Equals("scene_level_airplane") && DoubleBossesManager.db_bossSelectionType != 1)
            {
                self.menuItems[2].GetComponent<LocalizationHelper>().ApplyTranslation(Localization.Find("CameraRotationControl"), null);
                self.menuItems[2].text = string.Format(self.menuItems[2].text, (SettingsData.Data.rotateControlsWithCamera) ? "A" : "B");
            }
            if (SceneLoader.SceneName.StartsWith("scene_level") && SceneLoader.CurrentLevel != Levels.House && DoubleBossesManager.db_bossSelectionType == 1)
            {
                self.menuItems[2].text = "RETRY WITH NEW BOSS";
                self.menuItems[2].gameObject.SetActive(true);
            }
            ((AbstractPauseGUI)self).UpdateInput();
            if (self.state != AbstractPauseGUI.State.Paused || self.options.optionMenuOpen || self.options.justClosed || (self.achievements != null && (self.achievements.achievementsMenuOpen || self.achievements.justClosed)) || (self.restartTowerConfirm != null && (self.restartTowerConfirm.restartTowerConfirmMenuOpen || self.restartTowerConfirm.justClosed)))
            {
                return;
            }
            if (self.GetButtonDown(CupheadButton.Pause) || self.GetButtonDown(CupheadButton.Cancel))
            {
                self.Unpause();
                return;
            }
            if (DoubleBossesManager.db_bossSelectionType != 1 && Level.Current != null && Level.Current.CurrentLevel == Levels.Airplane && self.selection == 2 && (self.GetButtonDown(CupheadButton.Accept) || self.GetButtonDown(CupheadButton.MenuLeft) || self.GetButtonDown(CupheadButton.MenuRight)))
            {
                self.MenuSelectSound();
                self.ToggleRotateControls();
                return;
            }
            if (self.GetButtonDown(CupheadButton.Accept))
            {
                self.MenuSelectSound();
                self.Select();
                return;
            }
            if (self._selectionTimer >= 0.15f)
            {
                if (self.GetButton(CupheadButton.MenuUp))
                {
                    self.MenuMoveSound();
                    int selection = self.selection;
                    self.selection = selection - 1;
                }
                if (self.GetButton(CupheadButton.MenuDown))
                {
                    self.MenuMoveSound();
                    int selection2 = self.selection;
                    self.selection = selection2 + 1;
                    return;
                }
            }
            else
            {
                self._selectionTimer += Time.deltaTime;
            }
        }

        private void Select(On.LevelPauseGUI.orig_Select orig, LevelPauseGUI self)
        {
            switch (self.selection)
            {
                case 0:
                    self.Unpause();
                    return;
                case 1:
                    DoubleBossesManager.db_retrying = true;
                    self.Restart();
                    return;
                case 2:
                    if (!SceneLoader.SceneName.StartsWith("scene_level") || SceneLoader.CurrentLevel == Levels.House)
                    {
                        DoubleBossesManager.doubleBossOptions = true;
                        self.Options();
                        return;
                    }
                    if (DoubleBossesManager.db_bossSelectionType == 1)
                    {
                        DoubleBossesManager.db_retryingNewBoss = true;
                        self.Restart();
                        return;
                    }
                    self.Options();
                    return;
                case 3:
                    self.Options();
                    return;
                case 4:
                    self.Player2Leave();
                    return;
                case 5:
                    self.Exit();
                    return;
                case 6:
                    self.ExitToTitle();
                    return;
                case 7:
                    self.ExitToDesktop();
                    return;
                default:
                    return;
            }
        }

        private void Restart(On.LevelPauseGUI.orig_Restart orig, LevelPauseGUI self)
        {
            if (Level.IsTowerOfPower)
            {
                self.RestartTowerConfirm();
            }
            else
            {
                if (Level.IsDicePalace && !Level.IsDicePalaceMain)
                {
                    DoubleBossesManager.db_retrying = false;
                }
                self.OnUnpauseSound();
                self.state = AbstractPauseGUI.State.Animating;
                PlayerManager.SetPlayerCanSwitch(PlayerId.PlayerOne, false);
                PlayerManager.SetPlayerCanSwitch(PlayerId.PlayerTwo, false);
                SceneLoader.ReloadLevel();
                Dialoguer.EndDialogue();
                if (Level.IsDicePalaceMain || Level.IsDicePalace)
                {
                    DicePalaceMainLevelGameInfo.CleanUpRetry();
                }
            }
        }
    }
}
