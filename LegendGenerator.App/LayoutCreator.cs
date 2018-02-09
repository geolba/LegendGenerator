using System;
using System.Windows;
using System.IO;
using ESRI.ArcGIS.Framework;
using ESRI.ArcGIS.ArcMapUI;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Output;
using System.Collections.Generic;
using System.Linq;
using LegendGenerator.App.Model;
using LegendGenerator.App.Utils;
using LegendGenerator.App.ViewModel;

namespace LegendGenerator.App
{ 
    class LayoutCreator
    {
        #region class members   

        public string test;
        private IApplication m_application;//for having the access to the ESRI-map!! - wird  vom Command via Konstruktor initialisiert 
        private IMxDocument pMxDoc;
        MainWindow pLegendGeneratorForm;//for the acces to the formular data
        private int zeilen = 0;

        private IGroupElement pLegendGroupElement;

        //our progress dialog window
        LegendGenerator.App.View.ProgressDialog pd;  
        #endregion

        #region properties

        //property for the private class member pGroupElement:
        public IGroupElement LegendGroupElement
        {
            get
            {
                return this.pLegendGroupElement;
            }
            set
            {
                this.pLegendGroupElement = value;
            }
        }

        #endregion

        #region constructor

        public LayoutCreator(MainWindow legendGeneratorForm, IApplication application)
        {
            if (null == application)
            {
                throw new Exception("Hook helper is not initialized");
            }

            this.pLegendGeneratorForm = legendGeneratorForm;
            this.m_application = application; //?? throw new Exception("Hook helper is not initialize");
           this.pMxDoc = (IMxDocument)m_application.Document;
           this.pLegendGeneratorForm.Activate();
        }

        #endregion

