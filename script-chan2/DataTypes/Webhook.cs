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
        public Webhook(string name, string url, int id = 0)
        {
            Name = name;
            URL = url;
            Id = id;
        }

        public int Id { get; private set; }

        public string Name { get; set; }

        public string URL { get; set; }

        public void Save()
        {
            if (Id == 0)
                Id = Database.Database.AddWebhook(this);
            else
                Database.Database.UpdateWebhook(this);
        }

        public void Delete()
        {
            foreach (var tournament in Database.Database.Tournaments)
            {
                tournament.RemoveWebhook(this);
            }
            Database.Database.DeleteWebhook(this);
        }
    }
}
