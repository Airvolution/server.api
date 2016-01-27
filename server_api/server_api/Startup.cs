using System;
using Microsoft.Owin;
using Owin;
using System.Web;
using System.Web.Http;
using Microsoft.Owin.Security.OAuth;
using server_api.Providers;
using Swashbuckle.Application;

[assembly: OwinStartup(typeof(server_api.App_Start.Startup))]

namespace server_api.App_Start
{
    /// <summary>
    /// *xml comment*
    /// </summary>
    public class Startup
    {
        /// <summary>
        /// *xml comment*
        /// </summary>
        /// <param name="app">*xml comment*</param>
        public void Configuration(IAppBuilder app)
        {

            HttpConfiguration config = new HttpConfiguration();

            WebApiConfig.Register(config);
            ConfigureOAuth(app);
            app.UseCors(Microsoft.Owin.Cors.CorsOptions.AllowAll);

            config.EnableSwagger(c => {
                c.SingleApiVersion("v1", "server_api");
                c.IncludeXmlComments(GetXmlCommentsPath());
                c.RootUrl(req => req.RequestUri.GetLeftPart(UriPartial.Authority) + VirtualPathUtility.ToAbsolute("~/").TrimEnd('/'));
            } ).EnableSwaggerUi();

           //Swashbuckle.Bootstrapper.Init(config);

            app.UseWebApi(config);
            // For more information on how to configure your application, visit http://go.microsoft.com/fwlink/?LinkID=316888
        }

        public void ConfigureOAuth(IAppBuilder app)
        {
            OAuthAuthorizationServerOptions OAuthServerOptions = new OAuthAuthorizationServerOptions()
            {
                AllowInsecureHttp = true,
                TokenEndpointPath = new PathString("/token"),
                AccessTokenExpireTimeSpan = TimeSpan.FromDays(1),
                Provider = new SimpleAuthorizationServerProvider()
            };

            // Token Generation
            app.UseOAuthAuthorizationServer(OAuthServerOptions);
            app.UseOAuthBearerAuthentication(new OAuthBearerAuthenticationOptions());
        }
        /// <summary>
        /// *xml comment*
        /// </summary>
        /// <returns></returns>
        protected static string GetXmlCommentsPath()
        {
            return System.String.Format(@"{0}\bin\server_api.XML", System.AppDomain.CurrentDomain.BaseDirectory);
        }
    }
}
