using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using ProcessManagement.Controllers;
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
        public FileManager saveFile(int idGroup, HttpPostedFileBase file, string savePath, FileDirection Derection)
        {
            FileManager f = new FileManager();
            if (file != null)
            {
                if (file.ContentLength > 0)
                {
                    string AppPath = AppDomain.CurrentDomain.BaseDirectory;
                    string filePath = AppPath + savePath;

                    string fileName = Path.GetFileName(file.FileName);
                    string path = Path.Combine(filePath, fileName);
                    string extension = Path.GetExtension(path);
                    if (File.Exists(path))
                    {
                        string fileNameWithOutExtension = Path.GetFileNameWithoutExtension(file.FileName);
                        DirectoryInfo d = new DirectoryInfo(filePath);
                        FileInfo[] files = d.GetFiles(string.Format("*{0}", extension));
                        int count = 0;
                        foreach (FileInfo fs in files)
                        {
                            
                            if (fs.Name.Replace(extension,"").StartsWith(fileNameWithOutExtension))
                                count++;
                        }
                        fileName = string.Format("{0} ({1}){2}", fileNameWithOutExtension, count, extension);
                        path = Path.Combine(filePath, fileName);
                    }
                    file.SaveAs(path);


                    f.Id = commonService.getRandomString(50);
                    f.IdGroup = idGroup;
                    f.Name = fileName;
                    f.Path = string.Format("{0}/{1}", savePath, fileName);
                    f.Type = extension;
                    f.Direction = Derection.ToString();
                    f.Create_At = DateTime.Now;
                    f.Update_At = DateTime.Now;
                    db.FileManagers.Add(f);
                    db.SaveChanges();
                }
            }
            return f;

        }
        //public List<string> getAllFileNameFromFolder(string path)
        //{
        //    List<string> f = new List<string>();
        //    DirectoryInfo d = new DirectoryInfo(path);//Assuming Test is your Folder
        //    if (d.Exists)
        //    {
        //        FileInfo[] Files = d.GetFiles(); //Getting Text files

        //        foreach (FileInfo file in Files)
        //        {
        //            f.Add(file.Name);
        //        }
        //    }
        //    return f;

        //}
        public List<FileManager> getAllFileNameFromFolder(int idGroup,FileDirection Direction)
        {
            var file = db.FileManagers.Where(x => x.IdGroup == idGroup && x.Direction == Direction.ToString()).ToList();
            return file;

        }
        public FileManager findFile(string id)
        {
            FileManager file = db.FileManagers.Find(id);
            return file;
        }
        public void removeFile(FileManager file)
        {
            string AppPath = AppDomain.CurrentDomain.BaseDirectory;
            string filePath = AppPath + file.Path;
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
            db.FileManagers.Remove(file);
            db.SaveChanges();

        }
        public FileManager changeFileName(FileManager file,string filename)
        {
            string AppPath = AppDomain.CurrentDomain.BaseDirectory;
            string newFilePath = file.Path.Replace(file.Name, filename);
            string filePath = AppPath + file.Path;
            string targetPath = AppPath + newFilePath;
            if (File.Exists(filePath))
            {
                File.Copy(filePath, targetPath, true);
                File.Delete(filePath);
            }
            file.Name = filename;
            file.Path = newFilePath;
            //TODO: Update Type cho file
            file.Update_At = DateTime.Now;
            db.SaveChanges();
            return file;
        }
        public bool checkFileExist(int idGroup,string name, FileDirection direction)
        {
            var file = db.FileManagers.FirstOrDefault(x => x.IdGroup == idGroup && x.Direction == direction.ToString() && x.Name == name);
            return file != null ? true : false;
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
        public void createDirectory(string stringPath)
        {
            string AppPath = AppDomain.CurrentDomain.BaseDirectory;
            string filePath = AppPath + stringPath;
            DirectoryInfo introDirectory = Directory.CreateDirectory(filePath);
        }
        public bool checkFileOverSize(HttpPostedFileBase file)
        {
            ConfigRule fileSizeRule = db.ConfigRules.Find("filesize");
            if (fileSizeRule != null && file != null)
            {
                //get file size
                int fileSize = file.ContentLength;

                //convert to byte
                int value = fileSizeRule.Value;
                string unit = fileSizeRule.Unit.ToLower();
                int pow;
                switch (unit)
                {
                    case "kb":
                        pow = 1;
                        break;
                    case "mb":
                        pow = 2;
                        break;
                    case "gb":
                        pow = 3;
                        break;
                    default:
                        pow = 0;
                        break;
                }
                double toByte = value * Math.Pow(1024, pow);
                return fileSize > toByte ? true : false; 
            }
            return false;
        }
    }
}