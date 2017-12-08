using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;

namespace Reports.Presentation
{
    public static class ApplicationTracer
    {
        public static void ApplicationTrace(TraceLevel type, Type theClass, string controllerName, string actionName, string message, Exception e)
        {
            string messageToShow;
            log4net.ILog log;
            log = log4net.LogManager.GetLogger(theClass);
            if (e != null)
            {
                messageToShow = string.Format("\tClass={0} \r\n\tController= {1} \r\n\tAction= {2} \r\n\tMessage= {3} \r\n\tGetBaseException= {4}",
                            theClass.ToString(), controllerName, actionName, message, e.GetBaseException().ToString());
            }
            else
            {
                messageToShow = string.Format("\tClass={0} \r\n\tController= {1} \r\n\tAction= {2} \r\n\tMessage= {3}",
                            theClass.ToString(), controllerName, actionName, message);
            }
            switch (type)
            {
                case TraceLevel.Info:
                    Trace.TraceInformation(messageToShow);
                    log.Info(messageToShow);
                    break;
                case TraceLevel.Error:
                    Trace.TraceError(messageToShow);
                    log.Error(messageToShow);
                    break;
                case TraceLevel.Warning:
                    Trace.TraceWarning(messageToShow);
                    log.Warn(messageToShow);
                    break;
            }
        }

        public static void SignalExceptionToElmahAndTrace(Exception ex, Type theClass, string controlerName, string actionName)
        {
            ApplicationTracer.ApplicationTrace(
                System.Diagnostics.TraceLevel.Error,
                theClass,
                controlerName,
                actionName,
                ex.Message, ex);

            Elmah.ErrorSignal.FromCurrentContext().Raise(ex); //ELMAH Signaling
        }
    }


}