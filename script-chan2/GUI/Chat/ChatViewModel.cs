using Caliburn.Micro;
using MaterialDesignThemes.Wpf;
using script_chan2.DataTypes;
using Serilog;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace script_chan2.GUI
{
    public class ChatViewModel : Screen, IHandle<object>
    {
        private ILogger localLog = Log.ForContext<ChatViewModel>();

        #region Lists
        public BindableCollection<ChatUserViewModel> UserViews
        {
            get
            {
                var list = new BindableCollection<ChatUserViewModel>();
                foreach (var chat in ChatList.UserChats)
                {
                    list.Add(new ChatUserViewModel(chat));
                }
                return list;
            }
        }
        #endregion

        #region Constructor
        protected override void OnActivate()
        {
            ReloadChat();
            Events.Aggregator.Subscribe(this);
        }
        #endregion

        #region Events
        public void Handle(object message)
        {
            if (message is PrivateMessageData)
            {
                var data = (PrivateMessageData)message;
                localLog.Information("irc message received '{message}' from user '{user}'", data.Message, data.User);

                var ircMessage = new IrcMessage() { Channel = data.Channel, User = data.User, Timestamp = DateTime.Now, Message = data.Message };

                NotifyOfPropertyChange(() => UserViews);

                if (ChatList.GetActiveChat().User == data.Channel)
                    AddMessageToChat(ircMessage);
            }
        }
        #endregion

        #region Properties
        private FlowDocument chat;
        public FlowDocument Chat
        {
            get { return chat; }
            set
            {
                if (value != chat)
                {
                    chat = value;
                    NotifyOfPropertyChange(() => Chat);
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

        #region Actions
        private void AddMessageToChat(IrcMessage message)
        {
            localLog.Information("add message '{message}' to chat", message.Message);
            var paragraph = new Paragraph(new Run($"[{message.Timestamp.ToString("HH:mm")}] {message.User.PadRight(15)} {message.Message}")) { Margin = new Thickness(202, 0, 0, 0), TextIndent = -202 };
            Chat.Blocks.Add(paragraph);
            NotifyOfPropertyChange(() => Chat);

            var view = (ChatView)GetView();
            if (view != null)
            {
                var chatWindow = view.ChatWindow;
                var scrollViewer = FindScroll(chatWindow);
                if (scrollViewer != null)
                    scrollViewer.ScrollToEnd();
            }
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

        private void ReloadChat()
        {
            localLog.Information("reload chat");
            Execute.OnUIThread(() =>
            {
                Chat = new FlowDocument
                {
                    FontFamily = new FontFamily("Lucida Console"),
                    FontSize = 12,
                    TextAlignment = TextAlignment.Left
                };
                foreach (var message in ChatList.GetActiveChat().Messages)
                {
                    AddMessageToChat(message);
                }
            });
        }

        public void OpenChat(ChatUserViewModel dataContext)
        {
            localLog.Information("open chat of user '{user}'", dataContext.UserChat.User);
            if (ChatList.GetActiveChat() == dataContext.UserChat)
                return;

            ChatList.ActivateChat(dataContext.UserChat);
            NotifyOfPropertyChange(() => UserViews);

            ReloadChat();
        }

        public void CloseChat(ChatUserViewModel dataContext)
        {
            localLog.Information("close chat of user '{user}'", dataContext.UserChat.User);
            ChatList.RemoveChat(dataContext.UserChat);
            NotifyOfPropertyChange(() => UserViews);

            ReloadChat();
        }

        public void ChatMessageEnter()
        {
            SendMessage();
        }

        public async void OpenNewChatDialog()
        {
            localLog.Information("open new chat dialog");
            var model = new NewChatDialogViewModel();
            var view = ViewLocator.LocateForModel(model, null, null);
            ViewModelBinder.Bind(model, view, null);

            var result = Convert.ToBoolean(await DialogHost.Show(view, "MainDialogHost"));

            if (result)
            {
                localLog.Information("open new chat for user '{user}'", model.NewChatChannel);
                if (!ChatList.UserChats.Any(x => x.User == model.NewChatChannel))
                {
                    var newUserChat = new UserChat() { User = model.NewChatChannel };
                    newUserChat.LoadMessages();
                    ChatList.UserChats.Add(newUserChat);
                }

                ChatList.ActivateChat(ChatList.UserChats.First(x => x.User == model.NewChatChannel));
                NotifyOfPropertyChange(() => UserViews);

                ReloadChat();
            }
        }

        public void SendMessage()
        {
            if (string.IsNullOrEmpty(ChatMessage))
                return;
            localLog.Information("user '{user}' send irc message '{message}'", ChatList.GetActiveChat().User, ChatMessage);
            var message = ChatMessage;
            ChatMessage = "";
            OsuIrc.OsuIrc.SendMessage(ChatList.GetActiveChat().User, message);
        }
        #endregion
    }
}
