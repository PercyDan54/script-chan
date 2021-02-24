using Caliburn.Micro;
using MaterialDesignColors.ColorManipulation;
using script_chan2.DataTypes;
using Serilog;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace script_chan2.GUI
{
    public class MatchBeatmapViewModel : Screen, IHandle<string>
    {
        private ILogger localLog = Log.ForContext<MatchBeatmapViewModel>();

        #region Constructor
        public MatchBeatmapViewModel(Match match, MappoolMap beatmap, bool sendIrcCommands)
        {
            this.match = match;
            this.beatmap = beatmap;
            this.sendIrcCommands = sendIrcCommands;
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

        private bool sendIrcCommands;

        public string ModTag
        {
            get { return beatmap.Tag; }
        }

        public string BeatmapName
        {
            get
            {
                return string.Format(Properties.Resources.MatchBeatmapViewModel_LabelText,
                    beatmap.Beatmap.Artist,
                    beatmap.Beatmap.Title,
                    beatmap.Beatmap.Version,
                    beatmap.Beatmap.Creator,
                    beatmap.Beatmap.BPM,
                    beatmap.Beatmap.AR,
                    beatmap.Beatmap.CS);
            }
        }

        public string ToolTip
        {
            get
            {
                return string.Format(Properties.Resources.MatchBeatmapViewModel_LabelTooltip,
                    beatmap.Beatmap.Creator,
                    beatmap.Beatmap.BPM,
                    beatmap.Beatmap.AR,
                    beatmap.Beatmap.CS);
            }
        }

        public Brush Background
        {
            get
            {
                var brush = new LinearGradientBrush();
                if (beatmap.Mods.Count > 0)
                {
                    for (var i = 0; i < beatmap.Mods.Count; i++)
                    {
                        Color colorToAdd = Colors.White;
                        switch (beatmap.Mods[i])
                        {
                            case Enums.GameMods.Hidden: colorToAdd = Settings.UserColors.First(x => x.Key == "HD").Color; break;
                            case Enums.GameMods.HardRock: colorToAdd = Settings.UserColors.First(x => x.Key == "HR").Color; break;
                            case Enums.GameMods.DoubleTime: colorToAdd = Settings.UserColors.First(x => x.Key == "DT").Color; break;
                            case Enums.GameMods.Flashlight: colorToAdd = Settings.UserColors.First(x => x.Key == "FL").Color; break;
                            case Enums.GameMods.Freemod: colorToAdd = Settings.UserColors.First(x => x.Key == "Freemod").Color; break;
                            case Enums.GameMods.TieBreaker: colorToAdd = Settings.UserColors.First(x => x.Key == "Tiebreaker").Color; break;
                            case Enums.GameMods.NoFail: colorToAdd = Settings.UserColors.First(x => x.Key == "NoFail").Color; break;
                        }
                        if (!CanBanOrPick)
                        {
                            colorToAdd = colorToAdd.Darken(1);
                        }
                        brush.GradientStops.Add(new GradientStop(colorToAdd, 1f / (beatmap.Mods.Count - 1) * i));
                    }
                }
                else
                {
                    Color colorToAdd = (Color)ColorConverter.ConvertFromString("#1A1A1A");
                    if (!CanBanOrPick)
                    {
                        colorToAdd = colorToAdd.Darken(1);
                    }
                    brush.GradientStops.Add(new GradientStop(colorToAdd, 0));
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
                    return Brushes.DeepSkyBlue;
                }
                if (match.TeamMode == Enums.TeamModes.TeamVS && match.Picks.Any(x => x.Map == beatmap))
                {
                    if (match.Picks.First(x => x.Map == beatmap).Team == match.TeamRed)
                        return Brushes.Red;
                    return Brushes.DeepSkyBlue;
                }
                if (match.TeamMode == Enums.TeamModes.HeadToHead && match.Picks.Any(x => x.Map == beatmap))
                    return Brushes.Green;
                return Brushes.White;
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

        public bool CanBanOrPick
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
                if (CanBanOrPick)
                {
                    if (match.TeamMode == Enums.TeamModes.TeamVS)
                    {
                        list.Add(new MatchBeatmapMenuItem { Type = MatchBeatmapMenuItemTypes.Ban, Team = match.TeamRed });
                        list.Add(new MatchBeatmapMenuItem { Type = MatchBeatmapMenuItemTypes.Ban, Team = match.TeamBlue });
                        list.Add(new MatchBeatmapMenuItem { Type = MatchBeatmapMenuItemTypes.Pick, Team = match.TeamRed });
                        list.Add(new MatchBeatmapMenuItem { Type = MatchBeatmapMenuItemTypes.Pick, Team = match.TeamBlue });
                    }
                    if (match.TeamMode == Enums.TeamModes.HeadToHead)
                    {
                        foreach (var player in match.Players)
                            list.Add(new MatchBeatmapMenuItem { Type = MatchBeatmapMenuItemTypes.Ban, Player = player.Key });
                        foreach (var player in match.Players)
                            list.Add(new MatchBeatmapMenuItem { Type = MatchBeatmapMenuItemTypes.Pick, Player = player.Key });
                    }
                    if (match.TeamMode == Enums.TeamModes.BattleRoyale)
                    {
                        list.Add(new MatchBeatmapMenuItem { Type = MatchBeatmapMenuItemTypes.Pick });
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
                else if (context.Team != null)
                {
                    if (context.Team == match.TeamRed)
                        PickRed();
                    else
                        PickBlue();
                }
                else
                {
                    Pick();
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
                Team = match.TeamRed,
                ListIndex = match.Bans.Count + 1
            });
            match.Save();
            NotifyOfPropertyChange(() => CanBanOrPickTeam);
            NotifyOfPropertyChange(() => CanUnban);
            NotifyOfPropertyChange(() => TextDecoration);
            NotifyOfPropertyChange(() => FontColor);
            NotifyOfPropertyChange(() => Background);
            Events.Aggregator.PublishOnUIThread("MapBanned");
        }

        public void BanBlue()
        {
            localLog.Information("match '{match}' ban beatmap '{beatmap}' by blue", match.Name, beatmap.Beatmap.Title);
            match.Bans.Add(new MatchPick()
            {
                Match = match,
                Map = beatmap,
                Team = match.TeamBlue,
                ListIndex = match.Bans.Count + 1
            });
            match.Save();
            NotifyOfPropertyChange(() => CanBanOrPickTeam);
            NotifyOfPropertyChange(() => CanUnban);
            NotifyOfPropertyChange(() => TextDecoration);
            NotifyOfPropertyChange(() => FontColor);
            NotifyOfPropertyChange(() => Background);
            Events.Aggregator.PublishOnUIThread("MapBanned");
        }

        public void BanPlayer(Player player)
        {
            localLog.Information("match '{match}' ban beatmap '{beatmap}' by player '{player}'", match.Name, beatmap.Beatmap.Title, player.Name);
            match.Bans.Add(new MatchPick()
            {
                Match = match,
                Map = beatmap,
                Player = player,
                ListIndex = match.Bans.Count + 1
            });
            match.Save();
            NotifyOfPropertyChange(() => CanBanOrPickTeam);
            NotifyOfPropertyChange(() => CanUnban);
            NotifyOfPropertyChange(() => TextDecoration);
            NotifyOfPropertyChange(() => FontColor);
            NotifyOfPropertyChange(() => Background);
            Events.Aggregator.PublishOnUIThread("MapBanned");
        }

        public void RemoveBan()
        {
            localLog.Information("match '{match}' unban beatmap '{beatmap}'", match.Name, beatmap.Beatmap.Title);
            match.Bans.RemoveAll(x => x.Map == beatmap);
            for (var i = 0; i < match.Bans.Count; i++)
            {
                match.Bans[i].ListIndex = i + 1;
            }
            match.Save();
            NotifyOfPropertyChange(() => CanBanOrPickTeam);
            NotifyOfPropertyChange(() => CanUnban);
            NotifyOfPropertyChange(() => TextDecoration);
            NotifyOfPropertyChange(() => FontColor);
            NotifyOfPropertyChange(() => Background);
            Events.Aggregator.PublishOnUIThread("MapUnbanned");
        }

        public void PickRed()
        {
            localLog.Information("match '{match}' pick beatmap '{beatmap}' by red", match.Name, beatmap.Beatmap.Title);
            match.Picks.Add(new MatchPick()
            {
                Match = match,
                Map = beatmap,
                Team = match.TeamRed,
                ListIndex = match.Picks.Count + 1
            });
            match.WarmupMode = false;
            match.Save();
            NotifyOfPropertyChange(() => CanBanOrPickTeam);
            NotifyOfPropertyChange(() => CanUnpick);
            NotifyOfPropertyChange(() => FontColor);
            NotifyOfPropertyChange(() => Background);
            Events.Aggregator.PublishOnUIThread("MapPicked");
            SendPickMessage();
        }

        public void PickBlue()
        {
            localLog.Information("match '{match}' pick beatmap '{beatmap}' by blue", match.Name, beatmap.Beatmap.Title);
            match.Picks.Add(new MatchPick()
            {
                Match = match,
                Map = beatmap,
                Team = match.TeamBlue,
                ListIndex = match.Picks.Count + 1
            });
            match.WarmupMode = false;
            match.Save();
            NotifyOfPropertyChange(() => CanBanOrPickTeam);
            NotifyOfPropertyChange(() => CanUnpick);
            NotifyOfPropertyChange(() => FontColor);
            NotifyOfPropertyChange(() => Background);
            Events.Aggregator.PublishOnUIThread("MapPicked");
            SendPickMessage();
        }

        public void PickPlayer(Player player)
        {
            localLog.Information("match '{match}' pick beatmap '{beatmap}' by player '{player}'", match.Name, beatmap.Beatmap.Title, player.Name);
            match.Picks.Add(new MatchPick()
            {
                Match = match,
                Map = beatmap,
                Player = player,
                ListIndex = match.Picks.Count + 1
            });
            match.WarmupMode = false;
            match.Save();
            NotifyOfPropertyChange(() => CanBanOrPickTeam);
            NotifyOfPropertyChange(() => CanUnban);
            NotifyOfPropertyChange(() => TextDecoration);
            NotifyOfPropertyChange(() => FontColor);
            NotifyOfPropertyChange(() => Background);
            Events.Aggregator.PublishOnUIThread("MapPicked");
            SendPickMessage();
        }

        public void Pick()
        {
            localLog.Information("match '{match}' pick beatmap '{beatmap}'", match.Name, beatmap.Beatmap.Title);
            var pick = new MatchPick()
            {
                Match = match,
                Map = beatmap,
                ListIndex = match.Picks.Count + 1
            };
            if (match.TeamMode == Enums.TeamModes.TeamVS)
            {
                if (match.Picks.Count > 0)
                {
                    if (match.Picks.Last().Team.Id == match.TeamRed.Id)
                        pick.Team = match.TeamBlue;
                    else
                        pick.Team = match.TeamRed;
                }
                else
                {
                    if (match.FirstPickerTeam != null)
                    {
                        if (match.FirstPickerTeam.Id == match.TeamRed.Id)
                            pick.Team = match.TeamRed;
                        else
                            pick.Team = match.TeamBlue;
                    }
                }
            }
            match.Picks.Add(pick);
            match.WarmupMode = false;
            match.Save();
            NotifyOfPropertyChange(() => CanBanOrPickTeam);
            NotifyOfPropertyChange(() => CanUnban);
            NotifyOfPropertyChange(() => TextDecoration);
            NotifyOfPropertyChange(() => FontColor);
            NotifyOfPropertyChange(() => Background);
            Events.Aggregator.PublishOnUIThread("MapPicked");
            SendPickMessage();
        }

        public void RemovePick()
        {
            localLog.Information("match '{match}' unpick beatmap '{beatmap}'", match.Name, beatmap.Beatmap.Title);
            match.Picks.RemoveAll(x => x.Map == beatmap);
            for (var i = 0; i < match.Picks.Count; i++)
            {
                match.Picks[i].ListIndex = i + 1;
            }
            match.Save();
            NotifyOfPropertyChange(() => CanBanOrPickTeam);
            NotifyOfPropertyChange(() => CanUnpick);
            NotifyOfPropertyChange(() => FontColor);
            NotifyOfPropertyChange(() => Background);
            Events.Aggregator.PublishOnUIThread("MapUnpicked");
        }

        private void SendPickMessage()
        {
            if (match.RoomId <= 0)
                return;
            if (match.ViewerMode)
                return;
            if (!sendIrcCommands)
                return;
            localLog.Information("match '{match}' send pick message for '{beatmap}'", match.Name, beatmap.Beatmap.Title);
            var mods = Utils.ConvertGameModsToString(beatmap.Mods);
            if (match.AllPicksFreemod && !mods.Contains("Freemod"))
                mods += " Freemod";
            if (beatmap.PickCommand)
            {
                OsuIrc.OsuIrc.SendMessage("#mp_" + match.RoomId, "!mp mods " + mods);
                OsuIrc.OsuIrc.SendMessage("#mp_" + match.RoomId, $"!mp map {beatmap.Beatmap.Id} {(int)match.GameMode}");
            }
            if (match.MpTimerAfterPick > 0)
                OsuIrc.OsuIrc.SendMessage("#mp_" + match.RoomId, "!mp timer " + match.MpTimerAfterPick);
        }

        public void RootDoubleClick()
        {
            if (CanBanOrPick && match.TeamMode == Enums.TeamModes.TeamVS)
            {
                if (match.Picks.Count > 0)
                {
                    if (match.Picks.Last().Team.Id == match.TeamRed.Id)
                        PickBlue();
                    else
                        PickRed();
                }
                else
                {
                    if (match.FirstPickerTeam != null)
                    {
                        if (match.FirstPickerTeam.Id == match.TeamRed.Id)
                            PickRed();
                        else
                            PickBlue();
                    }
                }
            }
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
                        return string.Format(Properties.Resources.MatchBeatmapViewModel_ContextItemBannedByText, Player.Name);
                    }
                    else if (Team != null)
                    {
                        return string.Format(Properties.Resources.MatchBeatmapViewModel_ContextItemBannedByText, Team.Name);
                    }
                    else
                    {
                        return string.Format(Properties.Resources.MatchBeatmapViewModel_ContextItemBanText);
                    }
                }
                if (Type == MatchBeatmapMenuItemTypes.Pick)
                {
                    if (Player != null)
                    {
                        return string.Format(Properties.Resources.MatchBeatmapViewModel_ContextItemPickedByText, Player.Name);
                    }
                    else if (Team != null)
                    {
                        return string.Format(Properties.Resources.MatchBeatmapViewModel_ContextItemPickedByText, Team.Name);
                    }
                    else
                    {
                        return string.Format(Properties.Resources.MatchBeatmapViewModel_ContextItemPickText);
                    }
                }
                if (Type == MatchBeatmapMenuItemTypes.Unban)
                {
                    return string.Format(Properties.Resources.MatchBeatmapViewModel_ContextItemUnbanText);
                }
                return string.Format(Properties.Resources.MatchBeatmapViewModel_ContextItemUnpickText);
            }
        }
    }
}
