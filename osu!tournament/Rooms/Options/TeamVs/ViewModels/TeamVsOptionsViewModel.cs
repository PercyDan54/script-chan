using System;
using Caliburn.Micro;
using Osu.Api;
using Osu.Ircbot;
using Osu.Mvvm.Miscellaneous;
using Osu.Scores;
using Osu.Utils;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Osu.Mvvm.Rooms.Options.TeamVs.ViewModels
{
    public class TeamVsOptionsViewModel : Screen, IOptionsViewModel
    {
        #region Attributes
        /// <summary>
        /// The room
        /// </summary>
        protected Room room;

        /// <summary>
        /// The ranking
        /// </summary>
        protected Osu.Scores.TeamVs ranking;
        #endregion

        #region Constructor
        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="room">the room</param>
        public TeamVsOptionsViewModel(Room room)
        {
            this.room = room;
            ranking = (Osu.Scores.TeamVs)room.Ranking;

            Update();
        }
        #endregion

        #region Properties
        /// <summary>
        /// Enable commands property
        /// </summary>
        public bool EnableCommands
        {
            get
            {
                return room.Commands;
            }
            set
            {
                if (value != room.Commands)
                {
                    room.Commands = value;
                    NotifyOfPropertyChange(() => EnableCommands);
                }
            }
        }

        /// <summary>
        /// The list of allowed BO number property
        /// </summary>
        public List<string> Modes
        {
            get
            {
                return new List<string> { "3", "5", "7", "9", "11", "13", "15" };
            }
        }

        /// <summary>
        /// The selected BO property
        /// </summary>
        public string SelectedMode
        {
            get
            {
                return room.Mode;
            }

            set
            {
                if (room.Mode != value)
                {
                    room.Mode = value;
                    NotifyOfPropertyChange(() => SelectedMode);
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

        public List<OsuMode> GameMode
        {
            get
            {
                return new List<OsuMode>() { OsuMode.Standard, OsuMode.Taiko, OsuMode.CTB, OsuMode.Mania };
            }
        }

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
        /// The list of allowed BO number property
        /// </summary>
        public List<bool> FirstTeamIsRed
        {
            get
            {
                return new List<bool> { true, false };
            }
        }

        public bool SelectedFirstTeamIsRed
        {
            get
            {
                return ranking.First == Api.OsuTeam.Red;
            }
            set
            {
                ranking.First = value ? Api.OsuTeam.Red : Api.OsuTeam.Blue;
                RefereeMatchHelper.GetInstance(room.Id).UpdateTeamBanOrder(room, ranking.First);
                NotifyOfPropertyChange(() => SelectedFirstTeamIsRed);
            }
        }

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
        #endregion

        #region Public Methods
        /// <summary>
        /// Updates the view model
        /// </summary>
        public void Update()
        {

        }

        public void UpdateRoomConfiguration()
        {
            OsuIrcBot.GetInstancePrivate().UpdateRoomConfiguration(room.Id.ToString(), room.RoomConfiguration);
        }
        #endregion
    }
}
