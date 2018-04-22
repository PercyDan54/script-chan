using Caliburn.Micro;
using Osu.Ircbot;
using Osu.Mvvm.Miscellaneous;
using Osu.Scores;
using Osu.Scores.Status;
using Osu.Tournament.Ov.ViewModels;
using Osu.Utils.Info;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace Osu.Mvvm.Ov.ViewModels
{
    /// <summary>
    /// The delegate for the event when a match has been created
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
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

        protected OvViewModel parent;

        /// <summary>
        /// The duration of the game
        /// </summary>
        protected int duration;

        /// <summary>
        /// The blue team name
        /// </summary>
        protected string blueteam;

        /// <summary>
        /// The red team name
        /// </summary>
        protected string redteam;

        /// <summary>
        /// The batch letter selected
        /// </summary>
        protected string batch;

        /// <summary>
        /// The room id
        /// </summary>
        protected long roomId;

        /// <summary>
        /// The match created event
        /// </summary>
        public event MatchCreatedEvent MatchCreated;
        #endregion

        #region Constructor
        /// <summary>
        /// The constructor
        /// </summary>
        /// <param name="room">the room</param>
        public OvRoomViewModel(OvViewModel ovvm, Room room)
        {
            parent = ovvm;
            this.room = room;
            this.room.RankingTypeChanged += OnRankingTypeChanged;
            this.roomId = room.Id;
            IsCreated = true;

            Initialize();
        }

        public OvRoomViewModel(OvViewModel ovvm, string teamblue, string teamred, string b)
        {
            parent = ovvm;
            blueteam = teamblue;
            redteam = teamred;
            batch = b;
            IsCreated = false;

            Initialize();
        }

        /// <summary>
        /// Function called to initialize the overview of the room, starting a timer to know the duration of the game while the tool is open
        /// </summary>
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

        /// <summary>
        /// The RoomId property
        /// </summary>
        public long RoomId
        {
            get
            {
                return roomId;
            }
        }

        /// <summary>
        /// The InBetweenText property if you need to print the room name or the red vs blue name
        /// </summary>
        public String InBetweenText
        {
            get
            {
                return (room == null || room.Ranking.GetType() == typeof(Osu.Scores.TeamVs)) ? " VS " : room.Name;
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
                else if(room.Ranking.GetType() == typeof(TeamVs))
                {
                    return ((TeamVs)room.Ranking).Blue.Name;
                }
                else
                {
                    return string.Empty;
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
                if (room != null && room.Ranking.GetType() == typeof(TeamVs))
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
                {
                    return redteam;

                }
                else if(room.Ranking.GetType() == typeof(TeamVs))
                {
                    return ((TeamVs)room.Ranking).Red.Name;
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        /// <summary>
        /// Red team score property
        /// </summary>
        public String ScoreRed
        {
            get
            {
                if (room != null && room.Ranking.GetType() == typeof(TeamVs))
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
                if(room != null && room.Ranking.GetType() == typeof(TeamVs))
                {
                    if (room.Status != RoomStatus.Finished)
                    {
                        return Tournament.Properties.Resources.OvRoomView_NextTeamPick + " : " + ((TeamVs)room.Ranking).NextTeam.Name;
                    }
                    else
                    {
                        return Tournament.Properties.Resources.OvRoomView_Winner + " : " + ((((TeamVs)room.Ranking).Red.Points + ((TeamVs)room.Ranking).Red.PointAddition) > (((TeamVs)room.Ranking).Blue.Points + ((TeamVs)room.Ranking).Blue.PointAddition) ? ((TeamVs)room.Ranking).Red.Name : ((TeamVs)room.Ranking).Blue.Name);
                    }
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        /// <summary>
        /// The ButtonName property for the content of the button in case the room is created or not
        /// </summary>
        public String ButtonName
        {
            get
            {
                if (IsCreated)
                    return Tournament.Properties.Resources.OvRoomView_Link;
                else
                    return Tournament.Properties.Resources.OvRoomView_Create;
            }
        }

        /// <summary>
        /// The timer property
        /// </summary>
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

        /// <summary>
        /// The scorelabel property
        /// </summary>
        public String ScoreLabel
        {
            get
            {
                if (IsCreated && room.Ranking.GetType() == typeof(TeamVs))
                    return Tournament.Properties.Resources.OvRoomView_Scores + " :";
                else
                    return string.Empty;
            }
        }

        /// <summary>
        /// The batchUI letter property
        /// </summary>
        public String BatchUI
        {
            get
            {
                if (!IsCreated)
                    return Tournament.Properties.Resources.OvRoomView_Batch + " " + batch;
                else
                    return string.Empty;
            }
        }

        /// <summary>
        /// The batch letter property
        /// </summary>
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

        /// <summary>
        /// The IsCreated property
        /// </summary>
        protected bool IsCreated { get; set; }

        /// <summary>
        /// The IsMatchRunning property
        /// </summary>
        public bool IsMatchRunning { get { return !IsCreated; } }
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

        /// <summary>
        /// Function called to create the room on osu! if you click the button to create the room
        /// </summary>
        private async void CreateMatch()
        {
            if(string.IsNullOrEmpty(InfosHelper.TourneyInfos.Acronym))
            {
                Dialog.ShowDialog(Tournament.Properties.Resources.Error_Title, Tournament.Properties.Resources.Error_MissingAcronym);
            }
            else
            {
                await Dialog.ShowProgress(Tournament.Properties.Resources.Wait_Title, Tournament.Properties.Resources.Wait_CreatingRoom);
                OsuIrcBot.GetInstancePrivate().CreateMatch(blueteam, redteam);
            }
        }

        /// <summary>
        /// Function called when the room id has been grabbed to join the room on irc and configure the osu! room
        /// </summary>
        /// <param name="id">the room id</param>
        public void SetCreation(long id)
        {
            roomId = id;
            MatchCreated(this, new MatchCreatedArgs(id));
            IsCreated = true;
            OsuIrcBot.GetInstancePrivate().ConfigureMatch(id.ToString());

        }

        /// <summary>
        /// The button to create or show the mp link
        /// </summary>
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
                    Dialog.ShowDialog(Tournament.Properties.Resources.Error_Title, Tournament.Properties.Resources.Error_RoomDoesNotExist);
                }
            }
        }

        /// <summary>
        /// Function called to delete the overview if the match has not been created yet
        /// </summary>
        public void DeleteOverview()
        {
            parent.RemoveOverview(this);
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

        /// <summary>
        /// Update the overview but for ranking informations
        /// </summary>
        public void UpdateRanking()
        {
            NotifyOfPropertyChange(() => TeamBlue);
            NotifyOfPropertyChange(() => TeamRed);
            NotifyOfPropertyChange(() => InBetweenText);
            NotifyOfPropertyChange(() => ScoreBlue);
            NotifyOfPropertyChange(() => ScoreRed);
            NotifyOfPropertyChange(() => Pick);
            NotifyOfPropertyChange(() => ScoreLabel);
        }

        /// <summary>
        /// Function called when an overview created with the tool has now the room in game created
        /// </summary>
        /// <param name="r"></param>
        public void NotifyRoomCreated(Room r)
        {
            // Removing variables used for overview before room creation
            redteam = null;
            blueteam = null;

            this.Room = r;
            this.Room.RankingTypeChanged += OnRankingTypeChanged;
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

        /// <summary>
        /// Event when the ranking type changed in the tool
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnRankingTypeChanged(object sender, EventArgs e)
        {
            UpdateRanking();
        }
        #endregion
    }
}
