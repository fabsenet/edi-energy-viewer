using System.Web.Http;
using JetBrains.Annotations;

namespace Fabsenet.EdiEnergy
{
    public static class WebApiConfig
    {
        [UsedImplicitly]
        public static void Register(HttpConfiguration config)
        {
            // Web API configuration and services

            // Web API routes
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );
        }
    }
}
