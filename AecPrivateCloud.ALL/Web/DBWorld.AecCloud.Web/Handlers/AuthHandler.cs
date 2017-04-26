using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace DBWorld.AecCloud.Web.Handlers
{
    public class AuthHandler : DelegatingHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (request.IsLocal())
            {
                return base.SendAsync(request, cancellationToken);
            }
            try
            {
                if (request.GetOwinEnvironment().ContainsKey("server.IsLocal"))
                {
                    var local = (bool) request.GetOwinEnvironment()["server.IsLocal"];
                    if (local) return base.SendAsync(request, cancellationToken);
                }
            }
            catch
            {
            }
            var hasKey = IsAuthenticated(request);
            if (!hasKey)
            {
                var res = new HttpResponseMessage(HttpStatusCode.Unauthorized);
                res.Headers.Add("WWW-ApiKey", "Please provide your apikey...");
                return Task.FromResult(res);
            }
            return base.SendAsync(request, cancellationToken);
        }

        private static readonly string WebClientKey = "DBWorldWeb";

        bool IsValidApiKey(string apiKey, string controllerName, string actionName)
        {
            if (controllerName == null || actionName == null) return true;
            switch (controllerName)
            {
                case "ACCOUNT":
                    if (actionName == "EmailActivate".ToUpper()) return apiKey == WebClientKey;
                    if (actionName == "SetPassword".ToUpper()) return apiKey == WebClientKey;
                    if (actionName == "Register".ToUpper()) return apiKey == WebClientKey;
                    if (actionName == "SendActivateEmail".ToUpper()) return apiKey == WebClientKey;
                    //if (actionName == "UserPrivate".ToUpper()) return apiKey == CloudClientKey;
                    return true;
                default:
                    return true;
            }
        }

        bool IsAuthenticated(HttpRequestMessage req)
        {
            string controllerName = null;
            string actionName = null;
            var routeData = req.GetRouteData();
            if (routeData != null && routeData.Values["controller"] != null)
            {
                controllerName = ((string)routeData.Values["controller"]).ToUpper();
                if (routeData.Values["action"] != null)
                {
                    actionName = ((string) routeData.Values["action"]).ToUpper();
                }
            }
            if (controllerName == null || actionName == null) return true;
            if (controllerName == "ProjectMembers".ToUpper())
            {
                if (actionName == "InviteContractMemberByEmail".ToUpper()
                    || actionName == "InviteMemberByEmailAnonymous".ToUpper()
                    || actionName == "SendInvitationEmail".ToUpper())
                    return true;
            }
            else if (controllerName == "Account".ToUpper())
            {
                if (actionName == "UserPrivate".ToUpper()) return true;
            }
            else if (controllerName == "FILES")
            {
                if (actionName == "UploadPreviewFiles".ToUpper()) return true;
            }
            var headers = req.Headers;
            IEnumerable<string> values = null;
            var ok = headers.TryGetValues("apikey", out values);
            if (ok)
            {
                var apiKey = values.FirstOrDefault();
                var isWeb = IsValidApiKey(apiKey, controllerName, actionName);
                if (!isWeb) return false;
                return !String.IsNullOrWhiteSpace(apiKey);
            }

            return false;
        }
    }
    
}