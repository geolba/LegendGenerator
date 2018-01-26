using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.IO;
using Microsoft.Win32;

namespace LegendGenerator.App.View
{
    /// <summary>
    /// Interaktionslogik für StylefileDialog.xaml
    /// </summary>
    public partial class StylefileDialog : Window
    {
        MainWindow lgw;
        bool btnClicked = false;

        //constructor:
        public StylefileDialog(MainWindow window1)
        {
            lgw = window1;
            InitializeComponent();
        }

        //properties:
        public bool BtnClicked
        {
            get
            {
                return this.btnClicked;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void btnSearchStylefile_Click(object sender, RoutedEventArgs e)
        {
            //Öffnen eines File Dialogs zur Auswahl der Access-Datenbank:
            OpenFileDialog oDlg = new OpenFileDialog();
            oDlg.Title = "Please select a style-file!";
            oDlg.Filter = "STYLE (*.Style)|*.style";
            //oDlg.RestoreDirectory = true;
            string dir = Environment.GetFolderPath(Environment.SpecialFolder.MyComputer);
            oDlg.InitialDirectory = dir;

            // Show open file dialog box
            Nullable<bool> result = oDlg.ShowDialog();

            // button OK is pressed:
            if (result == true)
            {
                this.txtStyleFilePath.Text = oDlg.FileName.ToString();

            }
            else
            {
                MessageBox.Show("The style-file fining process has been stopped!", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void btnStylefileAdd_Click(object sender, RoutedEventArgs e)
        {
            this.btnClicked = true;
            this.Close();
        }

        private void txtStyleFilePath_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (this.txtStyleFilePath.Text != String.Empty && File.Exists(this.txtStyleFilePath.Text) == true)
            {
                this.btnStylefileAdd.IsEnabled = true;
            }
            else
            {
                this.btnStylefileAdd.IsEnabled = false;
            }
        }

        private void Window_Activated(object sender, EventArgs e)
        {
            Topmost = true;
        }

       

    }
}
