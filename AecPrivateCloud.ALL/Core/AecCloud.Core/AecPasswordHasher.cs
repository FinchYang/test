using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AecCloud.BaseCore;
using Microsoft.AspNet.Identity;

namespace AecCloud.Core
{
    public class AecPasswordHasher : IPasswordHasher
    {
        public virtual string HashPassword(string password)
        {
            return AecCloudCrypto.HashPassword(password);
        }

        public virtual PasswordVerificationResult VerifyHashedPassword(string hashedPassword, string providedPassword)
        {
            if (AecCloudCrypto.VerifyHashedPassword(hashedPassword, providedPassword))
            {
                return PasswordVerificationResult.Success;
            }
            return PasswordVerificationResult.Failed;
        }
    }
}
