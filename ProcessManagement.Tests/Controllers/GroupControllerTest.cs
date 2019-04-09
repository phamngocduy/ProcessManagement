using System;
using System.Web.Routing;
using System.Web.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProcessManagement.Controllers;
using ProcessManagement.Models;
using Moq;
using System.Web;
using ProcessManagement.Tests.Support;
using ProcessManagement.Services;
using System.Security.Principal;
using System.IO;
using Microsoft.AspNet.Identity;
using System.Web.ApplicationServices;
using System.Web.SessionState;
using System.Reflection;
using System.Text;
using System.Security.Claims;
using System.Transactions;
using System.Linq;
using MvcContrib.TestHelper;
using Newtonsoft.Json;


namespace ProcessManagement.Tests.Controllers
{
	[TestClass]
	public class UnitTest1
	{
		private HttpContext FakeHttpContext()
		{
			var httpRequest = new HttpRequest("", "http://localhost:8080/", "");
			var stringWriter = new StringWriter();
			var httpResponce = new HttpResponse(stringWriter);
			var httpContext = new HttpContext(httpRequest, httpResponce);

			var sessionContainer =
				new HttpSessionStateContainer("id", new SessionStateItemCollection(),
													new HttpStaticObjectsCollection()
													, 10,
													true,
													HttpCookieMode.AutoDetect,
													SessionStateMode.InProc, false);

			httpContext.Items["AspSession"] =
				typeof(HttpSessionState).GetConstructor(
										BindingFlags.NonPublic | BindingFlags.Instance,
										null, CallingConventions.Standard,
										new[] { typeof(HttpSessionStateContainer) },
										null)
								.Invoke(new object[] { sessionContainer });

			return httpContext;
		}
		/// <summary>
		/// Purpose of TC: 
		/// - Ruturn view Group when login		
		/// </summary>
		[TestMethod]
		public void Login_WithValidModel_ExpectNavigation()
		{
			var url = "https://login.microsoftonline.com/common/oauth2/v2.0/authorize?client_id=ea2300f8-22a5-4f18-86d4-78316fb2c5e9&scope=https%3A%2F%2Fgraph.microsoft.com%2Fuser.read&response_type=code&redirect_uri=https%3A%2F%2Fcntttest.vanlanguni.edu.vn%3A18081%2FCap21T4%2FLoginManagement%2Fsignin-microsoft&state=BvUQx9mARz_k9qob2OUSRMapu1YMf-XoZC6r2inrcj_xjoMx5888f-BSPXC0fcwU4kM2c9tZkmLAvjGPFgJ0bX35tTaPUVO0FiyHfkuHBhetZqMX3E43WVUH4kdqALfWSMBftB0XBJSmIvOa6F7H_m_p-BGIqSNfa4k7EzZld0_2XbzFmh1Cwvd_-VXvbXqsUFjlpF9dMe42M46PvbBwNiGnvkOn68OV5ximyzKmq9oT5TXQqUtbv-qALNkGGPGixwZoz1X2shxhbdW4sR-O22CNKgW6vMclA_Aw29Dt1jnE-geBZkhFOXs0k1PTiA4N4GBxfQ9O4aQaAAPVdArO1XfUEXKjE8Ofx5WNr6hHQa3jef8xjU3wTpus0X4VmYwgoeq9OWTTYaJWNw_Lc8EZJAYc6uB-ueyej5U2ePhXjewubDuQK1sHpEUz3Ak7_d09z1H8HJxmOyUZfxc4jIksxoIMTHc8i64bHYTMPVpKZHNcugsa32DYY483lS_x5GLT";
			var moqUrlHelper = new Mock<UrlHelper>();

			AccountController accountController = new AccountController();
			//Arrange
			LoginViewModel loginViewModel = new LoginViewModel()
			{
				Email = "tovo1@vanlanguni.vn",
				Password = "VLUt151543",
			};

			//action
			var result = accountController.Login(loginViewModel, url);

			//assert
			Assert.IsNotNull(result);
		}
		/// <summary>
		/// Purpose of TC: 
		/// - Ruturn view List of Group when login and available	
		/// </summary>
		[TestMethod]
		public void ReturnViewListOfGroup()
		{
			GroupController controllerUnderTest = new GroupController();
			var userName = "tuanho10@vanlanguni.vn";
			var mockGroup = new Mock<Group>();
			//arrange
			//get user id
			var controllerContext = new Mock<ControllerContext>();
			var principal = new Moq.Mock<IPrincipal>();
			principal.Setup(p => p.IsInRole("owner")).Returns(true);
			principal.SetupGet(x => x.Identity.Name).Returns(userName);
			controllerContext.SetupGet(x => x.HttpContext.User).Returns(principal.Object);
			controllerUnderTest.ControllerContext = controllerContext.Object;

			ViewResult result = controllerUnderTest.Show(4103) as ViewResult;
			//assert
			Assert.IsNotNull(result);
			//Assert.AreEqual("Show", result.ViewName, userName);
		}
		/// <summary>
		/// Purpose of TC: 
		//   Return view Create Group
		/// </summary>
		[TestMethod]
		public void CreateGroup1_WithValidModel_ExpectValidNavigation()
		{
			TestControllerBuilder testControllerBuilder = new TestControllerBuilder();
			var controller = new GroupController();
			testControllerBuilder.InitializeController(controller);

			var results = controller.NewGroup() as ViewResult;
			//assert
			Assert.IsNotNull(results);
		}
		/// <summary>
		/// Purpose of TC: 
		/// - Create Role
		/// </summary>
		/// /// <summary>
		/// Purpose of TC: 
		/// - Validate whether with valid input data, a group record is created and saved into database, 
		///     and then the user is redirected to the Index action
		/// </summary>
		[TestMethod]
		public void CreateGroup2_WithValidModel_ExpectValidNavigation()
		{
			var userName = "tovo1@vanlanguni.vn";
			Mock<HttpRequestBase> moqRequest = new Mock<HttpRequestBase>();
			Mock<HttpFileCollectionBase> moqFiles = new Mock<HttpFileCollectionBase>();
			Mock<HttpPostedFileBase> moqPostedFile = new Mock<HttpPostedFileBase>();
			var controllerContext = new Mock<ControllerContext>();
			var session = new Mock<HttpSessionStateBase>();
			var mockHttpContext = new Mock<HttpContextBase>();

			moqRequest.Setup(r => r.Files.Count).Returns(0);
			mockHttpContext.Setup(x => x.Request).Returns(moqRequest.Object);
			//Setup a fake HttpRequest
			HttpPostedFileBase httpPostedFile = Mock.Of<HttpPostedFileBase>();
			var mock = Mock.Get(httpPostedFile);
			mock.Setup(_ => _.FileName).Returns("fakeFileName.extension");
			var memoryStream = new MemoryStream();
			//...populate fake stream
			//setup mock to return stream
			mock.Setup(_ => _.InputStream).Returns(memoryStream);
			//get session
			mockHttpContext.Setup(ctx => ctx.Session).Returns(session.Object);
			controllerContext.Setup(ctx => ctx.HttpContext).Returns(mockHttpContext.Object);
			controllerContext.Setup(p => p.HttpContext.Session["Group"]).Returns(new Group
			{
				IdOwner = "b0245219 - c69e - 428f - bfc7 - ead7192d5936",
			});
			controllerContext.Setup(p => p.HttpContext.Session["User"]).Returns(new AspNetUser
			{
				Id = "b0245219 - c69e - 428f - bfc7 - ead7192d5936",
			});
			//Arrange
			TestControllerBuilder testControllerBuilder = new TestControllerBuilder();
			var db = new PMSEntities();
			var controller = new GroupController();
			testControllerBuilder.InitializeController(controller);
			var group = new Group();
			group.Id = db.Groups.First().Id;
			group.Name = "Test Group Demo";
			group.Description = "test";
			group.ownerSlug = "Pet";
			group.groupSlug = "Pet_Group";
			controller.ControllerContext = new ControllerContext(mockHttpContext.Object, new RouteData(), controller);
			//get user id
			var principal = new Moq.Mock<IPrincipal>();
			principal.Setup(p => p.IsInRole("owner")).Returns(true);
			principal.SetupGet(x => x.Identity.Name).Returns(userName);
			controllerContext.SetupGet(x => x.HttpContext.User).Returns(principal.Object);
			controller.ControllerContext = controllerContext.Object;

			using (var scope = new TransactionScope())
			{
				var result0 = controller.NewGroup(group, httpPostedFile) as RedirectToRouteResult;
				Assert.IsNotNull(result0);
				Assert.AreEqual("Index", result0.RouteValues["action"]);

				var result1 = controller.NewGroup(group, httpPostedFile) as ViewResult;
				Assert.IsNotNull(result1);
			}

			//Act
			var results = controller.NewGroup(group, httpPostedFile) as ViewResult;

			//Assert
			Assert.IsNotNull(results);
		}
		/// <summary>
		/// Purpose of TC: 
		/// - Return view Create Role
		/// </summary>
		[TestMethod]
		public void CreateRole1_WithValidModel_ExpectValidNavigation()
		{
			TestControllerBuilder testControllerBuilder = new TestControllerBuilder();
			var controller = new ProcessController();
			testControllerBuilder.InitializeController(controller);
			int Idprocess = 135;

			var results = controller.AddRole(Idprocess) as ViewResult;
			//assert
			Assert.IsNotNull(results);
		}
		/// <summary>
		/// Purpose of TC: 
		/// - Create Role
		/// </summary>
		[TestMethod]
		public void CreateRole2_WithValidModel_ExpectValidNavigation()
		{
			TestControllerBuilder testControllerBuilder = new TestControllerBuilder();
			var userName = "tovo1@vanlanguni.vn";
			var processid = 137;
			var identity = new GenericIdentity("tovo1@vanlanguni.vn");
			var moqRequest = new Mock<HttpRequestBase>();
			var moqResponse = new Mock<HttpResponseBase>();
			var moqServer = new Mock<HttpServerUtilityBase>();
			var moqUser = new Mock<IPrincipal>();
			var moqIdentity = new Mock<IIdentity>();
			var moqUrlHelper = new Mock<UrlHelper>();
			var controllerContext = new Mock<ControllerContext>();
			var session = new Mock<HttpSessionStateBase>();
			var mockHttpContext = new Mock<HttpContextBase>();
			//Setup a fake HttpRequest
			Mock<HttpPostedFileBase> moqPostedFile = new Mock<HttpPostedFileBase>();
			var principal = new Mock<IPrincipal>();
			principal.Setup(p => p.IsInRole("owner")).Returns(true);
			principal.SetupGet(x => x.Identity.Name).Returns("tovo1@vanlanguni.vn");
			principal.Setup(x => x.Identity).Returns(identity);
			principal.Setup(x => x.IsInRole(It.IsAny<string>())).Returns(true);

			controllerContext.SetupGet(x => x.HttpContext.User).Returns(principal.Object);
			moqRequest.Setup(r => r.Files.Count).Returns(0);
			mockHttpContext.Setup(x => x.Request).Returns(moqRequest.Object);
			principal.Setup(p => p.IsInRole("Manager")).Returns(true);
			controllerContext.SetupGet(p => p.HttpContext.Request.IsAuthenticated).Returns(true);
			moqIdentity.Setup(id => id.IsAuthenticated).Returns(true);
			//get session
			mockHttpContext.Setup(ctx => ctx.Session).Returns(session.Object);
			controllerContext.Setup(ctx => ctx.HttpContext).Returns(mockHttpContext.Object);
			controllerContext.Setup(p => p.HttpContext.Session["processid"]).Returns(processid);
			//arrange
			HttpContext.Current = FakeHttpContext();
			var controller = new ProcessController();
			controller.ControllerContext = controllerContext.Object;
			testControllerBuilder.InitializeController(controller);
			var db = new PMSEntities();
			var role = new Role();
			role.Id = db.Roles.First().Id;
			role.IdProcess = processid;
			role.Name = "Role Unittest";
			role.Description = "This is a role unit test";
			//get user id
			principal.Setup(p => p.IsInRole("owner")).Returns(true);
			principal.SetupGet(x => x.Identity.Name).Returns(userName);
			controllerContext.SetupGet(x => x.HttpContext.User).Returns(principal.Object);
			controller.ControllerContext = controllerContext.Object;

			//act
			var result = controller.AddRole(role) as RedirectResult;
			//assert
			Assert.IsNotNull(result);
			Assert.IsTrue(role.Name.ToString().Equals("Role Unittest"));
			Assert.IsTrue(role.Description.ToString().Equals("This is a role unit test"));
		}
		/// <summary>
		/// Purpose of TC: 
		/// - Create Process1	
		/// </summary>
		[TestMethod]
		public void CreateProcess1_WithValidModel_ExpectValidNavigation()
		{
			TestControllerBuilder testControllerBuilder = new TestControllerBuilder();
			var controller = new ProcessController();
			testControllerBuilder.InitializeController(controller);
			int Idprocess = 135;

			var results = controller.NewProcess(Idprocess) as ViewResult;
			//assert
			Assert.IsNotNull(results);

		}
		/// <summary>
		/// Purpose of TC: 
		/// - Create Process	
		/// </summary>
		[TestMethod]
		public void CreateProcess2_WithValidModel_ExpectValidNavigation()
		{	
			var groupId = 4104;
			var idOwner = "64e10037-6c10-4544-a853-a2952330bf8e";
			var userName = "tovo1@vanlanguni.vn";
			Mock<HttpRequestBase> moqRequest = new Mock<HttpRequestBase>();
			Mock<HttpFileCollectionBase> moqFiles = new Mock<HttpFileCollectionBase>();
			Mock<HttpPostedFileBase> moqPostedFile = new Mock<HttpPostedFileBase>();
			var controllerContext = new Mock<ControllerContext>();
			var session = new Mock<HttpSessionStateBase>();
			var mockHttpContext = new Mock<HttpContextBase>();

			moqRequest.Setup(r => r.Files.Count).Returns(0);
			mockHttpContext.Setup(x => x.Request).Returns(moqRequest.Object);
			//Setup a fake HttpRequest
			HttpPostedFileBase httpPostedFile = Mock.Of<HttpPostedFileBase>();
			var mock = Mock.Get(httpPostedFile);
			mock.Setup(_ => _.FileName).Returns("fakeFileName.extension");
			var memoryStream = new MemoryStream();
			//...populate fake stream
			//setup mock to return stream
			mock.Setup(_ => _.InputStream).Returns(memoryStream);
			//get session
			mockHttpContext.Setup(ctx => ctx.Session).Returns(session.Object);
			controllerContext.Setup(ctx => ctx.HttpContext).Returns(mockHttpContext.Object);
			controllerContext.Setup(p => p.HttpContext.Session["IdOwner"]).Returns(groupId);
			controllerContext.Setup(p => p.HttpContext.Session["idUser"]).Returns("64e10037-6c10-4544-a853-a2952330bf8e");
			//Arrange
			TestControllerBuilder testControllerBuilder = new TestControllerBuilder();
			var db = new PMSEntities();
			var controller = new ProcessController();
			testControllerBuilder.InitializeController(controller);
			var process = new Process();
			process.Id = db.Groups.First().Id;
			process.IdOwner = idOwner;
			process.IdGroup = 4104;
			process.Name = "Process Test";
			process.Description = "Demo test create process is correct";
			process.Avatar = "image.png";
			controller.ControllerContext = new ControllerContext(mockHttpContext.Object, new RouteData(), controller);
			//get user id
			var principal = new Moq.Mock<IPrincipal>();
			principal.Setup(p => p.IsInRole("owner")).Returns(true);
			principal.SetupGet(x => x.Identity.Name).Returns(userName);
			controllerContext.SetupGet(x => x.HttpContext.User).Returns(principal.Object);
			controller.ControllerContext = controllerContext.Object;
			ProcessService processService = new ProcessService();
			processService.createProcess(4104,idOwner,process);

			using (var scope = new TransactionScope())
			{
				var result0 = controller.NewProcess(groupId, process, httpPostedFile) as RedirectToRouteResult;
				Assert.IsNotNull(result0);
				Assert.AreEqual("Index", result0.RouteValues["action"]);
				Assert.AreEqual("",process.IdOwner);

				var result1 = controller.NewProcess(groupId, process, httpPostedFile) as ViewResult;
				Assert.IsNotNull(result1);
			}
		}
		/// <summary>
		/// Purpose of TC: 
		/// - Create Task	
		/// </summary>
		[TestMethod]
		public void CreateTask_WithValidModel_ExpectValidNavigation()
		{
			var userName = "tovo1@vanlanguni.vn";
			var moqContext = new Mock<HttpContextBase>();
			var moqRequest = new Mock<HttpRequestBase>();
			var moqResponse = new Mock<HttpResponseBase>();
			var moqSession = new Mock<HttpSessionStateBase>();
			var moqServer = new Mock<HttpServerUtilityBase>();
			var moqUser = new Mock<IPrincipal>();
			var moqIdentity = new Mock<IIdentity>();
			var moqUrlHelper = new Mock<UrlHelper>();
			//Setup a fake HttpRequest
			Mock<HttpPostedFileBase> moqPostedFile = new Mock<HttpPostedFileBase>();

			moqRequest.Setup(r => r.Files.Count).Returns(0);
			moqContext.Setup(x => x.Request).Returns(moqRequest.Object);

			moqIdentity.Setup(id => id.IsAuthenticated).Returns(true);
			//arrange 
			var controller = new ProcessController();
			var db = new PMSEntities();
			var taskProcess = new TaskProcess();
			taskProcess.Id = 1220;
			taskProcess.IdStep = 273;
			taskProcess.Name = "Task Test";
			taskProcess.Description = "Demo test create task is correct";

			controller.ControllerContext = new ControllerContext(moqContext.Object, new RouteData(), controller);
			var validationResults = TestModelHelper.ValidateModel(controller, taskProcess);

			//get user id
			var controllerContext = new Mock<ControllerContext>();
			var principal = new Moq.Mock<IPrincipal>();
			principal.Setup(p => p.IsInRole("owner")).Returns(true);
			principal.SetupGet(x => x.Identity.Name).Returns(userName);
			controllerContext.SetupGet(x => x.HttpContext.User).Returns(principal.Object);
			controller.ControllerContext = controllerContext.Object;
			//act
			var results = controller.AddTask(1220) as ViewResult;

			//assert
			Assert.IsNotNull(results);
		}
		/// <summary>
		/// Purpose of TC: 
		/// - Create Form Task	
		/// </summary>
		[TestMethod]
		public void CreateFormTask_WithValidModel_ExpectValidNavigation()
		{
			TestControllerBuilder testControllerBuilder = new TestControllerBuilder();
			var userName = "tovo1@vanlanguni.vn";
			var moqContext = new Mock<HttpContextBase>();
			var moqRequest = new Mock<HttpRequestBase>();
			var moqResponse = new Mock<HttpResponseBase>();
			var moqSession = new Mock<HttpSessionStateBase>();
			var moqServer = new Mock<HttpServerUtilityBase>();
			var moqUser = new Mock<IPrincipal>();
			var moqIdentity = new Mock<IIdentity>();
			var moqUrlHelper = new Mock<UrlHelper>();
			//Setup a fake HttpRequest
			Mock<HttpPostedFileBase> moqPostedFile = new Mock<HttpPostedFileBase>();

			moqRequest.Setup(r => r.Files.Count).Returns(0);
			moqContext.Setup(x => x.Request).Returns(moqRequest.Object);

			moqIdentity.Setup(id => id.IsAuthenticated).Returns(true);
			//arrange 
			var controller = new ProcessController();
			var taskProcess = new TaskProcess()
			{
				Id = 1220,
				IdStep = 273,
				Name = "Task Test",
				Description = "Demo test create task is correct"
			};

			controller.ControllerContext = new ControllerContext(moqContext.Object, new RouteData(), controller);
			var validationResults = TestModelHelper.ValidateModel(controller, taskProcess);

			//get user id
			var controllerContext = new Mock<ControllerContext>();
			var principal = new Moq.Mock<IPrincipal>();
			principal.Setup(p => p.IsInRole("owner")).Returns(true);
			principal.SetupGet(x => x.Identity.Name).Returns(userName);
			controllerContext.SetupGet(x => x.HttpContext.User).Returns(principal.Object);
			controller.ControllerContext = controllerContext.Object;
			//act
			var results = controller.AddFormTask(1220) as RedirectResult;
			testControllerBuilder.InitializeController(controller);
			//assert
			Assert.IsNull(results);
		}
		/// <summary>
		/// Purpose of TC: 
		/// - Return Task View Available
		/// </summary>
		[TestMethod]
		public void ReturnTaskView_Available()
		{	
			var controller = new ProcessController();
			//user need get
			var userName = "tovo1@vanlanguni.vn";
			var controllerContext = new Mock<ControllerContext>();
			var session = new Mock<HttpSessionStateBase>();
			var mockHttpContext = new Mock<HttpContextBase>();

			var principal = new Mock<IPrincipal>();
			//get user id
			principal.Setup(p => p.IsInRole("owner")).Returns(true);
			principal.SetupGet(x => x.Identity.Name).Returns(userName);
			controllerContext.SetupGet(x => x.HttpContext.User).Returns(principal.Object);
			controller.ControllerContext = controllerContext.Object;

			principal.Setup(x => x.IsInRole(It.IsAny<string>())).Returns(true);
			//get session
			mockHttpContext.Setup(ctx => ctx.Session).Returns(session.Object);
			controllerContext.Setup(ctx => ctx.HttpContext).Returns(mockHttpContext.Object);
			controllerContext.Setup(p => p.HttpContext.Session["idTask"]).Returns(new TaskProcess
			{
				Id = 1212,
				IdRole = 72,
			});
			controllerContext.SetupGet(x => x.HttpContext.User).Returns(principal.Object);
			controller.ControllerContext = controllerContext.Object;
			// action
			ViewResult result = controller.ShowTask(1212) as ViewResult;

			// assert
			Assert.IsNotNull(result);
		}
		/// <summary>
		/// Purpose of TC: 
		/// - Return Task View UnAvailable
		/// </summary>
		[TestMethod]
		public void ReturnTaskView_UnAvailable()
		{
			var controller = new ProcessController();
			//user need get
			var userName = "tovo1@vanlanguni.vn";
			var controllerContext = new Mock<ControllerContext>();
			var session = new Mock<HttpSessionStateBase>();
			var mockHttpContext = new Mock<HttpContextBase>();

			var principal = new Mock<IPrincipal>();
			//get user id
			principal.Setup(p => p.IsInRole("owner")).Returns(true);
			principal.SetupGet(x => x.Identity.Name).Returns(userName);
			controllerContext.SetupGet(x => x.HttpContext.User).Returns(principal.Object);
			controller.ControllerContext = controllerContext.Object;

			principal.Setup(x => x.IsInRole(It.IsAny<string>())).Returns(true);
			//get session
			mockHttpContext.Setup(ctx => ctx.Session).Returns(session.Object);
			controllerContext.Setup(ctx => ctx.HttpContext).Returns(mockHttpContext.Object);
			controllerContext.Setup(p => p.HttpContext.Session["idTask"]).Returns(new TaskProcess
			{
				Id = 69,
				IdRole = 72,
			});
			controllerContext.SetupGet(x => x.HttpContext.User).Returns(principal.Object);
			controller.ControllerContext = controllerContext.Object;
			// action
			ViewResult result = controller.ShowTask(69) as ViewResult;

			// assert
			Assert.IsNull(result);
			System.Diagnostics.Trace.WriteLine("HTTP not found");
		}
		/// <summary>
		/// Purpose of TC: 
		/// - Return Step View
		/// </summary>
		[TestMethod]
		public void SelectProcess_ReturnStep()
		{
			var userName = "tovo1@vanlanguni.vn";
			var IdProcess = 99;
			var session = new Mock<HttpSessionStateBase>();
			var mockHttpContext = new Mock<HttpContextBase>();
			var mockControllerContext = new Mock<ControllerContext>();
			var groupService = new GroupService();
			TestControllerBuilder testControllerBuilder = new TestControllerBuilder();
			//get session
			//mockHttpContext.Setup(ctx => ctx.Session).Returns(session.Object);
			//mockControllerContext.Setup(ctx => ctx.HttpContext).Returns(mockHttpContext.Object);
			//mockControllerContext.Setup(p => p.HttpContext.Session["Process"]).Returns(new Process
			//{
			//	Id = 99,
			//	IdGroup = 4104
			//});
			//mockControllerContext.Setup(p => p.HttpContext.Session["IdGroup"]).Returns(new Group
			//{
			//	Id = 4104
			//});
			//mockControllerContext.Setup(p => p.HttpContext.Session["IdGroup"]).Returns(new GroupService
			//{

			//});
			var controller = new ProcessController();
			testControllerBuilder.InitializeController(controller);
			controller.ControllerContext = mockControllerContext.Object;
			//get user id
			var controllerContext = new Mock<ControllerContext>();
			var principal = new Moq.Mock<IPrincipal>();
			principal.Setup(p => p.IsInRole("owner")).Returns(true);
			principal.SetupGet(x => x.Identity.Name).Returns(userName);
			controllerContext.SetupGet(x => x.HttpContext.User).Returns(principal.Object);
			controller.ControllerContext = controllerContext.Object;
			//Act         
			var result = controller.ShowStep(IdProcess) as ViewResult;

			//Assert
			Assert.IsNotNull(result);
		}
		/// <summary>
		/// Purpose of TC: 
		/// - Ruturn view Edit Role Available		
		/// </summary>
		[TestMethod]
		public void ReturnViewEditRole_Available()
		{
			ProcessController controllerUnderTest = new ProcessController();
			//user need get
			var userName = "tovo1@vanlanguni.vn";
			var mockGroup = new Mock<Group>();
			var controllerContext = new Mock<ControllerContext>();
			var session = new Mock<HttpSessionStateBase>();
			var mockHttpContext = new Mock<HttpContextBase>();

			//get session
			mockHttpContext.Setup(ctx => ctx.Session).Returns(session.Object);
			controllerContext.Setup(ctx => ctx.HttpContext).Returns(mockHttpContext.Object);
			controllerContext.Setup(p => p.HttpContext.Session["Role"]).Returns(new Role
			{
				Id = 70,
				IdProcess = 137
			});
			//arrange
			//get user id
			var principal = new Moq.Mock<IPrincipal>();
			principal.Setup(p => p.IsInRole("owner")).Returns(true);
			principal.SetupGet(x => x.Identity.Name).Returns(userName);
			controllerContext.SetupGet(x => x.HttpContext.User).Returns(principal.Object);
			controllerUnderTest.ControllerContext = controllerContext.Object;

			ViewResult result = controllerUnderTest.EditRole(70) as ViewResult;
			//assert
			Assert.IsNotNull(result);
		}
		/// <summary>
		/// Purpose of TC: 
		/// - Ruturn view Edit Role UnAvailable
		/// </summary>
		[TestMethod]
		public void ReturnViewEditRole_UnAvailable()
		{
			ProcessController controllerUnderTest = new ProcessController();
			//user need get
			var userName = "tovo1@vanlanguni.vn";
			var mockGroup = new Mock<Group>();
			var controllerContext = new Mock<ControllerContext>();
			var session = new Mock<HttpSessionStateBase>();
			var mockHttpContext = new Mock<HttpContextBase>();

			//get session
			mockHttpContext.Setup(ctx => ctx.Session).Returns(session.Object);
			controllerContext.Setup(ctx => ctx.HttpContext).Returns(mockHttpContext.Object);
			controllerContext.Setup(p => p.HttpContext.Session["Role"]).Returns(new Role
			{
				Id = 69,
				IdProcess = 69
			});
			//arrange
			//get user id
			var principal = new Moq.Mock<IPrincipal>();
			principal.Setup(p => p.IsInRole("owner")).Returns(true);
			principal.SetupGet(x => x.Identity.Name).Returns(userName);
			controllerContext.SetupGet(x => x.HttpContext.User).Returns(principal.Object);
			controllerUnderTest.ControllerContext = controllerContext.Object;

			ViewResult result = controllerUnderTest.EditRole(999) as ViewResult;
			//assert
			Assert.IsNull(result);
			System.Diagnostics.Trace.WriteLine("HTTP not found");
		}
		/// <summary>
		/// Purpose of TC: 
		/// - Hanle Edit Role	
		/// </summary>
		[TestMethod]
		public void EditRole_WithValidModel_ExpectValidNavigation()
		{
			TestControllerBuilder testControllerBuilder = new TestControllerBuilder();
			var controller = new ProcessController();
			testControllerBuilder.InitializeController(controller);
			var role = new Role();
			var db = new PMSEntities();
			//arrange
			Role editrole = db.Roles.First();
			editrole.Name = "Role UnitTest";
			editrole.Description = "This is role unittest";

			using (var scope = new TransactionScope()) {
				var results = controller.EditRole(69) as ViewResult;
				//assert
				Assert.IsNotNull(results);
				System.Diagnostics.Trace.WriteLine("Edit Role successfully!");
			}	
		}
		/// <summary>
		/// Purpose of TC: 
		/// - Hanle Edit Role Member in Group	
		/// </summary>
		[TestMethod]
		public void EditRoleMemeber_WithValidModel_ExpectValidNavigation()
		{
			int idGroup = 4102;
			var application = new Mock<HttpApplicationStateBase>();
			var mockGroup = new Mock<Group>();
			var mockParicipate = new Mock<Participate>();
			var controllerContext = new Mock<ControllerContext>();
			var session = new Mock<HttpSessionStateBase>();
			var mockHttpContext = new Mock<HttpContextBase>();

			TestControllerBuilder testControllerBuilder = new TestControllerBuilder();
			var controller = new GroupController();
			testControllerBuilder.InitializeController(controller);
			var participate = new Participate();
			var db = new PMSEntities();

			//get session
			mockHttpContext.Setup(ctx => ctx.Session).Returns(session.Object);
			controllerContext.Setup(ctx => ctx.HttpContext).Returns(mockHttpContext.Object);
			controllerContext.Setup(p => p.HttpContext.Session["groupid"]).Returns(idGroup);

			//arrange
			Group group = db.Groups.First();
			group.Id = 4102;
			Participate editrolemenmber = db.Participates.First();
			editrolemenmber.IsAdmin = true;
			editrolemenmber.IdGroup = 4102;
			//check mock
			controller.ControllerContext = controllerContext.Object;

			using (var scope = new TransactionScope())
			{
				var results = controller.EditRoleMember(4142) as ViewResult;
				//assert
				Assert.IsNotNull(results);
			}
		}
		/// <summary>
		/// Purpose of TC: 
		/// - Hanle Edit Step	
		/// </summary>
		[TestMethod]
		public void EditStep_WithValidModel_ExpectValidNavigation()
		{
			TestControllerBuilder testControllerBuilder = new TestControllerBuilder();
			var controller = new ProcessController();
			testControllerBuilder.InitializeController(controller);
			var step = new Step();
			var db = new PMSEntities();
			//arrange
			Step editstep = db.Steps.First();
			editstep.Id = 399;
			editstep.Description = "Edit Step UnitTest Process";
			int group = 4102;

			using (var scope = new TransactionScope())
			{
				var results = controller.EditStep(group, editstep) as ViewResult;
				//assert
				Assert.IsNull(results);
			}
		}
		/// <summary>
		/// Purpose of TC: 
		/// - Hanle Edit Task	
		/// </summary>
		[TestMethod]
		public void EditTask_WithValidModel_ExpectValidNavigation()
		{
			TestControllerBuilder testControllerBuilder = new TestControllerBuilder();
			var taskService = new TaskService();
			var controller = new ProcessController();
			testControllerBuilder.InitializeController(controller);
			var task = new TaskProcess();
			var db = new PMSEntities();
			//arrange
			TaskProcess edittask = db.TaskProcesses.First();
			edittask.Name = "Task Process UnitTest";
			edittask.IdRole = 67;
			edittask.ValueInputText = "task value input text";
			edittask.ValueInputFile = "task value input file";

			//using (var scope = new TransactionScope())
			//{
			//	ViewResult results = taskService.editTask(edittask) as ViewResult;
			//	//assert
			//	Assert.IsNotNull(results);
			//}
		}
		/// <summary>
		/// Purpose of TC: 
		/// - Hanle Edit Group		
		/// </summary>
		[TestMethod]
		public void EditGroup_WithValidModel_ExpectValidNavigation()
		{
			var idGroup = 4104;
			var controllerContext = new Mock<ControllerContext>();
			Mock<HttpFileCollectionBase> moqFiles = new Mock<HttpFileCollectionBase>();
			Mock<HttpPostedFileBase> moqPostedFile = new Mock<HttpPostedFileBase>();
			//mock session
			//var controllerContext = new Mock<ControllerContext>();
			//var session = new Mock<HttpSessionStateBase>();
			//var mockHttpContext = new Mock<HttpContextBase>();
			//Setup a fake HttpRequest
			HttpPostedFileBase httpPostedFile = Mock.Of<HttpPostedFileBase>();
			var mock = Mock.Get(httpPostedFile);
			mock.Setup(_ => _.FileName).Returns("fakeFileName.extension");
			var memoryStream = new MemoryStream();
			mock.Setup(_ => _.InputStream).Returns(memoryStream);
			//...populate fake stream
			//setup mock to return stream
			//get session
			//mockHttpContext.Setup(ctx => ctx.Session).Returns(session.Object);
			//controllerContext.Setup(ctx => ctx.HttpContext).Returns(mockHttpContext.Object);
			//controllerContext.Setup(p => p.HttpContext.Session["groupid"]).Returns(new Group
			//{
			//	Id = idGroup
			//});


			//TestControllerBuilder testControllerBuilder = new TestControllerBuilder();
			var controller = new GroupController();
			//testControllerBuilder.InitializeController(controller);

			var group = new Group();
			var db = new PMSEntities();
			Group editgroup = db.Groups.First();
			editgroup.Name = "Group UnitTest";
			editgroup.Description = "This is group unittest";

			using (var scope = new TransactionScope())
			{
				var results = controller.Edit(editgroup, httpPostedFile) as ViewResult;
				//assert
				Assert.IsNotNull(results);
			}
		}
		/// <summary>
		/// Purpose of TC: 
		/// - Hanle Edit Information Process about Name, Description		
		/// </summary>
		[TestMethod]
		public void EditProcess1_WithValidModel_ExpectValidNavigation()
		{
			TestControllerBuilder testControllerBuilder = new TestControllerBuilder();
			var controller = new ProcessController();
			testControllerBuilder.InitializeController(controller);
			var process = new Process();
			var db = new PMSEntities();
			Process editprocess = db.Processes.First();
			editprocess.Name = "Process UnitTest";
			editprocess.Description = "This is process unittest";

			using (var scope = new TransactionScope())
			{
				var results = controller.EditProcess(140) as ViewResult;
				//assert
				Assert.IsNotNull(results);
			}
		}
		/// <summary>
		/// Purpose of TC: 
		/// - Hanle Edit Process		
		/// </summary>
		[TestMethod]
		public void EditProcess2_WithValidModel_ExpectValidNavigation()
		{
			TestControllerBuilder testControllerBuilder = new TestControllerBuilder();
			var controller = new ProcessController();
			testControllerBuilder.InitializeController(controller);
			var process = new Process();
			var db = new PMSEntities();
			Process editprocess = db.Processes.First();
			editprocess.DataJson = "";
			editprocess.IsRun = false;

			using (var scope = new TransactionScope())
			{
				var results = controller.EditProcess(140) as ViewResult;
				//assert
				Assert.IsNotNull(results);
			}
		}
		/// <summary>
		/// Purpose of TC: 
		/// - Ruturn view Edit Process Available		
		/// </summary>
		[TestMethod]
		public void ReturnViewEditProcess_Available()
		{
			ProcessController controllerUnderTest = new ProcessController();
			//user need get
			var userName = "tovo1@vanlanguni.vn";
			var mockGroup = new Mock<Group>();
			//arrange
			//get user id
			var controllerContext = new Mock<ControllerContext>();
			var principal = new Moq.Mock<IPrincipal>();
			principal.Setup(p => p.IsInRole("owner")).Returns(true);
			principal.SetupGet(x => x.Identity.Name).Returns(userName);
			controllerContext.SetupGet(x => x.HttpContext.User).Returns(principal.Object);
			controllerUnderTest.ControllerContext = controllerContext.Object;

			ViewResult result = controllerUnderTest.EditProcess(137) as ViewResult;
			//assert
			Assert.IsNotNull(result);
		}
		/// <summary>
		/// Purpose of TC: 
		/// - Ruturn view Edit Process UnAvailable		
		/// </summary>
		[TestMethod]
		public void ReturnViewEditProcess_UnAvailable()
		{
			ProcessController controllerUnderTest = new ProcessController();
			//user need get
			var userName = "tovo1@vanlanguni.vn";
			var mockGroup = new Mock<Group>();
			//arrange
			//get user id
			var controllerContext = new Mock<ControllerContext>();
			var principal = new Moq.Mock<IPrincipal>();
			principal.Setup(p => p.IsInRole("owner")).Returns(true);
			principal.SetupGet(x => x.Identity.Name).Returns(userName);
			controllerContext.SetupGet(x => x.HttpContext.User).Returns(principal.Object);
			controllerUnderTest.ControllerContext = controllerContext.Object;

			ViewResult result = controllerUnderTest.EditProcess(69) as ViewResult;
			//assert
			Assert.IsNull(result);
			System.Diagnostics.Trace.WriteLine("HTTP not found");
		}
		/// <summary>
		/// Purpose of TC: 
		/// - Ruturn view Draw Process Available	
		/// </summary>
		[TestMethod]
		public void ReturnViewDrawProcess_Available()
		{
			ProcessController controllerUnderTest = new ProcessController();
			//user need get
			var userName = "tovo1@vanlanguni.vn";
			var mockGroup = new Mock<Group>();
			//arrange
			//get user id
			var controllerContext = new Mock<ControllerContext>();
			var principal = new Moq.Mock<IPrincipal>();
			principal.Setup(p => p.IsInRole("owner")).Returns(true);
			principal.SetupGet(x => x.Identity.Name).Returns(userName);
			controllerContext.SetupGet(x => x.HttpContext.User).Returns(principal.Object);
			controllerUnderTest.ControllerContext = controllerContext.Object;

			ViewResult result = controllerUnderTest.Draw(152) as ViewResult;
			//assert
			Assert.IsNotNull(result);
		}
		/// <summary>
		/// Purpose of TC: 
		/// - Ruturn view Draw Process UnAvailable	
		/// </summary>
		[TestMethod]
		public void ReturnViewDrawProcess_UnAvailable()
		{
			ProcessController controllerUnderTest = new ProcessController();
			//user need get
			var userName = "tovo1@vanlanguni.vn";
			var mockGroup = new Mock<Group>();
			//arrange
			//get user id
			var controllerContext = new Mock<ControllerContext>();
			var principal = new Moq.Mock<IPrincipal>();
			principal.Setup(p => p.IsInRole("owner")).Returns(true);
			principal.SetupGet(x => x.Identity.Name).Returns(userName);
			controllerContext.SetupGet(x => x.HttpContext.User).Returns(principal.Object);
			controllerUnderTest.ControllerContext = controllerContext.Object;

			ViewResult result = controllerUnderTest.Draw(120) as ViewResult;
			//assert
			Assert.IsNull(result);
			System.Diagnostics.Trace.WriteLine("HTTP not found");
		}
		/// <summary>
		/// Purpose of TC: 
		/// - Hanle Delete Role		
		/// </summary>
		[TestMethod]
		public void DeleteRole_WithValidModel_ExpectValidNavigation()
		{
			TestControllerBuilder testControllerBuilder = new TestControllerBuilder();
			var controller = new ProcessController();
			testControllerBuilder.InitializeController(controller);
			var db = new PMSEntities();

			var result1 = controller.DeleteRole(db.Roles.First().Id) as ViewResult;
			Assert.IsNotNull(result1);

			int id = 71;

			ActionResult actual;
			Role delete = db.Roles.Find(id);
			actual = controller.DeleteRole(id);
			Assert.IsFalse(db.Roles.Contains(delete));
		}
		/// <summary>
		/// Purpose of TC: 
		/// - Hanle Delete Group		
		/// </summary>
		[TestMethod]
		public void DeleteGroup_WithValidModel_ExpectValidNavigation()
		{

		}
		/// <summary>
		/// Purpose of TC: 
		/// - Hanle Delete Process	
		/// </summary>
		[TestMethod]
		public void DeleteProcess_WithValidModel_ExpectValidNavigation()
		{

		}
		/// <summary>
		/// Purpose of TC: 
		/// - Hanle Delete Task	
		/// </summary>
		[TestMethod]
		public void DeleteTask_WithValidModel_ExpectValidNavigation()
		{
			TestControllerBuilder testControllerBuilder = new TestControllerBuilder();
			var controller = new ProcessController();
			testControllerBuilder.InitializeController(controller);
			var db = new PMSEntities();

			var result1 = controller.DeleteRole(db.TaskProcesses.First().Id) as ViewResult;
			Assert.IsNull(result1);

			int id = 71;

			ActionResult actual;
			TaskProcess delete = db.TaskProcesses.Find(id);
			actual = controller.DeleteRole(id);
			Assert.IsFalse(db.TaskProcesses.Contains(delete));
		}
		/// <summary>
		/// Purpose of TC: 
		/// - ShowFormTask	
		/// </summary>
		[TestMethod]
		public void ReturnShowFormTask_WithValidModel_ExpectValidNavigation()
		{
			
		}
		/// <summary>
		/// Purpose of TC: 
		/// - Return view FileManager
		/// </summary>
		[TestMethod]
		public void FileManager1_WithValidModel_ExpectValidNavigation()
		{

		}
		/// <summary>
		/// Purpose of TC: 
		/// - FileManager
		/// </summary>
		[TestMethod]
		public void FileManager2_WithValidModel_ExpectValidNavigation()
		{

		}
		[TestMethod]
		public void TestMultipleLanguage()
		{
			// Arrange
			var coll = new GlobalFilterCollection();
			// Act
			FilterConfig.RegisterGlobalFilters(coll);
			var authorized = coll.Any(x => x.Instance is HandleErrorAttribute);
			//Assert
			Assert.IsTrue(authorized);
		}
	}
}
