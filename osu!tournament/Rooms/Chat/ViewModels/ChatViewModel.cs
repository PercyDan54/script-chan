using Caliburn.Micro;
using Osu.Api;
using Osu.Ircbot;
using Osu.Mvvm.Miscellaneous;
using Osu.Scores;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Osu.Mvvm.Rooms.Ranking.TeamVs.ViewModels;

namespace Osu.Mvvm.Rooms.Chat.ViewModels
{
    public class ChatViewModel : PropertyChangedBase
    {
        #region Attributes
        /// <summary>
        /// The room
        /// </summary>
        private Room room;

        private string _messages;

        private MultiplayerCommandsViewModel commandsVM;
        #endregion

        #region Properties
        public string MultiplayerChat
        {
            get
            {
                return _messages;
            }
        }

        public string Message { get; set; }

        public MultiplayerCommandsViewModel MultiCommands
        {
            get
            {
                return commandsVM;
            }
            set
            {
                if (value != commandsVM)
                {
                    commandsVM = value;
                    NotifyOfPropertyChange(() => MultiCommands);
                }
            }
        }
        #endregion

        #region Constructor
        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="room">the room</param>
        public ChatViewModel(Room room, Osu.Scores.TeamVs ranking)
        {
            this.room = room;
            _messages = "";

            MultiCommands = new MultiplayerCommandsViewModel(room, ranking.Red.Name, ranking.Blue.Name);
        }

        public ChatViewModel(Room room)
        {
            this.room = room;
            _messages = "";

            MultiCommands = new MultiplayerCommandsViewModel(room);
        }
        #endregion

        #region Public Methods
        public void SendMessage()
        {
            room.RoomMessages.Add("=> " + Message);
            OsuIrcBot.GetInstancePrivate().SendMessage("#mp_" + room.Id, Message);
            Update();
            Message = string.Empty;
            NotifyOfPropertyChange(() => Message);
        }

        public void PressEnterToSend(ActionExecutionContext context)
        {
            var keyArgs = context.EventArgs as KeyEventArgs;

            if (keyArgs != null && keyArgs.Key == Key.Enter)
            {
                SendMessage();
            }
        }

        public void Update()
        {
            _messages = string.Empty;
            foreach(var message in room.RoomMessages.ToList())
            {
                _messages += message + "\n";
            }
            NotifyOfPropertyChange(() => MultiplayerChat);
        }
        #endregion
    }
}
