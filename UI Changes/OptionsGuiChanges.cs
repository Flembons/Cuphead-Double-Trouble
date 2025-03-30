using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using On;
using System.Net.NetworkInformation;

namespace DoubleBosses
{
    public class OptionsGuiChanges
    {
        public void Init()
        {
            On.OptionsGUI.ToggleSubMenu += ToggleSubMenu;
            On.OptionsGUI.Update += Update;
            On.OptionsGUI.AudioSelect += AudioSelect;
            On.OptionsGUI.AudioHorizontalSelect += AudioHorizontalSelect;
            On.OptionsGUI.ShowMainOptionMenu += ShowMainOptionMenu;
        }

        private void ToggleSubMenu(On.OptionsGUI.orig_ToggleSubMenu orig, OptionsGUI self, OptionsGUI.State state)
        {
            self.currentItems.Clear();
            switch (state)
            {
                case OptionsGUI.State.MainOptions:
                    self.mainObject.SetActive(true);
                    self.visualObject.SetActive(false);
                    self.audioObject.SetActive(false);
                    self.languageObject.SetActive(false);
                    self.bigCard.SetActive(false);
                    self.bigNoise.SetActive(false);
                    self.currentItems.AddRange(self.mainObjectButtons);
                    break;
                case OptionsGUI.State.Visual:
                    self.mainObject.SetActive(false);
                    self.visualObject.SetActive(true);
                    self.audioObject.SetActive(false);
                    self.bigCard.SetActive(true);
                    self.bigNoise.SetActive(true);
                    self.currentItems.AddRange(self.visualObjectButtons);
                    break;
                case OptionsGUI.State.Audio:
                    self.mainObject.SetActive(false);
                    self.visualObject.SetActive(false);
                    self.audioObject.SetActive(true);
                    self.languageObject.SetActive(false);
                    self.bigCard.SetActive(true);
                    self.bigNoise.SetActive(true);
                    if (DoubleBossesManager.doubleBossOptions)
                    {
                        GameObject audioMenu = GameObject.Find("AudioMenu");
                        GameObject.Find("BigCard").transform.localScale = new Vector3(1.15f, 1, 1);
                        self.audioObjectButtons[0].options = DoubleBossesManager.bossSelectionOptions;
                        self.audioObjectButtons[0].updateSelection(DoubleBossesManager.db_bossSelectionType);
                        self.audioObjectButtons[0].wrap = true;
                        audioMenu.transform.GetChild(1).gameObject.transform.GetChild(0).gameObject.GetComponent<Text>().text = "BOSS TYPE:";
                        self.audioObjectButtons[1].options = DoubleBossesManager.bossChoice;
                        self.audioObjectButtons[1].updateSelection(DoubleBossesManager.db_bossSelection);
                        self.audioObjectButtons[1].wrap = true;
                        audioMenu.transform.GetChild(0).gameObject.transform.GetChild(1).gameObject.SetActive(false);
                        audioMenu.transform.GetChild(1).gameObject.transform.GetChild(1).gameObject.SetActive(false);
                        if (self.audioObjectButtons[0].selection == 2)
                        {
                            audioMenu.transform.GetChild(0).gameObject.transform.GetChild(1).gameObject.SetActive(true);
                            audioMenu.transform.GetChild(1).gameObject.transform.GetChild(1).gameObject.SetActive(true);
                        }
                        audioMenu.transform.GetChild(1).gameObject.transform.GetChild(1).gameObject.GetComponent<Text>().text = "SELECTED BOSS:";
                        self.audioObjectButtons[2].options = DoubleBossesManager.bossStartingHealthOption;
                        self.audioObjectButtons[2].updateSelection(DoubleBossesManager.db_startingHealth);
                        self.audioObjectButtons[2].wrap = true;
                        audioMenu.transform.GetChild(1).gameObject.transform.GetChild(2).gameObject.GetComponent<Text>().text = "BASE HEALTH:";
                        audioMenu.transform.GetChild(0).gameObject.transform.GetChild(3).gameObject.SetActive(false);
                        audioMenu.transform.GetChild(1).gameObject.transform.GetChild(3).gameObject.SetActive(false);
                    }
                    self.currentItems.AddRange(self.audioObjectButtons);
                    break;
                case OptionsGUI.State.Controls:
                    self.mainObject.SetActive(false);
                    self.visualObject.SetActive(false);
                    self.audioObject.SetActive(false);
                    self.languageObject.SetActive(false);
                    self.ShowControlMapper();
                    break;
                case OptionsGUI.State.Language:
                    self.languageObjectButtons[0].updateSelection((int)Localization.language);
                    self.mainObject.SetActive(false);
                    self.audioObject.SetActive(false);
                    self.languageObject.SetActive(true);
                    self.bigCard.SetActive(false);
                    self.bigNoise.SetActive(false);
                    self.currentItems.AddRange(self.languageObjectButtons);
                    break;
            }
            if (state != OptionsGUI.State.Controls)
            {
                self.verticalSelection = 0;
                self.UpdateVerticalSelection();
            }
        }

