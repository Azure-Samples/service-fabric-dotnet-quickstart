using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using VotingStateService.Interfaces;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using System.Collections.Generic;

namespace VotingWeb.Controllers
{
    [Produces("application/json")]
    [Route("api/Votes")]
    public class VotesController : Controller
    {
        private readonly IVotingStateService _stateService;

        public VotesController()
        {
            _stateService = ServiceProxy.Create<IVotingStateService>(
                new Uri("fabric:/Voting/VotingStateService"),
                new Microsoft.ServiceFabric.Services.Client.ServicePartitionKey(0));
        }

        // GET: api/Votes
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            IEnumerable<KeyValuePair<string, int>> votes;

            try
            {
                votes = await _stateService.GetVoteCountAsync();
            }
            catch (Exception ex)
            {
                return new ContentResult() { Content = ex.Message, StatusCode = 500 };
            }

            return Json(votes);
        }

        // POST: api/Votes/name
        [HttpPost("{id}")]
        public async Task<IActionResult> Post(string id)
        {
            try
            {
                await _stateService.AddVoteAsync(id);
            }
            catch (Exception ex)
            {
                return new ContentResult() { Content = ex.Message, StatusCode = 500 };
            }

            return Json(id);
        }

        // DELETE: api/Votes/name
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                if (await _stateService.ClearVoteCountAsync(id))
                {
                    return Json(id);
                }

                return new NotFoundObjectResult(id);
            }
            catch (Exception ex)
            {
                return new ContentResult() { Content = ex.Message, StatusCode = 500 };
            }

        }
    }
}
