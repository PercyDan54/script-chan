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
using IrcSharp;
using System.Linq;
using Osu.Utils.Info;

namespace Osu.Ircbot
{
    /// <summary>
    /// The delegate when a match has been created
    /// </summary>
    public delegate void MatchCreatedHandler(object sender, MatchCatchedArgs e);

    /// <summary>
    /// The class handling everything for osu! irc
    /// </summary>
    public class OsuIrcBot
    {
        #region Constants
        /// <summary>
        /// The default port of bancho's irc
        /// </summary>
        private const int PORT_IRC_BANCHO = 6667;

        /// <summary>
        /// The default public irc ip address
        /// </summary>
        private const string PUBLIC_IRC_BANCHO = "irc.ppy.sh";

        /// <summary>
        /// The instance of the private irc server
        /// </summary>
        private static OsuIrcBot instanceMatch;

        /// <summary>
        /// The instance of the public irc server
        /// </summary>
        private static OsuIrcBot instancePublic;

        /// <summary>
        /// The logger for irc
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

        /// <summary>
        /// The event handler when a player wrote something in the room
        /// </summary>
        public event EventHandler PlayerMessageRoomCatched;

        /// <summary>
        /// The event handler when Banchobot is sending the room creation string
        /// </summary>
        public event MatchCreatedHandler RoomCreatedCatched;

        /// <summary>
        /// Boolean if the bot should wait on IRC for Banchobot prints from mp settings
        /// </summary>
        private bool shouldCatchSettings;

        /// <summary>
        /// The timer to wait until we are sure we catched all lines from mp settings
        /// </summary>
        private System.Timers.Timer catchSettingsTimer;

        /// <summary>
        /// The timer to wait every x ms before sending a new message from the queue if there is one
        /// </summary>
        private System.Timers.Timer sendMessageTimer;

        /// <summary>
        /// The queue keeping IRC messages until they are sent
        /// </summary>
        private Queue<IrcMessage> messageQueue;

        /// <summary>
        /// The regex to catch players line on mp settings
        /// </summary>
        private Regex regexPlayerLine;

        /// <summary>
        /// The regex to grab room infos on mp settings
        /// </summary>
        private Regex regexRoomLine;

        /// <summary>
        /// The regex to grab the map in mp settings
        /// </summary>
        private Regex regexMapLine;

        /// <summary>
        /// The regex to grab who has been switched to the private server
        /// </summary>
        private Regex regexSwitchedLine;

        /// <summary>
        /// The regex to grab room creation
        /// </summary>
        private Regex regexCreateCommand;

        /// <summary>
        /// The freemod viewer
        /// </summary>
        private FreemodViewer fmv;

        /// <summary>
        /// The IRC IP Address
        /// </summary>
        private string irc_address;
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

        /// <summary>
        /// The amount of milliseconds to wait between sending messages
        /// </summary>
        protected long rateLimit;

        /// <summary>
        /// Boolean if we are connected to the irc server or not
        /// </summary>
        protected bool isConnected;

        /// <summary>
        /// Boolean to know if we have to read inputs incoming or not for this instance
        /// </summary>
        protected bool readInput;

        /// <summary>
        /// Boolean to know if we have to highlight commentators when we are grabbing mods with mp settings and displaying it on discord
        /// </summary>
        private bool shouldHlCommentators;
        #endregion

