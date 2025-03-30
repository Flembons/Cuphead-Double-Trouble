﻿using BepInEx;
using On;
using static System.Net.Mime.MediaTypeNames;
using UnityEngine;
using Blender.Patching;
using Blender.Utility;
using DoubleBosses.Boss_Changes;

namespace DoubleBosses
{
    [BepInPlugin("CupheadDoubleBosses", "DoubleBosses", "1.0.0")]
    public class Plugin : BaseUnityPlugin
    {
        public static DoubleBossesManager doubleBossManager;

        private void Awake()
        {
            // Plugin startup logic
            doubleBossManager = new DoubleBossesManager();

            new DicePalaceChanges().Init();
            new OptionsGuiChanges().Init();
            new LevelChanges().Init();
            new LevelPauseGUIChanges().Init();
            new LevelGameOverGUIChanges().Init();
            new PlayerStatsManagerChanges().Init();
            new AirplaneLevelChanges().Init();
            new FlyingCowboyLevelChanges().Init();
            new FrogsLevelChanges().Init();
            new OldManLevelChanges().Init();
            new SaltbakerChanges().Init();
            new SnowCultChanges().Init();
            new RumRunnersChanges().Init();
            new CameraChanges().Init();
            new SceneLoaderChanges().Init();
            Logger.LogInfo($"Plugin DoubleBosses is loaded!");
        }
    }
}
