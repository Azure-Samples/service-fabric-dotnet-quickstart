using Microsoft.ServiceFabric.Services.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VotingWeb
{
    public static class ServiceNameConverter
    {

        public static string ToDnsName(long partitionKey, Uri serviceName)
        {
            return partitionKey + "." + String.Join(".", serviceName.Segments
                .Reverse()
                .Select(x => x.Replace("/",""))
                .Where(x => !String.IsNullOrWhiteSpace(x)))
            + ".fabric";
        }

        public static Uri ToFabricName(string serviceDns)
        {
            string result = String.Join("/", serviceDns.Split('.').Skip(1).Reverse());

            if (result.StartsWith("fabric/"))
            {
                result = result.Replace("fabric/", "fabric:/");
            }
            else
            {
                result = result + "fabric:/";
            }

            return new Uri(result);
        }

        public static ServicePartitionKey GetPartitionKey(string serviceDns)
        {
            long int64PartitionKey;

            if (!Int64.TryParse(serviceDns.Split('.').First(), out int64PartitionKey))
            {
                throw new ArgumentException("service DNS name must have an int-64 partition key as the first subdomain. Named and singleton partitions aren't supported here.");
            }

            return new ServicePartitionKey(int64PartitionKey);
        }
    }
}
