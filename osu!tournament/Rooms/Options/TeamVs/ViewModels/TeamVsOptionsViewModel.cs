using Caliburn.Micro;
using Osu.Scores;
using Osu.Utils;
using System.Collections.Generic;

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
        /// First team red property
        /// </summary>
        public bool FirstTeamIsRed
        {
            get
            {
                return ranking.First == Api.OsuTeam.Red;
            }
            set
            {
                ranking.First = value ? Api.OsuTeam.Red : Api.OsuTeam.Blue;
                RefereeMatchHelper.GetInstance(room.Id).UpdateTeamBanOrder(room, ranking.First);
                NotifyOfPropertyChange(() => FirstTeamIsRed);
            }
        }

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
                return new List<string> { "3", "5", "7", "9", "11", "13" };
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
        #endregion

        #region Public Methods
        /// <summary>
        /// Updates the view model
        /// </summary>
        public void Update()
        {

        }
        #endregion
    }
}
