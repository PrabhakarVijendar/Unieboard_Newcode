using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using StructureMap;
using UniEBoard.Service.Interfaces.ApplicationService;
using UniEBoard.Service.Models;
using WebMatrix.WebData;

namespace UniEBoard
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            WebApiConfig.Register(GlobalConfiguration.Configuration);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            AuthConfig.RegisterAuth();
            // Initialise Object Mapping Type relationships
            // Db Entities -> Domain Entities
            Repository.Mapping.BootStrapper.Initialize(ObjectFactory.GetInstance<Model.Interfaces.Adapter.IObjectMapperAdapter>());
            // Domain Entities -> ViewModels
            Service.Mapping.BootStrapper.Initialize(ObjectFactory.GetInstance<Model.Interfaces.Adapter.IObjectMapperAdapter>());
        }

        protected void Session_Start()
        {
        }

        protected void Session_End()
        {
            try
            {
                Session.Clear();
                Response.Clear();
                Response.RedirectToRoute("LogOff", new { controller = "Account", action = "LogOff" });
                Response.End();
            }
            catch (Exception ex)
            {
            }
        }
    }
}