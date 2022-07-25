using Owin;
using System.Text;
using System.Web.Http;

namespace Correlation
{
    public class Startup
    {
        private const string START_DATE = "startdate";
        private const string END_DATE = "enddate";

        public void Configuration(IAppBuilder app)
        {
            // Turn on Access-Control-Allow-Origin: * to API response
            app.UseCors(Microsoft.Owin.Cors.CorsOptions.AllowAll);

            // Configure Web API for Self-Host
            HttpConfiguration config = new HttpConfiguration();
            // Add text/html to supported media types so that the html page will look cleaner
            config.Formatters.JsonFormatter.SupportedMediaTypes.Add(new System.Net.Http.Headers.MediaTypeHeaderValue("text/html"));
            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            app.UseWebApi(config);

            // Allow user to put in date range on http://localhost:9000/
            // Submitting a date range will display http://localhost:9000/api/values with the results
            app.Run(context =>
            {
                StringBuilder sb = new StringBuilder();

                // HTML to get user input for start date and end date 
                // After user submits start date and end date, we post the values to the REST API
                sb.Append(@"<html><body>");
                sb.Append(@"<form action=""/api/values"" method=""post""");
                sb.Append(@"<label for=\""startdate\"">Please Enter Start Date (YYYY-MM-DD):</label><br>");
                sb.Append(@"<input type=""text"" id =""startdate"" name=""startdate"" value=""""><br>");
                sb.Append(@"<br>");
                sb.Append(@"<label for=\""enddate\"">Please Enter End Date (YYYY-MM-DD):</label><br>");
                sb.Append(@"<input type=""text"" id =""enddate"" name=""enddate"" value=""""><br>");
                sb.Append(@"<br>");
                sb.Append(@"<input type=""submit"" value=""Submit"">");
                sb.Append(@"</form>");
                sb.Append(@"</html></body>");

                return context.Response.WriteAsync(sb.ToString());
            
            });
        }
    }
}
