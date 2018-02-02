using GalaSoft.MvvmLight;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.Practices.ServiceLocation;

namespace LegendGenerator.App.ViewModel
{
    /// <summary>
    /// This class contains properties that a View can data bind to.  
    /// </summary>
    public class HelpViewModel : ViewModelBase
    {
        private ViewVM _currentView;
        private ReadOnlyCollection<ViewVM> _views;

        /// <summary>
        /// Initializes a new instance of the HelpViewModel class.
        /// </summary>
        public HelpViewModel()
        {
            this.CreateSubViews();
            SelectedView = this.Views[0];
        }

        public ViewVM SelectedView
        {
            get { return _currentView; }
            set
            {
                if (_currentView != null)
                {
                    _currentView.IsCurrentPage = false;
                }
                Set(ref _currentView, value);
                _currentView.IsCurrentPage = true;
            }
        }

        /// <summary>
        /// Returns a read-only collection of all page ViewModels.
        /// </summary>
        public ReadOnlyCollection<ViewVM> Views
        {
            get
            {
                if (_views == null)
                {
                    this.CreateSubViews();
                }
                return _views;
            }
        }

        #region Private Helpers

        private void CreateSubViews()
        {

            var views = new List<ViewVM>();
            var copyrightViewModel = ServiceLocator.Current.GetInstance<CopyrightViewModel>();
            views.Add(copyrightViewModel);
            var welcomeViewModel = ServiceLocator.Current.GetInstance<OverviewViewModel>();
            views.Add(welcomeViewModel);
            var xpsHelpViewModel = ServiceLocator.Current.GetInstance<XpsHelpViewModel>();
            views.Add(xpsHelpViewModel);
            var xpsHelpEnViewModel = ServiceLocator.Current.GetInstance<XpsHelpEnViewModel>();
            views.Add(xpsHelpEnViewModel);

            _views = new ReadOnlyCollection<ViewVM>(views);
        }

        #endregion


    }
}