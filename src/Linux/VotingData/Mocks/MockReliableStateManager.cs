// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace VotingData.Mocks
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Fabric;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.ServiceFabric.Data.Collections;
    using Microsoft.ServiceFabric.Data.Notifications;
    using Fabric = Microsoft.ServiceFabric.Data;

    public class MockReliableStateManager : Fabric.IReliableStateManagerReplica
    {
        private ConcurrentDictionary<Uri, Fabric.IReliableState> store = new ConcurrentDictionary<Uri, Fabric.IReliableState>();

        private Dictionary<Type, Type> dependencyMap = new Dictionary<Type, Type>()
        {
            {typeof(IReliableDictionary<,>), typeof(MockReliableDictionary<,>)},
            {typeof(IReliableQueue<>), typeof(MockReliableQueue<>)}
        };

        public Func<CancellationToken, Task<bool>> OnDataLossAsync { set; get; }

        public event EventHandler<NotifyTransactionChangedEventArgs> TransactionChanged;

        public event EventHandler<NotifyStateManagerChangedEventArgs> StateManagerChanged;

        public Fabric.ITransaction CreateTransaction()
        {
            return new MockTransaction();
        }

        public Task RemoveAsync(string name)
        {
            Fabric.IReliableState result;
            this.store.TryRemove(this.ToUri(name), out result);

            return Task.FromResult(true);
        }

        public Task RemoveAsync(Fabric.ITransaction tx, string name)
        {
            Fabric.IReliableState result;
            this.store.TryRemove(this.ToUri(name), out result);

            return Task.FromResult(true);
        }

        public Task RemoveAsync(string name, TimeSpan timeout)
        {
            Fabric.IReliableState result;
            this.store.TryRemove(this.ToUri(name), out result);

            return Task.FromResult(true);
        }

        public Task RemoveAsync(Fabric.ITransaction tx, string name, TimeSpan timeout)
        {
            Fabric.IReliableState result;
            this.store.TryRemove(this.ToUri(name), out result);

            return Task.FromResult(true);
        }

        public Task RemoveAsync(Uri name)
        {
            Fabric.IReliableState result;
            this.store.TryRemove(name, out result);

            return Task.FromResult(true);
        }

        public Task RemoveAsync(Uri name, TimeSpan timeout)
        {
            Fabric.IReliableState result;
            this.store.TryRemove(name, out result);

            return Task.FromResult(true);
        }

        public Task RemoveAsync(Fabric.ITransaction tx, Uri name)
        {
            Fabric.IReliableState result;
            this.store.TryRemove(name, out result);

            return Task.FromResult(true);
        }

        public Task RemoveAsync(Fabric.ITransaction tx, Uri name, TimeSpan timeout)
        {
            Fabric.IReliableState result;
            this.store.TryRemove(name, out result);

            return Task.FromResult(true);
        }

        public Task<Fabric.ConditionalValue<T>> TryGetAsync<T>(string name) where T : Fabric.IReliableState
        {
            Fabric.IReliableState item;
            bool result = this.store.TryGetValue(this.ToUri(name), out item);

            return Task.FromResult(new Fabric.ConditionalValue<T>(result, (T) item));
        }

        public Task<Fabric.ConditionalValue<T>> TryGetAsync<T>(Uri name) where T : Fabric.IReliableState
        {
            Fabric.IReliableState item;
            bool result = this.store.TryGetValue(name, out item);

            return Task.FromResult(new Fabric.ConditionalValue<T>(result, (T) item));
        }

        public Task<T> GetOrAddAsync<T>(string name) where T : Fabric.IReliableState
        {
            return Task.FromResult((T) this.store.GetOrAdd(this.ToUri(name), this.GetDependency(typeof(T))));
        }

        public Task<T> GetOrAddAsync<T>(Fabric.ITransaction tx, string name) where T : Fabric.IReliableState
        {
            return Task.FromResult((T) this.store.GetOrAdd(this.ToUri(name), this.GetDependency(typeof(T))));
        }

        public Task<T> GetOrAddAsync<T>(string name, TimeSpan timeout) where T : Fabric.IReliableState
        {
            return Task.FromResult((T) this.store.GetOrAdd(this.ToUri(name), this.GetDependency(typeof(T))));
        }

        public Task<T> GetOrAddAsync<T>(Fabric.ITransaction tx, string name, TimeSpan timeout) where T : Fabric.IReliableState
        {
            return Task.FromResult((T) this.store.GetOrAdd(this.ToUri(name), this.GetDependency(typeof(T))));
        }

        public Task<T> GetOrAddAsync<T>(Uri name) where T : Fabric.IReliableState
        {
            return Task.FromResult((T) this.store.GetOrAdd(name, this.GetDependency(typeof(T))));
        }

        public Task<T> GetOrAddAsync<T>(Uri name, TimeSpan timeout) where T : Fabric.IReliableState
        {
            return Task.FromResult((T) this.store.GetOrAdd(name, this.GetDependency(typeof(T))));
        }

        public Task<T> GetOrAddAsync<T>(Fabric.ITransaction tx, Uri name) where T : Fabric.IReliableState
        {
            return Task.FromResult((T) this.store.GetOrAdd(name, this.GetDependency(typeof(T))));
        }

        public Task<T> GetOrAddAsync<T>(Fabric.ITransaction tx, Uri name, TimeSpan timeout) where T : Fabric.IReliableState
        {
            return Task.FromResult((T) this.store.GetOrAdd(name, this.GetDependency(typeof(T))));
        }

        public bool TryAddStateSerializer<T>(Fabric.IStateSerializer<T> stateSerializer)
        {
            throw new NotImplementedException();
        }

        public Fabric.IAsyncEnumerator<Fabric.IReliableState> GetAsyncEnumerator()
        {
            return new MockAsyncEnumerator<Fabric.IReliableState>(this.store.Values.GetEnumerator());
        }

        public void Initialize(StatefulServiceInitializationParameters initializationParameters)
        {
        }

        public Task<IReplicator> OpenAsync(ReplicaOpenMode openMode, IStatefulServicePartition partition, CancellationToken cancellationToken)
        {
            return null;
        }

        public Task ChangeRoleAsync(ReplicaRole newRole, CancellationToken cancellationToken)
        {
            return Task.FromResult(true);
        }

        public Task CloseAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(true);
        }

        public void Abort()
        {
        }

        public Task BackupAsync(Func<Fabric.BackupInfo, CancellationToken, Task<bool>> backupCallback)
        {
            throw new NotImplementedException();
        }

        public Task BackupAsync(
            Fabric.BackupOption option, TimeSpan timeout, CancellationToken cancellationToken,
            Func<Fabric.BackupInfo, CancellationToken, Task<bool>> backupCallback)
        {
            throw new NotImplementedException();
        }

        public Task RestoreAsync(string backupFolderPath)
        {
            throw new NotImplementedException();
        }

        public Task RestoreAsync(string backupFolderPath, Fabric.RestorePolicy restorePolicy, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task ClearAsync(Fabric.ITransaction tx)
        {
            this.store.Clear();
            return Task.FromResult(true);
        }

        public Task ClearAsync()
        {
            this.store.Clear();
            return Task.FromResult(true);
        }

        private Fabric.IReliableState GetDependency(Type t)
        {
            Type mockType = this.dependencyMap[t.GetGenericTypeDefinition()];

            return (Fabric.IReliableState) Activator.CreateInstance(mockType.MakeGenericType(t.GetGenericArguments()));
        }

        private Uri ToUri(string name)
        {
            return new Uri("mock://" + name, UriKind.Absolute);
        }
    }
}