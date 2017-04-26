using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MfNotification.Core.Cryptography
{
    /// <summary>
    /// 加密信息
    /// </summary>
    public class CryptInfo
    {
        public string KeyStr { get; set; }

        public string IvStr { get; set; }
    }
}
