﻿using Caliburn.Micro;
using script_chan2.DataTypes;
using System.Windows.Media;

namespace script_chan2.GUI
{
    public class MatchPlayerListItemViewModel : Screen
    {
        #region Constructor
        public MatchPlayerListItemViewModel(Player player)
        {
            Player = player;
        }
        #endregion

        #region Properties
        public Player Player;

        public string Username
        {
            get { return $"{Player.Name} ({Player.Country})"; }
        }

        private bool hover = false;
        public SolidColorBrush Background
        {
            get
            {
                if (hover)
                    return Brushes.LightGray;
                return Brushes.Transparent;
            }
        }
        #endregion

        #region Actions
        public void MouseEnter()
        {
            hover = true;
            NotifyOfPropertyChange(() => Background);
        }

        public void MouseLeave()
        {
            hover = false;
            NotifyOfPropertyChange(() => Background);
        }
        #endregion
    }
}
