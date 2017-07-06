using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Net.Http.Headers;
using System.Web.Http;
using System.Fabric;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using VotingStateService.Interfaces;
using System.Threading.Tasks;

namespace VotingService.Controllers
{
    [ServiceRequestActionFilter]
    public class VotesController : ApiController
    {
        // Used for health checks.
        public static long _requestCount = 0L;

        // Holds the votes and counts. NOTE: THIS IS NOT THREAD SAFE FOR THE PURPOSES OF THE LAB ONLY.
        // static Dictionary<string, int> _counts = new Dictionary<string, int>();
        private readonly IVotingStateService _stateService;

        public VotesController()
        {
            _stateService = ServiceProxy.Create<IVotingStateService>(
                new Uri("fabric:/Voting/VotingStateService"),
                new Microsoft.ServiceFabric.Services.Client.ServicePartitionKey(0));
        }

        [HttpGet]
        [Route("api/votes")]
        public async Task<HttpResponseMessage> Get()
        {
            string activityId = Guid.NewGuid().ToString();
            ServiceEventSource.Current.ServiceRequestStart("VotesController.Get", activityId);

            Interlocked.Increment(ref _requestCount);

            // NEW -- callout to the service to get the votes
            var votes = await _stateService.GetVoteCountAsync();

            var response = Request.CreateResponse(HttpStatusCode.OK, votes);
            response.Headers.CacheControl = new CacheControlHeaderValue() { NoCache = true, MustRevalidate = true };

            ServiceEventSource.Current.ServiceRequestStop("VotesController.Get", activityId);

            return response;
        }

        [HttpPost]
        [Route("api/{key}")]
        public async Task<HttpResponseMessage> Post(string key)
        {
            string activityId = Guid.NewGuid().ToString();
            ServiceEventSource.Current.ServiceRequestStart("VotesController.Post", activityId);

            Interlocked.Increment(ref _requestCount);

            // NEW -- callout to the service to get the votes
            await _stateService.AddVoteAsync(key);


            ServiceEventSource.Current.ServiceRequestStop("VotesController.Post", activityId);

            return Request.CreateResponse(HttpStatusCode.NoContent);
        }

        [HttpDelete]
        [Route("api/{key}")]
        public async Task<HttpResponseMessage> Delete(string key)
        {
            string activityId = Guid.NewGuid().ToString();
            ServiceEventSource.Current.ServiceRequestStart("VotesController.Delete", activityId);

            Interlocked.Increment(ref _requestCount);

            if (await _stateService.ClearVoteCountAsync(key))
            {
                ServiceEventSource.Current.ServiceRequestStop("VotesController.Delete", activityId);
                return Request.CreateResponse(HttpStatusCode.OK);
            }

            ServiceEventSource.Current.ServiceRequestStop("VotesController.Delete", activityId);

            return Request.CreateResponse(HttpStatusCode.NotFound);
        }

        [HttpGet]
        [Route("api/{file}")]
        public HttpResponseMessage GetFile(string file)
        {
            string activityId = Guid.NewGuid().ToString();
            ServiceEventSource.Current.ServiceRequestStart("VotesController.GetFile", activityId);

            string response = null;
            string responseType = "text/html";

            Interlocked.Increment(ref _requestCount);

            // Validate file name.
            if ("index.html" == file)
            {
                string path = Path.Combine(FabricRuntime.GetActivationContext().GetCodePackageObject("Code").Path, "index.html");
                response = File.ReadAllText(path);
            }

            ServiceEventSource.Current.ServiceRequestStop("VotesController.GetFile", activityId);

            if (null != response)
                return Request.CreateResponse(HttpStatusCode.OK, response, responseType);
            else
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, "File");
        }


    }

}
