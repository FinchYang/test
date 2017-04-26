using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AecCloud.Core.Domain;
using AecCloud.Core.Domain.Vaults;

namespace AecCloud.Service.Apps
{
    public interface ICloudService
    {
        IList<Cloud> GetAllClouds();

        IList<Cloud> GetDefaultClouds();

        Cloud GetCloud();

        Cloud GetCloudById(long appId);

        Cloud GetCloudByName(string name);

        //Cloud GetAppByVaultTemplate(int templateId);

        //Cloud GetCloudByVault(int vaultId);

        void InsertCloud(Cloud app);

        void UpdateCloud(Cloud app);

        void DeleteCloud(Cloud app);


        //IList<VaultTemplate> GetTemplates(int cloudId);

        IList<Cloud> GetCloudsByUserRoles(params string[] roleNames);
    }
}
