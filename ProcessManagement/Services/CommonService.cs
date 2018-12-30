using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using ProcessManagement.Models;
namespace ProcessManagement.Services
{
    public class CommonService
    {

        /// <summary>
        /// Save hình vào thư mục muốn save
        /// </summary>
        /// <param name="group">Model Group</param>
        /// <param name="ImageGr">Data hình muốn save</param>
        /// <param name="savePath">Đường dẫn tới thư mục muốn save</param>
        /// <return>Return tên của hình đã save</return>
        public void saveAvatarGroup(Group group, HttpPostedFileBase ImageGr, string savePath)
        {
            string avatar = "";
            //set avatar
            if (ImageGr != null)
            {
               
                if (ImageGr.ContentLength > 0)
                {
                    var filename = Path.GetFileName(ImageGr.FileName);
                    var path = Path.Combine(savePath, filename);
                    ImageGr.SaveAs(path);
                    avatar = filename;
                }
            }
            else
            {
                avatar =  group.Avatar != null ? group.Avatar : null;
            }
            group.Avatar = avatar;
            if (group.AvatarDefault == null)
            {
                //set avatar default 
                string charName = group.Name.Substring(0,1).ToUpper();
                string background = getRandomColor();
                var DefaultBackground = new JavaScriptSerializer().Serialize(new { name = charName, background = background });
                group.AvatarDefault = DefaultBackground;
            }
        }
        public string getRandomColor()
        {
            Random rnd = new Random();
            int red = rnd.Next(0, 255);
            int blue = rnd.Next(0, 255);
            int green = rnd.Next(0, 255);
            string color = String.Format("rgb({0},{1},{2})",red,green,blue);
            return color;
        }
    }
}