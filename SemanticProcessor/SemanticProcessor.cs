using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace SemanticProcessor
{
    internal static class SemanticProcessor
    {
#if DEBUG
        private static bool _lDebug = true;
#else
        private static bool _lDebug = false;
#endif
        private static bool bExit = false;
        /// <summary>
        /// Punto di ingresso principale dell'applicazione.
        /// </summary>
        static void Main()
        {
            try
            {
                if (!_lDebug)
                {
                    ServiceBase[] ServicesToRun;
                    ServicesToRun = new ServiceBase[]
                    {
                        new SemanticProcessorService()
                    };
                    ServiceBase.Run(ServicesToRun);
                }
                else
                {
                    SemanticProcessorService w = new SemanticProcessorService();
                    w.MainCycleService();
                    while (!bExit)
                    {

                    }
                }
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
                if (ex.InnerException != null)
                {
                    EventLog.WriteEntry("WintTTab service", string.Format("Exceprtion: {0} Inner exception: {1} ", msg, ex.InnerException.Message), EventLogEntryType.Error, 0x7f0f);
                    //        sw.WriteLine("INNER:" + ex.InnerException.Message);
                }
                else
                {
                    EventLog.WriteEntry("WintTTab service", string.Format("Exceprtion: {0} ", msg), EventLogEntryType.Error, 0x7f0f);
                }
            }
        }
    }
}
