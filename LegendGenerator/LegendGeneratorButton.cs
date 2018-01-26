using System;
using System.Windows.Forms;
using System.Threading;
using ESRI.ArcGIS.Framework;
using System.Windows.Forms.Integration;
using LegendGenerator.App;

namespace LegendGenerator
{
    public class LegendGeneratorButton : ESRI.ArcGIS.Desktop.AddIns.Button
    {
        //klassenweite private Felder:
        //Seite 1 
        private IApplication m_application;
        //LegendGeneratorWindow wpfwindow = null;

        //constructor:
        public LegendGeneratorButton()
        {
            m_application = ArcMap.Application;
        }

        protected override void OnClick()
        {
            try
            {
                //LegendGeneratorForm dlg = new LegendGeneratorForm(m_application);
                //dlg.ShowDialog();

                MainWindow wpfwindow = new MainWindow(m_application);
                //Enable Keyboard Input:
                ElementHost.EnableModelessKeyboardInterop(wpfwindow);
                //wpfwindow.ShowDialog();//Modal
             
                System.Windows.Interop.WindowInteropHelper helper = new System.Windows.Interop.WindowInteropHelper(wpfwindow);              
                helper.Owner = (IntPtr) ArcMap.Application.hWnd;//winFormWindow.Handle.
                wpfwindow.Show();//Not Modal

                ArcMap.Application.CurrentTool = null;               
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }        
           
        }

        //private void NewWindowThread<T, P>(Func<P, T> constructor, P param) where T : LegendGeneratorWindow
        //{

        //    Thread thread = new Thread(() =>
        //    {

        //        T w = constructor(param);

        //        w.Show();

        //        w.Closed += (sender, e) => w.Dispatcher.InvokeShutdown();

        //        System.Windows.Threading.Dispatcher.Run();

        //    });

        //    thread.SetApartmentState(ApartmentState.STA);

        //    thread.Start();

        //}


        protected override void OnUpdate()
        {           
        }
    }

}
