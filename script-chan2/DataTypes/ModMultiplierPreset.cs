using System.Collections.Generic;
using script_chan2.Enums;
using Serilog;

namespace script_chan2.DataTypes
{
    public class ModMultiplierPreset
    {        
        private ILogger localLog = Log.ForContext<ModMultiplierPreset>();

        public ModMultiplierPreset(int id = 0)
        {
            Id = id;
        }

        public int Id { get; private set; }

        public string Name { get; set; }

        public List<ModMultiplier> Multipliers { get; set; } = new List<ModMultiplier>();

        public void Save()
        {
            localLog.Information("'{name}' save", Name);
            if (Id == 0)
                Id = Database.Database.AddModMultiplierPreset(this);
            else
                Database.Database.UpdateModMultiplierPreset(this);
        }

        public void Delete()
        {
            localLog.Information("'{name}' delete", Name);
            foreach (var match in Database.Database.Matches)
            {
                if (match.ModMultiplierPreset == this)
                {
                    match.ModMultiplierPreset = null;
                    match.Save();
                }
            }
            Database.Database.DeleteModMultiplierPreset(this);
        }
    }
}
