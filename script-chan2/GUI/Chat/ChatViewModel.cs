using Caliburn.Micro;
using script_chan2.DataTypes;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace script_chan2.GUI
{
    public class ChatViewModel : Screen, IHandle<object>
    {
        #region Lists
        public BindableCollection<ChatUserViewModel> UserViews { get; set; }

        private List<UserChat> userChats;

        private List<IrcMessage> messagesToSave;
        #endregion

        #region Constructor
        public ChatViewModel()
        {
            userChats = new List<UserChat>();
            UserViews = new BindableCollection<ChatUserViewModel>();
            messagesToSave = new List<IrcMessage>();

            userChats.Add(new UserChat() { User = "Server" });
            var serverModel = new ChatUserViewModel("Server", false);
            serverModel.SetActive();
            UserViews.Add(serverModel);

            currentChat = userChats[0];

            ReloadChat();

            Events.Aggregator.Subscribe(this);
        }

        protected override void OnDeactivate(bool close)
        {
            Database.Database.AddIrcMessages(messagesToSave);
        }
        #endregion

        #region Events
        public void Handle(object message)
        {
            if (message is PrivateMessageData)
            {
                var data = (PrivateMessageData)message;
                Log.Information("ChatViewModel: irc message received '{message}' from user '{user}'", data.Message, data.User);

                var ircMessage = new IrcMessage() { Channel = data.Channel, User = data.User, Timestamp = DateTime.Now, Message = data.Message };
                if (ircMessage.Channel != "Server")
                {
                    messagesToSave.Add(ircMessage);
                    if (messagesToSave.Count >= 5)
                    {
                        Database.Database.AddIrcMessages(messagesToSave);
                        messagesToSave.Clear();
                    }
                }

                if (!userChats.Any(x => x.User == data.Channel))
                {
                    var newUserChat = new UserChat() { User = data.Channel };
                    newUserChat.LoadMessages();
                    userChats.Add(newUserChat);

                    UserViews.Add(new ChatUserViewModel(data.Channel, true));
                }

                var userChat = userChats.First(x => x.User == data.Channel);
                userChat.Messages.Add(ircMessage);

                if (currentChat.User != data.Channel)
                {
                    var userModel = UserViews.First(x => x.User == data.Channel);
                    userModel.AddedNewMessages();
                }

                if (currentChat.User == data.Channel)
                    AddMessageToChat(ircMessage);
            }
        }
        #endregion

        #region Properties
        private UserChat currentChat;

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

        private string newChatChannel;
        public string NewChatChannel
        {
            get { return newChatChannel; }
            set
            {
                if (value != newChatChannel)
                {
                    newChatChannel = value;
                    NotifyOfPropertyChange(() => NewChatChannel);
                    NotifyOfPropertyChange(() => NewChatOpenEnabled);
                }
            }
        }

        public bool NewChatOpenEnabled
        {
            get
            {
                if (string.IsNullOrEmpty(NewChatChannel))
                    return false;
                return true;
            }
        }
        #endregion

        #region Actions
        private void AddMessageToChat(IrcMessage message)
        {
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
            Execute.OnUIThread(() =>
            {
                Chat = new FlowDocument
                {
                    FontFamily = new FontFamily("Lucida Console"),
                    FontSize = 12,
                    TextAlignment = TextAlignment.Left
                };
                foreach (var message in currentChat.Messages)
                {
                    AddMessageToChat(message);
                }
            });
        }

        public void OpenChat(ChatUserViewModel dataContext)
        {
            if (currentChat.User == dataContext.User)
                return;

            currentChat = userChats.First(x => x.User == dataContext.User);
            foreach (var model in UserViews)
            {
                if (model.User == dataContext.User)
                    model.SetActive();
                else
                    model.SetInactive();
            }

            ReloadChat();
        }

        public void CloseChat(ChatUserViewModel dataContext)
        {
            if (currentChat.User == dataContext.User)
            {
                currentChat = userChats[0];
                foreach (var model in UserViews)
                    model.SetInactive();
                UserViews[0].SetActive();

                ReloadChat();
            }

            userChats.RemoveAll(x => x.User == dataContext.User);
            UserViews.Remove(UserViews.First(x => x.User == dataContext.User));
        }

        public void ChatMessageKeyDown(ActionExecutionContext context)
        {
            var keyArgs = context.EventArgs as KeyEventArgs;
            if (keyArgs != null && keyArgs.Key == Key.Enter)
                SendMessage();
        }

        public void SendMessage()
        {
            if (currentChat.User == "Server")
                return;
            if (string.IsNullOrEmpty(ChatMessage))
                return;
            Log.Information("ChatViewModel: user '{user}' send irc message '{message}'", currentChat.User, ChatMessage);
            var message = ChatMessage;
            ChatMessage = "";
            OsuIrc.OsuIrc.SendMessage(currentChat.User, message);
        }

        public void NewChatDialogOpened()
        {
            NewChatChannel = "";
        }

        public void NewChatDialogClosed()
        {
            if (!userChats.Any(x => x.User == NewChatChannel))
            {
                var newUserChat = new UserChat() { User = NewChatChannel };
                newUserChat.LoadMessages();
                userChats.Add(newUserChat);

                UserViews.Add(new ChatUserViewModel(NewChatChannel, true));
            }

            currentChat = userChats.First(x => x.User == NewChatChannel);
            foreach (var model in UserViews)
            {
                if (model.User == NewChatChannel)
                    model.SetActive();
                else
                    model.SetInactive();
            }

            ReloadChat();

            NewChatChannel = "";
        }
        #endregion
    }
}
