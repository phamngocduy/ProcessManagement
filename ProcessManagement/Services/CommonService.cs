using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
namespace ProcessManagement.Services
{
    public class CommonService
    {
        
        /// <summary>
        /// Save hình vào thư mục muốn save
        /// </summary>
        /// <param name="ImageGr">Data hình muốn save</param>
        /// <param name="savePath">Đường dẫn tới thư mục muốn save</param>
        /// <return>Return tên của hình đã save</return>
        public string saveAvatarGroup(HttpPostedFileBase ImageGr,string savePath)
        {
            if (ImageGr != null)
            {
                string avatar = "";
                if (ImageGr.ContentLength > 0)
                {
                    var filename = Path.GetFileName(ImageGr.FileName);
                    var path = Path.Combine(savePath, filename);
                    ImageGr.SaveAs(path);
                    avatar = filename;
                }
                return avatar;
            }
            else
            {
                return "default.jpg";
            }
        }
    }
}