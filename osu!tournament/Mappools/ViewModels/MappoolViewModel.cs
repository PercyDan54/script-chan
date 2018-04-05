using Caliburn.Micro;
using Osu.Api;
using Osu.Mvvm.Miscellaneous;
using Osu.Scores;
using System.Collections.Generic;
using System.Linq;

namespace Osu.Mvvm.Mappools.ViewModels
{
    /// <summary>
    /// Represents a mappools view model
    /// </summary>
    public class MappoolViewModel : Screen
    {
        #region Attributes
        /// <summary>
        /// The mappool
        /// </summary>
        protected Mappool mappool;

        /// <summary>
        /// The list of beatmap view models
        /// </summary>
        protected List<BeatmapViewModel> beatmaps;
        #endregion

        #region Constructors
        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="mappool">the mappool</param>
        public MappoolViewModel(Mappool mappool)
        {
            DisplayName = mappool.Name;
            this.mappool = mappool;

            beatmaps = new List<BeatmapViewModel>();

            foreach (Beatmap beatmap in mappool.Pool.Values)
                beatmaps.Add(new BeatmapViewModel(this, beatmap));
        }
        #endregion

        #region Properties
        /// <summary>
        /// Beatmaps property
        /// </summary>
        public IObservableCollection<BeatmapViewModel> Beatmaps
        {
            get
            {
                return new BindableCollection<BeatmapViewModel>(beatmaps.OrderBy(entry => entry.Beatmap));
            }
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Updates the view model
        /// </summary>
        public void Update()
        {
            NotifyOfPropertyChange(() => Beatmaps);
        }

        /// <summary>
        /// Adds a beatmap
        /// </summary>
        public async void AddBeatmap()
        {
            // If the osu!api is not valid
            if (!OsuApi.Valid)
                // Error
                Dialog.ShowDialog("Whoops!", "The osu!api key is not valid!");
            // Else
            else
            {
                // Get an input
                string input = await Dialog.ShowInput("Add Beatmap", "Enter the beatmap id (Must be a valid number). You can use ';' as a separator");

                // If something was entered
                if (!string.IsNullOrEmpty(input))
                {
                    List<string> maps = new List<string>();
                    if (input.Contains(";"))
                    {
                        foreach (string map in input.Split(';'))
                        {
                            maps.Add(map);
                        }
                    }
                    else
                    {
                        maps.Add(input);
                    }

                    foreach (string map in maps)
                    {
                        // If the input is not valid
                        long id = -1;
                        if (!long.TryParse(map, out id))
                            // Error
                            Dialog.ShowDialog("Whoops!", "The entered id is not a valid number!");
                        // Else
                        else
                        {
                            // If the beatmap already exists in the mappool
                            if (mappool.Pool.ContainsKey(id))
                                // Error
                                Dialog.ShowDialog("Whoops!", "The beatmap already exists!");
                            // Else
                            else
                            {
                                // Get the osu!beatmap
                                OsuBeatmap osu_beatmap = await OsuApi.GetBeatmap(id, false);

                                // Beatmap doesn't exist
                                if (osu_beatmap == null)
                                    // Error
                                    Dialog.ShowDialog("Whoops!", "The beatmap does not exist!");
                                // Beatmap exists
                                else
                                {
                                    // Create a new beatmap wrapper
                                    Beatmap beatmap = new Beatmap();

                                    // Change its osu!beatmap
                                    beatmap.OsuBeatmap = osu_beatmap;
                                    beatmap.Id = osu_beatmap.BeatmapID;
                                    beatmap.AddMod(PickType.None);

                                    // Add this beatmap to the mappool list
                                    mappool.Pool[osu_beatmap.BeatmapID] = beatmap;

                                    // Create a new view model and add it to our vm list
                                    beatmaps.Add(new BeatmapViewModel(this, beatmap));

                                    // Beatmap list has changed
                                    NotifyOfPropertyChange(() => Beatmaps);
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Deletes a beatmap
        /// </summary>
        public async void Delete(BeatmapViewModel model)
        {
            if (await Dialog.ShowConfirmation("Delete beatmap", "Are you sure you want to delete the beatmap \"" + model.DisplayName + "\"?"))
            {
                // Check all rooms if the mappool is already in use
                if (Room.Rooms.Values.ToList().Any(x => x.Mappool == mappool))
                {
                    Dialog.ShowDialog("Delete beatmap", "Beatmap could not be deleted because the mappool is used in a room.");
                    return;
                }
                
                beatmaps.Remove(model);
                mappool.Pool.Remove(model.Beatmap.OsuBeatmap.BeatmapID);

                NotifyOfPropertyChange(() => Beatmaps);
            }
        }
        #endregion
    }
}