        //first method: create the legend template:
        public void CreateTemplate ( )
        {         
           string styleSet = String.Empty;
           IStyleGallery pStyGall = pMxDoc.StyleGallery;         
           IStyleGalleryStorage pStyleGalleryStorage = (IStyleGalleryStorage)pStyGall;       
            for (int i = 0; i < pStyleGalleryStorage.FileCount; i++)
            {               
                if (pStyleGalleryStorage.get_File(i).Equals(pLegendGeneratorForm.cboStyleFile.Text))
                {                   
                    styleSet = pStyleGalleryStorage.get_File(i);
                    break;//leaf the for loop!
                }
            }
            if (styleSet == String.Empty)
            {
                MessageBox.Show("For creating the template a correct style file must be loaded!",
                    "Error in the style-finding-process", MessageBoxButton.OK, MessageBoxImage.Error);
                //Output status:
                pLegendGeneratorForm.staLblMessage.Content = "For creating the template a correct style file must be loaded!";
                return;
            }
            
            //Methodenvariablen:
            IActiveView pActiveView = pMxDoc.ActiveView;
            IPageLayout pPageLayout = pMxDoc.PageLayout;
            //pPageLayout.Page.Units = pPageLayout.Page.Units = esriUnits.esriCentimeters;
            IElement pElement;
            string[] legtexte = new string[7];

            ILineElement pLineElm;
            ILineSymbol pLineSym;
            IPolyline pPlyLn;
            IMarkerElement pMrkElm;
            IMarkerSymbol pMarkerSym = null;
            IFillSymbol pFillSym;
            ISymbol pSymbol;
            IFillShapeElement pFillElm;
            IPoint pPnt;
            IPoint fromPt;
            IPoint toPt;
            
            IEnumStyleGalleryItem pEnStyGall;
            //IStyleGallery pStyGall;
            IStyleGalleryItem pStyItem;
            IGraphicsContainer pGraphicsContainer = (IGraphicsContainer) pPageLayout;
            IGeometry pGeometry;
            IEnvelope pFillEnv = null;
            WKSEnvelope nPatch;
            //double x_start = pPageLayout.Page.PrintableBounds.XMax -9;
            //double y_start = pPageLayout.Page.PrintableBounds.YMax +2;
            double x_start = pPageLayout.Page.PrintableBounds.XMax + 2;
            double y_start = pPageLayout.Page.PrintableBounds.YMax + 2;

            //Initialize the style gallery legendbox:          
            pEnStyGall = pStyGall.get_Items("Fill Symbols", styleSet, "layout");//cboStyleFile.Text
            pEnStyGall.Reset(); //resets the enumerator
            pStyItem = pEnStyGall.Next();
            //
            while (pStyItem != null && pStyItem.Name != "legendbox")
            {
                pStyItem = pEnStyGall.Next();//nächster Iterationsschritt!!
            }

            if (pStyItem != null && pStyItem.Name == "legendbox")
            {
                //pFillEnv = new EnvelopeClass();
                pFillEnv = new Envelope() as IEnvelope;
            }
            else
            {
                //iterate alternative stylefiles:
                for (int m = 0; m < pStyleGalleryStorage.FileCount; m++)
                {
                    //MessageBox.Show(pStyleGalleryStorage.get_File(i));
                    styleSet = pStyleGalleryStorage.get_File(m);
                    pEnStyGall = pStyGall.get_Items("Fill Symbols", styleSet, "layout");//cboStyleFile.Text
                    pEnStyGall.Reset(); //resets the enumerator
                    pStyItem = pEnStyGall.Next();

                    //solange bis der Mustername im Style File gefunden wurde:
                    while (pStyItem != null && pStyItem.Name != "legendbox")//bricht ja dann mal ab, fall der Mustername nicht existiert
                    {
                        pStyItem = pEnStyGall.Next();//nächster Iterationsschritt!!
                    }
                    if (pStyItem != null && pStyItem.Name == "legendbox")
                    {
                        //pFillEnv = new EnvelopeClass();
                        pFillEnv = new Envelope() as IEnvelope;
                        break;//for-Schleife wird beendet, falls die Namen überein stimmen!
                    }

                }//for-Schleife zu - wo durch die alternativen Stylefiles durch iteriert wird!!!
                if (pStyItem == null) // && pStyItem.Name != "legendbox")
                {
                   MessageBox.Show("Also the alternative stylefiles don't contain the FillSymbol 'legendbox'! " +
                                       "Please define a correct stylefile! ", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                   pLegendGeneratorForm.Cursor = System.Windows.Input.Cursors.Arrow;
                   //exit the application
                   return;
                }
            }
           
            nPatch.XMin = x_start;
            nPatch.YMax = y_start + 0.55;
            nPatch.XMax = x_start + 1.1;
            nPatch.YMin = y_start;
            pFillEnv.PutCoords(nPatch.XMin, nPatch.YMin, nPatch.XMax, nPatch.YMax);
            pGeometry = pFillEnv;
            pFillSym = (IFillSymbol)pStyItem.Item;
            //Draw legendbox:
            pSymbol = (ISymbol)pFillSym;
            //pFillElm = new RectangleElementClass();
            pFillElm = new RectangleElement() as IFillShapeElement;
            pFillElm.Symbol = (IFillSymbol)pSymbol;
            pElement = (IElement)pFillElm;
            pElement.Geometry = pGeometry;
            pGraphicsContainer.AddElement(pElement, 0);
                
            ////initialize the style gallery marker
            pEnStyGall = pStyGall.get_Items("Marker Symbols", styleSet, "layout");//cboStyleFile.Text
            pEnStyGall.Reset(); //resets the enumerator
            pStyItem = pEnStyGall.Next();
            while (pStyItem != null && pStyItem.Name != "legendmarker")
            {
                pStyItem = pEnStyGall.Next();//nächster Iterationsschritt!!
            }

            if (pStyItem != null)
            {
                pMarkerSym = (IMarkerSymbol)pStyItem.Item;
            }
            else
            {
                MessageBox.Show("The defined stylefile doesn't contain the MarkerSymbol 'legendmarker'! " +
                                       "Please define a correct stylefile! ", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                pLegendGeneratorForm.Cursor = System.Windows.Input.Cursors.Arrow;
                //exit the application
                return;
            }

            pPnt = new ESRI.ArcGIS.Geometry.Point();
            pPnt.PutCoords(x_start + 0.55, y_start + 0.27);
            pGeometry = pPnt;          
            //draw the point
            pSymbol = (ISymbol) pMarkerSym;
            //pMrkElm = new MarkerElementClass();
            pMrkElm = new MarkerElement() as IMarkerElement;
            pMrkElm.Symbol = (IMarkerSymbol) pSymbol;
            pElement = (IElement)pMrkElm;
            pElement.Geometry = pGeometry;
            pGraphicsContainer.AddElement(pElement, 0);

            //initialize the style gallery line:
            pEnStyGall = pStyGall.get_Items("Line Symbols", styleSet, "layout");//cboStyleFile.Text
            pEnStyGall.Reset(); //resets the enumerator
            pStyItem = pEnStyGall.Next();
            while (pStyItem != null && pStyItem.Name != "legendline")
            {
                pStyItem = pEnStyGall.Next();//nächster Iterationsschritt!!
            }
            if (pStyItem != null)
            {
                pLineSym = (ILineSymbol)pStyItem.Item;
            }
            else
            {
                MessageBox.Show("The defined stylefile doesn't contain the LineSymbol 'legendline'! " +
                                       "Please define a correct stylefile! ", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                pLegendGeneratorForm.Cursor = System.Windows.Input.Cursors.Arrow;
                //exit the application
                return;
            }
            pPlyLn = new Polyline() as IPolyline;   
            fromPt = new ESRI.ArcGIS.Geometry.Point();
            toPt = new ESRI.ArcGIS.Geometry.Point();
            fromPt.PutCoords(x_start + 0.1, y_start + 0.07);
            toPt.PutCoords(x_start + 1, y_start + 0.48);
            pPlyLn.FromPoint = fromPt;
            pPlyLn.ToPoint = toPt;
            pGeometry = pPlyLn;    
            pSymbol = (ISymbol)pLineSym;          
            pLineElm = new LineElement() as ILineElement;
            pLineElm.Symbol = (ILineSymbol) pSymbol;
            pElement = (IElement) pLineElm;
            pElement.Geometry = pGeometry;
            pGraphicsContainer.AddElement(pElement, 0);

            //initialize the brackets:
            IPointCollection pPolyline2D;
            IPoint pPoint1, pPoint2, pPoint3, pPoint4;
            pEnStyGall = pStyGall.get_Items("Line Symbols", styleSet, "layout");//cboStyleFile.Text
            pEnStyGall.Reset(); //resets the enumerator
            pStyItem = pEnStyGall.Next();
            while (pStyItem != null && pStyItem.Name != "bracketline")
            {
                pStyItem = pEnStyGall.Next();//nächster Iterationsschritt!!
            }
            if (pStyItem != null)
            {
                pLineSym = (ILineSymbol)pStyItem.Item;
            }
            else
            {
                MessageBox.Show("The defined stylefile doesn't contain the LineSymbol 'legendline'! " +
                                       "Please define a correct stylefile! ", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                pLegendGeneratorForm.Cursor = System.Windows.Input.Cursors.Arrow;
                //exit the application:
                return;
            }           
            pPolyline2D = new Polyline();
            pPoint1 = new ESRI.ArcGIS.Geometry.Point(); 
            pPoint2 = new ESRI.ArcGIS.Geometry.Point();
            pPoint3 = new ESRI.ArcGIS.Geometry.Point();
            pPoint4 = new ESRI.ArcGIS.Geometry.Point();
            pPoint1.PutCoords(x_start + 7.5, y_start);
            pPoint2.PutCoords(x_start + 7.7, y_start);
            pPoint3.PutCoords(x_start + 7.7, y_start + 0.55);
            pPoint4.PutCoords(x_start + 7.5, y_start + 0.55);
            pPolyline2D.AddPoint(pPoint1);
            pPolyline2D.AddPoint(pPoint2);
            pPolyline2D.AddPoint(pPoint3);
            pPolyline2D.AddPoint(pPoint4);
            pGeometry = (IGeometry) pPolyline2D; 
            //draw the bracket:
            pSymbol = (ISymbol) pLineSym;
            //pLineElm = new LineElementClass();
            pLineElm = new LineElement() as ILineElement;
            pLineElm.Symbol = (ILineSymbol)pSymbol;
            pElement = (IElement)pLineElm;
            pElement.Geometry = pGeometry;
            pGraphicsContainer.AddElement(pElement, 0);

            //headings:
            ISimpleTextSymbol tmpTxtSym;
            ITextElement pTxtElm;
            double x_textoff, y_textoff;
            ITransform2D pTrans2d;
            legtexte[0] = "heading1";
            legtexte[1] ="heading2";
            legtexte[2] = "heading3";
            legtexte[3] ="label";
            legtexte[4] = "legend text";
            legtexte[5] = "bracket";
            legtexte[6] = "graphics";
            for (int i = 0; i < legtexte.Length; i++)
            {
                pEnStyGall = pStyGall.get_Items("Text Symbols", styleSet, "layout");//cboStyleFile.Text
                pEnStyGall.Reset(); //resets the enumerator
                pStyItem = pEnStyGall.Next();
                while (pStyItem != null && pStyItem.Name != legtexte[i])
                {
                    pStyItem = pEnStyGall.Next();//nächster Iterationsschritt!!
                }
                if (pStyItem != null)
                {
                    //tmpTxtSym = new TextSymbolClass();
                    tmpTxtSym = (ISimpleTextSymbol)pStyItem.Item;
                }
                else
                {
                    MessageBox.Show("The defined stylefile doesn't contain the TextSymbol! " +
                                           "Please define a correct stylefile! ", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    pLegendGeneratorForm.Cursor = System.Windows.Input.Cursors.Arrow;
                    //exit the application:
                    return;
                }           
                pSymbol = (ISymbol) tmpTxtSym;
                //pTxtElm = new TextElementClass();
                pTxtElm = new TextElement() as ITextElement;
                pTxtElm.Text = legtexte[i];
                pTxtElm.Symbol = (ITextSymbol) pSymbol;
                pTxtElm.ScaleText = false;
                pElement = (IElement) pTxtElm;
                //pPnt = new PointClass();
                pPnt = new ESRI.ArcGIS.Geometry.Point();

                if (legtexte[i] == "label")
                {
                    x_textoff = 1;
                    y_textoff = 0.1;
                }
                else if (legtexte[i] == "legendtext")
                {
                    x_textoff = 1.44;
                    y_textoff = 0.2;
                }
                else if (legtexte[i] == "bracket")
                {
                    x_textoff = 8.04;
                    y_textoff = 0.27;
                }
                else if (legtexte[i] == "graphics")
                {
                    x_textoff = 0.55;
                    y_textoff = 0.05;
                }
                else
                {
                    x_textoff = 0;
                    y_textoff = 0.2;
                }

                pPnt.PutCoords(x_start + x_textoff, y_start + y_textoff);
                pElement = (IElement) pTxtElm;
                pGeometry = pPnt;
                pElement.Geometry = pGeometry;

                if (legtexte[i] == "bracket")
                {
                    pTrans2d = (ITransform2D) pElement;
                    pTrans2d.Rotate(pPnt, 1.57);
                    pGraphicsContainer.AddElement((IElement) pTrans2d, 0);
                }
                else
                {
                    pGraphicsContainer.AddElement(pElement, 0);
                }

            }

            //refresh everything:            
            pMxDoc.UpdateContents();         
            // Rfresh the map
            pActiveView.Refresh();

            MessageBox.Show("A new legend template was created - right on top of the layout page!", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
            //Ausgabe an die Statuszeile: Statuszeile ausnahmsweise public in der Klasse LegendGeneratorForm:
            pLegendGeneratorForm.staLblMessage.Content = "A new legend template was created - right on top of the page";
        }
             
        //second method: cretae the legend
        public void CreateLegend()
        {
            //show the wait cursor:         
            pLegendGeneratorForm.Cursor = System.Windows.Input.Cursors.Wait;

            //createTemplate();

            //zuerst wird überprüft ob der Geolba Style geladen ist: ansonsten wird abgebrochen!
            string styleSet = String.Empty;
            IStyleGallery pStyGall = pMxDoc.StyleGallery;
            //IStyleGalleryStorage StyleGalleryStorage;
            IStyleGalleryStorage pStyleGalleryStorage = (IStyleGalleryStorage)pStyGall;
            //cboStyleFile.Items.Clear();
            for (int i = 0; i < pStyleGalleryStorage.FileCount; i++)
            {
                //cboStyleFile.Items.Add(StyleGalleryStorage.get_File(i));
                //if (pStyleGalleryStorage.get_File(i).Contains(pLegendGeneratorForm.txtStyleManually.Text))
                if (pStyleGalleryStorage.get_File(i).Contains(pLegendGeneratorForm.cboStyleFile.Text))
                {
                    //MessageBox.Show(pStyleGalleryStorage.get_File(i));
                    styleSet = pStyleGalleryStorage.get_File(i);
                    break;// und anschließend wird die for-Schleife verlassen
                }
            }
            if (styleSet == String.Empty)//ein vordefiniertes Stylefile muss immer angegeben werden!!!
            {
                MessageBox.Show("For creating the legend a correct style file must be loaded!",
                   "Error in the style-finding-process", MessageBoxButton.OK, MessageBoxImage.Error);
                //Ausgabe an die Statuszeile:
                pLegendGeneratorForm.staLblMessage.Content = "For creating the legend a correct style file must be loaded!";
                //pLegendGeneratorForm.Cursor = Cursors.Default;
                pLegendGeneratorForm.Cursor = System.Windows.Input.Cursors.Arrow;

                return;
            }

            //Methodenvariablen:
            IActiveView pActiveView = pMxDoc.ActiveView;
            IPageLayout pPageLayout = pMxDoc.PageLayout;
          
            //Feldnamen in der Legendentabelle belegen:
            string grafik_feld, bez_feld, us1_feld, us2_feld, us3_feld, us4_feld, us5_feld, us6_feld,
                kl1_feld, kl2_feld, kl3_feld, gruppe_feld, leg_sort_feld, leglab_feld, notes_feld,
                fillsymbol_feld = String.Empty, linesymbol_feld = String.Empty, markersymbol_feld = String.Empty, geometrie_feld = String.Empty;
            string legendentabelle, leg_tab_pfad, leg_ID_feld;
            //Variablen für den Gif-Export:
            //'***********************************************************************************
            //'Legenden Kästechen als GIF exportieren
            double xmin_GE = 0;
            double ymin_GE = 0;
            double xmax_GE = 0;
            double ymax_GE = 0;
            //IExport pExport = new ExportGIFClass();
            IExport pExport = new ExportGIF() as IExport;
            pExport.Resolution = 300;
            IEnvelope pPixelBoundsEnv;
            tagRECT exportRect;
            long hDC;
            int iWidthPixels = 130, iHeightPixels = 71;
            IEnvelope pExportEnv;
            string pExportFileDir = String.Empty;
            if (pLegendGeneratorForm.FormData.ChkGraphicExport == true)
            {
                if (pLegendGeneratorForm.FormData.GraphicExportDirectory != String.Empty && Directory.Exists(pLegendGeneratorForm.FormData.GraphicExportDirectory) == true)
                {
                    pExportFileDir = pLegendGeneratorForm.FormData.GraphicExportDirectory;
                }
                else
                {
                    MessageBox.Show("No correct export folder was defined for the symbol export (gif)!", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            //Variablen für den Gif-Export- Ende:
            //'***********************************************************************************
            try
            {
                legendentabelle = pLegendGeneratorForm.cboTable.Text;
                leg_tab_pfad = pLegendGeneratorForm.txtAccessDatabase.Text;
                leg_ID_feld = pLegendGeneratorForm.txtVerknuepfung.Text;
                grafik_feld = pLegendGeneratorForm.txtLegendengrafik.Text;
                bez_feld = pLegendGeneratorForm.txtLegendentext.Text;
                us1_feld = pLegendGeneratorForm.txtUeberschrift1.Text;
                us2_feld = pLegendGeneratorForm.txtUeberschrift2.Text;
                us3_feld = pLegendGeneratorForm.txtUeberschrift3.Text;
                us4_feld = pLegendGeneratorForm.txtInfozeile4.Text;
                us5_feld = pLegendGeneratorForm.txtInfozeile5.Text;
                us6_feld = pLegendGeneratorForm.txtInfozeile6.Text;
                kl1_feld = pLegendGeneratorForm.txtKlammerebene1.Text;
                kl2_feld = pLegendGeneratorForm.txtKlammerebene2.Text;
                kl3_feld = pLegendGeneratorForm.txtKlammerebene3.Text;
                gruppe_feld = pLegendGeneratorForm.txtGruppierung.Text;
                leg_sort_feld = pLegendGeneratorForm.txtSortierung.Text;
                //leglab_feld = this.txtLegendentext.Text;
                leglab_feld = pLegendGeneratorForm.txtLegendennummer.Text;
                notes_feld = pLegendGeneratorForm.txtZweiteSpalte.Text;
                fillsymbol_feld = pLegendGeneratorForm.txtFlaeche.Text;
                linesymbol_feld = pLegendGeneratorForm.txtLinie.Text;
                markersymbol_feld = pLegendGeneratorForm.txtMarker.Text;
                geometrie_feld = pLegendGeneratorForm.txtZeichenelemente.Text;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                pLegendGeneratorForm.staLblMessage.Content = ex.Message;
                pLegendGeneratorForm.Cursor = System.Windows.Input.Cursors.Arrow;
                return;
            }

            //check if values for the database-connection are defined:
            bool Fe = CheckFehler(legendentabelle, leg_tab_pfad);
            if (Fe == true)
            {
                pLegendGeneratorForm.Cursor = System.Windows.Input.Cursors.Arrow;
                return;
            }


            //now the legend will be created: 
            string Fehlertext = String.Empty;
            //IGraphicsContainer pGraphContainer = null;
            IGraphicsContainer pGraphContainer = (IGraphicsContainer)pPageLayout;
            IElement pElement;
            IFields pFlds;
            IField pFld;
            IEnvelope pEnvelope;
            IFeatureLayerDefinition flDef;
            ICmykColor pCmykColor = new CmykColor();

            ///////////////////////////////////////////////////////////////////////////////////////////////////
            //////////////////////////////////definitions for the viewable area - Definitionen für die Analyse des sichtbaren Bereiches:
            IMap pMap;
            IMapFrame pMapFrame;
            IGeoFeatureLayer pGFLayer;
            ISpatialFilter pSpatialFilter;
            IFeatureClass pFeatcls;
            IFeatureLayer pFeatLayer;
            IFeature pFeat;
            IFeatureCursor pFeatCur;
            IEnumLayer pEnumLayer;
            int lAIndex;
            //string pFeatureCodes = String.Empty;// nicht verwenden, da Länge einer string Variable zu klein!
            List<string> pFeatureCodesList = new List<string>(); // Create new list of strings

            int flNUM = 1;//Neu 15.05.2011
            string flNUM_file;//neu 15.05.2011

            int LayerCount;
            //UID pUID = new UIDClass();
            UID pUID = new ESRI.ArcGIS.esriSystem.UID();

            #region Analyse des sichtbaren Bereichs: analyse of the viewable area:

            if (pLegendGeneratorForm.chkNurSichtbarerBereich.IsChecked == true)
            {
                pPageLayout = pMxDoc.PageLayout;
                pGraphContainer = (IGraphicsContainer)pPageLayout;
                pMap = pMxDoc.FocusMap;
                pMapFrame = (IMapFrame)pGraphContainer.FindFrame(pMap);
                pEnvelope = pMapFrame.MapBounds.Envelope;
                pSpatialFilter = new SpatialFilter
                {
                    Geometry = pEnvelope,
                    SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects
                };

                //Search for intersecting features in each feature layer in the focus map
                //alle layer mit gen_ID-Feld werden beruecksichtigt:
                LayerCount = 0;
                pUID.Value = "{E156D7E5-22AF-11D3-9F99-00C04F6BC78E}";//GUID for IGeoFeatureLayer
                pEnumLayer = pMap.get_Layers(pUID, true);
                pEnumLayer.Reset();
                pFeatLayer = (IFeatureLayer)pEnumLayer.Next();

                while (pFeatLayer != null)
                {
                    if (pFeatLayer.FeatureClass != null)
                    {
                        pFeatcls = pFeatLayer.FeatureClass;
                        pFlds = pFeatcls.Fields;                     
                        lAIndex = pFlds.FindField(pLegendGeneratorForm.txtSpatialQueryId.Text);
                        pGFLayer = (IGeoFeatureLayer)pFeatLayer;

                        if (lAIndex > -1 && pFeatLayer.Visible == true)
                        {
                            pFld = pFlds.get_Field(lAIndex);
                            pSpatialFilter.GeometryField = pFeatcls.ShapeFieldName;
                            flDef = (IFeatureLayerDefinition)pFeatLayer;
                            pSpatialFilter.WhereClause = flDef.DefinitionExpression;
                            pFeatCur = pFeatcls.Search(pSpatialFilter, false);

                            if (pFeatcls.FeatureCount(pSpatialFilter) > 0)
                            {
                                pFeat = pFeatCur.NextFeature();
                                while (pFeat != null)
                                {
                                    //wenn im string der code noch nicht drinnen ist:
                                    if (pFeatureCodesList.Contains(pFeat.get_Value(lAIndex).ToString()) == false)
                                    {
                                        pFeatureCodesList.Add(pFeat.get_Value(lAIndex).ToString());
                                    }//end if

                                    pFeat = pFeatCur.NextFeature();
                                }//while schleife zu
                            }//endif
                        }//endif                       
                    }//end if FeatureClass != null
                    pFeatLayer = (IFeatureLayer)pEnumLayer.Next();
                    LayerCount = LayerCount + 1;
                }//closing while-loop
            }//  if (chkNurSichtbarerBereich.Checked == true) zu

            if (pLegendGeneratorForm.chkNurSichtbarerBereich.IsChecked == true && pFeatureCodesList.Count == 0)
            {
                MessageBox.Show("Thera are no features in the viewable features for creating the legend! \n " +
                    "Please define another visible area!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            #endregion

            #region Database:

            //Legendentabelle für die Abfrage vorbereiten: //////////////////////////////////////////////////////////////////////////////////
            IQueryFilter pQueryFilter;
            ITableSort pTableSort;
            ICursor tabCursor;
            IRow pRow;
            ITable pTable = null;
            //ILegendGeneratorController repository = new LegendGeneratorController();
            IDataService repository = new DataService();
            if (pLegendGeneratorForm.chkAccess.IsChecked == true)
            //if (pLegendGeneratorForm.tabItemAccess.IsSelected == true)
            {
                pTable = repository.GetArcObjectsAccessTable(leg_tab_pfad, legendentabelle);
                if (pTable == null)
                {
                    MessageBox.Show("Please define a correct legend table!");
                    pLegendGeneratorForm.staLblMessage.Content = "Please define the legend table!";
                    pLegendGeneratorForm.Cursor = System.Windows.Input.Cursors.Arrow;
                    return;
                }
            }
            else//SDE
            {
                if (pLegendGeneratorForm.chkAuthentifizierung.IsChecked == false)
                {
                    pTable = repository.GetArcObjectsSdeTable(pLegendGeneratorForm.txtSqlServer.Text, pLegendGeneratorForm.txtInstance.Text, pLegendGeneratorForm.txtDatabase.Text, pLegendGeneratorForm.txtVersion.Text, legendentabelle);
                    if (pTable == null)
                    {
                        MessageBox.Show("Please define a correct legend table!", "Error in the database finding process", MessageBoxButton.OK, MessageBoxImage.Warning);
                        pLegendGeneratorForm.staLblMessage.Content = "Please define a correct legend table!";
                        pLegendGeneratorForm.Cursor = System.Windows.Input.Cursors.Arrow;
                        return;
                    }
                }
                else if (pLegendGeneratorForm.chkAuthentifizierung.IsChecked == true)
                {
                    pTable = repository.GetArcObjectsSdeTable(pLegendGeneratorForm.txtSqlServer.Text, pLegendGeneratorForm.txtInstance.Text, pLegendGeneratorForm.txtDatabase.Text, pLegendGeneratorForm.txtVersion.Text, legendentabelle,
                       pLegendGeneratorForm.txtUser.Text, pLegendGeneratorForm.txtPassword.Text);
                    if (pTable == null)
                    {
                        MessageBox.Show("Please define a correct legend table!", "Error in the database finding process", MessageBoxButton.OK, MessageBoxImage.Warning);
                        pLegendGeneratorForm.staLblMessage.Content = "Please define a correct legend table!";
                        pLegendGeneratorForm.Cursor = System.Windows.Input.Cursors.Arrow;
                        return;
                    }
                }
            }
            pQueryFilter = new QueryFilter();

            #endregion

            //neu: check if the sorting field name in the database exists, otherwise return to the main program:
            if (pTable.FindField(leg_sort_feld) == -1)
            { ErrorMessageBox(leg_sort_feld); return; }

            //Legendentabelle abfragen: QueryFilter wurde zuvor mit der Table angelegt:
            pQueryFilter.WhereClause = pLegendGeneratorForm.txtAttributeQuery.Text; //creleg_form.QueryFilter z.B. L_SORT LIKE 'A*'
            
            if (pTable.HasOID == true && pLegendGeneratorForm.chkSde.IsChecked == false) // für reg Access-Datenbanken
            {
                try
                {
                    pTableSort = new TableSort
                    {
                        Fields = leg_sort_feld
                    };
                    pTableSort.set_Ascending(leg_sort_feld, true);
                    pTableSort.set_CaseSensitive(leg_sort_feld, true);
                    pTableSort.QueryFilter = pQueryFilter;
                    pTableSort.Table = pTable;
                    pTableSort.Sort(null);
                    tabCursor = pTableSort.Rows;
                }
                catch
                {
                    MessageBox.Show("Error in the attribute query! The query is not correct!", "Query Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }
            else //fuer SQL-Datenbanken:
            {
                try
                {
                    tabCursor = pTable.Search(pQueryFilter, false);
                }
                catch
                {
                    MessageBox.Show("Error in the attribute query! The query is not correct!", "Query Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }//endif

            //Legendentabelle fertig vorbereitet:
            ///////////////////Create the legend - Legende erstellen///////////////////////////////////////////////////////////////////////////////////////////////////
            ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

            //this.pLegendGroupElement = new GroupElementClass();
            this.pLegendGroupElement = new GroupElement() as IGroupElement;
            IFillShapeElement pFillShapeElement;
            IFillShapeElement p2FillShapeElement;
            ITransform2D pTransform2D;
            IFillSymbol pFS_grafik = null;

            ITransform2D chefTransform2D = null;
            IMarkerElement pMarkerElement;
            ILineElement pLineElement; 
            IElement p2Element, p3Element = null; 

            //vb-seite 9: Anlegen aller Clone-Objekte
            IClone pSymCloned, pSymSource, pRectCloned, pRectSource = null, pxCloned, p2xCloned, pxSource;
            IClone pLineCloned, pLineSource = null, pKlaLineCloned, pKlaLineSource = null, pPointCloned, pPointSource = null;
            IClone pBezCloned, pBezSource = null, pRefCloned, pUs1Cloned, pUs1Source = null, pUs2Cloned, pUs2Source = null;
            IClone pUs3Cloned, pUs3Source = null, pKlaCloned, pKlaSource = null, pLabCloned = null, pLabSource = null, pGraCloned, pGraSource = null;//Gra fue Grafik
            IClone p2MrkCloned = null, p2MrkSource = null;//für Marker setzen
            IElement p2El;//für Marker setzen
            ITransform2D p2Tr;//für Marker setzen

            IElement pRectangleElement = null;

            //Musterfarben definieren:
            ICmykColor pColorArot = null, pColorAblau = null, pColorAgruen = null, pColorAbraun = null,
               pColorGrau = null, pColorMagenta = null, pColorCyan = null, pColorBlack = null, pColorWhite = null, pColorFehler,
               pColorGelb = null, pColorOrange = null, pColorNull = null, pColorFEHLER = null;
            try
            {
                //Musterfarben definieren: zum Schluss auch etwaige Fehlerfarben:          
                pColorArot = GetCMYKColor(Convert.ToInt32(pLegendGeneratorForm.txtRoc.Text), Convert.ToInt32(pLegendGeneratorForm.txtRom.Text), Convert.ToInt32(pLegendGeneratorForm.txtRoy.Text), Convert.ToInt32(pLegendGeneratorForm.txtRok.Text));// = new CmykColor();
                pColorAblau = GetCMYKColor(Convert.ToInt32(pLegendGeneratorForm.txtBlc.Text), Convert.ToInt32(pLegendGeneratorForm.txtBlm.Text), Convert.ToInt32(pLegendGeneratorForm.txtBly.Text), Convert.ToInt32(pLegendGeneratorForm.txtBlk.Text));
                pColorAgruen = GetCMYKColor(Convert.ToInt32(pLegendGeneratorForm.txtGrc.Text), Convert.ToInt32(pLegendGeneratorForm.txtGrm.Text), Convert.ToInt32(pLegendGeneratorForm.txtGry.Text), Convert.ToInt32(pLegendGeneratorForm.txtGrk.Text));
                pColorAbraun = GetCMYKColor(Convert.ToInt32(pLegendGeneratorForm.txtBrc.Text), Convert.ToInt32(pLegendGeneratorForm.txtBrm.Text), Convert.ToInt32(pLegendGeneratorForm.txtBry.Text), Convert.ToInt32(pLegendGeneratorForm.txtBrk.Text));
                pColorGrau = GetCMYKColor(Convert.ToInt32(pLegendGeneratorForm.txtGac.Text), Convert.ToInt32(pLegendGeneratorForm.txtGam.Text), Convert.ToInt32(pLegendGeneratorForm.txtGay.Text), Convert.ToInt32(pLegendGeneratorForm.txtGak.Text));
                pColorMagenta = GetCMYKColor(Convert.ToInt32(pLegendGeneratorForm.txtMac.Text), Convert.ToInt32(pLegendGeneratorForm.txtMam.Text), Convert.ToInt32(pLegendGeneratorForm.txtMay.Text), Convert.ToInt32(pLegendGeneratorForm.txtMak.Text));
                pColorCyan = GetCMYKColor(Convert.ToInt32(pLegendGeneratorForm.txtCyc.Text), Convert.ToInt32(pLegendGeneratorForm.txtCym.Text), Convert.ToInt32(pLegendGeneratorForm.txtCyy.Text), Convert.ToInt32(pLegendGeneratorForm.txtCyk.Text));
                pColorBlack = GetCMYKColor(0, 0, 0, 100);
                pColorWhite = GetCMYKColor(0, 0, 0, 0);
                pColorFehler = GetCMYKColor(0, 100, 100, 0);
                pColorGelb = GetCMYKColor(Convert.ToInt32(pLegendGeneratorForm.txtGec.Text), Convert.ToInt32(pLegendGeneratorForm.txtGem.Text), Convert.ToInt32(pLegendGeneratorForm.txtGey.Text), Convert.ToInt32(pLegendGeneratorForm.txtGek.Text));
                pColorOrange = GetCMYKColor(Convert.ToInt32(pLegendGeneratorForm.txtOrc.Text), Convert.ToInt32(pLegendGeneratorForm.txtOrm.Text), Convert.ToInt32(pLegendGeneratorForm.txtOry.Text), Convert.ToInt32(pLegendGeneratorForm.txtOrk.Text));
                pColorNull = new CmykColor
                {
                    NullColor = true
                };
                pColorFEHLER = GetCMYKColor(0, 100, 100, 0);
            }
            catch (FormatException)
            {
                MessageBox.Show("False input format for the colors! Please only define numbers!");
                pLegendGeneratorForm.staLblMessage.Content = "False input format for the colors! Please only define numbers!";
                pLegendGeneratorForm.Cursor = System.Windows.Input.Cursors.Arrow;
                return;
            }
            catch (Exception ex)
            {
                pLegendGeneratorForm.staLblMessage.Content = ex.Message;
                pLegendGeneratorForm.Cursor = System.Windows.Input.Cursors.Arrow;
                return;
            }
            //vb-code seite 10:
            IPoint pOrigin = null, chefOrigin = null;
            ITextElement pTextElement, pTextElement2 = null, pbezTextElement = null, prefTextElement = null;
            IFillSymbol pSFS = new SimpleFillSymbol(); 
            IFillSymbol p2SFS = new SimpleFillSymbol();

            ILineSymbol pLS_undermost = new SimpleLineSymbol();
            pLS_undermost.Color.NullColor = true;
            pLS_undermost.Width = 0;

            ITextSymbol pTextSymbol = new TextSymbol();
            IFormattedTextSymbol pFormattedTextSymbol;

            IMultiLayerFillSymbol pMultiLayerFillSymbol = null;
            IMultiLayerLineSymbol pMultiLayerLineSymbol = null;
            IMultiLayerMarkerSymbol pMultiLayerMarkerSymbol = null;

            IEnumStyleGalleryItem pEnumStyleGalleryItem;
            IStyleGalleryItem pStyleGalleryItem;
            IStyleGallery pStyleGallery = pMxDoc.StyleGallery;
            IStyleGalleryStorage StyleGalleryStorage;
            StyleGalleryStorage = (IStyleGalleryStorage)pStyleGallery;

            //search elements from the legend template:
            //Rechteck, Linie Punkt kopieren: in die entsprechedne Clone-Objekte werden sie kopiert:
            pGraphContainer.Reset();
            pElement = pGraphContainer.Next();
            double Kastlhoehe = 0;
            double LegendentextAbstand = 0;
            double farbdef_offset = 0;
            while (pElement != null)
            {
                if (pElement is IRectangleElement)
                {
                    if (pElement.Geometry.Envelope.Width < 3)
                    {
                        pRectangleElement = pElement;

                        pRectSource = (IClone)pElement;
                        Kastlhoehe = pElement.Geometry.Envelope.Height;
                        LegendentextAbstand = pElement.Geometry.Envelope.Width;
                        farbdef_offset = pElement.Geometry.Envelope.Width;
                        xmin_GE = pElement.Geometry.Envelope.XMin;
                        ymin_GE = pElement.Geometry.Envelope.YMin;
                        xmax_GE = pElement.Geometry.Envelope.XMax;
                        ymax_GE = pElement.Geometry.Envelope.YMax;

                    }//end if
                }
                else if (pElement is ILineElement)
                {
                    if (pElement.Geometry.Envelope.Width > 0.7 && pElement.Geometry.Envelope.Height < 3)
                    {
                        pLineSource = (IClone)pElement;
                    }
                    else if (pElement.Geometry.Envelope.Width < 0.7 && pElement.Geometry.Envelope.Height < 3)
                    {
                        pKlaLineSource = (IClone)pElement;
                    }//endif
                }
                else if (pElement is IMarkerElement)
                {
                    pPointSource = (IClone)pElement;
                }//endif

                pElement = pGraphContainer.Next();
            }//close while loop

            //copy the text elements from the legend template:
            ITextSymbol pTextSymbol2 = null;
            string grafont = String.Empty;
            pGraphContainer.Reset();
            //Bezeichnung kopieren:
            pElement = pGraphContainer.Next();
            while (pElement != null)
            {
                if (pElement is ITextElement)
                {
                    pTextElement = (ITextElement)pElement;
                    if (pTextElement.Text == "legend text")
                    {
                        pBezSource = (IClone)pElement;
                    }
                    else if (pTextElement.Text == "graphics")
                    {
                        pGraSource = (IClone)pElement;
                        grafont = pTextElement.Symbol.Font.Name;
                    }
                    else if (pTextElement.Text == "label")
                    {
                        pxSource = (IClone)pTextElement.Symbol;
                        pxCloned = (IClone)pxSource.Clone();
                        pTextSymbol2 = (ITextSymbol)pxCloned;
                        pLabSource = (IClone)pElement;//neu
                    }
                    else if (pTextElement.Text == "heading1")
                    {
                        pUs1Source = (IClone)pElement;
                    }
                    else if (pTextElement.Text == "heading2")
                    {
                        pUs2Source = (IClone)pElement;
                    }
                    else if (pTextElement.Text == "heading3")
                    {
                        pUs3Source = (IClone)pElement;
                    }
                    else if (pTextElement.Text == "bracket")
                    {
                        pKlaSource = (IClone)pElement;
                    }
                }//end if
                pElement = pGraphContainer.Next();
            }

            //if the legend template elements are not found, return to the main program:
            if (pRectangleElement == null || pLineSource == null || pKlaLineSource == null)
            {
                MessageBox.Show("There is no template in the page layout!", "Template Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }



            //variables for the graphical positioning:
            double x_offset, y_offset, kastlabstand, kastlhoehe_variabel, bezAbstand, klammerabstand_feld, spaltenoffset;
            int zeile_us1, zeile_us2, zeile_us3, zeile_bez, spaltenlaenge;
            string us1_alt = String.Empty, us2_alt = String.Empty, us3_alt = String.Empty, us4_alt = String.Empty, us5_alt = String.Empty, us6_alt = String.Empty,
                kl1_alt = String.Empty, kl2_alt = String.Empty, kl3_alt = String.Empty, gruppe_alt = "#";// klammer_alt = "#";

            try
            {
                x_offset = Convert.ToDouble(pLegendGeneratorForm.x_Offset.Text);
                y_offset = 0 - Convert.ToDouble(pLegendGeneratorForm.y_offset.Text);
                kastlabstand = Convert.ToDouble(pLegendGeneratorForm.kastlabstand.Text);                
                klammerabstand_feld = Convert.ToDouble(pLegendGeneratorForm.klammerabstand.Text);
                bezAbstand = Convert.ToDouble(pLegendGeneratorForm.txtBezAbstand.Text);

                zeile_us1 = Convert.ToInt32(pLegendGeneratorForm.zeile_us1.Text);
                zeile_us2 = Convert.ToInt32(pLegendGeneratorForm.zeile_us2.Text);
                zeile_us3 = Convert.ToInt32(pLegendGeneratorForm.zeile_us3.Text);
                zeile_bez = Convert.ToInt32(pLegendGeneratorForm.zeile_bez.Text);
                spaltenlaenge = Convert.ToInt32(pLegendGeneratorForm.spaltenlaenge.Text);
                spaltenoffset = Convert.ToDouble(pLegendGeneratorForm.spaltenoffset.Text);
                
                ////Kastlhoehe wurde vorher bei den Klonobjekten definiert:
                kastlhoehe_variabel = Kastlhoehe;
            }
            catch (Exception)
            {
                MessageBox.Show("False input format for the variables of the graphical positioning! Please only define numbers!", "Format Exception", MessageBoxButton.OK, MessageBoxImage.Error);
                pLegendGeneratorForm.staLblMessage.Content = "False input format for the variables of the graphical positioning! Please only define numbers!";
                pLegendGeneratorForm.Cursor = System.Windows.Input.Cursors.Arrow;
                return;
            }

            ////Stream writer object for the sequential numbering:
            //StreamWriter sw = null;             
            StreamReader sr = null;
            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            if (pLegendGeneratorForm.chkFortlaufendeNummer.IsChecked == true)
            {
                //System.IO.FileStream fs;//um den Esri Filestream zu untzerbinden!
                if (File.Exists(pLegendGeneratorForm.txtFortlaufendeNummer.Text) == true)
                {
                    flNUM_file = pLegendGeneratorForm.txtFortlaufendeNummer.Text;
                    try
                    {
                        //fs = new System.IO.FileStream(flNUM_file, FileMode.Open);
                        //sw = new StreamWriter(fs);

                        using (sr = new StreamReader(flNUM_file))
                        {                           
                            while (sr.Peek() >= 0)
                            {
                                var line = sr.ReadLine();
                                var items = line.Split(new Char[] { ',' });
                                dictionary.Add(items[1], items[0]);
                            }
                            sr.Close();
                        }
                      
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "Error while opening the textfile", MessageBoxButton.OK, MessageBoxImage.Error);
                        pLegendGeneratorForm.staLblMessage.Content = ex.Message;
                        return;
                    }
                }
                else
                {
                    MessageBox.Show(this.CreateTextFileDoesNotExistMsg(), "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                    pLegendGeneratorForm.staLblMessage.Content = "Please define a textfile!";
                    return;
                }
            }
            //var test = dictionary;
            #region writing the date stamp to the Page Layout:
            /////////////////////////////writing date stamp /////////////////////////////////////////////////////////////////////
            pUs1Cloned = pUs1Source.Clone();
            pTransform2D = (ITransform2D)pUs1Cloned;
            pTransform2D.Move(0, y_offset + 1);
            pTextElement = (ITextElement)pTransform2D;
            DateTime d1 = new DateTime();
            d1 = DateTime.Now;
            string leg_head = "legend table: ";
            //if a SDE-Database is used:
            if (pLegendGeneratorForm.chkSde.IsChecked == true)
            {
                leg_head = leg_head + "SDE-table " + pLegendGeneratorForm.cboTable.Text;
            }
                //if an Access-Database is used:
            else
            {
                leg_head = leg_head + leg_tab_pfad;
            }
            leg_head = leg_head + " " + pLegendGeneratorForm.cboTable.Text + '\n';
            leg_head = leg_head + "printed at " + d1.ToString() + '\n';
            leg_head = leg_head + "QueryFilter: " + pLegendGeneratorForm.txtAttributeQuery.Text + '\n';
            if (pLegendGeneratorForm.chkNurSichtbarerBereich.IsChecked == true)
            {
                leg_head = leg_head + "legend calculated for visible area only\n";
            }
            if (pLegendGeneratorForm.chkFortlaufendeNummer.IsChecked == true)
            {
                leg_head = leg_head + "textfile with L_ID-Label: " + pLegendGeneratorForm.txtFortlaufendeNummer.Text;
            }

            pTextElement.Text = leg_head;
            pGraphContainer.AddElement((IElement)pTextElement, 0);

            pMxDoc.get_ContentsView(0).Visible = false;
            pMxDoc.get_ContentsView(0).Refresh(0);

            #endregion


            ////////////////////////Legendendurchlauf Einträge Schreiben://////////////////////////////////
            string errorMessage = String.Empty;
            int LegendenPosition = 1;

            pRow = tabCursor.NextRow();
            //LegendenPosition = 1;

            #region check if database fields exist:

            if (pTable.FindField(leg_ID_feld) == -1)//txtVerknuepfung.Text e.g.GBA!
            { ErrorMessageBox2(leg_ID_feld, pLegendGeneratorForm.lblDatabaseId.Content.ToString()); return; }

            if (pTable.FindField(grafik_feld) == -1 && pLegendGeneratorForm.chkLegendengrafik.IsChecked == true)
            { ErrorMessageBox2(grafik_feld, pLegendGeneratorForm.lblLegendengrafik.Content.ToString()); return; }

            //txtLegendentext
            if (pTable.FindField(bez_feld) == -1)
            { ErrorMessageBox2(bez_feld, pLegendGeneratorForm.lblLegendentext.Content.ToString()); return; }

            //neu:
            if (pTable.FindField(geometrie_feld) == -1)
            { ErrorMessageBox2(geometrie_feld, pLegendGeneratorForm.lblZeichenelemente.Content.ToString()); return; }
            
            if (pTable.FindField(us1_feld) == -1 && pLegendGeneratorForm.chkUeberschrift1.IsChecked == true)
            { ErrorMessageBox2(us1_feld, pLegendGeneratorForm.lblUeberschrift1.Content.ToString()); return; }

            if (pTable.FindField(us2_feld) == -1 && pLegendGeneratorForm.chkUeberschrift2.IsChecked == true)
            { ErrorMessageBox2(us2_feld, pLegendGeneratorForm.lblUeberschrift2.Content.ToString()); return; }

            if (pTable.FindField(us3_feld) == -1 && pLegendGeneratorForm.chkUeberschrift3.IsChecked == true)
            { ErrorMessageBox2(us3_feld, pLegendGeneratorForm.lblUeberschrift3.Content.ToString()); return; }

            if (pTable.FindField(kl1_feld) == -1 && pLegendGeneratorForm.chkKlammerebene1.IsChecked == true)
            { ErrorMessageBox2(kl1_feld, pLegendGeneratorForm.lblKlammerebene1.Content.ToString()); return; }

            if (pTable.FindField(kl2_feld) == -1 && pLegendGeneratorForm.chkKlammerebene2.IsChecked == true)
            { ErrorMessageBox2(kl2_feld, pLegendGeneratorForm.lblKlammerebene2.Content.ToString()); return; }

            if (pTable.FindField(kl3_feld) == -1 && pLegendGeneratorForm.chkKlammerebene3.IsChecked == true)
            { ErrorMessageBox2(kl3_feld, pLegendGeneratorForm.lblKlammerebene3.Content.ToString()); return; }

            if (pTable.FindField(fillsymbol_feld) == -1 && pLegendGeneratorForm.chkFlaeche.IsChecked == true)
            { ErrorMessageBox2(fillsymbol_feld, pLegendGeneratorForm.lblFlaeche.Content.ToString()); return; }

            if (pTable.FindField(linesymbol_feld) == -1 && pLegendGeneratorForm.chkLinie.IsChecked == true)
            { ErrorMessageBox2(linesymbol_feld, pLegendGeneratorForm.lblLinie.Content.ToString()); return; }

            if (pTable.FindField(markersymbol_feld) == -1 && pLegendGeneratorForm.chkMarker.IsChecked == true)
            { ErrorMessageBox2(markersymbol_feld, pLegendGeneratorForm.lblMarker.Content.ToString()); return; }

            ////neu:
            //if (pTable.FindField(geometrie_feld) == -1 && pLegendGeneratorForm.chkZeichenelemente.IsChecked == true)
            //{ ErrorMessageBox2(geometrie_feld, pLegendGeneratorForm.lblZeichenelemente.Content.ToString()); return; }

            if (pTable.FindField(gruppe_feld) == -1 && pLegendGeneratorForm.chkGruppierung.IsChecked == true)
            { ErrorMessageBox2(gruppe_feld, pLegendGeneratorForm.lblGruppierung.Content.ToString()); return; }

            

            if (pTable.FindField(leglab_feld) == -1 && pLegendGeneratorForm.chkLegendennummer.IsChecked == true)
            { ErrorMessageBox2(leglab_feld, pLegendGeneratorForm.lblLegendennummer.Content.ToString()); return; }

            if (pTable.FindField(us4_feld) == -1 && pLegendGeneratorForm.chkInfozeile4.IsChecked == true)
            {
                ErrorMessageBox2(us4_feld, pLegendGeneratorForm.lblInfozeile4.Content.ToString()); return; 
            }

            if (pTable.FindField(us5_feld) == -1 && pLegendGeneratorForm.chkInfozeile5.IsChecked == true)
            {
                ErrorMessageBox2(us5_feld, pLegendGeneratorForm.lblInfozeile5.Content.ToString()); return;
            }

            if (pTable.FindField(us6_feld) == -1 && pLegendGeneratorForm.chkInfozeile6.IsChecked == true)
            {
                ErrorMessageBox2(us6_feld, pLegendGeneratorForm.lblInfozeile6.Content.ToString()); return;
            }

            //Notizenfeld:
            if (pTable.FindField(notes_feld) == -1 && pLegendGeneratorForm.chkNotizenfeld.IsChecked == true)
            {
                ErrorMessageBox2(notes_feld, pLegendGeneratorForm.lblNotizen.Content.ToString()); return;
            }
            #endregion

            //////////while Schleife: Beginn Legendenprogramm: //////////////////////////////////////////////////////////////////////////////
            /////////////////////////////Definition of variables////////////////////////////////////////////////////////////////////////
            string gen_ID = String.Empty;
            string neu_gruppe = String.Empty;
            string grafik = String.Empty;
            string bez = String.Empty;
            //neu
            string bez_vater = String.Empty, not_vater = String.Empty;//bez wurd bereits auf vb-page 13 deklariert

            string us1, us2, us3, us4 = String.Empty, us5 = String.Empty, us6 = String.Empty, refS = String.Empty;
            string kl1 = String.Empty, kl2 = String.Empty, kl3 = String.Empty;//werden dann in der wersten while-Schleif gesetzt
            string fillsym, linesym, markersym;
            string geom, gruppe = String.Empty;
            string leglab;

            //zusätzlich:
            //int iGroup = 0;
            ITransform2D pMMarkerTransform = null;
            //List<IFillShapeElement> pFillShapeList = new List<IFillShapeElement>();
            Dictionary<int, IFillShapeElement> pFillShapeList = new Dictionary<int, IFillShapeElement>();
            //List<IMarkerElement> pMarkerElementList = new List<IMarkerElement>();
            Dictionary<int, IMarkerElement> pMarkerElementList = new Dictionary<int, IMarkerElement>();
            Dictionary<int, ILineElement> pLineElementList = new Dictionary<int, ILineElement>();
            List<ITextElement> pTextElementList = new List<ITextElement>();
            //List<ITextElement> pGrafikTextElementList = new List<ITextElement>();
            Dictionary<int, ITextElement> pGrafikTextElementList = new Dictionary<int, ITextElement>();
            List<ITextElement> pFarbeTextElementList = new List<ITextElement>();

            bool mehrling = false;
            int mehrlingPosition = 0;
            //bool mehrlingsende = false;
            int m_zaehler = 0;
            int Kastl_Typ = 0;
            bool buegel = false;
            double ykla1_anfang = 0, ykla1_ende = 0, ykla2_anfang = 0, ykla2_ende = 0, ykla3_anfang = 0, ykla3_ende = 0;//werden dann gesetzt
            ICmykColor pCmyk;
            ICmykColor p2Cmyk = null;
            string Cyan1, Magenta1, Yellow1, Black1;
            int leg_pos_count = 0;

            int maxRecords = pTable.RowCount(pQueryFilter);        
            //show progress-text:
            this.pd = new LegendGenerator.App.View.ProgressDialog();
            //pd.ProgressText = pUVRenderer.ValueCount.ToString() + " features are in the rendering process. Please stand by...";
            pd.Show();
            //Configure the ProgressBar
            pd.Progress.Minimum = 0;
            pd.Progress.Maximum = maxRecords;
            pd.Progress.Value = 0;
            //Stores the value of the ProgressBar
            double value = 0;
            //Create a new instance of our ProgressBar Delegate that points
            //  to the ProgressBar's SetValue method.
            UpdateProgressBarDelegate updatePbDelegate = new UpdateProgressBarDelegate(pd.Progress.SetValue);
            UpdateProgressDelegate update = new UpdateProgressDelegate(UpdateProgressText);

            while (pRow != null)//vb-code seite 15 & 16 ----> ENDe dieser while-Schleife ist auf vb-page 28
            {
                
                value = value + 1;
                /*Update the Value of the ProgressBar:
                    1)  Pass the "updatePbDelegate" delegate that points to the ProgressBar1.SetValue method
                    2)  Set the DispatcherPriority to "Background"
                    3)  Pass an Object() Array containing the property to update (ProgressBar.ValueProperty) and the new value */
                pd.Dispatcher.Invoke(updatePbDelegate, System.Windows.Threading.DispatcherPriority.Background,
                    new object[] { System.Windows.Controls.ProgressBar.ValueProperty, value });
                //Fortschrittsanzeige:
                pd.Dispatcher.Invoke(update, Convert.ToInt32(((decimal)value / (decimal)maxRecords) * 100), maxRecords, (int)value);

                #region initializing of the database variables:
                gen_ID = pRow.get_Value(pTable.FindField(leg_ID_feld)).ToString();//leg_ID_feld = txtVerknuepfung.Text!

                if (pLegendGeneratorForm.chkLegendengrafik.IsChecked == true)
                {
                    grafik = pRow.get_Value(pTable.FindField(grafik_feld)).ToString();
                }
                else
                {
                    grafik = "#";
                }

                //Bezeichnung hat auch kein Checkbox:
                bez = pRow.get_Value(pTable.FindField(bez_feld)).ToString();

                if (pLegendGeneratorForm.chkUeberschrift1.IsChecked == true)
                {
                    us1 = pRow.get_Value(pTable.FindField(us1_feld)).ToString();
                }
                else
                {
                    us1 = "#";
                }


                if (pLegendGeneratorForm.chkUeberschrift2.IsChecked == true)
                {
                    us2 = pRow.get_Value(pTable.FindField(us2_feld)).ToString();
                }
                else
                {
                    us2 = "#";
                }


                if (pLegendGeneratorForm.chkUeberschrift3.IsChecked == true)
                {
                    us3 = pRow.get_Value(pTable.FindField(us3_feld)).ToString();
                }
                else
                {
                    us3 = "#";
                }

                if (pLegendGeneratorForm.chkInfozeile4.IsChecked == true)
                {
                    us4 = pRow.get_Value(pTable.FindField(us4_feld)).ToString();
                }//endif

                if (pLegendGeneratorForm.chkInfozeile5.IsChecked == true)
                {
                    us5 = pRow.get_Value(pTable.FindField(us5_feld)).ToString();
                }

                if (pLegendGeneratorForm.chkInfozeile6.IsChecked == true)
                {
                    us6 = pRow.get_Value(pTable.FindField(us6_feld)).ToString();
                }

                if (pLegendGeneratorForm.chkNotizenfeld.IsChecked == true)
                {
                    refS = pRow.get_Value(pTable.FindField(notes_feld)).ToString();
                }

                if (pLegendGeneratorForm.chkKlammerebene1.IsChecked == true)
                {
                    kl1 = pRow.get_Value(pTable.FindField(kl1_feld)).ToString();
                }
                else
                {
                    kl1 = "#";
                }

                if (pLegendGeneratorForm.chkKlammerebene2.IsChecked == true)
                {
                    kl2 = pRow.get_Value(pTable.FindField(kl2_feld)).ToString();
                }
                else
                {
                    kl2 = "#";
                }

                if (pLegendGeneratorForm.chkKlammerebene3.IsChecked == true)
                {
                    kl3 = pRow.get_Value(pTable.FindField(kl3_feld)).ToString();
                }
                else
                {
                    kl3 = "#";
                }

                if (pLegendGeneratorForm.chkFlaeche.IsChecked == true)
                {
                    fillsym = pRow.get_Value(pTable.FindField(fillsymbol_feld)).ToString();//fillsymbol_feld = this.txtFlaeche
                }
                else
                {
                    fillsym = "#";
                }

                if (pLegendGeneratorForm.chkLinie.IsChecked == true)
                {
                    linesym = pRow.get_Value(pTable.FindField(linesymbol_feld)).ToString();//linesymbol_feld = this.txtLinie
                }
                else
                {
                    linesym = "#";
                }

                if (pLegendGeneratorForm.chkMarker.IsChecked == true)
                {
                    markersym = pRow.get_Value(pTable.FindField(markersymbol_feld)).ToString();//markersymbol_feld = this.txtMarker
                }
                else
                {
                    markersym = "#";
                }

                if (pLegendGeneratorForm.chkGruppierung.IsChecked == true)
                {
                    geom = pRow.get_Value(pTable.FindField(geometrie_feld)).ToString();//geometrie_feld = this.txtZeichenelement
                }
                else
                {
                    geom = "FLM";
                }

                if (pLegendGeneratorForm.chkGruppierung.IsChecked == true)
                {
                    gruppe = pRow.get_Value(pTable.FindField(gruppe_feld)).ToString();//gruppe_feld = this.txtGruppierung
                }
                else
                {
                    gruppe = "#";
                }

                if (pLegendGeneratorForm.chkLegendennummer.IsChecked == true)
                {
                    leglab = pRow.get_Value(pTable.FindField(leglab_feld)).ToString();//leglab_feld = this.txtLegendennummer
                }
                else
                {
                    leglab = "100";
                }

                //}//while-Schleife zu esrt auf vb-page Seite 28 zu:
                pRow = tabCursor.NextRow();//fuer naechsten Schleifendurchlauf
                if (pRow != null && pLegendGeneratorForm.chkGruppierung.IsChecked == true)
                {
                    neu_gruppe = pRow.get_Value(pTable.FindField(gruppe_feld)).ToString();
                }
                else
                {
                    neu_gruppe = String.Empty;
                }

                #endregion


                //Beginn der if-Schleife: ende auf vb-page 28
                if (pLegendGeneratorForm.chkNurSichtbarerBereich.IsChecked == false || pFeatureCodesList.Contains(gen_ID) == true)//entweder gesamte Legendentabelle oder nur diejenigen ID's die gerade sichtbar sind!
                {                 

                    //mehrlinge nach unten:
                    /////////////////////////////////////////////////////Gruppenverhältnisse klären mittels Kastl_Typ: vb-code seite 17///
                    // 1 - normales Kastl
                    //2- Gruppen-Vater, erstes Kastl
                    //3 - Gruppen-Kind
                    //4 - erstes Gruppen-Kind ohne Vater (erster Bruder)
                    //5 - Gruppen-Kind ohne Vater (Bruder)
                    //6 - Lieblingskind
                    //7 - Mehrlinge, erstes Kastl
                    //8 - Mehrlinge

                    //mit den folgenden if-Abfragen werden nun die Kastltypen zugeordnet
                    if (gruppe.PartString(0, 1) == "+") // at the frist position wit a length of 1
                    {
                        //8 - Mehrlinge
                        if ((gruppe_alt == gruppe))
                        {
                            Kastl_Typ = 8;
                            mehrling = true;
                            m_zaehler = m_zaehler + 1;//z.B.2
                        }
                        //7 - Mehrlinge, erstes Kastl und Mehrlinge beenden
                        else
                        {
                            m_zaehler = 1;
                            Kastl_Typ = 7;
                            gruppe_alt = gruppe;
                            if (("+" + gruppe_alt == gruppe))
                            //if ((gruppe_alt == gruppe))
                            {
                                y_offset = y_offset + kastlabstand;
                            }
                        }
                    }
                    else if (gruppe == gen_ID)
                    {
                        Kastl_Typ = 2; //Gruppen-Vater, erstes Kastl
                        buegel = true;

                        if (gruppe_alt == gruppe)//Onkel
                        {
                            y_offset = y_offset + kastlabstand;//Kompensation für unten
                        }
                        gruppe_alt = gen_ID;
                    }
                    else if (gruppe == "#")
                    {
                        Kastl_Typ = 1;
                        //1 - normales Kastl
                        buegel = false;
                        gruppe_alt = "x";
                    }
                    else if ((gruppe_alt == gruppe) && (buegel == true))
                    {
                        //if (Left(bez, 1) == "#")
                        if (bez.PartString(0, 1) == "#")//bez = txtLegendentext
                        {
                            Kastl_Typ = 6;
                            //6 - Lieblings-Kind,
                        }
                        else
                        {
                            Kastl_Typ = 3;//3 - Gruppen-Kind
                        }
                    }
                    //else if ((gruppe_alt == gruppe) || ("-" + gruppe_alt == gruppe) || (gruppe_alt == "-" + gruppe))
                    else if ((gruppe_alt == gruppe) || ("-" + gruppe_alt == gruppe)) //|| (gruppe_alt == "-" + gruppe))
                    {
                        Kastl_Typ = 5;//5 - Gruppen-Kind ohne Vater
                        buegel = false;
                    }
                    else
                    {
                        Kastl_Typ = 4;//4 - erstes Gruppen-Kind ohne Vater
                        buegel = false;
                        gruppe_alt = gruppe;
                    }

                    //bei Gruppenkinder: Vorschub in der Hoehe berechnen: vb-code seite 17
                    if ((Kastl_Typ == 3) || (Kastl_Typ == 5))//Gruppenkind oder Gruppenkind ohne Vater
                    {
                        y_offset = y_offset - kastlhoehe_variabel;//kastlhoehe_variabel: siehe vb-code seite 12 definitionen
                    }
                    else if ((Kastl_Typ == 6) || (Kastl_Typ == 8))//bei Lieblingskind und Mehrlingen keinen Vorschub setzen: da in der gleichen Höhe
                    {
                    }
                    else
                    {
                        y_offset = y_offset - kastlabstand - kastlhoehe_variabel;//für einfaches Kastl;7,8
                    }

                    #region set brackets - Klammern setzen:

                    //set bracket number 1 - Klammer 1 kl1 setzen:
                    if (kl1_alt != kl1 && kl1_alt.Length > 2)
                    {
                        if ((Kastl_Typ == 3) || (Kastl_Typ == 5) || (Kastl_Typ == 6)) //je nach Kaestchentyp wird die Klammergröße bestimmt
                        {
                            //size of bracket1 via y_offset - Klammergröße via y_offset
                            ykla1_ende = y_offset;
                        }
                        else
                        {
                            ykla1_ende = y_offset + kastlabstand;
                        }
                        pKlaLineCloned = pKlaLineSource.Clone(); 
                        pTransform2D = (ITransform2D)pKlaLineCloned;

                        pTransform2D.Move(x_offset, ykla1_anfang);

                        pElement = (IElement)pTransform2D;                     
                        pOrigin = new ESRI.ArcGIS.Geometry.Point();
                        pOrigin.PutCoords(pElement.Geometry.Envelope.XMax, pElement.Geometry.Envelope.YMax);
                        pTransform2D.Scale(pOrigin, 1, (ykla1_anfang - ykla1_ende) / Kastlhoehe);

                        pLineElement = (ILineElement)pTransform2D;
                        //pGraphContainer.AddElement((IElement)pLineElement, 0);
                        pLegendGroupElement.AddElement((IElement)pLineElement);
                        //On Error GoTo Templatefehler
                        pKlaCloned = pKlaSource.Clone();//pKlaSource wir auf vb-Seite 9 gesetzt und dann auf 12 gesetzt
                        pTransform2D = (ITransform2D)pKlaCloned;
                        pTextElement = (ITextElement)pTransform2D;
                        pTextElement.Text = Zeilenumbruch(kl1_alt, zeile_bez);//siehe Zeilenumbruch - muss noch gedebugt werden

                        pTransform2D.Move(x_offset, ykla1_anfang + kastlabstand - (ykla1_anfang - ykla1_ende) / 2);
                        //pGraphContainer.AddElement((IElement)pTextElement, 0);
                        pLegendGroupElement.AddElement((IElement)pTextElement);
                        if (kl1.Length < 3)
                        {
                            kl1_alt = kl1;
                        }//End if          
                    }

                    //Klammer 2 kl2 setzen:
                    //aller erster Aufruf von kl2_alt -> vorher empty-string
                    if (kl2_alt != kl2 && kl2_alt.Length > 2)
                    {
                        if ((Kastl_Typ == 3) || (Kastl_Typ == 5) || (Kastl_Typ == 6)) //je nach Kaestchentyp wird die Klammergröße bestimmt
                        {
                            //Klammergröße via y_offset
                            ykla2_ende = y_offset; //ykla1_ende = bei der großen Variablendefinition vp-page 13:
                        }
                        else
                        {
                            ykla2_ende = y_offset + kastlabstand;
                        }
                        //On ErrorGoTo Templatefehler
                        pKlaLineCloned = pKlaLineSource.Clone(); //siehe vb-page 9: anlegen aller clone-objekte
                        pTransform2D = (ITransform2D)pKlaLineCloned;

                        pTransform2D.Move(x_offset + klammerabstand_feld, ykla2_anfang);//Aenderung zu Klammer1 xOffsetz wird vereändert

                        pElement = (IElement)pTransform2D;
                        //pOrigin = new PointClass();
                        pOrigin = new ESRI.ArcGIS.Geometry.Point();
                        pOrigin.PutCoords(pElement.Geometry.Envelope.XMax, pElement.Geometry.Envelope.YMax);

                        pTransform2D.Scale(pOrigin, 1, (ykla2_anfang - ykla2_ende) / Kastlhoehe);
                        pLineElement = (ILineElement)pTransform2D;
                        //pGraphContainer.AddElement((IElement)pLineElement, 0);
                        pLegendGroupElement.AddElement((IElement)pLineElement);
                        //On Error GoTo Templatefehler
                        pKlaCloned = pKlaSource.Clone();//pKlaSource wir auf vb-Seite 9 gesetzt und dann auf 12 gesetzt
                        pTransform2D = (ITransform2D)pKlaCloned;
                        pTextElement = (ITextElement)pTransform2D;
                        pTextElement.Text = Zeilenumbruch(kl2_alt, zeile_bez);//zeile_bez ist meisten 50 Zeichen lang!
                        pTransform2D.Move(x_offset + klammerabstand_feld, ykla2_anfang + kastlabstand - (ykla2_anfang - ykla2_ende) / 2);//Aenderung zu Klammer1 xOffsetz wird vereändert
                        //pGraphContainer.AddElement((IElement)pTextElement, 0);
                        pLegendGroupElement.AddElement((IElement)pTextElement);
                        if (kl2.Length < 3)
                        {
                            kl2_alt = kl2;
                        }//End if          
                    }//Ende Klammer2

                    //Klammer 3 kl3 setzen:
                    //aller erster Aufruf von kl3_alt -> vorher empty-string
                    if (kl3_alt != kl3 && kl3_alt.Length > 2)
                    {
                        if ((Kastl_Typ == 3) || (Kastl_Typ == 5) || (Kastl_Typ == 6)) //je nach Kaestchentyp wird die Klammergröße bestimmt
                        {
                            //Klammergröße via y_offset
                            ykla3_ende = y_offset; //ykla1_ende = bei der großen Variablendefinition vp-page 13:
                        }
                        else
                        {
                            ykla3_ende = y_offset + kastlabstand;
                        }
                        //On ErrorGoTo Templatefehler
                        pKlaLineCloned = pKlaLineSource.Clone(); //siehe vb-page 9: anlegen aller clone-objekte
                        pTransform2D = (ITransform2D)pKlaLineCloned;

                        pTransform2D.Move(x_offset + (klammerabstand_feld * 2), ykla3_anfang);//Aenderung zu Klammer2 xOffsetz wird vereändert mit 2 multipliziert

                        pElement = (IElement)pTransform2D;                       
                        pOrigin = new ESRI.ArcGIS.Geometry.Point();
                        pOrigin.PutCoords(pElement.Geometry.Envelope.XMax, pElement.Geometry.Envelope.YMax);

                        pTransform2D.Scale(pOrigin, 1, (ykla3_anfang - ykla3_ende) / Kastlhoehe);
                        pLineElement = (ILineElement)pTransform2D;
                        //pGraphContainer.AddElement((IElement)pLineElement, 0);
                        pLegendGroupElement.AddElement((IElement)pLineElement);
                        //On Error GoTo Templatefehler
                        pKlaCloned = pKlaSource.Clone();//pKlaSource wir auf vb-Seite 9 gesetzt und dann auf 12 gesetzt
                        pTransform2D = (ITransform2D)pKlaCloned;
                        pTextElement = (ITextElement)pTransform2D;
                        pTextElement.Text = Zeilenumbruch(kl3_alt,zeile_bez);//zeile_bez ist meisten 50 Zeichen lang!
                        pTransform2D.Move(x_offset + (klammerabstand_feld * 2), ykla3_anfang + kastlabstand - (ykla3_anfang - ykla3_ende) / 2);//Aenderung zu Klammer2 xOffsetz wird vereändert; mit 2 multipliziert
                        //pGraphContainer.AddElement((IElement)pTextElement, 0);
                        pLegendGroupElement.AddElement((IElement)pTextElement);
                        if (kl3.Length < 3)
                        {
                            kl3_alt = kl3;
                        }//End if          
                    }//Ende Klammer3
                    #endregion

                    //Spaltenumbruch
                    if (LegendenPosition > spaltenlaenge && Kastl_Typ == 1 &&
                        ((kl1.Length < 2 && kl2.Length < 2 && kl3.Length < 2) || (kl1_alt != kl1 && kl2_alt != kl2 && kl3_alt != kl3)))
                    {
                        x_offset = x_offset + spaltenoffset;
                        y_offset = 0 - Convert.ToDouble(pLegendGeneratorForm.y_offset.Text) - 1;
                        LegendenPosition = 1;
                    }
                    LegendenPosition = LegendenPosition + 1;

                    #region set headings - Überschriften Offset setzen
                    //für Überschrift 1:
                    if (us1_alt != us1 && us1.Length > 2)//mindestens aus 2 Zeichen bestehen
                    {
                        //us1 setzen
                        us1_alt = us1;
                        us2_alt = "x";
                        //Fehlertext = "ERROR - heading1 not in template";
                        // ERROR: Not supported in C#: OnErrorStatement

                        pUs1Cloned = pUs1Source.Clone();
                        pTransform2D = (ITransform2D)pUs1Cloned;
                        pTextElement = (ITextElement)pTransform2D;
                        pTextElement.Text = Zeilenumbruch(us1, zeile_us1);
                      
                        try
                        {
                            y_offset = y_offset - Convert.ToDouble(pLegendGeneratorForm.vorus1.Text);
                        }
                        catch (FormatException)
                        {
                            MessageBox.Show("The variable 'before heading 1 wasn't defined as number");
                            pd.Close();
                            return;
                        }
                        pTransform2D.Move(x_offset, y_offset);
                        try
                        {
                            y_offset = y_offset - (pTextElement.Symbol.Size * 0.04 * (zeilen - 1)) - Convert.ToDouble(pLegendGeneratorForm.nachus1.Text);
                        }
                        catch (FormatException)
                        {
                            MessageBox.Show("The variable 'after heading 1 wasn't defined as number");
                            pd.Close();
                            return;
                        }

                        us1 = us1_alt;
                        //pGraphContainer.AddElement((IElement)pTextElement, 0);
                        pLegendGroupElement.AddElement((IElement)pTextElement);
                    }

                    //for heading2 - für Überschrift2:
                    if (us2_alt != us2 && us2.Length > 2)//mindesten aus 2 Zeichen bestehen
                    {
                        //us2 setzen
                        us2_alt = us2;
                        us3_alt = "x";
                        //Fehlertext = "ERROR - heading1 not in template";
                        // ERROR: Not supported in C#: OnErrorStatement

                        pUs2Cloned = pUs2Source.Clone();
                        pTransform2D = (ITransform2D)pUs2Cloned;
                        pTextElement = (ITextElement)pTransform2D;
                        pTextElement.Text = Zeilenumbruch(us2, zeile_us2);
                       
                        //y_offset = y_offset - Convert.ToDouble(this.vorus2.Text);fällt nun weg; da vorus2 nicht ueber User Interface gegeben ist!
                        pTransform2D.Move(x_offset, y_offset);
                        try
                        {
                            y_offset = y_offset - (pTextElement.Symbol.Size * 0.04 * (zeilen - 1)) - Convert.ToDouble(pLegendGeneratorForm.nachus2.Text);
                        }
                        catch
                        {
                            MessageBox.Show("The variable 'after heading 2 wasn't defined as number");
                            pd.Close();
                            return;
                        }
                        us2 = us2_alt;
                        //pGraphContainer.AddElement((IElement)pTextElement, 0);
                        pLegendGroupElement.AddElement((IElement)pTextElement);
                    }

                    //for heading3 - für Überschrift 3:
                    if (us3_alt != us3 && us3.Length > 2)//mindesten aus 2 Zeichen bestehen
                    {
                        //us2 setzen
                        us3_alt = us3;
                        pUs3Cloned = pUs3Source.Clone();
                        pTransform2D = (ITransform2D)pUs3Cloned;
                        pTextElement = (ITextElement)pTransform2D;
                        pTextElement.Text = Zeilenumbruch(us3, zeile_us3);
                      
                        //y_offset = y_offset - Convert.ToDouble(this.vorus2.Text);fällt nun weg; da vorus2 nicht ueber User Interface gegeben ist!
                        pTransform2D.Move(x_offset, y_offset);
                        y_offset = y_offset - (pTextElement.Symbol.Size * 0.04 * (zeilen - 1)) - Kastlhoehe - kastlabstand;
                        us3 = us3_alt;
                        //pGraphContainer.AddElement((IElement)pTextElement, 0);
                        pLegendGroupElement.AddElement((IElement)pTextElement);
                    }
                    #endregion

                    #region unnötige headingen setzen - nicht aber bei Kastltypen 3, 5 und 6:
                    //setting heading4, 5 and 6; unnötige headingen setzen - nicht aber bei Kastltypen 3, 5 und 6:                   
                    if (pLegendGeneratorForm.chkInfozeile4.IsChecked == true)
                    {
                        if (us4_alt != us4 && us4.Length > 2)
                        {
                            //us4 setzen
                            if ((Kastl_Typ == 3) || (Kastl_Typ == 5))
                            {
                                //MsgBox "unable to place infoline4 at ID " & gen_ID$, 64
                                pLegendGeneratorForm.staLblMessage.Content = "unable to place infoline4 at" + gen_ID;
                                MessageBox.Show("unable to place infoline4 at" + gen_ID);
                            }
                            else
                            {
                                us4_alt = us4;
                                Fehlertext = "ERROR - 'legend text' not in template";
                                // On Error GoTo Fehler
                                pBezCloned = pBezSource.Clone();//pBezSource = auf vb-page 8
                                pTransform2D = (ITransform2D)pBezCloned;
                                pTextElement = (ITextElement)pTransform2D;
                                pTextElement.Text = Zeilenumbruch(us4, zeile_us3);
                               
                                pTransform2D.Move(x_offset, y_offset);
                                y_offset = y_offset - (pTextElement.Symbol.Size * 0.04 * (zeilen - 1)) - Kastlhoehe - kastlabstand + 0.1;
                                us4 = us4_alt;
                                //pGraphContainer.AddElement((IElement)pTextElement, 0);
                                pLegendGroupElement.AddElement((IElement)pTextElement);
                            }
                        }
                    }

                    //if (creleg_form.us5_on.Value == true)
                    if (pLegendGeneratorForm.chkInfozeile5.IsChecked == true)
                    {
                        if (us5_alt != us5 && us5.Length > 2)
                        {
                            //us4 setzen
                            if ((Kastl_Typ == 3) || (Kastl_Typ == 5))
                            {
                                //MsgBox "unable to place infoline4 at ID " & gen_ID$, 64
                                pLegendGeneratorForm.staLblMessage.Content = "unable to place infoline5 at" + gen_ID;
                                MessageBox.Show("unable to place infoline5 at" + gen_ID);
                            }
                            else
                            {
                                us5_alt = us5;
                                Fehlertext = "ERROR - 'legend text' not in template";
                                // On Error GoTo Fehler
                                pBezCloned = pBezSource.Clone();//pBezSource = auf vb-page 8
                                pTransform2D = (ITransform2D)pBezCloned;
                                pTextElement = (ITextElement)pTransform2D;
                                pTextElement.Text = Zeilenumbruch(us5, zeile_us3);//passt schon so mit zeile_us3
                               
                                pTransform2D.Move(x_offset, y_offset);
                                y_offset = y_offset - (pTextElement.Symbol.Size * 0.04 * (zeilen - 1)) - Kastlhoehe - kastlabstand + 0.1;
                                us5 = us5_alt;
                                //pGraphContainer.AddElement((IElement)pTextElement, 0);
                                pLegendGroupElement.AddElement((IElement)pTextElement);
                            }
                        }
                    }

                    if (pLegendGeneratorForm.chkInfozeile6.IsChecked == true)
                    {
                        if (us6_alt != us6 && us6.Length > 2)
                        {
                            //us4 setzen
                            if ((Kastl_Typ == 3) || (Kastl_Typ == 5))
                            {
                                //MsgBox "unable to place infoline4 at ID " & gen_ID$, 64
                                pLegendGeneratorForm.staLblMessage.Content = "unable to place infoline6 at" + gen_ID;
                                MessageBox.Show("unable to place infoline6 at" + gen_ID);
                            }
                            else
                            {
                                us6_alt = us6;
                                Fehlertext = "ERROR - 'legend text' not in template";
                                pBezCloned = pBezSource.Clone();//pBezSource = auf vb-page 8
                                pTransform2D = (ITransform2D)pBezCloned;
                                pTextElement = (ITextElement)pTransform2D;
                                pTextElement.Text = Zeilenumbruch(us6, zeile_us3);//passt schon so mit zeile_us3
                             
                                pTransform2D.Move(x_offset, y_offset);
                                y_offset = y_offset - (pTextElement.Symbol.Size * 0.04 * (zeilen - 1)) - Kastlhoehe - kastlabstand + 0.1;
                                us6 = us6_alt;
                                //pGraphContainer.AddElement((IElement)pTextElement, 0);
                                pLegendGroupElement.AddElement((IElement)pTextElement);
                            }
                        }
                    }
                    #endregion //end of setting adiitional heading 4, 5 and 6; - unnötige Headingen setzen fertig!!!

                    //resetting of the bracket variables:
                    if (kl1_alt != kl1 && kl1.Length > 2)
                    {
                        kl1_alt = kl1;
                        ykla1_anfang = y_offset;
                    }
                    if (kl2_alt != kl2 && kl2.Length > 2)
                    {
                        kl2_alt = kl2;
                        ykla2_anfang = y_offset;
                    }
                    if (kl3_alt != kl3 && kl3.Length > 2)
                    {
                        kl3_alt = kl3;
                        ykla3_anfang = y_offset;
                    }

                    //create - clones - Clones erstellen:
                    pBezCloned = pBezSource.Clone();             
                    pRectCloned = pRectSource.Clone();//siehe vb-seite 9: Anlegen aller Clone-Objekte für pRectSource                    
                    pLabCloned = pLabSource.Clone();//derweilen noch mal herausen gelassen                    
                    pGraCloned = pGraSource.Clone();
                    pPointCloned = pPointSource.Clone();
                    pLineCloned = pLineSource.Clone();                    

                    #region Bezeichnung schreiben: Legendentext
                    //Bezeichnung schreiben
                    //string bez_vater = String.Empty, not_vater = String.Empty;//bez wurd bereits auf vb-page 13 deklariert
                    pTransform2D = (ITransform2D)pBezCloned;

                    pTransform2D.Move(x_offset + LegendentextAbstand + bezAbstand, y_offset);

                    pTextElement = (ITextElement)pTransform2D;
                    if (Kastl_Typ == 6)//Lieblingskind
                    {
                        //bez = bez_vater + ", " + Mid(bez, 2, 400);alte Mid-Funktion gibt es nicht mehr
                        int endeSubstring = bez.Length - 1;
                        bez = bez_vater + ", " + bez.PartString(1, endeSubstring);//Lieblingssohn!!!
                        //pGraphContainer.DeleteElement((IElement)pbezTextElement);
                        pLegendGroupElement.DeleteElement((IElement)pbezTextElement);
                    }
                    if (Kastl_Typ == 8)
                    {
                        bez = bez_vater + " / " + bez; //Mehrlinge
                        //pGraphContainer.DeleteElement((IElement)pbezTextElement);
                        pLegendGroupElement.DeleteElement((IElement)pbezTextElement);
                    }
                    bez_vater = bez;
                    pTextElement.Text = Zeilenumbruch(bez, zeile_bez);
                    if (bez != String.Empty && pTextElement.Text == "error")
                    {
                        return;
                    }
                    if (zeilen > 1)
                    {
                        pTransform2D.Move(0, 0.17); //in die y-Richtung
                    }
                    //pGraphContainer.AddElement((IElement)pTextElement, 0);
                    pLegendGroupElement.AddElement((IElement)pTextElement);

                    pbezTextElement = pTextElement; //beim Lieblingssohn entfernen
                    pTransform2D = (ITransform2D)pRectCloned;
                    pTransform2D.Move(x_offset, y_offset);
                    pElement = (IElement)pTransform2D;                  
                    pOrigin = new ESRI.ArcGIS.Geometry.Point();
                    pOrigin.PutCoords(pElement.Geometry.Envelope.XMax, pElement.Geometry.Envelope.YMax);

                    if ((zeilen > 2))
                    {
                        kastlhoehe_variabel = (0.3 * zeilen);

                        if ((Kastl_Typ != 1) && (Kastl_Typ != 7) && (Kastl_Typ != 8)) //1 - normales Kastl
                        {
                            pTransform2D.Scale(pOrigin, 1, kastlhoehe_variabel / Kastlhoehe);
                        }
                    }
                    else
                    {
                        kastlhoehe_variabel = Kastlhoehe;
                    }

                    if (Kastl_Typ == 3)//Gruppenkind
                    {
                        //Gruppen-Kind verschmälern und Vater verlängern
                        pTransform2D.Scale(pOrigin, 0.85, 1);
                        p2Element = (IElement)chefTransform2D;
                        chefTransform2D.Scale(chefOrigin, 1, kastlhoehe_variabel / p2Element.Geometry.Envelope.Height + 1);
                    }
                    #endregion



                    #region Legendenkästchen schreiben:

                    pxSource = (IClone)pTransform2D;
                    pxCloned = pxSource.Clone();
                    pFillShapeElement = (IFillShapeElement)pxCloned;
                    leg_pos_count = leg_pos_count + 1;
                    if (fillsym.Length > 3) //then '3
                    {
                        if (fillsym.PartString(0, 4) != "XXXX")//ab Position1 und 4 Stellen lang
                        {
                            //Hintergrundfarbe ermitteln und pFillShapeElemnet zuordnen
                            Cyan1 = fillsym.PartString(0, 1);
                            //MessageBox.Show(Cyan1);
                            Magenta1 = fillsym.PartString(1, 1);
                            Yellow1 = fillsym.PartString(2, 1);
                            Black1 = fillsym.PartString(3, 1);
                            pCmyk = new CmykColor
                            {
                                Cyan = Hintergrundfarbe(Cyan1),
                                Magenta = Hintergrundfarbe(Magenta1),
                                Yellow = Hintergrundfarbe(Yellow1),
                                Black = Hintergrundfarbe(Black1)
                            };

                            pSFS = new SimpleFillSymbol
                            {
                                Color = pCmyk,
                                Outline = pFillShapeElement.Symbol.Outline
                            };

                            if ((grafik.PartString(0, 1) != "F") && (geom.Contains("F") != false))
                            {
                                pFillShapeElement.Symbol = pSFS;
                            }//end if
                            pFS_grafik = new SimpleFillSymbol();
                            pFS_grafik = pSFS;
                        }

                        if (fillsym.Length > 10 && geom.Contains("F") != false) //then '2
                        {
                            //Flächenmuster ermitteln und zuordnen
                            //string mustername = fillsym.Substring(4, 7);//ab Position 5 und 7 Stellen lang
                            string mustername = fillsym.PartString(4, 7);//ab Position 5 und 7 Stellen lang
                            pEnumStyleGalleryItem = pStyleGallery.get_Items("Fill Symbols", styleSet, null);//definion der style gallery auf vb-page 11
                            pEnumStyleGalleryItem.Reset();//neu
                            pStyleGalleryItem = pEnumStyleGalleryItem.Next();
                            /*while (pStyleGalleryItem.Name == mustername || pStyleGalleryItem != null)//bricht ja dann mal ab, fall der Mustername nicht existiert
                            {
                                pMultiLayerFillSymbol = (IMultiLayerFillSymbol)pStyleGalleryItem.Item;
                            }*/
                            while (pStyleGalleryItem != null && pStyleGalleryItem.Name != mustername)//bricht ja dann mal ab, fall der Mustername nicht existiert
                            {
                                //if (pStyleGalleryItem.Name == mustername)
                                //{
                                //    break;//while-Schleife wird beendet, falls die Namen überein stimmen!
                                //}
                               pStyleGalleryItem = pEnumStyleGalleryItem.Next();
                            }
                            if (pStyleGalleryItem != null && pStyleGalleryItem.Name == mustername)
                            {
                                pMultiLayerFillSymbol = (IMultiLayerFillSymbol)pStyleGalleryItem.Item;
                            }
                            else //wenn das Stylefile den definierten Musternamen nicht enthält: Suche des Musternamens in alternativen Stylefiles:
                            {
                                //alternative Stylefiles verwenden:
                                for (int m = 0; m < pStyleGalleryStorage.FileCount; m++)
                                {
                                    //MessageBox.Show(pStyleGalleryStorage.get_File(i));
                                    styleSet = pStyleGalleryStorage.get_File(m);
                                    pEnumStyleGalleryItem = pStyleGallery.get_Items("Fill Symbols", styleSet, null);
                                    pEnumStyleGalleryItem.Reset();//neu
                                    pStyleGalleryItem = pEnumStyleGalleryItem.Next();

                                    //solange bis der Mustername im Style File gefunden wurde:
                                    while (pStyleGalleryItem != null && pStyleGalleryItem.Name != mustername)//bricht ja dann mal ab, fall der Mustername nicht existiert
                                    {
                                        //if (pStyleGalleryItem.Name == mustername)
                                        //{
                                        //    break;//while-Schleife wird beendet, falls die Namen überein stimmen!
                                        //}
                                        pStyleGalleryItem = pEnumStyleGalleryItem.Next();
                                    }
                                    if (pStyleGalleryItem != null && pStyleGalleryItem.Name == mustername)
                                    {
                                        //wenn die Namen übereinstimmen, dann wird das MultiLayerFillSymbol zugewiesen!!!
                                        pMultiLayerFillSymbol = (IMultiLayerFillSymbol)pStyleGalleryItem.Item;
                                        break;//for-Schleife wird beendet, falls die Namen überein stimmen!
                                    }

                                }//for-Schleife zu - wo durch alle Stylefiles durch iteriert wird!!!
                                if (pStyleGalleryItem == null) // && pStyleGalleryItem.Name != mustername)
                                {
                                    MessageBox.Show("Also the alternative stylefiles don't contain the fillSymbol pattern name " + mustername +
                                        " which is defined in the legend table! Please add a correct stylefile to the StyleGalleryStorage!",
                                        "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                                    pLegendGeneratorForm.Cursor = System.Windows.Input.Cursors.Arrow;
                                    pd.Close();
                                    //exit the application:
                                    pMxDoc.ActiveView.Refresh();
                                    return;
                                }
                            }

                            //pMultiLayerFillSymbol = (IMultiLayerFillSymbol)pStyleGalleryItem.Item;
                            //wenn eine Musterfarbe definiert wird:
                            if (fillsym.Length > 13)//Then '1
                            {
                                string musterfarbe = fillsym.PartString(11, 3);//ab Position 12 und 3 Stellen lang
                                if (musterfarbe == "ROT" || musterfarbe == "RED")
                                { p2Cmyk = pColorArot; }
                                else if (musterfarbe == "BLA" || musterfarbe == "BLU")
                                { p2Cmyk = pColorAblau; }
                                else if (musterfarbe == "GRN")
                                { p2Cmyk = pColorAgruen; }
                                else if (musterfarbe == "BRN")
                                { p2Cmyk = pColorAbraun; }
                                else if (musterfarbe == "GRA" || musterfarbe == "GRY")
                                { p2Cmyk = pColorGrau; }
                                else if (musterfarbe == "CYN")
                                { p2Cmyk = pColorCyan; }
                                else if (musterfarbe == "MGT")
                                { p2Cmyk = pColorMagenta; }
                                else if (musterfarbe == "GLB" || musterfarbe == "YLW")
                                { p2Cmyk = pColorGelb; }
                                else if (musterfarbe == "ORA")
                                { p2Cmyk = pColorOrange; }
                                else if (musterfarbe == "BLK")
                                { p2Cmyk = pColorBlack; }
                                else if (musterfarbe == "WHT")
                                { p2Cmyk = pColorWhite; }
                                else
                                {
                                    MessageBox.Show("Error at ID: " + gen_ID + "Musterfarbe is not defined");
                                }//endif aller if und else
                                for (int j = 0; j < pMultiLayerFillSymbol.LayerCount; j++)
                                {
                                    //Fill Color des Musternamens zuweisen:
                                    pMultiLayerFillSymbol.get_Layer(j).Color = p2Cmyk;
                                    //next j;
                                }
                            }//end if '1:                      Musterfarbe fertig definiert 

                            if (fillsym.PartString(0, 4) != "XXXX")
                            {
                                //pMultiLayerFillSymbol.AddLayer(pSFS);
                                //pMultiLayerFillSymbol.MoveLayer(pSFS, pMultiLayerFillSymbol.LayerCount - 1);
                                IFillSymbol pSFS_undermost = new SimpleFillSymbol
                                {
                                    Color = pSFS.Color,
                                    Outline = pLS_undermost
                                };
                                //'MsgBox "hallo"
                                pMultiLayerFillSymbol.AddLayer(pSFS_undermost);
                                pMultiLayerFillSymbol.MoveLayer(pSFS_undermost, pMultiLayerFillSymbol.LayerCount - 1);
                            }
                            pMultiLayerFillSymbol.Outline = pFillShapeElement.Symbol.Outline;
                            //if (grafik.Substring(0, 1) != "F")
                            if (grafik.PartString(0, 1) != "F")
                            {
                                pFillShapeElement.Symbol = pMultiLayerFillSymbol;
                            }
                            pFS_grafik = new MultiLayerFillSymbol();
                            pFS_grafik = pMultiLayerFillSymbol;
                        }//'2 zugemacht
                    }//'3 zugemacht


                    if (Kastl_Typ == 2)//Then 'Vater als chefTransform2D speichern
                    {
                        chefOrigin = pOrigin;
                        chefTransform2D = (ITransform2D)pFillShapeElement;
                    }// End If

                    if (leglab != "0")
                    {
                        if ((Kastl_Typ == 7))
                        {
                            mehrlingPosition = 0;                           
                            pFillShapeList.Add(mehrlingPosition, pFillShapeElement);
                            //mehrlingPosition = mehrlingPosition + 1;

                        }


                        else if ((Kastl_Typ == 8))//zweiter Mehrling
                        {
                            mehrlingPosition = mehrlingPosition + 1;
                            pFillShapeList.Add(mehrlingPosition, pFillShapeElement);
                            //if (geom.Contains("M") != true || grafik.Length > 1 || geom.Contains("L") != true)
                            //{
                                //mehrlingPosition = mehrlingPosition + 1;
                            //}
                        }

                        else
                        {
                           //pGraphContainer.AddElement((IElement)pFillShapeElement, 0);                          
                            pLegendGroupElement.AddElement((IElement)pFillShapeElement);

                            if (grafik == "H" && mehrling == false)
                            {
                                p3Element = (IElement)pTransform2D; //siehe vb-page 8 for p3Element = IElement
                                pTransform2D.Scale(pOrigin, 0.5, 1);
                                pTransform2D.Move(-(p3Element.Geometry.Envelope.Width), 0);

                                pxSource = (IClone)pTransform2D;
                                p2xCloned = pxSource.Clone();
                                p2FillShapeElement = (IFillShapeElement)p2xCloned;
                                pSFS = new SimpleFillSymbol
                                {
                                    Color = pColorWhite,
                                    Outline = pFillShapeElement.Symbol.Outline
                                };
                                p2FillShapeElement.Symbol = pSFS;
                                //pGraphContainer.AddElement((IElement)p2FillShapeElement, 0);
                                pLegendGroupElement.AddElement((IElement)p2FillShapeElement);
                            }
                        }
                    }//End If



                    #endregion

                    #region grafik setzen :

                    if (grafik.Length > 1)
                    {
                        pTransform2D = (ITransform2D)pGraCloned;
                        pTransform2D.Move(x_offset, y_offset);
                        pTextElement = (ITextElement)pTransform2D;
                        pTextElement.Text = grafik.PartString(1, 1);//Position '2' des Strings
                        if (grafik.Length > 2)
                        {
                            string musterfarbe = grafik.PartString(2, 3);//ab Position 3 und 3 Stellen lang!
                            pxSource = (IClone)pTextElement.Symbol;
                            pxCloned = pxSource.Clone();
                            pTextSymbol = (ITextSymbol)pxCloned;
                            p2Cmyk = new CmykColor();
                            if (musterfarbe == "ROT" || musterfarbe == "RED")
                            {
                                p2Cmyk = pColorArot;
                            }
                            else if (musterfarbe == "BLA" || musterfarbe == "BLU")
                            {
                                p2Cmyk = pColorAblau;
                            }
                            else if (musterfarbe == "GRN")
                            {
                                p2Cmyk = pColorAgruen;
                            }
                            else if (musterfarbe == "BRN")
                            {
                                p2Cmyk = pColorAbraun;
                            }
                            else if (musterfarbe == "GRA" || musterfarbe == "GRY")
                            {
                                p2Cmyk = pColorGrau;
                            }
                            else if (musterfarbe == "CYN")
                            {
                                p2Cmyk = pColorCyan;
                            }
                            else if (musterfarbe == "MGT")
                            {
                                p2Cmyk = pColorMagenta;
                            }
                            else if (musterfarbe == "GLB" || musterfarbe == "YLW")
                            {
                                p2Cmyk = pColorGelb;
                            }
                            else if (musterfarbe == "ORA")
                            {
                                p2Cmyk = pColorOrange;
                            }
                            else if (musterfarbe == "BLK")
                            {
                                p2Cmyk = pColorBlack;
                            }
                            else if (musterfarbe == "WHT")
                            {
                                p2Cmyk = pColorWhite;
                            }
                            else if ((musterfarbe == "XXX") && (fillsym.Length > 3))
                            {
                            }
                            else
                            {
                                MessageBox.Show("ERROR at ID: " + gen_ID + " - pattern color " + musterfarbe + " not defined");
                                pLegendGeneratorForm.staLblMessage.Content = "ERROR at ID: " + gen_ID + " - pattern color " + musterfarbe + " not defined";
                                //Fehlertext = "ERROR at ID : " + gen_ID + "  - pattern color '" + musterfarbe + "' not defined";
                            }

                            if (musterfarbe == "XXX")
                            {
                                pSymSource = (IClone)pTextElement.Symbol;
                                pSymCloned = pSymSource.Clone();
                                //pFormattedTextSymbol = new TextSymbolClass();
                                pFormattedTextSymbol = (IFormattedTextSymbol)pSymCloned;
                                pFormattedTextSymbol.FillSymbol = pFS_grafik;
                                pTextElement.Symbol = pFormattedTextSymbol;
                            }
                            else
                            {
                                pTextSymbol.Color = p2Cmyk;
                                pTextElement.Symbol = pTextSymbol;
                            }
                        }
                        if (Kastl_Typ == 7)
                        {
                            //erster Mehrling:
                            mehrlingPosition = 0;
                            pGrafikTextElementList.Add(mehrlingPosition, pTextElement);
                            //mehrlingPosition = mehrlingPosition + 1;
                        }
                        else if (Kastl_Typ == 8)
                        {
                            pGrafikTextElementList.Add(mehrlingPosition, pTextElement);
                            //mehrlingPosition = mehrlingPosition + 1;
                        }
                        else
                        {
                            //pGraphContainer.AddElement((IElement)pTextElement, 0);
                            pLegendGroupElement.AddElement((IElement)pTextElement);
                        }
                    }


                    #endregion

                    #region Linie setzen:

                    pTransform2D = (ITransform2D)pLineCloned;
                    pTransform2D.Move(x_offset, y_offset);
                    if (Kastl_Typ == 3)
                    {
                        pTransform2D.Scale(pOrigin, 0.85, 1);
                    }
                    if (grafik == "H" && mehrling == false && leglab != "0")
                    {
                        pTransform2D.Scale(pOrigin, 0.5, 1);
                        pTransform2D.Move(-(p3Element.Geometry.Envelope.Width), 0);
                    }
                    pLineElement = (ILineElement)pTransform2D;

                    //If InStr(geom$, "L") <> 0 And Len(linesym$) > 6 And Left(grafik$, 1) <> "L" Then
                    if (geom.Contains("L") && linesym.Length > 6 && grafik.PartString(0, 1) != "L")
                    {
                        //Linentyp ermitteln und zuordnen:
                        string mustername = linesym.PartString(0, 7);//ab Position 1 und 7 Stellen lang
                        pEnumStyleGalleryItem = pStyleGallery.get_Items("Line Symbols", styleSet, null);
                        pEnumStyleGalleryItem.Reset();
                        pStyleGalleryItem = pEnumStyleGalleryItem.Next();

                        while (pStyleGalleryItem != null && pStyleGalleryItem.Name != mustername)//bricht ja dann mal ab, fall der Mustername nicht existiert
                        {
                            //if (pStyleGalleryItem.Name == mustername)
                            //{
                            //    break;//while-Schleife wird beendet, falls die Namen überein stimmen!
                            //}
                            pStyleGalleryItem = pEnumStyleGalleryItem.Next();
                        }
                        if (pStyleGalleryItem != null && pStyleGalleryItem.Name == mustername)
                        {
                            pMultiLayerLineSymbol = (IMultiLayerLineSymbol)pStyleGalleryItem.Item;
                        }

                        //wenn das Stylefile den definierten Musternamen nicht enthält: Suche des Musternamens in alternativen Stylefiles:
                        else 
                        {
                            //alternative Stylefiles verwenden:
                            for (int m = 0; m < pStyleGalleryStorage.FileCount; m++)
                            {
                                //MessageBox.Show(pStyleGalleryStorage.get_File(i));
                                styleSet = pStyleGalleryStorage.get_File(m);
                                pEnumStyleGalleryItem = pStyleGallery.get_Items("Line Symbols", styleSet, null);
                                pEnumStyleGalleryItem.Reset();//neu
                                pStyleGalleryItem = pEnumStyleGalleryItem.Next();

                                //solange bis der Mustername im Style File gefunden wurde:
                                while (pStyleGalleryItem != null && pStyleGalleryItem.Name != mustername)//bricht ja dann mal ab, fall der Mustername nicht existiert
                                {
                                    //if (pStyleGalleryItem.Name == mustername)
                                    //{
                                    //    break;//while-Schleife wird beendet, falls die Namen überein stimmen!
                                    //}
                                    pStyleGalleryItem = pEnumStyleGalleryItem.Next();
                                }
                                if (pStyleGalleryItem != null && pStyleGalleryItem.Name == mustername)
                                {
                                    //wenn die Namen übereinstimmen, dann wird das MultiLayerLineSymbol aus dem alternativen Stylefile zugewiesen!!!
                                    pMultiLayerLineSymbol = (IMultiLayerLineSymbol)pStyleGalleryItem.Item;
                                    break;//die äußere for-Schleife wird beendet, falls die Namen überein stimmen!
                                }

                            }//for-Schleife zu - wo durch alle Stylefiles durch iteriert wird!!!
                            if (pStyleGalleryItem == null)// && pStyleGalleryItem.Name != mustername)
                            {
                                MessageBox.Show("Also the alternative stylefiles don't contain the lineSymbol pattern name " + mustername +
                                    " which is defined in the legend table! Please add a correct stylefile to the StyleGalleryStorage!",
                                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                                pLegendGeneratorForm.Cursor = System.Windows.Input.Cursors.Arrow;
                                pd.Close();
                                //exit the application:
                                return;
                            }
                        }                    

                        //wenn eine Musterfarbe definiert wird:
                        if (linesym.Length > 9)//4
                        {
                            //markerfarbe ermitteln und zuordnen:
                            string musterfarbe = linesym.PartString(7, 3);//ab Position 8 und 3 Stellen lang
                            //neues Markeersymbol einfärben:
                            if (musterfarbe == "ROT" || musterfarbe == "RED")
                            { p2Cmyk = pColorArot; }
                            else if (musterfarbe == "BLA" || musterfarbe == "BLU")
                            { p2Cmyk = pColorAblau; }
                            else if (musterfarbe == "GRN")
                            { p2Cmyk = pColorAgruen; }
                            else if (musterfarbe == "BRN")
                            { p2Cmyk = pColorAbraun; }
                            else if (musterfarbe == "GRA" || musterfarbe == "GRY")
                            { p2Cmyk = pColorGrau; }
                            else if (musterfarbe == "CYN")
                            { p2Cmyk = pColorCyan; }
                            else if (musterfarbe == "MGT")
                            { p2Cmyk = pColorMagenta; }
                            else if (musterfarbe == "GLB" || musterfarbe == "YLW")
                            { p2Cmyk = pColorGelb; }
                            else if (musterfarbe == "ORA")
                            { p2Cmyk = pColorOrange; }
                            else if (musterfarbe == "BLK")
                            { p2Cmyk = pColorBlack; }
                            else if (musterfarbe == "WHT")
                            { p2Cmyk = pColorWhite; }
                            else
                            {
                                MessageBox.Show("Angegebene Musterfarbe ist in der Legendentabelle nicht definiert");
                            }
                            for (int j = 0; j < pMultiLayerLineSymbol.LayerCount; j++)
                            {
                                //Fill Color des Musternamens zuweisen:
                                pMultiLayerLineSymbol.get_Layer(j).Color = p2Cmyk;
                            }

                        }//4 zu if (linesym.Legth > 9)
                        pLineElement.Symbol = pMultiLayerLineSymbol;

                        //Linien Element.Symbol = pSymbol:
                        if ((Kastl_Typ == 7))
                        {
                            //pGroupElement.AddElement((IElement)pLineElement);
                            mehrlingPosition = 0;
                            pLineElementList.Add(mehrlingPosition, pLineElement);
                        }
                        else if ((Kastl_Typ == 8))
                        {
                            //pTransform2D.Move(farbdef_offset * (m_zaehler - 1), 0);
                            //pxSource = (IClone)pTransform2D;
                            //p2xCloned = pxSource.Clone();
                            //pLineElement2 = (ILineElement)p2xCloned;
                            //pLineElement2.Symbol = pLineElement.Symbol;
                            //pGroupElement.AddElement((IElement)pLineElement2);
                            pLineElementList.Add(mehrlingPosition, pLineElement);
                            
                        }
                        else
                        {
                            //pGraphContainer.AddElement((IElement)pLineElement, 0);
                            pLegendGroupElement.AddElement((IElement)pLineElement);
                        }
                    }//End If                    

                    #endregion

                   
                    #region marker setzen:
                    //Seite 26:
                    pTransform2D = (ITransform2D)pPointCloned;
                    pTransform2D.Move(x_offset, y_offset);
                    if (grafik == "H" && mehrling == false)
                    {
                        pTransform2D.Scale(pOrigin, 0.5, 1);

                        if (geom.Contains("F") == true)
                        {
                            pTransform2D.Move(-(p3Element.Geometry.Envelope.Width), 0);
                        }
                    }
                    pMarkerElement = (IMarkerElement)pTransform2D;

                    if (geom.Contains("M") == true && markersym.Length > 6 && grafik.PartString(0, 1) != "M")
                    {
                        //Markertytyp ermitteln und zuordnen:
                        string mustername = markersym.PartString(0, 7);//ab Position 1 und 7 Stellen lang
                        pEnumStyleGalleryItem = pStyleGallery.get_Items("Marker Symbols", styleSet, null);//geolba
                        pEnumStyleGalleryItem.Reset();
                        pStyleGalleryItem = pEnumStyleGalleryItem.Next();

                        while (pStyleGalleryItem != null && pStyleGalleryItem.Name != mustername)//bricht ja dann mal ab, fall der Mustername nicht existiert
                        {
                            //if (pStyleGalleryItem.Name == mustername)
                            //{
                            //    break;//while-Schleife wird beendet, falls die Namen überein stimmen!
                            //}
                            pStyleGalleryItem = pEnumStyleGalleryItem.Next();
                        }
                        if (pStyleGalleryItem != null)
                        {
                            pMultiLayerMarkerSymbol = (IMultiLayerMarkerSymbol)pStyleGalleryItem.Item;
                        }
                        else //wenn das Stylefile den definierten Musternamen nicht enthält: Suche des Musternamens in alternativen Stylefiles:
                        {
                            //alternative Stylefiles verwenden:
                            for (int m = 0; m < pStyleGalleryStorage.FileCount; m++)
                            {
                                //MessageBox.Show(pStyleGalleryStorage.get_File(i));
                                styleSet = pStyleGalleryStorage.get_File(m);
                                pEnumStyleGalleryItem = pStyleGallery.get_Items("Marker Symbols", styleSet, null);
                                pEnumStyleGalleryItem.Reset();//neu
                                pStyleGalleryItem = pEnumStyleGalleryItem.Next();

                                //solange bis der Mustername im Style File gefunden wurde:
                                while (pStyleGalleryItem != null && pStyleGalleryItem.Name != mustername)//bricht ja dann mal ab, fall der Mustername nicht existiert
                                {
                                    //if (pStyleGalleryItem.Name == mustername)
                                    //{
                                    //    break;//while-Schleife wird beendet, falls die Namen überein stimmen!
                                    //}
                                    pStyleGalleryItem = pEnumStyleGalleryItem.Next();
                                }
                                if (pStyleGalleryItem != null && pStyleGalleryItem.Name == mustername)
                                {
                                    //wenn die Namen übereinstimmen, dann wird das MultiLayerMarkerSymbol aus dem alternativen Stylefile zugewiesen!!!
                                    pMultiLayerMarkerSymbol = (IMultiLayerMarkerSymbol)pStyleGalleryItem.Item;
                                    break;//for-Schleife wird beendet, falls die Namen überein stimmen!
                                }

                            }//for-Schleife zu - wo durch alle Stylefiles durch iteriert wird!!!
                            if (pStyleGalleryItem == null) // && pStyleGalleryItem.Name != mustername)
                            {
                                MessageBox.Show("Also the alternative stylefiles don't contain the markerSymbol pattern name " + mustername +
                                    " which is defined in the legend table! Please add a correct stylefile to the StyleGalleryStorage!",
                                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                                pLegendGeneratorForm.Cursor = System.Windows.Input.Cursors.Arrow;
                                pd.Close();
                                //exit the application:
                                return;
                            }
                        }

                        //pMultiLayerMarkerSymbol = (IMultiLayerMarkerSymbol)pStyleGalleryItem.Item;
                        //Musterfarbe
                        if (markersym.Length > 9)//4
                        {
                            string musterfarbe = markersym.PartString(7, 3);//ab Position 8 und 3 Stellen lang
                            if (musterfarbe == "ROT" || musterfarbe == "RED")
                            { p2Cmyk = pColorArot; }
                            else if (musterfarbe == "BLA" || musterfarbe == "BLU")
                            { p2Cmyk = pColorAblau; }
                            else if (musterfarbe == "GRN")
                            { p2Cmyk = pColorAgruen; }
                            else if (musterfarbe == "BRN")
                            { p2Cmyk = pColorAbraun; }
                            else if (musterfarbe == "GRA" || musterfarbe == "GRY")
                            { p2Cmyk = pColorGrau; }
                            else if (musterfarbe == "CYN")
                            { p2Cmyk = pColorCyan; }
                            else if (musterfarbe == "MGT")
                            { p2Cmyk = pColorMagenta; }
                            else if (musterfarbe == "GLB" || musterfarbe == "YLW")
                            { p2Cmyk = pColorGelb; }
                            else if (musterfarbe == "ORA")
                            { p2Cmyk = pColorOrange; }
                            else if (musterfarbe == "BLK")
                            { p2Cmyk = pColorBlack; }
                            else if (musterfarbe == "WHT")
                            { p2Cmyk = pColorWhite; }
                            else
                            {
                                MessageBox.Show("ERROR at ID: " + gen_ID + " - pattern color " + musterfarbe + " not defined");
                                pLegendGeneratorForm.staLblMessage.Content = "ERROR at ID: " + gen_ID + " - pattern color " + musterfarbe + " not defined";
                            }
                            for (int j = 0; j < pMultiLayerMarkerSymbol.LayerCount; j++)
                            {
                                //Fill Color des Musternamens zuweisen:
                                pMultiLayerMarkerSymbol.get_Layer(j).Color = p2Cmyk;
                            }

                        }//4 zu
                        pMarkerElement.Symbol = pMultiLayerMarkerSymbol;

                        if ((Kastl_Typ == 7))//erster Mehrling
                        {
                            mehrlingPosition = 0;                            
                            //pMarkerElementList.Add(pMarkerElement);
                            pMarkerElementList.Add(mehrlingPosition, pMarkerElement);
                            //mehrlingPosition = mehrlingPosition + 1;
                        }
                        else if ((Kastl_Typ == 8))//zweiter Mehrling
                        {
                            pMarkerElementList.Add(mehrlingPosition, pMarkerElement);
                            //mehrlingPosition = mehrlingPosition + 1;
                        }

                        else
                        {
                            //- Multimarker Kastlhoehe 5,5mm
                            if (geom.Contains("MM") != false)
                            {
                                p2MrkSource = (IClone)pMarkerElement;
                                p2MrkCloned = p2MrkSource.Clone();
                                p2Tr = (ITransform2D)p2MrkCloned;
                                p2Tr.Move(-(Kastlhoehe / 5.5 * 1.8), Kastlhoehe / 5.5 * 1.1);
                                p2El = (IElement)p2Tr;
                                //pGraphContainer.AddElement(p2El, 0);
                                pLegendGroupElement.AddElement((IElement) p2El);

                                p2MrkSource = (IClone)pMarkerElement;
                                p2MrkCloned = p2MrkSource.Clone();
                                p2Tr = (ITransform2D)p2MrkCloned;
                                p2Tr.Move(Kastlhoehe / 5.5 * 1.2, Kastlhoehe / 5.5 * 0.7);
                                p2El = (IElement)p2Tr;
                                //pGraphContainer.AddElement(p2El, 0);
                                pLegendGroupElement.AddElement((IElement)p2El);

                                p2MrkSource = (IClone)pMarkerElement;
                                p2MrkCloned = p2MrkSource.Clone();
                                p2Tr = (ITransform2D)p2MrkCloned;
                                p2Tr.Move(-(Kastlhoehe / 5.5 * 0.6), -(Kastlhoehe / 5.5 * 1.4));
                                p2El = (IElement)p2Tr;
                                //pGraphContainer.AddElement(p2El, 0);
                                pLegendGroupElement.AddElement((IElement)p2El);
                            }
                            else
                            {
                                //pGraphContainer.AddElement((IElement)pMarkerElement, 0);
                                pLegendGroupElement.AddElement((IElement)pMarkerElement);
                            }
                        }
                    }//if geom.Contains("M")..... zu gemacht
                    

                    #endregion

                    #region Label schreiben:L_NUM //Nummerierung der Legendenkästchen
                    if (pLegendGeneratorForm.chkLegendennummer.IsChecked == true || pLegendGeneratorForm.chkFortlaufendeNummer.IsChecked == true)
                    {
                        pTransform2D = (ITransform2D)pLabCloned;
                        pTransform2D.Move(x_offset, y_offset);
                        pTextElement = (ITextElement)pTransform2D;
                         test = null;

                        if (pLegendGeneratorForm.chkFortlaufendeNummer.IsChecked == true)
                        {
                            //leglab = flNUM.ToString();
                            if (dictionary.ContainsKey(gen_ID))
                            {
                                test = dictionary[gen_ID];
                                leglab = dictionary[gen_ID];

                            }
                            if (Kastl_Typ == 6) //Then
                            {
                                leglab = "";
                                flNUM = flNUM - 1;
                            }
                            //else
                            //{
                            //    //write the value into the text file:
                            //    try
                            //    {
                            //        sw.WriteLine(flNUM.ToString() + " ,L_ID = " + gen_ID);
                            //    }
                            //    catch (Exception ex)
                            //    {
                            //        MessageBox.Show(ex.Message);
                            //    }
                            //}//end if
                            flNUM = flNUM + 1;

                        }
                        else
                        {
                            if (Kastl_Typ == 6)
                            {
                                leglab = "";
                            }
                        }

                        pTextElement.Text = leglab;
                        if (leglab != string.Empty)
                        {
                            if (pLegendGeneratorForm.chkAllowNullLegendNumber.IsChecked == true && leglab == "0")
                            {
                                if (Kastl_Typ == 7)//erster Mehrling
                                {
                                    //pGroupElement.AddElement((IElement) pTextElement);
                                    pTextElementList.Add(pTextElement);
                                }
                                else if (Kastl_Typ == 8)//zweiter Mehrling
                                {
                                    pTextElementList.Add(pTextElement);
                                }
                                else
                                {
                                    //pGraphContainer.AddElement((IElement)pTextElement, 0);
                                    pLegendGroupElement.AddElement((IElement)pTextElement);
                                }
                            }
                            else if (pLegendGeneratorForm.chkAllowNullLegendNumber.IsChecked == false && leglab != "0")
                            {
                                if (Kastl_Typ == 7)//erster Mehrling
                                {
                                    //pGroupElement.AddElement((IElement) pTextElement);
                                    pTextElementList.Add(pTextElement);
                                }
                                else if (Kastl_Typ == 8)//zweiter Mehrling
                                {
                                    pTextElementList.Add(pTextElement);
                                }
                                else
                                {
                                    //pGraphContainer.AddElement((IElement)pTextElement, 0);
                                    pLegendGroupElement.AddElement((IElement)pTextElement);
                                }
                            }
                        }
                    }
                    #endregion



                    #region Farbdefinition schreiben:
                    //falls aktiviert:
                    if (pLegendGeneratorForm.chkFarbwerte.IsChecked == true)
                    {
                        pLabCloned = pLabSource.Clone();
                        pTransform2D = (ITransform2D)pLabCloned;
                        pTransform2D.Move(x_offset - farbdef_offset - 0.8, y_offset + 0.3);
                        pTransform2D.Scale(pOrigin, 0.7, 0.7);
                        pTextElement = (ITextElement)pTransform2D;
                        pTextElement.Text = fillsym + '\n' + linesym + '\n' + markersym;
                        if (Kastl_Typ == 7 || Kastl_Typ == 8)
                        {
                            //List<ITextElement> pFarbeTextElementList = new List<ITextElement>();
                            pFarbeTextElementList.Add(pTextElement);
                        }
                        else if (Kastl_Typ != 6)
                        {
                            //pGraphContainer.AddElement((IElement)pTextElement, 0);
                            pLegendGroupElement.AddElement((IElement)pTextElement);
                           
                        }
                    }

                    //Notiz schreiben falls vorhanden: also gecheckt und keine Raute: NOTES im Formular und DB
                    if (pLegendGeneratorForm.chkNotizenfeld.IsChecked == true && refS != "#")
                    {
                        pRefCloned = pBezSource.Clone();
                        pTransform2D = (ITransform2D)pRefCloned;
                        try
                        {
                            pTransform2D.Move(x_offset + Convert.ToInt32(pLegendGeneratorForm.notoff.Text), y_offset);
                        }
                        catch
                        {
                            MessageBox.Show("The variable 'notes offset' wasn't defined as number");
                            pd.Close();
                            return;
                        }
                        pTextElement = (ITextElement)pTransform2D;

                        //Code vom alten LegendGenerator, mach aber keinen Sinn:
                        //warum Überschrift aufkumulieren bei den Lieblingssöhnen und deren Legendennummer löschen:???
                        //if (Kastl_Typ == 6)//Lieblingssohn
                        //{
                        //    refS = not_vater + ", " + refS;// Überschrift kumliert sich nur auf
                        //    pGraphContainer.DeleteElement((IElement)prefTextElement); //legendennummer löschen
                        //}
                        if (Kastl_Typ == 8)
                        {
                            refS = not_vater + " / " + refS; //Mehrlinge
                            //pGraphContainer.DeleteElement((IElement)prefTextElement);
                            pLegendGroupElement.DeleteElement((IElement)prefTextElement);
                        }
                        not_vater = refS;

                        pTextElement.Text = Zeilenumbruch(refS, zeile_bez);

                        if (zeilen > 1)
                        {
                            pTransform2D.Move(0, 0.17);
                        }
                        //pGraphContainer.AddElement((IElement)pTextElement, 0);
                        pLegendGroupElement.AddElement((IElement)pTextElement);
                    }

                    #endregion

                    prefTextElement = pTextElement; //beim Lieblingssohn entfernen

                    #region Mehrlimgsgruppe Kastltyp 7 und 8
                    //Mehrlingsgruppe abschließen und schreiben:
                    if (Kastl_Typ == 8 && (gruppe != neu_gruppe))// && gruppe_alt != gruppe)// && gruppe_alt != gruppe) //gruppe_alt -> siehe seite 12; gruppe (als empty string) ist eh vor der ersten while Schleife; mehrling is laut def false
                    {

                        int anzahlMehrlinge = pFillShapeList.Count;
                        //pGraphContainer.AddElement((IElement)pRectCloned, 0);//funktioniert
                        //pGraphContainer.AddElement((IElement)pRectangleElement, 0);
                        IElement element = (IElement)pRectCloned;
                        IElement ppElement;
                        //pFillShapeList.Clear();
                        WKSEnvelope nPatch;
                        IEnvelope pFillEnv;
                        IGeometry pGeometry;
                        nPatch.XMin = element.Geometry.Envelope.XMin;
                        nPatch.YMax = element.Geometry.Envelope.YMin + Kastlhoehe;
                        nPatch.XMax = element.Geometry.Envelope.XMin + (farbdef_offset / anzahlMehrlinge);
                        nPatch.YMin = element.Geometry.Envelope.YMin;

                        //Legendenkästchen der Mehrlinge setzen:
                        for (int a = 0; a < pFillShapeList.Count; a++)
                        {
                            if (a > 0)
                            {
                                nPatch.XMin = nPatch.XMin + (farbdef_offset / anzahlMehrlinge);
                                nPatch.XMax = nPatch.XMax + (farbdef_offset / anzahlMehrlinge);
                            }
                            //pFillEnv = new EnvelopeClass();
                            pFillEnv = new Envelope() as IEnvelope;
                            pFillEnv.PutCoords(nPatch.XMin, nPatch.YMin, nPatch.XMax, nPatch.YMax);
                            pGeometry = pFillEnv;
                            ppElement = (IElement)pFillShapeList.Values.ElementAt(a);
                            ppElement.Geometry = pGeometry;
                            //pGraphContainer.AddElement(ppElement, 0);
                            pLegendGroupElement.AddElement((IElement)ppElement);

                        }

                        

                        double moveXt = farbdef_offset / anzahlMehrlinge;
                        double halbeMoveXt = moveXt / 2;
                        double halbeMoveYt = Kastlhoehe / 2;
                        //IPoint tempOrigin = new PointClass();
                        IPoint tempOrigin = new ESRI.ArcGIS.Geometry.Point();
                        // für die einzelnen Linien:
                        for (int m = 0; m < pLineElementList.Count; m++)
                        {
                            anzahlMehrlinge = pFillShapeList.Count;                         
                            IElement plElement = (IElement)pLineElementList.Values.ElementAt(m);

                            //ppElement = (IElement)pFillShapeList[pMarkerElementList.ElementAt(m)];
                            ppElement = (IElement)pFillShapeList.Values.ElementAt(pLineElementList.Keys.ElementAt(m));
                            double tempXMax = ppElement.Geometry.Envelope.XMax;
                            double tempYMax = ppElement.Geometry.Envelope.YMax;                          

                            tempOrigin.X = tempXMax;
                            tempOrigin.Y = tempYMax;                          

                            if (plElement != null)
                            {
                                double sx = 1 / (double) anzahlMehrlinge;

                                pxSource = (IClone)plElement;
                                pLineCloned = pxSource.Clone();
                                pTransform2D = (ITransform2D)pLineCloned;
                                //abhängig von der Anzahl der Mehrlinge wir die x-Größe skaliert:
                                pTransform2D.Scale(pOrigin ,sx, 1);
                                pTransform2D.Move(-(pOrigin.X-tempOrigin.X), 0);
                                //pGraphContainer.AddElement((IElement)pTransform2D, 0);
                                pLegendGroupElement.AddElement((IElement)pTransform2D);
                            }

                        }

                        // für die einzelnen Marker:
                        for (int m = 0; m < pMarkerElementList.Count; m++)
                        {
                            anzahlMehrlinge = pFillShapeList.Count;

                            //double moveX = farbdef_offset / anzahlMehrlinge;
                            //double halbeMoveX = moveX / 2;
                            //double halbeMoveY = Kastlhoehe / 2;

                            IElement pmElement = (IElement)pMarkerElementList.Values.ElementAt(m);

                            //ppElement = (IElement)pFillShapeList[pMarkerElementList.ElementAt(m)];
                            ppElement = (IElement)pFillShapeList.Values.ElementAt(pMarkerElementList.Keys.ElementAt(m));
                            double tempXMax = ppElement.Geometry.Envelope.XMax;
                            double tempYMax = ppElement.Geometry.Envelope.YMax;

                            tempOrigin.X = tempXMax + ((anzahlMehrlinge-1) * halbeMoveXt);
                            tempOrigin.Y = tempYMax;

                            if (pmElement != null)
                            {
                                pxSource = (IClone)pmElement;
                                pxCloned = pxSource.Clone();
                                pMMarkerTransform = (ITransform2D)pxCloned;
                                
                                pMMarkerTransform.Scale(pOrigin, 1, 1);
                                pMMarkerTransform.Move(-(pOrigin.X - tempOrigin.X), 0);
                                //pGraphContainer.AddElement((IElement)pMMarkerTransform, 0);
                                pLegendGroupElement.AddElement((IElement)pMMarkerTransform);
                            }

                        }

                        //Grafik setzen für die einzelnen Mehrlinge:
                        IClone pGrafikCloned;
                        for (int m = 0; m < pGrafikTextElementList.Count; m++)
                        {
                            anzahlMehrlinge = pFillShapeList.Count;
                            
                            IElement pgElement = (IElement)pGrafikTextElementList.Values.ElementAt(m);
                            //ppElement = (IElement)pFillShapeList[pMarkerElementList.ElementAt(m)];
                            ppElement = (IElement)pFillShapeList.Values.ElementAt(pGrafikTextElementList.Keys.ElementAt(m));
                            double tempXMax = ppElement.Geometry.Envelope.XMax;
                            double tempYMax = ppElement.Geometry.Envelope.YMax;

                            //tempOrigin.X = tempXMax;
                            //tempOrigin.Y = tempYMax;
                            tempOrigin.X = tempXMax + ((anzahlMehrlinge - 1) * halbeMoveXt);
                            tempOrigin.Y = tempYMax;

                            if (pgElement != null)
                            {
                                //double sx = 1 / (double)anzahlMehrlinge;//Grafiken doch nicht skalieren

                                pxSource = (IClone)pgElement;
                                pGrafikCloned = pxSource.Clone();
                                pTransform2D = (ITransform2D)pGrafikCloned;

                                //pTransform2D.Scale(pOrigin, sx, sx);//Grafiken doch nicht verkleinern!!!
                                //pTransform2D.Move(-(pOrigin.X - tempOrigin.X), 0);
                                ////pGraphContainer.AddElement((IElement)pTransform2D, 0);
                                //pGroupElement.AddElement((IElement)pTransform2D);

                                pTransform2D.Scale(pOrigin, 1, 1);
                                pTransform2D.Move(-(pOrigin.X - tempOrigin.X), 0);
                                //pGraphContainer.AddElement((IElement)pMMarkerTransform, 0);
                                pLegendGroupElement.AddElement((IElement)pTransform2D);
                            }

                        }

                        //Label schreiben:L_NUM //Nummerierung der Legendenkästchen
                        int iAndereTextReihenfolge = pTextElementList.Count - 1;
                        for (int i = 0; i < pTextElementList.Count; i++)
                        {
                           
                            double moveX = farbdef_offset / anzahlMehrlinge;

                            IElement ptElement = (IElement)pTextElementList[i];
                            pxSource = (IClone)ptElement;
                            pxCloned = pxSource.Clone();
                            pTransform2D = (ITransform2D)pxCloned;
                            //pTransform2D.Move(-(moveX * iAndereTextReihenfolge) + 0.09, 0);
                            pTransform2D.Move(-(moveX * iAndereTextReihenfolge) + 0.045, 0);

                            pxSource = (IClone)pTransform2D;
                            p2xCloned = pxSource.Clone();
                            pTextElement2 = (ITextElement)p2xCloned;
                            pTextElement2.Symbol = pTextElement.Symbol;
                            //pGraphContainer.AddElement((IElement)pTextElement2, 0);
                            pLegendGroupElement.AddElement((IElement)pTextElement2);

                            iAndereTextReihenfolge = iAndereTextReihenfolge - 1;
                        }

                       
                       

                        //Farbwerte notieren für Mehrlinge:
                        for (int i = 0; i < pFarbeTextElementList.Count; i++)
                        {
                            IElement pfElement = (IElement)pFarbeTextElementList[i];


                            pxSource = (IClone)pfElement;
                            pxCloned = pxSource.Clone();
                            pTransform2D = (ITransform2D)pxCloned;
                            pTransform2D.Move(-i, 0);

                            pxSource = (IClone)pTransform2D;
                            p2xCloned = pxSource.Clone();
                            pTextElement2 = (ITextElement)p2xCloned;
                            pTextElement2.Symbol = pTextElement.Symbol;
                            //pGraphContainer.AddElement((IElement)pTextElement2, 0);
                            pLegendGroupElement.AddElement((IElement)pTextElement2);
                        }

                        pFarbeTextElementList.Clear();
                        pGrafikTextElementList.Clear();
                        pTextElementList.Clear();
                        pMarkerElementList.Clear();
                        pLineElementList.Clear();
                        pFillShapeList.Clear();
                        mehrling = false; //Bool Variable wieder auf 0 setzen;
                        m_zaehler = 0;
                        anzahlMehrlinge = 0;

                    }//End If:  if ((mehrling == true || Kastl_Typ == 7) && gruppe_alt != gruppe)
                    #endregion


                    #region gif eport:
                    //*************************************************************************
                    //'GIF-EXPORT

                    if (pLegendGeneratorForm.FormData.ChkGraphicExport == true)
                    {

                        IActiveView pActiveView2 = (IActiveView)pMxDoc.PageLayout;
                        pPageLayout = pMxDoc.PageLayout;
                        pPageLayout.Page.Units = esriUnits.esriCentimeters; //pPageLayout.Page.Units = 8 ?????
                        pGraphContainer = (IGraphicsContainer)pPageLayout;


                        //IActiveView pActiveView = (IActiveView)pMxDoc.PageLayout;
                        exportRect.left = 0;
                        exportRect.top = 0;
                        exportRect.right = iWidthPixels;
                        exportRect.bottom = iHeightPixels;
                        //pPixelBoundsEnv = new EnvelopeClass();
                        pPixelBoundsEnv = new Envelope() as IEnvelope;
                        pPixelBoundsEnv.PutCoords(exportRect.left, exportRect.top, exportRect.right, exportRect.bottom);
                        pExport.PixelBounds = pPixelBoundsEnv;
                        //pExportEnv = new EnvelopeClass();
                        pExportEnv = new Envelope() as IEnvelope;

                        if (pExportFileDir != String.Empty)
                        {
                            pExport.ExportFileName = pExportFileDir + "\\" + gen_ID + ".gif";
                            pExportEnv.PutCoords(xmin_GE + x_offset, ymin_GE + y_offset, xmax_GE + x_offset, ymax_GE + y_offset);
                            //pExportEnv.PutCoords(0, 0, 1.1, 0.55);//Kästchengröße
                            hDC = pExport.StartExporting();
                            pActiveView2.Output((int)hDC, (int)pExport.Resolution, exportRect, pExportEnv, null);
                            pExport.FinishExporting();
                            pExport.Cleanup();

                        }
                    }
                    //'**************************************************************************
                    #endregion                   

                }//Ende der if-Schleife auf vb-code-gage seite 28/////////////////////////////////////////////
                if (pd.IsEnabled == false)
                {
                    pLegendGeneratorForm.staLblMessage.Content = "The rendering process has been canceled!";
                    return;
                }
                GC.Collect();//EsriDatenbankobjekte zerstören
            }//Ende der while Schleife    
            
            #region letzte Klammern schließen:

            //kl1 schließen:
            if (kl1.Length > 2)//Then
            {
                ykla1_ende = y_offset - kastlhoehe_variabel;
                //On Error GoTo Templatefehler
                pKlaLineCloned = pKlaLineSource.Clone();
                pTransform2D = (ITransform2D)pKlaLineCloned;
                pTransform2D.Move(x_offset, ykla1_anfang);
                pElement = (IElement)pTransform2D;               
                pOrigin = new ESRI.ArcGIS.Geometry.Point();
                pOrigin.PutCoords(pElement.Geometry.Envelope.XMax, pElement.Geometry.Envelope.YMax);

                pTransform2D.Scale(pOrigin, 1, (ykla1_anfang - ykla1_ende) / Kastlhoehe);
                pLineElement = (ILineElement)pTransform2D;
                //pGraphContainer.AddElement((IElement)pLineElement, 0);
                pLegendGroupElement.AddElement((IElement)pLineElement);
                //On Error GoTo Templatefehler
                pKlaCloned = pKlaSource.Clone();
                pTransform2D = (ITransform2D)pKlaCloned;
                pTextElement = (ITextElement)pTransform2D;
                pTextElement.Text = Zeilenumbruch(kl1_alt, zeile_bez);
                pTransform2D.Move(x_offset, ykla1_anfang + kastlabstand - (ykla1_anfang - ykla1_ende) / 2);
                //pGraphContainer.AddElement((IElement)pTextElement, 0);
                pLegendGroupElement.AddElement((IElement)pTextElement);
            }//End If

            //kl2 schließen:
            if (kl2.Length > 2)//Then
            {
                ykla2_ende = y_offset - kastlhoehe_variabel;
                //On Error GoTo Templatefehler
                pKlaLineCloned = pKlaLineSource.Clone();
                pTransform2D = (ITransform2D)pKlaLineCloned;
                pTransform2D.Move(x_offset + klammerabstand_feld, ykla2_anfang);
                pElement = (IElement)pTransform2D;              
                pOrigin = new ESRI.ArcGIS.Geometry.Point();
                pOrigin.PutCoords(pElement.Geometry.Envelope.XMax, pElement.Geometry.Envelope.YMax);

                pTransform2D.Scale(pOrigin, 1, (ykla2_anfang - ykla2_ende) / Kastlhoehe);
                pLineElement = (ILineElement)pTransform2D;
                //pGraphContainer.AddElement((IElement)pLineElement, 0);
                pLegendGroupElement.AddElement((IElement)pLineElement);
                //On Error GoTo Templatefehler
                pKlaCloned = pKlaSource.Clone();
                pTransform2D = (ITransform2D)pKlaCloned;
                pTextElement = (ITextElement)pTransform2D;
                pTextElement.Text = Zeilenumbruch(kl2_alt, zeile_bez);
                pTransform2D.Move(x_offset + klammerabstand_feld, ykla2_anfang + kastlabstand - (ykla2_anfang - ykla2_ende) / 2);
                //pGraphContainer.AddElement((IElement)pTextElement, 0);
                pLegendGroupElement.AddElement((IElement)pTextElement);
            }//End If

            //kl3 schließen:
            if (kl2.Length > 2)//Then
            {
                ykla3_ende = y_offset - kastlhoehe_variabel;
                //On Error GoTo Templatefehler
                pKlaLineCloned = pKlaLineSource.Clone();
                pTransform2D = (ITransform2D)pKlaLineCloned;
                pTransform2D.Move(x_offset + klammerabstand_feld * 2, ykla3_anfang);
                pElement = (IElement)pTransform2D;
                pOrigin = new ESRI.ArcGIS.Geometry.Point();
                pOrigin.PutCoords(pElement.Geometry.Envelope.XMax, pElement.Geometry.Envelope.YMax);

                pTransform2D.Scale(pOrigin, 1, (ykla3_anfang - ykla3_ende) / Kastlhoehe);
                pLineElement = (ILineElement)pTransform2D;
                //pGraphContainer.AddElement((IElement)pLineElement, 0);
                pLegendGroupElement.AddElement((IElement)pLineElement);
                //On Error GoTo Templatefehler
                pKlaCloned = pKlaSource.Clone();
                pTransform2D = (ITransform2D)pKlaCloned;
                pTextElement = (ITextElement)pTransform2D;
                pTextElement.Text = Zeilenumbruch(kl3_alt, zeile_bez);
                pTransform2D.Move(x_offset + klammerabstand_feld * 2, ykla3_anfang + kastlabstand - (ykla3_anfang - ykla3_ende) / 2);
                //pGraphContainer.AddElement((IElement)pTextElement, 0);
                pLegendGroupElement.AddElement((IElement)pTextElement);
            }//End If


            #endregion

            
            if (pLegendGroupElement.ElementCount != 0)
            {
                pGraphContainer.AddElement((IElement)pLegendGroupElement, 0);
            }
            IPageLayout docPageLayout;
            docPageLayout = pMxDoc.PageLayout;
            IGraphicsContainerSelect docPageLayoutGraphicsSelect;
            docPageLayoutGraphicsSelect = docPageLayout as IGraphicsContainerSelect;
            docPageLayoutGraphicsSelect.UnselectAllElements();

            


            //refresh map and toc:            
            pMxDoc.UpdateContents();
            pMxDoc.ActiveView.Refresh();
            // Rfresh the map
            //MessageBox.Show("A new legend was created in the page-layout!", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
            pLegendGeneratorForm.staLblMessage.Content = "A new legend was created in the page-layout!";
            //pLegendGeneratorForm.Cursor = System.Windows.Input.Cursors.Arrow;
            //ESRI Datenbankobjekte explizit zerstören!!! 
            GC.Collect();
            //ProgressDialog wieder schließen:s
            pd.Close();

        }

        #region delegates:

        //our delegate used for updating the UI
        public delegate void UpdateProgressDelegate(int percentage, int recordCount, int value);
        //this is the method that the deleagte will execute
        public void UpdateProgressText(int percentage, int recordCount, int value)
        {
            //set our progress dialog text and value
            if (recordCount - 1 > value)
            {
                pd.ProgressText = string.Format("{0}% of {1} features are processed! ", percentage.ToString(), recordCount);
                //pd.ProgressValue = percentage;          
            }
            else
            {
                pd.ProgressText = string.Format("The Page Layout is now updating");
            }
        }

        //Create a Delegate that matches the Signature of the ProgressBar's SetValue method          
        private delegate void UpdateProgressBarDelegate(System.Windows.DependencyProperty dp, System.Object value);

        #endregion
        
        #region private class methods: helper methods:

        private bool CheckFehler(string legendentabelle, string leg_tab_pfad)
        {
            bool Fehler = false;
            if (legendentabelle == String.Empty)
            {
                MessageBox.Show("LEGENDTABLE is not defined");
                pLegendGeneratorForm.staLblMessage.Content = "LEGENDTABLE is not defined!";
                Fehler = true;
            }

            if (leg_tab_pfad == String.Empty && pLegendGeneratorForm.chkAccess.IsChecked == true)         
            {
                MessageBox.Show("Access-database is not defined");
                pLegendGeneratorForm.staLblMessage.Content = "Access-database is not defined!";
                Fehler = true;
            }
            return Fehler;

        }

        //Create CMYK Color from Cyan, Magenta, Yellow, Black value:
        private ICmykColor GetCMYKColor(int c, int m, int y, int k)
        {
            ICmykColor cmykColor = new CmykColor
            {
                Cyan = c,
                Magenta = m,
                Yellow = y,
                Black = k
            };
            return cmykColor;
        }

        private int Hintergrundfarbe(string bgc_str)
        {
            int bgc_int;// = 0;
            if (bgc_str == "S")
                bgc_int = 6;
            else if (bgc_str == "F")
                bgc_int = 15;
            else if (bgc_str == "V")
                bgc_int = 100;
            else if (bgc_str == "X")
                bgc_int = 0;
            else if (bgc_str == "0")
                bgc_int = 0;
            else if (bgc_str == "1")
                bgc_int = 10;
            else if (bgc_str == "2")
                bgc_int = 20;
            else if (bgc_str == "3")
                bgc_int = 30;
            else if (bgc_str == "4")
                bgc_int = 40;
            else if (bgc_str == "5")
                bgc_int = 50;
            else if (bgc_str == "6")
                bgc_int = 60;
            else if (bgc_str == "7")
                bgc_int = 70;
            else
                bgc_int = 0;


            return bgc_int;
        }

        private void ErrorMessageBox(string feld)
        {
            string errorMessage;
            if (feld == String.Empty)
            {
                errorMessage = "ERROR - one field name is empty!";
            }
            else
            {
                errorMessage = "ERROR - field name " + feld + " doesn't exist!";
            }
            pLegendGeneratorForm.staLblMessage.Content = errorMessage;
            MessageBox.Show(errorMessage);
            pLegendGeneratorForm.Cursor = System.Windows.Input.Cursors.Arrow;
        }

        private void ErrorMessageBox2(string feld, string label)
        {
            string errorMessage;
            if (feld == String.Empty)
            {
                errorMessage = "ERROR - field name for " + label + " is empty!";
            }
            else
            {
                errorMessage = "ERROR - field name " + feld + " doesn't exist!";
            }
            pLegendGeneratorForm.staLblMessage.Content = errorMessage;
            MessageBox.Show(errorMessage);
            pLegendGeneratorForm.Cursor = System.Windows.Input.Cursors.Arrow;
        }

        private string Zeilenumbruch(string pLabel, int laenge)
        {
            int zela = 0;
            this.zeilen = 1;
            int firstLeerzeichen_pos = pLabel.IndexOf(" ");  //first empty space
            if (firstLeerzeichen_pos > laenge)
            {
                MessageBox.Show("Der Legendentext ist zu lange!! Bitte erhöhen Sie die Länge für den Zeilenumbruch! " +
                    "Der Zeilenumbruch ist derzeit bei "+ laenge + " eingestellt, das erste Leerzeichen befindest sich aber bei " + 
                    firstLeerzeichen_pos + "!",
                    "zu langer Legendentext", MessageBoxButton.OK, MessageBoxImage.Error);
                return "error";
            }
            int leerzeichen_pos = firstLeerzeichen_pos;  //first emoty space
            int umbruch_pos = 0;

            if (pLabel.Length > 0)
            {
                for (int i = 0; i < pLabel.Length -1 ; i++)
                {

                    try
                    {
                        char buchstabe = pLabel.PartString(i , 1)[0];//was bei der mid-Funktion 1 ist, ist bei substring 0: deswegen i -1+ Länge = 1 Buchstabe


                        if (char.IsWhiteSpace(buchstabe))
                        {
                            leerzeichen_pos = i;
                        }
                        if (buchstabe.ToString() == "$")
                        {
                            umbruch_pos = i;
                        }

                        if ((i - zela) > laenge)//Zeilenumbruch durch Längenbegrenzung: $ ist noch nicht vorgekommen
                        {
                            //Mid(pLabel, leerzeichen_pos, 1) = Chr(13);
                            //pLabel.Substring(leerzeichen_pos - 1, 1) = new string((char)32, 1); 
                            if (leerzeichen_pos != 0)
                            {
                                pLabel = pLabel.Remove(leerzeichen_pos, 1);//Leerzeichen wird gelöscht: zweiter 1er ist für die count-Anzahl der zu löschenden characters!
                                pLabel = pLabel.Insert(leerzeichen_pos, "\r");//vorm Leerzeichen wird eine Carriage Return eingefügt                               
                            }                            

                            zela = leerzeichen_pos;
                            this.zeilen = this.zeilen + 1;
                        }
                        if (buchstabe.ToString() == "$")//Zeilenumbruch durch konventionelle Längenbegrenzung:
                        {
                            pLabel = pLabel.Remove(umbruch_pos, 1);//Leerzeichen wird gelöscht: zweiter 1er ist für die count-Anzahl der zu löschenden characters!
                            pLabel = pLabel.Insert(umbruch_pos, "\r");//vorm Leerzeichen wird eine Carriage Return eingefügt
                           
                            zela = umbruch_pos;
                            this.zeilen = this.zeilen + 1;
                        }
                       
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                       
                    }                

                   

                }
                return pLabel;

            }
            else
            {
                return pLabel;
            }
        }
        
        private string CreateTextFileDoesNotExistMsg()
        {
            return "The TXT-file '" + pLegendGeneratorForm.txtFortlaufendeNummer.Text + "' does not exist." + "\n\n" +
            "Please select an existing textfile.";
        }

        #endregion

    }
    
}
