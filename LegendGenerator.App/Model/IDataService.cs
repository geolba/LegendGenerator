using System;
using System.Collections.Generic;
using ESRI.ArcGIS.Geodatabase;

namespace LegendGenerator.App.Model
{
    public interface IDataService
    {
        #region Operations (6)

        void GetData(Action<FormularData, Exception> callback);

        List<string> GetTables(string file);
        //List<string> GetColumnNames(string file, string table);
        ITable GetArcObjectsAccessTable(string leg_tab_pfad, string legendentabelle);

        List<string> GetSqlServerTables(string server, string instance, string database, string version, string user = "", string password = "");        
        //List<string> GetSqlServerColumnNames(string table, string server, string instance, string database, string version, string user = "", string password = "");        
        ITable GetArcObjectsSdeTable(string server, string instance, string database, string version, string legendentabelle, string user = "", string password = "");

        #endregion
    }
}
