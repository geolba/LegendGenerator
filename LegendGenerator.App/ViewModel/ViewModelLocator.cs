/*
  In App.xaml:
  <Application.Resources>
      <vm:ViewModelLocator xmlns:vm="clr-namespace:LegendGenerator.App"
                           x:Key="Locator" />
  </Application.Resources>
  
  In the View:
  DataContext="{Binding Source={StaticResource Locator}, Path=ViewModelName}"
*/

using GalaSoft.MvvmLight.Ioc;
using Microsoft.Practices.ServiceLocation;
using LegendGenerator.App.Model;

namespace LegendGenerator.App.ViewModel
{
    /// <summary>
    /// This class contains static references to all the view models in the
    /// application and provides an entry point for the bindings.
    /// </summary>
    public class ViewModelLocator
    {
        /// <summary>
        /// Initializes a new instance of the ViewModelLocator class.
        /// </summary>
        static ViewModelLocator()
        {
            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);   
            SimpleIoc.Default.Register<IDataService, DataService>();
            

            SimpleIoc.Default.Register<MainViewModel>(true);

            SimpleIoc.Default.Register<CopyrightViewModel>(true);
            SimpleIoc.Default.Register<OverviewViewModel>(true);
            SimpleIoc.Default.Register<XpsHelpViewModel>(true);
            SimpleIoc.Default.Register<XpsHelpEnViewModel>(true);
            SimpleIoc.Default.Register<HelpViewModel>(true);
          
        }

        public MainViewModel Main
        {
            get
            {
                return ServiceLocator.Current.GetInstance<MainViewModel>();
            }
        }

        public HelpViewModel Help
        {
            get
            {
                return ServiceLocator.Current.GetInstance<HelpViewModel>();
            }
        }
       
        public static void Cleanup()
        {
            // TODO Clear the ViewModels
        }
    }
}