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

        public static void Login()
        {
            Log.Information("OsuIrc: login");
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
                Log.Information("OsuIrc: " + ex.Message);
            }
        }

        private static void SendNextMessage(object sender, ElapsedEventArgs e)
        {
            if (messageQueue.Count == 0)
                return;

            var ircMessage = messageQueue.Dequeue();
            if (Settings.EnablePrivateIrc && !string.IsNullOrEmpty(Settings.IrcIpPrivate) && !ircMessage.Message.StartsWith("!mp switch"))
                privateClient.SendMessage(ircMessage.User, ircMessage.Message);
            else
                client.SendMessage(ircMessage.User, ircMessage.Message);
        }

        private static void Client_ExceptionThrown(object sender, ExceptionEventArgs e)
        {
            Log.Error("OsuIrc: " + e.Exception.Message);
            client.Disconnect();
            IsConnected = false;
        }

        private static void Client_OnConnect(object sender, EventArgs e)
        {
            Log.Information("OsuIrc: connected");
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
        }

        private static void Client_ServerMessage(object sender, StringEventArgs e)
        {
            log.Information("$server$: {message}", e.Result);
        }

        public static bool IsConnected { get; private set; } = false;

        public static void SendMessage(string user, string message)
        {
            messageQueue.Enqueue(new IrcMessage { User = user, Message = message });
        }

        public static void JoinChannel(string channel)
        {
            Log.Information("OsuIrc: join channel '{name}'", channel);
            if (client != null)
                client.JoinChannel(channel);
        }

        public static void LeaveChannel(string channel)
        {
            Log.Information("OsuIrc: leave channel '{name}'", channel);
            if (client != null)
                client.PartChannel(channel);
        }
    }
}
