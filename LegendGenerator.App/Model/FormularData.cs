using System.Collections.Generic;
using System.Xml.Serialization;
using LegendGenerator.App.Utils;
using System.ComponentModel.DataAnnotations;

/// <summary>
/// FormularData test class to demonstrate how to include custom metadata attributes in a 
/// class so that it can be serialized/deserialized to/from XML.
/// </summary>

namespace LegendGenerator.App.Model 
{
    // Set this 'Customer' class as the root node of any XML file its serialized to.
    [XmlRootAttribute("Formular_Data", Namespace = "", IsNullable = false)]
    public class FormularData : NotifyBase
    {
        #region fields

        private System.DateTime _dateTimeValue;

        //fields for access database       
        private bool _chkAccess;

        //fields for sqlserver!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
        private bool _chkSde;
                     
        //fields for the column names!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
        private List<string> _tables;
       
        #endregion

        #region constructor 

        public FormularData()
        {
            //ColumnNames = new System.Collections.ArrayList();
            _tables = new List<string>();
            this._dateTimeValue = System.DateTime.Now;
        }

        #endregion

        // Set this 'DateTimeValue' field to be an attribute of the root node.
        [XmlAttributeAttribute(DataType = "date")]
        public System.DateTime DateTimeValue
        {
            get
            {
                return this._dateTimeValue;
            }
            set
            {
                this._dateTimeValue = value;
            }
        }

        //attributes for sub windows:
        //public string Konfigurationsdatei { get; set; }
        public bool ChkGifExport { get; set; }
        public string SymbolGifDirectory { get; set; }

        //allgemeine boolsche Attribute:!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
        public bool TabAccess { get; set; }
        public bool TabSde { get; set; }

        //attributes for the access database connection!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
        //[Required(ErrorMessage = "Connection string is required!")]
        [DoesExistFileName("ChkAccess", ErrorMessage = "Uh oh, no existing database file")]
        public string AccessDatabase
        {
            get { return base.Get(() => AccessDatabase); }
            set { base.Set(() => AccessDatabase, value); }
        }
        public bool ChkAccess
        {
            get { return _chkAccess; }
            set
            {
                _chkAccess = value;
                base.OnPropertyChanged("ChkAccess");
                CalculateDependentCheckBox(ref _chkSde, "ChkSde", !value);
                if (value == true && this.ChkAuthentifizierung == true)
                {
                    this.ChkAuthentifizierung = false;
                }
                base.OnPropertyChanged("AccessDatabase");
            }
        }

        //Attribute fuer den SQL-Server:!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
        public bool ChkSde
        {
            get { return _chkSde; }
            set
            {
                _chkSde = value;
                base.OnPropertyChanged("ChkSde");
                CalculateDependentCheckBox(ref _chkAccess, "ChkAccess", !value);
                if(value == false && this.ChkAuthentifizierung == true)
                {
                    this.ChkAuthentifizierung = false;
                }
                base.OnPropertyChanged("AccessDatabase");
            }
        }
        public bool ChkAuthentifizierung
        {
            get { return base.Get(() => ChkAuthentifizierung); }
            set { base.Set(() => ChkAuthentifizierung, value); }
        }       
        public string SqlServer
        {
            get { return base.Get(() => SqlServer); }
            set { base.Set(() => SqlServer, value); }
        }
        public string ServerInstance
        {
            get { return base.Get(() => ServerInstance); }
            set { base.Set(() => ServerInstance, value); }
        }
        public string ServerDatenbank
        {
            get { return base.Get(() => ServerDatenbank); }
            set { base.Set(() => ServerDatenbank, value); }
        }
        public string ServerUser
        {
            get { return base.Get(() => ServerUser); }
            set { base.Set(() => ServerUser, value); }
        }
        public string ServerPasswort
        {
            get { return base.Get(() => ServerPasswort); }
            set { base.Set(() => ServerPasswort, value); }
        }
        public string ServerVersion
        {
            get { return base.Get(() => ServerVersion); }
            set { base.Set(() => ServerVersion, value); }
        }        
     

