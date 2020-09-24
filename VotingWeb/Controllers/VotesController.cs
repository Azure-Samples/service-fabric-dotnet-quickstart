// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

using Microsoft.Extensions.Logging;

namespace VotingWeb.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Fabric;
    using System.Fabric.Query;
    using System.Linq;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Newtonsoft.Json;

    [Produces("application/json")]
    [Route("api/[controller]")]
    public class VotesController : Controller
    {
        private readonly HttpClient httpClient;
        private readonly FabricClient fabricClient;
        private readonly string reverseProxyBaseUri;
        private readonly StatelessServiceContext serviceContext;

        public VotesController(HttpClient httpClient, StatelessServiceContext context, FabricClient fabricClient)
        {
            this.fabricClient = fabricClient;
            this.httpClient = httpClient;
            this.serviceContext = context;
            this.reverseProxyBaseUri = Environment.GetEnvironmentVariable("ReverseProxyBaseUri");
        }

        private static void log(string s)
        {
            // using (var f = System.IO.File.AppendText(("E:\\tmp\\logfk")))
            // {
            //     f.WriteLine(s);
            // }
        }

        // GET: api/Votes
        [HttpGet("")]
        public async Task<IActionResult> Get()
        {
            // (new Uri("fabric:/Voting/VotingData")
            // var x = await fabricClient.QueryManager.GetServiceListAsync());
            // Console.WriteLine(x.);

            // Console.WriteLine(x.First().);


            Uri serviceName = VotingWeb.GetVotingDataServiceName(this.serviceContext);
            // Uri proxyAddress = this.GetProxyAddress(serviceName);

            ServicePartitionList partitions = await this.fabricClient.QueryManager.GetPartitionListAsync(serviceName);

            List<KeyValuePair<string, int>> result = new List<KeyValuePair<string, int>>();

            foreach (Partition partition in partitions)
            {
                var s = await fks(((Int64RangePartitionInformation) partition.PartitionInformation).LowKey);



                string proxyUrl =
                    $"{s}/api/VoteData?PartitionKey={((Int64RangePartitionInformation) partition.PartitionInformation).LowKey}&PartitionKind=Int64Range";

                log(proxyUrl);

                using (HttpResponseMessage response = await this.httpClient.GetAsync(proxyUrl))
                {
                    if (response.StatusCode != System.Net.HttpStatusCode.OK)
                    {
                        continue;
                    }

                    result.AddRange(JsonConvert.DeserializeObject<List<KeyValuePair<string, int>>>(await response.Content.ReadAsStringAsync()));
                }
            }

            return this.Json(result);
        }

        private async Task<string> fks(long partitionKey)
        {
            log(partitionKey.ToString());
            var x = await fabricClient.ServiceManager.ResolveServicePartitionAsync(new Uri("fabric:/Voting/VotingData"),
                partitionKey);
            var s = x.Endpoints.First().Address;

            var j = JsonConvert.DeserializeAnonymousType(s, new
            {
                Endpoints = new Dictionary<string, string>()
            });


            var first = j.Endpoints.Values.First();
            log(first);
            return first;

            // return s;
        }

        // PUT: api/Votes/name
        [HttpPut("{name}")]
        public async Task<IActionResult> Put(string name)
        {
            Uri serviceName = VotingWeb.GetVotingDataServiceName(this.serviceContext);
            // Uri proxyAddress = this.GetProxyAddress(serviceName);
            long partitionKey = this.GetPartitionKey(name);

            // var x = await fabricClient.ServiceManager.ResolveServicePartitionAsync(new Uri("fabric:/Voting/VotingData"), partitionKey);
            var s = await fks(partitionKey);
            string proxyUrl = $"{s}/api/VoteData/{name}?PartitionKey={partitionKey}&PartitionKind=Int64Range";
            log(proxyUrl);

            StringContent putContent = new StringContent($"{{ 'name' : '{name}' }}", Encoding.UTF8, "application/json");
            putContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            using (HttpResponseMessage response = await this.httpClient.PutAsync(proxyUrl, putContent))
            {
                return new ContentResult()
                {
                    StatusCode = (int) response.StatusCode,
                    Content = await response.Content.ReadAsStringAsync()
                };
            }
        }

        // DELETE: api/Votes/name
        [HttpDelete("{name}")]
        public async Task<IActionResult> Delete(string name)
        {
            Uri serviceName = VotingWeb.GetVotingDataServiceName(this.serviceContext);
            // Uri proxyAddress = this.GetProxyAddress(serviceName);
            long partitionKey = this.GetPartitionKey(name);
            // var proxyAddress = 
            // string proxyUrl = $"{proxyAddress}/api/VoteData/{name}?PartitionKey={partitionKey}&PartitionKind=Int64Range";

            // long partitionKey = this.GetPartitionKey(name);

            // var x = await fabricClient.ServiceManager.ResolveServicePartitionAsync(new Uri("fabric:/Voting/VotingData"), partitionKey);
            // var s = x.Endpoints.First().Address;
            var s = await fks(partitionKey);

            string proxyUrl = $"{s}/api/VoteData/{name}?PartitionKey={partitionKey}&PartitionKind=Int64Range";
                log(proxyUrl);

            using (HttpResponseMessage response = await this.httpClient.DeleteAsync(proxyUrl))
            {
                if (response.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    return this.StatusCode((int) response.StatusCode);
                }
            }

            return new OkResult();
        }


        /// <summary>
        /// Constructs a reverse proxy URL for a given service.
        /// Example: http://localhost:19081/VotingApplication/VotingData/
        /// </summary>
        /// <param name="serviceName"></param>
        /// <returns></returns>
        // private Uri GetProxyAddress(Uri serviceName)
        // {
        //     // return new Uri($"{this.reverseProxyBaseUri}{serviceName.AbsolutePath}");
        // }

        /// <summary>
        /// Creates a partition key from the given name.
        /// Uses the zero-based numeric position in the alphabet of the first letter of the name (0-25).
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        private long GetPartitionKey(string name)
        {
            return Math.Abs(name.GetHashCode()) % 26;
            // return Char.ToUpper(name.First()) - 'A' % 25;
        }
    }
}