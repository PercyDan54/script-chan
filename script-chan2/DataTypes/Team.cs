using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace script_chan2.DataTypes
{
    public class Team
    {
        public Team(Tournament tournament, string name, int id = 0)
        {
            Players = new List<Player>();
            this.tournament = tournament;
            this.name = name;
            this.id = id;
        }

        private int id;
        public int Id
        {
            get { return id; }
        }

        private Tournament tournament;
        public Tournament Tournament
        {
            get { return tournament; }
        }

        private string name;
        public string Name
        {
            get { return name; }
            set
            {
                if (value != name)
                {
                    name = value;
                }
            }
        }

        public List<Player> Players;

        public void AddPlayer(Player player)
        {
            if (!Players.Contains(player))
            {
                Players.Add(player);
                Database.Database.AddPlayerToTeam(player, this);
            }
        }

        public void RemovePlayer(Player player)
        {
            if (Players.Remove(player))
            {
                Database.Database.RemovePlayerFromTeam(player, this);
            }
        }

        public void Save()
        {
            if (id == 0)
                id = Database.Database.AddTeam(this);
            else
                Database.Database.UpdateTeam(this);
        }

        public void Delete()
        {
            Database.Database.DeleteTeam(this);
        }
    }
}
