using Caliburn.Micro;
using Osu.Ircbot;
using Osu.Mvvm.Miscellaneous;
using Osu.Scores;
using Osu.Utils;
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

        
        #endregion

        #region Constructor
        public TeamVsRankingViewModel(Room room, Osu.Scores.TeamVs ranking)
        {
            this.room = room;
            this.ranking = ranking;
            abortHappened = false;

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
            await Switch(BlueTeamName);
            await Switch(RedTeamName);
        }

        public void MoveAll()
        {
            MoveBlue();
            MoveRed();
        }

        public async Task<bool> Switch(string name)
        {
            SwitchHandler sh = new SwitchHandler();

            if(sh.FillPlayerList(name))
            {
                await Dialog.ShowProgress("Please wait", "Sending all switch commands...");
                SwitchHandler shnew = await OsuIrcBot.GetInstancePublic().SwitchPlayers(sh);
                string message = "Players switched successfully : " + System.Environment.NewLine;
                foreach(var p in shnew.Players.FindAll(x => x.IsSwitched))
                {
                    message += string.Format("{0}" + System.Environment.NewLine, p.Username);
                }
                message += System.Environment.NewLine + System.Environment.NewLine + "Players not found : " + System.Environment.NewLine;
                foreach (var p in shnew.Players.FindAll(x => !x.IsSwitched))
                {
                    message += string.Format("{0}" + System.Environment.NewLine, p.Username);
                }

                await Dialog.HideProgress();
                Dialog.ShowDialog("Success (" + name + ")", message);
                return true;
            }
            else
            {
                await Dialog.HideProgress();
                Dialog.ShowDialog("Team not found", BlueTeamName + " doesn't have any player stored in osu!players.db");
                return false;
            }
        }

        public async void SwitchBlue()
        {
            await Switch(BlueTeamName);
        }

        public async void SwitchRed()
        {
            await Switch(RedTeamName);
        }

        public void MoveBlue()
        {

        }

        public void MoveRed()
        {

        }
        #endregion
    }
}
