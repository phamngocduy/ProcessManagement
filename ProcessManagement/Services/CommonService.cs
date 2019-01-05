using System;
using System.IO;
using System.Text;
using System.Web;
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
            double opacity = rnd.Next(50, 100);
            opacity = opacity / 100;
            string color = String.Format("rgba({0},{1},{2},{3})",red,green,blue,opacity);
            return color;
        }
        /// <summary>
        /// Chuyển chữ có dấu thành chữ không dấu
        /// </summary>
        /// <param name="utf8String">Chứ có dấu</param>
        /// <returns>Return chữ không dấu</returns>
        public string convertToNonUtf8(string utf8String)
        {
            System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex("\\p{IsCombiningDiacriticalMarks}+");
            string temp = utf8String.Normalize(NormalizationForm.FormD);
            return regex.Replace(temp, String.Empty).Replace('\u0111', 'd').Replace('\u0110', 'D');
        }
        public string converToSlug(string s)
        {
            string slug = convertToNonUtf8(s).Trim().ToLower().Replace(" ", "-").Trim();
            return slug;
        }
    }
}