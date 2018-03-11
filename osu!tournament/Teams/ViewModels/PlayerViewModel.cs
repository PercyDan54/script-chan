using Caliburn.Micro;
using Osu.Utils.TeamsOv;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Osu.Mvvm.Teams.ViewModels
{
    public class PlayerViewModel : Screen
    {
        public PlayerOv Player { get; set; }
        protected TeamViewModel parent;

        public PlayerViewModel(TeamViewModel parent, PlayerOv player)
        {
            this.parent = parent;
            this.Player = player;
        }

        public override string DisplayName
        {
            get
            {
                return Player.Name + " [" + Player.Country + "]";
            }
        }

        public void Delete()
        {
            parent.Delete(this);
        }
    }
}