        ////Attribute für die Legendenspalten:!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
        // Serializes an ArrayList as a "Legend_Tables" array of XML elements of type string named "Legend_Table".
        [XmlArray("LegendTables"), XmlArrayItem("Legend_Table", typeof(string))]
        public List<string> Tables
        {
            get { return base.Get(() => Tables); }
            set { base.Set(() => Tables, value); }
        }
        [Required(ErrorMessage = "Table is required!")]
        public string Table
        {
            get { return base.Get(() => Table); }
            set { base.Set(() => Table, value); }
        }
        [Required(ErrorMessage = "database ID is required!")]
        public string DatabaseId
        {
            get { return base.Get(() => DatabaseId); }
            set { base.Set(() => DatabaseId, value); }
        }
        [Required(ErrorMessage = "sort key is required!")]
        public string Sortierschluessel
        {
            get { return base.Get(() => Sortierschluessel); }
            set { base.Set(() => Sortierschluessel, value); }
        }
        [Required(ErrorMessage = "column for legend text is required!")]
        public string Legendentext
        {
            get { return base.Get(() => Legendentext); }
            set { base.Set(() => Legendentext, value); }
        }       
        [Required(ErrorMessage = "column for symbols is required!")]
        public string Zeichenelemente
        {
            get { return base.Get(() => Zeichenelemente); }
            set { base.Set(() => Zeichenelemente, value); }
        }      

        [RequiredAttributeDependentOnBoolFlag(DependentBooleanFlagProperty = "ChkLegendengrafik", AllowEmptyStrings = false, ErrorMessage = "column for graphics is required!")]
        public string Legendengrafik
        {
            get { return base.Get(() => Legendengrafik); }
            set { base.Set(() => Legendengrafik, value); }
        }
        public bool ChkLegendengrafik
        {
            get { return base.Get(() => ChkLegendengrafik); }
            set { base.Set(() => ChkLegendengrafik, value); base.OnPropertyChanged("Legendengrafik"); }
        }

        [RequiredAttributeDependentOnBoolFlag(DependentBooleanFlagProperty = "ChkUeberschrift1", AllowEmptyStrings = false, ErrorMessage = "column for heading1 is required!")]
        public string Ueberschrift1
        {
            get { return base.Get(() => Ueberschrift1); }
            set { base.Set(() => Ueberschrift1, value); }
        }
        public bool ChkUeberschrift1
        {
            get { return base.Get(() => ChkUeberschrift1); }
            set { base.Set(() => ChkUeberschrift1, value); base.OnPropertyChanged("Ueberschrift1"); }
        }
        [RequiredAttributeDependentOnBoolFlag(DependentBooleanFlagProperty = "ChkUeberschrift2", AllowEmptyStrings = false, ErrorMessage = "column for heading2 is required!")]
        public string Ueberschrift2
        {
            get { return base.Get(() => Ueberschrift2); }
            set { base.Set(() => Ueberschrift2, value); }
        }
        public bool ChkUeberschrift2
        {
            get { return base.Get(() => ChkUeberschrift2); }
            set { base.Set(() => ChkUeberschrift2, value); base.OnPropertyChanged("Ueberschrift2"); }
        }
        [RequiredAttributeDependentOnBoolFlag(DependentBooleanFlagProperty = "ChkUeberschrift3", AllowEmptyStrings = false, ErrorMessage = "column for heading3 is required!")]
        public string Ueberschrift3
        {
            get { return base.Get(() => Ueberschrift3); }
            set { base.Set(() => Ueberschrift3, value); }
        }
        public bool ChkUeberschrift3
        {
            get { return base.Get(() => ChkUeberschrift3); }
            set { base.Set(() => ChkUeberschrift3, value); base.OnPropertyChanged("Ueberschrift3"); }
        }


