using Caliburn.Micro;
using Meebey.SmartIrc4net;
using script_chan2.DataTypes;
using script_chan2.Enums;
using script_chan2.GUI;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Timers;

namespace script_chan2.OsuIrc
{
    public static class OsuIrc
    {
        private static ILogger localLog = Log.ForContext(typeof(OsuIrc));
        private static ILogger log = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.File("logs\\irc.txt", rollingInterval: RollingInterval.Day)
            .CreateLogger();

        private static IrcClient client;
        private static IrcClient privateClient;

        private static Regex regexPlayerLine = new Regex("^Slot (\\d+)\\s+(\\w+)\\s+https:\\/\\/osu\\.ppy\\.sh\\/u\\/(\\d+)\\s+([a-zA-Z0-9_ ]+)\\s+\\[Team (\\w+)\\s*(?:\\/ ([\\w, ]+))?\\]$");
        private static Regex regexRoomLine = new Regex("^Room name: ([^,]*), History:");
        private static Regex regexMapLine = new Regex("Beatmap: [^ ]* (.*)");
        private static Regex regexSwitchedLine = new Regex("^Switched ([a-zA-Z0-9_\\- ]+) to the tournament server$");
        private static Regex regexCreateCommand = new Regex(@"^Created the tournament match https:\/\/osu\.ppy\.sh\/mp\/(\d+) (.*)$");

        private static System.Timers.Timer sendMessageTimer;
        private static Queue<IrcMessage> messageQueue;

        private static List<IrcMessage> messagesToSave = new List<IrcMessage>();

        public static void Login()
        {
            localLog.Information("login");

            if (privateClient != null && privateClient.IsConnected)
            {
                privateClient.RfcQuit();
                privateClient.Disconnect();
            }
            if (client != null && client.IsConnected)
            {
                client.RfcQuit();
                client.Disconnect();
            }

            if (Settings.IrcUsername == null || Settings.IrcPassword == null)
                return;

            try
            {
                ConnectionStatus = IrcStatus.Connecting;

                client = new IrcClient() { ActiveChannelSyncing = true };
                client.OnError += Client_OnError;
                client.OnErrorMessage += Client_OnErrorMessage;
                client.OnQueryMessage += Client_OnQueryMessage;
                client.OnChannelMessage += Client_OnChannelMessage;
                client.OnRegistered += Client_OnRegistered;
                client.OnDisconnected += Client_OnDisconnected;
                client.Connect(new string[] { "irc.ppy.sh" }, 6667);
                client.Login(Settings.IrcUsername, Settings.IrcUsername, 0, Settings.IrcUsername, Settings.IrcPassword);
                new Thread(new ThreadStart(client.Listen)).Start();

                if (Settings.EnablePrivateIrc && !string.IsNullOrEmpty(Settings.IrcIpPrivate))
                {
                    PrivateConnectionStatus = IrcStatus.Connecting;

                    privateClient = new IrcClient();
                    privateClient.OnError += PrivateClient_OnError;
                    privateClient.OnErrorMessage += Client_OnErrorMessage;
                    privateClient.OnQueryMessage += Client_OnQueryMessage;
                    privateClient.OnChannelMessage += Client_OnChannelMessage;
                    privateClient.OnRegistered += PrivateClient_OnRegistered;
                    privateClient.OnDisconnected += PrivateClient_OnDisconnected;
                    privateClient.Connect(new string[] { Settings.IrcIpPrivate }, 6667);
                    privateClient.Login(Settings.IrcUsername, Settings.IrcUsername, 0, Settings.IrcUsername, Settings.IrcPassword);
                    new Thread(new ThreadStart(privateClient.Listen)).Start();
                }

                messageQueue = new Queue<IrcMessage>();

                if (sendMessageTimer != null)
                    sendMessageTimer.Stop();
                sendMessageTimer = new System.Timers.Timer();
                sendMessageTimer.Elapsed += SendNextMessage;
                sendMessageTimer.Interval = Settings.IrcTimeout;
                sendMessageTimer.AutoReset = true;
                sendMessageTimer.Start();
            }
            catch (Exception ex)
            {
                localLog.Error(ex, "Irc exception caught");
            }
        }

        private static void Client_OnDisconnected(object sender, EventArgs e)
        {
            localLog.Information("disconnected");
            ConnectionStatus = IrcStatus.Disconnected;
        }

