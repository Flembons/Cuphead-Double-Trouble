using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using On;
using Blender.Patching;
using Blender.Utility;
using UnityEngine.SceneManagement;
using UnityEngine.U2D;

namespace DoubleBosses
{
    public class SceneLoaderChanges
    {
        public void Init()
        {
            On.SceneLoader.loop_cr += loop_cr;
            On.SceneLoader.load_cr += load_cr;
            On.SceneLoader.LoadScene_Scenes_Transition_Transition_Icon_Context += LoadScene;
            On.SceneLoader.ReloadLevel += ReloadLevel;
            On.SceneLoader.LoadDicePalaceLevel += LoadDicePalaceLevel;
        }
        private IEnumerator loop_cr(On.SceneLoader.orig_loop_cr orig, SceneLoader self)
        {
            SceneLoader.currentlyLoading = true;
            yield return self.StartCoroutine(self.in_cr());
            self.StartCoroutine(self.load_cr());
            yield return self.StartCoroutine(self.iconFadeIn_cr());
            while (!self.doneLoadingSceneAsync)
            {
                yield return null;
            }
            if (SceneLoader.SceneName != Scenes.scene_slot_select.ToString())
            {
                AudioManager.SnapshotReset(SceneLoader.SceneName, 0.15f);
            }
            AsyncOperation op = Resources.UnloadUnusedAssets();
            while (!op.isDone)
            {
                yield return null;
            }
            yield return self.StartCoroutine(self.iconFadeOut_cr());
            bool flag = false;
            for (int i = 0; i < DoubleBossesManager.db_possibleLevels.Length; i++)
            {
                if (SceneLoader.SceneName == LevelProperties.GetLevelScene(DoubleBossesManager.db_possibleLevels[i]))
                {
                    flag = true;
                }
            }
            if (flag)
            {
                GameObject[] array = GameObject.FindGameObjectsWithTag("MainCamera");
                array[0].GetComponent<CupheadLevelCamera>().gameObject.SetActive(false);
                GameObject gameObject = GameObject.Find("Level_HUD").transform.GetChild(0).transform.GetChild(0).transform.GetChild(0).GetChild(2).gameObject;
                for (int j = 0; j < gameObject.transform.childCount; j++)
                {
                    Vector3 end = gameObject.transform.GetChild(j).GetComponent<LevelHUDPlayerSuperCard>().end;
                    end.y = 0f;
                    gameObject.transform.GetChild(j).GetComponent<LevelHUDPlayerSuperCard>().end = end;
                }
                array[0].GetComponent<CupheadLevelCamera>().gameObject.SetActive(true);
            }
            yield return self.StartCoroutine(self.out_cr());
            SceneLoader.properties.Reset();
            SceneLoader.currentlyLoading = false;
            yield break;
        }

