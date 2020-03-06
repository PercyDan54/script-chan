﻿using Caliburn.Micro;
using script_chan2.DataTypes;
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
        public MappoolListItemViewModel(Mappool mappool)
        {
            this.mappool = mappool;
            Events.Aggregator.Subscribe(this);
        }

        private Mappool mappool;

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

        public void Handle(string message)
        {
            if (message == "DeleteMappoolMap" || message == "MoveMappoolMap")
                NotifyOfPropertyChange(() => BeatmapsViews);
        }

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
                if (mappool.Tournament == null)
                    return "";
                return mappool.Tournament.Name;
            }
        }

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
                if (string.IsNullOrEmpty(editName))
                    return false;
                if (Database.Database.Teams.Any(x => x.Name == editName && x.Tournament == editTournament && x.Id != mappool.Id))
                    return false;
                return true;
            }
        }

        private SharpClipboard clipboard;

        public void EditMaps()
        {
            clipboard = new SharpClipboard();
            clipboard.ClipboardChanged += Clipboard_ClipboardChanged;
        }

        public void EditMapsClose()
        {
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
            var beatmapIds = beatmapId.Split(';');
            foreach (var id in beatmapIds)
            {
                if (int.TryParse(id, out int actualId))
                {
                    var beatmap = Database.Database.GetBeatmap(actualId);
                    if (beatmap != null)
                    {
                        var mappoolMap = new MappoolMap(mappool, beatmap, "");
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

        public void Edit()
        {
            EditName = mappool.Name;
            EditTournament = mappool.Tournament;
        }

        public void Save()
        {
            if (EditMappoolSaveEnabled)
            {
                mappool.Name = EditName;
                mappool.Tournament = EditTournament;
                mappool.Save();
                NotifyOfPropertyChange(() => Name);
                NotifyOfPropertyChange(() => TournamentName);
            }
        }

        public void Delete()
        {
            mappool.Delete();
            Events.Aggregator.PublishOnUIThread("DeleteMappool");
        }
    }
}
