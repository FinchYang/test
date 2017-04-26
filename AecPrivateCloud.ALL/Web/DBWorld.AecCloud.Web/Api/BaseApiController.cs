using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using AecCloud.BaseCore;
using Microsoft.AspNet.Identity;

namespace DBWorld.AecCloud.Web.Api
{
    public abstract class BaseApiController : ApiController
    {
        /// <summary>
        /// 端口映射可能会有问题
        /// </summary>
        /// <returns></returns>
        protected internal string GetHost()
        {
            return Utility.GetHost(Request.RequestUri);
        }

        protected internal long GetUserId()
        {
            return User.Identity.GetUserId<long>();
        }

        protected internal string GetUserName()
        {
            return User.Identity.GetUserName();
        }
    }
}
