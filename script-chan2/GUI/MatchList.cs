using script_chan2.DataTypes;
using script_chan2.Discord;
using System;
using System.Collections.Generic;

namespace script_chan2.GUI
{
    public static class MatchList
    {
        public static List<Match> OpenedMatches = new List<Match>();

        public static void AddMatch(Match match)
        {
            OpenedMatches.Add(match);
            UpdateRichPresence();
        }

        public static void RemoveMatch(Match match)
        {
            OpenedMatches.Remove(match);
            UpdateRichPresence();
        }

        private static void UpdateRichPresence()
        {
            if (OpenedMatches.Count > 0)
            {
                string matches = string.Empty;
                foreach (var match in OpenedMatches)
                {
                    matches += match.Name + Environment.NewLine;
                }
                matches.Trim(Environment.NewLine.ToCharArray());
                DiscordApi.SetRichPresence(Properties.Resources.DiscordRPC_ReffingStatus, matches);
            }
            else
            {
                DiscordApi.SetRichPresence(Properties.Resources.DiscordRPC_DefaultStatus);
            }
        }
    }
}
