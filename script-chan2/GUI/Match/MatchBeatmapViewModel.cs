using Caliburn.Micro;
using script_chan2.DataTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace script_chan2.GUI
{
    public class MatchBeatmapViewModel : Screen
    {
        #region Constructor
        public MatchBeatmapViewModel(Match match, MappoolMap beatmap)
        {
            this.match = match;
            this.beatmap = beatmap;
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
                        case Enums.GameMods.Hidden: brushToAdd = ModBrushes.HDLight; break;
                        case Enums.GameMods.HardRock: brushToAdd = ModBrushes.HRLight; break;
                        case Enums.GameMods.DoubleTime: brushToAdd = ModBrushes.DTLight; break;
                        case Enums.GameMods.Flashlight: brushToAdd = ModBrushes.FLLight; break;
                        case Enums.GameMods.Freemod: brushToAdd = ModBrushes.FreeModLight; break;
                        case Enums.GameMods.TieBreaker: brushToAdd = ModBrushes.TieBreakerLight; break;
                        case Enums.GameMods.NoFail: brushToAdd = ModBrushes.NoFailLight; break;
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
        #endregion

        #region Actions
        public void BanRed()
        {
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
            match.Bans.RemoveAll(x => x.Map == beatmap);
            match.Save();
            NotifyOfPropertyChange(() => CanBanOrPick);
            NotifyOfPropertyChange(() => CanUnban);
            NotifyOfPropertyChange(() => TextDecoration);
            NotifyOfPropertyChange(() => FontColor);
        }

        public void PickRed()
        {
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
