using BepInEx;
using On;
using static System.Net.Mime.MediaTypeNames;
using UnityEngine;
using Blender.Patching;
using Blender.Utility;
using DoubleBosses.Boss_Changes;

namespace DoubleBosses
{
    [BepInPlugin("CupheadDoubleBosses", "DoubleBosses", "1.0.0")]

    /*
     * This class is the main class for this mod that creates the DoubleBossesManager and 
     * initializes all of the changes made to each existing class in the game. Each class that
     * has been changed calls an Init function which hooks into functions from that class. You can
     * see the details of these function hooks in the corresponding .cs files
     */
    public class Plugin : BaseUnityPlugin
    {
        public static DoubleBossesManager doubleBossManager;

        private void Awake()
        {
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