        #region Constructors
        /// <summary>
        /// Default constructor
        /// </summary>
        private OsuIrcBot(string ircIP, bool shouldRead)
        {
            cache = Cache.GetCache("osu!ircbot.db");
            handler = new AdminHandler();
            handler.RegisterBot(this);
            username = cache.Get("username", "");
            password = cache.Get("password", "");
            rateLimit = cache.Get<long>("ratelimit", 1000);
            isConnected = false;
            shouldCatchSettings = false;
            fmv = new FreemodViewer();
            messageQueue = new Queue<IrcMessage>();
            readInput = shouldRead;

            // Regex
            regexPlayerLine = new Regex("^Slot (\\d+)\\s+(\\w+)\\s+https:\\/\\/osu\\.ppy\\.sh\\/u\\/(\\d+)\\s+([a-zA-Z0-9_ ]+)\\s+\\[Team (\\w+)\\s*(?:\\/ ([\\w, ]+))?\\]$");
            regexRoomLine = new Regex("^Room name: ([^,]*), History:");
            regexMapLine = new Regex("Beatmap: [^ ]* (.*)");
            regexSwitchedLine = new Regex("^Switched ([a-zA-Z0-9_\\- ]+) to the tournament server$");
            regexCreateCommand = new Regex(@"^Created the tournament match https:\/\/osu\.ppy\.sh\/mp\/(\d+)[^\:]*: \(([^\)]*)\)\s\w+\s\(([^\)]*)\)$");
            irc_address = ircIP;

            // Timer to catch mp settings infos
            catchSettingsTimer = new System.Timers.Timer();
            catchSettingsTimer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
            catchSettingsTimer.Interval = 3000;

            // Timer to send messages
            sendMessageTimer = new System.Timers.Timer();
            sendMessageTimer.Elapsed += new ElapsedEventHandler(SendNextMessage);
            sendMessageTimer.Interval = rateLimit;
            sendMessageTimer.AutoReset = true;
            sendMessageTimer.Start();

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

        /// <summary>
        /// The rate limit used by the timer to send messages on IRC (interval)
        /// </summary>
        public long RateLimit
        {
            get
            {
                return rateLimit;
            }
            set
            {
                rateLimit = value;
                sendMessageTimer.Interval = rateLimit;
                cache["ratelimit"] = value;
            }
        }

        /// <summary>
        /// Properties to know if we are connected to irc or not
        /// </summary>
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
                messageQueue.Enqueue(new IrcMessage { User = user, Message = message });
            }
        }