        private void Update(On.OptionsGUI.orig_Update orig, OptionsGUI self)
        {
            self.justClosed = false;
            if (!self.inputEnabled)
            {
                return;
            }
            if (self.state == OptionsGUI.State.Controls)
            {
                if (Cuphead.Current.controlMapper.isOpen)
                {
                    return;
                }
                self.state = OptionsGUI.State.MainOptions;
                self.canvasGroup.alpha = 1f;
                self.ToggleSubMenu(OptionsGUI.State.MainOptions);
                PlayerManager.ControlsChanged();
                return;
            }
            else if (self.GetButtonDown(CupheadButton.Pause) || self.GetButtonDown(CupheadButton.Cancel))
            {
                if (DoubleBossesManager.doubleBossOptions && self.state == OptionsGUI.State.Audio)
                {
                    GameObject audioMenu = GameObject.Find("AudioMenu");
                    GameObject.Find("BigCard").transform.localScale = new Vector3(1, 1, 1);
                    DoubleBossesManager.doubleBossOptions = false;
                    self.audioObjectButtons[0].options = self.slider;
                    self.audioObjectButtons[0].wrap = false;
                    self.audioObjectButtons[0].updateSelection(self.floatToSliderIndex(SettingsData.Data.masterVolume, -48f, 0f));
                    audioMenu.transform.GetChild(1).gameObject.transform.GetChild(0).gameObject.GetComponent<Text>().text = "MASTER VOLUME:";
                    self.audioObjectButtons[1].options = self.slider;
                    self.audioObjectButtons[1].wrap = false;
                    self.audioObjectButtons[1].updateSelection(self.floatToSliderIndex(SettingsData.Data.sFXVolume, -48f, 0f));
                    audioMenu.transform.GetChild(1).gameObject.transform.GetChild(1).gameObject.GetComponent<Text>().text = "SFX VOLUME:";
                    self.audioObjectButtons[2].options = self.slider;
                    self.audioObjectButtons[2].wrap = false;
                    self.audioObjectButtons[2].updateSelection(self.floatToSliderIndex(SettingsData.Data.musicVolume, -48f, 0f));
                    audioMenu.transform.GetChild(1).gameObject.transform.GetChild(2).gameObject.GetComponent<Text>().text = "MUSIC VOLUME:";
                    if (PlayerData.inGame && (PlayerData.Data.unlockedBlackAndWhite || PlayerData.Data.unlocked2Strip || PlayerData.Data.unlockedChaliceRecolor))
                    {
                        audioMenu.transform.GetChild(0).gameObject.transform.GetChild(3).gameObject.SetActive(true);
                        audioMenu.transform.GetChild(1).gameObject.transform.GetChild(3).gameObject.SetActive(true);
                        audioMenu.transform.GetChild(1).gameObject.transform.GetChild(3).gameObject.GetComponent<Text>().text = "VINTAGE MODE:";
                        self.audioObjectButtons[3].updateSelection((!SettingsData.Data.vintageAudioEnabled) ? 0 : 1);
                    }
                    else
                    {
                        audioMenu.transform.GetChild(0).gameObject.transform.GetChild(3).gameObject.SetActive(false);
                        audioMenu.transform.GetChild(1).gameObject.transform.GetChild(3).gameObject.SetActive(false);
                    }
                    self.ToPauseMenu();
                    return;
                }
                if (self.state == OptionsGUI.State.MainOptions)
                {
                    self.MenuSelectSound();
                    self.HideMainOptionMenu();
                    return;
                }
                self.MenuSelectSound();
                self.ToMainOptions();
                return;
            }
            else
            {
                if (!self.GetButtonDown(CupheadButton.Accept))
                {
                    if (self._selectionTimer >= 0.15f)
                    {
                        if (self.GetButton(CupheadButton.MenuUp))
                        {
                            self.MenuMoveSound();
                            int verticalSelection = self.verticalSelection;
                            self.verticalSelection = verticalSelection - 1;
                        }
                        if (self.GetButton(CupheadButton.MenuDown))
                        {
                            self.MenuMoveSound();
                            int verticalSelection2 = self.verticalSelection;
                            self.verticalSelection = verticalSelection2 + 1;
                        }
                        if (self.GetButton(CupheadButton.MenuRight) && self.currentItems[self.verticalSelection].options.Length != 0)
                        {
                            self.currentItems[self.verticalSelection].incrementSelection();
                            self.UpdateHorizontalSelection();
                        }
                        if (self.GetButton(CupheadButton.MenuLeft) && self.currentItems[self.verticalSelection].options.Length != 0)
                        {
                            self.currentItems[self.verticalSelection].decrementSelection();
                            self.UpdateHorizontalSelection();
                            return;
                        }
                    }
                    else
                    {
                        self._selectionTimer += Time.deltaTime;
                    }
                    return;
                }
                switch (self.state)
                {
                    case OptionsGUI.State.MainOptions:
                        self.OptionSelect();
                        return;
                    case OptionsGUI.State.Visual:
                        self.VisualSelect();
                        return;
                    case OptionsGUI.State.Audio:
                        self.AudioSelect();
                        return;
                    case OptionsGUI.State.Controls:
                        return;
                    case OptionsGUI.State.Language:
                        self.LanguageSelect();
                        return;
                    default:
                        return;
                }
            }
        }

