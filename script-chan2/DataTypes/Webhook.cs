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
        public Webhook(int id = 0)
        {
            Id = id;
        }

        public int Id { get; private set; }

        public string Name { get; set; }

        public string URL { get; set; }

        public void Save()
        {
            Log.Information("Webhook: '{name}' save", Name);
            if (Id == 0)
                Id = Database.Database.AddWebhook(this);
            else
                Database.Database.UpdateWebhook(this);
        }

        public void Delete()
        {
            Log.Information("Webhook: '{name}' delete", Name);
            foreach (var tournament in Database.Database.Tournaments)
            {
                tournament.RemoveWebhook(this);
            }
            Database.Database.DeleteWebhook(this);
        }
    }
}
