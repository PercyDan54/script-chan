using Caliburn.Micro;
using MaterialDesignThemes.Wpf;
using script_chan2.DataTypes;
using Serilog;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;

namespace script_chan2.GUI
{
    public class MappoolBeatmapsDialogViewModel : Screen, IHandle<string>
    {
        private ILogger localLog = Log.ForContext<MappoolBeatmapsDialogViewModel>();

        #region Constructor
        public MappoolBeatmapsDialogViewModel(Mappool mappool)
        {
            this.mappool = mappool;
            Events.Aggregator.Subscribe(this);
        }
        #endregion

        #region Events
        public void Handle(string message)
        {
            if (message == "DeleteMappoolMap" || message == "MoveMappoolMap")
                NotifyOfPropertyChange(() => BeatmapViews);
        }
        #endregion

        #region Properties
        private Mappool mappool;

        public BindableCollection<MappoolBeatmapListItemViewModel> BeatmapViews
        {
            get
            {
                var list = new BindableCollection<MappoolBeatmapListItemViewModel>();
                foreach (var beatmap in mappool.Beatmaps)
                    list.Add(new MappoolBeatmapListItemViewModel(beatmap));
                return list;
            }
        }

        private string beatmapId;
        public string BeatmapId
        {
            get { return beatmapId; }
            set
            {
                if (value != beatmapId)
                {
                    beatmapId = value;
                    NotifyOfPropertyChange(() => BeatmapId);
                }
            }
        }

        private bool isAddingBeatmap;
        public bool IsAddingBeatmap
        {
            get { return isAddingBeatmap; }
            set
            {
                if (value != isAddingBeatmap)
                {
                    isAddingBeatmap = value;
                    NotifyOfPropertyChange(() => AddButtonVisible);
                    NotifyOfPropertyChange(() => AddProgressVisible);
                    NotifyOfPropertyChange(() => CloseButtonEnabled);
                }
            }
        }

        public Visibility AddButtonVisible
        {
            get
            {
                if (IsAddingBeatmap)
                    return Visibility.Collapsed;
                return Visibility.Visible;
            }
        }

        public Visibility AddProgressVisible
        {
            get
            {
                if (IsAddingBeatmap)
                    return Visibility.Visible;
                return Visibility.Collapsed;
            }
        }

        public bool CloseButtonEnabled
        {
            get { return !IsAddingBeatmap; }
        }
        #endregion

        #region Actions
        public void Activate()
        {
            localLog.Information("beatmap list dialog of mappool '{mappool}' open", mappool.Name);
            BeatmapId = "";
        }

        public void Deactivate()
        {
            localLog.Information("beatmap list dialog of mappool '{mappool}' close", mappool.Name);
        }

        public async Task AddBeatmap()
        {
            if (string.IsNullOrEmpty(beatmapId))
                return;

            localLog.Information("mappool beatmap list dialog: add beatmaps '{beatmaps}'", beatmapId);
            IsAddingBeatmap = true;
            var beatmapIds = beatmapId.Split(';');
            foreach (var id in beatmapIds)
            {
                if (int.TryParse(id, out int actualId))
                {
                    await AddBeatmapInternal(actualId);
                }
            }
            BeatmapId = "";
            IsAddingBeatmap = false;
        }

        private async Task AddBeatmapInternal(int id)
        {
            var beatmap = await Database.Database.GetBeatmap(id);
            if (beatmap != null)
            {
                var mappoolMap = new MappoolMap()
                {
                    Mappool = mappool,
                    Beatmap = beatmap,
                    PickCommand = true
                };
                mappool.AddBeatmap(mappoolMap);
                mappoolMap.Save();
                NotifyOfPropertyChange(() => BeatmapViews);
            }
        }

        public void DialogEscape()
        {
            if (!IsAddingBeatmap)
                DialogHost.CloseDialogCommand.Execute(false, null);
        }
        #endregion
    }
}
