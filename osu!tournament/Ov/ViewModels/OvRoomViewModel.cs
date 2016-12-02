using Caliburn.Micro;
using Osu.Scores;
using Osu.Scores.Status;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace Osu.Mvvm.Ov.ViewModels
{
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
        #endregion

        #region Constructor
        /// <summary>
        /// The constructor
        /// </summary>
        /// <param name="room">the room</param>
        public OvRoomViewModel(Room room)
        {
            this.room = room;
            this.timer = new Timer(60000);

            this.timer.Elapsed += new ElapsedEventHandler(timer_Elapsed);

            this.duration = 0;

            if (room.Status != RoomStatus.NotStarted && room.Status != RoomStatus.Finished)
            {
                StartTimer();
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
        }

        /// <summary>
        /// Room name property
        /// </summary>
        public String RoomName
        {
            get
            {
                return room.Name;
            }
        }

        /// <summary>
        /// Blue team name property
        /// </summary>
        public String TeamBlue
        {
            get
            {
                return ((TeamVs)room.Ranking).Blue.Name;
            }
        }

        /// <summary>
        /// Blue team score property
        /// </summary>
        public String ScoreBlue
        {
            get
            {
                return "" + ( ((TeamVs)room.Ranking).Blue.Points + ((TeamVs)room.Ranking).Blue.PointAddition );
            }
        }

        /// <summary>
        /// Red team name property
        /// </summary>
        public String TeamRed
        {
            get
            {
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
                return "" + ( ((TeamVs)room.Ranking).Red.Points + ((TeamVs)room.Ranking).Red.PointAddition );
            }
        }

        /// <summary>
        /// Room's status property
        /// </summary>
        public String Status
        {
            get
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
        }

        /// <summary>
        /// Pick property
        /// </summary>
        public String Pick
        {
            get
            {
                if (room.Status != RoomStatus.Finished)
                {
                    return "Next team to pick : " + ((TeamVs)room.Ranking).NextTeam.Name;
                }
                else
                {
                    return "Winner : " + ( ( ((TeamVs)room.Ranking).Red.Points + ((TeamVs)room.Ranking).Red.PointAddition ) > ( ((TeamVs)room.Ranking).Blue.Points + ((TeamVs)room.Ranking).Blue.PointAddition ) ? ((TeamVs)room.Ranking).Red.Name : ((TeamVs)room.Ranking).Blue.Name);
                }
            }
        }

        public String Timer
        {
            get
            {
                return duration + "min";
            }
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Opening the browser with mp link when double click on room's name
        /// </summary>
        public void OpenMpLink()
        {
            System.Diagnostics.Process.Start("https://osu.ppy.sh/mp/" + room.Id);
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
                StartTimer();
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
