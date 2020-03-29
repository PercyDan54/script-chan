using Caliburn.Micro;
using script_chan2.DataTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace script_chan2.GUI
{
    public class ExportViewModel : Screen
    {
        #region Constructor
        protected override void OnActivate()
        {
            ExportItems = new BindableCollection<ExportItem>();
            foreach (var tournament in Database.Database.Tournaments)
            {
                var tournamentItem = new ExportItem { Type = ExportItemTypes.Tournament, Id = tournament.Id, Name = tournament.Name };
                var teamsCategory = new ExportCategory { Name = "Teams", Parent = tournamentItem };
                foreach (var team in tournament.Teams)
                {
                    var teamItem = new ExportItem { Type = ExportItemTypes.Team, Id = team.Id, Name = team.Name, Parent = teamsCategory };
                    teamsCategory.ExportItems.Add(teamItem);
                }
                tournamentItem.ExportCategories.Add(teamsCategory);

                var webhooksCategory = new ExportCategory { Name = "Webhooks", Parent = tournamentItem };
                foreach (var webhook in tournament.Webhooks)
                {
                    var webhookItem = new ExportItem { Type = ExportItemTypes.Webhook, Id = webhook.Id, Name = webhook.Name, Parent = webhooksCategory };
                    webhooksCategory.ExportItems.Add(webhookItem);
                }
                tournamentItem.ExportCategories.Add(webhooksCategory);

                var mappoolsCategory = new ExportCategory { Name = "Mappools", Parent = tournamentItem };
                foreach (var mappool in tournament.Mappools)
                {
                    var mappoolItem = new ExportItem { Type = ExportItemTypes.Mappool, Id = mappool.Id, Name = mappool.Name, Parent = mappoolsCategory };
                    mappoolsCategory.ExportItems.Add(mappoolItem);
                }
                tournamentItem.ExportCategories.Add(mappoolsCategory);

                var matchesCategory = new ExportCategory { Name = "Matches", Parent = tournamentItem };
                foreach (var match in Database.Database.Matches.Where(x => x.Tournament == tournament))
                {
                    var matchItem = new ExportItem { Type = ExportItemTypes.Match, Id = match.Id, Name = match.Name, Parent = matchesCategory };
                    matchesCategory.ExportItems.Add(matchItem);
                }
                tournamentItem.ExportCategories.Add(matchesCategory);
                
                ExportItems.Add(tournamentItem);
            }
            NotifyOfPropertyChange(() => ExportItems);
        }
        #endregion

        #region Properties
        public bool ExportEnabled
        {
            get { return true; }
        }
        #endregion

        #region Tree items
        public BindableCollection<ExportItem> ExportItems { get; set; }
        #endregion
    }

    public class ExportItem : Screen
    {
        public ExportItem()
        {
            ExportCategories = new BindableCollection<ExportCategory>();
            Export = false;
        }

        public ExportItemTypes Type { get; set; }
        private bool export;
        public bool Export
        {
            get { return export; }
            set
            {
                if (value != export)
                {
                    export = value;
                    if (value)
                    {
                        if (Parent != null && Parent.Parent != null)
                        {
                            Parent.Parent.SetExportFromChild(value);
                        }
                    }
                    foreach (var category in ExportCategories)
                    {
                        foreach (var item in category.ExportItems)
                        {
                            item.SetExportFromParent(value);
                        }
                    }
                    NotifyOfPropertyChange(() => Export);
                }
            }
        }
        public int Id { get; set; }
        public string Name { get; set; }
        public BindableCollection<ExportCategory> ExportCategories { get; set; }
        public ExportCategory Parent { get; set; }

        public void SetExportFromChild(bool export)
        {
            this.export = export;
            NotifyOfPropertyChange(() => Export);
            if (Parent != null && Parent.Parent != null)
            {
                Parent.Parent.SetExportFromChild(export);
            }
        }

        public void SetExportFromParent(bool export)
        {
            this.export = export;
            NotifyOfPropertyChange(() => Export);
            foreach (var category in ExportCategories)
            {
                foreach (var item in category.ExportItems)
                {
                    item.SetExportFromParent(export);
                }
            }
        }
    }

    public class ExportCategory
    {
        public ExportCategory()
        {
            ExportItems = new BindableCollection<ExportItem>();
        }

        public string Name { get; set; }
        public BindableCollection<ExportItem> ExportItems { get; set; }
        public ExportItem Parent { get; set; }
    }

    public enum ExportItemTypes
    {
        Tournament,
        Webhook,
        Team,
        Mappool,
        Match
    }
}
