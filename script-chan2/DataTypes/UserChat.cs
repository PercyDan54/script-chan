using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace script_chan2.DataTypes
{
    public class UserChat
    {
        private ILogger localLog = Log.ForContext<UserChat>();

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
            localLog.Information("'{channel}' load messages", User);
            if (Messages == null || Messages.Count == 0)
                Messages = Database.Database.GetIrcMessages(User);
        }

        public void AddMessage(IrcMessage message)
        {
            localLog.Information("'{channel}' add message '{message}' from user '{user}'", User, message.Message, message.User);
            Messages.Add(message);
            if (!Active)
                NewMessages = true;
        }

        public void Activate()
        {
            localLog.Information("'{channel}' activate", User);
            Active = true;
            NewMessages = false;
        }
        #endregion
    }
}
