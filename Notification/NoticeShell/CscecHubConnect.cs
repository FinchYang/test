using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR.Client;

namespace Notice
{
    class CscecHubConnect:HubConnection
    {
        public CscecHubConnect(string url) : base(url)
        {
        }

        public CscecHubConnect(string url, bool useDefaultUrl) : base(url, useDefaultUrl)
        {
        }

        public CscecHubConnect(string url, string queryString) : base(url, queryString)
        {
        }

        public CscecHubConnect(string url, string queryString, bool useDefaultUrl) : base(url, queryString, useDefaultUrl)
        {
        }

        public CscecHubConnect(string url, IDictionary<string, string> queryString) : base(url, queryString)
        {
        }

        public CscecHubConnect(string url, IDictionary<string, string> queryString, bool useDefaultUrl) : base(url, queryString, useDefaultUrl)
        {
        }
        
    }
}
