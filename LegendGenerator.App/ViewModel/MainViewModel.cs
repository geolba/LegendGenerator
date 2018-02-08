using System;
using System.IO;
using GalaSoft.MvvmLight;
using LegendGenerator.App.Model;
using LegendGenerator.App.Utils;
using System.Windows;
using Microsoft.Win32;
using GalaSoft.MvvmLight.CommandWpf;
using System.Collections.Generic;
using System.Linq;
using LegendGenerator.App.View;
using System.Threading;
using LegendGenerator.App.Resources;

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
        private readonly IDataService _dataService;
        //fields for language settings
        private bool _chkEnglish;
        private bool _chkGerman;
        private Resource _resources = new Resource();


        #endregion

        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public MainViewModel(IDataService dataService)
        {
            _dataService = dataService;
          
            _dataService.GetData(
            callback: delegate (FormularData formData, Exception error)
            {
                if (error != null)
                {
                    // Report error here
                    //to do sett error message on property
                    return;
                }
                this.FormData = formData;
            });

            string language = System.Globalization.CultureInfo.CurrentCulture.Name;
            if (language.Contains("de") == true)
            {
                this.ChkGerman = true;
            }
            else
            {
                this.ChkEnglish = true;
            }

            //define viewmodel commands
            LoadProjectCommand = new RelayCommand(MnuAppLoad_Click);
            SaveProjectCommand = new RelayCommand(MnuAppSave_Click);
            SaveAsProjectCommand = new RelayCommand(MnuAppSaveNew_Click);
            LoadAccessDbCommand = new RelayCommand(BtnLoadAccessDb_Click);
            LoadAccessTablesCommand = new RelayCommand(BtnLoadAccessTables_Click);
            LoadSqlTablesCommand = new RelayCommand(BtnLoadSqlServerTables_Click);
            CloseCommand = new RelayCommand(Close);           
        }

        #region commands

        public RelayCommand LoadProjectCommand { get; private set; }
        public RelayCommand SaveProjectCommand { get; private set; }
        public RelayCommand SaveAsProjectCommand { get; private set; }
        public RelayCommand LoadAccessDbCommand { get; private set; }
        public RelayCommand LoadAccessTablesCommand { get; private set; }
        public RelayCommand LoadSqlTablesCommand { get; private set; }
        public RelayCommand CloseCommand { get; private set; }
     
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

        public Resource LocalizedText
        {
            get
            {
                return _resources;
            }
        }

        public bool ChkEnglish
        {
            get { return _chkEnglish; }
            set
            {
                _chkEnglish = value;
                base.RaisePropertyChanged("ChkEnglish");
                if (value == true)
                {
                    this.ChangeCulture("en-US");
                }

                this.CalculateDependentCheckBox(ref _chkGerman, "chkGerman", !value);

            }
        }
        public bool ChkGerman
        {
            get { return _chkGerman; }
            set
            {
                _chkGerman = value;
                base.RaisePropertyChanged("ChkGerman");
                if (value == true)
                {
                    this.ChangeCulture("de-AT");
                }
                this.CalculateDependentCheckBox(ref _chkEnglish, "ChkEnglish", !value);

            }
        }

        #region private helper methods

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

        private void BtnLoadAccessDb_Click()
        {
            OpenFileDialog oDlg = new OpenFileDialog
            {
                Title = "Select Access Database",
                Filter = "MDB (*.Mdb)|*.mdb|" +
                           "ACCDB (*.Accdb)|*.accdb"
            };
            //oDlg.RestoreDirectory = true;
            string dir = Environment.GetFolderPath(Environment.SpecialFolder.MyComputer);
            oDlg.InitialDirectory = dir;

            // Show open file dialog box
            Nullable<bool> result = oDlg.ShowDialog();

            // OK wurde gedrückt:
            if (result == true)
            {
                this.FormData.AccessDatabase = oDlg.FileName.ToString();
            }
        }

        private void BtnLoadAccessTables_Click()
        {
            if (File.Exists(this.FormData.AccessDatabase) == true)
            {
                IDataService controller = new DataService();
                List<string> tables = controller.GetTables(this.FormData.AccessDatabase);
                this.FormData.Tables = tables;
                //FormData.Tables.Clear();
                //tables.ForEach(this.FormData.Tables.Add);
                FormData.Table = tables.FirstOrDefault();
            }
            else
            {
                MessageBox.Show("Please define a correct Access database!", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnLoadSqlServerTables_Click()
        {
            WaitDialog wd = null;
            try
            {
                wd = new WaitDialog();
                wd.Show();//not modal
            }
            catch
            {
                MessageBox.Show("Please restart the about window!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                wd.Close();
            }

            try
            {
                IDataService controller = new DataService();
                List<string> tables = controller.GetSqlServerTables(this.FormData.SqlServer, this.FormData.ServerInstance, this.FormData.ServerDatenbank, 
                    this.FormData.ServerVersion, this.FormData.ServerUser, this.FormData.ServerPasswort);               
                this.FormData.Tables = tables;
                FormData.Table = tables.FirstOrDefault();
            }
            catch (Exception ex)
            {
                MessageBox.Show("The legend table wasn't defined or the session has been canceled! " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                //this.staLblMessage.Content = "The legend table wasn't defined or the session has been canceled!";
            }

            wd.Close();
        }

        private void Close()
        {            
            this.RaiseRequestClose(new FeedbackEventArgs(true, false));
        } 

        private void CalculateDependentCheckBox(ref bool otherCheckBox, string otherProperty, bool negatedCheckValue)
        {
            otherCheckBox = negatedCheckValue;
            base.RaisePropertyChanged(otherProperty);
        }

        private void ChangeCulture(string lang)
        {
            var culture = new System.Globalization.CultureInfo(lang);            
            Thread.CurrentThread.CurrentUICulture = culture;         
            this.RaisePropertyChanged("LocalizedText");
        }

        private string CreateFileDoesNotExistMsg()
        {
            return "The example XML file '" + _pfadKonfigurationsdatei + "' does not exist." + "\n\n" +
            "To create the example XML file, enter formular data details, then click the 'Save' button.";
        }

        #endregion

        #region events

        public event EventHandler<FeedbackEventArgs> RequestClose;
        /// <summary>
        /// Raised when the wizard should be removed from the UI.
        /// </summary>
        void RaiseRequestClose(FeedbackEventArgs e)
        {
            EventHandler<FeedbackEventArgs> handler = this.RequestClose;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        #endregion // Events            

    }
}