using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AecCloud.Core.Domain.Vaults;

namespace AecCloud.Service.Vaults
{
    public interface IVaultServerService
    {
        string GetVaultMainDataPath();
        //需要根据服务器的情况选择
        VaultServer GetServer();

        ICollection<VaultServer> GetServers();

        VaultServer GetServerById(int serverId);

        void AddServer(VaultServer server);

        void UpdateServer(VaultServer server);

        void DeleteServer(VaultServer server);

        VaultServer GetServerByHost(string hostOrIp);

        VaultServer GetServerByLocalHost(string localHostOrIp);

    }
}
