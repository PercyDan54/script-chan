using Caliburn.Micro;
using MaterialDesignThemes.Wpf;
using script_chan2.DataTypes;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace script_chan2.GUI
{
    public class MatchesWikiImportDialogViewModel : Screen
    {
        private ILogger localLog = Log.ForContext<MatchesWikiImportDialogViewModel>();

        #region Constructor
        public MatchesWikiImportDialogViewModel()
        {
            Tournament = Settings.DefaultTournament;
            ImportText = "";
        }
        #endregion

        #region Properties
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

        private Tournament tournament;
        public Tournament Tournament
        {
            get { return tournament; }
            set
            {
                if (value != tournament)
                {
                    tournament = value;
                    NotifyOfPropertyChange(() => Tournament);
                    NotifyOfPropertyChange(() => ImportEnabled);
                }
            }
        }

        public BindableCollection<Mappool> Mappools
        {
            get
            {
                var list = new BindableCollection<Mappool>();
                foreach (var mappool in Database.Database.Mappools.Where(x => x.Tournament == Tournament).OrderBy(x => x.Name))
                    list.Add(mappool);
                return list;
            }
        }

        private Mappool mappool;
        public Mappool Mappool
        {
            get { return mappool; }
            set
            {
                if (value != mappool)
                {
                    mappool = value;
                    NotifyOfPropertyChange(() => Mappool);
                }
            }
        }

        private string importText;
        public string ImportText
        {
            get { return importText; }
            set
            {
                if (value != importText)
                {
                    importText = value;
                    NotifyOfPropertyChange(() => ImportText);
                    NotifyOfPropertyChange(() => ImportEnabled);
                }
            }
        }

        public bool ImportEnabled
        {
            get
            {
                if (string.IsNullOrEmpty(ImportText))
                    return false;
                if (Tournament == null)
                    return false;
                if (IsImporting)
                    return false;
                return true;
            }
        }

        private bool isImporting;
        public bool IsImporting
        {
            get { return isImporting; }
            set
            {
                if (value != isImporting)
                {
                    isImporting = value;
                    NotifyOfPropertyChange(() => ImportEnabled);
                    NotifyOfPropertyChange(() => CloseEnabled);
                    NotifyOfPropertyChange(() => ProgressVisible);
                    NotifyOfPropertyChange(() => ImportTextVisible);
                }
            }
        }

        public bool CloseEnabled
        {
            get { return !IsImporting; }
        }

        private int matchCount;
        public int MatchCount
        {
            get { return matchCount; }
            set
            {
                if (value != matchCount)
                {
                    matchCount = value;
                    NotifyOfPropertyChange(() => MatchCount);
                }
            }
        }

        private int importProgress;
        public int ImportProgress
        {
            get { return importProgress; }
            set
            {
                if (value != importProgress)
                {
                    importProgress = value;
                    NotifyOfPropertyChange(() => ImportProgress);
                }
            }
        }

        public Visibility ProgressVisible
        {
            get
            {
                if (IsImporting)
                    return Visibility.Visible;
                return Visibility.Collapsed;
            }
        }

        public Visibility ImportTextVisible
        {
            get
            {
                if (!IsImporting)
                    return Visibility.Visible;
                return Visibility.Collapsed;
            }
        }

        private string importStatus;
        public string ImportStatus
        {
            get { return importStatus; }
            set
            {
                if (value != importStatus)
                {
                    importStatus = value;
                    NotifyOfPropertyChange(() => ImportStatus);
                }
            }
        }
        #endregion

        #region Actions
        public void StartImport()
        {
            localLog.Information("start import");
            IsImporting = true;
            ImportStatus = "Parsing text";

            var importMatches = new List<ImportMatch>();

            var rootSplit = ImportText.Split('\n');
            foreach (var line in rootSplit)
            {
                var trimmedText = line.Trim();
                var lineSplit = trimmedText.Split('\t');

                var match = new ImportMatch();
                match.TeamRed = Tournament.Teams.First(x => x.Name == lineSplit[0].Trim());
                match.TeamBlue = Tournament.Teams.First(x => x.Name == lineSplit[3].Trim());

                importMatches.Add(match);
            }

            MatchCount = importMatches.Count;

            for (var i = 0; i < importMatches.Count; i++)
            {
                ImportProgress = i;
                var importMatch = importMatches[i];
                ImportStatus = $"{importMatch.TeamRed.Name} vs {importMatch.TeamBlue.Name} ({i + 1}/{MatchCount})";

                var match = new Match()
                {
                    Name = $"{Tournament.Acronym}: ({importMatch.TeamRed.Name}) VS ({importMatch.TeamBlue.Name})",
                    Tournament = Tournament,
                    Mappool = Mappool,
                    TeamRed = importMatch.TeamRed,
                    TeamBlue = importMatch.TeamBlue,
                    BO = Settings.DefaultBO,
                    GameMode = Tournament.GameMode,
                    TeamMode = Tournament.TeamMode,
                    WinCondition = Tournament.WinCondition,
                    RoomSize = Tournament.RoomSize,
                    TeamSize = Tournament.TeamSize,
                    MpTimerCommand = Tournament.MpTimerCommand,
                    MpTimerAfterGame = Tournament.MpTimerAfterGame,
                    MpTimerAfterPick = Tournament.MpTimerAfterPick,
                    PointsForSecondBan = Tournament.PointsForSecondBan,
                    AllPicksFreemod = Tournament.AllPicksFreemod,
                    WarmupMode = true
                };
                match.Save();
            }

            Events.Aggregator.PublishOnUIThread("AddMatch");
            IsImporting = false;

            DialogHost.CloseDialogCommand.Execute(true, null);
        }

        public void DialogEscape()
        {
            if (!IsImporting)
                DialogHost.CloseDialogCommand.Execute(false, null);
        }
        #endregion

        private class ImportMatch
        {
            public Team TeamBlue;
            public Team TeamRed;
        }
    }
}
