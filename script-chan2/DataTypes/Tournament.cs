﻿using Serilog;
using System.Collections.Generic;
using System.Linq;

namespace script_chan2.DataTypes
{
    public class Tournament
    {
        private ILogger localLog = Log.ForContext<Tournament>();

        public Tournament(int id = 0)
        {
            Webhooks = new List<Webhook>();
            HeadToHeadPoints = new Dictionary<int, int>();
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

        public bool AllPicksNofail { get; set; }

        public int MpTimerCommand { get; set; }

        public int MpTimerAfterGame { get; set; }

        public int MpTimerAfterPick { get; set; }

        public string WelcomeString { get; set; }

        public Dictionary<int, int> HeadToHeadPoints { get; set; }

        public int BRInitialLivesAmount { get; set; }

        public void Save()
        {
            localLog.Information("'{name}' save", Name);
            if (Id == 0)
                Id = Database.Database.AddTournament(this);
            else
                Database.Database.UpdateTournament(this);
        }

        public List<Webhook> Webhooks;

        public void AddWebhook(Webhook webhook)
        {
            localLog.Information("'{name}' add webhook '{webhook}'", Name, webhook.Name);
            if (!Webhooks.Contains(webhook))
            {
                Webhooks.Add(webhook);
                Database.Database.AddWebhookToTournament(webhook, this);
            }
        }

        public void RemoveWebhook(Webhook webhook)
        {
            localLog.Information("'{name}' remove webhook '{webhook}'", Name, webhook.Name);
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

        public List<CustomCommand> CustomCommands
        {
            get { return Database.Database.CustomCommands.Where(x => x.Tournament == this).ToList(); }
        }

        public void Delete()
        {
            localLog.Information("'{name}' delete", Name);
            foreach (var match in Database.Database.Matches.Where(x => x.Tournament == this).ToList())
            {
                match.Delete();
            }
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
            foreach (var customCommand in CustomCommands.ToList())
            {
                customCommand.Delete();
            }
            if (Settings.DefaultTournament == this)
            {
                Settings.DefaultTournament = null;
            }
            Database.Database.DeleteTournament(this);
        }
    }
}
