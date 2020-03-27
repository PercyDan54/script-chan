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
    public class ChatUserViewModel : Screen
    {
        public ChatUserViewModel(UserChat userChat)
        {
            UserChat = userChat;
        }

        public UserChat UserChat;
        
        public string Name
        {
            get { return UserChat.User; }
        }

        public Visibility NewMessagesVisible
        {
            get
            {
                if (UserChat.NewMessages)
                    return Visibility.Visible;
                return Visibility.Hidden;
            }
        }

        public Visibility CloseVisible
        {
            get
            {
                if (UserChat.User != "Server")
                    return Visibility.Visible;
                return Visibility.Collapsed;
            }
        }

        public Brush Background
        {
            get
            {
                if (UserChat.Active)
                    return Brushes.LightGray;
                return Brushes.White;
            }
        }
    }
}
