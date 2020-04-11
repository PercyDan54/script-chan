using Caliburn.Micro;
using script_chan2.DataTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace script_chan2.GUI
{
    public class MatchCustomCommandViewModel : Screen
    {
        #region Constructor
        public MatchCustomCommandViewModel(Match match, CustomCommand customCommand)
        {
            this.match = match;
            this.customCommand = customCommand;
        }
        #endregion

        #region Properties
        private Match match;

        private CustomCommand customCommand;

        public string Name
        {
            get { return customCommand.Name; }
        }
        #endregion

        #region Actions
        public void Execute()
        {
            if (match.RoomId > 0)
            {
                OsuIrc.OsuIrc.SendMessage("#mp_" + match.RoomId, customCommand.Command);
            }
        }
        #endregion
    }
}