        [RequiredAttributeDependentOnBoolFlag(DependentBooleanFlagProperty = "ChkKlammerebene1", AllowEmptyStrings = false, ErrorMessage = "column for bracket1 is required!")]
        public string Klammerebene1
        {
            get { return base.Get(() => Klammerebene1); }
            set { base.Set(() => Klammerebene1, value); }
        }
        public bool ChkKlammerebene1
        {
            get { return base.Get(() => ChkKlammerebene1); }
            set { base.Set(() => ChkKlammerebene1, value); base.OnPropertyChanged("Klammerebene1"); }
        }
        [RequiredAttributeDependentOnBoolFlag(DependentBooleanFlagProperty = "ChkKlammerebene2", AllowEmptyStrings = false, ErrorMessage = "column for bracket2 is required!")]
        public string Klammerebene2
        {
            get { return base.Get(() => Klammerebene2); }
            set { base.Set(() => Klammerebene2, value); }
        }
        public bool ChkKlammerebene2
        {
            get { return base.Get(() => ChkKlammerebene2); }
            set { base.Set(() => ChkKlammerebene2, value); base.OnPropertyChanged("Klammerebene2"); }
        }
        [RequiredAttributeDependentOnBoolFlag(DependentBooleanFlagProperty = "ChkKlammerebene3", AllowEmptyStrings = false, ErrorMessage = "column for bracket3 is required!")]
        public string Klammerebene3
        {
            get { return base.Get(() => Klammerebene3); }
            set { base.Set(() => Klammerebene3, value); }
        }
        public bool ChkKlammerebene3
        {
            get { return base.Get(() => ChkKlammerebene3); }
            set { base.Set(() => ChkKlammerebene3, value); base.OnPropertyChanged("Klammerebene3"); }
        }
        [RequiredAttributeDependentOnBoolFlag(DependentBooleanFlagProperty = "ChkGruppierung", AllowEmptyStrings = false, ErrorMessage = "column for group attribute is required!")]
        public string Gruppierung
        {
            get { return base.Get(() => Gruppierung); }
            set { base.Set(() => Gruppierung, value); }
        }
        public bool ChkGruppierung
        {
            get { return base.Get(() => ChkGruppierung); }
            set { base.Set(() => ChkGruppierung, value); base.OnPropertyChanged("Gruppierung"); }
        }
        [RequiredAttributeDependentOnBoolFlag(DependentBooleanFlagProperty = "ChkFlaechensymbolname", AllowEmptyStrings = false, ErrorMessage = "column for area is required!")]
        public string Flaechensymbolname
        {
            get { return base.Get(() => Flaechensymbolname); }
            set { base.Set(() => Flaechensymbolname, value); }
        }
        public bool ChkFlaechensymbolname
        {
            get { return base.Get(() => ChkFlaechensymbolname); }
            set { base.Set(() => ChkFlaechensymbolname, value); base.OnPropertyChanged("Flaechensymbolname"); }
        }
        [RequiredAttributeDependentOnBoolFlag(DependentBooleanFlagProperty = "ChkLiniensymbolname", AllowEmptyStrings = false, ErrorMessage = "column for line is required!")]
        public string Liniensymbolname
        {
            get { return base.Get(() => Liniensymbolname); }
            set { base.Set(() => Liniensymbolname, value); }
        }
        public bool ChkLiniensymbolname
        {
            get { return base.Get(() => ChkLiniensymbolname); }
            set { base.Set(() => ChkLiniensymbolname, value); base.OnPropertyChanged("Liniensymbolname"); }
        }
        [RequiredAttributeDependentOnBoolFlag(DependentBooleanFlagProperty = "ChkMarkersymbolname", AllowEmptyStrings = false, ErrorMessage = "column for marker is required!")]
        public string Markersymbolname
        {
            get { return base.Get(() => Markersymbolname); }
            set { base.Set(() => Markersymbolname, value); }
        }
        public bool ChkMarkersymbolname
        {
            get { return base.Get(() => ChkMarkersymbolname); }
            set { base.Set(() => ChkMarkersymbolname, value); base.OnPropertyChanged("Markersymbolname"); }
        }
        [RequiredAttributeDependentOnBoolFlag(DependentBooleanFlagProperty = "ChkLegendennummer", AllowEmptyStrings = false, ErrorMessage = "column for legend number is required!")]
        public string Legendennummer
        {
            get { return base.Get(() => Legendennummer); }
            set { base.Set(() => Legendennummer, value); }
        }
        public bool ChkLegendennummer
        {
            get { return base.Get(() => ChkLegendennummer); }
            set { base.Set(() => ChkLegendennummer, value); base.OnPropertyChanged("Legendennummer"); }
        }
        [DoesExistFileName("ChkFortlaufendeNummer", ErrorMessage = "Uh oh, no existing text file")]
        public string FortlaufendeNummer
        {
            get { return base.Get(() => FortlaufendeNummer); }
            set { base.Set(() => FortlaufendeNummer, value); }
        }
        public bool ChkFortlaufendeNummer
        {
            get { return base.Get(() => ChkFortlaufendeNummer); }
            set { base.Set(() => ChkFortlaufendeNummer, value); base.OnPropertyChanged("FortlaufendeNummer"); }
        }

