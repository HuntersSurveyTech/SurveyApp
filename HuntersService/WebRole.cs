using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.WindowsAzure.ServiceRuntime;

namespace HuntersService
{
    public class WebRole : RoleEntryPoint
    {
        public override bool OnStart()
        {
            ////// To enable the AzureLocalStorageTraceListner, uncomment relevent section in the web.config  
            //var diagnosticConfig = DiagnosticMonitor.GetDefaultInitialConfiguration();
            //diagnosticConfig.DiagnosticInfrastructureLogs.ScheduledTransferLogLevelFilter = LogLevel.Verbose;
            //diagnosticConfig.Directories.ScheduledTransferPeriod = TimeSpan.FromMinutes(1);
            //diagnosticConfig.Directories.DataSources.Add(AzureLocalStorageTraceListener.GetLogDirectory());

            //DiagnosticMonitor.Start("Microsoft.WindowsAzure.Plugins.Diagnostics.ConnectionString", diagnosticConfig);

            return base.OnStart();
        }
    }
}