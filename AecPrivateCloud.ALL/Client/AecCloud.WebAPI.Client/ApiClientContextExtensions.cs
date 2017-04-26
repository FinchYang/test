using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AecCloud.WebAPI.Client
{
    public static class ApiClientContextExtensions
    {
        public static AuthenticationClient GetAuthClient(this ApiClientContext context)
        {
            return context.GetClient(() => new AuthenticationClient(context.HttpClient));
        }

        public static CloudClient GetCloudClient(this ApiClientContext context)
        {
            return context.GetClient(() => new CloudClient(context.HttpClient));
        }

        public static TokenClient GetTokenClient(this ApiClientContext context)
        {
            return context.GetClient(() => new TokenClient(context.HttpClient));
        }

        public static ProjectClient GetProjectClient(this ApiClientContext context)
        {
            return context.GetClient(() => new ProjectClient(context.HttpClient));
        }

        public static VaultClient GetVaultClient(this ApiClientContext context)
        {
            return context.GetClient(() => new VaultClient(context.HttpClient));
        }

        public static GeneralClient GetGeneralClient(this ApiClientContext context)
        {
            return context.GetClient(() => new GeneralClient(context.HttpClient));
        }

        internal static TClient GetClient<TClient>(this ApiClientContext apiClientContext, Func<TClient> valueFactory)
        {

            return (TClient)apiClientContext.Clients.GetOrAdd(typeof(TClient), k => valueFactory());
        }
    }
}
