using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;
using ProcessManagement.Models;
using ProcessManagement.Services;
namespace ProcessManagement.Services
{
    public class UserService
    {
        ///=============================================================================================
        public readonly PMSEntities db = new PMSEntities();
        CommonService commonService = new CommonService();
        ///=============================================================================================

        public string createAvatar(string chara)
        {
            string charName = chara.Substring(0, 1).ToUpper();
            string background = commonService.getRandomColor();
            var DefaultBackground = new JavaScriptSerializer().Serialize(new { name = charName, background = background });
            return DefaultBackground;
        }
        /// <summary>
        /// Lấy User Avatar
        /// </summary>
        /// <param name="id">User id</param>
        /// <returns></returns>
        public string getAvatar(string id)
        {
            AspNetUser user = db.AspNetUsers.Find(id);
            if (user.Avatar != "")
            {
                return user.Avatar;
            }
            return user.AvatarDefault;
        }
    }
}