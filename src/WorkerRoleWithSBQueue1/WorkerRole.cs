using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Conversations.Common;
using Infrastructure;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.ServiceRuntime;

namespace Sonatribe.CommandProcessor.WorkerRole
{
    public class WorkerRole : RoleEntryPoint
    {
        private bool _running;

        public override void Run()
        {
            TaskScheduler.UnobservedTaskException += this.OnUnobservedTaskException;
            this._running = true;

            while (this._running)
            {
                if (!MaintenanceMode.IsInMaintainanceMode)
                {
                    Trace.WriteLine("Starting the command processor", "Information");
                    using (var processor = new ConversationsCommandProcessor(this.InstrumentationEnabled))
                    {
                        processor.Start();

                        while (this._running && !MaintenanceMode.IsInMaintainanceMode)
                        {
                            Thread.Sleep(10000);
                        }

                        processor.Stop();

                        // cause the process to recycle
                        return;
                    }
                }
                else
                {
                    Trace.TraceWarning("Starting the command processor in mantainance mode.");
                    while (this._running && MaintenanceMode.IsInMaintainanceMode)
                    {
                        Thread.Sleep(10000);
                    }
                }
            }

            TaskScheduler.UnobservedTaskException -= this.OnUnobservedTaskException;
        }

        private void OnUnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            Trace.TraceError("Unobserved task exception: \r\n{0}", e.Exception);
        }

        private bool InstrumentationEnabled
        {
            get
            {
                bool instrumentationEnabled;
                if (!bool.TryParse(RoleEnvironment.GetConfigurationSettingValue("InstrumentationEnabled"), out instrumentationEnabled))
                {
                    instrumentationEnabled = false;
                }

                return instrumentationEnabled;
            }
        }
    }
}