        private IEnumerator load_cr(On.SceneLoader.orig_load_cr orig, SceneLoader self)
        {
            self.doneLoadingSceneAsync = false;
            GC.Collect();
            if (SceneLoader.SceneName != SceneLoader.previousSceneName && SceneLoader.SceneName != Scenes.scene_slot_select.ToString())
            {
                string text = null;
                if (!Array.Exists<Levels>(Level.kingOfGamesLevelsWithCastle, (Levels level) => LevelProperties.GetLevelScene(level) == SceneLoader.SceneName))
                {
                    text = Scenes.scene_level_chess_castle.ToString();
                }
                AssetBundleLoader.UnloadAssetBundles();
                AssetLoader<SpriteAtlas>.UnloadAssets(new string[]
                {
                text
                });
                if (SceneLoader.SceneName != Scenes.scene_cutscene_dlc_saltbaker_prebattle.ToString())
                {
                    AssetLoader<AudioClip>.UnloadAssets(new string[0]);
                }
                AssetLoader<Texture2D[]>.UnloadAssets(new string[0]);
            }
            if (SceneLoader.SceneName == Scenes.scene_title.ToString())
            {
                DLCManager.RefreshDLC();
            }
            AssetLoaderOption atlasOption = AssetLoaderOption.None();
            if (SceneLoader.SceneName == Scenes.scene_level_chess_castle.ToString())
            {
                atlasOption = AssetLoaderOption.PersistInCacheTagged(SceneLoader.SceneName);
            }
            string[] preloadAtlases = AssetLoader<SpriteAtlas>.GetPreloadAssetNames(SceneLoader.SceneName);
            string[] preloadMusic = AssetLoader<AudioClip>.GetPreloadAssetNames(SceneLoader.SceneName);
            if (SceneLoader.SceneName != SceneLoader.previousSceneName && (preloadAtlases.Length != 0 || preloadMusic.Length != 0))
            {
                AsyncOperation intermediateSceneAsyncOp = SceneManager.LoadSceneAsync(self.LOAD_SCENE_NAME);
                while (!intermediateSceneAsyncOp.isDone)
                {
                    yield return null;
                }
                int num;
                for (int i = 0; i < preloadAtlases.Length; i = num + 1)
                {
                    yield return AssetLoader<SpriteAtlas>.LoadAsset(preloadAtlases[i], atlasOption);
                    num = i;
                }
                AssetLoaderOption musicOption = AssetLoaderOption.None();
                for (int i = 0; i < preloadMusic.Length; i = num + 1)
                {
                    yield return AssetLoader<AudioClip>.LoadAsset(preloadMusic[i], musicOption);
                    num = i;
                }
                Coroutine[] persistentAssetsCoroutines = DLCManager.LoadPersistentAssets();
                if (persistentAssetsCoroutines != null)
                {
                    for (int i = 0; i < persistentAssetsCoroutines.Length; i = num + 1)
                    {
                        yield return persistentAssetsCoroutines[i];
                        num = i;
                    }
                }
                yield return null;
                intermediateSceneAsyncOp = null;
                musicOption = null;
                persistentAssetsCoroutines = null;
            }
            AsyncOperation async2 = null;
            AsyncOperation async3;
            if (DoubleBossesManager.db_retrying || DoubleBossesManager.db_retryingNewBoss)
            {
                async3 = SceneManager.LoadSceneAsync(DoubleBossesManager.db_firstLevel);
            }
            else
            {
                async3 = SceneManager.LoadSceneAsync(SceneLoader.SceneName);
            }
            bool flag = false;
            for (int j = 0; j < DoubleBossesManager.db_possibleLevels.Length; j++)
            {
                if (SceneLoader.SceneName == LevelProperties.GetLevelScene(DoubleBossesManager.db_possibleLevels[j]))
                {
                    flag = true;
                    if (!DoubleBossesManager.db_retrying && !DoubleBossesManager.db_retryingNewBoss)
                    {
                        DoubleBossesManager.db_firstLevel = SceneLoader.SceneName;
                    }
                }
            }
            if (flag)
            {
                if (DoubleBossesManager.db_bossSelectionType == 1)
                {
                    if (DoubleBossesManager.db_retrying)
                    {
                        async2 = SceneManager.LoadSceneAsync(LevelProperties.GetLevelScene(DoubleBossesManager.db_secondLevel), UnityEngine.SceneManagement.LoadSceneMode.Additive);
                    }
                    else
                    {
                        int num2 = UnityEngine.Random.Range(0, DoubleBossesManager.db_possibleLevels.Length - 1);
                        DoubleBossesManager.db_secondLevel = DoubleBossesManager.db_possibleLevels[num2];
                        async2 = SceneManager.LoadSceneAsync(LevelProperties.GetLevelScene(DoubleBossesManager.db_secondLevel), UnityEngine.SceneManagement.LoadSceneMode.Additive);
                    }
                }
                else if (DoubleBossesManager.db_bossSelectionType == 0)
                {
                    async2 = SceneManager.LoadSceneAsync(SceneLoader.SceneName, UnityEngine.SceneManagement.LoadSceneMode.Additive);
                }
                else
                {
                    async2 = SceneManager.LoadSceneAsync(LevelProperties.GetLevelScene(DoubleBossesManager.db_possibleLevels[DoubleBossesManager.db_bossSelection]), UnityEngine.SceneManagement.LoadSceneMode.Additive);
                }
                while ((!async3.isDone && !async2.isDone) || AssetBundleLoader.loadCounter > 0)
                {
                    self.UpdateProgress(async3.progress);
                    yield return null;
                }
                DoubleBossesManager.db_retryingNewBoss = false;
                DoubleBossesManager.db_retrying = false;
                self.doneLoadingSceneAsync = true;
                yield break;
            }
            while (!async3.isDone || AssetBundleLoader.loadCounter > 0)
            {
                self.UpdateProgress(async3.progress);
                yield return null;
            }
            DoubleBossesManager.db_retryingNewBoss = false;
            DoubleBossesManager.db_retrying = false;
            self.doneLoadingSceneAsync = true;
            yield break;
        }

