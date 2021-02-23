using Caliburn.Micro;
using script_chan2.DataTypes;
using script_chan2.Enums;
using Serilog;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace script_chan2.GUI
{
    public class MatchRoomSlotViewModel : Screen
    {
        private ILogger localLog = Log.ForContext<MatchRoomSlotViewModel>();

        #region Constructor
        public MatchRoomSlotViewModel(Match match, int slotNumber)
        {
            this.match = match;
            SlotNumber = slotNumber;
            Mods = new List<GameMods>();
        }
        #endregion

        #region Properties
        private Match match;

        private int slotNumber;
        public int SlotNumber
        {
            get { return slotNumber; }
            set
            {
                if (value != slotNumber)
                {
                    slotNumber = value;
                    NotifyOfPropertyChange(() => SlotNumber);
                }
            }
        }

        private Player player;
        public Player Player
        {
            get { return player; }
            set
            {
                if (value != player)
                {
                    player = value;
                    NotifyOfPropertyChange(() => Player);
                    NotifyOfPropertyChange(() => PlayerName);
                    NotifyOfPropertyChange(() => Flag);
                    NotifyOfPropertyChange(() => NameColor);
                    NotifyOfPropertyChange(() => Background);
                    NotifyOfPropertyChange(() => MenuItems);
                }
            }
        }

        public string PlayerName
        {
            get
            {
                if (Player != null)
                    return Player.Name;
                return "";
            }
        }

        public BitmapImage Flag
        {
            get
            {
                if (Player != null)
                    return new BitmapImage(new Uri($"https://osu.ppy.sh/images/flags/{Player.Country}.png"));
                return null;
            }
        }

        private TeamColors? team;
        public TeamColors? Team
        {
            get { return team; }
            set
            {
                if (value != team)
                {
                    team = value;
                    NotifyOfPropertyChange(() => Team);
                    NotifyOfPropertyChange(() => TeamColor);
                }
            }
        }

        public SolidColorBrush TeamColor
        {
            get
            {
                if (Team == TeamColors.Red)
                    return Brushes.Red;
                if (Team == TeamColors.Blue)
                    return Brushes.Blue;
                return Brushes.White;
            }
        }

        public SolidColorBrush NameColor
        {
            get
            {
                if (Player != null)
                    return Brushes.Black;
                return Brushes.White;
            }
        }

        private List<GameMods> mods;
        public List<GameMods> Mods
        {
            get { return mods; }
            set
            {
                if (value != mods)
                {
                    mods = value;
                    NotifyOfPropertyChange(() => Mods);
                    NotifyOfPropertyChange(() => ModViews);
                }
            }
        }

        public BindableCollection<MatchRoomSlotModViewModel> ModViews
        {
            get
            {
                var list = new BindableCollection<MatchRoomSlotModViewModel>();
                foreach (var mod in Mods)
                {
                    list.Add(new MatchRoomSlotModViewModel(mod));
                }
                return list;
            }
        }

        private RoomSlotStates state;
        public RoomSlotStates State
        {
            get { return state; }
            set
            {
                if (value != state)
                {
                    state = value;
                    NotifyOfPropertyChange(() => State);
                    NotifyOfPropertyChange(() => Background);
                }
            }
        }

        public SolidColorBrush Background
        {
            get
            {
                if (Player == null)
                    return Brushes.Black;
                switch (State)
                {
                    case RoomSlotStates.NoMap: return Brushes.LightCoral;
                    case RoomSlotStates.NotReady: return Brushes.LightGreen;
                    case RoomSlotStates.Ready: return Brushes.Green;
                }
                return Brushes.White;
            }
        }

        public BindableCollection<MenuItem> MenuItems
        {
            get
            {
                var list = new BindableCollection<MenuItem>();
                if (Player != null)
                {
                    list.Add(new MenuItem() { Header = Properties.Resources.MatchRoomSlotViewModel_ContextItemSetHostText });
                    list.Add(new MenuItem() { Header = Properties.Resources.MatchRoomSlotViewModel_ContextItemKickText });
                }
                return list;
            }
        }
        #endregion

        #region Actions
        public void ChangeTeam()
        {
            if (match.TeamMode == TeamModes.TeamVS && Player != null)
            {
                localLog.Information("change team of player '{player}'", Player.Name);
                string color;
                if (Team == TeamColors.Red)
                    color = "blue";
                else
                    color = "red";
                OsuIrc.OsuIrc.SendMessage("#mp_" + match.RoomId, $"!mp team {Player.Name} {color}");
            }
        }

        public void Drag(MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && Player != null)
            {
                DragDrop.DoDragDrop((MatchRoomSlotView)GetView(), this, DragDropEffects.Move);
            }
        }

        public void ChangeSlot(DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(MatchRoomSlotViewModel)) && Player == null)
            {
                MatchRoomSlotViewModel oldSlot = e.Data.GetData(typeof(MatchRoomSlotViewModel)) as MatchRoomSlotViewModel;
                localLog.Information("drag player '{player}' to slot '{slot}'", oldSlot.Player.Name, SlotNumber);
                OsuIrc.OsuIrc.SendMessage("#mp_" + match.RoomId, $"!mp move {oldSlot.Player.Name} {SlotNumber}");
            }
        }

        public void MenuItemClick(MenuItem context)
        {
            localLog.Information("room slot '{slot}' click context menu item '{item}'", SlotNumber, context.Header);
            if (context.Header.ToString() == Properties.Resources.MatchRoomSlotViewModel_ContextItemSetHostText)
            {
                OsuIrc.OsuIrc.SendMessage("#mp_" + match.RoomId, $"!mp host {Player.Name}");
            }
            if (context.Header.ToString() == Properties.Resources.MatchRoomSlotViewModel_ContextItemKickText)
            {
                OsuIrc.OsuIrc.SendMessage("#mp_" + match.RoomId, $"!mp kick {Player.Name}");
            }
        }
        #endregion
    }
}