        //for additional columns!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
        [RequiredAttributeDependentOnBoolFlag(DependentBooleanFlagProperty = "ChkInfozeile4", AllowEmptyStrings = false, ErrorMessage = "column for heading4 is required!")]
        public string Infozeile4
        {
            get { return base.Get(() => Infozeile4); }
            set { base.Set(() => Infozeile4, value); }
        }
        public bool ChkInfozeile4
        {
            get { return base.Get(() => ChkInfozeile4); }
            set { base.Set(() => ChkInfozeile4, value); base.OnPropertyChanged("Infozeile4"); }
        }
        [RequiredAttributeDependentOnBoolFlag(DependentBooleanFlagProperty = "ChkInfozeile5", AllowEmptyStrings = false, ErrorMessage = "column for heading5 is required!")]
        public string Infozeile5
        {
            get { return base.Get(() => Infozeile5); }
            set { base.Set(() => Infozeile5, value); }
        }
        public bool ChkInfozeile5
        {
            get { return base.Get(() => ChkInfozeile5); }
            set { base.Set(() => ChkInfozeile5, value); base.OnPropertyChanged("Infozeile5"); }
        }
        [RequiredAttributeDependentOnBoolFlag(DependentBooleanFlagProperty = "ChkInfozeile6", AllowEmptyStrings = false, ErrorMessage = "column for heading6 is required!")]
        public string Infozeile6
        {
            get { return base.Get(() => Infozeile6); }
            set { base.Set(() => Infozeile6, value); }
        }
        public bool ChkInfozeile6
        {
            get { return base.Get(() => ChkInfozeile6); }
            set { base.Set(() => ChkInfozeile6, value); base.OnPropertyChanged("Infozeile6"); }
        }
        [RequiredAttributeDependentOnBoolFlag(DependentBooleanFlagProperty = "ChkNotizen", AllowEmptyStrings = false, ErrorMessage = "column for notes is required!")]
        public string Notizen
        {
            get { return base.Get(() => Notizen); }
            set { base.Set(() => Notizen, value); }
        }
        public bool ChkNotizen
        {
            get { return base.Get(() => ChkNotizen); }
            set { base.Set(() => ChkNotizen, value); base.OnPropertyChanged("Notizen"); }
        }


        //attribute for attribute and spatial filtering !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!        
        [RequiredAttributeDependentOnBoolFlag(DependentBooleanFlagProperty = "ChkSpatialQuery", AllowEmptyStrings = false, ErrorMessage = "column for spatial query is required!")]
        public string SpatialQueryId
        {
            get { return base.Get(() => SpatialQueryId); }
            set { base.Set(() => SpatialQueryId, value); }
        }
        public bool ChkSpatialQuery
        {
            get { return base.Get(() => ChkSpatialQuery); }
            set { base.Set(() => ChkSpatialQuery, value); base.OnPropertyChanged("SpatialQueryId"); }
        }
        public string AttributeQuery
        {
            get { return base.Get(() => AttributeQuery); }
            set { base.Set(() => AttributeQuery, value); }
        }

        //attributes for symbol:!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
        public bool ChkFarbwerte
        {
            get { return base.Get(() => ChkFarbwerte); }
            set { base.Set(() => ChkFarbwerte, value); }
        }
        public bool ChkAllowNullLegendNumber
        {
            get { return base.Get(() => ChkAllowNullLegendNumber); }
            set { base.Set(() => ChkAllowNullLegendNumber, value); }
        }
        public string StyleFile
        {
            get { return base.Get(() => StyleFile); }
            set { base.Set(() => StyleFile, value); }
        }
        

