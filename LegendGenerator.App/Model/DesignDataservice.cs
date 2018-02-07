using System;
using System.Collections.Generic;
using ESRI.ArcGIS.Geodatabase;

namespace LegendGenerator.App.Model
{
    public class DesignDataservice : IDataService
    {
        public void GetData(Action<FormularData, Exception> callback)
        {
            FormularData formData = new FormularData();
            formData.AccessDatabase = "...select database";
            formData.ChkAccess = true;
            formData.ChkSde = false;
            formData.ChkAuthentifizierung = false;
            formData.ServerVersion = "sde.default";
            formData.TabAccess = true;
            formData.TabSde = false;

            formData.DatabaseId = "ID";
            formData.Sortierschluessel = "L_SORT";
            formData.Legendentext = "L_TEXT";
            formData.Zeichenelemente = "L_SYMB";
            formData.Legendengrafik = "L_GRAPHICS";
            formData.ChkLegendengrafik = true;
            formData.Ueberschrift1 = "HEADING1";
            formData.ChkUeberschrift1 = true;
            formData.Ueberschrift2 = "HEADING2";
            formData.ChkUeberschrift2 = true;
            formData.Ueberschrift3 = "HEADING3";
            formData.ChkUeberschrift3 = true;
            formData.Klammerebene1 = "BRACKET1";
            formData.ChkKlammerebene1 = true;
            formData.Klammerebene2 = "BRACKET2";
            formData.ChkKlammerebene2 = true;
            formData.Klammerebene3 = "BRACKET3";
            formData.ChkKlammerebene3 = true;
            formData.Gruppierung = "L_GROUP";
            formData.ChkGruppierung = true;
            formData.Flaechensymbolname = "FILL_SYMBOL";
            formData.ChkFlaechensymbolname = true;
            formData.Liniensymbolname = "LINE_SYMBOL";
            formData.ChkLiniensymbolname = true;
            formData.Markersymbolname = "MARKER_SYMBOL";
            formData.ChkMarkersymbolname = true;
            formData.Legendennummer = "L_NUM";
            formData.ChkLegendennummer = true;
            formData.FortlaufendeNummer = "fortlaufendeNummer.txt";
            formData.ChkFortlaufendeNummer = false;
            formData.Infozeile4 = "HEADING4";
            formData.ChkInfozeile4 = false;
            formData.Infozeile5 = "HEADING5";
            formData.ChkInfozeile5 = false;
            formData.Infozeile6 = "HEADING6";
            formData.ChkInfozeile6 = false;
            formData.Notizen = "NOTES´";
            formData.ChkNotizen = false;
            formData.SpatialQueryId = "GBANR";
            formData.ChkSpatialQuery = false;
            formData.AttributeQuery = "";
            formData.ChkFarbwerte = false;
            formData.ChkAllowNullLegendNumber = false;
            formData.ChkGifExport = false;

            formData.XOffset = "10";
            formData.YOffset = "3";
            formData.Vertikalabstand = "0.3";
            formData.Klammerabstand = "0.4";
            formData.KaestchenLegendentextAbstand = "0.4";
            formData.VorUeberschrift1 = "0.4";
            formData.NachUeberschrift1 = "0.6";
            formData.NachUeberschrift2 = "0.6";
            formData.NotizenOffset = "9";
            formData.UmbruchUeberschrift1 = "40";
            formData.UmbruchUeberschrift2 = "40";
            formData.UmbruchUeberschrift3 = "40";
            formData.UmbruchLegendentext = "50";
            formData.UmbruchSpaltenLaenge = "50";
            formData.UmbruchSpaltenOffset = "12";

            formData.Roc = "0";
            formData.Rom = "100";
            formData.Roy = "100";
            formData.Rok = "21";
            formData.Blc = "70";
            formData.Blm = "30";
            formData.Bly = "0";
            formData.Blk = "21";
            formData.Grc = "70";
            formData.Grm = "0";
            formData.Gry = "100";
            formData.Grk = "21";
            formData.Brc = "20";
            formData.Brm = "50";
            formData.Bry = "60";
            formData.Brk = "21";
            formData.Gac = "10";
            formData.Gam = "10";
            formData.Gay = "10";
            formData.Gak = "21";
            formData.Mac = "0";
            formData.Mam = "100";
            formData.May = "0";
            formData.Mak = "21";
            formData.Cyc = "100";
            formData.Cym = "0";
            formData.Cyy = "0";
            formData.Cyk = "21";            
            formData.Gec = "0";
            formData.Gem = "0";
            formData.Gey = "100";
            formData.Gek = "21";
            formData.Orc = "0";
            formData.Orm = "30";
            formData.Ory = "100";
            formData.Ork = "21";
            
            callback(formData, null);
        }

        public ITable GetArcObjectsAccessTable(string leg_tab_pfad, string legendentabelle)
        {
            throw new NotImplementedException();
        }

        public ITable GetArcObjectsSdeTable(string server, string instance, string database, string version, string legendentabelle, string user = "", string password = "")
        {
            throw new NotImplementedException();
        }       

        public List<string> GetSqlServerTables(string server, string instance, string database, string version, string user = "", string password = "")
        {
            throw new NotImplementedException();
        }

        public List<string> GetTables(string file)
        {
            throw new NotImplementedException();
        }
    }
}
