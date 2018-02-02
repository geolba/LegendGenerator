using LegendGenerator.App.Resources;

namespace LegendGenerator.App.ViewModel
{
    /// <summary>
    /// This class contains properties that a View can data bind to.
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class OverviewViewModel : ViewVM
    {
        /// <summary>
        /// Initializes a new instance of the OverviewViewModel class.
        /// </summary>
        public OverviewViewModel()
        {
        }

        public override string DisplayName
        {
            get
            {
                return "Übersicht";
            }
        }

    }
}