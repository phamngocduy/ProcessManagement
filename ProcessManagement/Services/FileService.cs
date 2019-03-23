using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using ProcessManagement.Models;
namespace ProcessManagement.Services
{
    public class FileService
    {
        ///=============================================================================================
        public readonly PMSEntities db = new PMSEntities();
        CommonService commonService = new CommonService();
        ///=============================================================================================
        ///
        public void saveFile(HttpPostedFileBase file, string savePath)
        {
            if (file != null)
            {
                if (file.ContentLength > 0)
                {
                    string AppPath = AppDomain.CurrentDomain.BaseDirectory;
                    string filePath = AppPath + "App_Data\\Files\\" + savePath;

                    string _FileName = Path.GetFileName(file.FileName);
                    string _path = Path.Combine(filePath, _FileName);
                    file.SaveAs(_path);


                    FileManager f = new FileManager();
                    f.Id = commonService.getRandomString(30);
                    f.Name = _FileName;
                    f.Path = string.Format("{0}/{1}", savePath, _FileName);
                    f.Type = Path.GetExtension(_path);
                    f.Create_At = DateTime.Now;
                    f.Update_At = DateTime.Now;
                    db.FileManagers.Add(f);
                    db.SaveChanges();
                }
            }
        }
        public List<string> getAllFileNameFromFolder(string path)
        {
            List<string> f = new List<string>();
            DirectoryInfo d = new DirectoryInfo(path);//Assuming Test is your Folder
            if (d.Exists)
            {
                FileInfo[] Files = d.GetFiles(); //Getting Text files
                
                foreach (FileInfo file in Files)
                {
                    f.Add(file.Name);
                }
            }
            return f;
            
        }
        public FileManager findFile(string id)
        {
            FileManager file = db.FileManagers.Find(id);
            return file;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="id">Group Id</param>
        //public void CreateDirectory(int id)
        //{
        //    string AppPath = AppDomain.CurrentDomain.BaseDirectory;
        //    string filePath = AppPath + "App_Data\\Files\\group";
        //    string folderName = string.Format("{0}",id);
        //    filePath += String.Format("\\{0}", folderName);
        //    string stepPath = filePath + String.Format("\\process");
        //    string introPath = filePath + String.Format("\\intro");
        //    DirectoryInfo stepDirectory = Directory.CreateDirectory(stepPath);
        //    DirectoryInfo introDirectory = Directory.CreateDirectory(introPath);


        //}
        public void CreateDirectory(string stringPath)
        {
            string AppPath = AppDomain.CurrentDomain.BaseDirectory;
            string filePath = AppPath + "App_Data\\Files\\" + stringPath;
            DirectoryInfo introDirectory = Directory.CreateDirectory(filePath);
        }

    }
}