        //Attribute für die grafische Positionierung:!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
        [Range(0.0, 50.0, ErrorMessage = "XOffset must be within the range of 0 and 50.")]
         public string XOffset
        {
            get { return base.Get(() => XOffset); }
            set { base.Set(() => XOffset, value); }
        }
        [Range(0.0, 50.0, ErrorMessage = "YOffset must be within the range of 0 and 50.")]
        public string YOffset
        {
            get { return base.Get(() => YOffset); }
            set { base.Set(() => YOffset, value); }
        }
        [Range(0.0, 50.0, ErrorMessage = "vertical distance must be within the range of 0 and 50.")]
        public string Vertikalabstand
        {
            get { return base.Get(() => Vertikalabstand); }
            set { base.Set(() => Vertikalabstand, value); }
        }
        [Range(0.0, 50.0, ErrorMessage = "bracket offset must be within the range of 0 and 50.")]
        public string Klammerabstand
        {
            get { return base.Get(() => Klammerabstand); }
            set { base.Set(() => Klammerabstand, value); }
        }
        [Range(0.0, 50.0, ErrorMessage = "distance must be within the range of 0 and 50.")]
        public string KaestchenLegendentextAbstand
        {
            get { return base.Get(() => KaestchenLegendentextAbstand); }
            set { base.Set(() => KaestchenLegendentextAbstand, value); }
        }
        [Range(0.0, 50.0, ErrorMessage = "distance before heading1 must be within the range of 0 and 50.")]
        public string VorUeberschrift1
        {
            get { return base.Get(() => VorUeberschrift1); }
            set { base.Set(() => VorUeberschrift1, value); }
        }
        [Range(0.0, 50.0, ErrorMessage = "distance after heading1 must be within the range of 0 and 50.")]
        public string NachUeberschrift1
        {
            get { return base.Get(() => NachUeberschrift1); }
            set { base.Set(() => NachUeberschrift1, value); }
        }
        [Range(0.0, 50.0, ErrorMessage = "distance after heading2 must be within the range of 0 and 50.")]
        public string NachUeberschrift2
        {
            get { return base.Get(() => NachUeberschrift2); }
            set { base.Set(() => NachUeberschrift2, value); }
        }
        [Range(0.0, 50.0, ErrorMessage = "notes offset must be within the range of 0 and 50.")]
        public string NotizenOffset
        {
            get { return base.Get(() => NotizenOffset); }
            set { base.Set(() => NotizenOffset, value); }
        }

        //Attribute für Zeilen- und Spaltenumbruch:!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
        [Range(0, 150, ErrorMessage = "value must be within the int range of 0 and 150.")]
        public string UmbruchUeberschrift1
        {
            get { return base.Get(() => UmbruchUeberschrift1); }
            set { base.Set(() => UmbruchUeberschrift1, value); }
        }
        [Range(0, 150, ErrorMessage = "value must be within the int range of 0 and 150.")]
        public string UmbruchUeberschrift2
        {
            get { return base.Get(() => UmbruchUeberschrift2); }
            set { base.Set(() => UmbruchUeberschrift2, value); }
        }
        [Range(0, 150, ErrorMessage = "value must be within the int range of 0 and 150.")]
        public string UmbruchUeberschrift3
        {
            get { return base.Get(() => UmbruchUeberschrift3); }
            set { base.Set(() => UmbruchUeberschrift3, value); }
        }
        [Range(0, 150, ErrorMessage = "value must be within the int range of 0 and 150.")]
        public string UmbruchLegendentext
        {
            get { return base.Get(() => UmbruchLegendentext); }
            set { base.Set(() => UmbruchLegendentext, value); }
        }
        [Range(0, 150, ErrorMessage = "value must be within the int range of 0 and 150.")]
        public string UmbruchSpaltenLaenge
        {
            get { return base.Get(() => UmbruchSpaltenLaenge); }
            set { base.Set(() => UmbruchSpaltenLaenge, value); }
        }
        [Range(0, 150, ErrorMessage = "value must be within the int range of 0 and 150.")]
        public string UmbruchSpaltenOffset
        {
            get { return base.Get(() => UmbruchSpaltenOffset); }
            set { base.Set(() => UmbruchSpaltenOffset, value); }
        }

        #region colors !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!

