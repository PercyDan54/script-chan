using Caliburn.Micro;
using MaterialDesignThemes.Wpf;
using script_chan2.DataTypes;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using WK.Libraries.SharpClipboardNS;

namespace script_chan2.GUI
{
    public class MappoolBeatmapsDialogViewModel : Screen, IHandle<string>
    {
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

        private SharpClipboard clipboard;

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
        #endregion

        #region Actions
        public void Activate()
        {
            Log.Information("MappoolListItemViewModel: beatmap list dialog of mappool '{mappool}' open", mappool.Name);
            BeatmapId = "";
            Clipboard.SetText("");
            clipboard = new SharpClipboard();
            clipboard.ClipboardChanged += Clipboard_ClipboardChanged;
        }

        public void Deactivate()
        {
            Log.Information("MappoolListItemViewModel: beatmap list dialog of mappool '{mappool}' close", mappool.Name);
            clipboard.ClipboardChanged -= Clipboard_ClipboardChanged;
        }

        private void Clipboard_ClipboardChanged(object sender, SharpClipboard.ClipboardChangedEventArgs e)
        {
            if (e.ContentType != SharpClipboard.ContentTypes.Text)
                return;

            var text = clipboard.ClipboardText;

            if (Regex.IsMatch(text, @"https://osu.ppy.sh/beatmapsets/\d*"))
                text = text.Split('/').Last();

            if (int.TryParse(text, out int id))
            {
                Log.Information("MappoolListItemViewModel: mappool beatmap list dialog clipboard event, found id {id}", id);
                AddBeatmapInternal(id);
            }
        }

        public void AddBeatmap()
        {
            if (string.IsNullOrEmpty(beatmapId))
                return;
            Log.Information("MappoolListItemViewModel: mappool beatmap list dialog: add beatmaps '{beatmaps}'", beatmapId);
            var beatmapIds = beatmapId.Split(';');
            foreach (var id in beatmapIds)
            {
                if (int.TryParse(id, out int actualId))
                {
                    AddBeatmapInternal(actualId);
                }
            }
            BeatmapId = "";
        }

        private void AddBeatmapInternal(int id)
        {
            var beatmap = Database.Database.GetBeatmap(id);
            if (beatmap != null)
            {
                var mappoolMap = new MappoolMap()
                {
                    Mappool = mappool,
                    Beatmap = beatmap
                };
                mappool.AddBeatmap(mappoolMap);
                mappoolMap.Save();
                NotifyOfPropertyChange(() => BeatmapViews);
            }
        }

        public void DialogEscape()
        {
            DialogHost.CloseDialogCommand.Execute(false, null);
        }
        #endregion
    }
}
