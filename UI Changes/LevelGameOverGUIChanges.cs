using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace DoubleBosses
{
    /*
     * This class alters the Game Over GUI that is seen when a player dies during a fight. The options in the menu will be 
     * changed depending on how the mod options are configured. This class also fixes an issue with the Retry button for King Dice's
     * fight. Restarting in a miniboss fight will restart the entire King Dice fight properly now.
     */
    public class LevelGameOverGUIChanges
    {
        public void Init()
        {
            On.LevelGameOverGUI.Select += Select;
            On.LevelGameOverGUI.Update += Update;
            On.LevelGameOverGUI.Retry += Retry;
        }

        private void Select(On.LevelGameOverGUI.orig_Select orig, LevelGameOverGUI self)
        {
            if (!Level.IsGraveyard)
            {
                AudioManager.SnapshotReset(SceneLoader.SceneName, 2f);
                AudioManager.ChangeBGMPitch(1f, 2f);
            }
            if (Level.Current != null && Level.Current.CurrentLevel == Levels.Airplane)
            {
                SettingsData.Save();
                if (PlatformHelper.IsConsole)
                {
                    SettingsData.SaveToCloud();
                }
            }
            int num = self.selection;

            switch (self.selection)
            {
                case 0:
                    // Tell the DBManager the player is restarting
                    DoubleBossesManager.db_retrying = true;
                    self.Retry();
                    AudioManager.Play("level_menu_card_down");
                    break;
                case 1:
                    self.ExitToMap();
                    AudioManager.Play("level_menu_card_down");
                    break;
                case 2:
                    // If bossSelectionType is set to Random, this option will instead load a new boss
                    if (DoubleBossesManager.db_bossSelectionType == 1)
                    {
                        DoubleBossesManager.db_retryingNewBoss = true;
                        self.Retry();
                        AudioManager.Play("level_menu_card_down");

                    }
                    // otherwise, this option will be Quit Game, which, you guessed it, quits the game
                    else
                    {
                        self.QuitGame();
                        AudioManager.Play("level_menu_card_down");
                    }
                    break;
            }
        }

        private void Update(On.LevelGameOverGUI.orig_Update orig, LevelGameOverGUI self)
        {
            if (self.state != LevelGameOverGUI.State.Ready)
            {
                return;
            }

            // Set the third menu option to be "RETRY WITH NEW BOSS" if Random bosses are on
            if (SceneLoader.SceneName.StartsWith("scene_level") && SceneLoader.CurrentLevel != Levels.House && DoubleBossesManager.db_bossSelectionType == 1)
            {
                self.menuItems[2].text = "RETRY WITH NEW BOSS";
                self.menuItems[2].gameObject.SetActive(true);
            }

            // Ignore the "R-Control" option if Random bosses are on
            if (DoubleBossesManager.db_bossSelectionType != 1 && self.selection == 2 && Level.Current != null && Level.Current.CurrentLevel == Levels.Airplane && (self.getButtonDown(CupheadButton.Accept) || self.getButtonDown(CupheadButton.MenuLeft) || self.getButtonDown(CupheadButton.MenuRight)))
            {
                AudioManager.Play("level_menu_card_down");
                self.toggleRotateControls();
                return;
            }
            int num = 0;
            if (self.getButtonDown(CupheadButton.Accept))
            {
                self.Select();
                AudioManager.Play("level_menu_select");
                self.state = LevelGameOverGUI.State.Exiting;
            }
            if (!Level.IsTowerOfPower && self.getButtonDown(CupheadButton.EquipMenu))
            {
                self.ChangeEquipment();
            }
            if (self.getButtonDown(CupheadButton.MenuDown))
            {
                AudioManager.Play("level_menu_move");
                num++;
            }
            if (self.getButtonDown(CupheadButton.MenuUp))
            {
                AudioManager.Play("level_menu_move");
                num--;
            }
            self.selection += num;
            self.selection = Mathf.Clamp(self.selection, 0, self.menuItems.Length - 1);
            if (!self.menuItems[self.selection].gameObject.activeSelf)
            {
                self.selection -= num;
                self.selection = Mathf.Clamp(self.selection, 0, self.menuItems.Length - 1);
            }
            self.UpdateSelection();
        }

        public void Retry(On.LevelGameOverGUI.orig_Retry orig, LevelGameOverGUI self)
        {
            // If the reload takes place in a Dice Palace miniboss, the miniboss would be reloaded instead of the main King Dice arena
            // this check ensures that King Dice's fight works like it does in the base game
            if (Level.IsDicePalace && !Level.IsDicePalaceMain)
            {
                DoubleBossesManager.db_retrying = false;
            }
            if (Level.IsDicePalaceMain || Level.IsDicePalace)
            {
                DicePalaceMainLevelGameInfo.CleanUpRetry();
            }

            SceneLoader.ReloadLevel();
        }
    }
}
