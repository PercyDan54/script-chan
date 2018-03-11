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
        protected IObservableCollection<SelectableObject<string>> items;
        protected TeamOv selectedRedTeam;
        protected TeamOv selectedBlueTeam;
        protected char batchLetter;

        public event MatchCreatedEvent MatchCreated;
        #endregion

        #region Constructor
        public OvViewModel()
        {
            // Event to create an overview when we create a room with mp make on irc
            OsuIrcBot.GetInstancePrivate().RoomCreatedCatched += CreateMatchNext;

            rooms = new BindableCollection<OvRoomViewModel>();
            items = new BindableCollection<SelectableObject<string>>();

            batchLetter = 'A';

            if (InfosHelper.TourneyInfos.Matches != null)
            {
                foreach (Game g in InfosHelper.TourneyInfos.Matches)
                {
                    if (items.Count(x => x.ObjectData == g.Batch) == 0)
                    {
                        var so = new SelectableObject<string>(g.Batch, false);
                        so.BatchSelected += OnItemChange;
                        items.Add(so);
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
                if(items.Count(x => x.IsSelected == true) == 0)
                {
                    return rooms;
                }
                else
                {
                    var vrooms = items.ToList().FindAll(y => y.IsSelected == true);
                    return new BindableCollection<OvRoomViewModel>(rooms.Where(item => vrooms.Exists(y => y.ObjectData == item.Batch)).OrderBy(x => x.Batch));
                }
            }
        }

        public IObservableCollection<SelectableObject<string>> Items
        {
            get
            {
                return items;
            }
        }

        public IEnumerable<TeamOv> RedTeam
        {
            get
            {
                return TeamManager.Teams.OrderBy(x => x.Name);
            }
        }

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

        public IEnumerable<TeamOv> BlueTeam
        {
            get
            {
                return TeamManager.Teams.OrderBy(x => x.Name);
            }
        }

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
                    Dialog.ShowDialog("Whoops!", "Please, enter a letter!");
                }
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
        public bool CanCreateMatch
        {
            get { return selectedBlueTeam != null && selectedRedTeam != null; }
        }

        public void CreateOverview()
        {
            if(selectedRedTeam.Name == selectedBlueTeam.Name)
            {
                Dialog.ShowDialog("Whoops!", "You can't use the same team twice!");
            }
            else
            {
                if (InfosHelper.TourneyInfos.Matches == null)
                    InfosHelper.TourneyInfos.Matches = new List<Game>();

                InfosHelper.TourneyInfos.Matches.Add(new Game() { TeamRedName = selectedRedTeam.Name, TeamBlueName = selectedBlueTeam.Name, Batch = batchLetter.ToString() });
                addOverview(selectedBlueTeam.Name, selectedRedTeam.Name, batchLetter.ToString());
            }

        }

        public OvRoomViewModel addOverview(Room room)
        {
            OvRoomViewModel ovvm = new OvRoomViewModel(this, room);
            ovvm.MatchCreated += OnMatchCreated;
            rooms.Add(ovvm);
            NotifyOfPropertyChange(() => ViewRooms);

            return ovvm;
        }

        public OvRoomViewModel addOverview(string blueteam, string redteam, string batch)
        {
            OvRoomViewModel ovvm = new OvRoomViewModel(this, blueteam, redteam, batch);
            ovvm.MatchCreated += OnMatchCreated;
            rooms.Add(ovvm);
            NotifyOfPropertyChange(() => ViewRooms);

            return ovvm;
        }

        public void UpdateBatch(string blueteam, string redteam, string batch)
        {
            var ov = rooms.FirstOrDefault(x => x.TeamBlue == blueteam && x.TeamRed == redteam);
            if(ov != null)
            {
                ov.Batch = batch;
            }
        }

        private void OnMatchCreated(object sender, MatchCreatedArgs e)
        {
            MatchCreated(this, e);
        }

        public void removeOverview(Room room)
        {
            OvRoomViewModel ov = findOvRoom(room);
            if(ov != null)
            {
                rooms.Remove(ov);
                NotifyOfPropertyChange(() => ViewRooms);
            }      
        }

        public void RemoveOverview(OvRoomViewModel ov)
        {
            InfosHelper.TourneyInfos.Matches.RemoveAll(x => x.TeamBlueName == ov.TeamBlue && x.TeamRedName == ov.TeamRed && x.Batch == ov.Batch);
            rooms.Remove(ov);
            NotifyOfPropertyChange(() => ViewRooms);
        }

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

        public void UpdateStatus(Room room)
        {
            OvRoomViewModel ov = findOvRoom(room);
            if (ov != null)
            {
                ov.UpdateStatus();
            }
        }

        public void Update(Room room)
        {
            OvRoomViewModel ov = findOvRoom(room);
            if (ov != null)
            {
                ov.Update();
            }
        }

        public void UpdateTeams()
        {
            NotifyOfPropertyChange(() => BlueTeam);
            NotifyOfPropertyChange(() => RedTeam);
        }

        public OvRoomViewModel getOverview(Room room)
        {
            foreach(OvRoomViewModel orvm in rooms)
            {
                if(room.Id == orvm.RoomId)
                {
                    orvm.Room = room;
                    orvm.Update();
                    return orvm;
                }
            }
            return null;
        }
        #endregion

        #region private methods
        private void CreateMatchNext(object sender, MatchCatchedArgs e)
        {
            if (e.Id != null)
            {
                long id;
                if (long.TryParse(e.Id, out id))
                {
                    rooms.FirstOrDefault(x => x.TeamBlue == e.BlueTeam && x.TeamRed == e.RedTeam).SetCreation(id);
                    System.Windows.Application.Current.Dispatcher.Invoke(new System.Action(async () => { await Dialog.HideProgress(); Dialog.ShowDialog("OK!", "Match has been created!"); }));
                    DiscordHelper.SendNewMatch(id.ToString(), e.RedTeam, e.BlueTeam);
                    //osu_discord.DiscordBot.GetInstance().SendMessage("Match created : https://osu.ppy.sh/community/matches/" + id + " . If you want to join, click irc://cho.ppy.sh:6667/mp_" + id);
                    //}
                    //else
                    //{
                    //Dialog.ShowDialog("Whoops!", "The match hasn't been created on script chan!");
                    //}
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

        private void OnItemChange(object sender, EventArgs e)
        {
            NotifyOfPropertyChange(() => ViewRooms);
        }
            #endregion
        }
}
