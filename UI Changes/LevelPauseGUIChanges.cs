using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace DoubleBosses
{
    public class LevelPauseGUIChanges
    {
        /*
         * This class adds a new menu option to the pause menu while on the overworld map. "Mod Options" will now appear
         * as an option in the pause menu where the mod can be configured. It also fixes an issue with King Dice's fight
         * where restarting from the pause menu would reload minibosses instead of the main King Dice fight.
         */
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

            // Set "Mod Options" to be available whenever the player is not in a boss fight
            if (!SceneLoader.SceneName.StartsWith("scene_level") || SceneLoader.CurrentLevel == Levels.House)
            {
                self.menuItems[2].gameObject.SetActive(true);
                self.updateRotateControlsToggleVisualValue();
            }
        }

        protected void Update(On.LevelPauseGUI.orig_Update orig, LevelPauseGUI self)
        {
            // Change the text for the menu options when the Pause Menu is on screen
            if (self.state != AbstractPauseGUI.State.Animating)
            {
                self.menuItems[2].text = "MOD OPTIONS";
                self.menuItems[1].text = "RETRY";
            }

            // R-Control will be replaced if Random bosses is selected
            if (SceneLoader.SceneName.Equals("scene_level_airplane") && DoubleBossesManager.db_bossSelectionType != 1)
            {
                self.menuItems[2].GetComponent<LocalizationHelper>().ApplyTranslation(Localization.Find("CameraRotationControl"), null);
                self.menuItems[2].text = string.Format(self.menuItems[2].text, (SettingsData.Data.rotateControlsWithCamera) ? "A" : "B");
            }
            // Replace R-Control with "RETRY WITH NEW BOSS"
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

            // Ensure the R-Control option still works when in the Howling Aces fight
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
                    // Menu option 2 will be "Mod Options" when outside of a boss fight
                    if (!SceneLoader.SceneName.StartsWith("scene_level") || SceneLoader.CurrentLevel == Levels.House)
                    {
                        DoubleBossesManager.doubleBossOptions = true;
                        self.Options();
                        return;
                    }
                    // Otherwise, option 2 will be "Retry with a New Boss"
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
                // This check ensures that restarting on a Dice Palace miniboss will reload King Dice and not the miniboss
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
