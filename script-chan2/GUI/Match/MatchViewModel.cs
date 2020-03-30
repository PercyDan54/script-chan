﻿using Caliburn.Micro;
using MaterialDesignThemes.Wpf;
using script_chan2.DataTypes;
using script_chan2.Discord;
using script_chan2.Enums;
using script_chan2.OsuIrc;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace script_chan2.GUI
{
    public class MatchViewModel : Screen, IHandle<object>, IHandle<string>
    {
        private ILogger localLog = Log.ForContext<MatchViewModel>();

        #region Lists
        public BindableCollection<MatchTeamViewModel> TeamsViews
        {
            get
            {
                var list = new BindableCollection<MatchTeamViewModel>();
                if (match.TeamMode == TeamModes.TeamVS)
                {
                    list.Add(new MatchTeamViewModel(match, TeamColors.Red));
                    list.Add(new MatchTeamViewModel(match, TeamColors.Blue));
                }
                return list;
            }
        }

        public BindableCollection<MatchPlayerViewModel> PlayersViews
        {
            get
            {
                var list = new BindableCollection<MatchPlayerViewModel>();
                if (match.TeamMode == TeamModes.HeadToHead)
                {
                    foreach (var player in match.Players.OrderByDescending(x => x.Value))
                    {
                        list.Add(new MatchPlayerViewModel(match, player.Key));
                    }
                }
                return list;
            }
        }

        public BindableCollection<Mappool> Mappools
        {
            get
            {
                var list = new BindableCollection<Mappool>();
                foreach (var mappool in Database.Database.Mappools)
                {
                    if (mappool.Tournament != match.Tournament)
                        continue;
                    list.Add(mappool);
                }
                return list;
            }
        }

        public BindableCollection<Team> Teams
        {
            get
            {
                var list = new BindableCollection<Team>();
                if (match.TeamMode == TeamModes.TeamVS)
                {
                    list.Add(match.TeamBlue);
                    list.Add(match.TeamRed);
                }
                return list;
            }
        }

        public BindableCollection<Player> Players
        {
            get
            {
                var list = new BindableCollection<Player>();
                if (match.TeamMode == TeamModes.HeadToHead)
                {
                    foreach (var player in match.Players.OrderBy(x => x.Key.Name))
                        list.Add(player.Key);
                }
                return list;
            }
        }

        public BindableCollection<MatchBeatmapViewModel> BeatmapsViews
        {
            get
            {
                var list = new BindableCollection<MatchBeatmapViewModel>();
                if (SelectedMappool != null)
                {
                    foreach (var beatmap in SelectedMappool.Beatmaps)
                    {
                        list.Add(new MatchBeatmapViewModel(match, beatmap));
                    }
                }
                return list;
            }
        }

        public BindableCollection<MatchBanchoEventViewModel> BanchoEventsViews
        {
            get
            {
                var list = new BindableCollection<MatchBanchoEventViewModel>();
                foreach (var message in match.ChatMessages)
                {
                    if (message.User == "BanchoBot")
                    {
                        if (message.Message.Contains("All players are ready"))
                            list.Add(new MatchBanchoEventViewModel(MatchBanchoEvents.AllPlayersReady, message.Timestamp));
                        if (message.Message.Contains("The match has finished"))
                            list.Add(new MatchBanchoEventViewModel(MatchBanchoEvents.AllPlayersFinished, message.Timestamp));
                        if (message.Message.Contains("rolls"))
                        {
                            var data = message.Message.Replace(" rolls", "").Replace(" point(s)", "");
                            var user = data.Split(' ')[0];
                            var roll = data.Split(' ')[1];
                            TeamColors? team = null;
                            if (match.TeamMode == TeamModes.TeamVS)
                            {
                                if (match.TeamRed.Players.Any(x => x.Name == user))
                                    team = TeamColors.Red;
                                if (match.TeamBlue.Players.Any(x => x.Name == user))
                                    team = TeamColors.Blue;
                            }
                            list.Add(new MatchBanchoEventViewModel(MatchBanchoEvents.PlayerRoll, message.Timestamp, user, team, roll));
                        }
                    }
                }
                return list;
            }
        }

        public List<GameModes> GameModesList
        {
            get { return Enum.GetValues(typeof(GameModes)).Cast<GameModes>().ToList(); }
        }

        public List<WinConditions> WinConditionsList
        {
            get { return Enum.GetValues(typeof(WinConditions)).Cast<WinConditions>().ToList(); }
        }

        private List<IrcMessage> messagesToSave = new List<IrcMessage>();
        #endregion

        #region Constructor
        public MatchViewModel(Match match)
        {
            this.match = match;
            if (match.RoomId > 0)
                OsuIrc.OsuIrc.JoinChannel("#mp_" + match.RoomId);
        }
        #endregion

        #region Events
        protected override void OnActivate()
        {
            localLog.Information("open window of match '{match}'", match.Name);
            Events.Aggregator.Subscribe(this);
            match.ReloadMessages();
            Execute.OnUIThread(() =>
            {
                MultiplayerChat = new FlowDocument
                {
                    FontFamily = new FontFamily("Lucida Console"),
                    FontSize = 12,
                    TextAlignment = TextAlignment.Left
                };
                foreach (var message in match.ChatMessages)
                {
                    AddMessageToChat(message, true);
                }
            });
        }

        protected override void OnViewLoaded(object view)
        {
            var chatWindow = ((MatchView)GetView()).ChatWindow;
            var scrollViewer = FindScroll(chatWindow);
            if (scrollViewer != null)
                scrollViewer.ScrollToEnd();
        }

        protected override void OnDeactivate(bool close)
        {
            localLog.Information("close window of match '{name}'", match.Name);
            MatchList.OpenedMatches.Remove(match);
            Database.Database.AddIrcMessages(messagesToSave);
        }

        public void Handle(object message)
        {
            if (message is RoomCreatedData)
            {
                var data = (RoomCreatedData)message;
                if (data.Name == match.Name)
                {
                    localLog.Information("match '{match}' room creation data received", match.Name);
                    match.RoomId = data.Id;
                    match.Status = MatchStatus.InProgress;
                    match.Save();
                    NotifyOfPropertyChange(() => RoomLinkName);
                    NotifyOfPropertyChange(() => RoomClosedVisible);
                    NotifyOfPropertyChange(() => RoomOpenVisible);
                    OsuIrc.OsuIrc.JoinChannel("#mp_" + data.Id);
                    SendRoomSet();
                    if (match.Mappool != null && match.Mappool.Beatmaps.Count > 0)
                    {
                        if (match.Mappool.Beatmaps.Any(x => x.Mods.Contains(GameMods.TieBreaker)))
                            SendRoomMessage($"!mp map {match.Mappool.Beatmaps.First(x => x.Mods.Contains(GameMods.TieBreaker)).Beatmap.Id} {(int)match.GameMode}");
                        else
                            SendRoomMessage($"!mp map {match.Mappool.Beatmaps[0].Beatmap.Id} {(int)match.GameMode}");
                    }
                    SendRoomMessage("!mp unlock");
                    SendRoomMessage("!mp settings");

                    DiscordApi.SendMatchCreated(match);
                }
            }
            else if (message is ChannelMessageData)
            {
                var data = (ChannelMessageData)message;
                localLog.Information("match '{match}' irc message '{message}' received from user '{user}'", match.Name, data.Message, data.User);
                if (data.Channel == "#mp_" + match.RoomId)
                {
                    var ircMessage = new IrcMessage() { Channel = "#mp_" + match.RoomId, User = data.User, Timestamp = DateTime.Now, Match = match, Message = data.Message };
                    messagesToSave.Add(ircMessage);
                    if (messagesToSave.Count >= 5)
                    {
                        Database.Database.AddIrcMessages(messagesToSave);
                        messagesToSave.Clear();
                    }
                    AddMessageToChat(ircMessage, false);

                    if (data.User == "BanchoBot" && data.Message.Contains("All players are ready"))
                    {
                        PlayNotificationSound();
                    }
                    if (data.User == "BanchoBot" && data.Message.Contains("The match has finished"))
                    {
                        UpdateScore();
                        if (!match.WarmupMode && match.EnableWebhooks)
                        {
                            DiscordApi.SendGameRecap(match);
                        }
                        if (!match.WarmupMode && match.MpTimerAfterGame > 0)
                        {
                            SendRoomMessage("!mp timer " + match.MpTimerAfterGame);
                        }
                    }
                }
            }
        }

        public void Handle(string message)
        {
            if (message == "UpdateColors")
            {
                localLog.Information("match '{match}' update colors", match.Name);
                Execute.OnUIThread(() =>
                {
                    MultiplayerChat = new FlowDocument
                    {
                        FontFamily = new FontFamily("Lucida Console"),
                        FontSize = 12,
                        TextAlignment = TextAlignment.Left
                    };
                    foreach (var chatMessage in match.ChatMessages)
                    {
                        AddMessageToChat(chatMessage, false);
                    }
                });
            }
            else if (message == "MapPicked")
            {
                NotifyOfPropertyChange(() => WarmupMode);
            }
        }
        #endregion

        #region Properties
        private Match match;

        public string WindowTitle
        {
            get { return match.Name; }
        }

        public string DialogIdentifier
        {
            get { return "MatchDialogHost" + match.Id; }
        }

        public Visibility TeamVsVisible
        {
            get
            {
                if (match.TeamMode == Enums.TeamModes.TeamVS)
                    return Visibility.Visible;
                return Visibility.Collapsed;
            }
        }

        public Visibility HeadToHeadVisible
        {
            get
            {
                if (match.TeamMode == Enums.TeamModes.HeadToHead)
                    return Visibility.Visible;
                return Visibility.Collapsed;
            }
        }

        public Visibility RoomClosedVisible
        {
            get
            {
                if (match.RoomId == 0)
                    return Visibility.Visible;
                return Visibility.Collapsed;
            }
        }

        public Visibility RoomOpenVisible
        {
            get
            {
                if (match.RoomId > 0)
                    return Visibility.Visible;
                return Visibility.Collapsed;
            }
        }

        public Mappool SelectedMappool
        {
            get { return match.Mappool; }
            set
            {
                if (value != match.Mappool)
                {
                    match.Mappool = value;
                    match.Save();
                    NotifyOfPropertyChange(() => SelectedMappool);
                    NotifyOfPropertyChange(() => BeatmapsViews);
                }
            }
        }

        public Team RollWinnerTeam
        {
            get { return match.RollWinnerTeam; }
            set
            {
                if (value != match.RollWinnerTeam)
                {
                    match.RollWinnerTeam = value;
                    match.Save();
                    NotifyOfPropertyChange(() => RollWinnerTeam);
                }
            }
        }

        public Player RollWinnerPlayer
        {
            get { return match.RollWinnerPlayer; }
            set
            {
                if (value != match.RollWinnerPlayer)
                {
                    match.RollWinnerPlayer = value;
                    match.Save();
                    NotifyOfPropertyChange(() => RollWinnerPlayer);
                }
            }
        }

        public GameModes GameMode
        {
            get { return match.GameMode; }
            set { }
        }

        public WinConditions WinCondition
        {
            get { return match.WinCondition; }
            set { }
        }

        public int BO
        {
            get { return match.BO; }
            set
            {
                if (value != match.BO)
                {
                    match.BO = value;
                    match.Save();
                    NotifyOfPropertyChange(() => BO);
                }
            }
        }

        public int RoomSize
        {
            get { return match.RoomSize; }
            set { }
        }

        public int TeamSize
        {
            get { return match.TeamSize; }
            set { }
        }

        public int TimerCommand
        {
            get { return match.MpTimerCommand; }
            set
            {
                if (value != match.MpTimerCommand)
                {
                    match.MpTimerCommand = value;
                    match.Save();
                    NotifyOfPropertyChange(() => TimerCommand);
                }
            }
        }

        public int TimerAfterGame
        {
            get { return match.MpTimerAfterGame; }
            set
            {
                if (value != match.MpTimerAfterGame)
                {
                    match.MpTimerAfterGame = value;
                    match.Save();
                    NotifyOfPropertyChange(() => TimerAfterGame);
                }
            }
        }

        public int TimerAfterPick
        {
            get { return match.MpTimerAfterPick; }
            set
            {
                if (value != match.MpTimerAfterPick)
                {
                    match.MpTimerAfterPick = value;
                    match.Save();
                    NotifyOfPropertyChange(() => TimerAfterPick);
                }
            }
        }

        public int PointsForSecondBan
        {
            get { return match.PointsForSecondBan; }
            set
            {
                if (value != match.PointsForSecondBan)
                {
                    match.PointsForSecondBan = value;
                    match.Save();
                    NotifyOfPropertyChange(() => PointsForSecondBan);
                }
            }
        }

        public bool AllPicksFreemod
        {
            get { return match.AllPicksFreemod; }
            set
            {
                if (value != match.AllPicksFreemod)
                {
                    match.AllPicksFreemod = value;
                    match.Save();
                    NotifyOfPropertyChange(() => AllPicksFreemod);
                }
            }
        }

        public bool EnableWebhooks
        {
            get { return match.EnableWebhooks; }
            set
            {
                if (value != match.EnableWebhooks)
                {
                    match.EnableWebhooks = value;
                    match.Save();
                    NotifyOfPropertyChange(() => EnableWebhooks);
                }
            }
        }

        public string TeamBlueName
        {
            get
            {
                if (match.TeamMode == TeamModes.TeamVS)
                    return match.TeamBlue.Name;
                return "";
            }
        }

        public string TeamRedName
        {
            get
            {
                if (match.TeamMode == TeamModes.TeamVS)
                    return match.TeamRed.Name;
                return "";
            }
        }

        public string RoomLinkName
        {
            get
            {
                return "#mp__" + match.RoomId;
            }
        }

        private FlowDocument multiplayerChat;
        public FlowDocument MultiplayerChat
        {
            get { return multiplayerChat; }
            set
            {
                if (value != multiplayerChat)
                {
                    multiplayerChat = value;
                    NotifyOfPropertyChange(() => MultiplayerChat);
                }
            }
        }

        private string chatMessage;
        public string ChatMessage
        {
            get { return chatMessage; }
            set
            {
                if (value != chatMessage)
                {
                    chatMessage = value;
                    NotifyOfPropertyChange(() => ChatMessage);
                }
            }
        }

        public bool WarmupMode
        {
            get { return match.WarmupMode; }
            set
            {
                if (value != match.WarmupMode)
                {
                    match.WarmupMode = value;
                    match.Save();
                    NotifyOfPropertyChange(() => WarmupMode);
                }
            }
        }
        #endregion

        #region Window Events
        public void Drag(MouseButtonEventArgs e)
        {
            localLog.Information("match '{match}' drag window", match.Name);
            if (e.ChangedButton != MouseButton.Left)
                return;
            ((MatchView)GetView()).DragMove();
        }

        public void MinimizeWindow()
        {
            localLog.Information("match '{match}' minimize window", match.Name);
            ((MatchView)GetView()).WindowState = WindowState.Minimized;
        }

        public Visibility WindowMaximizeVisible
        {
            get
            {
                if (((MatchView)GetView()).WindowState != WindowState.Maximized)
                    return Visibility.Visible;
                return Visibility.Collapsed;
            }
        }

        public void MaximizeWindow()
        {
            localLog.Information("match '{match}' maximize window", match.Name);
            ((MatchView)GetView()).WindowState = WindowState.Maximized;
            NotifyOfPropertyChange(() => WindowMaximizeVisible);
            NotifyOfPropertyChange(() => WindowRestoreVisible);
        }

        public Visibility WindowRestoreVisible
        {
            get
            {
                if (((MatchView)GetView()).WindowState == WindowState.Maximized)
                    return Visibility.Visible;
                return Visibility.Collapsed;
            }
        }

        public void RestoreWindow()
        {
            localLog.Information("match '{match}' restore window", match.Name);
            ((MatchView)GetView()).WindowState = WindowState.Normal;
            NotifyOfPropertyChange(() => WindowMaximizeVisible);
            NotifyOfPropertyChange(() => WindowRestoreVisible);
        }

        public void CloseWindow()
        {
            localLog.Information("match '{match}' close window", match.Name);
            TryClose();
        }
        #endregion

        #region Actions
        public void OpenMpLink()
        {
            localLog.Information("match '{match}' open mp link", match.Name);
            System.Diagnostics.Process.Start("https://osu.ppy.sh/community/matches/" + match.RoomId);
        }

        private void SendRoomMessage(string message)
        {
            OsuIrc.OsuIrc.SendMessage("#mp_" + match.RoomId, message);
        }

        public void CreateRoom()
        {
            localLog.Information("match '{match}' create room", match.Name);
            OsuIrc.OsuIrc.SendMessage("BanchoBot", "!mp make " + match.Name);
        }

        public async void JoinRoom()
        {
            localLog.Information("match '{match}' open join room dialog", match.Name);
            var model = new MatchJoinRoomDialogViewModel();
            var view = ViewLocator.LocateForModel(model, null, null);
            ViewModelBinder.Bind(model, view, null);

            var result = Convert.ToBoolean(await DialogHost.Show(view, DialogIdentifier));

            if (result)
            {
                localLog.Information("match '{match}' join room '{room}'", match.Name, model.RoomId);
                match.RoomId = model.RoomId;
                match.Status = MatchStatus.InProgress;
                match.Save();
                NotifyOfPropertyChange(() => RoomLinkName);
                NotifyOfPropertyChange(() => RoomClosedVisible);
                NotifyOfPropertyChange(() => RoomOpenVisible);
                OsuIrc.OsuIrc.JoinChannel("#mp_" + model.RoomId);
            }
        }

        public async void CloseRoom()
        {
            localLog.Information("match '{match}' open close room dialog", match.Name);
            var model = new MatchCloseRoomDialogViewModel();
            var view = ViewLocator.LocateForModel(model, null, null);
            ViewModelBinder.Bind(model, view, null);

            var result = Convert.ToBoolean(await DialogHost.Show(view, DialogIdentifier));

            if (result)
            {
                localLog.Information("match '{match}' close room", match.Name);
                SendRoomMessage("!mp close");
                match.RoomId = 0;
                match.Status = MatchStatus.Finished;
                match.Save();
                NotifyOfPropertyChange(() => RoomLinkName);
                NotifyOfPropertyChange(() => RoomClosedVisible);
                NotifyOfPropertyChange(() => RoomOpenVisible);
            }
        }

        public void SwitchPlayers()
        {
            localLog.Information("match '{match}' switch players", match.Name);
            foreach (var player in match.GetPlayerList())
            {
                OsuIrc.OsuIrc.SendMessage("BanchoBot", "!mp switch " + player);
            }
        }

        public void InvitePlayers()
        {
            localLog.Information("match '{match}' invite players", match.Name);
            foreach (var player in match.GetPlayerList())
            {
                SendRoomMessage("!mp invite " + player);
            }
        }

        public void SendWelcomeString()
        {
            localLog.Information("match '{match}' send welcome string", match.Name);
            if (match.TeamMode == TeamModes.TeamVS)
            {
                SendRoomMessage($"{TeamRedName} is RED, slots 1 to {match.TeamSize} --- {TeamBlueName} is BLUE, slots {match.TeamSize + 1} to {match.TeamSize * 2}");
            }

            SendRoomMessage(match.Tournament.WelcomeString);
        }

        public void SendMessage()
        {
            if (string.IsNullOrEmpty(ChatMessage))
                return;
            localLog.Information("match '{match}' send irc message '{message}'", match.Name, ChatMessage);
            var message = ChatMessage;
            ChatMessage = "";
            SendRoomMessage(message);
        }

        public void SendSettings()
        {
            localLog.Information("match '{match}' send settings", match.Name);
            SendRoomMessage("!mp settings");
        }

        public void StartTimer()
        {
            localLog.Information("match '{match}' start timer", match.Name);
            if (match.MpTimerCommand > 0)
                SendRoomMessage("!mp timer " + match.MpTimerCommand);
        }

        public void StartGame()
        {
            localLog.Information("match '{match}' start game", match.Name);
            SendRoomMessage("!mp start 5");
        }

        public async void AbortMatch()
        {
            localLog.Information("match '{match}' open abort game dialog", match.Name);
            var model = new MatchAbortMapDialogViewModel();
            var view = ViewLocator.LocateForModel(model, null, null);
            ViewModelBinder.Bind(model, view, null);

            var result = Convert.ToBoolean(await DialogHost.Show(view, DialogIdentifier));

            if (result)
            {
                localLog.Information("match '{match}' abort game", match.Name);
                SendRoomMessage("!mp abort");
            }
        }

        public async void SetPassword()
        {
            localLog.Information("match '{match}' open set password dialog", match.Name);
            var model = new MatchPasswordDialogViewModel();
            var view = ViewLocator.LocateForModel(model, null, null);
            ViewModelBinder.Bind(model, view, null);

            var result = Convert.ToBoolean(await DialogHost.Show(view, DialogIdentifier));

            if (result)
            {
                if (string.IsNullOrEmpty(model.Password))
                    localLog.Information("match '{match}' remove password", match.Name);
                else
                    localLog.Information("match '{match}' set password", match.Name);
                SendRoomMessage("!mp password " + model.Password);
            }
        }

        private void AddMessageToChat(IrcMessage message, bool init)
        {
            localLog.Information("match '{match}' add message to chat", match.Name);
            if (!init)
                match.ChatMessages.Add(message);

            var scrollToEnd = false;
            var view = (MatchView)GetView();
            var chatWindow = view.ChatWindow;
            var scrollViewer = FindScroll(chatWindow);
            if (scrollViewer != null)
            {
                if (scrollViewer.VerticalOffset == scrollViewer.ScrollableHeight)
                    scrollToEnd = true;
            }

            var scrollEventsToEnd = false;
            var eventsScrollViewer = view.BanchoEventsScrollViewer;
            if (eventsScrollViewer.VerticalOffset == eventsScrollViewer.ScrollableHeight)
                scrollEventsToEnd = true;

                var brush = new SolidColorBrush();
            if (message.User == Settings.IrcUsername)
                brush.Color = Settings.UserColors.First(x => x.Key == "Self").Color;
            else if (message.User == "BanchoBot")
                brush.Color = Settings.UserColors.First(x => x.Key == "BanchoBot").Color;
            var paragraph = new Paragraph(new Run($"[{message.Timestamp.ToString("HH:mm")}] {message.User.PadRight(15)} {message.Message}")) { Margin = new Thickness(202, 0, 0, 0), TextIndent = -202, Foreground = brush };
            MultiplayerChat.Blocks.Add(paragraph);
            NotifyOfPropertyChange(() => MultiplayerChat);
            NotifyOfPropertyChange(() => BanchoEventsViews);

            if (scrollToEnd)
                scrollViewer.ScrollToEnd();

            if (scrollEventsToEnd)
                eventsScrollViewer.ScrollToEnd();
        }

        public void ChatMessageKeyDown(ActionExecutionContext context)
        {
            var keyArgs = context.EventArgs as KeyEventArgs;
            if (keyArgs != null && keyArgs.Key == Key.Enter)
                SendMessage();
        }

        private ScrollViewer FindScroll(FlowDocumentScrollViewer flowDocumentScrollViewer)
        {
            if (VisualTreeHelper.GetChildrenCount(flowDocumentScrollViewer) == 0)
            {
                return null;
            }

            // Border is the first child of first child of a ScrolldocumentViewer
            DependencyObject firstChild = VisualTreeHelper.GetChild(flowDocumentScrollViewer, 0);
            if (firstChild == null)
            {
                return null;
            }

            Decorator border = VisualTreeHelper.GetChild(firstChild, 0) as Decorator;

            if (border == null)
            {
                return null;
            }

            return border.Child as ScrollViewer;
        }

        private void PlayNotificationSound()
        {
            localLog.Information("match '{match}' play notification sound", match.Name);
            var player = new MediaPlayer();
            player.Open(new Uri(Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location), "notification.mp3")));
            player.Play();
        }

        public void UpdateScore()
        {
            localLog.Information("match '{match}' update scores", match.Name);
            match.UpdateScores();
            if (match.TeamMode == TeamModes.TeamVS)
                NotifyOfPropertyChange(() => TeamsViews);
            else
                NotifyOfPropertyChange(() => PlayersViews);
        }

        private void SendRoomSet()
        {
            localLog.Information("match '{match}' set room {mode}", match.Name, $"{(int)match.TeamMode} {(int)match.WinCondition} {match.RoomSize}");
            SendRoomMessage($"!mp set {(int)match.TeamMode} {(int)match.WinCondition} {match.RoomSize}");
        }

        public async void EditRoomOptions()
        {
            localLog.Information("match '{match}' open room options dialog", match.Name);
            var model = new MatchRoomOptionsDialogViewModel();
            var view = ViewLocator.LocateForModel(model, null, null);
            ViewModelBinder.Bind(model, view, null);

            model.GameMode = match.GameMode;
            model.WinCondition = match.WinCondition;
            model.TeamSize = match.TeamSize;
            model.RoomSize = match.RoomSize;

            var result = Convert.ToBoolean(await DialogHost.Show(view, DialogIdentifier));

            if (result)
            {
                localLog.Information("match '{match}' set room options", match.Name);
                match.GameMode = model.GameMode;
                match.WinCondition = model.WinCondition;
                match.TeamSize = model.TeamSize;
                match.RoomSize = model.RoomSize;
                match.Save();
                NotifyOfPropertyChange(() => GameMode);
                NotifyOfPropertyChange(() => WinCondition);
                NotifyOfPropertyChange(() => RoomSize);
                NotifyOfPropertyChange(() => TeamSize);
                SendRoomSet();
            }
        }

        public void SendBanRecap()
        {
            localLog.Information("match '{match}' send ban recap", match.Name);
            DiscordApi.SendMatchBanRecap(match);
        }

        public void SendPickRecap()
        {
            localLog.Information("match '{match}' send pick recap", match.Name);
            DiscordApi.SendMatchPickRecap(match);
        }
        #endregion
    }
}
