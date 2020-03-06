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
            this.name = name;
            this.url = url;
            this.id = id;
        }

        private int id;
        public int Id
        {
            get { return id; }
        }

        private string name;
        public string Name
        {
            get { return name; }
            set
            {
                name = value;
            }
        }

        private string url;
        public string URL
        {
            get { return url; }
            set
            {
                url = value;
            }
        }

        public void Save()
        {
            if (id == 0)
                id = Database.Database.AddWebhook(this);
            else
                Database.Database.UpdateWebhook(this);
        }

        public void Delete()
        {
            foreach (var tournament in Database.Database.Tournaments)
            {
                tournament.RemoveWebhook(this);
            }
            Database.Database.DeleteWebhook(Id);
        }
    }
}
