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
    public class TournamentsViewModel : Screen, IHandle<string>
    {
        public BindableCollection<Tournament> Tournaments { get; set; }

        public BindableCollection<TournamentListItemViewModel> TournamentsViews
        {
            get
            {
                var list = new BindableCollection<TournamentListItemViewModel>();
                foreach (var tournament in Tournaments)
                    list.Add(new TournamentListItemViewModel(tournament));
                return list;
            }
        }

        protected override void OnActivate()
        {
            Reload();
            Events.Aggregator.Subscribe(this);
        }

        public void Reload()
        {
            Tournaments = new BindableCollection<Tournament>();
            foreach (var tournament in Database.Database.Tournaments.OrderBy(x => x.Name))
            {
                Tournaments.Add(tournament);
            }
            NotifyOfPropertyChange(() => Tournaments);
            NotifyOfPropertyChange(() => TournamentsViews);
        }

        public void Handle(string message)
        {
            if (message.ToString() == "DeleteTournament")
                Reload();
        }

        private string newTournamentName;
        public string NewTournamentName
        {
            get { return newTournamentName; }
            set
            {
                if (value != newTournamentName)
                {
                    newTournamentName = value;
                    NotifyOfPropertyChange(() => NewTournamentName);
                    NotifyOfPropertyChange(() => NewTournamentSaveEnabled);
                }
            }
        }

        public List<GameModes> GameModesList
        {
            get { return Enum.GetValues(typeof(GameModes)).Cast<GameModes>().ToList(); }
        }

        private GameModes newTournamentGameMode;
        public GameModes NewTournamentGameMode
        {
            get { return newTournamentGameMode; }
            set
            {
                if (value != newTournamentGameMode)
                {
                    newTournamentGameMode = value;
                    NotifyOfPropertyChange(() => NewTournamentGameMode);
                }
            }
        }

        public List<TeamModes> TeamModesList
        {
            get { return Enum.GetValues(typeof(TeamModes)).Cast<TeamModes>().ToList(); }
        }

        private TeamModes newTournamentTeamMode;
        public TeamModes NewTournamentTeamMode
        {
            get { return newTournamentTeamMode; }
            set
            {
                if (value != newTournamentTeamMode)
                {
                    newTournamentTeamMode = value;
                    NotifyOfPropertyChange(() => NewTournamentTeamMode);
                }
            }
        }

        public List<WinConditions> WinConditionsList
        {
            get { return Enum.GetValues(typeof(WinConditions)).Cast<WinConditions>().ToList(); }
        }

        private WinConditions newTournamentWinCondition;
        public WinConditions NewTournamentWinCondition
        {
            get { return newTournamentWinCondition; }
            set
            {
                if (value != newTournamentWinCondition)
                {
                    newTournamentWinCondition = value;
                    NotifyOfPropertyChange(() => NewTournamentWinCondition);
                }
            }
        }

        private string newTournamentAcronym;
        public string NewTournamentAcronym
        {
            get { return newTournamentAcronym; }
            set
            {
                if (value != newTournamentAcronym)
                {
                    newTournamentAcronym = value;
                    NotifyOfPropertyChange(() => NewTournamentAcronym);
                    NotifyOfPropertyChange(() => NewTournamentSaveEnabled);
                }
            }
        }

        private int newTournamentTeamSize;
        public int NewTournamentTeamSize
        {
            get { return newTournamentTeamSize; }
            set
            {
                if (value != newTournamentTeamSize)
                {
                    newTournamentTeamSize = value;
                    NotifyOfPropertyChange(() => NewTournamentTeamSize);
                    NotifyOfPropertyChange(() => NewTournamentSaveEnabled);
                }
            }
        }

        private int newTournamentRoomSize;
        public int NewTournamentRoomSize
        {
            get { return newTournamentRoomSize; }
            set
            {
                if (value != newTournamentRoomSize)
                {
                    newTournamentRoomSize = value;
                    NotifyOfPropertyChange(() => NewTournamentRoomSize);
                    NotifyOfPropertyChange(() => NewTournamentSaveEnabled);
                }
            }
        }

        private int newTournamentPointsForSecondBan;
        public int NewTournamentPointsForSecondBan
        {
            get { return newTournamentPointsForSecondBan; }
            set
            {
                if (value != newTournamentPointsForSecondBan)
                {
                    newTournamentPointsForSecondBan = value;
                    NotifyOfPropertyChange(() => NewTournamentPointsForSecondBan);
                }
            }
        }

        private bool newTournamentAllPicksFreemod;
        public bool NewTournamentAllPicksFreemod
        {
            get { return newTournamentAllPicksFreemod; }
            set
            {
                if (value != newTournamentAllPicksFreemod)
                {
                    newTournamentAllPicksFreemod = value;
                    NotifyOfPropertyChange(() => NewTournamentAllPicksFreemod);
                }
            }
        }

        public bool NewTournamentSaveEnabled
        {
            get
            {
                if (string.IsNullOrEmpty(newTournamentName))
                    return false;
                if (Tournaments.Any(x => x.Name == newTournamentName))
                    return false;
                if (string.IsNullOrEmpty(newTournamentAcronym))
                    return false;
                if (newTournamentTeamSize < 1)
                    return false;
                if (newTournamentRoomSize < 1)
                    return false;
                return true;
            }
        }

        public void NewTournamentDialogOpened()
        {
            NewTournamentName = "";
            NewTournamentGameMode = GameModes.Standard;
            NewTournamentTeamMode = TeamModes.TeamVS;
            NewTournamentWinCondition = WinConditions.ScoreV2;
            NewTournamentAcronym = "";
            NewTournamentTeamSize = 4;
            NewTournamentRoomSize = 8;
            NewTournamentPointsForSecondBan = 0;
            NewTournamentAllPicksFreemod = false;
        }

        public void NewTournamentDialogClosed()
        {
            var tournament = new Tournament(NewTournamentName, NewTournamentGameMode, NewTournamentTeamMode, NewTournamentWinCondition, NewTournamentAcronym, NewTournamentTeamSize, NewTournamentRoomSize, NewTournamentPointsForSecondBan, NewTournamentAllPicksFreemod);
            tournament.Save();
            Reload();
        }
    }
}
