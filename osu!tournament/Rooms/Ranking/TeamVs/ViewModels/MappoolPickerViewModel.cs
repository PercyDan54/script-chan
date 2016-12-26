using Caliburn.Micro;
using Osu.Scores;
using osu_discord;
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
            string message = RefereeMatchHelper.GetInstance(r.Id).GenerateBanRecapMessage();
            DiscordBot.GetInstance().SendMessage(message);
        }

        public void SendPickRecap()
        {
            string message = RefereeMatchHelper.GetInstance(r.Id).GeneratePickRecapMessage();
            DiscordBot.GetInstance().SendMessage(message);
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
