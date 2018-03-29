using Caliburn.Micro;
using Osu.Api;
using Osu.Ircbot;
using Osu.Mvvm.Rooms.ViewModels;
using Osu.Scores;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Osu.Mvvm.Miscellaneous;
using Osu.Mvvm.Ov.ViewModels;
using osu_discord;
using Osu.Mvvm.General.ViewModels;
using Osu.Tournament.Ov.ViewModels;
using Osu.Utils;

namespace Osu.Mvvm.Rooms.ViewModels
{
    /// <summary>
    /// Represents the rooms screen
    /// </summary>
    public class RoomsViewModel : Conductor<RoomViewModel>.Collection.OneActive
    {
        #region Attributes
        /// <summary>
        /// The regex used to get the room's id if we get a link
        /// </summary>
        protected static readonly Regex LinkRegex = new Regex("(http|https):\\/\\/(?:osu|new)\\.ppy\\.sh\\/(?:mp|community\\/matches)\\/(\\d+)");

        /// <summary>
        /// The irc bot
        /// </summary>
        private OsuIrcBot bot;

        /// <summary>
        /// The discord bot
        /// </summary>
        private DiscordClient discordClient;

        /// <summary>
        /// The selected room
        /// </summary>
        private Room selected;

        /// <summary>
        /// The selected view model
        /// </summary>
        private RoomViewModel selected_view_model;
        
        /// <summary>
        /// The overview view model
        /// </summary>
        private OvViewModel overview;

        private MainViewModel mainview;
        #endregion

        #region Constructor
        /// <summary>
        /// Default constructor
        /// </summary>
        public RoomsViewModel(OvViewModel ov, MainViewModel mv)
        {
            DisplayName = "Current Room: ";
            selected = null;

            bot = OsuIrcBot.GetInstancePrivate();
            bot.MessageRoomCatched += ircbot_MessageRoomCatched;
            bot.PlayerMessageRoomCatched += ircbot_PlayerMessageRoomCatched;

            discordClient = DiscordClient.GetInstance();

            overview = ov;
            overview.MatchCreated += OnMatchCreated;

            mainview = mv;
        }

        private void OnMatchCreated(object sender, MatchCreatedArgs e)
        {
            AddRoom(e.Id);
        }
        #endregion

        #region Properties
        /// <summary>
        /// The list of rooms
        /// </summary>
        public IEnumerable<Room> Rooms
        {
            get
            {
                return Room.Rooms.Values.ToList();
            }
        }

