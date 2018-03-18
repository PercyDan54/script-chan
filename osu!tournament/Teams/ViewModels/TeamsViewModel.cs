using Caliburn.Micro;
using Osu.Mvvm.Miscellaneous;
using Osu.Mvvm.Ov.ViewModels;
using Osu.Utils.TeamsOv;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Osu.Mvvm.Teams.ViewModels
{
    public class TeamsViewModel : Conductor<TeamViewModel>.Collection.OneActive
    {
        #region Attributes
        /// <summary>
        /// The list of teams
        /// </summary>
        private IObservableCollection<TeamViewModel> teams;

        /// <summary>
        /// The OvViewModel to update teams when I create a new one
        /// </summary>
        private OvViewModel ov;

        /// <summary>
        /// The selected team
        /// </summary>
        private TeamViewModel selected;

        /// <summary>
        /// Adds player with same name as team at teamcreation
        /// </summary>
        private bool addPlayerWithTeamname;
        #endregion


        public TeamsViewModel(OvViewModel overview)
        {
            ov = overview;
            DisplayName = "Teams";
            teams = new BindableCollection<TeamViewModel>();
            selected = null;

            foreach (TeamOv team in TeamManager.Teams)
                teams.Add(new TeamViewModel(team));
        }

        #region Properties
        /// <summary>
        /// Teams property
        /// </summary>
        public IObservableCollection<TeamViewModel> Teams
        {
            get
            {
                return teams;
            }
        }

        /// <summary>
        /// Selected mappool property
        /// </summary>
        public TeamViewModel SelectedTeam
        {
            get
            {
                return selected;
            }
            set
            {
                if (value != selected)
                {
                    selected = value;

                    // Activate the selected item
                    if(selected != null)
                        ActivateItem(selected);

                    // Selected mappool has changed
                    NotifyOfPropertyChange(() => SelectedTeam);

                    // We have a new mappool that can be deleted
                    NotifyOfPropertyChange(() => CanDeleteTeam);
                }
            }
        }

        public bool AddPlayerWithTeamname
        {
            get { return addPlayerWithTeamname; }
            set
            {
                if (value != addPlayerWithTeamname)
                {
                    addPlayerWithTeamname = value;
                    NotifyOfPropertyChange(() => AddPlayerWithTeamname);
                }
            }
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Checks if we can delete a team
        /// </summary>
        /// <returns>true/false</returns>
        public bool CanDeleteTeam
        {
            get
            {
                return SelectedTeam != null;
            }
        }

        /// <summary>
        /// Adds a new team
        /// </summary>
        public async void AddTeam()
        {
            // Get the team name
            string name = await Dialog.ShowInput("Add a team", "Enter the team name");

            // If the user entered a name
            if (!string.IsNullOrEmpty(name))
            {
                // Create a new team
                TeamOv team = TeamManager.Get(name);

                TeamViewModel model = new TeamViewModel(team);

                // Add this model to the team list
                teams.Add(model);

                Log.Info("Adding team \"" + name + "\"");

                // Change the currently selected team
                SelectedTeam = model;

                // Update the overview view
                ov.UpdateTeams();

                // team list has changed
                NotifyOfPropertyChange(() => Teams);

                if (addPlayerWithTeamname)
                {
                    model.AddPlayers(name);
                }
            }
        }

        /// <summary>
        /// Deletes the currently selected team
        /// </summary>
        public void DeleteTeam()
        {
            // Get the currently selected team
            TeamViewModel team = SelectedTeam;

            // Remove this team from our lists of teams
            teams.Remove(team);
            TeamManager.Remove(team.DisplayName);

            DeactivateItem(team, true);

            Log.Info("Deleting team \"" + team.DisplayName + "\"");

            // Select the first team from our list (Or null if there is no team)
            SelectedTeam = teams.Count == 0 ? null : teams[0];

            // Update the overview view for team creation
            ov.UpdateTeams();

            // team list has changed
            NotifyOfPropertyChange(() => Teams);
            NotifyOfPropertyChange(() => SelectedTeam);
        }
        #endregion
    }
}
