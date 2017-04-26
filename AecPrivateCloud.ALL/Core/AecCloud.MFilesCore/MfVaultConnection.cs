using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MFilesAPI;

namespace AecCloud.MFilesCore
{
    public class MfVaultConnection
    {
        public string Name { get; set; }

        public string Guid { get; set; }

        public string IconPath { get; set; }

        public override string ToString()
        {
            return Name + " # " + Guid;
        }

        public Vault BindToVault()
        {
            var app = ClientUtils.GetClientApp();
            return app.BindToVault(Name, IntPtr.Zero, true, true);
        }
    }
}
