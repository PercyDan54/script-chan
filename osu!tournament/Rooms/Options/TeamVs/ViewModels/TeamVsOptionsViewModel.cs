using Caliburn.Micro;
using Osu.Mvvm.Miscellaneous;
using Osu.Scores;
using Osu.Utils;
using Osu.Utils.Bans;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Osu.Mvvm.Rooms.Options.TeamVs.ViewModels
{
    public class TeamVsOptionsViewModel : Screen, IOptionsViewModel
    {
        #region Attributes
        /// <summary>
        /// The room
        /// </summary>
        protected Room room;

        /// <summary>
        /// The ranking
        /// </summary>
        protected Osu.Scores.TeamVs ranking;
        #endregion

        #region Constructor
        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="room">the room</param>
        public TeamVsOptionsViewModel(Room room)
        {
            this.room = room;
            ranking = (Osu.Scores.TeamVs)room.Ranking;

            Update();
        }
        #endregion

        #region Properties
        /// <summary>
        /// First team red property
        /// </summary>
        public bool FirstTeamIsRed
        {
            get
            {
                return ranking.First == Api.OsuTeam.Red;
            }
            set
            {
                ranking.First = value ? Api.OsuTeam.Red : Api.OsuTeam.Blue;
                RefereeMatchHelper.GetInstance(room.Id).UpdateTeamBanOrder(room, ranking.First);
                NotifyOfPropertyChange(() => FirstTeamIsRed);
            }
        }

        /// <summary>
        /// Enable commands property
        /// </summary>
        public bool EnableCommands
        {
            get
            {
                return room.Commands;
            }
            set
            {
                if (value != room.Commands)
                {
                    room.Commands = value;
                    NotifyOfPropertyChange(() => EnableCommands);
                }
            }
        }

        public bool EnableStreamMode
        {
            get
            {
                return room.IsStreamed;
            }
            set
            {
                if(value)
                {
                    SetStreamMode();
                }
                else
                {
                    room.IsStreamed = value;
                    NotifyOfPropertyChange(() => EnableStreamMode);
                }
            }
        }

        /// <summary>
        /// The list of allowed BO number property
        /// </summary>
        public List<string> Modes
        {
            get
            {
                return new List<string> { "3", "5", "7", "9", "11", "13" };
            }
        }

        /// <summary>
        /// The selected BO property
        /// </summary>
        public string SelectedMode
        {
            get
            {
                return room.Mode;
            }

            set
            {
                if (room.Mode != value)
                {
                    room.Mode = value;
                    NotifyOfPropertyChange(() => SelectedMode);
                }

            }
        }

        public bool IsObsPathValid
        {
            get
            {
                return ObsBanHelper.IsValid;
            }
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Updates the view model
        /// </summary>
        public void Update()
        {

        }

        public async void SetStreamMode()
        {
            if(ObsBanHelper.IsValid)
            {
                var confirm = await Dialog.ShowConfirmation("OBS Mode", "Are you sure to set this match as streamed? It will set OBS bans to this match and remove the one currently set.");
                if (confirm)
                {
                    var roomToUnstream = Room.Rooms.FirstOrDefault(x => x.Value.IsStreamed).Value;
                    if (roomToUnstream != null)
                    {
                        roomToUnstream.IsStreamed = false;
                    }
                    Room.Rooms.First(x => x.Value.Id == room.Id).Value.IsStreamed = true;
                    room.IsStreamed = true;
                    ObsBanHelper.SetInstance(((Scores.TeamVs)room.Ranking).Blue.Name, ((Scores.TeamVs)room.Ranking).Red.Name);
                    ObsBanHelper.GetInstance().SetTeamsOnObs();
                    NotifyOfPropertyChange(() => EnableStreamMode);
                }
            }
            else
            {
                NotifyOfPropertyChange(() => IsObsPathValid);
            }
            
        }
        #endregion
    }
}
