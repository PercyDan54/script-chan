using System;
using script_chan2.Enums;

namespace script_chan2.DataTypes
{
    public class ModMultiplier : IEquatable<ModMultiplier>
    {
        public ModMultiplier(GameMods mods, double multiplier = 1)
        {
            Mods = mods;
            Multiplier = multiplier;
        }

        public GameMods Mods { get; set; }
        public double Multiplier { get; set; }

        public bool Equals(ModMultiplier other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            return Mods == other.Mods;
        }

        public override bool Equals(object obj)
        {
            if (obj is null) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((ModMultiplier)obj);
        }

        public override int GetHashCode() => (int)Mods;

        public static bool operator ==(ModMultiplier left, ModMultiplier right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(ModMultiplier left, ModMultiplier right)
        {
            return !Equals(left, right);
        }
    }
}
