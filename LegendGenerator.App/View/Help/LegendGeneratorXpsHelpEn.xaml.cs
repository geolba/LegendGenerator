using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Xps.Packaging; //Required namespace 
using System.IO;
using System.IO.Packaging;
using System.Reflection;
using System.Windows.Controls.Primitives;       // DocumentPageView.
using System.Collections.ObjectModel; 

namespace LegendGenerator.App.View.Help
{
    /// <summary>
    /// Interaktionslogik für LegendGeneratorXpsHelpEn.xaml
    /// </summary>
    public partial class LegendGeneratorXpsHelpEn : UserControl
    {
       XpsDocument doc;

        public LegendGeneratorXpsHelpEn()
        {
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            // Getting a Stream out of the Resource file, strDocument
            string strDocument = "View.Help.legendgenerator_english.xps";
            string strSchemaPath = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name + "." + strDocument;
            Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(strSchemaPath);
            // Getting the length of the Stream we just obtained
            int length = (int)stream.Length;
            // Setting up a new MemoryStream and Byte Array
            MemoryStream ms = new MemoryStream();
            ms.Capacity = (int)length;
            byte[] buffer = new byte[length];
            // Copying the Stream to the Byte Array (Buffer)
            stream.Read(buffer, 0, length);
            // Copying the Byte Array (Buffer) to the MemoryStream
            ms.Write(buffer, 0, length);
            // Setting up a new Package based on the MemoryStream
            Package pkg = Package.Open(ms);
            // Putting together a Uri for the Package using the document name (strDocument)
            string strMemoryPackageName = string.Format("memorystream://{0}.xps", "legendgenerator_english.xps");
            Uri packageUri = new Uri(strMemoryPackageName);
            // Adding the Package to PackageStore using the Uri
            if (PackageStore.GetPackage(packageUri) == null)
            {
                PackageStore.AddPackage(packageUri, pkg);
            }
            // Finally, putting together the XpsDocument
           doc = new XpsDocument(pkg, CompressionOption.Maximum, strMemoryPackageName);
           // Feeding the DocumentViewer, which was declared at Design Time as a variable called "viewer"
           documentViewer1.Document = doc.GetFixedDocumentSequence();
           documentViewer1.FitToWidth();
           documentViewer1.FitToHeight(); 
        }

        private void btnFirst_Click(object sender, RoutedEventArgs e)
        {
            this.documentViewer1.FirstPage();
        }

        private void btnPrevious_Click(object sender, RoutedEventArgs e)
        {
            this.documentViewer1.PreviousPage();
        }

        private void btnNext_Click(object sender, RoutedEventArgs e)
        {
            this.documentViewer1.NextPage();
        }

        private void btnLast_Click(object sender, RoutedEventArgs e)
        {
            this.documentViewer1.LastPage();
        }

        private void txtPageNumber_TextChanged(object sender, TextChangedEventArgs e)
        {
            int iCount = this.documentViewer1.PageCount;

            int iPageNum = 0;
            if (txtPageNumber.Text.Length != 0)
            {
                try
                {
                    iPageNum = Convert.ToInt32(txtPageNumber.Text);
                    if (iPageNum < 1) iPageNum = 1;
                    if (iPageNum > iCount) iPageNum = iCount;
                    this.documentViewer1.GoToPage(iPageNum);    // 1-based !
                }
                catch (FormatException ex)
                {
                    MessageBox.Show(ex.Message, "Format Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void documentViewer1_PageViewsChanged(object sender, EventArgs e)
        {
            // Get the page number from the PageViews currently visible.
            ReadOnlyCollection<DocumentPageView> oP = documentViewer1.PageViews;
            if ((doc != null) && (oP.Count > 0))
            {
                DocumentPageView pv = oP[oP.Count - 1];     // Take last page.
                //pv.SizeChanged += new SizeChangedEventHandler(pv_SizeChanged);
                int iPageNum = pv.PageNumber + 1;           // 1-based.
                //BlockEvent = true;                          // For txtBoxPageNum_TextChanged().
                txtPageNumber.Text = iPageNum.ToString();
                //BlockEvent = false;
                // Show total number of pages.
                lblTotalPages.Content = "of " + documentViewer1.PageCount.ToString();
            }
        }
    }
}