        public static void LoadScene(On.SceneLoader.orig_LoadScene_Scenes_Transition_Transition_Icon_Context orig,
                    Scenes scene, SceneLoader.Transition transitionStart, SceneLoader.Transition transitionEnd,
                    SceneLoader.Icon icon = SceneLoader.Icon.Hourglass, SceneLoader.Context context = null)
        {
            if (SceneLoader.currentlyLoading)
            {
                return;
            }
            InterruptingPrompt.SetCanInterrupt(false);
            SceneLoader.properties.transitionStart = transitionStart;
            SceneLoader.properties.transitionEnd = transitionEnd;
            SceneLoader.properties.icon = icon;
            SceneLoader.EndTransitionDelay = 0.6f;
            if (DoubleBossesManager.db_retrying || DoubleBossesManager.db_retryingNewBoss)
            {
                SceneLoader.SceneName = DoubleBossesManager.db_firstLevel;
                SceneLoader.previousSceneName = DoubleBossesManager.db_firstLevel;
            }
            else
            {
                SceneLoader.previousSceneName = SceneLoader.SceneName;
                SceneLoader.SceneName = scene.ToString();
            }
            SceneLoader.CurrentContext = context;
            SceneLoader.instance.load();
        }

        public static void ReloadLevel(On.SceneLoader.orig_ReloadLevel orig)
        {
            if (Level.IsTowerOfPower)
            {
                if (TowerOfPowerLevelGameInfo.IsTokenLeft(0))
                {
                    TowerOfPowerLevelGameInfo.PLAYER_STATS[0].HP = 3;
                    TowerOfPowerLevelGameInfo.PLAYER_STATS[0].BonusHP = 3;
                    TowerOfPowerLevelGameInfo.PLAYER_STATS[0].SuperCharge = 0f;
                    TowerOfPowerLevelGameInfo.ReduceToken(0);
                }
                else
                {
                    TowerOfPowerLevelGameInfo.PLAYER_STATS[0].HP = 0;
                    TowerOfPowerLevelGameInfo.PLAYER_STATS[0].BonusHP = 0;
                    TowerOfPowerLevelGameInfo.PLAYER_STATS[0].SuperCharge = 0f;
                }
                if (PlayerManager.Multiplayer)
                {
                    if (TowerOfPowerLevelGameInfo.IsTokenLeft(1))
                    {
                        TowerOfPowerLevelGameInfo.PLAYER_STATS[1].HP = 3;
                        TowerOfPowerLevelGameInfo.PLAYER_STATS[1].BonusHP = 3;
                        TowerOfPowerLevelGameInfo.PLAYER_STATS[1].SuperCharge = 0f;
                        TowerOfPowerLevelGameInfo.ReduceToken(1);
                    }
                    else
                    {
                        TowerOfPowerLevelGameInfo.PLAYER_STATS[1].HP = 0;
                        TowerOfPowerLevelGameInfo.PLAYER_STATS[1].BonusHP = 0;
                        TowerOfPowerLevelGameInfo.PLAYER_STATS[1].SuperCharge = 0f;
                    }
                }
            }
            if (Level.IsDicePalace && DoubleBossesManager.db_bossSelectionType == 0)
            {
                SceneLoader.LoadDicePalaceLevel(DicePalaceLevels.DicePalaceMain);
                return;
            }

            float transitionStartTime = SceneLoader.properties.transitionStartTime;
            SceneLoader.properties.transitionStartTime = 0.25f;
            SceneLoader.LoadLevel(SceneLoader.CurrentLevel, SceneLoader.Transition.Fade, SceneLoader.Icon.None, null);
            SceneLoader.properties.transitionStartTime = transitionStartTime;
        }

        public static void LoadDicePalaceLevel(On.SceneLoader.orig_LoadDicePalaceLevel orig, DicePalaceLevels dicePalaceLevel)
        {
            Levels level = (SceneLoader.CurrentLevel = LevelProperties.GetDicePalaceLevel(dicePalaceLevel));
            SceneLoader.LoadScene(LevelProperties.GetLevelScene(level), SceneLoader.Transition.Fade, SceneLoader.Transition.Fade, SceneLoader.Icon.None);
        }
    }
}