        [Range(0.0, 100.0, ErrorMessage = "cmyk value must be within the range of 0 and 100.")]
        public string Roc //red
        {
            get { return base.Get(() => Roc); }
            set { base.Set(() => Roc, value); }
        }
        [Range(0.0, 100.0, ErrorMessage = "cmyk value must be within the range of 0 and 100.")]
        public string Rom
        {
            get { return base.Get(() => Rom); }
            set { base.Set(() => Rom, value); }
        }
        [Range(0.0, 100.0, ErrorMessage = "cmyk value must be within the range of 0 and 100.")]
        public string Roy
        {
            get { return base.Get(() => Roy); }
            set { base.Set(() => Roy, value); }
        }
        [Range(0.0, 100.0, ErrorMessage = "cmyk value must be within the range of 0 and 100.")]
        public string Rok
        {
            get { return base.Get(() => Rok); }
            set { base.Set(() => Rok, value); }
        }

        [Range(0.0, 100.0, ErrorMessage = "cmyk value must be within the range of 0 and 100.")]
        public string Blc //blue
        {
            get { return base.Get(() => Blc); }
            set { base.Set(() => Blc, value); }
        }
        [Range(0.0, 100.0, ErrorMessage = "cmyk value must be within the range of 0 and 100.")]
        public string Blm
        {
            get { return base.Get(() => Blm); }
            set { base.Set(() => Blm, value); }
        }
        [Range(0.0, 100.0, ErrorMessage = "cmyk value must be within the range of 0 and 100.")]
        public string Bly
        {
            get { return base.Get(() => Bly); }
            set { base.Set(() => Bly, value); }
        }
        [Range(0.0, 100.0, ErrorMessage = "cmyk value must be within the range of 0 and 100.")]
        public string Blk
        {
            get { return base.Get(() => Blk); }
            set { base.Set(() => Blk, value); }
        }

        [Range(0.0, 100.0, ErrorMessage = "cmyk value must be within the range of 0 and 100.")]
        public string Grc //green
        {
            get { return base.Get(() => Grc); }
            set { base.Set(() => Grc, value); }
        }
        [Range(0.0, 100.0, ErrorMessage = "cmyk value must be within the range of 0 and 100.")]
        public string Grm
        {
            get { return base.Get(() => Grm); }
            set { base.Set(() => Grm, value); }
        }
        [Range(0.0, 100.0, ErrorMessage = "cmyk value must be within the range of 0 and 100.")]
        public string Gry
        {
            get { return base.Get(() => Gry); }
            set { base.Set(() => Gry, value); }
        }
        [Range(0.0, 100.0, ErrorMessage = "cmyk value must be within the range of 0 and 100.")]
        public string Grk
        {
            get { return base.Get(() => Grk); }
            set { base.Set(() => Grk, value); }
        }

        [Range(0.0, 100.0, ErrorMessage = "cmyk value must be within the range of 0 and 100.")]
        public string Brc //brown
        {
            get { return base.Get(() => Brc); }
            set { base.Set(() => Brc, value); }
        }
        [Range(0.0, 100.0, ErrorMessage = "cmyk value must be within the range of 0 and 100.")]
        public string Brm
        {
            get { return base.Get(() => Brm); }
            set { base.Set(() => Brm, value); }
        }
        [Range(0.0, 100.0, ErrorMessage = "cmyk value must be within the range of 0 and 100.")]
        public string Bry
        {
            get { return base.Get(() => Bry); }
            set { base.Set(() => Bry, value); }
        }
        [Range(0.0, 100.0, ErrorMessage = "cmyk value must be within the range of 0 and 100.")]
        public string Brk
        {
            get { return base.Get(() => Brk); }
            set { base.Set(() => Brk, value); }
        }

        [Range(0.0, 100.0, ErrorMessage = "cmyk value must be within the range of 0 and 100.")]
        public string Gac//gray
        {
            get { return base.Get(() => Gac); }
            set { base.Set(() => Gac, value); }
        }
        [Range(0.0, 100.0, ErrorMessage = "cmyk value must be within the range of 0 and 100.")]
        public string Gam
        {
            get { return base.Get(() => Gam); }
            set { base.Set(() => Gam, value); }
        }
        [Range(0.0, 100.0, ErrorMessage = "cmyk value must be within the range of 0 and 100.")]
        public string Gay
        {
            get { return base.Get(() => Gay); }
            set { base.Set(() => Gay, value); }
        }
        [Range(0.0, 100.0, ErrorMessage = "cmyk value must be within the range of 0 and 100.")]
        public string Gak
        {
            get { return base.Get(() => Gak); }
            set { base.Set(() => Gak, value); }
        }

