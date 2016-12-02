using Caliburn.Micro;
using Osu.Scores;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Osu.Mvvm.Ov.ViewModels
{
    public class OvViewModel : Screen
    {
        #region Attributes
        /// <summary>
        /// The list of OverviewRoom view models
        /// </summary>
        protected IObservableCollection<OvRoomViewModel> rooms;
        #endregion

        #region Constructor
        public OvViewModel()
        {
            rooms = new BindableCollection<OvRoomViewModel>();
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
                return rooms;
            }
        }
        #endregion

        #region Public Methods
        public OvRoomViewModel addOverview(Room room)
        {
            OvRoomViewModel ovvm = new OvRoomViewModel(room);
            rooms.Add(ovvm);
            NotifyOfPropertyChange(() => ViewRooms);

            return ovvm;
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
                if (room.Id == r.Room.Id)
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
        #endregion
    }
}
