using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.IO;
//using LegendGenerator.Core.CommonDialogWrappers;
using WPFFolderBrowser;
using LegendGenerator.App.Model;

namespace LegendGenerator.App.View
{
    /// <summary>
    /// Interaktionslogik für SymbolDialog.xaml
    /// </summary>
    public partial class SymbolDialog : Window
    {
        FormularData _formData;
        private bool _isInitializing = false;
        private bool btnClicked = false;

        //konstruktor:
        public SymbolDialog(FormularData formData)
        {
            this._isInitializing = true;

            this._formData = formData;
            InitializeComponent();
            this._isInitializing = false;
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

        private void btnSymbolDirectory_Click(object sender, RoutedEventArgs e)
        {
           //BrowseForFolderDialog dlg = new BrowseForFolderDialog();
            WPFFolderBrowserDialog dlg = new WPFFolderBrowserDialog();
            dlg.Title = "Select a directory and click OK!";
            //dlg.InitialExpandedFolder = Environment.SpecialFolder.Desktop.ToString();
            dlg.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            //dlg.OKButtonText = "OK!";            
            if (true == dlg.ShowDialog(this))
            {
                // Do something with the selected folder...
                //this.txtSymbolDirectory.Text = dlg.SelectedFolder;
                this.txtSymbolDirectory.Text = dlg.FileName;
            }
        }

        private void chkGifExport_Checked(object sender, RoutedEventArgs e)
        {
            if (this._isInitializing == false)
            {
                if (Directory.Exists(this.txtSymbolDirectory.Text))
                {
                    this.btnSymbolOk.IsEnabled = true;
                }

            }
        }

        private void chkGifExport_Unchecked(object sender, RoutedEventArgs e)
        {
            if (this._isInitializing == false)
            {
                this.btnSymbolOk.IsEnabled = false;
            }
        }

        private void txtSymbolDirectory_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (this._isInitializing == false)
            {
                if (this.txtSymbolDirectory.Text != String.Empty && Directory.Exists(this.txtSymbolDirectory.Text) == true)
                {
                    if (this.chkGifExport.IsChecked == false)
                    {
                        this.chkGifExport.IsChecked = true;
                    }
                    this.btnSymbolOk.IsEnabled = true;
                }
                else
                {
                    if (this.chkGifExport.IsChecked == true)
                    {
                        this.chkGifExport.IsChecked = false;
                    }
                    this.btnSymbolOk.IsEnabled = false;
                }
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (this._formData.ChkGraphicExport == true && this._formData.GraphicExportDirectory != String.Empty)
            {
                this.chkGifExport.IsChecked = true;
                this.txtSymbolDirectory.Text = this._formData.GraphicExportDirectory;
            }
            else
            {
                this.chkGifExport.IsChecked = false;
                this.txtSymbolDirectory.Text = String.Empty;
            }
        }

        private void btnSymbolOk_Click(object sender, RoutedEventArgs e)
        {
            if (Directory.Exists(this.txtSymbolDirectory.Text))
            {
                _formData.GraphicExportDirectory = this.txtSymbolDirectory.Text;
                _formData.ChkGraphicExport = true;
            }
            //Dialog wieder schließen:
            this.btnClicked = true;
            this.Close();
        }

        private void btnSymbolNotSave_Click(object sender, RoutedEventArgs e)
        {
            _formData.GraphicExportDirectory = String.Empty;
            _formData.ChkGraphicExport = false;
            this.Close();
        }

        private void Window_Activated(object sender, EventArgs e)
        {
            Topmost = true;
        }
        
    }
}
