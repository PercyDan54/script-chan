using Caliburn.Micro;
using Osu.Scores;
using System.Linq;
using System.Collections.Generic;
using Osu.Api;
using System;
using Osu.Ircbot;

namespace Osu.Mvvm.Rooms.Options.HeadToHead.ViewModels
{
    /// <summary>
    /// The Head to head options view model
    /// </summary>
    public class HeadToHeadOptionsViewModel : PropertyChangedBase, IOptionsViewModel
    {
        #region Attributes
        /// <summary>
        /// The room
        /// </summary>
        private Room room;

        /// <summary>
        /// The ranking
        /// </summary>
        private Osu.Scores.HeadToHead ranking;

        /// <summary>
        /// The list of lines
        /// </summary>
        private IObservableCollection<OptionsLineViewModel> lines;
        #endregion

        #region Constructors
        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="room">the room</param>
        public HeadToHeadOptionsViewModel(Room room)
        {
            this.room = room;
            ranking = (Osu.Scores.HeadToHead)room.Ranking;
            lines = new BindableCollection<OptionsLineViewModel>();

            Update();
        }
        #endregion

        #region Properties
        /// <summary>
        /// Lines property
        /// </summary>
        public IObservableCollection<OptionsLineViewModel> Lines
        {
            get
            {
                return lines;
            }
        }

        /// <summary>
        /// The GameMode property for the room
        /// </summary>
        public List<OsuMode> GameMode
        {
            get
            {
                return new List<OsuMode>() { OsuMode.Standard, OsuMode.Taiko, OsuMode.CTB, OsuMode.Mania };
            }
        }

        /// <summary>
        /// The selectedgamemode property for the room
        /// </summary>
        public OsuMode SelectedGameMode
        {
            get
            {
                return room.Wctype;
            }
            set
            {
                if (room.Wctype != value)
                {
                    room.Wctype = value;
                    NotifyOfPropertyChange(() => SelectedGameMode);
                }
            }
        }

        /// <summary>
        /// The NotificationsEnabled for the room (sending events on discord and irc or not)
        /// </summary>
        public bool NotificationsEnabled
        {
            get
            {
                return room.NotificationsEnabled;
            }
            set
            {
                if (room.NotificationsEnabled != value)
                {
                    room.NotificationsEnabled = value;
                    NotifyOfPropertyChange(() => NotificationsEnabled);
                }
            }
        }
        
        /// <summary>
        /// The timer property
        /// </summary>
        public int Timer
        {
            get { return room.Timer; }
            set
            {
                if (room.Timer != value)
                {
                    room.Timer = Math.Abs(value);
                    NotifyOfPropertyChange(() => Timer);
                }
            }
        }

        /// <summary>
        /// The list of allowed BO number property
        /// </summary>
        public List<OsuTeamType> TeamModeBox
        {
            get
            {
                return new List<OsuTeamType> { OsuTeamType.HeadToHead, OsuTeamType.TeamVs };
            }
        }

        /// <summary>
        /// The selected BO property
        /// </summary>
        public OsuTeamType SelectedTeamModeBox
        {
            get
            {
                return room.RoomConfiguration.TeamMode;
            }

            set
            {
                if (room.RoomConfiguration.TeamMode != value)
                {
                    room.RoomConfiguration.TeamMode = value;
                    NotifyOfPropertyChange(() => SelectedTeamModeBox);
                }

            }
        }

        /// <summary>
        /// The list of allowed BO number property
        /// </summary>
        public List<OsuScoringType> ScoreModeBox
        {
            get
            {
                return new List<OsuScoringType> { OsuScoringType.Score, OsuScoringType.ScoreV2 };
            }
        }

        /// <summary>
        /// The selected BO property
        /// </summary>
        public OsuScoringType SelectedScoreModeBox
        {
            get
            {
                return room.RoomConfiguration.ScoreMode;
            }

            set
            {
                if (room.RoomConfiguration.ScoreMode != value)
                {
                    room.RoomConfiguration.ScoreMode = value;
                    NotifyOfPropertyChange(() => SelectedScoreModeBox);
                }

            }
        }

        /// <summary>
        /// The list of allowed BO number property
        /// </summary>
        public List<string> RoomSizeBox
        {
            get
            {
                return new List<string> { "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12", "13", "14", "15", "16" };
            }
        }

        /// <summary>
        /// The selected BO property
        /// </summary>
        public string SelectedRoomSizeBox
        {
            get
            {
                return room.RoomConfiguration.RoomSize;
            }

            set
            {
                if (room.RoomConfiguration.RoomSize != value)
                {
                    room.RoomConfiguration.RoomSize = value;
                    NotifyOfPropertyChange(() => SelectedScoreModeBox);
                }

            }
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Updates the view model
        /// </summary>
        public void Update()
        {
            // Clear our lines
            lines.Clear();

            // Add a line for each possible setting
            foreach (int number in ranking.Settings.Keys)
                lines.Add(new OptionsLineViewModel(room, number));

            // Notify the lines have changed
            NotifyOfPropertyChange(() => Lines);
        }

        public void UpdateRoomConfiguration()
        {
            OsuIrcBot.GetInstancePrivate().UpdateRoomConfiguration(room.Id.ToString(), room.RoomConfiguration);
        }
        #endregion
    }
}
