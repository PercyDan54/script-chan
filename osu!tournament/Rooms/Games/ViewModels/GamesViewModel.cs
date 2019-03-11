using Caliburn.Micro;
using Osu.Api;
using Osu.Scores;
using System.Linq;
using System.Collections.Generic;

namespace Osu.Mvvm.Rooms.Games.ViewModels
{
    public class GamesViewModel : Screen
    {
        #region Attributes
        /// <summary>
        /// The room
        /// </summary>
        protected Room room;

        /// <summary>
        /// The beatmaps
        /// </summary>
        protected IObservableCollection<GameViewModel> beatmaps;
        #endregion

        #region Constructors
        /// <summary>
        /// The Constructor
        /// </summary>
        /// <param name="room">the room</param>
        public GamesViewModel(Room room)
        {
            this.room = room;
            this.beatmaps = new BindableCollection<GameViewModel>();

            Update();
        }
        #endregion

        #region Properties
        /// <summary>
        /// Beatmaps property
        /// </summary>
        public IObservableCollection<GameViewModel> Beatmaps
        {
            get
            {
                return beatmaps;
            }
            set
            {
                if (value != beatmaps)
                {
                    beatmaps = value;
                    NotifyOfPropertyChange("Beatmaps");
                }
            }
        }

        /// <summary>
        /// Mappools property
        /// </summary>
        public IEnumerable<Mappool> Mappools
        {
            get
            {
                return MappoolManager.Mappools.ToList();
            }
        }

        /// <summary>
        /// Selected mappool property
        /// </summary>
        public Mappool SelectedMappool
        {
            get
            {
                return room.Mappool;
            }
            set
            {
                if (value != room.Mappool)
                {
                    room.Mappool = value;
                    NotifyOfPropertyChange(() => SelectedMappool);
                }
            }
        }

        /// <summary>
        /// Mappool enabled property
        /// </summary>
        public bool MappoolsEnabled
        {
            get
            {
                return !room.Manual;
            }
        }

        /// <summary>
        /// Manual property
        /// </summary>
        public bool Manual
        {
            get
            {
                return room.Manual;
            }
            set
            {
                room.Manual = value;
                NotifyOfPropertyChange(() => Manual);
                NotifyOfPropertyChange(() => MappoolsEnabled);
                NotifyOfPropertyChange(() => WarmupEnabled);
            }
        }

        /// <summary>
        /// WarmupColor property
        /// </summary>
        public string WarmupColor
        {
            get
            {
                if (room.Warmup)
                    return "Green";
                else
                    return "Red";
            }
        }

        /// <summary>
        /// WarmupEnabled property
        /// </summary>
        public string WarmupEnabled
        {
            get
            {
                if (room.Manual)
                    return "Visible";
                else
                    return "Hidden";

            }
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Updates the control
        /// </summary>
        public void Update()
        {
            // Clear the game list
            Beatmaps.Clear();

            // If the osu!api is valid
            if (OsuApi.Valid)
                // For each game
                foreach (OsuGame game in room.OsuRoom.Games)
                    // Add a new game control
                    Beatmaps.Add(new GameViewModel(room, game.GameId, game.BeatmapId));
        }

        /// <summary>
        /// Updates the room status if we are in warmup mode or not to count the map
        /// </summary>
        public void WarmupStatus()
        {
            room.Warmup = !room.Warmup;
            NotifyOfPropertyChange(() => WarmupColor);
        }
        #endregion
    }
}
