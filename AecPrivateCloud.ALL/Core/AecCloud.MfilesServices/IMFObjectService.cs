using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AecCloud.Core;
using AecCloud.Core.Domain.Vaults;
using AecCloud.MFilesCore.Metadata;

namespace AecCloud.MfilesServices
{
    public interface IMFObjectService
    {
        MFObject GetMFObject<T>(MFilesVault vault, MetadataAliases aliases, T entity) where T : InternalEntity;

        void Create<T>(MFilesVault vault, MetadataAliases aliases, T entity) where T : InternalEntity;

        //void CreateWithDialog(MFilesVault vault, MFObject obj);

        void Update<T>(MFilesVault vault, MetadataAliases aliases, T entity) where T : InternalEntity;

        //void UpdateWithDialog(MFilesVault vault, MFObject obj);

        void Delete(MFilesVault vault, int objType, int objId);

        MFProjectParty GetParty(MFilesVault vault, string partyName, MetadataAliases aliases, int? currentUserId=null);

        MfContact GetContact(MFilesVault vault, MetadataAliases aliases, string userName);

        MfContact GetContactByUserId(MFilesVault vault, MetadataAliases aliases, int userId);

        MfContact GetContactByContactId(MFilesVault vault, MetadataAliases aliases, int contactId);

        MFDownloadFile DownloadFile(MFilesVault vault, int objType, int objId, int fileId);

        int GetObjectVersion(MFilesVault vault, int objType, int objId);

    }
}
