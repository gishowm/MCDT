using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Threading.Tasks;
using System.Web.SessionState;

public static class Current
{
    public static HttpRequest Request
    {
        get
        {
            return HttpContext.Current.Request;
        }
    }
    public static HttpResponse Response
    {
        get
        {
            return HttpContext.Current.Response;
        }
    }
    public static HttpSessionState Session
    {
        get
        {
            return HttpContext.Current.Session;
        }
    }
    public static HttpServerUtility Server
    {
        get
        {
            return HttpContext.Current.Server;
        }
    }
}
