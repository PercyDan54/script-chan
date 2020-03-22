using Caliburn.Micro;
using script_chan2.DataTypes;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace script_chan2.GUI
{
    public class MatchBeatmapViewModel : Screen, IHandle<string>
    {
        #region Constructor
        public MatchBeatmapViewModel(Match match, MappoolMap beatmap)
        {
            this.match = match;
            this.beatmap = beatmap;
            Events.Aggregator.Subscribe(this);
        }
        #endregion

        #region Events
        public void Handle(string message)
        {
            if (message == "UpdateColors")
            {
                NotifyOfPropertyChange(() => Background);
            }
        }
        #endregion

        #region Properties
        private Match match;

        private MappoolMap beatmap;

        public string Tag
        {
            get { return beatmap.Tag; }
        }

        public string Name
        {
            get { return $"{beatmap.Beatmap.Artist} - {beatmap.Beatmap.Title} [{beatmap.Beatmap.Version}] ({beatmap.Beatmap.Creator})  BPM{beatmap.Beatmap.BPM} AR{beatmap.Beatmap.AR} CS{beatmap.Beatmap.CS}"; }
        }

        public string ToolTip
        {
            get
            {
                return "Mapper: " + beatmap.Beatmap.Creator + Environment.NewLine
                    + "BPM: " + beatmap.Beatmap.BPM + Environment.NewLine
                    + "AR: " + beatmap.Beatmap.AR + Environment.NewLine
                    + "CS: " + beatmap.Beatmap.CS;
            }
        }

        public Brush Background
        {
            get
            {
                var brush = new LinearGradientBrush();
                for (var i = 0; i < beatmap.Mods.Count; i++)
                {
                    Brush brushToAdd = Brushes.White;
                    switch (beatmap.Mods[i])
                    {
                        case Enums.GameMods.Hidden: brushToAdd = new SolidColorBrush(Settings.UserColors.First(x => x.Key == "HD").Color); break;
                        case Enums.GameMods.HardRock: brushToAdd = new SolidColorBrush(Settings.UserColors.First(x => x.Key == "HR").Color); break;
                        case Enums.GameMods.DoubleTime: brushToAdd = new SolidColorBrush(Settings.UserColors.First(x => x.Key == "DT").Color); break;
                        case Enums.GameMods.Flashlight: brushToAdd = new SolidColorBrush(Settings.UserColors.First(x => x.Key == "FL").Color); break;
                        case Enums.GameMods.Freemod: brushToAdd = new SolidColorBrush(Settings.UserColors.First(x => x.Key == "Freemod").Color); break;
                        case Enums.GameMods.TieBreaker: brushToAdd = new SolidColorBrush(Settings.UserColors.First(x => x.Key == "Tiebreaker").Color); break;
                        case Enums.GameMods.NoFail: brushToAdd = new SolidColorBrush(Settings.UserColors.First(x => x.Key == "NoFail").Color); break;
                    }
                    brush.GradientStops.Add(new GradientStop(((SolidColorBrush)brushToAdd).Color, 1f / (beatmap.Mods.Count - 1) * i));
                }
                return brush;
            }
        }

        public Visibility CanUnban
        {
            get
            {
                if (match.Bans.Any(x => x.Map == beatmap))
                    return Visibility.Visible;
                return Visibility.Collapsed;
            }
        }

        public Visibility CanBanOrPick
        {
            get
            {
                if (match.Bans.Any(x => x.Map == beatmap) || match.Picks.Any(x => x.Map == beatmap))
                    return Visibility.Collapsed;
                return Visibility.Visible;
            }
        }

        public Visibility CanUnpick
        {
            get
            {
                if (match.Picks.Any(x => x.Map == beatmap))
                    return Visibility.Visible;
                return Visibility.Collapsed;
            }
        }

        public TextDecorationCollection TextDecoration
        {
            get
            {
                if (match.Bans.Any(x => x.Map == beatmap))
                    return TextDecorations.Strikethrough;
                return null;
            }
        }

        public Brush FontColor
        {
            get
            {
                if (match.TeamMode == Enums.TeamModes.TeamVS && match.Bans.Any(x => x.Map == beatmap))
                {
                    if (match.Bans.First(x => x.Map == beatmap).Team == match.TeamRed)
                        return Brushes.Red;
                    return Brushes.Blue;
                }
                if (match.TeamMode == Enums.TeamModes.TeamVS && match.Picks.Any(x => x.Map == beatmap))
                {
                    if (match.Picks.First(x => x.Map == beatmap).Team == match.TeamRed)
                        return Brushes.Red;
                    return Brushes.Blue;
                }
                return Brushes.Black;
            }
        }

        public string TeamRedName
        {
            get { return match.TeamRed.Name; }
        }

        public string TeamBlueName
        {
            get { return match.TeamBlue.Name; }
        }
        #endregion

        #region Actions
        public void BanRed()
        {
            Log.Information("MatchBeatmapViewModel: match '{match}' ban map by red", match.Name);
            match.Bans.Add(new MatchPick()
            {
                Match = match,
                Map = beatmap,
                Team = match.TeamRed
            });
            match.Save();
            NotifyOfPropertyChange(() => CanBanOrPick);
            NotifyOfPropertyChange(() => CanUnban);
            NotifyOfPropertyChange(() => TextDecoration);
            NotifyOfPropertyChange(() => FontColor);
        }

        public void BanBlue()
        {
            Log.Information("MatchBeatmapViewModel: match '{match}' ban map by blue", match.Name);
            match.Bans.Add(new MatchPick()
            {
                Match = match,
                Map = beatmap,
                Team = match.TeamBlue
            });
            match.Save();
            NotifyOfPropertyChange(() => CanBanOrPick);
            NotifyOfPropertyChange(() => CanUnban);
            NotifyOfPropertyChange(() => TextDecoration);
            NotifyOfPropertyChange(() => FontColor);
        }

        public void RemoveBan()
        {
            Log.Information("MatchBeatmapViewModel: match '{match}' remove ban", match.Name);
            match.Bans.RemoveAll(x => x.Map == beatmap);
            match.Save();
            NotifyOfPropertyChange(() => CanBanOrPick);
            NotifyOfPropertyChange(() => CanUnban);
            NotifyOfPropertyChange(() => TextDecoration);
            NotifyOfPropertyChange(() => FontColor);
        }

        public void PickRed()
        {
            Log.Information("MatchBeatmapViewModel: match '{match}' pick map by red", match.Name);
            match.Picks.Add(new MatchPick()
            {
                Match = match,
                Map = beatmap,
                Team = match.TeamRed
            });
            match.Save();
            NotifyOfPropertyChange(() => CanBanOrPick);
            NotifyOfPropertyChange(() => CanUnpick);
            NotifyOfPropertyChange(() => FontColor);
            if (match.RoomId > 0)
                SendPickMessage();
        }

        public void PickBlue()
        {
            Log.Information("MatchBeatmapViewModel: match '{match}' pick map by blue", match.Name);
            match.Picks.Add(new MatchPick()
            {
                Match = match,
                Map = beatmap,
                Team = match.TeamBlue
            });
            match.Save();
            NotifyOfPropertyChange(() => CanBanOrPick);
            NotifyOfPropertyChange(() => CanUnpick);
            NotifyOfPropertyChange(() => FontColor);
            if (match.RoomId > 0)
                SendPickMessage();
        }

        public void RemovePick()
        {
            Log.Information("MatchBeatmapViewModel: match '{match}' remove pick", match.Name);
            match.Picks.RemoveAll(x => x.Map == beatmap);
            match.Save();
            NotifyOfPropertyChange(() => CanBanOrPick);
            NotifyOfPropertyChange(() => CanUnpick);
            NotifyOfPropertyChange(() => FontColor);
        }

        private void SendPickMessage()
        {
            var mods = Utils.ConvertGameModsToString(beatmap.Mods);
            if (match.AllPicksFreemod && !mods.Contains("Freemod"))
                mods += " Freemod";
            OsuIrc.OsuIrc.SendMessage("#mp_" + match.RoomId, "!mp mods " + mods);
            var data = new ChannelMessageData()
            {
                Channel = "#mp_" + match.RoomId,
                User = Settings.IrcUsername,
                Message = "!mp mods " + mods
            };
            Events.Aggregator.PublishOnUIThread(data);
            OsuIrc.OsuIrc.SendMessage("#mp_" + match.RoomId, $"!mp map {beatmap.Beatmap.Id} {(int)match.GameMode}");
            data = new ChannelMessageData()
            {
                Channel = "#mp_" + match.RoomId,
                User = Settings.IrcUsername,
                Message = $"!mp map {beatmap.Beatmap.Id} {(int)match.GameMode}"
            };
            Events.Aggregator.PublishOnUIThread(data);
        }
        #endregion
    }
}
