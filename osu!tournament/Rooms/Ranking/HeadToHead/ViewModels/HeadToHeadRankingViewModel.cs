using Caliburn.Micro;
using Osu.Mvvm.Rooms.Ranking.TeamVs.ViewModels;
using Osu.Scores;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Osu.Mvvm.Rooms.Ranking.HeadToHead.ViewModels
{
    /// <summary>
    /// Represents the head to head ranking view model
    /// </summary>
    public class HeadToHeadRankingViewModel : PropertyChangedBase, IRankingViewModel
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
        /// The status message
        /// </summary>
        private string status;

        /// <summary>
        /// The list of player lines
        /// </summary>
        private IObservableCollection<PlayerLineViewModel> lines;

        /// <summary>
        /// Mappool view model
        /// </summary>
        private MappoolPickerViewModel mpvm;
        #endregion

        #region Constructors
        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="room">the room</param>
        public HeadToHeadRankingViewModel(Room room)
        {
            this.room = room;
            ranking = (Osu.Scores.HeadToHead)room.Ranking;
            status = "Please wait...";
            lines = new BindableCollection<PlayerLineViewModel>();

            MappoolPicker = new MappoolPickerViewModel(room);

            Update();
        }
        #endregion

        #region Properties
        /// <summary>
        /// Status property
        /// </summary>
        public string Status
        {
            get
            {
                return status;
            }
            set
            {
                if (value != status)
                {
                    status = value;
                    NotifyOfPropertyChange(() => Status);
                }
            }
        }

        /// <summary>
        /// Lines property
        /// </summary>
        public IObservableCollection<PlayerLineViewModel> Lines
        {
            get
            {
                return lines;
            }
        }

        /// <summary>
        /// The MappoolPicker view model
        /// </summary>
        public MappoolPickerViewModel MappoolPicker
        {
            get
            {
                return mpvm;
            }
            set
            {
                if (value != mpvm)
                {
                    mpvm = value;
                    NotifyOfPropertyChange(() => MappoolPicker);
                }
            }
        }

        /// <summary>
        /// IsControlVisible property if mappool picker is visible or not
        /// </summary>
        public string IsControlVisible
        {
            get
            {
                if (room.Manual == false)
                    return "Visible";
                else
                    return "Hidden";
            }
        }

        /// <summary>
        /// RowSpanScore property when mappool picker is visible or not
        /// </summary>
        public string RowSpanScore
        {
            get
            {
                if (IsControlVisible == "Hidden")
                    return "2";
                else
                    return "1";
            }
        }

        /// <summary>
        /// SizePlayerList property when mappool picker is visible or not
        /// </summary>
        public string SizePlayerList
        {
            get
            {
                if(IsControlVisible == "Hidden")
                {
                    return "*";
                }
                else
                {
                    return "200";
                }
            }
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Updates the status message
        /// </summary>
        private void UpdateStatusMessage()
        {
            long current = ranking.Current;

            long next = ranking.Next;

            if (next == 0)
                Status = "No order defined";
            else
                Status = "Current player: " + (current == 0 ? "No map played yet" : room.Players[current].OsuUser.Username) + ". Next player: " + room.Players[next].OsuUser.Username + ".";
        }

        /// <summary>
        /// Updates the player lines
        /// </summary>
        private void UpdatePlayersLines()
        {
            lines.Clear();

            foreach (KeyValuePair<long, int> entry in ranking.Points.OrderByDescending(entry => entry.Value))
            {
                Player player = room.Players[entry.Key];

                if (player.Playing)
                {
                    int score = entry.Value;

                    lines.Add(new PlayerLineViewModel(player, score));
                }
            }

            NotifyOfPropertyChange(() => Lines);
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Updates the view model
        /// </summary>
        public void Update()
        {
            UpdateStatusMessage();
            UpdatePlayersLines();
            mpvm.Update();
            NotifyOfPropertyChange(() => IsControlVisible);
            NotifyOfPropertyChange(() => SizePlayerList);
            NotifyOfPropertyChange(() => RowSpanScore);
            NotifyOfPropertyChange(() => MappoolPicker);
        }

        /// <summary>
        /// Update the discord features on ranking tab
        /// </summary>
        public void DiscordUpdate()
        {
            MappoolPicker?.UpdateDiscord();
        }
        #endregion
    }
}
