using script_chan2.Enums;
using System.Collections.Generic;

namespace script_chan2.DataTypes
{
    public static class Utils
    {
        public static string ConvertGameModsToString(List<GameMods> mods)
        {
            if (mods.Count == 0)
                return "NoMod";
            var modString = "";
            for (var i = 0; i < mods.Count; i++)
            {
                if (i > 0)
                    modString += " ";
                switch (mods[i])
                {
                    case GameMods.Hidden: modString += "HD"; break;
                    case GameMods.HardRock: modString += "HR"; break;
                    case GameMods.DoubleTime: modString += "DT"; break;
                    case GameMods.Flashlight: modString += "FL"; break;
                    case GameMods.Freemod: modString += "Freemod"; break;
                    case GameMods.TieBreaker: modString += "Freemod"; break;
                    case GameMods.NoFail: modString += "NF"; break;
                }
            }
            return modString;
        }

        public static List<GameMods> ConvertStringtoGameMods(string modString)
        {
            var mods = new List<GameMods>();
            for (var i = 0; i < modString.Length; i += 2)
            {
                switch (modString.Substring(i, 2))
                {
                    case "HD": mods.Add(GameMods.Hidden); break;
                    case "HR": mods.Add(GameMods.HardRock); break;
                    case "DT": mods.Add(GameMods.DoubleTime); break;
                    case "TB": mods.Add(GameMods.TieBreaker); break;
                }
            }
            return mods;
        }

        public static int ConvertTeamModeToMpNumber(TeamModes mode)
        {
            switch (mode)
            {
                case TeamModes.HeadToHead:
                case TeamModes.BattleRoyale:
                    return 0;
                case TeamModes.TeamVS:
                    return 2;
            }
            return 0;
        }
    }
}