        private void AudioSelect(On.OptionsGUI.orig_AudioSelect orig, OptionsGUI self)
        {
            AudioManager.Play("level_menu_select");
            if (self.verticalSelection == 4)
            {
                if (DoubleBossesManager.doubleBossOptions)
                {
                    GameObject audioMenu = GameObject.Find("AudioMenu");
                    GameObject.Find("BigCard").transform.localScale = new Vector3(1, 1, 1);
                    DoubleBossesManager.doubleBossOptions = false;
                    self.audioObjectButtons[0].options = self.slider;
                    self.audioObjectButtons[0].wrap = false;
                    self.audioObjectButtons[0].updateSelection(self.floatToSliderIndex(SettingsData.Data.masterVolume, -48f, 0f));
                    audioMenu.transform.GetChild(1).gameObject.transform.GetChild(0).gameObject.GetComponent<Text>().text = "MASTER VOLUME:";
                    self.audioObjectButtons[1].options = self.slider;
                    self.audioObjectButtons[1].wrap = false;
                    self.audioObjectButtons[1].updateSelection(self.floatToSliderIndex(SettingsData.Data.sFXVolume, -48f, 0f));
                    audioMenu.transform.GetChild(0).gameObject.transform.GetChild(1).gameObject.SetActive(true);
                    audioMenu.transform.GetChild(1).gameObject.transform.GetChild(1).gameObject.SetActive(true);
                    audioMenu.transform.GetChild(1).gameObject.transform.GetChild(1).gameObject.GetComponent<Text>().text = "SFX VOLUME:";
                    self.audioObjectButtons[2].options = self.slider;
                    self.audioObjectButtons[2].wrap = false;
                    self.audioObjectButtons[2].updateSelection(self.floatToSliderIndex(SettingsData.Data.musicVolume, -48f, 0f));
                    audioMenu.transform.GetChild(1).gameObject.transform.GetChild(2).gameObject.GetComponent<Text>().text = "MUSIC VOLUME:";
                    if (PlayerData.inGame && (PlayerData.Data.unlockedBlackAndWhite || PlayerData.Data.unlocked2Strip || PlayerData.Data.unlockedChaliceRecolor))
                    {
                        audioMenu.transform.GetChild(0).gameObject.transform.GetChild(3).gameObject.SetActive(true);
                        audioMenu.transform.GetChild(1).gameObject.transform.GetChild(3).gameObject.SetActive(true);
                        audioMenu.transform.GetChild(1).gameObject.transform.GetChild(3).gameObject.GetComponent<Text>().text = "VINTAGE MODE:";
                        self.audioObjectButtons[3].updateSelection((!SettingsData.Data.vintageAudioEnabled) ? 0 : 1);
                    }
                    else
                    {
                        audioMenu.transform.GetChild(0).gameObject.transform.GetChild(3).gameObject.SetActive(false);
                        audioMenu.transform.GetChild(1).gameObject.transform.GetChild(3).gameObject.SetActive(false);
                    }
                    self.ToPauseMenu();
                    return;
                }
                self.ToMainOptions();
            }
        }

