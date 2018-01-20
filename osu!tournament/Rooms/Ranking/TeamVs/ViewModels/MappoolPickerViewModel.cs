using Caliburn.Micro;
using Osu.Mvvm.Miscellaneous;
using Osu.Scores;
using osu_discord;
using osu_utils.DiscordModels;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Osu.Mvvm.Rooms.Ranking.TeamVs.ViewModels
{
    public class MappoolPickerViewModel : Screen
    {
        #region Attributes
        private IObservableCollection<BeatmapPickerViewModel> beatmaps;

        private Room r;

        private string searchBoxValue;
        #endregion

        #region Constructor
        public MappoolPickerViewModel(Room room)
        {
            searchBoxValue = "";
            r = room;
            beatmaps = new BindableCollection<BeatmapPickerViewModel>();
            if (room.Mappool != null)
            {
                beatmaps.Clear();
                foreach (KeyValuePair<long, Beatmap> kvp in room.Mappool?.Pool)
                {
                    beatmaps.Add(new BeatmapPickerViewModel(room, kvp.Value));
                }
            }
        }
        #endregion

        #region Properties
        public IObservableCollection<BeatmapPickerViewModel> Beatmaps
        {
            get
            {
                if (searchBoxValue == "")
                    return beatmaps;

                return new BindableCollection<BeatmapPickerViewModel>(beatmaps.Where(x => x.Beatmap.OsuBeatmap.Title.ToUpper().Contains(searchBoxValue.ToUpper()) || x.Beatmap.OsuBeatmap.Artist.ToUpper().Contains(searchBoxValue.ToUpper())));
            }
        }

        public string SearchBox
        {
            get
            {
                return searchBoxValue;
            }
            set
            {
                if (value != searchBoxValue)
                {
                    searchBoxValue = value;
                    NotifyOfPropertyChange("SearchBox");
                    NotifyOfPropertyChange("Beatmaps");
                }
            }
        }

        public void SendBanRecap()
        {
            Embed message = RefereeMatchHelper.GetInstance(r.Id).GenerateBanRecapMessage();
            if(message != null)
            {
                string name;
                if (r.Ranking.GetType() == typeof(Osu.Scores.TeamVs))
                {
                    name = string.Format("{0} VS {1}", ((Osu.Scores.TeamVs)r.Ranking).Red.Name, ((Osu.Scores.TeamVs)r.Ranking).Blue.Name);
                }
                else
                {
                    name = r.Name;
                }
                DiscordHelper.SendRecap(message, DiscordChannelEnum.Default, name, r.Id.ToString());
            }
            else
            {
                Dialog.ShowDialog("Whoops!", "You need to ban the same number of maps for each team to display it!");
            }
        }

        public void SendPickRecap()
        {
            Embed message = RefereeMatchHelper.GetInstance(r.Id).GeneratePickRecapMessage();
            if (message != null)
            {
                string name;
                if (r.Ranking.GetType() == typeof(Osu.Scores.TeamVs))
                {
                    name = string.Format("{0} VS {1}", ((Osu.Scores.TeamVs)r.Ranking).Red.Name, ((Osu.Scores.TeamVs)r.Ranking).Blue.Name);
                }
                else
                {
                    name = r.Name;
                }
                DiscordHelper.SendRecap(message, DiscordChannelEnum.Default, name, r.Id.ToString());
            }
            //DiscordBot.GetInstance().SendMessage(message);
        }
        #endregion

        #region Public Methods
        public void Update()
        {
            if (r.Mappool != null)
            {
                beatmaps.Clear();
                Task<Room> re = Room.Get(r.Id);
                re.Wait();
                foreach (KeyValuePair<long, Beatmap> kvp in re.Result.Mappool?.Pool)
                {
                    beatmaps.Add(new BeatmapPickerViewModel(r, kvp.Value));
                }
            }
        }
        #endregion
    }
}
