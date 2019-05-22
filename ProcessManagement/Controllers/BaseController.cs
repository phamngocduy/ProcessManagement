using ProcessManagement.Helpers;
using System;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using ProcessManagement.Models;
namespace ProcessManagement.Controllers
{
    public enum UserRole
    {
        Owner,
        Admin,
        Manager

    }
    public enum FlashType
    {
        info,
        success,
        error,
        warning
    }
    public enum FlashPosition
    {
        BottomRight,
        BottomLeft,
        TopLeft,
        TopRight,
        TopCenter,
        BottomCenter
    }
    public enum Direction
    {
        Group,
        Process,
        Step,
        Task,
        ProcessRun,
        StepRun,
        TaskRun,
        TaskFormRun,
        Export
    }
    public class BaseController : Controller
    {
        private static string _cookieLangName = "LangForProcessManagementSystem";

        //protected override void OnActionExecuting(ActionExecutingContext filterContext)
        //{
        //    string cultureOnCookie = GetCultureOnCookie(filterContext.HttpContext.Request);
        //    string cultureOnURL = filterContext.RouteData.Values.ContainsKey("lang")
        //    ? filterContext.RouteData.Values["lang"].ToString()
        //    : GlobalHelper.DefaultCulture;
        //    string culture = (cultureOnCookie == string.Empty)
        //        ? (filterContext.RouteData.Values["lang"].ToString())
        //        : cultureOnCookie;

        //    if (cultureOnURL != culture)
        //    {
        //        filterContext.HttpContext.Response.RedirectToRoute("LocalizedDefault",
        //        new
        //        {
        //            lang = culture,
        //            controller = filterContext.RouteData.Values["controller"],
        //            action = filterContext.RouteData.Values["action"]
        //        });
        //        return;
        //    }

        //    SetCurrentCultureOnThread(culture);
        //    base.OnActionExecuting(filterContext);
        //}
        
        

        private static void SetCurrentCultureOnThread(string lang)
        {
            if (string.IsNullOrEmpty(lang))
                lang = GlobalHelper.DefaultCulture;
            System.Globalization.CultureInfo cultureInfo = new System.Globalization.CultureInfo(lang);
            System.Threading.Thread.CurrentThread.CurrentUICulture = cultureInfo;
            System.Threading.Thread.CurrentThread.CurrentCulture = cultureInfo;
        }

        public static String GetCultureOnCookie(HttpRequestBase request)
        {
            HttpCookie cookie = request.Cookies[_cookieLangName];
            string culture = string.Empty;
            if (cookie != null)
            {
                culture = cookie.Value;
            }
            return culture;
        }
       
        public void SetFlash(FlashType flashType, string flashMessage, FlashPosition flashPosition = FlashPosition.BottomLeft)
        {
            TempData["FlashMessage.Type"] = flashType;
            TempData["FlashMessage.Text"] = flashMessage;
            TempData["FlashMessage.Position"] = flashPosition;
        }
        public class ServerSideException : Exception
        {
            public ServerSideException(string messase): base(messase)
            {

            }
        }
    }
}