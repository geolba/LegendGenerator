using System;
using System.Resources;
using System.Collections;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Win32;
using ESRI.ArcGIS.Framework;
using ESRI.ArcGIS.ArcMapUI;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Display;
using System.IO;
using LegendGenerator.App.Resources;
using LegendGenerator.App.View.Help;
using LegendGenerator.App.Model;
using LegendGenerator.App.Utils;
using System.Windows.Threading;
using LegendGenerator.App.View;
using System.Linq;

namespace LegendGenerator.App
{   
    public partial class MainWindow : Window
    {
        #region class members

        private IApplication m_application;//um auf die Map zuzugreifen, wird  vom Command via Konstruktor initialisiert 
        private IMxDocument pMxDoc;
        private IActiveView pActiveView;
        private IMap pMap;
        
        private string language;
        private bool _isInitializing = false;
        private string pfadKonfigurationsdatei = String.Empty;//XML-Projektfile;

        private string pfadSymbolDirectory;
        private bool checkedGifExport;
        //Objekt zur Klasse mit den GIS-Funktionalitäten!!!
        LayoutCreator layoutCreator;
        protected AboutDialog aboutWindow;

        private FormularData _formData;

        #region esri event member variables:

        //private ESRI.ArcGIS.ArcMapUI.IDocumentEvents_NewDocumentEventHandler m_DocumentEventNewDocument = null;
        //private ESRI.ArcGIS.ArcMapUI.IDocumentEvents_OpenDocumentEventHandler m_DocumentEventOpenDocument = null;
        //private ESRI.ArcGIS.ArcMapUI.IDocumentEvents_Event m_docEvents = null;

        #endregion

        #endregion

        #region properties:

        public string PfadSymbolDirectory
        {
            get
            {
                return this.pfadSymbolDirectory;
            }
            set
            {
                this.pfadSymbolDirectory = value;
            }
        }

        public bool CheckedGifExport
        {
            get
            {
                return this.checkedGifExport;
            }
            set
            {
                this.checkedGifExport = value;
            }
        }

        /// <summary>
        /// Returns the cup of coffee ordered by the customer.
        /// If this returns null, the user cancelled the order.
        /// </summary>
        public FormularData FormData
        {
            get
            {
                return this._formData;
            }
            set
            {
                this. _formData = value;               
            }
        }

        #endregion
        
        #region constructor:

        public MainWindow()
        {
            this._isInitializing = true;
            InitializeComponent();
            this._isInitializing = false;
                       
            string path = Path.Combine(Environment.CurrentDirectory, @"Data\FormData.xml");           
            FormularData formData = ObjectXmlSerializer<FormularData>.Load(path);           
            this.FormData = formData;
            this.DataContext = this.FormData;
        }
        
        public MainWindow(IApplication application)
        {
            if (null == application)
            {
                throw new Exception("Hook helper is not initialized");
            }

            this._isInitializing = true;
            InitializeComponent();           
            this._isInitializing = false;

            m_application = application;//?? throw new Exception("Hook helper is not initialized");//for the communication with the ArcGIS-Application!         
            layoutCreator = new LayoutCreator(this, m_application);//um auf die GIS-Methoden zugreifen zu können

            //language settings: detect the windows language settings and load the correct language:
            language = System.Globalization.CultureInfo.CurrentCulture.Name;
            if (language.Contains("de") == true)
            {
                language = "de";
                this.chkSpracheDeutsch.IsChecked = true;
            }
            else
            {
                language = "en";
                this.chkSpracheEnglisch.IsChecked = true;
            }
            Resource.Culture = new System.Globalization.CultureInfo(language);
           
            string folder = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);         
            string path = Path.Combine(folder, @"Data\FormData.xml");           
            FormularData formData = ObjectXmlSerializer<FormularData>.Load(path);          
            this.FormData = formData;
            this.DataContext = this.FormData;
        }

        #endregion

        #region main GIS - methods (called through the layout creator class):

