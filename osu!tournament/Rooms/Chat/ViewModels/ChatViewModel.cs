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

        private bool _autoScrollDisabled;

        private MultiplayerCommandsViewModel commandsVM;
        #endregion

        #region Properties
        public FlowDocument MultiplayerChat => _messages;

        public bool AutoScrollDisabled
        {
            get => _autoScrollDisabled;
            set
            {
                if (_autoScrollDisabled == value) return;
                _autoScrollDisabled = value;
                NotifyOfPropertyChange(() => AutoScrollDisabled);
                if (!value)
                {
                    Update(false);
                }
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
        public ChatViewModel(Room room, Osu.Scores.TeamVs ranking = null)
        {
            this.room = room;
            Execute.OnUIThread(() =>
            {
                _messages = new FlowDocument {
                    FontFamily = new FontFamily("Lucida Console"),
                    FontSize = 14,
                    TextAlignment = TextAlignment.Left
                };
                _messages.MouseWheel += (sender, args) => DisableAutoscrolling();
            });

            _autoScrollDisabled = false;

            if (ranking != null)
                MultiCommands = new MultiplayerCommandsViewModel(room, ranking.Red.Name, ranking.Blue.Name);
            else
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
                Paragraph paragraph = null;
                _messages.Blocks.Clear();
                foreach (var message in room.RoomMessages.ToList())
                {
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

                if (scrollToNewLine)
                {
                    _autoScrollDisabled = true;
                    NotifyOfPropertyChange(() => AutoScrollDisabled);
                }
                if (!_autoScrollDisabled && paragraph != null)
                    paragraph.Loaded += (sender, args) => paragraph.BringIntoView();
                else if (scrollToNewLine && newMessageParagraph != null)
                    newMessageParagraph.Loaded += (sender, args) => newMessageParagraph.BringIntoView();
            });
        }

        public void EnableAutoscrolling()
        {
            AutoScrollDisabled = false;
        }

        public void DisableAutoscrolling()
        {
            AutoScrollDisabled = true;
        }
        #endregion

        #region Private Methods
        private ScrollViewer FindScroll(Visual visual)
        {
            if (visual is ScrollViewer)
                return visual as ScrollViewer;

            ScrollViewer searchChild = null;
            DependencyObject child;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(visual); i++)
            {
                child = VisualTreeHelper.GetChild(visual, i);
                if (child is Visual)
                    searchChild = FindScroll(child as Visual);
                if (searchChild != null)
                    return searchChild;
            }

            return null;
        }
        #endregion
    }
}
