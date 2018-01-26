using System.Collections.Generic;
using System.Data;
using ESRI.ArcGIS.Geodatabase;
using System.Data.OleDb;
using ESRI.ArcGIS.DataSourcesGDB;
using ESRI.ArcGIS.esriSystem;
using System;
using System.Windows;


namespace LegendGenerator.App.Model
{
    public class DataService : IDataService
    {
        public void GetData(Action<FormularData, Exception> callback)
        {
            // Use this to connect to the actual data service
            var item = new FormularData();
            callback(item, null);
        }

        #region operations for access database

        public List<string> GetTables(string file)
        {
            //ILegendGeneratorRepository repository = new LegendGeneratorRepository();
            //return repository.GetTables(file);
            List<string> Tables = new List<string>();
            System.Data.DataTable tables;
            string connString = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + file;

            // Verbindung erzeugen
            OleDbConnection conn = new OleDbConnection(connString);
            try
            {
                // Verbindung öffnen
                conn.Open();
                tables = conn.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, new object[] { null, null, null, "TABLE" });

                for (int i = 0; i < tables.Rows.Count; i++)
                {
                    Tables.Add(tables.Rows[i][2].ToString());
                }
            }
            catch (Exception ex)
            {
                // Gegebenenfalls Fehlerbehandlung
                System.Windows.MessageBox.Show("Error in finding the DB tables for the defined database! " + ex.Message,
                     "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                // Verbindung schließen
                conn.Dispose();
                conn.Close();
            }
            return Tables;
        }
           
        public ITable GetArcObjectsAccessTable(string leg_tab_pfad, string legendentabelle)
        {
            IWorkspaceFactory pFact;
            IWorkspace pWorkspace;
            IFeatureWorkspace pFeatws;
            ITable pTable = null;

            //Legendentabelle für die Abfrage vorbereiten:
            pFact = new AccessWorkspaceFactory();
            try
            {
                pWorkspace = pFact.OpenFromFile(leg_tab_pfad, 0);
                pFeatws = (IFeatureWorkspace)pWorkspace;
                pTable = pFeatws.OpenTable(legendentabelle);
            }
            catch (Exception)
            {
                pTable = null;
            }
            return pTable;
        }

        #endregion

        #region operations for sde server:

        public List<string> GetSqlServerTables(string server, string instance, string database, string version, string user = "", string password = "")
        {
            //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            //List<string> Tables = new List<string>();
            //System.Data.DataTable tables;
            //string connString = "Data Source=" + server + "\\" + instance + " ; Initial Catalog=" + database + ";  Trusted_Connection=True; User Id=" + user + "; Password=" + password;

            //// Verbindung erzeugen
            //SqlConnection conn = new SqlConnection(connString);
            //try
            //{
            //    // Verbindung öffnen
            //    conn.Open();
            //    tables = conn.GetSchema("Tables");

            //    for (int i = 0; i < tables.Rows.Count; i++)
            //    {
            //        Tables.Add(tables.Rows[i][2].ToString());
            //    }
            //}
            //catch (Exception ex)
            //{
            //    // Gegebenenfalls Fehlerbehandlung
            //    MessageBox.Show("Error in finding the DB tables for the defined database! " + ex.Message,
            //         "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            //}
            //finally
            //{
            //    // Verbindung schließen
            //    conn.Close();
            //}
            //return Tables;
            /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

            //SDE connection
            IPropertySet pPropSet;
            IWorkspaceFactory pFact;
            pPropSet = new PropertySet();
            pPropSet.SetProperty("dbclient", "SQLServer");
            pPropSet.SetProperty("SERVER", server);
            pPropSet.SetProperty("INSTANCE", instance);
            pPropSet.SetProperty("DATABASE", database);
            pPropSet.SetProperty("VESRSION", version);

            if (user != "" && password != "")
            {
                pPropSet.SetProperty("USER", user);
                pPropSet.SetProperty("PASSWORD", password);
                pPropSet.SetProperty("authentication_mode", "DBMS");
            }
            else
            {
                pPropSet.SetProperty("authentication_mode", "OSA");
            }
           
            pFact = new SdeWorkspaceFactory();
            List<string> Tables = new List<string>();
            try
            {
                IWorkspace workspace = pFact.Open(pPropSet, 0);
                IEnumDatasetName eDSNames = workspace.get_DatasetNames(esriDatasetType.esriDTTable);
                IDatasetName DSName = eDSNames.Next();

                //MessageBox.Show(DSName.ToString());
                while (DSName != null)
                {
                    Tables.Add(DSName.Name);
                    DSName = eDSNames.Next();
                }
            }
            catch (Exception ex)
            {
                // Gegebenenfalls Fehlerbehandlung
                MessageBox.Show("Error in finding the DB tables for the defined database! " + ex.Message,
                     "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            Tables.Sort();
            return Tables;
        }
             
        public ITable GetArcObjectsSdeTable(string server, string instance, string database, string version, string legendentabelle, string user = "", string password ="")
        {
            IWorkspaceFactory pFact;
            //IWorkspace pWorkspace;
            IFeatureWorkspace pFeatws;
            IPropertySet pPropSet;//für eine SDE-Verbindung!!!
            ITable pTable = null;

            //Write some Code for the SDE connection
            pPropSet = new PropertySet();
            pPropSet.SetProperty("SERVER", server);
            pPropSet.SetProperty("INSTANCE", instance);
            pPropSet.SetProperty("DATABASE", database);
            pPropSet.SetProperty("VESRSION", version);

            if (user != "" && password != "")
            {
                pPropSet.SetProperty("USER", user);
                pPropSet.SetProperty("PASSWORD", password);
                pPropSet.SetProperty("authentication_mode", "DBMS");
            }
            else
            {
                pPropSet.SetProperty("authentication_mode", "OSA");
            }
           
            pFact = new SdeWorkspaceFactory();
            try
            {
                pFeatws = (IFeatureWorkspace)pFact.Open(pPropSet, 0);
                pTable = pFeatws.OpenTable(legendentabelle);
            }
            catch (Exception)
            {
                pTable = null;
            }
            return pTable;
        }

        #endregion
       
    }
}
