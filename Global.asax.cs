﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace rqq_management_portal_asp_net
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS",
                Server.MapPath("~/App_Data/rqq-management-project-firebase-adminsdk-fbsvc-a228a51ee0.json"));
        }
    }
}
