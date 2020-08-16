using Caliburn.Micro;
using MaterialDesignThemes.Wpf;
using script_chan2.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace script_chan2.GUI
{
    public class MatchRoomOptionsDialogViewModel : Screen
    {
        #region Properties
        public List<GameModes> GameModesList
        {
            get { return Enum.GetValues(typeof(GameModes)).Cast<GameModes>().ToList(); }
        }

        private GameModes gameMode;
        public GameModes GameMode
        {
            get { return gameMode; }
            set
            {
                if (value != gameMode)
                {
                    gameMode = value;
                    NotifyOfPropertyChange(() => GameMode);
                }
            }
        }

        public List<WinConditions> WinConditionsList
        {
            get { return Enum.GetValues(typeof(WinConditions)).Cast<WinConditions>().ToList(); }
        }

        private WinConditions winCondition;
        public WinConditions WinCondition
        {
            get { return winCondition; }
            set
            {
                if (value != winCondition)
                {
                    winCondition = value;
                    NotifyOfPropertyChange(() => WinCondition);
                }
            }
        }

        private int roomSize;
        public int RoomSize
        {
            get { return roomSize; }
            set
            {
                if (value != roomSize)
                {
                    roomSize = value;
                    NotifyOfPropertyChange(() => RoomSize);
                    NotifyOfPropertyChange(() => SetEnabled);
                }
            }
        }

        private int teamSize;
        public int TeamSize
        {
            get { return teamSize; }
            set
            {
                if (value != teamSize)
                {
                    teamSize = value;
                    NotifyOfPropertyChange(() => TeamSize);
                    NotifyOfPropertyChange(() => SetEnabled);
                }
            }
        }

        public bool SetEnabled
        {
            get
            {
                if (RoomSize <= 0)
                    return false;
                if (TeamSize <= 0)
                    return false;
                if (TeamSize > RoomSize / 2)
                    return false;
                return true;
            }
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
