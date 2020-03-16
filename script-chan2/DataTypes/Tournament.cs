using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace script_chan2.DataTypes
{
    public class Tournament
    {
        public Tournament(string name, Enums.GameModes gameMode, Enums.TeamModes teamMode, Enums.WinConditions winCondition, string acronym, int teamSize, int roomSize, int pointsForSecondBan, bool allPicksFreemod, int mpTimerCommand, int mpTimerAfterGame, int mpTimerAfterPick, string welcomeString, int id = 0)
        {
            Webhooks = new List<Webhook>();
            Name = name;
            GameMode = gameMode;
            TeamMode = teamMode;
            WinCondition = winCondition;
            Acronym = acronym;
            TeamSize = teamSize;
            RoomSize = roomSize;
            PointsForSecondBan = pointsForSecondBan;
            AllPicksFreemod = allPicksFreemod;
            MpTimerCommand = mpTimerCommand;
            MpTimerAfterGame = mpTimerAfterGame;
            MpTimerAfterPick = mpTimerAfterPick;
            WelcomeString = welcomeString;
            Id = id;
        }

        public int Id { get; private set; }

        public string Name { get; set; }

        public Enums.GameModes GameMode { get; set; }

        public Enums.TeamModes TeamMode { get; set; }

        public Enums.WinConditions WinCondition { get; set; }

        public string Acronym { get; set; }

        public int TeamSize { get; set; }

        public int RoomSize { get; set; }

        public int PointsForSecondBan { get; set; }

        public bool AllPicksFreemod { get; set; }

        public int MpTimerCommand { get; set; }

        public int MpTimerAfterGame { get; set; }

        public int MpTimerAfterPick { get; set; }

        public string WelcomeString { get; set; }

        public void Save()
        {
            if (Id == 0)
                Id = Database.Database.AddTournament(this);
            else
                Database.Database.UpdateTournament(this);
        }

        public List<Webhook> Webhooks;

        public void AddWebhook(Webhook webhook)
        {
            if (!Webhooks.Contains(webhook))
            {
                Webhooks.Add(webhook);
                Database.Database.AddWebhookToTournament(webhook, this);
            }
        }

        public void RemoveWebhook(Webhook webhook)
        {
            if (Webhooks.Remove(webhook))
            {
                Database.Database.RemoveWebhookFromTournament(webhook, this);
            }
        }

        public List<Mappool> Mappools
        {
            get { return Database.Database.Mappools.Where(x => x.Tournament == this).ToList(); }
        }

        public List<Team> Teams
        {
            get { return Database.Database.Teams.Where(x => x.Tournament == this).ToList(); }
        }

        public void Delete()
        {
            foreach (var webhook in Webhooks.ToList())
            {
                RemoveWebhook(webhook);
            }
            foreach (var mappool in Mappools.ToList())
            {
                mappool.Delete();
            }
            foreach (var team in Teams.ToList())
            {
                team.Delete();
            }
            if (Settings.DefaultTournament == this)
            {
                Settings.DefaultTournament = null;
            }
            Database.Database.DeleteTournament(this);
        }
    }
}
