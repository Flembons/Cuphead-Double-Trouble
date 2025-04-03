using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Blender;

namespace DoubleBosses
{
    /*
     * This class initializes all of the settings for spawning two bosses. By default, the same boss will be spawned and the player will have a base health of 3.
     * The manager also keeps track of certain information like what levels to load when loading two bosses, whether the player is restarting a level, and
     * all of the configuration settings from the Mod Options menu.
     */

    public class DoubleBossesManager
    {
        public static bool db_randomBosses;
        public static string db_firstLevel;
        public static Levels db_secondLevel;
        public static bool db_retrying;
        public static Levels[] db_possibleLevels;
        public static bool db_retryingNewBoss;
        public static int db_bossSelection;
        public static int db_startingHealth;
        public static bool db_infiniteHealth;
        public static int db_bossSelectionType;

        public static bool doubleBossOptions;

        public static string[] bossChoice;
        public static int bossSelection;
        public static string[] bossSelectionOptions;
        public static string[] bossYesNoOption;
        public static string[] bossStartingHealthOption;

        public DoubleBossesManager() {
            db_bossSelection = 0;
            db_randomBosses = false;
            db_startingHealth = 2;
            db_infiniteHealth = false;
            db_bossSelectionType = 0;
            db_retrying = false;
            db_retryingNewBoss = false;
            db_firstLevel = "";
            db_secondLevel = Levels.Test;


            // Check if the DLC is installed, and adjust the possible levels to only include the base game levels if not
            if (BlenderAPI.HasDLC)
            {
                db_possibleLevels = new Levels[]
                {
                    Levels.Veggies,
                    Levels.Slime,
                    Levels.FlyingBlimp,
                    Levels.Flower,
                    Levels.Frogs,
                    Levels.Baroness,
                    Levels.Clown,
                    Levels.FlyingGenie,
                    Levels.Dragon,
                    Levels.FlyingBird,
                    Levels.Bee,
                    Levels.Pirate,
                    Levels.SallyStagePlay,
                    Levels.Mouse,
                    Levels.Robot,
                    Levels.FlyingMermaid,
                    Levels.Train,
                    Levels.DicePalaceBooze,
                    Levels.DicePalaceChips,
                    Levels.DicePalaceCigar,
                    Levels.DicePalaceDomino,
                    Levels.DicePalaceRabbit,
                    Levels.DicePalaceRoulette,
                    Levels.DicePalaceFlyingHorse,
                    Levels.DicePalaceEightBall,
                    Levels.DicePalaceFlyingMemory,
                    Levels.DicePalaceMain,
                    Levels.Devil,
                    Levels.OldMan,
                    Levels.RumRunners,
                    Levels.Airplane,
                    Levels.SnowCult,
                    Levels.FlyingCowboy,
                    Levels.Saltbaker,
                    Levels.Graveyard,
                    Levels.ChessPawn,
                    Levels.ChessKnight,
                    Levels.ChessBishop,
                    Levels.ChessRook,
                    Levels.ChessQueen
                };

                bossChoice = new string[]
                {
                    "ROOT PACK",
                    "GOOPY",
                    "HILDA BERG",
                    "CAGNEY",
                    "FROGS",
                    "BARONESS",
                    "BEPPI",
                    "DJIMMI",
                    "DRAGON",
                    "WALLY",
                    "BEE",
                    "PIRATE",
                    "STAGEPLAY",
                    "WERNER",
                    "DR KAHL",
                    "CALA MARIA",
                    "GHOST TRAIN",
                    "TIPSY TROOP",
                    "CHIPS",
                    "MR. WHEEZY",
                    "PIP AND DOT",
                    "RABBIT",
                    "PIROULETTA",
                    "PHEAR LAP",
                    "EIGHT BALL",
                    "MR. CHIMES",
                    "KING DICE",
                    "THE DEVIL",
                    "GLUMSTONE",
                    "MOONSHINE",
                    "DOGS",
                    "MORTIMER",
                    "ESTHER",
                    "SALTBAKER",
                    "ANGEL/DEVIL",
                    "THE PAWNS",
                    "THE KNIGHT",
                    "THE BISHOP",
                    "THE ROOK",
                    "THE QUEEN"
                };
            }
            else
            {
                db_possibleLevels = new Levels[]
                {
                    Levels.Veggies,
                    Levels.Slime,
                    Levels.FlyingBlimp,
                    Levels.Flower,
                    Levels.Frogs,
                    Levels.Baroness,
                    Levels.Clown,
                    Levels.FlyingGenie,
                    Levels.Dragon,
                    Levels.FlyingBird,
                    Levels.Bee,
                    Levels.Pirate,
                    Levels.SallyStagePlay,
                    Levels.Mouse,
                    Levels.Robot,
                    Levels.FlyingMermaid,
                    Levels.Train,
                    Levels.DicePalaceBooze,
                    Levels.DicePalaceChips,
                    Levels.DicePalaceCigar,
                    Levels.DicePalaceDomino,
                    Levels.DicePalaceRabbit,
                    Levels.DicePalaceRoulette,
                    Levels.DicePalaceFlyingHorse,
                    Levels.DicePalaceEightBall,
                    Levels.DicePalaceFlyingMemory,
                    Levels.DicePalaceMain,
                    Levels.Devil,
                };

                bossChoice = new string[]
                {
                    "ROOT PACK",
                    "GOOPY",
                    "HILDA BERG",
                    "CAGNEY",
                    "FROGS",
                    "BARONESS",
                    "BEPPI",
                    "DJIMMI",
                    "DRAGON",
                    "WALLY",
                    "BEE",
                    "PIRATE",
                    "STAGEPLAY",
                    "WERNER",
                    "DR KAHL",
                    "CALA MARIA",
                    "GHOST TRAIN",
                    "TIPSY TROOP",
                    "CHIPS",
                    "MR. WHEEZY",
                    "PIP AND DOT",
                    "RABBIT",
                    "PIROULETTA",
                    "PHEAR LAP",
                    "EIGHT BALL",
                    "MR. CHIMES",
                    "KING DICE",
                    "THE DEVIL"
                };
            }

            doubleBossOptions = false;


            bossStartingHealthOption = new string[]
            {
                "1",
                "2",
                "3",
                "4",
                "5",
                "6",
                "7",
                "8",
                "9",
                "∞"
            };

            bossYesNoOption = new string[]
            {
                "NO",
                "YES"
            };

            bossSelectionOptions = new string[]
            {
                "SAME",
                "RANDOM",
                "CHOOSE"
            };

        }
    }
}
