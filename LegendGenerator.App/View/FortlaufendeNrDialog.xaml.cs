using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.IO;
using Microsoft.Win32;

namespace LegendGenerator.App.View
{
    /// <summary>
    /// Interaktionslogik für FortlaufendeNrDialog.xaml
    /// </summary>
    public partial class FortlaufendeNrDialog : Window
    {
        MainWindow lgw;
        private bool _isInitializing = false;
        private bool btnClicked = false;
       

        //konstruktor:
        public FortlaufendeNrDialog(MainWindow window1)
        {
            this._isInitializing = true;

            this.lgw = window1;
            InitializeComponent();
            this._isInitializing = false; ;
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

        private void btnFortlaufendeNummer_Click(object sender, RoutedEventArgs e)
        {
            //Öffnen eines File Dialogs zur Auswahl der Konfigurationsdatei:
            OpenFileDialog oTxt = new OpenFileDialog();
            oTxt.Title = "Select TXT-FILE fo the lookup-table";
            oTxt.Filter = "TXT (*.txt)|*.txt";
            //oDlg.RestoreDirectory = true;
            string dir = Environment.GetFolderPath(Environment.SpecialFolder.MyComputer);
            oTxt.InitialDirectory = dir;

            // Show open file dialog box
            Nullable<bool> result = oTxt.ShowDialog();
            // OK wurde gedrückt:
            if (result == true)
            {
                this.txtFortlaufendeNummer.Text = oTxt.FileName.ToString();
            }
        }

        private void chkFortlaufendeNummer_Checked(object sender, RoutedEventArgs e)
        {
            if (this._isInitializing == false  )
            {        
                if (File.Exists(this.txtFortlaufendeNummer.Text))
                {
                    this.btnFortlaufendeNrOk.IsEnabled = true;
                    this.lgw.chkLegendennummer.IsChecked = false;
                }
                
            }
           
        }

        private void chkFortlaufendeNummer_Unchecked(object sender, RoutedEventArgs e)
        {
            if (this._isInitializing == false)
            {
                this.btnFortlaufendeNrOk.IsEnabled = false;
               // this.lgw.chkLegendennummer.IsChecked = true;
            }
        }

        

        private void txtFortlaufendeNummer_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (this._isInitializing == false)
            {
                if (this.txtFortlaufendeNummer.Text != String.Empty && File.Exists(this.txtFortlaufendeNummer.Text) == true)
                {
                    //if (this.chkFortlaufendeNummer.IsChecked == false)
                    //{
                    //this.chkFortlaufendeNummer.IsChecked = false;
                    this.chkFortlaufendeNummer.IsChecked = true;
                    //}
                    this.btnFortlaufendeNrOk.IsEnabled = true;
                }
                else
                {
                    //if (this.chkFortlaufendeNummer.IsChecked == true)
                    //{
                    this.chkFortlaufendeNummer.IsChecked = false;
                    //}
                    this.btnFortlaufendeNrOk.IsEnabled = false;
                }
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //if (this.lgw.FortlaufendeNummer == true && this.lgw.PfadTextFileFortlaufendeNr != String.Empty)
            //{
            //    this.chkFortlaufendeNummer.IsChecked = true;
            //    this.txtFortlaufendeNummer.Text = this.lgw.PfadTextFileFortlaufendeNr;
            //}
            //else
            //{
            //    this.chkFortlaufendeNummer.IsChecked = false;
            //    this.txtFortlaufendeNummer.Text =String.Empty;
            //}
            
        }
        private void btnFortlaufendeNrOk_Click(object sender, RoutedEventArgs e)
        {
            //if (File.Exists(this.txtFortlaufendeNummer.Text))
            //{
            //    lgw.PfadTextFileFortlaufendeNr = this.txtFortlaufendeNummer.Text;
            //    lgw.FortlaufendeNummer = true;
            //    lgw.chkLegendennummer.IsChecked = false;//chechbox-control des Hauptfensters!!!

            //}
            ////Dialog wieder schließen:
            //this.btnClicked = true;
            this.Close();
        }


    }
}
