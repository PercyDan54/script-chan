using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Osu.Mvvm.Preparation.ViewModels
{
    public class PreparationViewModel : Screen
    {
        #region Attributes
        /// <summary>
        /// The list of PreparationRoom view models
        /// </summary>
        protected IObservableCollection<PreparationRoomViewModel> matches;
        #endregion

        #region Constructor
        public PreparationViewModel()
        {
            matches = new BindableCollection<PreparationRoomViewModel>();
        }
        #endregion

        #region Properties
        /// <summary>
        /// Preparation Property
        /// </summary>
        public IObservableCollection<PreparationRoomViewModel> ViewPreparations
        {
            get
            {
                return matches;
            }
        }
        #endregion

        #region Public Methods
        public void addPreparation()
        {
            matches.Add(new PreparationRoomViewModel());
            NotifyOfPropertyChange(() => ViewPreparations);
        }

        public void Generate()
        {
            addPreparation();
        }
        #endregion
    }
}
