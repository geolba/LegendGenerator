using GalaSoft.MvvmLight;
using LegendGenerator.App.Resources;
using System.Threading;

namespace LegendGenerator.App.ViewModel
{
    public class LocalizableViewModel : ViewModelBase
    {
        private Resource _resources = new Resource();

        public LocalizableViewModel() : base() { }

        protected void ChangeCulture(string lang)
        {
            var culture = new System.Globalization.CultureInfo(lang);
            Thread.CurrentThread.CurrentUICulture = culture;
            this.RaisePropertyChanged("LocalizedText");
        }

        public Resource LocalizedText
        {
            get
            {
                return _resources;
            }
        }

        private string selectedCulture;
        public string SelectedCulture
        {
            get { return this.selectedCulture; }
            set
            {
                if (value != this.selectedCulture)
                {
                    this.selectedCulture = value;
                    this.ChangeCulture(value);
                    base.RaisePropertyChanged("SelectedCulture");
                }
            }
        }
    }
}
