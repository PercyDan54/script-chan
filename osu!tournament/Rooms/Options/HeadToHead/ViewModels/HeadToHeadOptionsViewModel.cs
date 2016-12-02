using Caliburn.Micro;
using Osu.Scores;
using System.Linq;
using System.Collections.Generic;

namespace Osu.Mvvm.Rooms.Options.HeadToHead.ViewModels
{
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
        #endregion
    }
}
