using Caliburn.Micro;
using script_chan2.DataTypes;
using script_chan2.Enums;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace script_chan2.GUI
{
    public class MatchViewModel : Screen
    {
        #region Lists
        public BindableCollection<MatchTeamViewModel> TeamsViews
        {
            get
            {
                var list = new BindableCollection<MatchTeamViewModel>();
                if (match.TeamMode == Enums.TeamModes.TeamVS)
                {
                    list.Add(new MatchTeamViewModel(match, Enums.TeamColors.Blue));
                    list.Add(new MatchTeamViewModel(match, Enums.TeamColors.Red));
                }
                return list;
            }
        }

        public BindableCollection<Mappool> Mappools
        {
            get
            {
                var list = new BindableCollection<Mappool>();
                foreach (var mappool in Database.Database.Mappools)
                {
                    if (mappool.Tournament != match.Tournament)
                        continue;
                    list.Add(mappool);
                }
                return list;
            }
        }

        public BindableCollection<Team> Teams
        {
            get
            {
                var list = new BindableCollection<Team>();
                if (match.TeamMode == Enums.TeamModes.TeamVS)
                {
                    list.Add(match.TeamBlue);
                    list.Add(match.TeamRed);
                }
                return list;
            }
        }

        public BindableCollection<MatchBeatmapViewModel> BeatmapsViews
        {
            get
            {
                var list = new BindableCollection<MatchBeatmapViewModel>();
                if (SelectedMappool != null)
                {
                    foreach (var beatmap in SelectedMappool.Beatmaps)
                    {
                        list.Add(new MatchBeatmapViewModel(this.match, beatmap));
                    }
                }
                return list;
            }
        }

        public List<GameModes> GameModesList
        {
            get { return Enum.GetValues(typeof(GameModes)).Cast<GameModes>().ToList(); }
        }
        #endregion

        #region Constructor
        public MatchViewModel(Match match)
        {
            this.match = match;

        }
        #endregion

        #region Events
        protected override void OnDeactivate(bool close)
        {
            Log.Information("GUI close match '{name}'", match.Name);
            MatchList.OpenedMatches.Remove(match);
        }
        #endregion

        #region Properties
        private Match match;

        public string WindowTitle
        {
            get { return match.Name; }
        }

        public Visibility TeamVsVisible
        {
            get
            {
                if (match.TeamMode == Enums.TeamModes.TeamVS)
                    return Visibility.Visible;
                return Visibility.Collapsed;
            }
        }

        public Visibility HeadToHeadVisible
        {
            get
            {
                if (match.TeamMode == Enums.TeamModes.HeadToHead)
                    return Visibility.Visible;
                return Visibility.Collapsed;
            }
        }

        public Mappool SelectedMappool
        {
            get { return match.Mappool; }
            set
            {
                if (value != match.Mappool)
                {
                    match.Mappool = value;
                    match.Save();
                    NotifyOfPropertyChange(() => SelectedMappool);
                    NotifyOfPropertyChange(() => BeatmapsViews);
                }
            }
        }

        public Team RollWinnerTeam
        {
            get { return match.RollWinnerTeam; }
            set
            {
                if (value != match.RollWinnerTeam)
                {
                    match.RollWinnerTeam = value;
                    match.Save();
                    NotifyOfPropertyChange(() => RollWinnerTeam);
                }
            }
        }

        public Player RollWinnerPlayer
        {
            get { return match.RollWinnerPlayer; }
            set
            {
                if (value != match.RollWinnerPlayer)
                {
                    match.RollWinnerPlayer = value;
                    match.Save();
                    NotifyOfPropertyChange(() => RollWinnerPlayer);
                }
            }
        }

        public GameModes GameMode
        {
            get { return match.GameMode; }
            set
            {
                if (value != match.GameMode)
                {
                    match.GameMode = value;
                    match.Save();
                    NotifyOfPropertyChange(() => GameMode);
                }
            }
        }

        public int BO
        {
            get { return match.BO; }
            set
            {
                if (value != match.BO)
                {
                    match.BO = value;
                    match.Save();
                    NotifyOfPropertyChange(() => BO);
                }
            }
        }

        public int RoomSize
        {
            get { return match.RoomSize; }
            set
            {
                if (value != match.RoomSize)
                {
                    match.RoomSize = value;
                    match.Save();
                    NotifyOfPropertyChange(() => RoomSize);
                }
            }
        }

        public int TeamSize
        {
            get { return match.TeamSize; }
            set
            {
                if (value != match.TeamSize)
                {
                    match.TeamSize = value;
                    match.Save();
                    NotifyOfPropertyChange(() => TeamSize);
                }
            }
        }

        public int TimerCommand
        {
            get { return match.MpTimerCommand; }
            set
            {
                if (value != match.MpTimerCommand)
                {
                    match.MpTimerCommand = value;
                    match.Save();
                    NotifyOfPropertyChange(() => TimerCommand);
                }
            }
        }

        public int TimerAfterGame
        {
            get { return match.MpTimerAfterGame; }
            set
            {
                if (value != match.MpTimerAfterGame)
                {
                    match.MpTimerAfterGame = value;
                    match.Save();
                    NotifyOfPropertyChange(() => TimerAfterGame);
                }
            }
        }

        public int TimerAfterPick
        {
            get { return match.MpTimerAfterPick; }
            set
            {
                if (value != match.MpTimerAfterPick)
                {
                    match.MpTimerAfterPick = value;
                    match.Save();
                    NotifyOfPropertyChange(() => TimerAfterPick);
                }
            }
        }

        public int PointsForSecondBan
        {
            get { return match.PointsForSecondBan; }
            set
            {
                if (value != match.PointsForSecondBan)
                {
                    match.PointsForSecondBan = value;
                    match.Save();
                    NotifyOfPropertyChange(() => PointsForSecondBan);
                }
            }
        }

        public bool AllPicksFreemod
        {
            get { return match.AllPicksFreemod; }
            set
            {
                if (value != match.AllPicksFreemod)
                {
                    match.AllPicksFreemod = value;
                    match.Save();
                    NotifyOfPropertyChange(() => AllPicksFreemod);
                }
            }
        }

        public bool EnableWebhooks
        {
            get { return match.EnableWebhooks; }
            set
            {
                if (value != match.EnableWebhooks)
                {
                    match.EnableWebhooks = value;
                    match.Save();
                    NotifyOfPropertyChange(() => EnableWebhooks);
                }
            }
        }
        #endregion
    }
}
