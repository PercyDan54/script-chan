using Caliburn.Micro;
using Osu.Api;
using Osu.Mvvm.Miscellaneous;
using Osu.Mvvm.Ov.ViewModels;
using Osu.Mvvm.Rooms.Chat.ViewModels;
using Osu.Mvvm.Rooms.Export.ViewModels;
using Osu.Mvvm.Rooms.Games.ViewModels;
using Osu.Mvvm.Rooms.Irc.ViewModels;
using Osu.Mvvm.Rooms.Options;
using Osu.Mvvm.Rooms.Options.HeadToHead.ViewModels;
using Osu.Mvvm.Rooms.Options.TeamVs.ViewModels;
using Osu.Mvvm.Rooms.Players.ViewModels;
using Osu.Mvvm.Rooms.Ranking;
using Osu.Mvvm.Rooms.Ranking.HeadToHead.ViewModels;
using Osu.Mvvm.Rooms.Ranking.TeamVs.ViewModels;
using Osu.Scores;
using System;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Osu.Mvvm.Rooms.ViewModels
{
    public class RoomViewModel : Screen
    {
        #region Attributes

        /// <summary>
        /// The room
        /// </summary>
        private Room room;

        /// <summary>
        /// The ranking vm
        /// </summary>
        private IRankingViewModel ranking;

        /// <summary>
        /// The options vm
        /// </summary>
        private IOptionsViewModel options;

        /// <summary>
        /// The chat vm
        /// </summary>
        private ChatViewModel chat;

        /// <summary>
        /// The games vm
        /// </summary>
        private GamesViewModel games;

        /// <summary>
        /// The games vm
        /// </summary>
        private PlayersViewModel players;

        /// <summary>
        /// The games vm
        /// </summary>
        private ExportViewModel export;

        /// <summary>
        /// The games vm
        /// </summary>
        private IrcViewModel irc;

        /// <summary>
        /// The overview room view model
        /// </summary>
        private OvRoomViewModel orvm;

        private TabItem selectedTab;
        #endregion

        #region Constructors
        /// <summary>
        /// Default constructor
        /// </summary>
        public RoomViewModel(Room room)
        {
            this.room = room;
            this.ranking = null;
            this.options = null;
            this.games = new GamesViewModel(room);
            this.export = new ExportViewModel(room);
            this.irc = new IrcViewModel(this);
            this.players = new PlayersViewModel(room, irc);

            UpdateRanking();
            Execute.OnUIThread(() => UpdateChat(false));
        }

        #endregion

        #region Properties
        /// <summary>
        /// Room property
        /// </summary>
        public Room Room
        {
            get
            {
                return room;
            }
            set
            {
                if (value != room)
                {
                    room = value;
                }
            }
        }

        /// <summary>
        /// Ranking property
        /// </summary>
        public IRankingViewModel Ranking
        {
            get
            {
                return ranking;
            }
            set
            {
                if (value != ranking)
                {
                    ranking = value;
                    NotifyOfPropertyChange(() => Ranking);
                }
            }
        }

        /// <summary>
        /// Options property
        /// </summary>
        public IOptionsViewModel Options
        {
            get
            {
                return options;
            }
            set
            {
                if (value != options)
                {
                    options = value;
                    NotifyOfPropertyChange(() => Options);
                }
            }
        }

        /// <summary>
        /// Games property
        /// </summary>
        public ChatViewModel Chat
        {
            get
            {
                return chat;
            }
            set
            {
                if (value != chat)
                {
                    chat = value;
                    NotifyOfPropertyChange(() => Chat);
                }
            }
        }

        /// <summary>
        /// Games property
        /// </summary>
        public GamesViewModel Games
        {
            get
            {
                return games;
            }
            set
            {
                if (value != games)
                {
                    games = value;
                    NotifyOfPropertyChange(() => Games);
                }
            }
        }

        /// <summary>
        /// Players property
        /// </summary>
        public PlayersViewModel Players
        {
            get
            {
                return players;
            }
            set
            {
                if (value != players)
                {
                    players = value;
                    NotifyOfPropertyChange(() => Players);
                }
            }
        }

        /// <summary>
        /// Export property
        /// </summary>
        public ExportViewModel Export
        {
            get
            {
                return export;
            }
            set
            {
                if (value != export)
                {
                    export = value;
                    NotifyOfPropertyChange(() => Export);
                }
            }
        }



        /// <summary>
        /// Irc property
        /// </summary>
        public IrcViewModel Irc
        {
            get
            {
                return irc;
            }
            set
            {
                if (value != irc)
                {
                    irc = value;
                    NotifyOfPropertyChange(() => Irc);
                }
            }
        }

        public OvRoomViewModel OverView
        {
            get
            {
                return orvm;
            }
            set
            {
                if (value != orvm)
                {
                    orvm = value;
                    orvm.Update();
                }
            }
        }

        public TabItem SelectedTab
        {
            get => selectedTab;
            set
            {
                if (selectedTab != null && selectedTab.Header.ToString() == Tournament.Properties.Resources.RoomView_Chat && value.Header.ToString() != Tournament.Properties.Resources.RoomView_Chat)
                {
                    RemoveNewMessageLine();
                }

                selectedTab = value;
            }
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Updates the ranking type of the room (Changes the ranking and options views)
        /// </summary>
        private void UpdateRanking()
        {
            if (room.Ranking is HeadToHead)
            {
                Ranking = new HeadToHeadRankingViewModel(room);
                Options = new HeadToHeadOptionsViewModel(room);
                Chat = new ChatViewModel(room);
            }
            else if (room.Ranking is TeamVs)
            {
                Ranking = new TeamVsRankingViewModel(Room, (TeamVs)room.Ranking);
                Options = new TeamVsOptionsViewModel(room);
                Chat = new ChatViewModel(room, (TeamVs)room.Ranking);
            }
            else
                throw new SystemException("RoomViewModel - UpdateRanking: Ranking type is not correct");
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Updates the view model
        /// </summary>
        public async Task Update(bool isIrcTrigger)
        {
            // Show progress
            await Dialog.ShowProgress(Tournament.Properties.Resources.Wait_Title, Tournament.Properties.Resources.Wait_UpdatingRoom);

            // No api
            if (!OsuApi.Valid)
            {
                // Hide progress
                await Dialog.HideProgress();

                // Error
                Dialog.ShowDialog(Tournament.Properties.Resources.Error_Title, Tournament.Properties.Resources.Error_ApiKeyInvalid);
            }
            // Api is set
            else
            {
                // Update the room
                await room.Update(isIrcTrigger);

                // Update the controls
                ranking.Update();
                options.Update();
                games.Update();
                players.Update();
                irc.Update();

                if (orvm != null)
                {
                    orvm.Update();
                }

                // Hide progress
                await Dialog.HideProgress();
            }
        }

        public void UpdateChat(bool scrollToNewLine)
        {
            chat.Update(scrollToNewLine);
        }

        /// <summary>
        /// Changes the room ranking type
        /// </summary>
        public async void ChangeRankingType()
        {
            if (await Dialog.ShowConfirmation(Tournament.Properties.Resources.RoomView_ChangeRankingTitle, Tournament.Properties.Resources.RoomView_ChangeRankingMessage))
            {
                room.ChangeRankingType();

                UpdateRanking();

                await Update(false);
            }
        }

        public void RemoveNewMessageLine()
        {
            room.RemoveNewMessageLine();
            Execute.OnUIThread(() => UpdateChat(false));
        }

        public void DiscordActivationChanged()
        {
            Ranking.DiscordUpdate();
        }
        #endregion
    }
}
