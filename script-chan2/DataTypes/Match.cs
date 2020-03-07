using script_chan2.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace script_chan2.DataTypes
{
    public class Match
    {
        public Match(Tournament tournament, Mappool mappool, string name, GameModes gameMode, TeamModes teamMode, WinConditions winCondition, Team rollWinnerTeam, Player rollWinnerPlayer, Team firstPickerTeam, Player firstPickerPlayer,
            int bo, bool enableWebhooks, int mpTimer, int pointsForSecondBan, bool allPicksFreemod, MatchStatus status, int id = 0)
        {
            Players = new List<Player>();
            Teams = new List<Team>();
            Tournament = tournament;
            Mappool = mappool;
            Name = name;
            GameMode = gameMode;
            TeamMode = teamMode;
            WinCondition = winCondition;
            if (TeamMode == TeamModes.TeamVS)
            {
                RollWinnerTeam = rollWinnerTeam;
                FirstPickerTeam = firstPickerTeam;
            }
            else
            {
                RollWinnerPlayer = rollWinnerPlayer;
                FirstPickerPlayer = firstPickerPlayer;
            }
            BO = bo;
            EnableWebhooks = enableWebhooks;
            MpTimer = mpTimer;
            PointsForSecondBan = pointsForSecondBan;
            AllPicksFreemod = allPicksFreemod;
            Status = status;
            Id = id;
        }

        public int Id { get; private set; }

        public Tournament Tournament { get; set; }

        public Mappool Mappool { get; set; }

        public string Name { get; set; }

        public GameModes GameMode { get; set; }

        public TeamModes TeamMode { get; set; }

        public WinConditions WinCondition { get; set; }

        public Team RollWinnerTeam { get; set; }

        public Player RollWinnerPlayer { get; set; }

        public Team FirstPickerTeam { get; set; }

        public Player FirstPickerPlayer { get; set; }

        public int BO { get; set; }

        public bool EnableWebhooks { get; set; }

        public int MpTimer { get; set; }

        public int PointsForSecondBan { get; set; }

        public bool AllPicksFreemod { get; set; }

        public MatchStatus Status { get; set; }

        public List<Team> Teams { get; set; }

        public List<Player> Players { get; set; }

        public void Save()
        {
            if (Id == 0)
                Id = Database.Database.AddMatch(this);
            else
                Database.Database.UpdateMatch(this);
        }

        public void Delete()
        {
            Database.Database.DeleteMatch(this);
        }
    }
}
