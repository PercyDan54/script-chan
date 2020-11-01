using Caliburn.Micro;
using MaterialDesignThemes.Wpf;
using script_chan2.DataTypes;
using Serilog;
using System.Linq;

namespace script_chan2.GUI
{
    public class MatchPickOverviewDialogViewModel : Screen, IHandle<string>
    {
        private ILogger localLog = Log.ForContext<MatchPickOverviewDialogViewModel>();

        private Match match;

        #region Constructor
        public MatchPickOverviewDialogViewModel(Match match, double windowHeight)
        {
            this.match = match;
            WindowHeight = windowHeight - 80;
            Events.Aggregator.Subscribe(this);
        }

        public void OnDeactivate()
        {
            Events.Aggregator.Unsubscribe(this);
        }
        #endregion

        #region Events
        public void Handle(string message)
        {
            if (message == "MapPicked" || message == "MapBanned" || message == "MapUnpicked" || message == "MapUnbanned")
            {
                NotifyOfPropertyChange(() => BeatmapsViews);
                NotifyOfPropertyChange(() => PicksViews);
                NotifyOfPropertyChange(() => BansViews);
            }
        }
        #endregion

        #region Lists
        public BindableCollection<MatchBeatmapViewModel> BeatmapsViews
        {
            get
            {
                var list = new BindableCollection<MatchBeatmapViewModel>();
                if (match.Mappool != null)
                {
                    foreach (var beatmap in match.Mappool.Beatmaps.OrderBy(x => x.ListIndex))
                    {
                        if (match.Picks.Any(x => x.Map == beatmap))
                            continue;
                        if (match.Bans.Any(x => x.Map == beatmap))
                            continue;
                        list.Add(new MatchBeatmapViewModel(match, beatmap, false));
                    }
                }
                return list;
            }
        }

        public BindableCollection<MatchBeatmapViewModel> PicksViews
        {
            get
            {
                var list = new BindableCollection<MatchBeatmapViewModel>();
                if (match.Mappool != null)
                {
                    foreach (var pick in match.Picks.OrderBy(x => x.ListIndex))
                    {
                        list.Add(new MatchBeatmapViewModel(match, pick.Map, false));
                    }
                }
                return list;
            }
        }

        public BindableCollection<MatchBeatmapViewModel> BansViews
        {
            get
            {
                var list = new BindableCollection<MatchBeatmapViewModel>();
                if (match.Mappool != null)
                {
                    foreach (var ban in match.Bans.OrderBy(x => x.ListIndex))
                    {
                        list.Add(new MatchBeatmapViewModel(match, ban.Map, false));
                    }
                }
                return list;
            }
        }
        #endregion

        #region Properties
        private double windowHeight;

        public double WindowHeight
        {
            get { return windowHeight; }
            set
            {
                if (value != windowHeight)
                {
                    windowHeight = value;
                    NotifyOfPropertyChange(() => WindowHeight);
                }
            }
        }
        #endregion

        #region Actions
        public void DialogEscape()
        {
            DialogHost.CloseDialogCommand.Execute(true, null);
        }
        #endregion
    }
}