        /// <summary>
        /// Send a message
        /// </summary>
        public void SendMessage(string user, string message)
        {
            if(client != null)
                messageQueue.Enqueue(new IrcMessage { User = user, Message = message });
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

        /// <summary>
        /// Switch players to the private server
        /// </summary>
        /// <param name="sh">Switchhandler containing the list of players who need to be switched</param>
        public void SwitchPlayers(SwitchHandler sh)
        {
            foreach(var user in sh.Players)
            {
                SendMessage("BanchoBot", "!mp switch " + user.Username);
            }
        }

        /// <summary>
        /// Switch players to the private server
        /// </summary>
        /// <param name="players">List containing the players who need to be switched</param>
        public void SwitchPlayers(List<string> players)
        {
            foreach (var user in players)
            {
                SendMessage("BanchoBot", "!mp switch " + user);
            }
        }

        /// <summary>
        /// Invite players to a room
        /// </summary>
        /// <param name="id">the room id</param>
        /// <param name="sh">the switch handled containing players who need to be invited</param>
        public void InvitePlayers(long id, SwitchHandler sh)
        {
            foreach (var user in sh.Players)
            {
                SendMessage("#mp_" + id, "!mp invite " + user.Username);
            }
        }

        /// <summary>
        /// Invite players to a room
        /// </summary>
        /// <param name="id">the room id</param>
        /// <param name="players">the list of players who need to be invited</param>
        public void InvitePlayers(long id, List<string> players)
        {
            foreach (var user in players)
            {
                SendMessage("#mp_" + id, "!mp invite " + user);
            }
        }

        /// <summary>
        /// Sending the command to create a room
        /// </summary>
        /// <param name="blueteam">the blue team name</param>
        /// <param name="redteam">the red team name</param>
        public void CreateMatch(string blueteam, string redteam)
        {
            // Grabbing the acronym from the overview tab
            SendMessage("BanchoBot", string.Format("!mp make {2}: ({0}) vs ({1})", redteam, blueteam, InfosHelper.TourneyInfos.Acronym));
        }

        /// <summary>
        /// Function called when the bot joined the room, configuring the room with tourney informations
        /// </summary>
        /// <param name="roomCreatedId">the room id</param>
        public void ConfigureMatch(string roomCreatedId)
        {
            if (roomCreatedId != null)
            {
                SendMessage("#mp_" + roomCreatedId, string.Format("!mp set {0} {1} {2}", InfosHelper.TourneyInfos.TeamMode, InfosHelper.TourneyInfos.ScoreMode, InfosHelper.TourneyInfos.RoomSize));
                SendMessage("#mp_" + roomCreatedId, string.Format("!mp map {0} {1}", InfosHelper.TourneyInfos.DefaultMapId, InfosHelper.TourneyInfos.ModeType));

                // If we have some names to addref
                if(!string.IsNullOrEmpty(InfosHelper.UserDataInfos.Admins))
                    SendMessage("#mp_" + roomCreatedId, "!mp addref " + InfosHelper.UserDataInfos.Admins);

                SendMessage("#mp_" + roomCreatedId, "!mp unlock");
                SendMessage("#mp_" + roomCreatedId, "!mp settings");
            }
        }

        /// <summary>
        /// Function called when we are updating the room manually from the option tab
        /// </summary>
        /// <param name="roomCreatedId">the room id</param>
        /// <param name="config">the room configuration from the option tab</param>
        public void UpdateRoomConfiguration(string roomCreatedId, RoomConfiguration config)
        {
            SendMessage("#mp_" + roomCreatedId, string.Format("!mp set {0} {1} {2}", config.TeamMode, config.ScoreMode, config.RoomSize));
        }

        /// <summary>
        /// Function called with the welcome button, send strings for slots
        /// </summary>
        /// <param name="room">the room object</param>
        public void SendWelcomeMessage(Room room)
        {
            // If we have teams, we give reserved slots to players
            if(room.Ranking.GetType() == typeof(TeamVs))
            {
                TeamVs tvs = ((TeamVs)room.Ranking);
                SendMessage("#mp_" + room.Id, string.Format("{0} is RED, slots {2} to {3} --- {1} is BLUE, slots {4} to {5}", tvs.Red.Name, tvs.Blue.Name, "1", InfosHelper.TourneyInfos.PlayersPerTeam, InfosHelper.TourneyInfos.PlayersPerTeam + 1, InfosHelper.TourneyInfos.PlayersPerTeam + InfosHelper.TourneyInfos.PlayersPerTeam));
            }

            SendMessage("#mp_" + room.Id, InfosHelper.TourneyInfos.WelcomeMessage);
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
                /*
                if((e.From == client.Nick || IsMessageSentByAdmin(e.From)) && e.Message.Contains("settings discord"))
                {
                    shouldCatchSettings = true;
                    catchSettingsTimer.Enabled = true;
                    if(e.Message.Contains(" hl"))
                    {
                        shouldHlCommentators = true;
                    }
                }
                */
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
        /// Event which catch the mp settings command and print it to discord, NOT USED ANYMORE
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

            // TODO, not used anymore
            //DiscordBot.GetInstance().SendMessage(stringToSend);

            fmv.Players.Clear();
        }

        /// <summary>
        /// Send message if one is queued
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SendNextMessage(object sender, ElapsedEventArgs e)
        {
            if (messageQueue.Count == 0) return;
            var ircMessage = messageQueue.Dequeue();
            if (ircMessage.User.StartsWith("#mp_"))
            {
                var playerEvent = new PlayerSpokeEventArgs {
                    MatchId = int.Parse(ircMessage.User.Substring(4)),
                    Message = ircMessage.Message,
                    PlayerName = username
                };
                OnPlayerMessageCatched(playerEvent);
            }
            client.SendMessage(ircMessage.User, ircMessage.Message);
        }

        #endregion

        #region Static Methods
        /// <summary>
        /// Initialize the osu!ircbot
        /// </summary>
        public static bool Initialize()
        {
            if(!string.IsNullOrEmpty(InfosHelper.UserDataInfos.IPPrivateBancho))
            {
                // Initialize the instance
                instanceMatch = new OsuIrcBot(InfosHelper.UserDataInfos.IPPrivateBancho, true);
            }
            else
            {
                instanceMatch = null;
            }

            instancePublic = new OsuIrcBot(PUBLIC_IRC_BANCHO, instanceMatch == null ? true : false);
            return true;
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
