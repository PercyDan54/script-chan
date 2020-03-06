using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace script_chan2.DataTypes
{
    public class Player
    {
        public Player(string name, string country, int id)
        {
            Name = name;
            Country = country;
            Id = id;
        }

        public int Id { get; }

        public string Name { get; }

        public string Country { get; }

        public override bool Equals(object obj)
        {
            var other = obj as Player;

            if (other == null)
                return false;

            return Id == other.Id;
        }
    }
}
