using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace DoubleBosses
{
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
                    DoubleBossesManager.db_retrying = true;
                    self.Retry();
                    AudioManager.Play("level_menu_card_down");
                    break;
                case 1:
                    self.ExitToMap();
                    AudioManager.Play("level_menu_card_down");
                    break;
                case 2:
                    if (DoubleBossesManager.db_bossSelectionType == 1)
                    {
                        DoubleBossesManager.db_retryingNewBoss = true;
                        self.Retry();
                        AudioManager.Play("level_menu_card_down");

                    }
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
            if (SceneLoader.SceneName.StartsWith("scene_level") && SceneLoader.CurrentLevel != Levels.House && DoubleBossesManager.db_bossSelectionType == 1)
            {
                self.menuItems[2].text = "RETRY WITH NEW BOSS";
                self.menuItems[2].gameObject.SetActive(true);
            }
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
