using log4net;
using Osu.Ircbot.Handlers;
using Osu.Scores;
using Osu.Utils;
using osu_discord;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using TechLifeForum;
using System.Linq;
using Osu.Utils.Info;

namespace Osu.Ircbot
{
    /// <summary>
    /// The bot which will connect to irc
    /// </summary>
    public delegate void MatchCreatedHandler(object sender, MatchCatchedArgs e);

    public class OsuIrcBot
    {
        #region Constants
        /// <summary>
        /// The default port of bancho's irc
        /// </summary>
        private const int PORT_IRC_BANCHO = 6667;

        /// <summary>
        /// The instance of the private irc server
        /// </summary>
        private static OsuIrcBot instanceMatch;

        /// <summary>
        /// The instance of the public irc server
        /// </summary>
        private static OsuIrcBot instancePublic;

        /// <summary>
        /// The logger
        /// </summary>
        private static ILog log = LogManager.GetLogger("osu!irc");
        #endregion

        #region Events
        /// <summary>
        /// The event handler for the token error
        /// </summary>
        public event EventHandler<EventArgs> TokenEvent;

        /// <summary>
        /// The event handler for messages from BanchoBot
        /// </summary>
        public event EventHandler MessageRoomCatched;

        public event EventHandler PlayerMessageRoomCatched;

        public event MatchCreatedHandler RoomCreatedCatched;

        private bool shouldCatchSettings;

        private System.Timers.Timer catchSettingsTimer;

        private Regex regexPlayerLine;

        private Regex regexRoomLine;

        private Regex regexMapLine;

        private Regex regexSwitchedLine;

        private Regex regexCreateCommand;

        private FreemodViewer fmv;

        private string irc_address;

        private string adminlist;
        #endregion

        #region Attributes
        /// <summary>
        /// The linked cache
        /// </summary>
        protected Cache cache;

        /// <summary>
        /// The irc handler
        /// </summary>
        protected IrcHandler handler;

        /// <summary>
        /// Boolean which is used to ignore the first error event after a wrong user/password
        /// </summary>
        protected bool ignoreErrorEvent;

        /// <summary>
        /// The irc client
        /// </summary>
        protected IrcClient client;

        /// <summary>
        /// The osu! username
        /// </summary>
        protected string username;

        /// <summary>
        /// The password which allows direct connection to bancho
        /// </summary>
        protected string password;

        protected bool isConnected;

        protected bool readInput;

        private bool shouldHlCommentators;
        #endregion

        #region Constructors
        /// <summary>
        /// Default constructor
        /// </summary>
        private OsuIrcBot(string ircIP, string admins, bool shouldRead)
        {
            cache = Cache.GetCache("osu!ircbot.db");
            handler = new AdminHandler();
            handler.RegisterBot(this);
            username = cache.Get("username", "");
            password = cache.Get("password", "");
            isConnected = false;
            shouldCatchSettings = false;
            fmv = new FreemodViewer();

            readInput = shouldRead;

            // Regex
            regexPlayerLine = new Regex("^Slot (\\d+)\\s+(\\w+)\\s+https:\\/\\/osu\\.ppy\\.sh\\/u\\/(\\d+)\\s+([a-zA-Z0-9_ ]+)\\s+\\[Team (\\w+)\\s*(?:\\/ ([\\w, ]+))?\\]$");
            regexRoomLine = new Regex("^Room name: ([^,]*), History:");
            regexMapLine = new Regex("Beatmap: [^ ]* (.*)");
            regexSwitchedLine = new Regex("^Switched ([a-zA-Z0-9_\\- ]+) to the tournament server$");
            regexCreateCommand = new Regex("^Created the tournament match https:\\/\\/osu\\.ppy\\.sh\\/mp\\/(\\d+)[^\\:]*: \\(([^\\)]*)\\) vs \\(([^\\)]*)\\)$");
            irc_address = ircIP;
            adminlist = admins;

            // Timer to catch mp settings infos
            catchSettingsTimer = new System.Timers.Timer();
            catchSettingsTimer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
            catchSettingsTimer.Interval = 3000;

            ignoreErrorEvent = false;
        }
        #endregion

