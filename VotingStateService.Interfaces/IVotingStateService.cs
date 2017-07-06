using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Services.Remoting;

namespace VotingStateService.Interfaces
{
    public interface IVotingStateService : IService
    {
        Task AddVoteAsync(string name);
        Task<IEnumerable<KeyValuePair<string, int>>> GetVoteCountAsync();
        Task<bool> ClearVoteCountAsync(string key);
    }

}
