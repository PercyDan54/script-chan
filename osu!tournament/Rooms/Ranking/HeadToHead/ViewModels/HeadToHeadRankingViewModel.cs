using Caliburn.Micro;
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
        #endregion

        #region Private Methods
        /// <summary>
        /// Updates the status message
        /// </summary>
        private void UpdateStatusMessage()
        {
            long current = ranking.Current;

            long next = ranking.Next;

            if (current == 0 || next == 0)
                Status = "No order defined";
            else
                Status = "Current player: " + room.Players[current].OsuUser.Username + ". Next player: " + room.Players[next].OsuUser.Username + ".";
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
        }
        #endregion
    }
}
