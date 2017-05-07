﻿using System.Web.Http;

namespace CloneDeploy_App
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Web API configuration and services
            config.Formatters.XmlFormatter.SupportedMediaTypes.Clear();
            // Web API routes
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute("DefaultApi", "{controller}/{action}/{id}", new {id = RouteParameter.Optional}
                );
        }
    }
}