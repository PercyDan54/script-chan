using Caliburn.Micro;
using MaterialDesignThemes.Wpf;
using script_chan2.DataTypes;
using script_chan2.Enums;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace script_chan2.GUI
{
    public class EditTournamentDialogViewModel : Screen
    {
        #region Constructor
        public EditTournamentDialogViewModel(int id = 0)
        {
            this.id = id;
            if (id > 0)
            {
                var tournament = Database.Database.Tournaments.First(x => x.Id == id);
                Name = tournament.Name;
                Acronym = tournament.Acronym;
                RoomSize = tournament.RoomSize;
                TeamSize = tournament.TeamSize;
                GameMode = tournament.GameMode;
                WinCondition = tournament.WinCondition;
                TeamMode = tournament.TeamMode;
                PointsForSecondBan = tournament.PointsForSecondBan;
                AllPicksFreemod = tournament.AllPicksFreemod;
                MpTimerCommand = tournament.MpTimerCommand;
                MpTimerAfterGame = tournament.MpTimerAfterGame;
                MpTimerAfterPick = tournament.MpTimerAfterPick;
                WelcomeString = tournament.WelcomeString;
                HeadToHeadPoints = new Dictionary<int, int>();
                foreach (var headToHeadPoint in tournament.HeadToHeadPoints)
                    HeadToHeadPoints.Add(headToHeadPoint.Key, headToHeadPoint.Value);
                if (HeadToHeadPoints.Count > 0)
                    HeadToHeadPointPlace = HeadToHeadPoints.Keys.Max() + 1;
                HeadToHeadPointPoints = 0;
            }
            else
            {
                Name = "";
                Acronym = "";
                RoomSize = 8;
                TeamSize = 4;
                GameMode = GameModes.Standard;
                WinCondition = WinConditions.ScoreV2;
                TeamMode = TeamModes.TeamVS;
                PointsForSecondBan = 0;
                AllPicksFreemod = false;
                MpTimerCommand = Settings.DefaultTimerCommand;
                MpTimerAfterGame = Settings.DefaultTimerAfterGame;
                MpTimerAfterPick = Settings.DefaultTimerAfterPick;
                WelcomeString = "";
                HeadToHeadPoints = new Dictionary<int, int>();
                HeadToHeadPointPlace = 1;
                HeadToHeadPointPoints = 0;
            }
        }
        #endregion

        #region Properties
        private int id;

        public bool SaveEnabled
        {
            get
            {
                if (string.IsNullOrEmpty(Name))
                    return false;
                if (Database.Database.Tournaments.Any(x => x.Name == Name && x.Id != id))
                    return false;
                if (string.IsNullOrEmpty(Acronym))
                    return false;
                if (TeamSize < 1)
                    return false;
                if (RoomSize < 1)
                    return false;
                return true;
            }
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
                    NotifyOfPropertyChange(() => Name);
                    NotifyOfPropertyChange(() => SaveEnabled);
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
                    NotifyOfPropertyChange(() => Acronym);
                    NotifyOfPropertyChange(() => SaveEnabled);
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
                    NotifyOfPropertyChange(() => RoomSize);
                    NotifyOfPropertyChange(() => SaveEnabled);
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
                    NotifyOfPropertyChange(() => TeamSize);
                    NotifyOfPropertyChange(() => SaveEnabled);
                }
            }
        }

        public List<GameModes> GameModesList
        {
            get { return Enum.GetValues(typeof(GameModes)).Cast<GameModes>().ToList(); }
        }

        private GameModes gameMode;
        public GameModes GameMode
        {
            get { return gameMode; }
            set
            {
                if (value != gameMode)
                {
                    gameMode = value;
                    NotifyOfPropertyChange(() => GameMode);
                }
            }
        }

        public List<WinConditions> WinConditionsList
        {
            get { return Enum.GetValues(typeof(WinConditions)).Cast<WinConditions>().ToList(); }
        }

        private WinConditions winCondition;
        public WinConditions WinCondition
        {
            get { return winCondition; }
            set
            {
                if (value != winCondition)
                {
                    winCondition = value;
                    NotifyOfPropertyChange(() => WinCondition);
                }
            }
        }

        public List<TeamModes> TeamModesList
        {
            get { return Enum.GetValues(typeof(TeamModes)).Cast<TeamModes>().ToList(); }
        }

        private TeamModes teamMode;
        public TeamModes TeamMode
        {
            get { return teamMode; }
            set
            {
                if (value != teamMode)
                {
                    teamMode = value;
                    NotifyOfPropertyChange(() => TeamMode);
                    NotifyOfPropertyChange(() => HeadToHeadVisible);
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
                    NotifyOfPropertyChange(() => PointsForSecondBan);
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
                    NotifyOfPropertyChange(() => AllPicksFreemod);
                }
            }
        }

        private int mpTimerCommand;
        public int MpTimerCommand
        {
            get { return mpTimerCommand; }
            set
            {
                if (value != mpTimerCommand)
                {
                    mpTimerCommand = value;
                    NotifyOfPropertyChange(() => MpTimerCommand);
                }
            }
        }

        private int mpTimerAfterGame;
        public int MpTimerAfterGame
        {
            get { return mpTimerAfterGame; }
            set
            {
                if (value != mpTimerAfterGame)
                {
                    mpTimerAfterGame = value;
                    NotifyOfPropertyChange(() => MpTimerAfterGame);
                }
            }
        }

        private int mpTimerAfterPick;
        public int MpTimerAfterPick
        {
            get { return mpTimerAfterPick; }
            set
            {
                if (value != mpTimerAfterPick)
                {
                    mpTimerAfterPick = value;
                    NotifyOfPropertyChange(() => MpTimerAfterPick);
                }
            }
        }

        private string welcomeString;
        public string WelcomeString
        {
            get { return welcomeString; }
            set
            {
                if (value != welcomeString)
                {
                    welcomeString = value;
                    NotifyOfPropertyChange(() => WelcomeString);
                }
            }
        }

        public Visibility HeadToHeadVisible
        {
            get
            {
                if (TeamMode == TeamModes.HeadToHead)
                    return Visibility.Visible;
                return Visibility.Collapsed;
            }
        }

        public Dictionary<int, int> HeadToHeadPoints;
        public BindableCollection<TournamentPointListItemViewModel> HeadToHeadPointViews
        {
            get
            {
                var list = new BindableCollection<TournamentPointListItemViewModel>();
                foreach (var point in HeadToHeadPoints.OrderBy(x => x.Key))
                    list.Add(new TournamentPointListItemViewModel(point.Key, point.Value));
                return list;
            }
        }

        private int headToHeadPointPlace;
        public int HeadToHeadPointPlace
        {
            get { return headToHeadPointPlace; }
            set
            {
                if (value != headToHeadPointPlace)
                {
                    headToHeadPointPlace = value;
                    NotifyOfPropertyChange(() => HeadToHeadPointPlace);
                    NotifyOfPropertyChange(() => AddPointsEnabled);
                }
            }
        }

        private int headToHeadPointPoints;
        public int HeadToHeadPointPoints
        {
            get { return headToHeadPointPoints; }
            set
            {
                if (value != headToHeadPointPoints)
                {
                    headToHeadPointPoints = value;
                    NotifyOfPropertyChange(() => HeadToHeadPointPoints);
                    NotifyOfPropertyChange(() => AddPointsEnabled);
                }
            }
        }

        public bool AddPointsEnabled
        {
            get
            {
                if (HeadToHeadPointPlace <= 0)
                    return false;
                if (headToHeadPointPoints <= 0)
                    return false;
                if (HeadToHeadPoints.ContainsKey(HeadToHeadPointPlace))
                    return false;
                return true;
            }
        }
        #endregion

        #region Actions
        public void AddPoints()
        {
            HeadToHeadPoints.Add(HeadToHeadPointPlace, HeadToHeadPointPoints);
            HeadToHeadPointPlace = HeadToHeadPoints.Keys.Max() + 1;
            HeadToHeadPointPoints = 0;
            NotifyOfPropertyChange(() => HeadToHeadPointViews);
        }

        public void RemovePoints(TournamentPointListItemViewModel model)
        {
            Log.Information("EditTournamentDialogViewModel: remove points");
            HeadToHeadPoints.Remove(model.Place);
            NotifyOfPropertyChange(() => HeadToHeadPointViews);
        }

        public void DialogEscape()
        {
            DialogHost.CloseDialogCommand.Execute(false, null);
        }
        #endregion
    }
}