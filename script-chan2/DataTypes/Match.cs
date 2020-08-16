using script_chan2.Enums;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace script_chan2.DataTypes
{
    public class Match
    {
        private ILogger localLog = Log.ForContext<Match>();

        public Match(int id = 0)
        {
            Players = new Dictionary<Player, int>();
            TeamsBR = new Dictionary<Team, int>();
            Picks = new List<MatchPick>();
            Bans = new List<MatchPick>();
            ChatMessages = new List<IrcMessage>();
            Games = new List<Game>();
            Id = id;
            RoomId = 0;
            TeamBluePoints = 0;
            TeamRedPoints = 0;
            ViewerMode = false;
            Status = MatchStatus.New;
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

        public bool ViewerMode { get; set; }

        public int MpTimerCommand { get; set; }

        public int MpTimerAfterGame { get; set; }

        public int MpTimerAfterPick { get; set; }

        public int PointsForSecondBan { get; set; }

        public bool AllPicksFreemod { get; set; }

        public MatchStatus Status { get; set; }

        public bool WarmupMode { get; set; }

        public DateTime? MatchTime { get; set; }

        public Dictionary<Player, int> Players { get; set; }

        public Dictionary<Team, int> TeamsBR { get; set; }

        public List<MatchPick> Picks { get; set; }

        public List<MatchPick> Bans { get; set; }

        public List<IrcMessage> ChatMessages { get; set; }

        public List<Game> Games { get; set; }

        public void Save()
        {
            localLog.Information("'{name}' save", Name);
            if (Id == 0)
                Id = Database.Database.AddMatch(this);
            else
                Database.Database.UpdateMatch(this);
        }

        public void Delete()
        {
            localLog.Information("'{name}' delete", Name);
            Database.Database.DeleteMatch(this);
        }

        public List<string> GetPlayerList()
        {
            localLog.Information("'{name}' get player list", Name);
            var players = new List<string>();

            if (TeamMode == TeamModes.TeamVS)
            {
                foreach (var player in TeamBlue.Players)
                    players.Add(player.Name);
                foreach (var player in TeamRed.Players)
                    players.Add(player.Name);
            }
            if (TeamMode == TeamModes.HeadToHead)
            {
                foreach (var player in Players)
                    players.Add(player.Key.Name);
            }
            if (TeamMode == TeamModes.BattleRoyale)
            {
                foreach (var team in TeamsBR)
                {
                    foreach (var player in team.Key.Players)
                        players.Add(player.Name);
                }
            }

            return players;
        }

        public void ReloadMessages()
        {
            localLog.Information("'{name}' reload messages", Name);
            ChatMessages = Database.Database.GetIrcMessages(this);
        }

        public void ClearMessages()
        {
            localLog.Information("'{name}' clear messages", Name);
            ChatMessages.Clear();
        }

        public async Task UpdateScores(bool newGameExpected = false)
        {
            localLog.Information("'{name}' update scores", Name);
            var oldGameCount = Games.Count;
            var keepUpdating = true;
            while (keepUpdating)
            {
                await OsuApi.OsuApi.UpdateGames(this);
                if (newGameExpected && Games.Count == oldGameCount)
                    await Task.Delay(5000);
                else
                    keepUpdating = false;
            }
            foreach (var game in Games.Where(x => !x.Counted))
            {
                if (!WarmupMode)
                {
                    if (TeamMode == TeamModes.TeamVS)
                    {
                        var teamRedScore = 0;
                        var teamBlueScore = 0;
                        foreach (var score in game.Scores)
                        {
                            if (score.Passed)
                            {
                                if (TeamRed.Players.Any(x => x.Id == score.Player.Id))
                                    teamRedScore += score.Points;
                                if (TeamBlue.Players.Any(x => x.Id == score.Player.Id))
                                    teamBlueScore += score.Points;
                            }
                        }
                        if (teamRedScore > teamBlueScore)
                            TeamRedPoints++;
                        if (teamBlueScore > teamRedScore)
                            TeamBluePoints++;
                    }
                    if (TeamMode == TeamModes.HeadToHead)
                    {
                        var list = new List<Player>();
                        foreach (var score in game.Scores.Where(x => x.Passed && Players.Keys.Contains(x.Player)).OrderByDescending(x => x.Points))
                        {
                            list.Add(score.Player);
                        }
                        foreach (var score in game.Scores.Where(x => !x.Passed && Players.Keys.Contains(x.Player)).OrderByDescending(x => x.Points))
                        {
                            list.Add(score.Player);
                        }
                        for (var i = 0; i < list.Count; i++)
                        {
                            if (Tournament.HeadToHeadPoints.ContainsKey(i + 1))
                                Players[list[i]] += Tournament.HeadToHeadPoints[i + 1];
                        }
                    }
                    if (TeamMode == TeamModes.BattleRoyale)
                    {
                        var teams = new Dictionary<Team, int>();
                        foreach (var team in TeamsBR.Where(x => x.Value > 0))
                            teams.Add(team.Key, 0);
                        foreach (var score in game.Scores)
                        {
                            foreach (var team in teams)
                            {
                                if (team.Key.Players.Contains(score.Player))
                                {
                                    teams[team.Key] += score.Points;
                                }
                            }
                        }

                        var lastTeam = teams.OrderByDescending(x => x.Value).Last();
                        TeamsBR[lastTeam.Key]--;
                    }
                }
                game.Counted = true;
                game.Warmup = WarmupMode;
            }
            Save();
        }
    }
}
