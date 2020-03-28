using Serilog;
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
        private ILogger localLog = Log.ForContext<Team>();

        public Team(int id = 0)
        {
            Players = new List<Player>();
            Id = id;
        }

        public int Id { get; private set; }
        public Tournament Tournament { get; set; }

        public string Name { get; set; }

        public List<Player> Players;

        public void AddPlayer(Player player)
        {
            localLog.Information("'{name}' add player '{player}'", Name, player.Name);
            if (!Players.Contains(player))
            {
                Players.Add(player);
                Database.Database.AddPlayerToTeam(player, this);
            }
        }

        public void RemovePlayer(Player player)
        {
            localLog.Information("'{name}' remove player '{player}'", Name, player.Name);
            if (Players.Remove(player))
            {
                Database.Database.RemovePlayerFromTeam(player, this);
            }
        }

        public void Save()
        {
            localLog.Information("'{name}' save", Name);
            if (Id == 0)
                Id = Database.Database.AddTeam(this);
            else
                Database.Database.UpdateTeam(this);
        }

        public void Delete()
        {
            localLog.Information("'{name}' delete", Name);
            Database.Database.DeleteTeam(this);
        }
    }
}
