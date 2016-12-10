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

namespace Osu.Ircbot
{
    /// <summary>
    /// The bot which will connect to irc
    /// </summary>
    public class OsuIrcBot
    {
        #region Constants
        /// <summary>
        /// The ip address of bancho's irc
        /// </summary>
        private const string IP_PUBLIC_BANCHO = "irc.ppy.sh";
        /// <summary>
        /// The private ip
        /// </summary>
        private const string IP_PRIVATE_BANCHO = "54.201.131.176";
        /// <summary>
        /// The default port of bancho's irc
        /// </summary>
        private const int PORT_IRC_BANCHO = 6667;

        /// <summary>
        /// The unique instance of the class
        /// </summary>
        private static OsuIrcBot instance;

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

        private bool shouldCatchSettings;

        private System.Timers.Timer catchSettingsTimer;

        private Regex regexPlayerLine;

        private Regex regexRoomLine;

        private Regex regexMapLine;

        private FreemodViewer fmv;
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
        private bool shouldHlCommentators;
        #endregion

        #region Constructors
        /// <summary>
        /// Default constructor
        /// </summary>
        private OsuIrcBot()
        {
            cache = Cache.GetCache("osu!ircbot.db");
            handler = new AdminHandler();
            handler.RegisterBot(this);
            username = cache.Get("username", "");
            password = cache.Get("password", "");
            isConnected = false;
            shouldCatchSettings = false;
            regexPlayerLine = new Regex("^Slot (\\d+)\\s+(\\w+)\\s+https:\\/\\/osu\\.ppy\\.sh\\/u\\/(\\d+)\\s+([a-zA-Z0-9_ ]+)\\s+\\[Team (\\w+)\\s*(?:\\/ ([\\w, ]+))?\\]$");
            regexRoomLine = new Regex("^Room name: ([^,]*), History:");
            regexMapLine = new Regex("Beatmap: [^ ]* (.*)");

            fmv = new FreemodViewer();


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
            client.ExceptionThrown += (s, e) =>
            {
                log.Info(e.Exception.Message);
                client.Disconnect();
                isConnected = false;
            };
            client.OnConnect += (s, e) =>
            {
                log.Info("IRC Bot has been successfully connected!");
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
                        client = new IrcClient(IP_PRIVATE_BANCHO);
                        RegisterHandlers();
                        client.Nick = username;
                        client.ServerPass = password;
                        client.Connect();
                        value = true;
                        isConnected = true;
                        //client = new IrcClient(IP_PRIVATE_BANCHO, new IrcUser(username, username, password));
                        //client.ConnectAsync();
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
            client.SendMessage(user, message);
        }

        /// <summary>
        /// Join a channel with his name
        /// </summary>
        public void JoinChannel(string channel_name)
        {
            client.JoinChannel(channel_name);
        }

        /// <summary>
        /// Join a channel with his name
        /// </summary>
        public void LeaveChannel(string channel_name)
        {
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
        /// Handler which is handling messages from BanchoBot
        /// </summary>
        private void OnMessageRoomCatched(EventArgs e)
        {
            if (MessageRoomCatched != null)
                MessageRoomCatched(this, e);
        }

        /// <summary>
        /// Called on channel message
        /// </summary>
        /// <param name="sender">the sender</param>
        /// <param name="e">the arguments</param>
        private void HandleOnChannelMessage(object sender, ChannelMessageEventArgs e)
        {
            //var test = e.Text;

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
                        UserSettings us = new UserSettings() { Slot = int.Parse(playerline.Groups[1].Value), Status = playerline.Groups[2].Value, UserId = playerline.Groups[3].Value, Username = playerline.Groups[4].Value, TeamColor = playerline.Groups[5].Value};
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
                if((e.From == client.Nick || IsSentByAdmin(e.From)) && e.Message.Contains("settings discord"))
                {
                    shouldCatchSettings = true;
                    catchSettingsTimer.Enabled = true;
                    if(e.Message.Contains(" hl"))
                    {
                        shouldHlCommentators = true;
                    }
                }

                //BALLEK
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
                }
            }
        }

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
                stringToSend += "<@&228232322288189440>";
                shouldHlCommentators = false;
            }

            DiscordBot.GetInstance().SendMessage(stringToSend);

            fmv.Players.Clear();
        }

        private bool IsSentByAdmin(string name)
        {
            var admins = "LOCTAV SHARPII P3N DEIF";
            return admins.Contains(name.ToUpper());
        }
        #endregion

        #region Static Methods
        /// <summary>
        /// Initialize the osu!ircbot
        /// </summary>
        public static void Initialize()
        {
            // Initialize the instance
            instance = new OsuIrcBot();
        }

        /// <summary>
        /// Returns the unique instance of the osu!irc bot
        /// </summary>
        /// <returns>the unique instance</returns>
        public static OsuIrcBot GetInstance()
        {
            return instance;
        }
        #endregion
    }
}
