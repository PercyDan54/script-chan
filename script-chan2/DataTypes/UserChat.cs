using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace script_chan2.DataTypes
{
    public class UserChat
    {
        #region Constructor
        public UserChat()
        {
            Messages = new List<IrcMessage>();
        }
        #endregion

        #region Properties
        public string User { get; set; }

        public List<IrcMessage> Messages { get; set; }

        public bool Active { get; set; }

        public bool NewMessages { get; set; }
        #endregion

        #region Actions
        public void LoadMessages()
        {
            Messages = Database.Database.GetIrcMessages(User);
        }

        public void AddMessage(IrcMessage message)
        {
            Messages.Add(message);
            if (!Active)
                NewMessages = true;
        }

        public void Activate()
        {
            Active = true;
            NewMessages = false;
        }
        #endregion
    }
}
