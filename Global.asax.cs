using System;
using System.Web;
using System.Web.Routing;

namespace CSA
{
    public class Global : HttpApplication
    {
        void Application_Start(object sender, EventArgs e)
        {
            // Code that runs on application startup
            RouteConfig.RegisterRoutes(RouteTable.Routes);
        }

        /// <summary>
        /// Last-resort handler for anything a page did not catch. Without this an
        /// unhandled exception rendered the ASP.NET diagnostic page, which exposed
        /// stack traces, SQL text, and the full .mdf path to whoever triggered it.
        /// Requests are redirected to Error.aspx with a coarse reason instead.
        /// </summary>
        void Application_Error(object sender, EventArgs e)
        {
            HttpException httpError = Server.GetLastError() as HttpException;

            // Request validation rejecting markup is expected input, not a fault:
            // show the "characters not allowed" message rather than a 500.
            string reason = Server.GetLastError() is HttpRequestValidationException
                ? "input"
                : httpError != null && httpError.GetHttpCode() == 404
                    ? "notfound"
                    : "error";

            Server.ClearError();
            Response.Redirect("~/Error.aspx?reason=" + reason, false);
            Context.ApplicationInstance.CompleteRequest();
        }
    }
}
