using Caliburn.Micro;
using Osu.Ircbot;
using Osu.Mvvm.Miscellaneous;
using Osu.Scores;
using Osu.Utils;
using Osu.Utils.Info;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Osu.Mvvm.Rooms.Ranking.TeamVs.ViewModels
{
    public class TeamVsRankingViewModel : Screen, IRankingViewModel
    {
        #region Attributes
        /// <summary>
        /// The room
        /// </summary>
        protected Room room;

        /// <summary>
        /// The team vs object linked
        /// </summary>
        protected Osu.Scores.TeamVs ranking;

        /// <summary>
        /// The current status
        /// </summary>
        protected string currentStatus;

        private MappoolPickerViewModel mpvm;

        private bool abortHappened;

        private bool isMovingWithMessage;

        private Dictionary<string, SwitchHandler> switchhandlers;
        #endregion

        #region Constructor
        public TeamVsRankingViewModel(Room room, Osu.Scores.TeamVs ranking)
        {
            this.room = room;
            this.ranking = ranking;
            abortHappened = false;
            switchhandlers = new Dictionary<string, SwitchHandler>();
            isMovingWithMessage = true;

            MappoolPicker = new MappoolPickerViewModel(room);

            Update();
        }
        #endregion

        #region Properties
        public string RedTeamName
        {
            get
            {
                return ranking.Red.Name;
            }
        }

        public string BlueTeamName
        {
            get
            {
                return ranking.Blue.Name;
            }
        }

        public string RedTeamScore
        {
            get
            {
                return (ranking.Red.Points + ranking.Red.PointAddition).ToString();
            }
        }

        public string BlueTeamScore
        {
            get
            {
                return (ranking.Blue.Points + ranking.Blue.PointAddition).ToString();
            }
        }

        public string CurrentStatus
        {
            get
            {
                return currentStatus;
            }
            set
            {
                if (value != currentStatus)
                {
                    currentStatus = value;
                    NotifyOfPropertyChange("CurrentStatus");
                }
            }
        }

        public MappoolPickerViewModel MappoolPicker
        {
            get
            {
                return mpvm;
            }
            set
            {
                if (value != mpvm)
                {
                    mpvm = value;
                    NotifyOfPropertyChange(() => MappoolPicker);
                }
            }
        }

        public bool IsMovingWithMessage
        {
            get
            {
                return isMovingWithMessage;
            }
            set
            {
                if (value != isMovingWithMessage)
                    isMovingWithMessage = value;
            }
        }

        public string IsControlVisible
        {
            get
            {
                if (room.Manual == false)
                    return "Visible";
                else
                    return "Hidden";
            }
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Updates the status label
        /// </summary>
        private void UpdateStatusLabel()
        {
            Team current = abortHappened ? ranking.NextTeam : ranking.CurrentTeam;
            Team next = abortHappened ? ranking.CurrentTeam : ranking.NextTeam;

            currentStatus = "Current Team: " + current.Name + " | Next Team: " + next.Name;
            NotifyOfPropertyChange(() => CurrentStatus);
            
        }
        #endregion

        #region Public Methods
        public void Update()
        {
            mpvm.Update();

            NotifyOfPropertyChange(() => RedTeamScore);
            NotifyOfPropertyChange(() => BlueTeamScore);
            NotifyOfPropertyChange(() => IsControlVisible);
            UpdateStatusLabel();
        }

        public void BlueAddPoint()
        {
            ranking.Blue.PointAddition++;
            Update();
        }

        public void RedAddPoint()
        {
            ranking.Red.PointAddition++;
            Update();
        }

        public void BlueRemovePoint()
        {
            ranking.Blue.PointAddition--;
            Update();
        }

        public void RedRemovePoint()
        {
            ranking.Red.PointAddition--;
            Update();
        }

        public void RevertPointChanges()
        {
            ranking.Red.PointAddition = 0;
            ranking.Blue.PointAddition = 0;
            Update();
        }

        public void AbortHappened()
        {
            OsuIrcBot.GetInstancePrivate().SendMessage("#mp_" + room.Id, "!mp abort");
            abortHappened = !abortHappened;
            ranking.DidAbortHappened = abortHappened;
            Dialog.ShowDialog("Whoops!", "Abort taken in consideration!");
        }

        public async void SwitchAll()
        {
            await Dialog.ShowProgress("Please wait", "Sending all switch commands...");
            string res1 = await Switch(BlueTeamName);
            string res2 = await Switch(RedTeamName);
            await Dialog.HideProgress();
            Dialog.ShowDialog("Switch done!", res1 + System.Environment.NewLine + res2);
        }

        public async void MoveAll()
        {
            await Dialog.ShowProgress("Please wait", "Sending all moving commands...");
            string res1 = Move(BlueTeamName, true);
            string res2 = Move(RedTeamName, false);
            if (isMovingWithMessage)
            {
                await Task.Delay(3000);
                OsuIrcBot.GetInstancePrivate().SendWelcomeMessage(room);
            }
            await Dialog.HideProgress();
            Dialog.ShowDialog("Switch done!", res1 + System.Environment.NewLine + res2);
        }

        private async Task<string> Switch(string name)
        {
            SwitchHandler sh = new SwitchHandler();

            if (sh.FillPlayerList(name))
            {
                var switchhandler = await OsuIrcBot.GetInstancePublic().SwitchPlayers(sh);
                string message = "[" + name + "]" + System.Environment.NewLine + "players switched successfully : " + System.Environment.NewLine;
                List<string> playerToAdd = new List<string>();
                foreach(var p in switchhandler.Players.FindAll(x => x.IsSwitched))
                {
                    playerToAdd.Add(p.Username);
                }
                message += string.Join(", ", playerToAdd) + System.Environment.NewLine;
                message += System.Environment.NewLine + "players not found : " + System.Environment.NewLine;
                playerToAdd.Clear();
                foreach (var p in switchhandler.Players.FindAll(x => !x.IsSwitched))
                {
                    playerToAdd.Add(p.Username);
                }
                message += string.Join(", ", playerToAdd) + System.Environment.NewLine;

                if (switchhandlers.ContainsKey(name))
                {
                    switchhandlers.GetValueOrDefault(name).UpdateWithNewSwitch(switchhandler);
                }
                else
                {
                    switchhandlers.Add(name, switchhandler);
                }

                return message;
            }
            else
            {
                return "Team not found" + name + " doesn't have any player stored in osu!players.db";
            }
        }

        private string Move(string name, bool isBlueTeam)
        {
            string message;
            var switchhandler = switchhandlers.GetValueOrDefault(name);
            if(switchhandler != null)
            {
                var firstp = switchhandler.Players.Find(x => x.Team == name && x.IsSwitched == true);
                if(firstp != null)
                {
                    int slotnumber = InfosHelper.TourneyInfos.PlayersPerTeam + 1;
                    OsuIrcBot.GetInstancePrivate().SendMessage("#mp_" + room.Id, string.Format("!mp move {0} {1}", firstp.Username, isBlueTeam ? "1" : slotnumber + ""));
                    message = name + " handled successfully";
                }
                else
                {
                    message = name + " handled with errors : no players connected";
                }
                
            }
            else
            {
                message = "You haven't switched players for team " + name;
            }
            return message;
        }

        public async void SwitchBlue()
        {
            await Dialog.ShowProgress("Please wait", "Sending all switch commands...");
            string res = await Switch(BlueTeamName);
            await Dialog.HideProgress();
            Dialog.ShowDialog("Switch done!", res);
        }

        public async void SwitchRed()
        {
            await Dialog.ShowProgress("Please wait", "Sending all switch commands...");
            string res = await Switch(RedTeamName);
            await Dialog.HideProgress();
            Dialog.ShowDialog("Switch done!", res);
        }

        public async void MoveBlue()
        {
            await Dialog.ShowProgress("Please wait", "Sending all move commands...");
            string res1 = Move(BlueTeamName, true);
            if (isMovingWithMessage)
            {
                await Task.Delay(3000);
                OsuIrcBot.GetInstancePrivate().SendWelcomeMessage(room);
            }
            await Dialog.HideProgress();
            Dialog.ShowDialog("Switch done!", res1);
        }

        public async void MoveRed()
        {
            await Dialog.ShowProgress("Please wait", "Sending all move commands...");
            string res2 = Move(RedTeamName, false);
            if(isMovingWithMessage)
            {
                await Task.Delay(3000);
                OsuIrcBot.GetInstancePrivate().SendWelcomeMessage(room);
            }
            await Dialog.HideProgress();
            Dialog.ShowDialog("Switch done!", res2);
        }

        public void SendStart()
        {
            OsuIrcBot.GetInstancePrivate().SendMessage("#mp_" + room.Id, "!mp start 5");
        }

        public void SendSettings()
        {
            OsuIrcBot.GetInstancePrivate().SendMessage("#mp_" + room.Id, "!mp settings");
        }

        public async void SendQuickSwitch()
        {
            await Switch(BlueTeamName);
            await Switch(RedTeamName);
        }
        #endregion
    }
}
