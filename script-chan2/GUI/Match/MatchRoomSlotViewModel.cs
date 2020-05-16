using Caliburn.Micro;
using script_chan2.DataTypes;
using script_chan2.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace script_chan2.GUI
{
    public class MatchRoomSlotViewModel : Screen
    {
        #region Constructor
        public MatchRoomSlotViewModel(RoomSlot slot)
        {
            this.slot = slot;
        }
        #endregion

        #region Properties
        private RoomSlot slot;

        public int SlotNumber
        {
            get { return slot.Slot; }
        }

        public string PlayerName
        {
            get
            {
                return slot.Player.Name;
            }
        }

        public BitmapImage Flag
        {
            get
            {
                return new BitmapImage(new Uri($"https://osu.ppy.sh/images/flags/{slot.Player.Country}.png"));
            }
        }

        public SolidColorBrush TeamColor
        {
            get
            {
                if (slot.Team == TeamColors.Red)
                    return Brushes.Red;
                if (slot.Team == TeamColors.Blue)
                    return Brushes.Blue;
                return Brushes.White;
            }
        }

        public BindableCollection<MatchRoomSlotModViewModel> ModViews
        {
            get
            {
                var list = new BindableCollection<MatchRoomSlotModViewModel>();
                foreach (var mod in slot.Mods)
                {
                    list.Add(new MatchRoomSlotModViewModel(mod));
                }
                return list;
            }
        }

        public SolidColorBrush Background
        {
            get
            {
                switch (slot.State)
                {
                    case RoomSlotStates.NoMap: return Brushes.LightCoral;
                    case RoomSlotStates.NotReady: return Brushes.LightGreen;
                    case RoomSlotStates.Ready: return Brushes.Green;
                }
                return Brushes.White;
            }
        }
        #endregion
    }
}
