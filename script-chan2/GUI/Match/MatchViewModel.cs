using Caliburn.Micro;
using script_chan2.DataTypes;
using script_chan2.Enums;
using script_chan2.OsuIrc;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace script_chan2.GUI
{
    public class MatchViewModel : Screen, IHandle<object>
    {
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

        public List<GameModes> GameModesList
        {
            get { return Enum.GetValues(typeof(GameModes)).Cast<GameModes>().ToList(); }
        }

        private List<IrcMessage> messagesToSave = new List<IrcMessage>();
        #endregion

        #region Constructor
        public MatchViewModel(Match match)
        {
            this.match = match;
        }
        #endregion

        #region Events
        protected override void OnActivate()
        {
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
                    AddMessageToChat(message);
                }
            });
        }
        protected override void OnDeactivate(bool close)
        {
            Log.Information("GUI close match '{name}'", match.Name);
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
                    match.RoomId = data.Id;
                    match.Save();
                    NotifyOfPropertyChange(() => RoomLinkName);
                    NotifyOfPropertyChange(() => RoomClosedVisible);
                    NotifyOfPropertyChange(() => RoomOpenVisible);
                    OsuIrc.OsuIrc.JoinChannel("#mp_" + data.Id);
                }
            }
            else if (message is ChannelMessageData)
            {
                var data = (ChannelMessageData)message;
                if (data.Channel == "#mp_" + match.RoomId)
                {
                    var ircMessage = new IrcMessage() { User = data.User, Timestamp = DateTime.Now, Match = match, Message = data.Message };
                    messagesToSave.Add(ircMessage);
                    AddMessageToChat(ircMessage);
                }
            }
        }
        #endregion

        #region Properties
        private Match match;

        public string WindowTitle
        {
            get { return match.Name; }
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
            set
            {
                if (value != match.GameMode)
                {
                    match.GameMode = value;
                    match.Save();
                    NotifyOfPropertyChange(() => GameMode);
                }
            }
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
            set
            {
                if (value != match.RoomSize)
                {
                    match.RoomSize = value;
                    match.Save();
                    NotifyOfPropertyChange(() => RoomSize);
                }
            }
        }

        public int TeamSize
        {
            get { return match.TeamSize; }
            set
            {
                if (value != match.TeamSize)
                {
                    match.TeamSize = value;
                    match.Save();
                    NotifyOfPropertyChange(() => TeamSize);
                }
            }
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
        #endregion

        #region Window Events
        public void Drag(MouseButtonEventArgs e)
        {
            if (e.ChangedButton != MouseButton.Left)
                return;
            ((MatchView)GetView()).DragMove();
        }

        public void MinimizeWindow()
        {
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
            ((MatchView)GetView()).WindowState = WindowState.Normal;
            NotifyOfPropertyChange(() => WindowMaximizeVisible);
            NotifyOfPropertyChange(() => WindowRestoreVisible);
        }

        public void CloseWindow()
        {
            TryClose();
        }
        #endregion

        #region Actions
        public void OpenMpLink()
        {
            System.Diagnostics.Process.Start("https://osu.ppy.sh/community/matches/" + match.RoomId);
        }

        private void SendRoomMessage(string message)
        {
            OsuIrc.OsuIrc.SendMessage("#mp_" + match.RoomId, message);
            var ircMessage = new IrcMessage() { Match = match, User = Settings.IrcUsername, Timestamp = DateTime.Now, Message = message };
            AddMessageToChat(ircMessage);
            messagesToSave.Add(ircMessage);
            if (messagesToSave.Count >= 5)
            {
                Database.Database.AddIrcMessages(messagesToSave);
                messagesToSave.Clear();
            }
        }

        public void CreateRoom()
        {
            OsuIrc.OsuIrc.SendMessage("BanchoBot", "!mp make " + match.Name);
            messagesToSave.Add(new IrcMessage() { Match = null, User = Settings.IrcUsername, Timestamp = DateTime.Now, Message = "!mp make " + match.Name });
        }

        public void CloseRoom()
        {
            SendRoomMessage("!mp close");
            match.RoomId = 0;
            match.Save();
            NotifyOfPropertyChange(() => RoomLinkName);
            NotifyOfPropertyChange(() => RoomClosedVisible);
            NotifyOfPropertyChange(() => RoomOpenVisible);
        }

        public void SwitchPlayers()
        {
            foreach (var player in match.GetPlayerList())
            {
                OsuIrc.OsuIrc.SendMessage("BanchoBot", "!mp switch " + player);
                messagesToSave.Add(new IrcMessage() { Match = null, User = Settings.IrcUsername, Timestamp = DateTime.Now, Message = "!mp switch " + player });
            }
        }

        public void InvitePlayers()
        {
            foreach (var player in match.GetPlayerList())
            {
                SendRoomMessage("!mp invite " + player);
            }
        }

        public void SendWelcomeString()
        {
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
            var message = ChatMessage;
            ChatMessage = "";
            SendRoomMessage(message);
        }

        public void SendSettings()
        {
            SendRoomMessage("!mp settings");
        }

        public void StartGame()
        {
            SendRoomMessage("!mp start 5");
        }

        public void AbortMatch()
        {
            SendRoomMessage("!mp abort");
        }

        private void AddMessageToChat(IrcMessage message)
        {
            var brush = new SolidColorBrush();
            if (message.User == Settings.IrcUsername)
                brush.Color = Settings.SelfColor;
            else if (message.User == "BanchoBot")
                brush.Color = Settings.BanchoBotColor;
            var paragraph = new Paragraph(new Run($"[{message.Timestamp.ToString("HH:mm")}] {message.User.PadRight(15)} {message.Message}")) { Margin = new Thickness(202, 0, 0, 0), TextIndent = -202, Foreground = brush };
            MultiplayerChat.Blocks.Add(paragraph);
            NotifyOfPropertyChange(() => MultiplayerChat);
        }

        public void ChatMessageKeyDown(ActionExecutionContext context)
        {
            var keyArgs = context.EventArgs as KeyEventArgs;
            if (keyArgs != null && keyArgs.Key == Key.Enter)
                SendMessage();
        }
        #endregion
    }
}
