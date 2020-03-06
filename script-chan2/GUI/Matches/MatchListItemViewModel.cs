using Caliburn.Micro;
using script_chan2.DataTypes;
using script_chan2.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace script_chan2.GUI
{
    class MatchListItemViewModel : Screen
    {
        public MatchListItemViewModel(Match match)
        {
            this.match = match;
            Events.Aggregator.Subscribe(this);
        }

        private Match match;

        public BindableCollection<Tournament> Tournaments
        {
            get
            {
                var list = new BindableCollection<Tournament>();
                foreach (var tournament in Database.Database.Tournaments.OrderBy(x => x.Id))
                    list.Add(tournament);
                return list;
            }
        }

        public BindableCollection<Mappool> Mappools
        {
            get
            {
                var list = new BindableCollection<Mappool>();
                foreach (var mappool in Database.Database.Mappools.OrderBy(x => x.Name))
                {
                    if (mappool.Tournament != EditMatchTournament)
                        continue;
                    list.Add(mappool);
                }
                return list;
            }
        }

        public string Name
        {
            get { return match.Name; }
        }

        public bool HasTournament
        {
            get { return match.Tournament != null; }
        }

        public string TournamentName
        {
            get
            {
                if (match.Tournament == null)
                    return "";
                return match.Tournament.Name;
            }
        }

        private string editMatchName;
        public string EditMatchName
        {
            get { return editMatchName; }
            set
            {
                if (value != editMatchName)
                {
                    editMatchName = value;
                    NotifyOfPropertyChange(() => EditMatchName);
                    NotifyOfPropertyChange(() => EditMatchSaveEnabled);
                }
            }
        }

        private Tournament editMatchTournament;
        public Tournament EditMatchTournament
        {
            get { return editMatchTournament; }
            set
            {
                if (value != editMatchTournament)
                {
                    editMatchTournament = value;
                    NotifyOfPropertyChange(() => EditMatchTournament);
                    NotifyOfPropertyChange(() => Mappools);
                    if (value != null)
                    {
                        EditMatchGameMode = value.GameMode;
                        EditMatchTeamMode = value.TeamMode;
                        EditMatchWinCondition = value.WinCondition;
                    }
                }
            }
        }

        private Mappool editMatchMappool;
        public Mappool EditMatchMappool
        {
            get { return editMatchMappool; }
            set
            {
                if (value != editMatchMappool)
                {
                    editMatchMappool = value;
                    NotifyOfPropertyChange(() => EditMatchMappool);
                }
            }
        }

        public List<GameModes> GameModesList
        {
            get { return Enum.GetValues(typeof(GameModes)).Cast<GameModes>().ToList(); }
        }

        private GameModes editMatchGameMode;
        public GameModes EditMatchGameMode
        {
            get { return editMatchGameMode; }
            set
            {
                if (value != editMatchGameMode)
                {
                    editMatchGameMode = value;
                    NotifyOfPropertyChange(() => EditMatchGameMode);
                }
            }
        }

        public List<TeamModes> TeamModesList
        {
            get { return Enum.GetValues(typeof(TeamModes)).Cast<TeamModes>().ToList(); }
        }

        private TeamModes editMatchTeamMode;
        public TeamModes EditMatchTeamMode
        {
            get { return editMatchTeamMode; }
            set
            {
                if (value != editMatchTeamMode)
                {
                    editMatchTeamMode = value;
                    NotifyOfPropertyChange(() => EditMatchTeamMode);
                }
            }
        }

        public List<WinConditions> WinConditionsList
        {
            get { return Enum.GetValues(typeof(WinConditions)).Cast<WinConditions>().ToList(); }
        }

        private WinConditions editMatchWinCondition;
        public WinConditions EditMatchWinCondition
        {
            get { return editMatchWinCondition; }
            set
            {
                if (value != editMatchWinCondition)
                {
                    editMatchWinCondition = value;
                    NotifyOfPropertyChange(() => EditMatchWinCondition);
                }
            }
        }

        private int editMatchBO;
        public int EditMatchBO
        {
            get { return editMatchBO; }
            set
            {
                if (value != editMatchBO)
                {
                    editMatchBO = value;
                    NotifyOfPropertyChange(() => EditMatchBO);
                }
            }
        }

        public bool EditMatchSaveEnabled
        {
            get
            {
                if (match.Status != MatchStatus.New)
                    return false;
                if (string.IsNullOrEmpty(editMatchName))
                    return false;
                return true;
            }
        }

        public void Edit()
        {
            EditMatchName = match.Name;
            EditMatchTournament = match.Tournament;
            EditMatchMappool = match.Mappool;
            EditMatchGameMode = match.GameMode;
            EditMatchTeamMode = match.TeamMode;
            EditMatchWinCondition = match.WinCondition;
            EditMatchBO = match.BO;
        }

        public void Save()
        {
            if (EditMatchSaveEnabled)
            {
                match.Name = EditMatchName;
                match.Tournament = EditMatchTournament;
                match.Mappool = EditMatchMappool;
                match.GameMode = EditMatchGameMode;
                match.TeamMode = EditMatchTeamMode;
                match.WinCondition = EditMatchWinCondition;
                match.BO = EditMatchBO;
                match.Save();
                NotifyOfPropertyChange(() => Name);
                Events.Aggregator.PublishOnUIThread("EditMatch");
            }
        }

        public void Delete()
        {
            match.Delete();
            Events.Aggregator.PublishOnUIThread("DeleteMatch");
        }
    }
}
