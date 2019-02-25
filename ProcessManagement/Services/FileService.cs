using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace ProcessManagement.Services
{
    public class FileService
    {

        public void saveFile(HttpPostedFileBase file, string savePath)
        {
            if (file != null)
            {
                if (file.ContentLength > 0)
                {
                    string _FileName = Path.GetFileName(file.FileName);
                    string _path = Path.Combine(savePath, _FileName);
                    file.SaveAs(_path);
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="id">Group Id</param>
        public void CreateDirectory(int id)
        {
            string AppPath = AppDomain.CurrentDomain.BaseDirectory;
            string filePath = AppPath + "App_Data\\Files\\group";
            string folderName = string.Format("{0}",id);
            filePath += String.Format("\\{0}", folderName);
            string stepPath = filePath + String.Format("\\process");
            string introPath = filePath + String.Format("\\intro");
            DirectoryInfo stepDirectory = Directory.CreateDirectory(stepPath);
            DirectoryInfo introDirectory = Directory.CreateDirectory(introPath);


        }
    }
}