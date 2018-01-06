using Caliburn.Micro;
using Osu.Ircbot;
using Osu.Mvvm.Miscellaneous;
using Osu.Scores;
using Osu.Scores.Status;
using Osu.Tournament.Ov.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace Osu.Mvvm.Ov.ViewModels
{
    public delegate void MatchCreatedEvent(object sender, MatchCreatedArgs e);

    public class OvRoomViewModel : PropertyChangedBase
    {
        #region Attributes
        /// <summary>
        /// The room
        /// </summary>
        protected Room room;

        /// <summary>
        /// The timer
        /// </summary>
        protected Timer timer;

        /// <summary>
        /// The duration of the game
        /// </summary>
        protected int duration;

        protected string blueteam;

        protected string redteam;

        protected string batch;

        protected long roomId;

        public event MatchCreatedEvent MatchCreated;
        #endregion

        #region Constructor
        /// <summary>
        /// The constructor
        /// </summary>
        /// <param name="room">the room</param>
        public OvRoomViewModel(Room room)
        {
            this.room = room;
            this.roomId = room.Id;
            IsCreated = true;

            Initialize();
        }

        public OvRoomViewModel(string teamblue, string teamred, string b)
        {
            blueteam = teamblue;
            redteam = teamred;
            batch = b;
            IsCreated = false;

            Initialize();
        }

        private void Initialize()
        {
            this.timer = new Timer(60000);

            this.timer.Elapsed += new ElapsedEventHandler(timer_Elapsed);

            this.duration = 0;

            if(IsCreated)
            {
                if (room.Status != RoomStatus.NotStarted && room.Status != RoomStatus.Finished)
                {
                    StartTimer();
                }
            }
        }
        #endregion

        #region Properties
        /// <summary>
        /// Room property
        /// </summary>
        public Room Room
        {
            get
            {
                return room;
            }
            set
            {
                if(value != room)
                {
                    room = value;
                }
            }
        }

        public long RoomId
        {
            get
            {
                return roomId;
            }
        }

        /// <summary>
        /// Blue team name property
        /// </summary>
        public String TeamBlue
        {
            get
            {
                if(blueteam != null)
                {
                    return blueteam;
                }
                else
                {
                    return ((TeamVs)room.Ranking).Blue.Name;
                }
            }
        }

        /// <summary>
        /// Blue team score property
        /// </summary>
        public String ScoreBlue
        {
            get
            {
                if (room != null)
                    return "" + (((TeamVs)room.Ranking).Blue.Points + ((TeamVs)room.Ranking).Blue.PointAddition);
                else
                    return string.Empty;
            }
        }

        /// <summary>
        /// Red team name property
        /// </summary>
        public String TeamRed
        {
            get
            {
                if (redteam != null)
                    return redteam;
                else
                    return ((TeamVs)room.Ranking).Red.Name;
            }
        }

        /// <summary>
        /// Red team score property
        /// </summary>
        public String ScoreRed
        {
            get
            {
                if (room != null)
                    return "" + (((TeamVs)room.Ranking).Red.Points + ((TeamVs)room.Ranking).Red.PointAddition);
                else
                    return string.Empty;
            }
        }

        /// <summary>
        /// Room's status property
        /// </summary>
        public String Status
        {
            get
            {
                if(room != null)
                {
                    if (room.Status == RoomStatus.Playing)
                    {
                        if (Room.Playing)
                        {
                            return "Status : Playing";
                        }
                        else
                        {
                            return "Status : Picking";
                        }
                    }
                    else if (room.Playing)
                    {
                        return "Status : " + room.Status.ToString() + " - Playing";
                    }
                    else
                    {
                        return "Status : " + room.Status.ToString() + (room.Status == RoomStatus.Finished ? "" : " - Picking");
                    }
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        /// <summary>
        /// Pick property
        /// </summary>
        public String Pick
        {
            get
            {
                if(room != null)
                {
                    if (room.Status != RoomStatus.Finished)
                    {
                        return "Next team to pick : " + ((TeamVs)room.Ranking).NextTeam.Name;
                    }
                    else
                    {
                        return "Winner : " + ((((TeamVs)room.Ranking).Red.Points + ((TeamVs)room.Ranking).Red.PointAddition) > (((TeamVs)room.Ranking).Blue.Points + ((TeamVs)room.Ranking).Blue.PointAddition) ? ((TeamVs)room.Ranking).Red.Name : ((TeamVs)room.Ranking).Blue.Name);
                    }
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        public String ButtonName
        {
            get
            {
                if (IsCreated)
                    return "Link";
                else
                    return "Create";
            }
        }
        public String Timer
        {
            get
            {
                if (IsCreated)
                    return duration + "min";
                else
                    return string.Empty;
            }
        }

        public String ScoreLabel
        {
            get
            {
                if (IsCreated)
                    return "Scores :";
                else
                    return string.Empty;
            }
        }

        public String BatchUI
        {
            get
            {
                if (!IsCreated)
                    return "Batch " + batch;
                else
                    return string.Empty;
            }
        }

        public String Batch
        {
            get
            {
                return batch;
            }
            set
            {
                batch = value;
            }
        }

        public bool IsCreated { get; set; }
        #endregion

        #region Public Methods
        /// <summary>
        /// Opening the browser with mp link when double click on room's name
        /// </summary>
        public void OpenMpLink()
        {
            System.Diagnostics.Process.Start("https://osu.ppy.sh/community/matches/" + room.Id);
        }

        /// <summary>
        /// Update the status of the room in the overview UI
        /// </summary>
        public void UpdateStatus()
        {
            NotifyOfPropertyChange(() => Status);

            if (room.Status == RoomStatus.Finished && timer.Enabled)
            {
                StopTimer();
            }
            else if (!timer.Enabled)
            {
                if(room.Status != RoomStatus.NotStarted)
                    StartTimer();
            }
        }

        private async void CreateMatch()
        {
            await Dialog.ShowProgress("Please wait", "Creating the room...");
            OsuIrcBot.GetInstancePrivate().CreateMatch(blueteam, redteam);
        }

        public void SetCreation(long id)
        {
            roomId = id;
            MatchCreated(this, new MatchCreatedArgs(id));
            IsCreated = true;
            OsuIrcBot.GetInstancePrivate().ConfigureMatch(id.ToString());

        }

        public void ActivateButton()
        {
            if(!IsCreated)
            {
                CreateMatch();
            }
            else
            {
                if(room != null)
                {
                    System.Diagnostics.Process.Start("https://osu.ppy.sh/community/matches/" + room.Id);
                }
                else
                {
                    Dialog.ShowDialog("Whoops!", "The room doesn't exist in the view model");
                }
            }
        }

        /// <summary>
        /// Update the overview UI
        /// </summary>
        public void Update()
        {
            NotifyOfPropertyChange(() => ScoreBlue);
            NotifyOfPropertyChange(() => ScoreRed);
            NotifyOfPropertyChange(() => Pick);
            NotifyOfPropertyChange(() => ButtonName);
            NotifyOfPropertyChange(() => Timer);
            NotifyOfPropertyChange(() => ScoreLabel);
            NotifyOfPropertyChange(() => BatchUI);
            UpdateStatus();
        }
        #endregion

        #region Private Methods
        private void timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            duration++;
            NotifyOfPropertyChange(() => Timer);
        }

        /// <summary>
        /// Start the timer
        /// </summary>
        private void StartTimer()
        {
            this.timer.Start();
        }

        /// <summary>
        /// Stop the timer
        /// </summary>
        private void StopTimer()
        {
            this.timer.Stop();
        }
        #endregion
    }
}
