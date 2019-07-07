using Caliburn.Micro;
using Osu.Api;
using Osu.Ircbot;
using Osu.Mvvm.Miscellaneous;
using Osu.Scores;
using Osu.Utils.TeamsOv;
using Osu.Tournament.Ov.ViewModels;
using Osu.Utils;
using Osu.Utils.Info;
using osu_discord;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Osu.Mvvm.Ov.ViewModels
{
    public class OvViewModel : Screen
    {
        #region Attributes
        /// <summary>
        /// The list of OverviewRoom view models
        /// </summary>
        protected IObservableCollection<OvRoomViewModel> rooms;

        /// <summary>
        /// The list of batch letter used
        /// </summary>
        protected IObservableCollection<string> items;

        /// <summary>
        /// The selected batch letter in the combobox
        /// </summary>
        protected string selecteditem;

        /// <summary>
        /// The selected red team to create an overview
        /// </summary>
        protected TeamOv selectedRedTeam;

        /// <summary>
        /// The selected blue team to create an overview
        /// </summary>
        protected TeamOv selectedBlueTeam;

        /// <summary>
        /// The batch letter selected to create an overview
        /// </summary>
        protected char batchLetter;

        public event MatchCreatedEvent MatchCreated;
        #endregion

        #region Constructor
        public OvViewModel()
        {
            // Event to create an overview when we create a room with mp make on irc
            OsuIrcBot.GetInstancePrivate().RoomCreatedCatched += CreateMatchNext;

            rooms = new BindableCollection<OvRoomViewModel>();
            items = new BindableCollection<string>();

            batchLetter = 'A';

            // If we already have matches stored in the cache, we add letters to the combobox to filter batches
            if (InfosHelper.TourneyInfos.Matches != null)
            {
                foreach (Game g in InfosHelper.TourneyInfos.Matches)
                {
                    if (items.Count(x => x == g.Batch) == 0)
                    {
                        items.Add(g.Batch);
                    }
                }
            }
        }
        #endregion

        #region Properties
        /// <summary>
        /// Overview Property
        /// </summary>
        public IObservableCollection<OvRoomViewModel> ViewRooms
        {
            get
            {
                if(SelectedItem == null)
                {
                    return rooms;
                }
                else
                {
                    return new BindableCollection<OvRoomViewModel>(rooms.Where(item => item.Batch == SelectedItem).OrderBy(x => x.Batch));
                }
            }
        }

        /// <summary>
        /// The Items property (combobox of existing batch letters)
        /// </summary>
        public IObservableCollection<string> Items
        {
            get
            {
                return new BindableCollection<string>(items.OrderBy(q => q).ToList());
            }
        }

        /// <summary>
        /// The SelectedItem property (selected batch letter to filter on overviews)
        /// </summary>
        public string SelectedItem
        {
            get
            {
                return selecteditem;
            }
            set
            {
                if(value != selecteditem)
                {
                    selecteditem = value;
                    NotifyOfPropertyChange(() => ViewRooms);
                }
            }
        }

        /// <summary>
        /// The RedTeam property
        /// </summary>
        public IEnumerable<TeamOv> RedTeam
        {
            get
            {
                return TeamManager.Teams.OrderBy(x => x.Name);
            }
        }

        /// <summary>
        /// The SelectedRedTeam property
        /// </summary>
        public TeamOv SelectedRedTeam
        {
            get
            {
                return selectedRedTeam;
            }
            set
            {
                if(selectedRedTeam != value)
                {
                    selectedRedTeam = value;
                    NotifyOfPropertyChange(() => CanCreateMatch);
                }
            }
        }

        /// <summary>
        /// The BlueTeam property
        /// </summary>
        public IEnumerable<TeamOv> BlueTeam
        {
            get
            {
                return TeamManager.Teams.OrderBy(x => x.Name);
            }
        }

        /// <summary>
        /// The SelectedBlueTeam property
        /// </summary>
        public TeamOv SelectedBlueTeam
        {
            get
            {
                return selectedBlueTeam;
            }
            set
            {
                if (selectedBlueTeam != value)
                {
                    selectedBlueTeam = value;
                    NotifyOfPropertyChange(() => CanCreateMatch);
                }
            }
        }

        /// <summary>
        /// The BatchLetter property
        /// </summary>
        public char BatchLetter
        {
            get
            {
                return batchLetter;
            }
            set
            {
                if(batchLetter != value && char.IsLetter(value))
                {
                    batchLetter = value;
                    NotifyOfPropertyChange(() => BatchLetter);
                }
                else
                {
                    Dialog.ShowDialog(Utils.Properties.Resources.Error_Title, Utils.Properties.Resources.Error_ValueNotLetter);
                }
            }
        }

        public string WelcomeMessage
        {
            get => InfosHelper.TourneyInfos.WelcomeMessage;
            set
            {
                InfosHelper.TourneyInfos.WelcomeMessage = value;
                NotifyOfPropertyChange(() => WelcomeMessage);
            }
        }

        #region Configuration
        public string Acronym { get { return InfosHelper.TourneyInfos.Acronym; } set { InfosHelper.TourneyInfos.Acronym = value; } }
        public string AdminList { get { return InfosHelper.UserDataInfos.Admins; } set { InfosHelper.UserDataInfos.Admins = value; } }
        public string DefaultId { get { return InfosHelper.TourneyInfos.DefaultMapId; } set { InfosHelper.TourneyInfos.DefaultMapId = value; } }
        public List<int> PlayersPerTeam { get { return new List<int>() { 1, 2, 3, 4, 5, 6, 7, 8 }; } }
        public int SelectedPlayersPerTeam { get { return InfosHelper.TourneyInfos.PlayersPerTeam; } set { InfosHelper.TourneyInfos.PlayersPerTeam = value; } }
        public List<OsuTeamType> TeamMode { get { return new List<OsuTeamType>() { OsuTeamType.HeadToHead, OsuTeamType.TeamVs }; } }
        public OsuTeamType SelectedTeamMode { get { return (OsuTeamType)Enum.Parse(typeof(OsuTeamType), InfosHelper.TourneyInfos.TeamMode, true); } set { InfosHelper.TourneyInfos.TeamMode = value.ToString(); } }
        public List<OsuScoringType> ScoreMode { get { return new List<OsuScoringType>() { OsuScoringType.Score, OsuScoringType.ScoreV2 }; } }
        public OsuScoringType SelectedScoreMode { get { return (OsuScoringType)Enum.Parse(typeof(OsuScoringType), InfosHelper.TourneyInfos.ScoreMode, true); } set { InfosHelper.TourneyInfos.ScoreMode = value.ToString(); } }
        public List<string> Size { get { return new List<string>(){"1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12", "13", "14", "15", "16"}; } }
        public string SelectedSize { get { return InfosHelper.TourneyInfos.RoomSize; } set { InfosHelper.TourneyInfos.RoomSize = value; } }
        public List<OsuMode> GameMode { get { return new List<OsuMode>() { OsuMode.Standard, OsuMode.Taiko, OsuMode.CTB, OsuMode.Mania }; } }
        public OsuMode SelectedGameMode { get { return (OsuMode)Enum.Parse(typeof(OsuMode),InfosHelper.TourneyInfos.ModeType, true); } set { InfosHelper.TourneyInfos.ModeType = value.ToString(); } }
        #endregion
        #endregion

        #region Public Methods
        /// <summary>
        /// Property to activate or not the button to create the overview
        /// </summary>
        public bool CanCreateMatch
        {
            get { return selectedBlueTeam != null && selectedRedTeam != null; }
        }

        /// <summary>
        /// Function called to create the overview
        /// </summary>
        public void CreateOverview()
        {
            if(selectedRedTeam.Name == selectedBlueTeam.Name)
            {
                Dialog.ShowDialog(Utils.Properties.Resources.Error_Title, Utils.Properties.Resources.Error_SameTeam);
            }
            else
            {
                if (InfosHelper.TourneyInfos.Matches == null)
                    InfosHelper.TourneyInfos.Matches = new List<Game>();

                InfosHelper.TourneyInfos.Matches.Add(new Game() { TeamRedName = selectedRedTeam.Name, TeamBlueName = selectedBlueTeam.Name, Batch = batchLetter.ToString() });
                addOverview(selectedBlueTeam.Name, selectedRedTeam.Name, batchLetter.ToString());
            }

        }

        /// <summary>
        /// Function called to add an overview with an existing room
        /// </summary>
        /// <param name="room">the room</param>
        /// <returns></returns>
        public OvRoomViewModel addOverview(Room room)
        {
            OvRoomViewModel ovvm = new OvRoomViewModel(this, room);
            ovvm.MatchCreated += OnMatchCreated;
            rooms.Add(ovvm);
            NotifyOfPropertyChange(() => ViewRooms);

            return ovvm;
        }

        /// <summary>
        /// Function called to add an overview from the cache not created on osu!
        /// </summary>
        /// <param name="blueteam">the blue team name</param>
        /// <param name="redteam">the red team name</param>
        /// <param name="batch">the batch letter</param>
        /// <returns></returns>
        public OvRoomViewModel addOverview(string blueteam, string redteam, string batch)
        {
            if (!items.ToList().Exists(x => x == batch))
                items.Add(batch);

            OvRoomViewModel ovvm = new OvRoomViewModel(this, blueteam, redteam, batch);
            ovvm.MatchCreated += OnMatchCreated;
            rooms.Add(ovvm);
            NotifyOfPropertyChange(() => ViewRooms);
            NotifyOfPropertyChange(() => Items);

            return ovvm;
        }

        /// <summary>
        /// Function which update the batch of the two teams overview
        /// </summary>
        /// <param name="blueteam">the blue team name</param>
        /// <param name="redteam">the red team name</param>
        /// <param name="batch">the new batch letter</param>
        public void UpdateBatch(string blueteam, string redteam, string batch)
        {
            var ov = rooms.FirstOrDefault(x => x.TeamBlue == blueteam && x.TeamRed == redteam);
            if(ov != null)
            {
                ov.Batch = batch;
            }
        }

        /// <summary>
        /// Catch event if match has been created
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnMatchCreated(object sender, MatchCreatedArgs e)
        {
            MatchCreated(this, e);
        }

        /// <summary>
        /// Function called if we want to remove the overview of a room
        /// </summary>
        /// <param name="room">the room</param>
        public void removeOverview(Room room)
        {
            OvRoomViewModel ov = findOvRoom(room);
            if(ov != null)
            {
                rooms.Remove(ov);
                NotifyOfPropertyChange(() => ViewRooms);
            }      
        }

        /// <summary>
        /// Remove the selected overview
        /// </summary>
        /// <param name="ov">the overview</param>
        public void RemoveOverview(OvRoomViewModel ov)
        {
            InfosHelper.TourneyInfos.Matches.RemoveAll(x => x.TeamBlueName == ov.TeamBlue && x.TeamRedName == ov.TeamRed && x.Batch == ov.Batch);
            rooms.Remove(ov);
            NotifyOfPropertyChange(() => ViewRooms);
        }

        /// <summary>
        /// Find the overview of the selected room
        /// </summary>
        /// <param name="room">the room</param>
        /// <returns>the overview of the room</returns>
        public OvRoomViewModel findOvRoom(Room room)
        {
            foreach (OvRoomViewModel r in rooms)
            {
                if (r.Room != null && room.Id == r.Room.Id)
                {
                    return r;
                }
            }
            return null;
        }

        /// <summary>
        /// Function called to update overview status of the room
        /// </summary>
        /// <param name="room">the room</param>
        public void UpdateStatus(Room room)
        {
            OvRoomViewModel ov = findOvRoom(room);
            if (ov != null)
            {
                ov.UpdateStatus();
            }
        }

        /// <summary>
        /// Function called to update the overview of the room
        /// </summary>
        /// <param name="room">the room</param>
        public void Update(Room room)
        {
            OvRoomViewModel ov = findOvRoom(room);
            if (ov != null)
            {
                ov.Update();
            }
        }

        /// <summary>
        /// Send notification to the view teams has been updated
        /// </summary>
        public void UpdateTeams()
        {
            NotifyOfPropertyChange(() => BlueTeam);
            NotifyOfPropertyChange(() => RedTeam);
        }

        /// <summary>
        /// Get the overview of the room
        /// </summary>
        /// <param name="room">the room</param>
        /// <returns></returns>
        public OvRoomViewModel getOverview(Room room)
        {
            foreach(OvRoomViewModel orvm in rooms)
            {
                if(room.Id == orvm.RoomId)
                {
                    if (orvm.Room == null)
                        orvm.NotifyRoomCreated(room);
                    else
                        orvm.Room = room;

                    orvm.Update();
                    return orvm;
                }
            }
            return null;
        }
        #endregion

        #region private methods
        /// <summary>
        /// Function called after catching the bancho message containing the id of the room, creating the room in the tool
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CreateMatchNext(object sender, MatchCatchedArgs e)
        {
            if (e.Id != null)
            {
                long id;
                if (long.TryParse(e.Id, out id))
                {
                    var room = rooms.FirstOrDefault(x => x.TeamBlue == e.BlueTeam && x.TeamRed == e.RedTeam);
                    if (room != null)
                    {
                        room.SetCreation(id);
                        System.Windows.Application.Current.Dispatcher.Invoke(new System.Action(async () => { await Dialog.HideProgress(); Dialog.ShowDialog("OK!", "Match has been created!"); }));
                        DiscordHelper.SendNewMatch(id.ToString(), e.RedTeam, e.BlueTeam);
                    }
                }
                else
                {
                    System.Windows.Application.Current.Dispatcher.Invoke(new System.Action(async () => { await Dialog.HideProgress(); Dialog.ShowDialog("Whoops!", "There is an error with the room id!"); }));
                }
            }
            else
            {
                System.Windows.Application.Current.Dispatcher.Invoke(new System.Action(async () => { await Dialog.HideProgress(); Dialog.ShowDialog("Whoops!", "The mp room can't be created!"); }));
            }
        }
        #endregion
    }
}
