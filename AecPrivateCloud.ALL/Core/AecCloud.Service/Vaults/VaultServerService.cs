using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AecCloud.Core;
using AecCloud.Core.Domain.Vaults;

namespace AecCloud.Service.Vaults
{
    public class VaultServerService : IVaultServerService
    {
        private readonly IRepository<VaultServer> _serverRepo;

        public VaultServerService(IRepository<VaultServer> serverRepo)
        {
            _serverRepo = serverRepo;
        }
        public VaultServer GetServer()
        {
            return (from s in _serverRepo.Table orderby s.Id descending select s).FirstOrDefault();
            //return _serverRepo.Table.ToList().LastOrDefault();
        }

        public ICollection<VaultServer> GetServers()
        {
            return _serverRepo.Table.ToList();
        }

        public VaultServer GetServerById(int serverId)
        {
            return _serverRepo.GetById(serverId);
        }

        public void AddServer(VaultServer server)
        {
            if (server == null) throw new ArgumentNullException("server");
            _serverRepo.Insert(server);
        }

        public void UpdateServer(VaultServer server)
        {
            if (server == null) throw new ArgumentNullException("server");
            _serverRepo.Update(server);
        }

        public void DeleteServer(VaultServer server)
        {
            if (server == null) throw new ArgumentNullException("server");
            _serverRepo.Delete(server);
        }

        public VaultServer GetServerByHost(string hostOrIp)
        {
            if (String.IsNullOrWhiteSpace(hostOrIp)) throw new ArgumentException("hostOrIp");
            return _serverRepo.Table.FirstOrDefault(c => c.Ip == hostOrIp);
        }


        public VaultServer GetServerByLocalHost(string localHostOrIp)
        {
            if (String.IsNullOrWhiteSpace(localHostOrIp)) throw new ArgumentException("localHostOrIp");
            return _serverRepo.Table.FirstOrDefault(c => c.LocalIp == localHostOrIp);
        }
        /// <summary>
        /// todo
        /// </summary>
        /// <returns></returns>
        public string GetVaultMainDataPath()
        {
            return @"C:\Program Files\M-Files\Server Vaults\";
        }
    }
}
