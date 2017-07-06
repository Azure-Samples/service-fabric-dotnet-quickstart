using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using System.Fabric.Health;
using System.Fabric.Description;

namespace VotingService
{
    /// <summary>
    /// The FabricRuntime creates an instance of this class for each service type instance. 
    /// </summary>
    internal sealed class VotingService : StatelessService
    {
        private TimeSpan _interval = TimeSpan.FromSeconds(30);
        private long _lastCount = 0L;
        private DateTime _lastReport = DateTime.UtcNow;
        private Timer _healthTimer = null;
        private FabricClient _client = null;

        public VotingService(StatelessServiceContext context)
            : base(context)
        {
            // Create the timer here, so we can do a change operation on it later, avoiding creating/disposing of the 
            // timer.
            _healthTimer = new Timer(ReportHealthAndLoad, null, Timeout.Infinite, Timeout.Infinite);

            context.CodePackageActivationContext.ConfigurationPackageModifiedEvent += CodePackageActivationContext_ConfigurationPackageModifiedEvent;
        }

        /// <summary>
        /// Optional override to create listeners (like tcp, http) for this service instance.
        /// </summary>
        /// <returns>The collection of listeners.</returns>
        protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
        {
            return new ServiceInstanceListener[]
            {
                new ServiceInstanceListener(serviceContext => new OwinCommunicationListener(Startup.ConfigureApp, serviceContext, ServiceEventSource.Current, "ServiceEndpoint"))
            };
        }


        // report health

        protected override Task OnOpenAsync(CancellationToken cancellationToken)
        {
            //_client = new FabricClient();
            //_healthTimer.Change(_interval, _interval);
            //return base.OnOpenAsync(cancellationToken);

            // Force a call to LoadConfiguration because we missed the first event callback.
            LoadConfiguration();

            _client = new FabricClient();
            _healthTimer = new Timer(ReportHealthAndLoad, null, _interval, _interval);
            return base.OnOpenAsync(cancellationToken);

        }

        public void ReportHealthAndLoad(object notused)
        {
            // Calculate the values and then remember current values for the next report.
            long total = Controllers.VotesController._requestCount;
            long diff = total - _lastCount;
            long duration = Math.Max((long)DateTime.UtcNow.Subtract(_lastReport).TotalSeconds, 1L);
            long rps = diff / duration;
            _lastCount = total;
            _lastReport = DateTime.UtcNow;

            // Create the health information for this instance of the service and send report to Service Fabric.
            HealthInformation hi = new HealthInformation("VotingServiceHealth", "Heartbeat", HealthState.Ok)
            {
                TimeToLive = _interval.Add(_interval),
                Description = $"{diff} requests since last report. RPS: {rps} Total requests: {total}.",
                RemoveWhenExpired = false,
                SequenceNumber = HealthInformation.AutoSequenceNumber
            };
            var sshr = new StatelessServiceInstanceHealthReport(Context.PartitionId, Context.InstanceId, hi);
            _client.HealthManager.ReportHealth(sshr);

            // Report the load
            Partition.ReportLoad(new[] { new LoadMetric("RPS", (int)rps) });

            // Log the health report.
            ServiceEventSource.Current.HealthReport(hi.SourceId, hi.Property, Enum.GetName(typeof(HealthState), hi.HealthState), Context.PartitionId, Context.ReplicaOrInstanceId, hi.Description);


            //// Report failing health report to cause rollback.
            //var nodeList = _client.QueryManager.GetNodeListAsync(Context.NodeContext.NodeName).GetAwaiter().GetResult();
            //var node = nodeList[0];
            //if ("4" == node.UpgradeDomain || "3" == node.UpgradeDomain || "2" == node.UpgradeDomain)
            //{
            //    hi = new HealthInformation("VotingServiceHealth", "Error_Heartbeat", HealthState.Error);
            //    hi.TimeToLive = _interval.Add(_interval);
            //    hi.Description = $"Bogus health error to force rollback.";
            //    hi.RemoveWhenExpired = true;
            //    hi.SequenceNumber = HealthInformation.AutoSequenceNumber;
            //    sshr = new StatelessServiceInstanceHealthReport(Context.PartitionId, Context.InstanceId, hi);
            //    _client.HealthManager.ReportHealth(sshr);
            //}


        }

        private void CodePackageActivationContext_ConfigurationPackageModifiedEvent(object sender, PackageModifiedEventArgs<ConfigurationPackage> e)
        {
            ServiceEventSource.Current.Message("CodePackageActivationContext_ConfigurationPackageModifiedEvent");
            LoadConfiguration();
        }

        private void LoadConfiguration()
        {
            ServiceEventSource.Current.Message("LoadConfiguration");

            // Get the Health Check Interval configuration value.
            ConfigurationPackage pkg = Context.CodePackageActivationContext.GetConfigurationPackageObject("Config");
            if (null != pkg)
            {
                if (true == pkg.Settings?.Sections?.Contains("Health"))
                {
                    ConfigurationSection settings = pkg.Settings.Sections["Health"];
                    if (true == settings?.Parameters.Contains("HealthCheckIntervalSeconds"))
                    {
                        int value = 0;
                        ConfigurationProperty prop = settings.Parameters["HealthCheckIntervalSeconds"];
                        if (int.TryParse(prop?.Value, out value))
                        {
                            _interval = TimeSpan.FromSeconds(Math.Max(30, value));
                            _healthTimer.Change(_interval, _interval);
                        }

                        ServiceEventSource.Current.HealthReportIntervalChanged("VotingServiceHealth", "IntervalChanged", Context.PartitionId, Context.ReplicaOrInstanceId, (int)_interval.TotalSeconds);
                    }
                }
            }
        }

    }
}
