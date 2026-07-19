<%@ WebHandler Language="C#" Class="CSA.TerminalStateHandler" %>

using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.SessionState;

namespace CSA
{
    /// <summary>
    /// Stores the browser terminal's saved machine state per signed-in user so a
    /// lab session follows the account instead of living only in one browser.
    /// GET  ?key=uID-lab            -> the saved state bytes (204 when none exists)
    /// POST ?key=uID-lab            -> save the request body as the new state
    /// POST ?key=uID-lab&delete=1   -> discard the saved state
    /// The key must belong to the signed-in user ("u{UserID}-..."), so nobody can
    /// read or overwrite someone else's session. States are opaque blobs (the
    /// browser gzips them) kept in ~/App_Data/TerminalStates.
    /// </summary>
    public class TerminalStateHandler : IHttpHandler, IReadOnlySessionState
    {
        private const long MaxStateBytes = 100L * 1024 * 1024;

        public bool IsReusable { get { return true; } }

        public void ProcessRequest(HttpContext ctx)
        {
            ctx.Response.Cache.SetCacheability(HttpCacheability.NoCache);

            object uid = ctx.Session["UserID"];
            if (uid == null) { ctx.Response.StatusCode = 401; return; }

            string key = (ctx.Request.QueryString["key"] ?? "").ToLowerInvariant();
            if (!Regex.IsMatch(key, "^[a-z0-9-]{1,64}$")) { ctx.Response.StatusCode = 400; return; }
            // User IDs are uppercase codes (e.g. USRQXK472) while the key was
            // lowercased above, so the ownership check must ignore case too.
            string owner = ("u" + uid + "-").ToLowerInvariant();
            if (!key.StartsWith(owner)) { ctx.Response.StatusCode = 403; return; }

            string dir = ctx.Server.MapPath("~/App_Data/TerminalStates");
            Directory.CreateDirectory(dir);
            string path = Path.Combine(dir, key + ".state");

            if (ctx.Request.HttpMethod == "GET")
            {
                if (!File.Exists(path)) { ctx.Response.StatusCode = 204; return; }
                ctx.Response.ContentType = "application/octet-stream";
                ctx.Response.AddHeader("X-CSA-Saved-At",
                    new DateTimeOffset(File.GetLastWriteTimeUtc(path)).ToUnixTimeMilliseconds().ToString());
                ctx.Response.TransmitFile(path);
                return;
            }

            if (ctx.Request.HttpMethod == "POST" || ctx.Request.HttpMethod == "DELETE")
            {
                if (ctx.Request.HttpMethod == "DELETE" || ctx.Request.QueryString["delete"] == "1")
                {
                    if (File.Exists(path)) File.Delete(path);
                    ctx.Response.StatusCode = 204;
                    return;
                }

                if (ctx.Request.ContentLength <= 0 || ctx.Request.ContentLength > MaxStateBytes)
                { ctx.Response.StatusCode = 413; return; }

                // Write to a temp file first so a dropped upload can't corrupt the saved state.
                string tmp = path + ".tmp";
                using (var file = File.Create(tmp))
                    ctx.Request.InputStream.CopyTo(file);
                if (File.Exists(path)) File.Delete(path);
                File.Move(tmp, path);
                ctx.Response.StatusCode = 204;
                return;
            }

            ctx.Response.StatusCode = 405;
        }
    }
}
