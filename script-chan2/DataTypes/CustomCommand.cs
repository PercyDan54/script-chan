using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace script_chan2.DataTypes
{
    public class CustomCommand
    {
        private ILogger localLog = Log.ForContext<CustomCommand>();

        #region Constructor
        public CustomCommand(int id = 0)
        {
            Id = id;
        }
        #endregion

        #region Properties
        public int Id { get; private set; }

        public Tournament Tournament { get; set; }

        public string Name { get; set; }

        public string Command { get; set; }
        #endregion

        #region Actions
        public void Save()
        {
            localLog.Information("'{name}' save", Name);
            if (Id == 0)
                Id = Database.Database.AddCustomCommand(this);
            else
                Database.Database.UpdateCustomCommand(this);
        }

        public void Delete()
        {
            localLog.Information("'{name}' delete", Name);
            Database.Database.DeleteCustomCommand(this);
        }
        #endregion
    }
}
