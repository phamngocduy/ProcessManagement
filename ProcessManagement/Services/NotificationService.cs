using System;
using System.Linq;
using System.Configuration;
using System.Collections.Generic;
using System.Data.SqlClient;
using ProcessManagement.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.SignalR;
namespace ProcessManagement.Services
{
    public class NotificationService
    {
        ///=============================================================================================
        public readonly PMSEntities db = new PMSEntities();
        readonly string connectString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
        ///=============================================================================================
        public List<Notify> RegisterNotification()
        {
            string UserId = System.Web.HttpContext.Current.User.Identity.GetUserId();
            List<Notify> notifies = new List<Notify>();
            string sqlCommand = @"SELECT TOP 6 [Id], [Content], [viContent], [ActionLink], [isRead], [Created_At] FROM [dbo].[Notify] WHERE [IdUser] = @IdUser ORDER BY [Created_At] DESC";
            using (SqlConnection con = new SqlConnection(connectString))
            {
                con.Open();
                using (SqlCommand cmd = new SqlCommand(sqlCommand, con)) { 
                    cmd.Parameters.AddWithValue("@IdUser", UserId);
                    cmd.Notification = null;
                    SqlDependency sqlDep = new SqlDependency(cmd);
                    sqlDep.OnChange += new OnChangeEventHandler(sqlDep_OnChange);
                    if (con.State == System.Data.ConnectionState.Closed)
                        con.Open();
                   
                    var reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        notifies.Add(item: new Notify { Id = (int)reader["Id"], Content = (string)reader["Content"], viContent = (string)reader["viContent"], ActionLink = (string)reader["ActionLink"], isRead = (bool)reader["isRead"], Created_At = (DateTime)reader["Created_At"] });
                    }
                }
            }
            return notifies;
        }
        private void sqlDep_OnChange(object sender,SqlNotificationEventArgs e)
        {
            //if (e.Type == SqlNotificationType.Change)
            //{
            //    SqlDependency sqlDep = sender as SqlDependency;
            //    sqlDep.OnChange -= sqlDep_OnChange;
            //    IHubContext notificationHub = GlobalHost.ConnectionManager.GetHubContext<NotificationHub>();
            //    notificationHub.Clients.All.updateMessages();
            //    RegisterNotification();
            //}
            if (e.Type == SqlNotificationType.Change)
            {
                NotificationHub.SendNotify();
            }
        }
        public void createNotify(string userId, string content, string viContent, string actionlink)
        {
            Notify notify = new Notify();
            notify.IdUser = userId;
            notify.Content = content;
            notify.viContent = viContent;
            notify.ActionLink = actionlink;
            notify.isRead = false;
            notify.Created_At = DateTime.Now;
            notify.Updated_At = DateTime.Now;
            db.Notifies.Add(notify);
            db.SaveChanges();
            NotificationHub.SendNotify();
        }
        public Notify getNotify(int id)
        {
            Notify notify = db.Notifies.Find(id);
            return notify;
        }
        public List<Notify> getNotifies(string userId)
        {
            List<Notify> notifies = db.Notifies.Where(x => x.IdUser == userId).OrderByDescending(x => x.Created_At).ToList();
            return notifies;
        }
        public List<Notify> getNotifies(string userId, int quality)
        {
            List<Notify> notifies = db.Notifies.Where(x => x.IdUser == userId).Take(quality).OrderByDescending(x => x.Created_At).ToList();
            return notifies;
        }
        public int countNotRead(string userId)
        {
            int count = db.Notifies.Where(x => x.IdUser == userId && x.isRead == false).Count();
            return count;
        }
        public void changeStatus(int id, string userid)
        {
            Notify notify = db.Notifies.Find(id);
            if (notify != null && notify.IdUser == userid)
            {
                notify.isRead = true;
                db.SaveChanges();
            }
        }
    }
}