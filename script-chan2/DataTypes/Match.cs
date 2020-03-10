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
        public Match(Tournament tournament, Mappool mappool, string name, int roomId, GameModes gameMode, TeamModes teamMode, WinConditions winCondition, Team teamBlue, int teamBluePoints, Team teamRed, int teamRedPoints,
            int teamSize, int roomSize, Team rollWinnerTeam, Player rollWinnerPlayer, Team firstPickerTeam, Player firstPickerPlayer, int bo, bool enableWebhooks, int mpTimerCommand, int mpTimerAfterGame,
            int mpTimerAfterPick, int pointsForSecondBan, bool allPicksFreemod, MatchStatus status, int id = 0)
        {
            Players = new Dictionary<Player, int>();
            Tournament = tournament;
            Mappool = mappool;
            Name = name;
            RoomId = roomId;
            GameMode = gameMode;
            TeamMode = teamMode;
            WinCondition = winCondition;
            TeamSize = teamSize;
            RoomSize = roomSize;
            if (TeamMode == TeamModes.TeamVS)
            {
                TeamBlue = teamBlue;
                TeamBluePoints = teamBluePoints;
                TeamRed = teamRed;
                TeamRedPoints = teamRedPoints;
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
            MpTimerCommand = mpTimerCommand;
            MpTimerAfterGame = mpTimerAfterGame;
            MpTimerAfterPick = mpTimerAfterPick;
            PointsForSecondBan = pointsForSecondBan;
            AllPicksFreemod = allPicksFreemod;
            Status = status;
            Id = id;
        }

        public int Id { get; private set; }

        public Tournament Tournament { get; set; }

        public Mappool Mappool { get; set; }

        public string Name { get; set; }

        public int RoomId { get; set; }

        public GameModes GameMode { get; set; }

        public TeamModes TeamMode { get; set; }

        public WinConditions WinCondition { get; set; }

        public Team TeamBlue { get; set; }

        public int TeamBluePoints { get; set; }

        public Team TeamRed { get; set; }

        public int TeamRedPoints { get; set; }

        public int TeamSize { get; set; }

        public int RoomSize { get; set; }

        public Team RollWinnerTeam { get; set; }

        public Player RollWinnerPlayer { get; set; }

        public Team FirstPickerTeam { get; set; }

        public Player FirstPickerPlayer { get; set; }

        public int BO { get; set; }

        public bool EnableWebhooks { get; set; }

        public int MpTimerCommand { get; set; }

        public int MpTimerAfterGame { get; set; }

        public int MpTimerAfterPick { get; set; }

        public int PointsForSecondBan { get; set; }

        public bool AllPicksFreemod { get; set; }

        public MatchStatus Status { get; set; }

        public Dictionary<Player, int> Players { get; set; }

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
