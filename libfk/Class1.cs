using Microsoft.ServiceFabric.Services.Remoting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace libfk
{
    public interface IFK: IService
    {
        Task<bool> RemoveSnapshot(string avsName, string snapshot);

        Task<int> GetVhdReplicaNum(string vhdName);

        Task<int> SetVhdReplicaNum(string vhdName, int n);
    }
}
