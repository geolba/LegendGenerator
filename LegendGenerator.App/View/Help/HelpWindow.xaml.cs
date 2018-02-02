using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using System.Reflection;
using LegendGenerator.App.ViewModel;

namespace LegendGenerator.App.View.Help
{
    /// <summary>
    /// Interaktionslogik für LegendGeneratorHelp.xaml
    /// </summary>
    public partial class HelpWindow : Window
    {       
        private Dictionary<string, UserControl> _userControls = new Dictionary<string, UserControl>();

        public HelpWindow()
        {            
            InitializeComponent();            
            this.DataContext = (this.Resources["Locator"] as ViewModelLocator).Help;

            //List<string> userControlKeys = new List<string>();
            //userControlKeys.Add("LegendGeneratorCopyright");
            //userControlKeys.Add("LegendGeneratorOverView");           
            // userControlKeys.Add("LegendGeneratorXpsHelp");
            //userControlKeys.Add("LegendGeneratorXpsHelpEn");

            //Type type = this.GetType();
            //Assembly assembly = type.Assembly;
            //foreach (string userControlKey in userControlKeys)
            //{
            //    string userControlFullName = String.Format("{0}.{1}", type.Namespace, userControlKey);
            //    UserControl userControl = (UserControl)assembly.CreateInstance(userControlFullName);
            //    _userControls.Add(userControlKey, userControl);
            //}


            //TreeViewItem item = (TreeViewItem)tree.SelectedItem;           
            //this.frame.Content = _userControls[item.Tag.ToString()];
        }

        //private void HelponselectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        //{
        //    if (_isInitializing == false)
        //    {
        //    TreeViewItem item = (TreeViewItem)tree.SelectedItem;
        //    //this.frame.Source = new Uri(item.Tag.ToString(), UriKind.RelativeOrAbsolute);
        //    this.frame.Content = _userControls[item.Tag.ToString()];
        //    }
        //}

      

        
    }
}
