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
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace script_chan2.GUI
{
    public class ChatViewModel : Screen, IHandle<object>
    {
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

        private List<IrcMessage> messagesToSave;
        #endregion

        #region Constructor
        protected override void OnActivate()
        {
            messagesToSave = new List<IrcMessage>();
            ReloadChat();
            Events.Aggregator.Subscribe(this);
        }

        protected override void OnDeactivate(bool close)
        {
            Database.Database.AddIrcMessages(messagesToSave);
            messagesToSave.Clear();
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

                if (!ChatList.UserChats.Any(x => x.User == data.Channel))
                {
                    var newUserChat = new UserChat() { User = data.Channel };
                    newUserChat.LoadMessages();
                    ChatList.UserChats.Add(newUserChat);
                }

                var userChat = ChatList.UserChats.First(x => x.User == data.Channel);
                userChat.AddMessage(ircMessage);

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
                foreach (var message in ChatList.GetActiveChat().Messages)
                {
                    AddMessageToChat(message);
                }
            });
        }

        public void OpenChat(ChatUserViewModel dataContext)
        {
            if (ChatList.GetActiveChat() == dataContext.UserChat)
                return;

            ChatList.ActivateChat(dataContext.UserChat);
            NotifyOfPropertyChange(() => UserViews);

            ReloadChat();
        }

        public void CloseChat(ChatUserViewModel dataContext)
        {
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
            var model = new NewChatDialogViewModel();
            var view = ViewLocator.LocateForModel(model, null, null);
            ViewModelBinder.Bind(model, view, null);

            var result = Convert.ToBoolean(await DialogHost.Show(view));

            if (result)
            {
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
            Log.Information("ChatViewModel: user '{user}' send irc message '{message}'", ChatList.GetActiveChat().User, ChatMessage);
            var message = ChatMessage;
            ChatMessage = "";
            OsuIrc.OsuIrc.SendMessage(ChatList.GetActiveChat().User, message);
        }
        #endregion
    }
}
