using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ProcessManagement.Models;
using ProcessManagement.Services;
namespace ProcessManagement.Controllers
{
    public class FileController : Controller
    {
        ///=============================================================================================
        PMSEntities db = new PMSEntities();
        FileService fileService = new FileService();
        ///=============================================================================================
        // GET: File
        public ViewResult FileManager()
        {
            return View();
        }
        public void DownLoad(string file)
        {
            FileManager f = fileService.findFile(file);
            if (f != null)
            {
                Response.Clear();
                Response.ContentType = "application/octet-stream";
                Response.AddHeader("Content-Disposition", "attachment; filename=" + f.Name);
                Response.WriteFile(Server.MapPath("~/"+ f.Path));
                Response.End();
            }
            else
            {
                Response.Write("This file does not exist.");
                Response.End();
            }
        }
    }
}