        private static void PrivateClient_OnDisconnected(object sender, EventArgs e)
        {
            localLog.Information("private disconnected");
            PrivateConnectionStatus = IrcStatus.Disconnected;
        }

        private static void Client_OnErrorMessage(object sender, IrcEventArgs e)
        {
            localLog.Information("[{bancho}] Error | {error}", sender == client ? "Bancho" : "Private Bancho", e.Data.Message);
            if (e.Data.Message.Contains("Bad authentication token"))
            {
                ConnectionStatus = IrcStatus.Disconnected;
                PrivateConnectionStatus = IrcStatus.Disconnected;
            }
        }

        private static void Client_OnChannelMessage(object sender, IrcEventArgs e)
        {
            log.Information("[{bancho}] {channel} | {user}: {message}", sender == client ? "Bancho" : "Private Bancho", e.Data.Channel, e.Data.Nick, e.Data.Message);

            var data = new ChannelMessageData()
            {
                Channel = e.Data.Channel,
                User = e.Data.Nick,
                Message = e.Data.Message
            };
            Events.Aggregator.PublishOnUIThread(data);

            var ircMessage = new IrcMessage() { Channel = data.Channel, User = data.User, Timestamp = DateTime.Now, Message = data.Message };
            var match = Database.Database.Matches.FirstOrDefault(x => "#mp_" + x.RoomId == data.Channel);
            if (match != null)
                ircMessage.Match = match;
            messagesToSave.Add(ircMessage);
            if (messagesToSave.Count >= 5)
            {
                Database.Database.AddIrcMessages(messagesToSave.ToList());
                messagesToSave.Clear();
            }
        }

        private static void Client_OnQueryMessage(object sender, IrcEventArgs e)
        {
            log.Information("[{bancho}] {user}: {message}", sender == client ? "Bancho" : "Private Bancho", e.Data.Nick, e.Data.Message);

            if (e.Data.Nick == "BanchoBot")
            {
                var createData = regexCreateCommand.Match(e.Data.Message);
                if (createData.Success)
                {
                    var data = new RoomCreatedData()
                    {
                        Id = Convert.ToInt32(createData.Groups[1].Value),
                        Name = createData.Groups[2].Value
                    };
                    Events.Aggregator.PublishOnUIThread(data);
                }
            }

            var data2 = new PrivateMessageData()
            {
                Channel = e.Data.Nick,
                User = e.Data.Nick,
                Message = e.Data.Message
            };

            var ircMessage = new IrcMessage() { Channel = data2.Channel, User = data2.User, Timestamp = DateTime.Now, Message = data2.Message };
            if (!ChatList.UserChats.Any(x => x.User == data2.Channel))
            {
                var newUserChat = new UserChat() { User = data2.Channel };
                newUserChat.LoadMessages();
                ChatList.UserChats.Add(newUserChat);
            }
            var userChat = ChatList.UserChats.First(x => x.User == ircMessage.Channel);
            userChat.AddMessage(ircMessage);

            messagesToSave.Add(ircMessage);
            if (messagesToSave.Count >= 5)
            {
                Database.Database.AddIrcMessages(messagesToSave.ToList());
                messagesToSave.Clear();
            }

            Events.Aggregator.PublishOnUIThread(data2);
        }

        public static void Shutdown()
        {
            Database.Database.AddIrcMessages(messagesToSave.ToList());
            if (client != null && client.IsConnected)
                client.Disconnect();
            if (privateClient != null && privateClient.IsConnected)
                privateClient.Disconnect();
        }

