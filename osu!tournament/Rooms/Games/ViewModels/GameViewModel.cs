using Caliburn.Micro;
using Osu.Api;
using Osu.Scores;

namespace Osu.Mvvm.Rooms.Games.ViewModels
{
    public class GameViewModel : Screen
    {
        #region Attributes
        /// <summary>
        /// The room
        /// </summary>
        protected Room room;

        /// <summary>
        /// The game id
        /// </summary>
        protected long game_id;

        /// <summary>
        /// The beatmap id
        /// </summary>
        protected long beatmap_id;

        /// <summary>
        /// The beatmap name
        /// </summary>
        protected string beatmap_name;
        #endregion

        #region Constructor
        /// <summary>
        /// The Constructor
        /// </summary>
        /// <param name="room">The room</param>
        /// <param name="game_id">The game id</param>
        /// <param name="beatmap_id">The beatmap id</param>
        public GameViewModel(Room room, long game_id, long beatmap_id)
        {
            this.room = room;
            this.game_id = game_id;
            this.beatmap_id = beatmap_id;
            beatmap_name = "Please wait...";

            Update();
        }
        #endregion

        #region Properties
        /// <summary>
        /// Beatmap property
        /// </summary>
        public string Beatmap
        {
            get
            {
                return beatmap_name;
            }
        }

        /// <summary>
        /// Checked property
        /// </summary>
        public bool Checked
        {
            get
            {
                return !room.Blacklist.Contains(game_id);
            }
            set
            {
                if (value && room.Blacklist.Contains(game_id))
                    room.Blacklist.Remove(game_id);
                else if (!room.Blacklist.Contains(game_id))
                    room.Blacklist.Add(game_id);

                NotifyOfPropertyChange(() => Checked);
            }
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Updates the control
        /// </summary>
        private async void Update()
        {
            // Get the beatmap
            OsuBeatmap beatmap = await OsuApi.GetBeatmap(beatmap_id, false);

            // No beatmap
            if (beatmap == null)
                beatmap_name = Utils.Properties.Resources.GameView_MapNotFound;
            // A beatmap
            else
                beatmap_name = beatmap.Artist + " - " + beatmap.Title + " [" + beatmap.Version + "]";

            NotifyOfPropertyChange(() => Beatmap);
        }
        #endregion
    }
}