        private void AudioHorizontalSelect(On.OptionsGUI.orig_AudioHorizontalSelect orig, OptionsGUI self, OptionsGUI.Button button)
        {
            if (DoubleBossesManager.doubleBossOptions)
            {
                self.MenuSelectSound();
                switch (self.verticalSelection)
                {
                    case 0:
                        DoubleBossesManager.db_bossSelectionType = button.selection;
                        GameObject audioMenu = GameObject.Find("AudioMenu");
                        if (button.selection == 2)
                        {
                            audioMenu.transform.GetChild(0).gameObject.transform.GetChild(1).gameObject.SetActive(true);
                            audioMenu.transform.GetChild(1).gameObject.transform.GetChild(1).gameObject.SetActive(true);
                            audioMenu.transform.GetChild(1).gameObject.transform.GetChild(1).gameObject.GetComponent<Text>().text = "SELECTED BOSS:";
                            return;
                        }
                        audioMenu.transform.GetChild(0).gameObject.transform.GetChild(1).gameObject.SetActive(false);
                        audioMenu.transform.GetChild(1).gameObject.transform.GetChild(1).gameObject.SetActive(false);
                        return;
                    case 1:
                        DoubleBossesManager.db_bossSelection = button.selection;
                        return;
                    case 2:
                        DoubleBossesManager.db_startingHealth = button.selection;
                        if (button.selection == 9)
                        {
                            DoubleBossesManager.db_infiniteHealth = true;
                            return;
                        }
                        DoubleBossesManager.db_infiniteHealth = false;
                        return;
                    default:
                        return;
                }
            }
            else
            {
                switch (self.verticalSelection)
                {
                    case 0:
                        AudioManager.masterVolume = ((button.selection > 0) ? self.sliderIndexToFloat(button.selection, -48f, 0f) : -80f);
                        SettingsData.Data.masterVolume = AudioManager.masterVolume;
                        return;
                    case 1:
                        AudioManager.sfxOptionsVolume = ((button.selection > 0) ? self.sliderIndexToFloat(button.selection, -48f, 0f) : -80f);
                        SettingsData.Data.sFXVolume = AudioManager.sfxOptionsVolume;
                        return;
                    case 2:
                        AudioManager.bgmOptionsVolume = ((button.selection > 0) ? self.sliderIndexToFloat(button.selection, -48f, 0f) : -80f);
                        SettingsData.Data.musicVolume = AudioManager.bgmOptionsVolume;
                        return;
                    case 3:
                        self.MenuSelectSound();
                        if (button.selection == 0)
                        {
                            PlayerData.Data.vintageAudioEnabled = false;
                        }
                        else if (button.options[button.selection] == button.options[1])
                        {
                            PlayerData.Data.vintageAudioEnabled = true;
                        }
                        self.savePlayerData = true;
                        return;
                    default:
                        return;
                }
            }
        }
        public void ShowMainOptionMenu(On.OptionsGUI.orig_ShowMainOptionMenu orig, OptionsGUI self)
        {
            self.state = OptionsGUI.State.MainOptions;
            if (DoubleBossesManager.doubleBossOptions)
            {
                self.state = OptionsGUI.State.Audio;
            }
            self.ToggleSubMenu(self.state);
            self.optionMenuOpen = true;
            self.verticalSelection = 0;
            self.canvasGroup.alpha = 1f;
            self.FrameDelayedCallback(new Action(self.Interactable), 1);
            self.UpdateVerticalSelection();
        }

    }
}
