using Serilog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace script_chan2.DataTypes
{
    public class Webhook
    {
        private ILogger localLog = Log.ForContext<Webhook>();

        #region Constructor
        public Webhook(int id = 0)
        {
            Id = id;
        }
        #endregion

        #region Properties
        public int Id { get; private set; }

        public string Name { get; set; }

        public string URL { get; set; }

        public bool MatchCreated { get; set; }

        public bool BanRecap { get; set; }

        public bool PickRecap { get; set; }

        public bool GameRecap { get; set; }

        public string FooterText { get; set; }

        public string FooterIcon { get; set; }

        public string WinImage { get; set; }
        #endregion

        #region Actions
        public void Save()
        {
            localLog.Information("'{name}' save", Name);
            if (Id == 0)
                Id = Database.Database.AddWebhook(this);
            else
                Database.Database.UpdateWebhook(this);
        }

        public void Delete()
        {
            localLog.Information("'{name}' delete", Name);
            foreach (var tournament in Database.Database.Tournaments)
            {
                tournament.RemoveWebhook(this);
            }
            Database.Database.DeleteWebhook(this);
        }
        #endregion
    }
}
