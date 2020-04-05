using Caliburn.Micro;
using script_chan2.DataTypes;
using script_chan2.GUI;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Timers;
using TechLifeForum;

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

        private static Timer sendMessageTimer;
        private static Queue<IrcMessage> messageQueue;

        private static List<IrcMessage> messagesToSave = new List<IrcMessage>();

        public static void Login()
        {
            localLog.Information("login");
            IsConnected = false;
            if (Settings.IrcUsername == null || Settings.IrcPassword == null)
                return;
            try
            {
                if (client != null && client.Connected)
                    client.Disconnect();

                client = new IrcClient("irc.ppy.sh");
                client.ServerMessage += Client_ServerMessage;
                client.PrivateMessage += Client_PrivateMessage;
                client.ChannelMessage += Client_ChannelMessage;
                client.OnConnect += Client_OnConnect;
                client.ExceptionThrown += Client_ExceptionThrown;
                client.Nick = Settings.IrcUsername;
                client.ServerPass = Settings.IrcPassword;
                client.Connect();

                if (Settings.EnablePrivateIrc && !string.IsNullOrEmpty(Settings.IrcIpPrivate))
                {
                    if (privateClient != null && privateClient.Connected)
                        privateClient.Disconnect();

                    privateClient = new IrcClient(Settings.IrcIpPrivate);
                    privateClient.ServerMessage += Client_ServerMessage;
                    privateClient.PrivateMessage += Client_PrivateMessage;
                    privateClient.ChannelMessage += Client_ChannelMessage;
                    privateClient.OnConnect += Client_OnConnect;
                    privateClient.ExceptionThrown += Client_ExceptionThrown;
                    privateClient.Nick = Settings.IrcUsername;
                    privateClient.ServerPass = Settings.IrcPassword;
                    privateClient.Connect();
                }

                messageQueue = new Queue<IrcMessage>();

                if (sendMessageTimer != null)
                    sendMessageTimer.Stop();
                sendMessageTimer = new Timer();
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

        public static void Shutdown()
        {
            Database.Database.AddIrcMessages(messagesToSave);
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
                privateClient.SendMessage(ircMessage.Channel, ircMessage.Message);
            else
                client.SendMessage(ircMessage.Channel, ircMessage.Message);

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
                Database.Database.AddIrcMessages(messagesToSave);
                messagesToSave.Clear();
            }
        }

        private static void Client_ExceptionThrown(object sender, ExceptionEventArgs e)
        {
            localLog.Error(e.Exception, "Irc exception caught");
            client.Disconnect();
            IsConnected = false;
        }

        private static void Client_OnConnect(object sender, EventArgs e)
        {
            localLog.Information("connected");
            IsConnected = true;
        }

        private static void Client_ChannelMessage(object sender, ChannelMessageEventArgs e)
        {
            log.Information("#{channel} | {user}: {message}", e.Channel, e.From, e.Message);

            var data = new ChannelMessageData()
            {
                Channel = e.Channel,
                User = e.From,
                Message = e.Message
            };
            Events.Aggregator.PublishOnUIThread(data);

            var ircMessage = new IrcMessage() { Channel = data.Channel, User = data.User, Timestamp = DateTime.Now, Message = data.Message };
            var match = Database.Database.Matches.FirstOrDefault(x => "#mp_" + x.RoomId == data.Channel);
            if (match != null)
                ircMessage.Match = match;
            messagesToSave.Add(ircMessage);
            if (messagesToSave.Count >= 5)
            {
                Database.Database.AddIrcMessages(messagesToSave);
                messagesToSave.Clear();
            }
        }

        private static void Client_PrivateMessage(object sender, PrivateMessageEventArgs e)
        {
            log.Information("#{user}: {message}", e.From, e.Message);

            if (e.From == "BanchoBot")
            {
                var createData = regexCreateCommand.Match(e.Message);
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
                Channel = e.From,
                User = e.From,
                Message = e.Message
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
                Database.Database.AddIrcMessages(messagesToSave);
                messagesToSave.Clear();
            }

            Events.Aggregator.PublishOnUIThread(data2);
        }

        private static void Client_ServerMessage(object sender, StringEventArgs e)
        {
            log.Information("$server$: {message}", e.Result);

            var data = new PrivateMessageData()
            {
                Channel = "Server",
                User = "Server",
                Message = e.Result
            };

            var ircMessage = new IrcMessage() { Channel = data.Channel, User = data.User, Timestamp = DateTime.Now, Message = data.Message };
            var userChat = ChatList.UserChats.First(x => x.User == ircMessage.Channel);
            userChat.AddMessage(ircMessage);

            Events.Aggregator.PublishOnUIThread(data);
        }

        public static bool IsConnected { get; private set; } = false;

        public static void SendMessage(string channel, string message)
        {
            var ircMessage = new IrcMessage { Channel = channel, Message = message, User = Settings.IrcUsername };
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
                    privateClient.JoinChannel(channel);
            }
            else
            {
                if (client != null)
                    client.JoinChannel(channel);
            }
        }

        public static void LeaveChannel(string channel)
        {
            localLog.Information("leave channel '{name}'", channel);
            if (Settings.EnablePrivateIrc && !string.IsNullOrEmpty(Settings.IrcIpPrivate))
            {
                if (privateClient != null)
                    privateClient.PartChannel(channel);
            }
            else
            {
                if (client != null)
                    client.PartChannel(channel);
            }
        }
    }
}
