using System;
using System.Windows;

namespace LegendGenerator.App.View
{
    /// <summary>
    /// Interaktionslogik für ProgressDialog.xaml
    /// </summary>
    public partial class ProgressDialog : Window
    {
        public ProgressDialog()
        {
            InitializeComponent();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            //Cancel(sender, e);
            this.IsEnabled = false;
            this.Close();
        }

        public string ProgressText
        {
            set
            {
                this.StatusText.Content = value;
            }
        }

        public int ProgressValue
        {
            set
            {
                this.Progress.Value = value;
            }
        }

    }
}
