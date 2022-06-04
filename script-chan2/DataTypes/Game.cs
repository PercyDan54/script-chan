﻿using script_chan2.Enums;
using System.Collections.Generic;
using System.Linq;

namespace script_chan2.DataTypes
{
    public class Game
    {
        public Game()
        {
            Scores = new List<Score>();
            Mods = new List<GameMods>();
        }

        public int Id { get; set; }

        public Match Match { get; set; }

        public Beatmap Beatmap { get; set; }

        public List<GameMods> Mods;

        public List<Score> Scores;

        public bool Counted { get; set; }

        public bool Warmup { get; set; }

        public bool TeamRedWon
        {
            get
            {
                return Scores.Where(x => x.Team == LobbyTeams.Red && x.Passed).Sum(x => x.Points) > Scores.Where(x => x.Team == LobbyTeams.Blue && x.Passed).Sum(x => x.Points);
            }
        }

        public bool TeamBlueWon
        {
            get
            {
                return Scores.Where(x => x.Team == LobbyTeams.Red && x.Passed).Sum(x => x.Points) < Scores.Where(x => x.Team == LobbyTeams.Blue && x.Passed).Sum(x => x.Points);
            }
        }

        public bool Draw
        {
            get
            {
                return Scores.Where(x => x.Team == LobbyTeams.Red && x.Passed).Sum(x => x.Points) == Scores.Where(x => x.Team == LobbyTeams.Blue && x.Passed).Sum(x => x.Points);
            }
        }
    }
}
