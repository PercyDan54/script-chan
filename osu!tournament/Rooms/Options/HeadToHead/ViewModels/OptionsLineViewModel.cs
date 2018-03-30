using Caliburn.Micro;
using Osu.Scores;
using System.Collections.Generic;
using System.Linq;

namespace Osu.Mvvm.Rooms.Options.HeadToHead.ViewModels
{
    public class OptionsLineViewModel : PropertyChangedBase
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
        /// The line number
        /// </summary>
        private int number;

        /// <summary>
        /// The selected player
        /// </summary>
        private Player selected;
        #endregion

        #region Constructors
        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="room"></param>
        /// <param name="number"></param>
        public OptionsLineViewModel(Room room, int number)
        {
            this.room = room;
            ranking = (Osu.Scores.HeadToHead)room.Ranking;
            this.number = number;

            if (ranking.Order.ContainsKey(number))
            {
                long id = ranking.Order[number];

                if (id == Osu.Scores.HeadToHead.NoPlayer)
                    selected = null;
                else
                    selected = room.Players[id];
            }
        }
        #endregion

        #region Properties
        /// <summary>
        /// Number property
        /// </summary>
        public int Number
        {
            get
            {
                return number;
            }
        }

        /// <summary>
        /// Number shown on UI screen
        /// </summary>
        public int NumberDisplayed
        {
            get
            {
                return number + 1;
            }
        }

        /// <summary>
        /// Players property
        /// </summary>
        public IEnumerable<Player> Players
        {
            get
            {
                return room.Players.Values.ToList();
            }
        }

        /// <summary>
        /// Selected player property
        /// </summary>
        public Player SelectedPlayer
        {
            get
            {
                return selected;
            }
            set
            {
                if (value != selected)
                {
                    selected = value;

                    if (selected == null)
                        ranking.Order[number] = Osu.Scores.HeadToHead.NoPlayer;
                    else
                        ranking.Order[number] = selected.Id;

                    NotifyOfPropertyChange(() => SelectedPlayer);
                }
            }
        }

        /// <summary>
        /// Points property
        /// </summary>
        public int Points
        {
            get
            {
                return ranking.Settings[number];
            }
            set
            {
                if (value != ranking.Settings[number])
                {
                    ranking.Settings[number] = value;
                    NotifyOfPropertyChange(() => Points);
                }
            }
        }
        #endregion
    }
}
