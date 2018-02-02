namespace LegendGenerator.App.ViewModel
{
    public abstract class ViewVM : GalaSoft.MvvmLight.ViewModelBase
    {
        #region Fields   
       
        private bool _isCurrentPage;

        #endregion // Fields

        #region Constructor

        protected ViewVM()
        {
        }

        #endregion // Constructor

        #region Properties           

        public abstract string DisplayName { get; }

        public bool IsCurrentPage
        {
            get { return _isCurrentPage; }
            set
            {
                if (value == _isCurrentPage)
                {
                    return;
                }
                _isCurrentPage = value;
                this.RaisePropertyChanged("IsCurrentPage");
            }
        }

        #endregion     
               

    }
}