        /// <summary>
        /// Selected mappool property
        /// </summary>
        public Room SelectedRoom
        {
            get
            {
                return selected;
            }
            set
            {
                if (value != selected)
                {
                    // Select this room
                    selected = value;

                    // If we have a selected view model
                    if (selected_view_model != null)
                        // Deactive it
                        DeactivateItem(selected_view_model, true);

                    // If the new selected room is not null
                    if (selected != null)
                        // Create a new view model
                        selected_view_model = new RoomViewModel(selected);
                    // Else
                    else
                        // Delete the view model
                        selected_view_model = null;

                    // Activate the selected view model (Which can be null
                    ActivateItem(selected_view_model);

                    // Selected mappool has changed
                    NotifyOfPropertyChange(() => SelectedRoom);

                    // We have a new mappool that can be deleted
                    NotifyOfPropertyChange(() => CanDeleteRoom);
                }
            }
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Checks if the add room button should be enabled
        /// </summary>
        /// <returns>true/false</returns>
        public bool CanAddRoom
        {
            get
            {
                return OsuApi.Valid;
            }
        }

        /// <summary>
        /// Adds a room
        /// </summary>
        public async void AddRoom()
        {
            // Get the room id
            string input = await Dialog.ShowInput("Add Room", "Enter the room id");

            // If we have an input
            if (input != null && !string.IsNullOrEmpty(input))
            {
                // Show the progress dialog
                await Dialog.ShowProgress("Please wait", "Retrieving and creating the room...");

                // Parse an id from the input
                long id = -1;

                // Check if the input is a link or not
                Match match = LinkRegex.Match(input);

                // if we have a link, replace the link by the id
                if (match.Success)
                    input = match.Groups[2].Value;

                // Not a number
                if (!long.TryParse(input, out id))
                {
                    // Hide progress
                    await Dialog.HideProgress();

                    // Error
                    Dialog.ShowDialog("Whoops!", "The entered id is not a valid number!");
                }
                // Valid number
                else
                {
                    // If the id already exists
                    if (Room.Rooms.ContainsKey(id))
                    {
                        // Hide progress
                        await Dialog.HideProgress();

                        // Error
                        Dialog.ShowDialog("Whoops!", "This game is already in the list!");
                    }
                    // Else
                    else
                    {
                        // Create a new room
                        Room room = await Room.Get(id);

                        // No room
                        if (room == null)
                        {
                            // Hide progress
                            await Dialog.HideProgress();

                            // Error
                            Dialog.ShowDialog("Whoops!", "The room does not exist!");
                        }
                        // A room
                        else
                        {
                            // Update the room
                            await room.Update(false);

                            // Notify
                            Log.Info("Adding room \"" + room.Name + "\"");

                            // Change the currently selected mappool
                            SelectedRoom = room;

                            // Trying to connect to the channel
                            bot.OnAddRoom(room);

                            // Mappool list has changed
                            NotifyOfPropertyChange(() => Rooms);

                            // Add the room in overview
                            selected_view_model.OverView = overview.addOverview(room);

                            // UpdateTeamBanOrder with osuteam.blue which is the first team to pick
                            RefereeMatchHelper.GetInstance(room.Id).UpdateTeamBanOrder(room, OsuTeam.Blue);

                            // Hide progress
                            await Dialog.HideProgress();
                        }
                    }
                }
            }
        }

        private async void AddRoom(long id)
        {
            // Create a new room
            Room room = await Room.Get(id);

            // Update the room
            await room.Update(false);

            // Notify
            Log.Info("Adding room \"" + room.Name + "\"");

            // Change the currently selected mappool
            SelectedRoom = room;

            // Trying to connect to the channel
            bot.OnAddRoom(room);

            // Mappool list has changed
            NotifyOfPropertyChange(() => Rooms);

            // Add the room in overview
            selected_view_model.OverView = overview.getOverview(room);

            // UpdateTeamBanOrder with osuteam.blue which is the first team to pick
            RefereeMatchHelper.GetInstance(room.Id).UpdateTeamBanOrder(room, OsuTeam.Blue);
        }

        /// <summary>
        /// Checks if we can delete a room
        /// </summary>
        /// <returns>true/false</returns>
        public bool CanDeleteRoom
        {
            get
            {
                return SelectedRoom != null;
            }
        }

        /// <summary>
        /// Deletes the room
        /// </summary>
        public async void DeleteRoom()
        {
            if (await Dialog.ShowConfirmation("Delete room", "Are you sure you want to delete the room ?"))
            {
                Log.Info("Deleting room \"" + SelectedRoom.Name + "\"");

                // Notify the overview
                overview.removeOverview(SelectedRoom);

                // Trying to disconnect to the channel
                bot.OnDeleteRoom(SelectedRoom);

                // Remove the selected room
                Room.Remove(selected.Id);

                // Notify rooms list has changed
                NotifyOfPropertyChange(() => Rooms);

                // Select the first mappool from our list (Or null if there is no mappool)
                SelectedRoom = Room.Rooms.Values.ToList().Count == 0 ? null : Room.Rooms.First().Value;
            }
        }
        #endregion

        #region Handlers
        /// <summary>
        /// Called on activate
        /// </summary>
        protected override void OnActivate()
        {
            base.OnActivate();
        }

        /// <summary>
        /// Called on deactivate
        /// </summary>
        /// <param name="close">if the deactivation was a close operation</param>
        protected override void OnDeactivate(bool close)
        {
            base.OnDeactivate(close);
        }

        /// <summary>
        /// Called when the irc bot fires a room update event
        /// </summary>
        /// <param name="sender">the sender</param>
        /// <param name="e">the arguments</param>
        private async void ircbot_MessageRoomCatched(object sender, EventArgs e)
        {
            // Getting event type and informations
            MultiplayerEventArgs multi_room = (MultiplayerEventArgs)e;

            // If the beatmap just finished
            if (multi_room.Event_Type == MultiEvent.EndMap)
            {
                try
                {
                    // If it's the selected room, we're updating the room and UI + sending messages
                    if (selected.Id == multi_room.MatchId)
                    {
                        Caliburn.Micro.Execute.OnUIThread((async () =>
                        {
                            await selected_view_model.Update(true);
                            if (SelectedRoom.NotificationsEnabled)
                            {
                                bot.OnUpdateRoom(SelectedRoom);
                                discordClient.OnUpdateRoom(SelectedRoom);
                            }
                        }));
                    }
                    // It's not the selected room, we're updating the room + sending messages
                    else
                    {
                        Room room = null;
                        if (Room.Rooms.TryGetValue(multi_room.MatchId, out room))
                        {
                            await room.Update(true);

                            room.Playing = false;

                            overview.Update(room);

                            if (room.NotificationsEnabled)
                            {
                                bot.OnUpdateRoom(room);

                                discordClient.OnUpdateRoom(room);
                            }
                        }
                    }
                }
                catch(Exception ee)
                {
                    Log.Fatal("ERROR UPDATE : " + ee.Message + " " + ee.StackTrace);
                }
            }
            // If we're getting the command for changing map
            else if (multi_room.Event_Type == MultiEvent.ChangeMap)
            {
                Room room = null;
                if (Room.Rooms.TryGetValue(multi_room.MatchId, out room))
                {
                    // If we allow to use commands in this room and if we have selected a mappool for this room
                    if (room.Commands && !room.Manual)
                    {
                        Beatmap beatmap;
                        // If the beatmap exists in the mappool, we're changing the map
                        if (room.Mappool.Pool.TryGetValue(multi_room.Map_Id, out beatmap))
                        {
                            OsuIrcBot.GetInstancePrivate().OnChangeMapRoom(room, multi_room.Map_Id, beatmap);
                        }
                    }  
                }
            }
        }

        private async void ircbot_PlayerMessageRoomCatched(object sender, EventArgs e)
        {
            // Getting event type and informations
            PlayerSpokeEventArgs multi_room = (PlayerSpokeEventArgs)e;

            Room room = null;
            if (Room.Rooms.TryGetValue(multi_room.MatchId, out room))
            {
                Execute.OnUIThread(() =>
                {
                    if (mainview.ActiveItemName != "Rooms" || selected == null || selected.Id != multi_room.MatchId || selected_view_model?.SelectedTab != null && selected_view_model.SelectedTab.Header.ToString() != "Chat")
                        room.AddNewMessageLine();

                    room.AddMessage(new IrcMessage { Message = multi_room.Message, User = multi_room.PlayerName });

                    if (mainview.ActiveItemName == "Rooms" && selected != null && selected.Id == multi_room.MatchId && selected_view_model?.SelectedTab != null && selected_view_model.SelectedTab.Header.ToString() == "Chat")
                        selected_view_model.UpdateChat(false);
                    else
                        selected_view_model.UpdateChat(true);
                });
            }
        }
        #endregion
    }
}
