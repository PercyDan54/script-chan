using Caliburn.Micro;
using Osu.Api;
using Osu.Mvvm.Miscellaneous;
using Osu.Utils.TeamsOv;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Osu.Utils;

namespace Osu.Mvvm.Teams.ViewModels
{
    /// <summary>
    /// The team view model
    /// </summary>
    public class TeamViewModel : Screen
    {
        #region Attributes
        /// <summary>
        /// The team object
        /// </summary>
        private TeamOv Team { get; set; }
        
        /// <summary>
        /// The list of player view models
        /// </summary>
        protected List<PlayerViewModel> players;
        #endregion

        #region Constructors
        public TeamViewModel(TeamOv team)
        {
            this.Team = team;

            players = new List<PlayerViewModel>();

            foreach (PlayerOv playerOv in Team.Players)
                players.Add(new PlayerViewModel(this, playerOv));
        }
        #endregion

        #region Properties
        /// <summary>
        /// Players property
        /// </summary>
        public IObservableCollection<PlayerViewModel> Players
        {
            get
            {
                return new BindableCollection<PlayerViewModel>(players.OrderBy(entry => entry.Player.Name));
            }
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Updates the view model
        /// </summary>
        public void Update()
        {
            NotifyOfPropertyChange(() => Players);
        }

        /// <summary>
        /// The team name displayed on the UI property
        /// </summary>
        public override string DisplayName
        {
            get
            {
                return Team.Name;
            }
        }

        /// <summary>
        /// Add player(s)
        /// </summary>
        public async void AddPlayers(string input = null)
        {
            // If the osu!api is not valid
            if (!OsuApi.Valid)
                // Error
                Dialog.ShowDialog(Tournament.Properties.Resources.Error_Title, Tournament.Properties.Resources.Error_ApiKeyInvalid);
            // Else
            else
            {
                // Get an input if none is given
                if (input == null)
                    input = await Dialog.ShowInput(Tournament.Properties.Resources.TeamView_AddPlayersTitle, Tournament.Properties.Resources.TeamView_AddPlayersMessage);

                // If something was entered
                if (!string.IsNullOrEmpty(input))
                {
                    var t = Cache.GetCache("osu!options.db").Get("wctype", "Standard");
                    var mode = OsuMode.Standard;
                    switch (t)
                    {
                        case "Standard":
                            mode = OsuMode.Standard;
                            break;
                        case "Taiko":
                            mode = OsuMode.Taiko;
                            break;
                        case "CTB":
                            mode = OsuMode.CTB;
                            break;
                        case "Mania":
                            mode = OsuMode.Mania;
                            break;
                    }

                    List<string> playerslist = new List<string>();
                    if (input.Contains(";"))
                    {
                        foreach (string p in input.Split(';'))
                        {
                            playerslist.Add(p);
                        }
                    }
                    else
                    {
                        playerslist.Add(input);
                    }

                    foreach (string player in playerslist)
                    {
                        // If the input is not valid
                        long id = -1;
                        bool isNumber = long.TryParse(player, out id);
                         
                        // If the player already exists in the list
                        if(Team.Players.Exists(x => x.Name == player || x.Id == id))
                        {
                            // Error
                            Dialog.ShowDialog(Tournament.Properties.Resources.Error_Title, Tournament.Properties.Resources.Error_PlayerAlreadyInTeam);
                        }
                        else
                        {
                            OsuUser osu_player;

                            // Get the player
                            if (isNumber)
                                osu_player = await OsuApi.GetUser(id, mode, false);
                            else
                                osu_player = await OsuApi.GetUser(player, mode, false);

                            // Beatmap doesn't exist
                            if (osu_player == null)
                                // Error
                                Dialog.ShowDialog(Tournament.Properties.Resources.Error_Title, Tournament.Properties.Resources.Error_PlayerNotFound);
                            // Beatmap exists
                            else
                            {
                                // Create a new player wrapper
                                PlayerOv playerOv = new PlayerOv();

                                playerOv.Id = osu_player.Id;
                                playerOv.Name = osu_player.Username;
                                playerOv.Country = osu_player.Country;

                                // Add this player to the team
                                Team.Players.Add(playerOv);

                                // Create a new view model and add it to our vm list
                                players.Add(new PlayerViewModel(this, playerOv));

                                // Beatmap list has changed
                                NotifyOfPropertyChange(() => Players);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Deletes a beatmap
        /// </summary>
        public void Delete(PlayerViewModel model)
        {
            players.Remove(model);
            Team.Players.RemoveAll(x => x.Id == model.Player.Id);

            NotifyOfPropertyChange(() => Players);
        }
        #endregion
    }
}
