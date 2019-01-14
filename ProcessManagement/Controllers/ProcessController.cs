using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Newtonsoft.Json.Linq;
using Microsoft.AspNet.Identity;
using ProcessManagement.Models;
using ProcessManagement.Services;
namespace ProcessManagement.Controllers
{
	public class ProcessController : BaseController
	{
		///=============================================================================================
		PMSEntities db = new PMSEntities();
		GroupService groupService = new GroupService();
		CommonService commonService = new CommonService();
		ParticipateService participateService = new ParticipateService();
		ProcessService processService = new ProcessService();
		///=============================================================================================
	}
}