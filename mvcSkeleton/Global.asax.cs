using System;
using System.Data.SqlClient;
using System.Reflection;
using System.Web.Management;
using System.Web.Mvc;
using System.Web.Routing;
using Autofac;
using Autofac.Integration.Mvc;
using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using NHibernate;
using NHibernate.Cfg;
using NHibernate.Tool.hbm2ddl;

namespace mvcSkeleton
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);

            var nhibConfig = BuildNHibernateConfiguration();
            var factory = CreateSessionFactory(nhibConfig);
            Autofacify(nhibConfig, factory);
        }

        private void Autofacify(Configuration nhibConfig, ISessionFactory factory)
        {

            var builder = new ContainerBuilder();
            builder.RegisterControllers(typeof(MvcApplication).Assembly);
            builder.RegisterAssemblyTypes(Assembly.GetExecutingAssembly()).AsImplementedInterfaces();
           // builder.RegisterGeneric(typeof(Repository<>)).As(typeof(IRepository<>));
            builder.RegisterInstance(nhibConfig).As<Configuration>().SingleInstance();
            builder.RegisterInstance(factory).As<ISessionFactory>().SingleInstance();
            builder.Register(x => x.Resolve<ISessionFactory>().OpenSession()).As<ISession>().InstancePerLifetimeScope();
            var container = builder.Build();
            DependencyResolver.SetResolver(new AutofacDependencyResolver(container));
        }

        private Configuration BuildNHibernateConfiguration()
        {
            return Fluently.Configure()
                .Database(MsSqlConfiguration.MsSql2008.ConnectionString(c => c.FromConnectionStringWithKey("connectionString")))
                .Mappings(m => m.FluentMappings.AddFromAssembly(Assembly.GetExecutingAssembly()))
                .ExposeConfiguration(BuildSchema)
                .BuildConfiguration();
        }

        private ISessionFactory CreateSessionFactory(Configuration config)
        {
            try
            {
                return config.BuildSessionFactory();

            }
            catch (SqlException e)
            {
                throw new Exception("Cannot open database.  You may need to create the db before nhibernate can build the schema", e);
            }
        }

        private void BuildSchema(Configuration config)
        {
            new SchemaUpdate(config)
                .Execute(false, true);
        }
    }
}