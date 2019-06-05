using System;
using System.Linq;
using System.Web.Mvc;
using System.Collections.Generic;
using ProcessManagement.Models;
using Microsoft.AspNet.Identity;
using System.Net;
using ProcessManagement.Services;

namespace ProcessManagement.Areas.API.Controllers
{
    public class NotifyController : ProcessManagement.Controllers.BaseController
    {
        NotificationService notificationService = new NotificationService();
        CommonService commonService = new CommonService();
        
        public JsonResult getNotifies()
        {
            HttpStatusCode status = HttpStatusCode.OK;
            object response;

            string idUser = User.Identity.GetUserId();
            List<Notify> notifies = notificationService.RegisterNotification();
            int countNotRead = notificationService.countNotRead(idUser);
            List<object> jNotifies = new List<object>();
            foreach (Notify notify in notifies)
            {
                object n = new
                {
                    id = notify.Id,
                    content = new
                    {
                        en = notify.Content,
                        vi = notify.viContent
                    },
                    actionlink = notify.ActionLink,
                    isread = notify.isRead,
                    created_at = commonService.TimeAgo(notify.Created_At)
                };
                jNotifies.Add(n);
            }
            response = new { data = jNotifies, count = countNotRead, status = status };
            return Json(response, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public void changestatus(int id)
        {
            string idUser = User.Identity.GetUserId();
            notificationService.changeStatus(id:id, userid:idUser);
        }
    }
}