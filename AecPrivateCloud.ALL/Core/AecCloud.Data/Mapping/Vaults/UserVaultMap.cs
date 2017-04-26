﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AecCloud.Core.Domain.Vaults;

namespace AecCloud.Data.Mapping.Vaults
{
    public class UserVaultMap : EntityMap<UserVault>
    {
        public UserVaultMap()
        {
            ToTable("UserVault");
            HasKey(c => c.Id);
            Property(c => c.UserId).IsRequired();
            Property(c => c.VaultId).IsRequired();
            //HasRequired(c => c.Vault).WithMany().HasForeignKey(c => c.VaultId).WillCascadeOnDelete(false);
        }
    }
}
