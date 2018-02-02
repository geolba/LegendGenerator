namespace LegendGenerator.App.ViewModel
{
    /// <summary>
    /// This class contains properties that a View can data bind to.
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class XpsHelpViewModel : ViewVM
    {
        
        /// <summary>
        /// Initializes a new instance of the OverviewViewModel class.
        /// </summary>
        public XpsHelpViewModel()
        {
        }

        public override string DisplayName
        {
            get
            {
                return "Hilfe";
            }
        }

       
    }
}