        [Range(0.0, 100.0, ErrorMessage = "cmyk value must be within the range of 0 and 100.")]
        public string Mac //magenta
        {
            get { return base.Get(() => Mac); }
            set { base.Set(() => Mac, value); }
        }
        [Range(0.0, 100.0, ErrorMessage = "cmyk value must be within the range of 0 and 100.")]
        public string Mam
        {
            get { return base.Get(() => Mam); }
            set { base.Set(() => Mam, value); }
        }
        [Range(0.0, 100.0, ErrorMessage = "cmyk value must be within the range of 0 and 100.")]
        public string May
        {
            get { return base.Get(() => May); }
            set { base.Set(() => May, value); }
        }
        [Range(0.0, 100.0, ErrorMessage = "cmyk value must be within the range of 0 and 100.")]
        public string Mak
        {
            get { return base.Get(() => Mak); }
            set { base.Set(() => Mak, value); }
        }

        [Range(0.0, 100.0, ErrorMessage = "cmyk value must be within the range of 0 and 100.")]
        public string Cyc //cyan
        {
            get { return base.Get(() => Cyc); }
            set { base.Set(() => Cyc, value); }
        }
        [Range(0.0, 100.0, ErrorMessage = "cmyk value must be within the range of 0 and 100.")]
        public string Cym
        {
            get { return base.Get(() => Cym); }
            set { base.Set(() => Cym, value); }
        }
        [Range(0.0, 100.0, ErrorMessage = "cmyk value must be within the range of 0 and 100.")]
        public string Cyy
        {
            get { return base.Get(() => Cyy); }
            set { base.Set(() => Cyy, value); }
        }
        [Range(0.0, 100.0, ErrorMessage = "cmyk value must be within the range of 0 and 100.")]
        public string Cyk
        {
            get { return base.Get(() => Cyk); }
            set { base.Set(() => Cyk, value); }
        }

        [Range(0.0, 100.0, ErrorMessage = "cmyk value must be within the range of 0 and 100.")]
        public string Gec //yellow
        {
            get { return base.Get(() => Gec); }
            set { base.Set(() => Gec, value); }
        }
        [Range(0.0, 100.0, ErrorMessage = "cmyk value must be within the range of 0 and 100.")]
        public string Gem
        {
            get { return base.Get(() => Gem); }
            set { base.Set(() => Gem, value); }
        }
        [Range(0.0, 100.0, ErrorMessage = "cmyk value must be within the range of 0 and 100.")]
        public string Gey
        {
            get { return base.Get(() => Gey); }
            set { base.Set(() => Gey, value); }
        }
        [Range(0.0, 100.0, ErrorMessage = "cmyk value must be within the range of 0 and 100.")]
        public string Gek
        {
            get { return base.Get(() => Gek); }
            set { base.Set(() => Gek, value); }
        }

        [Range(0.0, 100.0, ErrorMessage = "cmyk value must be within the range of 0 and 100.")]
        public string Orc //orange
        {
            get { return base.Get(() => Orc); }
            set { base.Set(() => Orc, value); }
        }
        [Range(0.0, 100.0, ErrorMessage = "cmyk value must be within the range of 0 and 100.")]
        public string Orm
        {
            get { return base.Get(() => Orm); }
            set { base.Set(() => Orm, value); }
        }
        [Range(0.0, 100.0, ErrorMessage = "cmyk value must be within the range of 0 and 100.")]
        public string Ory
        {
            get { return base.Get(() => Ory); }
            set { base.Set(() => Ory, value); }
        }
        [Range(0.0, 100.0, ErrorMessage = "cmyk value must be within the range of 0 and 100.")]
        public string Ork
        {
            get { return base.Get(() => Ork); }
            set { base.Set(() => Ork, value); }
        }

       

        #endregion

        #region private methods

        private void CalculateDependentCheckBox(ref bool otherCheckBox, string otherProperty, bool negatedCheckValue)
        {
            otherCheckBox = negatedCheckValue;
            base.OnPropertyChanged(otherProperty);
        }
        
        #endregion

    }
}