        private void BtnLegendTemplate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                layoutCreator.CreateTemplate();
            }
            catch
            {
                MessageBox.Show("A general error the template creating process is occured!", "Template error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                this.Cursor = System.Windows.Input.Cursors.Arrow;
            }
        }

        private void BtnCreateLegend_Click(object sender, RoutedEventArgs e)
        {
            if (this.FormData.IsValid() == true)
            {
                try
                {
                    layoutCreator.CreateLegend();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("A general error  during the legend creation is occured! " + ex.Message, "Legend creation error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    this.Cursor = System.Windows.Input.Cursors.Arrow;
                }
            }
            else
            {
                MessageBox.Show(this.FormData.Error);
            }
        }

        private void MnuStyleDump_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                layoutCreator.StyleDump();
            }
            catch
            {
                MessageBox.Show("A general error during the stryle dumping process is occured!", "Style Dump Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion

        #region windows wpf-window methods
       
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.staLblMessage.Content = String.Empty;
            this.pMxDoc = (IMxDocument)this.m_application.Document;
            this.pActiveView = pMxDoc.ActiveView;
            this.pMap = pActiveView.FocusMap;
            this.SetupEvents((IDocument)pMxDoc);
            //load the style file into the comboBox:
            this.UpdateCboStyleFile();
        }

        #region "Add Event Wiring for New/Open Documents"

        // Event member variables
        private ESRI.ArcGIS.ArcMapUI.IDocumentEvents_Event m_docEvents = null;
        private ESRI.ArcGIS.ArcMapUI.IDocumentEvents_NewDocumentEventHandler m_DocumentEventNewDocument = null;
        private ESRI.ArcGIS.ArcMapUI.IDocumentEvents_OpenDocumentEventHandler m_DocumentEventOpenDocument = null;

        /// <summary>
        /// Set up the wiring of the events.
        /// </summary>
        /// <param name="myDocument"></param>
        /// <remarks></remarks>
        private void SetupEvents(ESRI.ArcGIS.Framework.IDocument myDocument)
        {
            //parameter check:
            if (myDocument == null)
            {
                return;
            }
            //IDocumentEvents_Event docEvents = myDocument as ESRI.ArcGIS.ArcMapUI.IDocumentEvents_Event; 
            m_docEvents = myDocument as IDocumentEvents_Event;

            //// Create an instance of the delegate, add it to NewDocument event:           
            m_DocumentEventNewDocument = new IDocumentEvents_NewDocumentEventHandler(OnNewDocument);
            m_docEvents.NewDocument += m_DocumentEventNewDocument;

            //// Create an instance of the delegate, add it to OpenDocument event:             
            m_DocumentEventOpenDocument = new IDocumentEvents_OpenDocumentEventHandler(OnOpenDocument);
            m_docEvents.OpenDocument += m_DocumentEventOpenDocument;
        }
        
        /// <summary>
        /// SECTION: Remove the event handlers for all of the IActiveViewEvents
        /// </summary>
        /// <param name="map">An IMap interface for which Event Handlers to remove.</param>
        /// <remarks>You do not have to remove Event Handlers for every event. You can pick and 
        /// choose which ones you want to use.</remarks>
        private void RemoveSetUpDocumentEvents(ESRI.ArcGIS.Framework.IDocument myDocument)
        {
            //parameter check
            if (myDocument == null)
            {
                return;
            }
            //ESRI.ArcGIS.ArcMapUI.IDocumentEvents_Event docEvents = myDocument as ESRI.ArcGIS.ArcMapUI.IDocumentEvents_Event;
            m_docEvents = myDocument as IDocumentEvents_Event;

            // Remove ItemAdded and ItemDeletd and ItemReordered and ContentsChanged Event Handler
            m_docEvents.NewDocument -= m_DocumentEventNewDocument;
            m_docEvents.OpenDocument -= m_DocumentEventOpenDocument;
        }

        /// <summary>
        /// The NewDocument event handler. 
        /// </summary>
        /// <remarks></remarks>
        void OnNewDocument()
        {
            pMxDoc = m_docEvents as ESRI.ArcGIS.ArcMapUI.IMxDocument;
            pActiveView = pMxDoc.ActiveView;
            pMap = pActiveView.FocusMap;
            this.UpdateCboStyleFile();
        }

        /// <summary>
        /// The OpenDocument event handler.
        /// </summary>
        /// <remarks></remarks>
        void OnOpenDocument()
        {
            pMxDoc = m_docEvents as ESRI.ArcGIS.ArcMapUI.IMxDocument;
            pActiveView = pMxDoc.ActiveView;
            pMap = pActiveView.FocusMap;
            this.UpdateCboStyleFile();
        }

        #endregion

        private void BtnLoadAccessDb_Click(object sender, RoutedEventArgs e)
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
                this.txtAccessDatabase.Text = oDlg.FileName.ToString();                
            }   
        }       
              
        private void BtnLoadSqlServerTables_Click(object sender, RoutedEventArgs e)
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
                List<string> tables = controller.GetSqlServerTables(this.txtSqlServer.Text, this.txtInstance.Text, this.txtDatabase.Text, this.txtVersion.Text, this.txtUser.Text, this.txtPassword.Text);
                //FormData.Tables.Clear();
                //tables.ForEach(this.FormData.Tables.Add);
                //FormData.Table = tables.FirstOrDefault();
                this.FormData.Tables = tables;                
                FormData.Table = tables.FirstOrDefault();
            }
            catch (Exception ex)
            {
                MessageBox.Show("The legend table wasn't defined or the session has been canceled! " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                this.staLblMessage.Content = "The legend table wasn't defined or the session has been canceled!";
            }
           
            wd.Close();
        }

        private void MnuAppLoad_Click(object sender, RoutedEventArgs e)
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
                this.pfadKonfigurationsdatei = oXml.FileName.ToString();
            }
            // Load the form object from the existing XML file (if any)...
            if (File.Exists(this.pfadKonfigurationsdatei) == true)
            {
                // Load the form object from the XML file using our custom class...
                FormularData formdata = ObjectXmlSerializer<FormularData>.Load(this.pfadKonfigurationsdatei);

                if (formdata == null)
                {
                    MessageBox.Show("Unable to load a formular data object from file '" + this.pfadKonfigurationsdatei + "'!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else  // Load form properties into form data...
                {
                    //this.LoadCustomerIntoForm(formdata);
                    this.FormData.DeepCopy(formdata);               
                    this.Title = this.pfadKonfigurationsdatei;
                }
            }
            else
            {
                MessageBox.Show(this.CreateFileDoesNotExistMsg(), "Information", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void MnuAppSave_Click(object sender, RoutedEventArgs e)
        {
            if (File.Exists(this.pfadKonfigurationsdatei) == true)
            {
                // Create form object based on Form values.
                FormularData formdata = this.FormData;// this.CreateFormularData();

                //Save form object to XML file using our ObjectXMLSerializer class...
                try
                {
                    ObjectXmlSerializer<FormularData>.Save(formdata, this.pfadKonfigurationsdatei);                    
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Unable to save formular data object!" + Environment.NewLine + Environment.NewLine + ex.Message,
                        "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                this.MnuAppSaveNew_Click(sender, e);
            }       
            
        }

        //save as the projectfile:
        private void MnuAppSaveNew_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.SaveFileDialog sXml = new Microsoft.Win32.SaveFileDialog();
            string dir = Environment.GetFolderPath(Environment.SpecialFolder.MyComputer);
            sXml.InitialDirectory = dir;
            sXml.Title = "Select a folder for creating a new projectfile!";
            sXml.Filter = "XML (*.xml)|*.xml";

            // pressed OK:
            if (sXml.ShowDialog() == true)
            {
                this.pfadKonfigurationsdatei = sXml.FileName.ToString();                
            }

            //if (File.Exists(this.txtKonfigurationsdatei.Text))
            if (this.pfadKonfigurationsdatei != String.Empty)
            {
                // Create customer object based on Form values.
                FormularData formdata = this.FormData;//this.CreateFormularData();

                //Save form object to XML file using our ObjectXMLSerializer class...
                try
                {
                    ObjectXmlSerializer<FormularData>.Save(formdata, this.pfadKonfigurationsdatei);                                   
                    this.Title = "Legend Generator 5.0 für ArcMap 10.0 - " + this.pfadKonfigurationsdatei;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Unable to save formular data object!" + Environment.NewLine + Environment.NewLine + ex.Message,
                        "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

        }
              
        private void MnuAppClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
          
        private void ChkSpracheDeutsch_Checked(object sender, RoutedEventArgs e)
        {
            this.chkSpracheEnglisch.IsChecked = false;
            this.language = "de";

            System.Globalization.CultureInfo test = new System.Globalization.CultureInfo(language);
            Resource.Culture = new System.Globalization.CultureInfo(language);

            Window window = this;
            List<string> ControlNames = new List<string>();

            ResourceSet resourceSet = Resource.ResourceManager.GetResourceSet(test, true, true);
            foreach (DictionaryEntry entry in resourceSet)
            {
                string resourceKey = entry.Key.ToString();//Schlüssel = Name des Controls
                object resource = entry.Value;//=Content der labels
                ControlNames.Add(resourceKey.ToString());
            }

            foreach (Label lbl in TreeHelper.FindChildren<Label>(window))
            {
                if (ControlNames.Contains(lbl.Name))
                {
                    // do something with lbl here
                    string content = lbl.Name;
                    lbl.Content = Resource.ResourceManager.GetString(content, Resource.Culture);
                }

            }

            foreach (System.Windows.Controls.Button btn in TreeHelper.FindChildren<System.Windows.Controls.Button>(window))
            {
                if (ControlNames.Contains(btn.Name))
                {
                    // do something with btn here
                    string content = btn.Name;
                    btn.Content = Resource.ResourceManager.GetString(content, Resource.Culture);
                }
            }

            foreach (CheckBox chk in TreeHelper.FindChildren<CheckBox>(window))
            {
                if (ControlNames.Contains(chk.Name))
                {
                    // do something with chk here
                    string content = chk.Name;
                    chk.Content = Resource.ResourceManager.GetString(content, Resource.Culture);
                }
            }

            foreach (GroupBox grp in TreeHelper.FindChildren<GroupBox>(window))
            {
                if (ControlNames.Contains(grp.Name))
                {
                    // do something with grp here
                    string header = grp.Name;
                    grp.Header = Resource.ResourceManager.GetString(header, Resource.Culture);
                }
            }

            foreach (TabItem tabi in TreeHelper.FindChildren<TabItem>(window))
            {
                if (ControlNames.Contains(tabi.Name))
                {
                    // do something with mni here
                    string header = tabi.Name;
                    tabi.Header = Resource.ResourceManager.GetString(header, Resource.Culture);
                }
            }

            foreach (MenuItem mni in TreeHelper.FindChildren<MenuItem>(window))
            {
                if (ControlNames.Contains(mni.Name))
                {
                    // do something with mni here
                    string header = mni.Name;
                    mni.Header = Resource.ResourceManager.GetString(header, Resource.Culture);
                }
            }
        }

        private void ChkSpracheDeutsch_Unchecked(object sender, RoutedEventArgs e)
        {
            this.chkSpracheEnglisch.IsChecked = true;
        }

        private void ChkSpracheEnglisch_Checked(object sender, RoutedEventArgs e)
        {

            this.chkSpracheDeutsch.IsChecked = false;
            this.language = "en";
            System.Globalization.CultureInfo test = new System.Globalization.CultureInfo(language);
            Resource.Culture = new System.Globalization.CultureInfo(language);

            Window window = this;

            List<string> ControlNames = new List<string>();

            ResourceSet resourceSet = Resource.ResourceManager.GetResourceSet(test, true, true);
            foreach (DictionaryEntry entry in resourceSet)
            {
                string resourceKey = entry.Key.ToString();//Schlüssel = Name des Controls
                object resource = entry.Value;//=Content der labels
                ControlNames.Add(resourceKey.ToString());


            }

            foreach (Label lbl in TreeHelper.FindChildren<Label>(window))
            {
                if (ControlNames.Contains(lbl.Name))
                {
                    // do something with lbl here
                    string content = lbl.Name;
                    lbl.Content = Resource.ResourceManager.GetString(content, Resource.Culture);
                }

            }

            foreach (System.Windows.Controls.Button btn in TreeHelper.FindChildren<System.Windows.Controls.Button>(window))
            {
                if (ControlNames.Contains(btn.Name))
                {
                    // do something with btn here
                    string content = btn.Name;
                    btn.Content = Resource.ResourceManager.GetString(content, Resource.Culture);
                }
            }

            foreach (CheckBox chk in TreeHelper.FindChildren<CheckBox>(window))
            {
                if (ControlNames.Contains(chk.Name))
                {
                    // do something with chk here
                    string content = chk.Name;
                    chk.Content = Resource.ResourceManager.GetString(content, Resource.Culture);
                }
            }

            foreach (GroupBox grp in TreeHelper.FindChildren<GroupBox>(window))
            {
                if (ControlNames.Contains(grp.Name))
                {
                    // do something with grp here
                    string header = grp.Name;
                    grp.Header = Resource.ResourceManager.GetString(header, Resource.Culture);
                }
            }

            foreach (TabItem tabi in TreeHelper.FindChildren<TabItem>(window))
            {
                if (ControlNames.Contains(tabi.Name))
                {
                    // do something with mni here
                    string header = tabi.Name;
                    tabi.Header = Resource.ResourceManager.GetString(header, Resource.Culture);
                }
            }

            foreach (MenuItem mni in TreeHelper.FindChildren<MenuItem>(window))
            {
                if (ControlNames.Contains(mni.Name))
                {
                    // do something with mni here
                    string header = mni.Name;
                    mni.Header = Resource.ResourceManager.GetString(header, Resource.Culture);
                }
            }
        }

        private void ChkSpracheEnglisch_Unchecked(object sender, RoutedEventArgs e)
        {
            this.chkSpracheDeutsch.IsChecked = true;
        }
            
        private void MnuAppHelp_Click(object sender, RoutedEventArgs e)
        {
            LegendGeneratorHelp lgh = null;
            try
            {
                lgh = new LegendGeneratorHelp
                {
                    //lgh.ShowDialog();//modal, aber bei der Hilfe nicht erwünscht!             
                    Owner = this
                };
                lgh.Show();
            }
            catch
            {
                MessageBox.Show("Please restart the help window!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                lgh.Close();
            }
        }

        private void MnuAppAbout_Click(object sender, RoutedEventArgs e)
        {
            DispatcherOperation op1 = Dispatcher.BeginInvoke(
                 DispatcherPriority.Normal,
                 new Action<bool>(SetWindowEnabled), false);

            try
            {
                aboutWindow = new AboutDialog();
                aboutWindow.Closed += new EventHandler(AboutWindow_Closed);
                aboutWindow.ShowDialog();
            }
            catch
            {
                MessageBox.Show("Please restart the about window!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                aboutWindow.Close();
            }
        }

        void AboutWindow_Closed(object sender, EventArgs e)
        {
            //SetWindowEnabled(true);
            DispatcherOperation op1 = Dispatcher.BeginInvoke(
                DispatcherPriority.Normal,
                new Action<bool>(SetWindowEnabled),
                true);
        }

        private void SetWindowEnabled(bool enabled)
        {
            this.IsEnabled = enabled;
        }

        private void MnuLoadStylefile_Click(object sender, RoutedEventArgs e)
        {
            DispatcherOperation op1 = Dispatcher.BeginInvoke(
                DispatcherPriority.Normal,
                new Action<bool>(SetWindowEnabled), false);

            StylefileDialog stf = null;
            try
            {
                stf = new StylefileDialog(this);
                stf.Closed += new EventHandler(AboutWindow_Closed);
                stf.Topmost = true;
                //ab.ShowDialog();
                stf.ShowDialog();//modal
                if (stf.BtnClicked == true && File.Exists(stf.txtStyleFilePath.Text))
                {
                    string filename = stf.txtStyleFilePath.Text;
                    IStyleGallery pStyleGallery;
                    IStyleGalleryStorage pStyleGalleryStorage;
                    pStyleGallery = pMxDoc.StyleGallery;
                    pStyleGalleryStorage = (IStyleGalleryStorage)pStyleGallery;
                    pStyleGalleryStorage.AddFile(filename);
                    //refill the ComboBox for the Style files!!!
                    UpdateCboStyleFile(filename);
                }
            }
            catch
            {
                MessageBox.Show("Please restart the about window!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                stf.Close();
            }
        }

        private void MnuResetPage_Click(object sender, RoutedEventArgs e)
        {
            IActiveView pActiveView = pMxDoc.ActiveView;
            IPageLayout pPageLayout = pMxDoc.PageLayout;
            IGraphicsContainer pGraphicsContainer = (IGraphicsContainer)pPageLayout;
            //pGraphicsContainer.DeleteAllElements();
            if (layoutCreator.LegendGroupElement != null)
            {
                pGraphicsContainer.DeleteElement((IElement)layoutCreator.LegendGroupElement);
            }

            pMxDoc.UpdateContents();
            pMxDoc.ActiveView.Refresh();
            // Rfresh the map
            pActiveView.Refresh();
            //this.Activate();
        }

        private void MnuFortlaufendeNummer_Click(object sender, RoutedEventArgs e)
        {
            FortlaufendeNrDialog fnd = null;
            try
            {
                fnd = new FortlaufendeNrDialog(this);
                //ab.ShowDialog();
                fnd.ShowDialog();//modal
                if (fnd.BtnClicked == true)
                {
                    MessageBox.Show("The legend entries will be numbered sequentially in the defined textfile!", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch
            {
                MessageBox.Show("Please restart the about window!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                fnd.Close();
            }
        }

        private void MnuGraphicExport_Click(object sender, RoutedEventArgs e)
        {
            SymbolDialog sd = null;
            try
            {
                sd = new SymbolDialog(this);
                //ab.ShowDialog();
                sd.ShowDialog();//modal
                if (sd.BtnClicked == true)
                {
                    MessageBox.Show("The images will be exported during the legend-creating process in the defined folder!", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch
            {
                MessageBox.Show("Please restart the about window!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                sd.Close();
            }
        }

        private void BtnFortlaufendeNummer_Click(object sender, RoutedEventArgs e)
        {
            //Öffnen eines File Dialogs zur Auswahl der Konfigurationsdatei:
            OpenFileDialog oTxt = new OpenFileDialog
            {
                Title = "Select TXT-FILE fo the lookup-table",
                Filter = "TXT (*.txt)|*.txt"
            };
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

        private void BtnLoadAccessTables_Click(object sender, RoutedEventArgs e)
        {
            if (File.Exists(this.txtAccessDatabase.Text) == true)
            {                
                IDataService controller = new DataService();
                List<string> tables = controller.GetTables(this.txtAccessDatabase.Text);               
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
               
        private void InfoImage_MouseEnter(object sender, MouseEventArgs e)
        {
            Mouse.OverrideCursor = Cursors.Help;
            PersonInfoPopup.IsOpen = !PersonInfoPopup.IsOpen;
        }

        private void InfoImage_MouseLeave(object sender, MouseEventArgs e)
        {
            Mouse.OverrideCursor = Cursors.Arrow;
            PersonInfoPopup.IsOpen = !PersonInfoPopup.IsOpen;
        }

        private void InfoPrimImage_MouseEnter(object sender, MouseEventArgs e)
        {
            Mouse.OverrideCursor = Cursors.Help;
            PrimärschlüsselInfoPopup.IsOpen = !PrimärschlüsselInfoPopup.IsOpen;
        }

        private void InfoPrimImage_MouseLeave(object sender, MouseEventArgs e)
        {
            Mouse.OverrideCursor = Cursors.Arrow;
            PrimärschlüsselInfoPopup.IsOpen = !PrimärschlüsselInfoPopup.IsOpen;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // //save the static variables for a new re-opening of the window:
            //this.SaveSettings();
            //removing all esri-events:
            RemoveSetUpDocumentEvents((IDocument)pMxDoc);
        }

        private void InfoSortImage_MouseEnter(object sender, MouseEventArgs e)
        {
            Mouse.OverrideCursor = Cursors.Help;
            SortColumnInfoPopup.IsOpen = !SortColumnInfoPopup.IsOpen;
        }

        private void InfoSortImage_MouseLeave(object sender, MouseEventArgs e)
        {
            Mouse.OverrideCursor = Cursors.Arrow;
            SortColumnInfoPopup.IsOpen = !SortColumnInfoPopup.IsOpen;
        }
        
        #endregion

        #region private form helper methods
        
        private string CreateFileDoesNotExistMsg()
        {
            return "The example XML file '" + pfadKonfigurationsdatei + "' does not exist." + "\n\n" +
            "To create the example XML file, enter formular data details, then click the 'Save' button.";
        }
           
        private void UpdateCboStyleFile()
        {
            cboStyleFile.Items.Clear();
            //Styles in die Combobox laden:
            IStyleGallery pStyleGallery;
            IStyleGalleryStorage pStyleGalleryStorage;
            pStyleGallery = pMxDoc.StyleGallery;
            pStyleGalleryStorage = (IStyleGalleryStorage)pStyleGallery;
            for (int i = 0; i < pStyleGalleryStorage.FileCount; i++)
            {
                cboStyleFile.Items.Add(pStyleGalleryStorage.get_File(i));                
            }
            cboStyleFile.SelectedIndex = 0;
        }

        private void UpdateCboStyleFile(string fileName)
        {
            cboStyleFile.Items.Clear();
            //Styles in die Combobox laden:
            IStyleGallery pStyleGallery;
            IStyleGalleryStorage pStyleGalleryStorage;
            pStyleGallery = pMxDoc.StyleGallery;
            pStyleGalleryStorage = (IStyleGalleryStorage)pStyleGallery;
            for (int i = 0; i < pStyleGalleryStorage.FileCount; i++)
            {
                cboStyleFile.Items.Add(pStyleGalleryStorage.get_File(i));
            }
            cboStyleFile.SelectedIndex = cboStyleFile.Items.IndexOf(fileName);
        }

        #endregion                
    }
}