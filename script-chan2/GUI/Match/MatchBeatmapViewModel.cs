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
        private ILogger localLog = Log.ForContext<MatchBeatmapViewModel>();

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

        public Visibility CanBanOrPickTeam
        {
            get
            {
                if (match.TeamMode != Enums.TeamModes.TeamVS)
                    return Visibility.Collapsed;
                if (match.Bans.Any(x => x.Map == beatmap) || match.Picks.Any(x => x.Map == beatmap))
                    return Visibility.Collapsed;
                return Visibility.Visible;
            }
        }

        public Visibility CanBanOrPickPlayer
        {
            get
            {
                if (match.TeamMode != Enums.TeamModes.HeadToHead)
                    return Visibility.Collapsed;
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
                if (match.TeamMode == Enums.TeamModes.HeadToHead && match.Picks.Any(x => x.Map == beatmap))
                    return Brushes.Green;
                return Brushes.Black;
            }
        }

        public string TeamRedName
        {
            get
            {
                if (match.TeamMode == Enums.TeamModes.TeamVS)
                    return match.TeamRed.Name;
                return "";
            }
        }

        public string TeamBlueName
        {
            get
            {
                if (match.TeamMode == Enums.TeamModes.TeamVS)
                    return match.TeamBlue.Name;
                return "";
            }
        }

        public BindableCollection<Player> Players
        {
            get
            {
                var list = new BindableCollection<Player>();
                if (match.TeamMode == Enums.TeamModes.HeadToHead)
                {
                    foreach (var player in match.Players.OrderBy(x => x.Key.Name))
                        list.Add(player.Key);
                }
                return list;
            }
        }

        private bool canBanOrPick
        {
            get { return !(match.Bans.Any(x => x.Map == beatmap) || match.Picks.Any(x => x.Map == beatmap)); }
        }

        private bool canUnban
        {
            get { return match.Bans.Any(x => x.Map == beatmap); }
        }

        private bool canUnpick
        {
            get { return match.Picks.Any(x => x.Map == beatmap); }
        }

        public BindableCollection<MatchBeatmapMenuItem> MenuItems
        {
            get
            {
                var list = new BindableCollection<MatchBeatmapMenuItem>();
                if (canBanOrPick)
                {
                    if (match.TeamMode == Enums.TeamModes.TeamVS)
                    {
                        list.Add(new MatchBeatmapMenuItem { Type = MatchBeatmapMenuItemTypes.Ban, Team = match.TeamRed });
                        list.Add(new MatchBeatmapMenuItem { Type = MatchBeatmapMenuItemTypes.Ban, Team = match.TeamBlue });
                        list.Add(new MatchBeatmapMenuItem { Type = MatchBeatmapMenuItemTypes.Pick, Team = match.TeamRed });
                        list.Add(new MatchBeatmapMenuItem { Type = MatchBeatmapMenuItemTypes.Pick, Team = match.TeamBlue });
                    }
                    else
                    {
                        foreach (var player in match.Players)
                            list.Add(new MatchBeatmapMenuItem { Type = MatchBeatmapMenuItemTypes.Ban, Player = player.Key });
                        foreach (var player in match.Players)
                            list.Add(new MatchBeatmapMenuItem { Type = MatchBeatmapMenuItemTypes.Pick, Player = player.Key });
                    }
                }
                else
                {
                    if (canUnban)
                    {
                        list.Add(new MatchBeatmapMenuItem { Type = MatchBeatmapMenuItemTypes.Unban });
                    }
                    else if (canUnpick)
                    {
                        list.Add(new MatchBeatmapMenuItem { Type = MatchBeatmapMenuItemTypes.Unpick });
                    }
                }
                return list;
            }
        }
        #endregion

        #region Actions
        public void MenuItemClick(MatchBeatmapMenuItem context)
        {
            localLog.Information("beatmap '{beatmap}' click context menu item '{item}'", beatmap.Beatmap.Title, context.Name);
            if (context.Type == MatchBeatmapMenuItemTypes.Unban)
                RemoveBan();
            else if (context.Type == MatchBeatmapMenuItemTypes.Unpick)
                RemovePick();
            else if (context.Type == MatchBeatmapMenuItemTypes.Ban)
            {
                if (context.Player != null)
                {
                    BanPlayer(context.Player);
                }
                else
                {
                    if (context.Team == match.TeamRed)
                        BanRed();
                    else
                        BanBlue();
                }
            }
            else if (context.Type == MatchBeatmapMenuItemTypes.Pick)
            {
                if (context.Player != null)
                {
                    PickPlayer(context.Player);
                }
                else
                {
                    if (context.Team == match.TeamRed)
                        PickRed();
                    else
                        PickBlue();
                }
            }
            NotifyOfPropertyChange(() => MenuItems);
        }

        public void BanRed()
        {
            localLog.Information("match '{match}' ban beatmap '{beatmap}' by red", match.Name, beatmap.Beatmap.Title);
            match.Bans.Add(new MatchPick()
            {
                Match = match,
                Map = beatmap,
                Team = match.TeamRed
            });
            match.Save();
            NotifyOfPropertyChange(() => CanBanOrPickTeam);
            NotifyOfPropertyChange(() => CanUnban);
            NotifyOfPropertyChange(() => TextDecoration);
            NotifyOfPropertyChange(() => FontColor);
        }

        public void BanBlue()
        {
            localLog.Information("match '{match}' ban beatmap '{beatmap}' by blue", match.Name, beatmap.Beatmap.Title);
            match.Bans.Add(new MatchPick()
            {
                Match = match,
                Map = beatmap,
                Team = match.TeamBlue
            });
            match.Save();
            NotifyOfPropertyChange(() => CanBanOrPickTeam);
            NotifyOfPropertyChange(() => CanUnban);
            NotifyOfPropertyChange(() => TextDecoration);
            NotifyOfPropertyChange(() => FontColor);
        }

        public void BanPlayer(Player player)
        {
            localLog.Information("match '{match}' ban beatmap '{beatmap}' by player '{player}'", match.Name, beatmap.Beatmap.Title, player.Name);
            match.Bans.Add(new MatchPick()
            {
                Match = match,
                Map = beatmap,
                Player = player
            });
            match.Save();
            NotifyOfPropertyChange(() => CanBanOrPickTeam);
            NotifyOfPropertyChange(() => CanUnban);
            NotifyOfPropertyChange(() => TextDecoration);
            NotifyOfPropertyChange(() => FontColor);
        }

        public void RemoveBan()
        {
            localLog.Information("match '{match}' unban beatmap '{beatmap}'", match.Name, beatmap.Beatmap.Title);
            match.Bans.RemoveAll(x => x.Map == beatmap);
            match.Save();
            NotifyOfPropertyChange(() => CanBanOrPickTeam);
            NotifyOfPropertyChange(() => CanUnban);
            NotifyOfPropertyChange(() => TextDecoration);
            NotifyOfPropertyChange(() => FontColor);
        }

        public void PickRed()
        {
            localLog.Information("match '{match}' pick beatmap '{beatmap}' by red", match.Name, beatmap.Beatmap.Title);
            match.Picks.Add(new MatchPick()
            {
                Match = match,
                Map = beatmap,
                Team = match.TeamRed
            });
            match.WarmupMode = false;
            match.Save();
            NotifyOfPropertyChange(() => CanBanOrPickTeam);
            NotifyOfPropertyChange(() => CanUnpick);
            NotifyOfPropertyChange(() => FontColor);
            Events.Aggregator.PublishOnUIThread("MapPicked");
            if (match.RoomId > 0)
                SendPickMessage();
        }

        public void PickBlue()
        {
            localLog.Information("match '{match}' pick beatmap '{beatmap}' by blue", match.Name, beatmap.Beatmap.Title);
            match.Picks.Add(new MatchPick()
            {
                Match = match,
                Map = beatmap,
                Team = match.TeamBlue
            });
            match.WarmupMode = false;
            match.Save();
            NotifyOfPropertyChange(() => CanBanOrPickTeam);
            NotifyOfPropertyChange(() => CanUnpick);
            NotifyOfPropertyChange(() => FontColor);
            Events.Aggregator.PublishOnUIThread("MapPicked");
            if (match.RoomId > 0)
                SendPickMessage();
        }

        public void PickPlayer(Player player)
        {
            localLog.Information("match '{match}' pick beatmap '{beatmap}' by player '{player}'", match.Name, beatmap.Beatmap.Title, player.Name);
            match.Picks.Add(new MatchPick()
            {
                Match = match,
                Map = beatmap,
                Player = player
            });
            match.WarmupMode = false;
            match.Save();
            NotifyOfPropertyChange(() => CanBanOrPickTeam);
            NotifyOfPropertyChange(() => CanUnban);
            NotifyOfPropertyChange(() => TextDecoration);
            NotifyOfPropertyChange(() => FontColor);
            Events.Aggregator.PublishOnUIThread("MapPicked");
            if (match.RoomId > 0)
                SendPickMessage();
        }

        public void RemovePick()
        {
            localLog.Information("match '{match}' unpick beatmap '{beatmap}'", match.Name, beatmap.Beatmap.Title);
            match.Picks.RemoveAll(x => x.Map == beatmap);
            match.Save();
            NotifyOfPropertyChange(() => CanBanOrPickTeam);
            NotifyOfPropertyChange(() => CanUnpick);
            NotifyOfPropertyChange(() => FontColor);
        }

        private void SendPickMessage()
        {
            localLog.Information("match '{match}' send pick message for '{beatmap}'", match.Name, beatmap.Beatmap.Title);
            var mods = Utils.ConvertGameModsToString(beatmap.Mods);
            if (match.AllPicksFreemod && !mods.Contains("Freemod"))
                mods += " Freemod";
            OsuIrc.OsuIrc.SendMessage("#mp_" + match.RoomId, "!mp mods " + mods);
            OsuIrc.OsuIrc.SendMessage("#mp_" + match.RoomId, $"!mp map {beatmap.Beatmap.Id} {(int)match.GameMode}");
            if (match.MpTimerAfterPick > 0)
                OsuIrc.OsuIrc.SendMessage("#mp_" + match.RoomId, "!mp timer " + match.MpTimerAfterPick);
        }
        #endregion
    }

    public enum MatchBeatmapMenuItemTypes
    {
        Pick,
        Ban,
        Unpick,
        Unban
    }

    public class MatchBeatmapMenuItem
    {
        public MatchBeatmapMenuItemTypes Type { get; set; }

        public Player Player;

        public Team Team;

        public string Name
        {
            get
            {
                if (Type == MatchBeatmapMenuItemTypes.Ban)
                {
                    if (Player != null)
                    {
                        return "Banned by " + Player.Name;
                    }
                    return "Banned by " + Team.Name;
                }
                if (Type == MatchBeatmapMenuItemTypes.Pick)
                {
                    if (Player != null)
                    {
                        return "Picked by " + Player.Name;
                    }
                    return "Picked by " + Team.Name;
                }
                if (Type == MatchBeatmapMenuItemTypes.Unban)
                {
                    return "Unban";
                }
                return "Unpick";
            }
        }
    }
}
