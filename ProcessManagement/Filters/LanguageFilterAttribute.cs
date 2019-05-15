using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Threading;
using ProcessManagement.Helpers;
using System.Globalization;

namespace ProcessManagement.Filters
{
    public class LanguageFilterAttribute : ActionFilterAttribute
    {
        private static string _cookieLangName = "LangForProcessManagementSystem";
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            string cultureOnCookie = GetCultureOnCookie(filterContext.HttpContext.Request);
            string cultureOnURL = filterContext.RouteData.Values.ContainsKey("lang")
             ? filterContext.RouteData.Values["lang"].ToString()
             : GlobalHelper.DefaultCulture;
            //string culture = (cultureOnCookie == string.Empty) ? cultureOnURL : cultureOnCookie;
            string culture = cultureOnURL;
            //string currentCulture = CultureInfo.CurrentUICulture.Name;
            string currentCulture = Thread.CurrentThread.CurrentUICulture.Name;
            if (currentCulture != culture)
            {
                SetCurrentCultureOnThread(culture);
                //SetCultureOnCookie(filterContext.HttpContext.Response,culture);
            }
            HttpContext.Current.Session["lang"] = culture;
            //base.OnActionExecuting(filterContext);
        }
        private static void SetCurrentCultureOnThread(string lang)
        {
            if (string.IsNullOrEmpty(lang))
                lang = GlobalHelper.DefaultCulture;
            //var cultureInfo = new System.Globalization.CultureInfo(lang);

            Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture(lang);
            Thread.CurrentThread.CurrentUICulture = new CultureInfo(lang);

        }

        private static String GetCultureOnCookie(HttpRequestBase request)
        {
            HttpCookie cookie = request.Cookies[_cookieLangName];
            string culture = string.Empty;
            if (cookie != null)
            {
                culture = cookie.Value;
            }
            return culture;
        }
        private static void SetCultureOnCookie(HttpResponseBase response, string lang)
        {
            HttpCookie myCookie = new HttpCookie(_cookieLangName,lang);
            myCookie.Expires = DateTime.Now.AddDays(7);
            response.Cookies.Add(myCookie);
        }
    }
}