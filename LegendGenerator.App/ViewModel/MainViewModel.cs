using System;
using System.IO;
using GalaSoft.MvvmLight;
using LegendGenerator.App.Model;
using LegendGenerator.App.Utils;
using System.Windows;
using Microsoft.Win32;
using GalaSoft.MvvmLight.CommandWpf;

namespace LegendGenerator.App.ViewModel
{
    /// <summary>
    /// This class contains properties that the main View can data bind to.
    /// <para>
    /// Use the <strong>mvvminpc</strong> snippet to add bindable properties to this ViewModel.
    /// </para> 
    /// </summary>
    public class MainViewModel : ViewModelBase
    {
        #region fields

        private string _title;
        private string _pfadKonfigurationsdatei = String.Empty;//XML-Projektfile;
        private FormularData _formData;

        #endregion

        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public MainViewModel()
        {
            if (IsInDesignMode)
            {
                // Code runs in Blend --> create design time data.
                FormularData formData = new FormularData();
                formData.AccessDatabase = "DesignDatabase";
                formData.TabAccess = true;
                formData.ChkAccess = true;
                this.FormData = formData;
            }
            else
            {
                // Code runs "for real"
                //string folder = Environment.CurrentDirectory;
                string folder = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
                string path = Path.Combine(folder, @"Data\FormData.xml");
                FormularData formData = ObjectXmlSerializer<FormularData>.Load(path);
                this.FormData = formData;

                LoadProjectCommand = new RelayCommand(MnuAppLoad_Click);
                SaveProjectCommand = new RelayCommand(MnuAppSave_Click);
                SaveAsProjectCommand = new RelayCommand(MnuAppSaveNew_Click);
            }
        }

        #region commands

        public RelayCommand LoadProjectCommand { get; private set; }
        public RelayCommand SaveProjectCommand { get; private set; }
        public RelayCommand SaveAsProjectCommand { get; private set; }

        #endregion

        public FormularData FormData
        {
            get
            {
                return this._formData;               
            }
            set
            {
                this._formData = value;
                base.RaisePropertyChanged("FormData");
            }
        }

        public string Title
        {
            get { return this._title; }
            set
            {
                this._title = value;
                base.RaisePropertyChanged("Title");
            }
        }
        
        private void MnuAppLoad_Click()
        {
            Microsoft.Win32.OpenFileDialog oXml = new OpenFileDialog
            {
                Title = "Select XML-ConfigFile",
                Filter = "XML (*.xml)|*.xml"
            };
            //oDlg.RestoreDirectory = true;
            string dir = Environment.GetFolderPath(Environment.SpecialFolder.MyComputer);
            oXml.InitialDirectory = dir;

            // Show open file dialog box
            Nullable<bool> result = oXml.ShowDialog();
            if (result == true)
            {
                this._pfadKonfigurationsdatei = oXml.FileName.ToString();
            }
            // Load the form object from the existing XML file (if any)...
            if (File.Exists(this._pfadKonfigurationsdatei) == true)
            {
                // Load the form object from the XML file using our custom class...
                FormularData formdata = ObjectXmlSerializer<FormularData>.Load(this._pfadKonfigurationsdatei);

                if (formdata == null)
                {
                    MessageBox.Show("Unable to load a formular data object from file '" + this._pfadKonfigurationsdatei + "'!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else  // Load form properties into form data...
                {
                    //this.LoadCustomerIntoForm(formdata);
                    this.FormData.DeepCopy(formdata);                   
                    //this.FormData = formdata;
                    this.Title = this._pfadKonfigurationsdatei;
                }
            }
            else
            {
                MessageBox.Show(this.CreateFileDoesNotExistMsg(), "Information", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void MnuAppSave_Click()
        {
            if (File.Exists(this._pfadKonfigurationsdatei) == true)
            {
                // Create form object based on Form values.
                FormularData formdata = this.FormData;// this.CreateFormularData();

                //Save form object to XML file using our ObjectXMLSerializer class...
                try
                {
                    ObjectXmlSerializer<FormularData>.Save(formdata, this._pfadKonfigurationsdatei);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Unable to save formular data object!" + Environment.NewLine + Environment.NewLine + ex.Message,
                        "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                this.MnuAppSaveNew_Click();
            }

        }

        //save as the projectfile:
        private void MnuAppSaveNew_Click()
        {
            Microsoft.Win32.SaveFileDialog sXml = new Microsoft.Win32.SaveFileDialog();
            string dir = Environment.GetFolderPath(Environment.SpecialFolder.MyComputer);
            sXml.InitialDirectory = dir;
            sXml.Title = "Select a folder for creating a new projectfile!";
            sXml.Filter = "XML (*.xml)|*.xml";

            // pressed OK:
            if (sXml.ShowDialog() == true)
            {
                this._pfadKonfigurationsdatei = sXml.FileName.ToString();
            }

            //if (File.Exists(this.txtKonfigurationsdatei.Text))
            if (this._pfadKonfigurationsdatei != String.Empty)
            {
                // Create customer object based on Form values.
                FormularData formdata = this.FormData;//this.CreateFormularData();

                //Save form object to XML file using our ObjectXMLSerializer class...
                try
                {
                    ObjectXmlSerializer<FormularData>.Save(formdata, this._pfadKonfigurationsdatei);
                    this.Title = "Legend Generator 5.0 für ArcMap 10.0 - " + this._pfadKonfigurationsdatei;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Unable to save formular data object!" + Environment.NewLine + Environment.NewLine + ex.Message,
                        "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

        }

        #region private helper methods

        private string CreateFileDoesNotExistMsg()
        {
            return "The example XML file '" + _pfadKonfigurationsdatei + "' does not exist." + "\n\n" +
            "To create the example XML file, enter formular data details, then click the 'Save' button.";
        }          

        #endregion                

    }
}