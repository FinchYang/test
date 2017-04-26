using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AecCloud.Core;
using AecCloud.Core.Domain;
using AecCloud.WebAPI.Models;

namespace AecCloud.Service.Apps
{
    public class CloudService : ICloudService
    {
        private readonly IRepository<Cloud> _appRepo;

        public CloudService(IRepository<Cloud> appRepo)
        {
            _appRepo = appRepo;
        }

        public IList<Cloud> GetAllClouds()
        {
            return _appRepo.Table.ToList();
        }

        public Cloud GetCloud()
        {
            return _appRepo.Table.FirstOrDefault(c => c.Id == 1);
        }

        public IList<Cloud> GetDefaultClouds()
        {
            return _appRepo.Table.Where(c => c.Default).ToList();
        }

        public Cloud GetCloudById(long appId)
        {
            if (appId <= 0) throw new ArgumentException("appId");
            return _appRepo.GetById(appId);
        }

        public Cloud GetCloudByName(string name)
        {
            if (String.IsNullOrWhiteSpace(name)) throw new ArgumentException("name");
            return _appRepo.Table.FirstOrDefault(c => c.Name == name);
        }

        public void InsertCloud(Cloud app)
        {
            if (app == null) throw new ArgumentNullException("app");
            _appRepo.Insert(app);
        }

        public void UpdateCloud(Cloud app)
        {
            if (app == null) throw new ArgumentNullException("app");
            _appRepo.Update(app);
        }

        public void DeleteCloud(Cloud app)
        {
            if (app == null) throw new ArgumentNullException("app");
            _appRepo.Delete(app);
        }

        public IList<Cloud> GetCloudsByUserRoles(params string[] roleNames)
        {
            var defaults = GetDefaultClouds();
            if (roleNames == null || roleNames.Length == 0)
            {
                return defaults;
            }
            if (roleNames.Contains(SystemUserRoleNames.CorperationLeaders) ||
                roleNames.Contains(SystemUserRoleNames.ProjectDirectors))
            {
                defaults.Add(GetCloudById(CloudConstants.ProjManagements));
                defaults.Add(GetCloudById(CloudConstants.SubContracts));
            }
            else if (roleNames.Contains(SystemUserRoleNames.SubContractors))
            {
                defaults.Add(GetCloudById(CloudConstants.SubContracts));
            }
            return defaults;
        }
    }
}
