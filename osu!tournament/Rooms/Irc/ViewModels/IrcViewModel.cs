using Caliburn.Micro;
using Osu.Ircbot;
using Osu.Mvvm.Miscellaneous;
using Osu.Mvvm.Rooms.ViewModels;
using Osu.Scores;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Osu.Mvvm.Rooms.Irc.ViewModels
{
    public class IrcViewModel : Screen
    {
        #region Attributes
        /// <summary>
        /// The room
        /// </summary>
        protected RoomViewModel roomVM;

        /// <summary>
        /// The targets
        /// </summary>
        protected IObservableCollection<IrcPlayerViewModel> ircTargets;
        #endregion

        #region Constructor
        public IrcViewModel(RoomViewModel roomVM)
        {
            this.roomVM = roomVM;
            this.ircTargets = new BindableCollection<IrcPlayerViewModel>();

            // Update control
            Update();
        }
        #endregion

        #region Properties
        public IObservableCollection<IrcPlayerViewModel> IrcTargets
        {
            get
            {
                return ircTargets;
            }
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Updates the element
        /// </summary>
        public void Update()
        {
            ircTargets.Clear();

            foreach (Player player in roomVM.Room.Players.Values)
                ircTargets.Add(new IrcPlayerViewModel(player, roomVM.Room));

            NotifyOfPropertyChange(() => IrcTargets);
        }

        public async void SendButton()
        {
            // No bot
            if (!OsuIrcBot.GetInstancePrivate().IsConnected)
            {
                // Error
                Dialog.ShowDialog("Whoops!", "You're not connected to bancho!");
            }
            // Bot is connected
            else
            {
                // Show progress
                await Dialog.ShowProgress("Please wait", "Sending the message...");

                //  Foreach messages we want to send
                foreach (string sentence in roomVM.Room.Ranking.GetStatus())
                {
                    // Send message to targets
                    OsuIrcBot.GetInstancePrivate().SendMessage(roomVM.Room.IrcTargets, sentence);
                }

                // Hide progress
                await Dialog.HideProgress();

                // Show success
                Dialog.ShowDialog("Good!", "The message has been successfully sent");
            }
        }
        #endregion
    }
}
