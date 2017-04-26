using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Reflection;
using System.Web.Http;
using System.Web.Mvc;
using AecCloud.Core;
using AecCloud.Core.Domain;
using AecCloud.Data;
using AecCloud.MfilesServices;
using AecCloud.Service;
using AecCloud.Service.Apps;
using AecCloud.Service.Projects;
using AecCloud.Service.Users;
using AecCloud.Service.Vaults;
using Autofac;
using Autofac.Integration.Mvc;
using Autofac.Integration.WebApi;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using Owin;

namespace DBWorld.AecCloud.Web
{
    public class AutofacConfig
    {
        public static IContainer Container = null;

        //public static T Resolve<T>(ILifetimeScope scope = null)
        //{
        //    if (Container == null && scope == null)
        //    {
        //        throw new ArgumentException("scope或者Container未初始化！");
        //    }
        //    try
        //    {
        //        var res = Container.Resolve<T>();
        //        if (!EqualityComparer<T>.Default.Equals(default(T), res)) return res;
        //    }
        //    catch
        //    {
        //    }
        //    if (scope == null)
        //    {
        //        scope = Container.BeginLifetimeScope();
        //    }
        //    return scope.Resolve<T>();
        //}

        public static void Initialize(HttpConfiguration config, IAppBuilder app)
        {
            var builder = new ContainerBuilder();

            RegisterTypes(builder);

            Container = builder.Build();

            DependencyResolver.SetResolver(new AutofacDependencyResolver(Container));

            config.DependencyResolver = new AutofacWebApiDependencyResolver(Container);

            app.UseAutofacMiddleware(Container);

        }

        // to make sure registered types using autofac can be used in owin context,
        // types' instances should be created per lifetimescope, not per request
        private static void RegisterTypes(ContainerBuilder builder)
        {
            builder.RegisterControllers(Assembly.GetExecutingAssembly());

            builder.RegisterApiControllers(Assembly.GetExecutingAssembly());

            // EF DbContext
            builder.RegisterType<EntityContext>().As<IDbContext>().InstancePerLifetimeScope(); //InstancePerApiRequest

            // Register repositories by using Autofac's OpenGenerics feature
            // More info: http://code.google.com/p/autofac/wiki/OpenGenerics
            builder.RegisterGeneric(typeof(EfRepository<>)).As(typeof(IRepository<>)).InstancePerLifetimeScope();

            //Services
            //builder.RegisterType<EncryptionService>().As<IEncryptionService>().InstancePerLifetimeScope(); //InstancePerApiRequest
            builder.RegisterType<UserService>().As<IUserService>().InstancePerLifetimeScope();
            //builder.RegisterType<ADService>().As<IADService>().InstancePerLifetimeScope();
            //builder.RegisterType<WinActiveDirectoryService>().As<IActiveDirectoryService>().InstancePerLifetimeScope();
            builder.RegisterType<EmailService>().As<IEmailService>().InstancePerLifetimeScope();

            builder.RegisterType<MFUserService>().As<IMFUserService>().InstancePerLifetimeScope();
            builder.RegisterType<MFObjectService>().As<IMFObjectService>().InstancePerLifetimeScope();
            builder.RegisterType<MFVaultService>().As<IMFVaultService>().InstancePerLifetimeScope();
            builder.RegisterType<VaultServerService>().As<IVaultServerService>().InstancePerLifetimeScope();
            builder.RegisterType<VaultTemplateService>().As<IVaultTemplateService>().InstancePerLifetimeScope();
            builder.RegisterType<MFilesVaultService>().As<IMFilesVaultService>().InstancePerLifetimeScope();
            builder.RegisterType<UserVaultService>().As<IUserVaultService>().InstancePerLifetimeScope();
            builder.RegisterType<MfUserGroupService>().As<IMfUserGroupService>().InstancePerLifetimeScope();
            builder.RegisterType<VaultAppService>().As<IVaultAppService>().InstancePerLifetimeScope();
            builder.RegisterType<MfProjectService>().As<IMfProjectService>().InstancePerLifetimeScope();
            builder.RegisterType<MfProjectService>().As<IMfProjectService>().InstancePerLifetimeScope();

            builder.RegisterType<MfilesWebService>().As<IMfilesWebService>().InstancePerLifetimeScope();

          //  builder.RegisterType<TasksService>().As<ITasksService>().InstancePerLifetimeScope();
            builder.RegisterType<CloudService>().As<ICloudService>().InstancePerLifetimeScope();
            //builder.RegisterType<UserCloudService>().As<IUserCloudService>().InstancePerLifetimeScope();
            builder.RegisterType<ProjectService>().As<IProjectService>().InstancePerLifetimeScope();
            builder.RegisterType<ProjectMemberService>().As<IProjectMemberService>().InstancePerLifetimeScope();
            builder.RegisterType<SharedFileService>().As<ISharedFileService>().InstancePerLifetimeScope();

            builder.RegisterType<AecUserStore>().As<IUserStore<User, long>>().InstancePerLifetimeScope();
            builder.RegisterType<AecUserManager>().As<UserManager<User, long>>().InstancePerLifetimeScope();
            builder.RegisterType<AecSignInManager>().As<SignInManager<User, long>>().InstancePerLifetimeScope();
            builder.Register<IAuthenticationManager>(c => HttpContext.Current.GetOwinContext().Authentication);

            builder.RegisterType<MFWorkHourService>().As<IMFWorkHourService>().InstancePerLifetimeScope();
        }
    }
}