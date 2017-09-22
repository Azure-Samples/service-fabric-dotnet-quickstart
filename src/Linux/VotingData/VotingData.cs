using System;
using System.Collections.Generic;
using System.Fabric;
using System.IO;
using System.Reflection;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.ServiceFabric.Services.Communication.AspNetCore;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using Microsoft.ServiceFabric.Data;

namespace VotingData
{
    /// <summary>
    /// The FabricRuntime creates an instance of this class for each service type instance. 
    /// </summary>
    internal sealed class VotingData : StatefulService
    {
        public VotingData(StatefulServiceContext context)
            : base(context)
        {
             Console.WriteLine("Stateful constructor.");
         }

        /// <summary>
        /// Optional override to create listeners (like tcp, http) for this service instance.
        /// </summary>
        /// <returns>The collection of listeners.</returns>
        protected override IEnumerable<ServiceReplicaListener> CreateServiceReplicaListeners()
        {
            return new ServiceReplicaListener[]
            {
                new ServiceReplicaListener(serviceContext =>
                    new KestrelCommunicationListener(serviceContext, (url, listener) =>
                    {
                        try
                        {

                            Console.WriteLine("Starting Kestrel on {0}", url);

                            return WebHost
                                .CreateDefaultBuilder()
                                .ConfigureServices(
                                    services => services
                                        .AddSingleton<StatefulServiceContext>(serviceContext)
                                                .AddSingleton<IReliableStateManager>(this.StateManager))
                                        .UseServiceFabricIntegration(listener, ServiceFabricIntegrationOptions.UseUniqueServiceUrl)
                                .UseStartup<Startup>()
                                .UseUrls(url)
                                .Build();

                        }
                        catch(Exception ex)
                        {
                            Console.WriteLine("Web HostBuilder exception: {0}", ex);
                            throw;
                        }
                    }
                    ))
            };
        }
    }
}
