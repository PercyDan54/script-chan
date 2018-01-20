using Caliburn.Micro;
using Osu.Ircbot;
using Osu.Mvvm.Miscellaneous;
using Osu.Scores;
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

        public event MatchCreatedEvent MatchCreated;
        #endregion

        #region Constructor
        public OvViewModel()
        {
            rooms = new BindableCollection<OvRoomViewModel>();
            items = new BindableCollection<SelectableObject<string>>();

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
                OsuIrcBot.GetInstancePrivate().RoomCreatedCatched += CreateMatchNext;
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

        #endregion

        #region Public Methods
        public OvRoomViewModel addOverview(Room room)
        {
            OvRoomViewModel ovvm = new OvRoomViewModel(room);
            ovvm.MatchCreated += OnMatchCreated;
            rooms.Add(ovvm);
            NotifyOfPropertyChange(() => ViewRooms);

            return ovvm;
        }

        public OvRoomViewModel addOverview(string blueteam, string redteam, string batch)
        {
            OvRoomViewModel ovvm = new OvRoomViewModel(blueteam, redteam, batch);
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