        private static void SendNextMessage(object sender, ElapsedEventArgs e)
        {
            if (messageQueue.Count == 0)
                return;

            var ircMessage = messageQueue.Dequeue();
            if (ircMessage.Message.StartsWith("/msg ") || ircMessage.Message.StartsWith("/notice ") || ircMessage.Message.StartsWith("/privmsg ") || ircMessage.Message.StartsWith("/query "))
            {
                var split = ircMessage.Message.Split(' ');
                if (split.Length > 2)
                {
                    ircMessage = new IrcMessage()
                    {
                        Channel = split[1],
                        Message = string.Join(" ", split.Skip(2)),
                        User = Settings.IrcUsername
                    };
                }
            }

            if (ircMessage.Channel == "Server")
                return;

            if (Settings.EnablePrivateIrc && !string.IsNullOrEmpty(Settings.IrcIpPrivate) && !ircMessage.Message.StartsWith("!mp switch"))
            {
                log.Information("[Private Bancho] {channel} | {user}: {message}", ircMessage.Channel, ircMessage.User, ircMessage.Message);
                privateClient.SendMessage(SendType.Message, ircMessage.Channel, ircMessage.Message);
            }
            else
            {
                log.Information("[Bancho] {channel} | {user}: {message}", ircMessage.Channel, ircMessage.User, ircMessage.Message);
                client.SendMessage(SendType.Message, ircMessage.Channel, ircMessage.Message);
            }

            if (ircMessage.Channel.StartsWith("#"))
            {
                var data = new ChannelMessageData()
                {
                    Channel = ircMessage.Channel,
                    User = Settings.IrcUsername,
                    Message = ircMessage.Message
                };
                Events.Aggregator.PublishOnUIThread(data);
            }
            else
            {
                var data = new PrivateMessageData()
                {
                    Channel = ircMessage.Channel,
                    User = Settings.IrcUsername,
                    Message = ircMessage.Message
                };

                if (!ChatList.UserChats.Any(x => x.User == ircMessage.Channel))
                {
                    var newUserChat = new UserChat() { User = ircMessage.Channel };
                    newUserChat.LoadMessages();
                    ChatList.UserChats.Add(newUserChat);
                }
                var userChat = ChatList.UserChats.First(x => x.User == ircMessage.Channel);
                userChat.AddMessage(ircMessage);

                Events.Aggregator.PublishOnUIThread(data);
            }

            messagesToSave.Add(ircMessage);
            if (messagesToSave.Count >= 5)
            {
                Database.Database.AddIrcMessages(messagesToSave.ToList());
                messagesToSave.Clear();
            }
        }

        private static void Client_OnError(object sender, ErrorEventArgs e)
        {
            localLog.Error(e.ErrorMessage, "irc exception caught");
            client.Disconnect();
            ConnectionStatus = IrcStatus.Disconnected;
        }

        private static void PrivateClient_OnError(object sender, ErrorEventArgs e)
        {
            localLog.Error(e.ErrorMessage, "private irc exception caught");
            client.Disconnect();
            PrivateConnectionStatus = IrcStatus.Disconnected;
        }

        private static void Client_OnRegistered(object sender, EventArgs e)
        {
            localLog.Information("connected");
            ConnectionStatus = IrcStatus.Connected;
        }

        private static void PrivateClient_OnRegistered(object sender, EventArgs e)
        {
            localLog.Information("private connected");
            PrivateConnectionStatus = IrcStatus.Connected;
        }

        private static IrcStatus connectionStatus;
        public static IrcStatus ConnectionStatus
        {
            get { return connectionStatus; }
            set
            {
                if (value != connectionStatus)
                {
                    connectionStatus = value;
                    var data = new IrcConnectedData();
                    Events.Aggregator.PublishOnUIThread(data);
                }
            }
        }

        private static IrcStatus privateConnectionStatus;
        public static IrcStatus PrivateConnectionStatus
        {
            get { return privateConnectionStatus; }
            set
            {
                if (value != privateConnectionStatus)
                {
                    privateConnectionStatus = value;
                    var data = new IrcConnectedData();
                    Events.Aggregator.PublishOnUIThread(data);
                }
            }
        }

        public static void SendMessage(string channel, string message)
        {
            var ircMessage = new IrcMessage { Channel = channel, Message = message, User = Settings.IrcUsername, Timestamp = DateTime.Now };
            var match = Database.Database.Matches.FirstOrDefault(x => "#mp_" + x.RoomId == channel);
            if (match != null)
                ircMessage.Match = match;
            messageQueue.Enqueue(ircMessage);
        }

        public static void JoinChannel(string channel)
        {
            localLog.Information("join channel '{name}'", channel);
            if (Settings.EnablePrivateIrc && !string.IsNullOrEmpty(Settings.IrcIpPrivate))
            {
                if (privateClient != null)
                    privateClient.RfcJoin(channel);
            }
            else
            {
                if (client != null)
                    client.RfcJoin(channel);
            }
        }

        public static void LeaveChannel(string channel)
        {
            localLog.Information("leave channel '{name}'", channel);
            if (Settings.EnablePrivateIrc && !string.IsNullOrEmpty(Settings.IrcIpPrivate))
            {
                if (privateClient != null)
                    privateClient.RfcPart(channel);
            }
            else
            {
                if (client != null)
                    client.RfcPart(channel);
            }
        }
    }
}
