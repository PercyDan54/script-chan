using script_chan2.DataTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace script_chan2.GUI
{
    public static class ChatList
    {
        static ChatList()
        {
            UserChats.Add(new UserChat { User = "Server", Active = true });
        }

        public static List<UserChat> UserChats = new List<UserChat>();

        public static UserChat GetActiveChat()
        {
            return UserChats.First(x => x.Active);
        }

        public static void ActivateChat(UserChat userChat)
        {
            foreach (var chat in UserChats)
                chat.Active = false;
            userChat.Activate();
        }

        public static void RemoveChat(UserChat userChat)
        {
            if (GetActiveChat() == userChat)
            {
                foreach (var chat in UserChats)
                    chat.Active = false;
                UserChats.First(x => x.User == "Server").Activate();
            }

            UserChats.Remove(userChat);
        }
    }
}
