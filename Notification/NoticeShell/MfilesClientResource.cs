using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MFilesAPI;
using Notification.Client;

namespace Notice
{
   
    public class MfilesClientResource
    {
        private readonly static Mutex _mfilesconnectMutex = new Mutex();
        public MFilesClientApplication MFilesClientApplication { set; get; }
        public VaultConnections VaultConnections { set; get; }
        public string Error { set; get; }

        public MfilesClientResource()
        {
            Error = string.Empty;
        }

        public bool IsError()
        {
            return Error != string.Empty;
        }
        public static MfilesClientResource GetMfilesClientResource()
        {
            var mcr = new MfilesClientResource();
            lock (_mfilesconnectMutex)
            {
                try
                {
                    mcr.MFilesClientApplication = new MFilesClientApplication();
                    mcr.VaultConnections = mcr.MFilesClientApplication.GetVaultConnections();
                }
                catch (Exception ex)
                {
                    mcr.Error="GetMfilesClientResource"+ex.Message;
                }
            }
            return mcr;
        }
    }
}
