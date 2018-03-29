using Caliburn.Micro;
using Osu.Api;
using Osu.Ircbot;
using Osu.Mvvm.Miscellaneous;
using Osu.Scores;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
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

        private FlowDocument _messages;

        private MultiplayerCommandsViewModel commandsVM;
        #endregion

        #region Properties
        public FlowDocument MultiplayerChat
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
            Execute.OnUIThread(() =>
            {
                _messages = new FlowDocument {
                    FontFamily = new FontFamily("Lucida Console"),
                    FontSize = 14,
                    TextAlignment = TextAlignment.Left
                };
            });

            MultiCommands = new MultiplayerCommandsViewModel(room, ranking.Red.Name, ranking.Blue.Name);
        }

        public ChatViewModel(Room room)
        {
            this.room = room;
            Execute.OnUIThread(() =>
            {
                _messages = new FlowDocument {
                    FontFamily = new FontFamily("Lucida Console"),
                    FontSize = 14,
                    TextAlignment = TextAlignment.Left
                };
            });

            MultiCommands = new MultiplayerCommandsViewModel(room);
        }
        #endregion

        #region Public Methods
        public void SendMessage()
        {
            OsuIrcBot.GetInstancePrivate().SendMessage("#mp_" + room.Id, Message);
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

        public void Update(bool scrollToNewLine)
        {
            Execute.OnUIThread(() =>
            {
                Paragraph newMessageParagraph = null;
                _messages.Blocks.Clear();
                foreach (var message in room.RoomMessages.ToList())
                {
                    Paragraph paragraph;
                    if (message.Message == "------------------ NEW MESSAGES ------------------")
                    {
                        paragraph = new Paragraph(new Run("------------------ NEW MESSAGES ------------------")) {Margin = new Thickness(135, 0, 0, 0), TextIndent = -135, Foreground = Brushes.Red, TextAlignment = TextAlignment.Center};
                        newMessageParagraph = paragraph;
                    }
                    else
                    {
                        paragraph = new Paragraph(new Run($"{message.User.PadRight(15)} {message.Message}")) {Margin = new Thickness(135, 0, 0, 0), TextIndent = -135};
                    }

                    _messages.Blocks.Add(paragraph);
                }

                NotifyOfPropertyChange(() => MultiplayerChat);

                if (scrollToNewLine && newMessageParagraph != null)
                    newMessageParagraph.Loaded += (sender, args) => newMessageParagraph.BringIntoView();
            });
        }
        #endregion
    }
}