        #region Properties
        /// <summary>
        /// Username property
        /// </summary>
        public string Username
        {
            get
            {
                return username;
            }
            set
            {
                username = value;
                cache["username"] = value;
            }
        }

        /// <summary>
        /// Password property
        /// </summary>
        public string Password
        {
            get
            {
                return password;
            }
            set
            {
                password = value;
                cache["password"] = value;
            }
        }

        public bool IsConnected
        {
            get
            {
                return isConnected;
            }
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Set all options for the connection
        /// </summary>
        protected void RegisterHandlers()
        {
            // set encoding to UTF8
            //client.Encoding = System.Text.Encoding.UTF8;

            // connect events to the API
            /*
            client.ChannelMessageRecieved += HandleOnChannelMessage;
            client.NetworkError += HandleOnNetworkError;
            client.ConnectionComplete += HandleOnConnectionComplete;
            client.RawMessageRecieved += HandleOnRawMessageRecieved;
            */

            client.ChannelMessage += (s, e) =>
            {
                log.Info(e.Channel + " " + e.From + " " + e.Message);
            };
            client.ChannelMessage += HandleOnChannelMessage;
            client.PrivateMessage += HandleOnPrivateMessage;

            client.ExceptionThrown += (s, e) =>
            {
                log.Info(e.Exception.Message);
                client.Disconnect();
                isConnected = false;
            };

            client.OnConnect += (s, e) =>
            {
                log.Info("IRC Bot has been successfully connected!");
                isConnected = true;
            };

            log.Info("The bot has been initialised!");
        }

        /// <summary>
        /// Calling the event handler
        /// </summary>
        private void FireTokenEvent()
        {
            if (TokenEvent != null)
            {
                TokenEvent(this, new EventArgs());
            }
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Connect to bancho's irc
        /// Return if the connection has been made or not
        /// </summary>
        /// <returns>a boolean</returns>
        public async Task<bool> Connect()
        {
            bool value = false;

            if (!isConnected)
            {
                await Task.Run(() =>
                {
                    // if we don't have the username or the password set
                    if (username == "" || password == "")
                        value = false;
                    else
                    {
                        client = new IrcClient(irc_address, readInput);

                        RegisterHandlers();
                        client.Nick = username;
                        client.ServerPass = password;
                        client.Connect();
                        value = true;
                        isConnected = true;
                    }
                });
            }

            return value;
        }

        /// <summary>
        /// Send a message to a list of users
        /// </summary>
        public void SendMessage(List<string> list_users, string message)
        {
            foreach (string user in list_users)
            {
                client.SendMessage(user, message);
            }
        }

        /// <summary>
        /// Send a message
        /// </summary>
        public void SendMessage(string user, string message)
        {
            if(client != null)
                client.SendMessage(user, message);
        }

        /// <summary>
        /// Join a channel with his name
        /// </summary>
        public void JoinChannel(string channel_name)
        {
            if (client != null)
                client.JoinChannel(channel_name);
        }

        /// <summary>
        /// Join a channel with his name
        /// </summary>
        public void LeaveChannel(string channel_name)
        {
            if (client != null)
                client.PartChannel(channel_name);
        }

        /// <summary>
        /// Disconnect from bancho's IRC
        /// </summary>
        public void Disconnect()
        {
            if (isConnected)
            {
                client.Disconnect();
                log.Info("IRC has been disconnected properly!");
                isConnected = false;
            }
        }

        public void SwitchPlayers(SwitchHandler sh)
        {
            foreach(var user in sh.Players)
            {
                SendMessage("BanchoBot", "!mp switch " + user.Username);
            }
        }

        public void SwitchPlayers(List<string> players)
        {
            foreach (var user in players)
            {
                SendMessage("BanchoBot", "!mp switch " + user);
            }
        }

        public void InvitePlayers(long id, SwitchHandler sh)
        {
            foreach (var user in sh.Players)
            {
                SendMessage("#mp_" + id, "!mp invite " + user.Username);
            }
        }

        public void InvitePlayers(long id, List<string> players)
        {
            foreach (var user in players)
            {
                SendMessage("#mp_" + id, "!mp invite " + user);
            }
        }

        public void CreateMatch(string blueteam, string redteam)
        {
            SendMessage("BanchoBot", string.Format("!mp make {2}: ({0}) vs ({1})", redteam, blueteam, InfosHelper.TourneyInfos.Acronym));
        }

        public void ConfigureMatch(string roomCreatedId)
        {
            if (roomCreatedId != null)
            {
                SendMessage("#mp_" + roomCreatedId, string.Format("!mp set {0} {1} {2}", InfosHelper.TourneyInfos.TeamMode, InfosHelper.TourneyInfos.ScoreMode, InfosHelper.TourneyInfos.RoomSize));
                SendMessage("#mp_" + roomCreatedId, string.Format("!mp map {0} {1}", InfosHelper.TourneyInfos.DefaultMapId, InfosHelper.TourneyInfos.ModeType));
                SendMessage("#mp_" + roomCreatedId, "!mp addref " + adminlist);
                SendMessage("#mp_" + roomCreatedId, "!mp unlock");
                SendMessage("#mp_" + roomCreatedId, "!mp settings");
            }
        }

        public void UpdateRoomConfiguration(string roomCreatedId, RoomConfiguration config)
        {
            SendMessage("#mp_" + roomCreatedId, string.Format("!mp set {0} {1} {2}", config.TeamMode, config.ScoreMode, config.RoomSize));
        }

        public void SendWelcomeMessage(Room room)
        {
            if(room.Ranking.GetType() == typeof(TeamVs))
            {
                TeamVs tvs = ((TeamVs)room.Ranking);
                SendMessage("#mp_" + room.Id, string.Format("{0} is RED, slots {2} to {3} --- {1} is BLUE, slots {4} to {5}", tvs.Red.Name, tvs.Blue.Name, "1", InfosHelper.TourneyInfos.PlayersPerTeam, InfosHelper.TourneyInfos.PlayersPerTeam + 1, InfosHelper.TourneyInfos.PlayersPerTeam + InfosHelper.TourneyInfos.PlayersPerTeam));
            }
            SendMessage("#mp_" + room.Id, "Please invite your teammates and sort yourself accordingly");
        }

        /// <summary>
        /// Called when a room is added
        /// </summary>
        /// <param name="room">the room</param>
        public void OnAddRoom(Room room)
        {
            log.Info("> Adding room " + room.Id);
            handler.OnAddRoom(room);
        }

        /// <summary>
        /// Called when a room is updated
        /// </summary>
        /// <param name="room">the room</param>
        public void OnUpdateRoom(Room room)
        {
            if (room.Ranking.Played != 0)
            {
                log.Info("> Updating room " + room.Id);
                handler.OnUpdateRoom(room);
            }
        }

        /// <summary>
        /// Called when a room is deleted
        /// </summary>
        /// <param name="room">the room</param>
        public void OnDeleteRoom(Room room)
        {
            log.Info("> Deleting room " + room.Id);
            handler.OnDeleteRoom(room);
        }

        /// <summary>
        /// Called when a room is changing map
        /// </summary>
        /// <param name="room">the room</param>
        public void OnChangeMapRoom(Room room, long map_id, Beatmap mod)
        {
            log.Info("> Changing map on room " + room.Id);
            handler.OnChangeMapRoom(room, map_id, mod);
        }
        #endregion

        #region Handlers
        /// <summary>
        /// Handler for messages from BanchoBot
        /// </summary>
        private void OnMessageRoomCatched(EventArgs e)
        {
            if (MessageRoomCatched != null)
                MessageRoomCatched(this, e);
        }

        /// <summary>
        /// Handler for messages from players in a room
        /// </summary>
        /// <param name="e"></param>
        private void OnPlayerMessageCatched(EventArgs e)
        {
            if (PlayerMessageRoomCatched != null)
                PlayerMessageRoomCatched(this, e);
        }

        /// <summary>
        /// Called on channel message
        /// </summary>
        /// <param name="sender">the sender</param>
        /// <param name="e">the arguments</param>
        private void HandleOnChannelMessage(object sender, ChannelMessageEventArgs e)
        {
            // Probably better to get bancho messages as well to the room chat..
            PlayerSpokeEventArgs playerEvent = new PlayerSpokeEventArgs() { MatchId = int.Parse(e.Channel.Substring(4)), PlayerName = e.From, Message = e.Message };
            OnPlayerMessageCatched(playerEvent);

            // If BanchoBot is speaking
            if (e.From.CompareTo("BanchoBot") == 0)
            {
                // If we catch that the match is finished
                if (e.Message.CompareTo("The match has finished!") == 0)
                {
                    // Creating the event and adding the channel and the id
                    MultiplayerEventArgs finishEvent = new MultiplayerEventArgs();
                    finishEvent.MatchId = int.Parse(e.Channel.Substring(4));
                    finishEvent.Event_Type = MultiEvent.EndMap;

                    // Sending the event
                    OnMessageRoomCatched(finishEvent);
                }
                else if(shouldCatchSettings)
                {
                    Match playerline = regexPlayerLine.Match(e.Message);
                    if(playerline.Success)
                    {
                        UserSettings us = new UserSettings() { Slot = int.Parse(playerline.Groups[1].Value), Status = playerline.Groups[2].Value, UserId = playerline.Groups[3].Value, Username = playerline.Groups[4].Value.Trim(), TeamColor = playerline.Groups[5].Value};
                        us.ModsSelected = playerline.Groups.Count != 7 ? "None" : playerline.Groups[6].Value;
                        fmv.AddPlayer(us);
                    }

                    Match roomline = regexRoomLine.Match(e.Message);
                    if(roomline.Success)
                    {
                        fmv.RoomString = roomline.Groups[1].Value;
                    }

                    Match mapline = regexMapLine.Match(e.Message);
                    if (mapline.Success)
                    {
                        fmv.MapString = mapline.Groups[1].Value;
                    }
                }
            }
            // A player is speaking
            else
            {
                if((e.From == client.Nick || IsMessageSentByAdmin(e.From)) && e.Message.Contains("settings discord"))
                {
                    shouldCatchSettings = true;
                    catchSettingsTimer.Enabled = true;
                    if(e.Message.Contains(" hl"))
                    {
                        shouldHlCommentators = true;
                    }
                }

                //BALLEK
                /*
                if (e.Message.ToLower().StartsWith("#map "))
                {
                    long id;
                    if (long.TryParse(e.Message.Split(' ')[1], out id))
                    {
                        // Creating the event and adding the channel and the id
                        MultiplayerEventArgs changeEvent = new MultiplayerEventArgs();
                        changeEvent.MatchId = int.Parse(e.Channel.Substring(4));
                        changeEvent.Event_Type = MultiEvent.ChangeMap;
                        changeEvent.Map_Id = id;

                        // Sending the event
                        OnMessageRoomCatched(changeEvent);
                    }
                }*/
            }
        }

        /// <summary>
        /// Event which catch all private messages from banchobot (switch command and create room)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void HandleOnPrivateMessage(object sender, PrivateMessageEventArgs e)
        {
            if(e.From == "BanchoBot")
            {
                // Switch command
                /*
                var switchCommand = regexSwitchedLine.Match(e.Message);
                if (switchCommand.Success && currentswitchhandler != null)
                {
                    currentswitchhandler.FoundPlayer(switchCommand.Groups[1].Value);
                }
                */

                // Create room command
                var createCommand = regexCreateCommand.Match(e.Message);
                if(createCommand.Success)
                {
                    await Task.Delay(3000);
                    RoomCreatedCatched(this, new MatchCatchedArgs() { Id = createCommand.Groups[1].Value, BlueTeam = createCommand.Groups[3].Value, RedTeam = createCommand.Groups[2].Value });
                }
            }
        }

        /// <summary>
        /// Event which catch the mp settings command and print it to discord
        /// </summary>
        /// <param name="source"></param>
        /// <param name="e"></param>
        private void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            catchSettingsTimer.Enabled = false;
            shouldCatchSettings = false;
            var stringToSend = "**FreeMod Pick** " + fmv.RoomString + Environment.NewLine + fmv.MapString + Environment.NewLine;
            List<string> values = new List<string>();

            int separator = 1;
            foreach (UserSettings us in fmv.Players.OrderBy(x => x.Slot))
            {
                values.Clear();
                if (us.ModsSelected.Contains(","))
                {
                    values.AddRange(us.ModsSelected.Split(new string[] { ", " }, StringSplitOptions.None));
                }
                else
                {
                    if(us.ModsSelected != "None")
                    {
                        values.Add(us.ModsSelected);
                    }
                }
                stringToSend += string.Format("[{0}] {1} ", us.TeamColor, us.Username);

                foreach(string mod in values)
                {
                    stringToSend += "`" + mod + "` ";
                }

                stringToSend += Environment.NewLine;

                if(separator == 4)
                {
                    stringToSend += "-----------------------" + Environment.NewLine;
                }
                separator++;
            }

            if (shouldHlCommentators)
            {
                stringToSend += InfosHelper.UserDataInfos.DiscordCommentatorGroup;
                shouldHlCommentators = false;
            }

            // TODO
            //DiscordBot.GetInstance().SendMessage(stringToSend);

            fmv.Players.Clear();
        }

        private bool IsMessageSentByAdmin(string name)
        {
            return adminlist.Contains(name.ToUpper());
        }
        #endregion

        #region Static Methods
        /// <summary>
        /// Initialize the osu!ircbot
        /// </summary>
        public static bool Initialize()
        {
            if(!string.IsNullOrEmpty(InfosHelper.UserDataInfos.IPPrivateBancho) && !string.IsNullOrEmpty(InfosHelper.UserDataInfos.Admins))
            {
                // Initialize the instance
                instanceMatch = new OsuIrcBot(InfosHelper.UserDataInfos.IPPrivateBancho, InfosHelper.UserDataInfos.Admins, true);
            }
            else
            {
                instanceMatch = null;
            }


            if(!string.IsNullOrEmpty(InfosHelper.UserDataInfos.IPPublicBancho) && !string.IsNullOrEmpty(InfosHelper.UserDataInfos.Admins))
            {
                instancePublic = new OsuIrcBot(InfosHelper.UserDataInfos.IPPublicBancho, InfosHelper.UserDataInfos.Admins, instanceMatch == null ? true : false);
                return true;
            }
            else
            {
                instanceMatch = new OsuIrcBot("", "", false);
                instancePublic = new OsuIrcBot("", "", false);
                return false;
            }
        }

        /// <summary>
        /// Returns the instance of the private osu!irc bot
        /// </summary>
        /// <returns>the unique instance</returns>
        public static OsuIrcBot GetInstancePrivate()
        {
            if (instanceMatch != null)
                return instanceMatch;
            else
                return instancePublic;
        }

        /// <summary>
        /// Returns the instance of the public osu!irc bot
        /// </summary>
        /// <returns>the unique instance</returns>
        public static OsuIrcBot GetInstancePublic()
        {
            return instancePublic;
        }
        #endregion
    }
}
