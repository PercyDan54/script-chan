using Caliburn.Micro;
using Osu.Api;
using Osu.Mvvm.Miscellaneous;
using Osu.Mvvm.Rooms.Irc.ViewModels;
using Osu.Scores;
using System.Linq;

namespace Osu.Mvvm.Rooms.Players.ViewModels
{
    public class PlayersViewModel : Screen
    {
        #region Attributes
        /// <summary>
        /// The room
        /// </summary>
        protected Room room;

        /// <summary>
        /// The room
        /// </summary>
        protected IrcViewModel ircVM;

        /// <summary>
        /// The targets
        /// </summary>
        protected IObservableCollection<PlayerViewModel> players;

        protected OsuMode wctype;

        private IObservableCollection<OsuMode> cbModeItems = new BindableCollection<OsuMode>
        {
            OsuMode.Standard,
            OsuMode.Taiko,
            OsuMode.CTB,
            OsuMode.Mania
        };
        #endregion

        #region Constructors
        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="room">the room</param>
        /// <param name="ircVM">the irc view model</param>
        public PlayersViewModel(Room room, IrcViewModel ircVM)
        {
            this.room = room;
            this.players = new BindableCollection<PlayerViewModel>();
            this.ircVM = ircVM;
            this.wctype = OsuMode.Standard;

            Update();
        }
        #endregion

        #region Properties

        public IObservableCollection<OsuMode> CbModeItems
        {
            get { return cbModeItems; }
        }

        public OsuMode CbModeItem
        {
            get
            {
                return wctype;
            }
            set
            {
                wctype = value;
            }
        }

        /// <summary>
        /// Players property
        /// </summary>
        public IObservableCollection<PlayerViewModel> Players
        {
            get
            {
                return players;
            }
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Updates the view model
        /// </summary>
        public void Update()
        {
            players.Clear();

            // Add all players in the control
            foreach (Player player in room.Players.Values.ToList())
                players.Add(new PlayerViewModel(player));

            NotifyOfPropertyChange(() => Players);
        }

        /// <summary>
        /// Adds a new player
        /// </summary>
        public async void Add()
        {
            string input = await Dialog.ShowInput("Add Target", "Enter the player username");
            
            if (!string.IsNullOrEmpty(input))
            {
                await Dialog.ShowProgress("Please wait", "Trying to retrieve and register the player");

                OsuUser user = await OsuApi.GetUser(input, wctype, false);
                if (user == null)
                {
                    await Dialog.HideProgress();

                    Dialog.ShowDialog("Whoops!", "The player does not exist!");
                }
                else
                {
                    Player player = null;

                    if (!room.Players.ContainsKey(user.Id))
                    {
                        player = new Player();

                        player.OsuUser = user;
                        player.Playing = false;

                        room.Players.Add(user.Id, player);
                    }
                    else
                        player = room.Players[user.Id];

                    if (!room.IrcTargets.Contains(user.Username))
                    {
                        room.IrcTargets.Add(user.Username);
                        ircVM.Update();
                    }
                    Update();

                    await Dialog.HideProgress();
                }
            }
        }
        #endregion
    }
}
