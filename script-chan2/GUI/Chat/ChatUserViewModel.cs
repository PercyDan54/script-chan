using Caliburn.Micro;
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
        public ChatUserViewModel(string user, bool canBeClosed)
        {
            User = user;
            this.canBeClosed = canBeClosed;
            newMessages = false;
        }

        public string User { get; }

        private bool newMessages;

        private bool canBeClosed;

        private bool active;
        
        public string Name
        {
            get { return User; }
        }

        public Visibility NewMessagesVisible
        {
            get
            {
                if (newMessages)
                    return Visibility.Visible;
                return Visibility.Hidden;
            }
        }

        public Visibility CloseVisible
        {
            get
            {
                if (canBeClosed)
                    return Visibility.Visible;
                return Visibility.Collapsed;
            }
        }

        public Brush Background
        {
            get
            {
                if (active)
                    return Brushes.LightGray;
                return Brushes.White;
            }
        }

        public void AddedNewMessages()
        {
            newMessages = true;
            NotifyOfPropertyChange(() => NewMessagesVisible);
        }

        public void SetActive()
        {
            newMessages = false;
            active = true;
            NotifyOfPropertyChange(() => NewMessagesVisible);
            NotifyOfPropertyChange(() => Background);
        }

        public void SetInactive()
        {
            active = false;
            NotifyOfPropertyChange(() => Background);
        }
    }
}
