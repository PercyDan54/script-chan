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
        public Tournament(string name, Enums.GameModes gameMode, Enums.TeamModes teamMode, Enums.WinConditions winCondition, string acronym, int teamSize, int roomSize, int pointsForSecondBan, bool allPicksFreemod, int id = 0)
        {
            Webhooks = new List<Webhook>();
            this.name = name;
            this.gameMode = gameMode;
            this.teamMode = teamMode;
            this.winCondition = winCondition;
            this.acronym = acronym;
            this.teamSize = teamSize;
            this.roomSize = roomSize;
            this.pointsForSecondBan = pointsForSecondBan;
            this.allPicksFreemod = allPicksFreemod;
            this.id = id;
        }

        private int id;
        public int Id
        {
            get { return id; }
        }

        private string name;
        public string Name
        {
            get { return name; }
            set
            {
                if (value != name)
                {
                    name = value;
                }
            }
        }

        private Enums.GameModes gameMode;
        public Enums.GameModes GameMode
        {
            get { return gameMode; }
            set
            {
                if (value != gameMode)
                {
                    gameMode = value;
                }
            }
        }

        private Enums.TeamModes teamMode;
        public Enums.TeamModes TeamMode
        {
            get { return teamMode; }
            set
            {
                if (value != teamMode)
                {
                    teamMode = value;
                }
            }
        }

        private Enums.WinConditions winCondition;
        public Enums.WinConditions WinCondition
        {
            get { return winCondition; }
            set
            {
                if (value != winCondition)
                {
                    winCondition = value;
                }
            }
        }

        private string acronym;
        public string Acronym
        {
            get { return acronym; }
            set
            {
                if (value != acronym)
                {
                    acronym = value;
                }
            }
        }

        private int teamSize;
        public int TeamSize
        {
            get { return teamSize; }
            set
            {
                if (value != teamSize)
                {
                    teamSize = value;
                }
            }
        }

        private int roomSize;
        public int RoomSize
        {
            get { return roomSize; }
            set
            {
                if (value != roomSize)
                {
                    roomSize = value;
                }
            }
        }

        private int pointsForSecondBan;
        public int PointsForSecondBan
        {
            get { return pointsForSecondBan; }
            set
            {
                if (value != pointsForSecondBan)
                {
                    pointsForSecondBan = value;
                }
            }
        }

        private bool allPicksFreemod;
        public bool AllPicksFreemod
        {
            get { return allPicksFreemod; }
            set
            {
                if (value != allPicksFreemod)
                {
                    allPicksFreemod = value;
                }
            }
        }

        public void Save()
        {
            if (id == 0)
                id = Database.Database.AddTournament(this);
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
