using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Blender;

namespace DoubleBosses
{
    public class DoubleBossesManager
    {
        // These came from SceneLoader
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
        public static bool db_elderKettle;

        // This came from LevelPauseGUI
        public static bool doubleBossOptions;

        // These came from OptionsGUI
        public static string[] bossChoice;
        public static int bossSelection;
        public static string[] bossSelectionOptions;
        public static string[] bossYesNoOption;
        public static string[] bossStartingHealthOption;

        public static int db_shortFrogCheck = 0;
        public static int db_tallFrogCheck = 0;

        public DoubleBossesManager() {
            db_elderKettle = false;
            db_bossSelection = 0;
            db_randomBosses = false;
            db_startingHealth = 2;
            db_infiniteHealth = false;
            db_bossSelectionType = 0;
            db_retrying = false;
            db_retryingNewBoss = false;
            db_firstLevel = "";
            db_secondLevel = Levels.Test;

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
                    "THE ROOT PACK",
                    "GOOPY LE GRANDE",
                    "HILDA BERG",
                    "CAGNEY CARNATION",
                    "RIBBY AND CROAKS",
                    "BARONESS VON BON BON",
                    "BEPPI THE CLOWN",
                    "DJIMMI THE GREAT",
                    "GRIM MATCHSTICK",
                    "WALLY WARBLES",
                    "RUMOR HONEYBOTTOMS",
                    "CAPTAIN BRINEYBEARD",
                    "SALLY STAGEPLAY",
                    "WERNER WERMAN",
                    "DR KAHL'S ROBOT",
                    "CALA MARIA",
                    "PHANTOM EXPRESS",
                    "TIPSY TROOP",
                    "CHIPS BETTIGAN",
                    "MR. WHEEZY",
                    "PIP AND DOT",
                    "HOPUS POCUS",
                    "PIROULETTA",
                    "PHEAR LAP",
                    "MANGOSTEEN",
                    "MR. CHIMES",
                    "KING DICE",
                    "THE DEVIL",
                    "GLUMSTONE THE GIANT",
                    "MOONSHINE MOB",
                    "THE HOWLING ACES",
                    "MORTIMER FREEZE",
                    "ESTHER WINCHESTER",
                    "CHEF SALTBAKER",
                    "ANGEL AND DEVIL",
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
                    "THE ROOT PACK",
                    "GOOPY LE GRANDE",
                    "HILDA BERG",
                    "CAGNEY CARNATION",
                    "RIBBY AND CROAKS",
                    "BARONESS VON BON BON",
                    "BEPPI THE CLOWN",
                    "DJIMMI THE GREAT",
                    "GRIM MATCHSTICK",
                    "WALLY WARBLES",
                    "RUMOR HONEYBOTTOMS",
                    "CAPTAIN BRINEYBEARD",
                    "SALLY STAGEPLAY",
                    "WERNER WERMAN",
                    "DR KAHL'S ROBOT",
                    "CALA MARIA",
                    "PHANTOM EXPRESS",
                    "TIPSY TROOP",
                    "CHIPS BETTIGAN",
                    "MR. WHEEZY",
                    "PIP AND DOT",
                    "HOPUS POCUS",
                    "PIROULETTA",
                    "PHEAR LAP",
                    "MANGOSTEEN",
                    "MR. CHIMES",
                    "KING DICE",
                    "THE DEVIL",
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
