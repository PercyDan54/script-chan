using Caliburn.Micro;
using script_chan2.DataTypes;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;
using WK.Libraries.SharpClipboardNS;

namespace script_chan2.GUI
{
    public class MappoolListItemViewModel : Screen, IHandle<string>
    {
        #region Constructor
        public MappoolListItemViewModel(Mappool mappool)
        {
            this.mappool = mappool;
            Events.Aggregator.Subscribe(this);
        }
        #endregion

        #region Properties
        private Mappool mappool;

        public string Name
        {
            get { return mappool.Name; }
        }

        public bool HasTournament
        {
            get { return mappool.Tournament != null; }
        }

        public string TournamentName
        {
            get
            {
                return mappool.Tournament.Name;
            }
        }
        #endregion

        #region Events
        public void Handle(string message)
        {
            if (message == "DeleteMappoolMap" || message == "MoveMappoolMap")
                NotifyOfPropertyChange(() => BeatmapsViews);
        }
        #endregion

        #region Edit mappool dialog
        private string editName;
        public string EditName
        {
            get { return editName; }
            set
            {
                if (value != editName)
                {
                    editName = value;
                    NotifyOfPropertyChange(() => EditName);
                    NotifyOfPropertyChange(() => EditMappoolSaveEnabled);
                }
            }
        }

        public BindableCollection<Tournament> Tournaments
        {
            get
            {
                var list = new BindableCollection<Tournament>();
                foreach (var tournament in Database.Database.Tournaments.OrderBy(x => x.Name))
                    list.Add(tournament);
                return list;
            }
        }

        private Tournament editTournament;
        public Tournament EditTournament
        {
            get { return editTournament; }
            set
            {
                if (value != editTournament)
                {
                    editTournament = value;
                    NotifyOfPropertyChange(() => EditTournament);
                    NotifyOfPropertyChange(() => EditMappoolSaveEnabled);
                }
            }
        }

        public bool EditMappoolSaveEnabled
        {
            get
            {
                if (string.IsNullOrEmpty(EditName))
                    return false;
                if (EditTournament == null)
                    return false;
                if (Database.Database.Teams.Any(x => x.Name == EditName && x.Tournament == EditTournament && x.Id != mappool.Id))
                    return false;
                return true;
            }
        }

        public void Edit()
        {
            Log.Information("MappoolListItemViewModel: edit dialog of mappool '{mappool}' open", mappool.Name);
            EditName = mappool.Name;
            EditTournament = mappool.Tournament;
        }

        public void Save()
        {
            if (EditMappoolSaveEnabled)
            {
                Log.Information("MappoolListItemViewModel: edit dialog of mappool '{mappool}' save", mappool.Name);
                mappool.Name = EditName;
                mappool.Tournament = EditTournament;
                mappool.Save();
                NotifyOfPropertyChange(() => Name);
                NotifyOfPropertyChange(() => TournamentName);
            }
        }
        #endregion

        #region Beatmap list dialog
        public BindableCollection<MappoolBeatmapListItemViewModel> BeatmapsViews
        {
            get
            {
                var list = new BindableCollection<MappoolBeatmapListItemViewModel>();
                foreach (var beatmap in mappool.Beatmaps)
                    list.Add(new MappoolBeatmapListItemViewModel(beatmap));
                return list;
            }
        }

        private SharpClipboard clipboard;

        public void EditMaps()
        {
            Log.Information("MappoolListItemViewModel: beatmap list dialog of mappool '{mappool}' open", mappool.Name);
            clipboard = new SharpClipboard();
            clipboard.ClipboardChanged += Clipboard_ClipboardChanged;
        }

        public void EditMapsClose()
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
                if (!string.IsNullOrEmpty(BeatmapId))
                    BeatmapId += ";";
                BeatmapId += text;
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
                    var beatmap = Database.Database.GetBeatmap(actualId);
                    if (beatmap != null)
                    {
                        var mappoolMap = new MappoolMap()
                        {
                            Mappool = mappool,
                            Beatmap = beatmap
                        };
                        mappool.AddBeatmap(mappoolMap);
                        mappoolMap.Save();
                    }
                }
            }
            BeatmapId = "";
            NotifyOfPropertyChange(() => BeatmapsViews);
        }

        public void BeatmapIdKeyDown(ActionExecutionContext context)
        {
            var keyArgs = context.EventArgs as KeyEventArgs;
            if (keyArgs != null && keyArgs.Key == Key.Enter)
                AddBeatmap();
        }
        #endregion

        public void Delete()
        {
            Log.Information("MappoolListItemViewModel: delete mappool '{mappool}'", mappool.Name);
            mappool.Delete();
            Events.Aggregator.PublishOnUIThread("DeleteMappool");
        }
    }
}
