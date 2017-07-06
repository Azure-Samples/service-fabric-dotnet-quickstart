using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Data.Collections;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using VotingStateService.Interfaces;
using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Services.Remoting.Runtime;

namespace VotingStateService
{
    /// <summary>
    /// An instance of this class is created for each service replica by the Service Fabric runtime.
    /// </summary>
    //internal sealed class VotingStateService : StatefulService
    internal sealed class VotingStateService : StatefulService, IVotingStateService
    {
        public VotingStateService(StatefulServiceContext context)
            : base(context)
        { }

        protected override IEnumerable<ServiceReplicaListener> CreateServiceReplicaListeners()
        {
            return new[]
            {
                new ServiceReplicaListener(ctx => this.CreateServiceRemotingListener(ctx))
            };
        }

        protected override async Task RunAsync(CancellationToken cancellationToken)
        {
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();
                await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
            }
        }

        public async Task AddVoteAsync(string name)
        {
            var votesDictionary = await this.StateManager.GetOrAddAsync<IReliableDictionary<string, int>>("counts");

            using (ITransaction tx = this.StateManager.CreateTransaction())
            {
                await votesDictionary.AddOrUpdateAsync(tx, name, 1, (key, oldvalue) => oldvalue + 1);
                await tx.CommitAsync();
            }

        }

        public async Task<IEnumerable<KeyValuePair<string, int>>> GetVoteCountAsync()
        {
            var ct = new CancellationToken();

            var votesDictionary = await this.StateManager.GetOrAddAsync<IReliableDictionary<string, int>>("counts");

            using (ITransaction tx = this.StateManager.CreateTransaction())
            {
                var list = await votesDictionary.CreateEnumerableAsync(tx);

                var enumerator = list.GetAsyncEnumerator();

                var result = new List<KeyValuePair<string, int>>();

                while (await enumerator.MoveNextAsync(ct))
                {
                    result.Add(enumerator.Current);
                }

                return result;
            }
        }

        public async Task<bool> ClearVoteCountAsync(string key)
        {
            var votesDictionary = await this.StateManager.GetOrAddAsync<IReliableDictionary<string, int>>("counts");

            using (ITransaction tx = this.StateManager.CreateTransaction())
            {
                if (await votesDictionary.ContainsKeyAsync(tx, key))
                {
                    await votesDictionary.TryRemoveAsync(tx, key);
                    await tx.CommitAsync();
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
    }
}
