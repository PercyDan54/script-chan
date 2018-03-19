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
        #endregion

        #region Constructor
        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="room">the room</param>
        public ChatViewModel(Room room)
        {
            this.room = room;
            _messages = "";
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
