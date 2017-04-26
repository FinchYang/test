using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace AecCloud.BaseCore
{
    public class AecCloudCrypto
    {
        private const int PBKDF2IterCount = 0x3e8;
        private const int PBKDF2SubkeyLength = 0x20;
        private static readonly byte[] Salts = new byte[] { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 };

        [MethodImpl(MethodImplOptions.NoOptimization)]
        private static bool ByteArraysEqual(byte[] a, byte[] b)
        {
            if (object.ReferenceEquals(a, b))
            {
                return true;
            }
            if (((a == null) || (b == null)) || (a.Length != b.Length))
            {
                return false;
            }
            bool flag = true;
            for (int i = 0; i < a.Length; i++)
            {
                flag &= a[i] == b[i];
            }
            return flag;
        }

        public static string HashPassword(string password)
        {
            byte[] salt;
            byte[] buffer2;
            if (password == null)
            {
                throw new ArgumentNullException("password");
            }
            using (var bytes = new Rfc2898DeriveBytes(password, Salts, PBKDF2IterCount))
            {
                salt = bytes.Salt;
                buffer2 = bytes.GetBytes(PBKDF2SubkeyLength);
            }
            var dst = new byte[0x31];
            Buffer.BlockCopy(salt, 0, dst, 1, Salts.Length);
            Buffer.BlockCopy(buffer2, 0, dst, 0x11, PBKDF2SubkeyLength);
            return Convert.ToBase64String(dst);
        }

        public static bool VerifyHashedPassword(string hashedPassword, string password)
        {
            byte[] buffer4;
            if (hashedPassword == null)
            {
                return false;
            }
            if (password == null)
            {
                throw new ArgumentNullException("password");
            }
            byte[] src = Convert.FromBase64String(hashedPassword);
            if ((src.Length != 0x31) || (src[0] != 0))
            {
                return false;
            }
            var dst = new byte[Salts.Length];
            Buffer.BlockCopy(src, 1, dst, 0, Salts.Length);
            var buffer3 = new byte[PBKDF2SubkeyLength];
            Buffer.BlockCopy(src, 0x11, buffer3, 0, PBKDF2SubkeyLength);

            using (var bytes = new Rfc2898DeriveBytes(password, dst, PBKDF2IterCount))
            {
                buffer4 = bytes.GetBytes(PBKDF2SubkeyLength);
            }
            return ByteArraysEqual(buffer3, buffer4);
        }

        internal static byte[] GetBytes(string hashedPassword)
        {
            if (hashedPassword == null)
            {
                throw new ArgumentNullException("hashedPassword");
            }
            byte[] src = Convert.FromBase64String(hashedPassword);
            if ((src.Length != 0x31) || (src[0] != 0))
            {
                return null;
            }
            byte[] dst = new byte[Salts.Length];
            Buffer.BlockCopy(src, 1, dst, 0, Salts.Length);
            byte[] buffer3 = new byte[PBKDF2SubkeyLength];
            Buffer.BlockCopy(src, 0x11, buffer3, 0, PBKDF2SubkeyLength);
            return buffer3;
        }

        public static bool VerifyHashEqual(string hashed1, string hashed2)
        {
            var bytes1 = GetBytes(hashed1);
            var bytes2 = GetBytes(hashed2);
            return ByteArraysEqual(bytes1, bytes2);
        }
    }
}
