using Caliburn.Micro;
using Osu.Mvvm.Miscellaneous;
using Osu.Scores;
using osu_discord;
using osu_utils.DiscordModels;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media.TextFormatting;

namespace Osu.Mvvm.Rooms.Ranking.TeamVs.ViewModels
{
    public class MappoolPickerViewModel : Screen
    {
        #region Attributes
        /// <summary>
        /// The list of beatmap picker view models
        /// </summary>
        private IObservableCollection<BeatmapPickerViewModel> beatmaps;

        /// <summary>
        /// The room for this mappool picker
        /// </summary>
        private Room r;

        /// <summary>
        /// The textbox content to search a beatmap
        /// </summary>
        private string searchBoxValue;
        #endregion

        #region Constructor
        public MappoolPickerViewModel(Room room)
        {
            searchBoxValue = "";
            r = room;

            if(beatmaps != null)
                beatmaps.Clear();
            else
                beatmaps = new BindableCollection<BeatmapPickerViewModel>();

            if (room.Mappool != null)
            {
                foreach (KeyValuePair<long, Beatmap> kvp in room.Mappool?.Pool)
                {
                    beatmaps.Add(new BeatmapPickerViewModel(room, kvp.Value));
                }
            }
        }
        #endregion

        #region Properties
        /// <summary>
        /// The Beatmaps property
        /// </summary>
        public IObservableCollection<BeatmapPickerViewModel> Beatmaps
        {
            get
            {
                if (searchBoxValue == "")
                    return new BindableCollection<BeatmapPickerViewModel>(beatmaps.OrderBy(x => x.Beatmap));

                return new BindableCollection<BeatmapPickerViewModel>(beatmaps.Where(x => x.Beatmap.OsuBeatmap.Title.ToUpper().Contains(searchBoxValue.ToUpper()) || x.Beatmap.OsuBeatmap.Artist.ToUpper().Contains(searchBoxValue.ToUpper())).OrderBy(entry => entry.Beatmap));
            }
        }

        /// <summary>
        /// The Searchbox property
        /// </summary>
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

        /// <summary>
        /// Check if webhooks are correctly working or not to send recaps
        /// </summary>
        public bool IsDiscordEnabled => DiscordHelper.IsEnabled();

        /// <summary>
        /// Function called to send ban recap on discord through webhooks
        /// </summary>
        public void SendBanRecap()
        {
            // Generate the embed for the ban recap
            Embed message = RefereeMatchHelper.GetInstance(r.Id).GenerateBanRecapMessage(r.Ranking.GetType());
            if(message != null)
            {
                // Depending of the ranking type, we change the name of the payload
                string name;
                if (r.Ranking.GetType() == typeof(Osu.Scores.TeamVs))
                {
                    name = string.Format("{0} VS {1}", ((Osu.Scores.TeamVs)r.Ranking).Red.Name, ((Osu.Scores.TeamVs)r.Ranking).Blue.Name);
                }
                else
                {
                    name = r.Name;
                }
                DiscordHelper.SendRecap(message, new List<DiscordChannelEnum> { DiscordChannelEnum.Default, DiscordChannelEnum.Admins }, name, r.Id.ToString());
            }
            else
            {
                Dialog.ShowDialog(Tournament.Properties.Resources.Error_Title, Tournament.Properties.Resources.Error_BanSameNumber);
            }
        }

        /// <summary>
        /// Function called to send pick recap on discord through webhooks
        /// </summary>
        public void SendPickRecap()
        {
            // Generate the embed for the pick recap
            Embed message = RefereeMatchHelper.GetInstance(r.Id).GeneratePickRecapMessage(r.Ranking.GetType());
            if (message != null)
            {
                // Depending of the ranking type, we change the name of the payload
                string name;
                if (r.Ranking.GetType() == typeof(Osu.Scores.TeamVs))
                {
                    name = string.Format("{0} VS {1}", ((Osu.Scores.TeamVs)r.Ranking).Red.Name, ((Osu.Scores.TeamVs)r.Ranking).Blue.Name);
                }
                else
                {
                    name = r.Name;
                }
                DiscordHelper.SendRecap(message, new List<DiscordChannelEnum> { DiscordChannelEnum.Admins }, name, r.Id.ToString());
            }
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Update UI function
        /// </summary>
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
                NotifyOfPropertyChange(() => Beatmaps);
            }
        }

        public void UpdateDiscord()
        {
            NotifyOfPropertyChange(() => IsDiscordEnabled);
        }
        #endregion
    }
}
