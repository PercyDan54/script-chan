using Caliburn.Micro;
using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace script_chan2.GUI
{
    public class MatchJoinRoomDialogViewModel : Screen
    {
        #region Properties
        private int roomId;
        public int RoomId
        {
            get { return roomId; }
            set
            {
                if (value != roomId)
                {
                    roomId = value;
                    NotifyOfPropertyChange(() => RoomId);
                    NotifyOfPropertyChange(() => JoinEnabled);
                }
            }
        }

        public bool JoinEnabled
        {
            get { return RoomId > 0; }
        }
        #endregion

        #region Actions
        public void DialogEscape()
        {
            DialogHost.CloseDialogCommand.Execute(false, null);
        }
        #endregion
    }
}
