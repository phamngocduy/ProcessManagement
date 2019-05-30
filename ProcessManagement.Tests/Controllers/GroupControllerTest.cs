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
using System.Web.Http;

namespace ProcessManagement.Tests.Controllers
{
	[TestClass]
	public class UnitTest1
	{
		private HttpContext FakeHttpContext()
		{
			HttpRequest httpRequest = new HttpRequest("", "http://localhost:8080/", "");
			StringWriter stringWriter = new StringWriter();
			HttpResponse httpResponce = new HttpResponse(stringWriter);
			HttpContext httpContext = new HttpContext(httpRequest, httpResponce);

			HttpSessionStateContainer sessionContainer =
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
			string url = "https://login.microsoftonline.com/common/oauth2/v2.0/authorize?client_id=ea2300f8-22a5-4f18-86d4-78316fb2c5e9&scope=https%3A%2F%2Fgraph.microsoft.com%2Fuser.read&response_type=code&redirect_uri=https%3A%2F%2Fcntttest.vanlanguni.edu.vn%3A18081%2FCap21T4%2FLoginManagement%2Fsignin-microsoft&state=BvUQx9mARz_k9qob2OUSRMapu1YMf-XoZC6r2inrcj_xjoMx5888f-BSPXC0fcwU4kM2c9tZkmLAvjGPFgJ0bX35tTaPUVO0FiyHfkuHBhetZqMX3E43WVUH4kdqALfWSMBftB0XBJSmIvOa6F7H_m_p-BGIqSNfa4k7EzZld0_2XbzFmh1Cwvd_-VXvbXqsUFjlpF9dMe42M46PvbBwNiGnvkOn68OV5ximyzKmq9oT5TXQqUtbv-qALNkGGPGixwZoz1X2shxhbdW4sR-O22CNKgW6vMclA_Aw29Dt1jnE-geBZkhFOXs0k1PTiA4N4GBxfQ9O4aQaAAPVdArO1XfUEXKjE8Ofx5WNr6hHQa3jef8xjU3wTpus0X4VmYwgoeq9OWTTYaJWNw_Lc8EZJAYc6uB-ueyej5U2ePhXjewubDuQK1sHpEUz3Ak7_d09z1H8HJxmOyUZfxc4jIksxoIMTHc8i64bHYTMPVpKZHNcugsa32DYY483lS_x5GLT";
			Mock<UrlHelper> moqUrlHelper = new Mock<UrlHelper>();

			AccountController accountController = new AccountController();
			//Arrange
			LoginViewModel loginViewModel = new LoginViewModel()
			{
				Email = "tovo1@vanlanguni.vn",
				Password = "VLUt151543",
			};

			//action
			System.Threading.Tasks.Task<ActionResult> result = accountController.Login(loginViewModel, url);

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
			string userName = "tuanho10@vanlanguni.vn";
			Mock<Group> mockGroup = new Mock<Group>();
			//arrange
			//get user id
			Mock<ControllerContext> controllerContext = new Mock<ControllerContext>();
			Mock<IPrincipal> principal = new Moq.Mock<IPrincipal>();
			principal.Setup(p => p.IsInRole("owner")).Returns(true);
			principal.SetupGet(x => x.Identity.Name).Returns(userName);
			controllerContext.SetupGet(x => x.HttpContext.User).Returns(principal.Object);
			controllerUnderTest.ControllerContext = controllerContext.Object;

			ViewResult result = controllerUnderTest.Show(6136) as ViewResult;
			//assert
			Assert.IsNotNull(result);
		}
		/// <summary>
		/// Purpose of TC: 
		//   Return view Create Group
		/// </summary>
		[TestMethod]
		public void CreateGroup1_WithValidModel_ExpectValidNavigation()
		{
			TestControllerBuilder testControllerBuilder = new TestControllerBuilder();
			GroupController controller = new GroupController();
			testControllerBuilder.InitializeController(controller);

			ViewResult results = controller.NewGroup() as ViewResult;
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
			string userName = "tovo1@vanlanguni.vn";
			string IdOwner = "b0245219-c69e-428f-bfc7-ead7192d5936";
			Mock<HttpRequestBase> moqRequest = new Mock<HttpRequestBase>();
			Mock<HttpFileCollectionBase> moqFiles = new Mock<HttpFileCollectionBase>();
			Mock<HttpPostedFileBase> moqPostedFile = new Mock<HttpPostedFileBase>();
			Mock<ControllerContext> controllerContext = new Mock<ControllerContext>();
			Mock<HttpSessionStateBase> session = new Mock<HttpSessionStateBase>();
			Mock<HttpContextBase> mockHttpContext = new Mock<HttpContextBase>();
			//
			GenericIdentity identity = new GenericIdentity("b0245219-c69e-428f-bfc7-ead7192d5936");
			Mock<HttpResponseBase> moqResponse = new Mock<HttpResponseBase>();
			Mock<HttpServerUtilityBase> moqServer = new Mock<HttpServerUtilityBase>();
			Mock<IPrincipal> moqUser = new Mock<IPrincipal>();
			Mock<IIdentity> moqIdentity = new Mock<IIdentity>();
			Mock<UrlHelper> moqUrlHelper = new Mock<UrlHelper>();
			//
			moqRequest.Setup(r => r.Files.Count).Returns(0);
			mockHttpContext.Setup(x => x.Request).Returns(moqRequest.Object);
			//Setup a fake HttpRequest
			HttpPostedFileBase httpPostedFile = Mock.Of<HttpPostedFileBase>();
			Mock<HttpPostedFileBase> mock = Mock.Get(httpPostedFile);
			mock.Setup(_ => _.FileName).Returns("fakeFileName.extension");
			MemoryStream memoryStream = new MemoryStream();
			//...populate fake stream
			//setup mock to return stream
			mock.Setup(_ => _.InputStream).Returns(memoryStream);
			//get session
			mockHttpContext.Setup(ctx => ctx.Session).Returns(session.Object);
			controllerContext.Setup(ctx => ctx.HttpContext).Returns(mockHttpContext.Object);
			controllerContext.Setup(p => p.HttpContext.Session["IdOwner"]).Returns(IdOwner);
			//Arrange
			TestControllerBuilder testControllerBuilder = new TestControllerBuilder();
			PMSEntities db = new PMSEntities();
			AspNetUser user = new AspNetUser();
			user.Id = db.AspNetUsers.First().Id;
			user.UserName = userName;
			GroupController controller = new GroupController();
			testControllerBuilder.InitializeController(controller);
			Group group = new Group();
			group.Id = db.Groups.First().Id;
			group.Name = "UnitTest Group Demo";
			group.IdOwner = IdOwner;
			group.Description = "This is a UnitTest Group";
			group.ownerSlug = "Pet";
			group.groupSlug = "Pet_Group";
			controller.ControllerContext = new ControllerContext(mockHttpContext.Object, new RouteData(), controller);
			//get user id
			Mock<IPrincipal> principal = new Moq.Mock<IPrincipal>();
			principal.Setup(p => p.IsInRole("IdOwner")).Returns(true);
			principal.SetupGet(x => x.Identity.Name).Returns(userName);
			controllerContext.SetupGet(x => x.HttpContext.User).Returns(principal.Object);
			controller.ControllerContext = controllerContext.Object;
			// mock group serivce
			GroupService groupService = new GroupService();
			groupService.createGroup(userName, group);

			//Act
			ViewResult result = controller.NewGroup(group, httpPostedFile) as ViewResult;
			//Assert
			Assert.IsNotNull(result);
			Assert.IsTrue(group.Name.ToString().Equals("UnitTest Group Demo"));
			Assert.IsTrue(group.Description.ToString().Equals("This is a UnitTest Group"));
			Assert.IsTrue(group.ownerSlug.ToString().Equals("Pet"));
			Assert.IsTrue(group.groupSlug.ToString().Equals("Pet_Group"));
		}
		/// <summary>
		/// Purpose of TC: 
		/// - Create Role
		/// </summary>
		/// /// <summary>
		/// Purpose of TC: 
		/// - Fail Name is require
		/// </summary>
		[TestMethod]
		public void CreateGroup2_WithValidModelFail_ExpectValidNavigation()
		{
			string userName = "tovo1@vanlanguni.vn";
			string IdOwner = "b0245219-c69e-428f-bfc7-ead7192d5936";
			Mock<HttpRequestBase> moqRequest = new Mock<HttpRequestBase>();
			Mock<HttpFileCollectionBase> moqFiles = new Mock<HttpFileCollectionBase>();
			Mock<HttpPostedFileBase> moqPostedFile = new Mock<HttpPostedFileBase>();
			Mock<ControllerContext> controllerContext = new Mock<ControllerContext>();
			Mock<HttpSessionStateBase> session = new Mock<HttpSessionStateBase>();
			Mock<HttpContextBase> mockHttpContext = new Mock<HttpContextBase>();
			//
			GenericIdentity identity = new GenericIdentity("b0245219-c69e-428f-bfc7-ead7192d5936");
			Mock<HttpResponseBase> moqResponse = new Mock<HttpResponseBase>();
			Mock<HttpServerUtilityBase> moqServer = new Mock<HttpServerUtilityBase>();
			Mock<IPrincipal> moqUser = new Mock<IPrincipal>();
			Mock<IIdentity> moqIdentity = new Mock<IIdentity>();
			Mock<UrlHelper> moqUrlHelper = new Mock<UrlHelper>();
			//
			moqRequest.Setup(r => r.Files.Count).Returns(0);
			mockHttpContext.Setup(x => x.Request).Returns(moqRequest.Object);
			//Setup a fake HttpRequest
			HttpPostedFileBase httpPostedFile = Mock.Of<HttpPostedFileBase>();
			Mock<HttpPostedFileBase> mock = Mock.Get(httpPostedFile);
			mock.Setup(_ => _.FileName).Returns("fakeFileName.extension");
			MemoryStream memoryStream = new MemoryStream();
			//...populate fake stream
			//setup mock to return stream
			mock.Setup(_ => _.InputStream).Returns(memoryStream);
			//get session
			mockHttpContext.Setup(ctx => ctx.Session).Returns(session.Object);
			controllerContext.Setup(ctx => ctx.HttpContext).Returns(mockHttpContext.Object);
			controllerContext.Setup(p => p.HttpContext.Session["IdOwner"]).Returns(IdOwner);
			//Arrange
			TestControllerBuilder testControllerBuilder = new TestControllerBuilder();
			PMSEntities db = new PMSEntities();
			AspNetUser user = new AspNetUser();
			user.Id = db.AspNetUsers.First().Id;
			user.UserName = userName;
			GroupController controller = new GroupController();
			testControllerBuilder.InitializeController(controller);
			Group group = new Group();
			group.Id = db.Groups.First().Id;
			group.Name = "";
			group.IdOwner = IdOwner;
			group.Description = "This is a UnitTest Group";
			group.ownerSlug = "Pet";
			group.groupSlug = "Pet_Group";
			controller.ControllerContext = new ControllerContext(mockHttpContext.Object, new RouteData(), controller);
			//get user id
			Mock<IPrincipal> principal = new Moq.Mock<IPrincipal>();
			principal.Setup(p => p.IsInRole("IdOwner")).Returns(true);
			principal.SetupGet(x => x.Identity.Name).Returns(userName);
			controllerContext.SetupGet(x => x.HttpContext.User).Returns(principal.Object);
			controller.ControllerContext = controllerContext.Object;
			// mock group serivce
			GroupService groupService = new GroupService();
			//groupService.createGroup(userName, group);

			//Act
			RedirectToRouteResult redirectRoute = controller.NewGroup(group, httpPostedFile) as RedirectToRouteResult;
			//Assert
			Assert.IsNull(redirectRoute);
			//			Assert.AreEqual("index", redirectRoute.RouteValues["action"]);
		}
		/// <summary>
		/// Purpose of TC: 
		/// - Return view Create Role
		/// </summary>
		[TestMethod]
		public void CreateRole1_WithValidModel_ExpectValidNavigation()
		{
			TestControllerBuilder testControllerBuilder = new TestControllerBuilder();
			ProcessController controller = new ProcessController();
			testControllerBuilder.InitializeController(controller);
			int Idprocess = 3366;

			ViewResult results = controller.AddRole(Idprocess) as ViewResult;
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
			string userName = "tovo1@vanlanguni.vn";
			int processid = 3366;
			GenericIdentity identity = new GenericIdentity("tovo1@vanlanguni.vn");
			Mock<HttpRequestBase> moqRequest = new Mock<HttpRequestBase>();
			Mock<HttpResponseBase> moqResponse = new Mock<HttpResponseBase>();
			Mock<HttpServerUtilityBase> moqServer = new Mock<HttpServerUtilityBase>();
			Mock<IPrincipal> moqUser = new Mock<IPrincipal>();
			Mock<IIdentity> moqIdentity = new Mock<IIdentity>();
			Mock<UrlHelper> moqUrlHelper = new Mock<UrlHelper>();
			Mock<ControllerContext> controllerContext = new Mock<ControllerContext>();
			Mock<HttpSessionStateBase> session = new Mock<HttpSessionStateBase>();
			Mock<HttpContextBase> mockHttpContext = new Mock<HttpContextBase>();
			//Setup a fake HttpRequest
			Mock<HttpPostedFileBase> moqPostedFile = new Mock<HttpPostedFileBase>();
			Mock<IPrincipal> principal = new Mock<IPrincipal>();
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
			ProcessController controller = new ProcessController();
			controller.ControllerContext = controllerContext.Object;
			testControllerBuilder.InitializeController(controller);
			PMSEntities db = new PMSEntities();
			Role role = new Role();
			role.Id = db.Roles.First().Id;
			role.IdProcess = processid;
			role.Name = "Role Unittest";
			role.Description = "This is a role unit test";
			//get user id
			principal.Setup(p => p.IsInRole("IdOWwner")).Returns(true);
			principal.SetupGet(x => x.Identity.Name).Returns(userName);
			controllerContext.SetupGet(x => x.HttpContext.User).Returns(principal.Object);
			controller.ControllerContext = controllerContext.Object;

			//act
			RedirectToRouteResult redirectRoute = controller.AddRole(role) as RedirectToRouteResult;
			//assert
			Assert.IsNotNull(redirectRoute);
			Assert.AreEqual("addrole", redirectRoute.RouteValues["action"]);
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
			ProcessController controller = new ProcessController();
			testControllerBuilder.InitializeController(controller);
			int Idprocess = 135;

			ViewResult results = controller.NewProcess(Idprocess) as ViewResult;
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
			string IdOwner = "64e10037-6c10-4544-a853-a2952330bf8e";
			int groupId = 4120;
			string userName = "tuanho10@vanlanguni.vn";
			Mock<HttpRequestBase> moqRequest = new Mock<HttpRequestBase>();
			Mock<HttpFileCollectionBase> moqFiles = new Mock<HttpFileCollectionBase>();
			Mock<HttpPostedFileBase> moqPostedFile = new Mock<HttpPostedFileBase>();
			Mock<ControllerContext> controllerContext = new Mock<ControllerContext>();
			Mock<HttpSessionStateBase> session = new Mock<HttpSessionStateBase>();
			Mock<HttpContextBase> mockHttpContext = new Mock<HttpContextBase>();
			//
			GenericIdentity identity = new GenericIdentity("64e10037-6c10-4544-a853-a2952330bf8e");
			Mock<HttpResponseBase> moqResponse = new Mock<HttpResponseBase>();
			Mock<HttpServerUtilityBase> moqServer = new Mock<HttpServerUtilityBase>();
			Mock<IPrincipal> moqUser = new Mock<IPrincipal>();
			Mock<IIdentity> moqIdentity = new Mock<IIdentity>();
			Mock<UrlHelper> moqUrlHelper = new Mock<UrlHelper>();
			//
			moqRequest.Setup(r => r.Files.Count).Returns(0);
			mockHttpContext.Setup(x => x.Request).Returns(moqRequest.Object);
			//Setup a fake HttpRequest
			HttpPostedFileBase httpPostedFile = Mock.Of<HttpPostedFileBase>();
			Mock<HttpPostedFileBase> mock = Mock.Get(httpPostedFile);
			mock.Setup(_ => _.FileName).Returns("fakeFileName.extension");
			MemoryStream memoryStream = new MemoryStream();
			//...populate fake stream
			//setup mock to return stream
			mock.Setup(_ => _.InputStream).Returns(memoryStream);
			//get session
			mockHttpContext.Setup(ctx => ctx.Session).Returns(session.Object);
			controllerContext.Setup(ctx => ctx.HttpContext).Returns(mockHttpContext.Object);
			controllerContext.Setup(p => p.HttpContext.Session["IdOwner"]).Returns(IdOwner);
			//Arrange
			TestControllerBuilder testControllerBuilder = new TestControllerBuilder();
			PMSEntities db = new PMSEntities();
			AspNetUser user = db.AspNetUsers.Find(IdOwner);
			ProcessController controller = new ProcessController();
			testControllerBuilder.InitializeController(controller);
			Process process = new Process();
			process.Id = db.Groups.First().Id;
			process.IdOwner = IdOwner;
			process.IdGroup = 4120;
			process.Name = "Process 13 UnitTest";
			process.Description = "Demo 1 unit test create process is correct";
			process.Avatar = "image.png";
			controller.ControllerContext = new ControllerContext(mockHttpContext.Object, new RouteData(), controller);
			//get user id
			Mock<IPrincipal> principal = new Moq.Mock<IPrincipal>();
			principal.Setup(p => p.IsInRole("IdOwner")).Returns(true);
			principal.SetupGet(x => x.Identity.Name).Returns(userName);
			controllerContext.SetupGet(x => x.HttpContext.User).Returns(principal.Object);
			controller.ControllerContext = controllerContext.Object;
			// mock group serivce
			//ProcessService processService = new ProcessService();
			//processService.createProcess(4120, userName, process);

			//Act
			RedirectToRouteResult redirectRoute = controller.NewProcess(groupId, process, httpPostedFile) as RedirectToRouteResult;
			//Assert
			Assert.IsNotNull(redirectRoute);
			Assert.AreEqual("Draw", redirectRoute.RouteValues["controller"]);
			Assert.IsTrue(process.Name.ToString().Equals("Process 13 UnitTest"));
			Assert.IsTrue(process.Description.ToString().Equals("Demo 1 unit test create process is correct"));
		}
		/// <summary>
		/// Purpose of TC: 
		/// - Create Process	
		/// </summary>
		[TestMethod]
		public void CreateDrawProcess_WithValidModel_ExpectValidNavigation()
		{
			string IdOwner = "64e10037-6c10-4544-a853-a2952330bf8e";
			int groupId = 4120;
			string userName = "tuanho10@vanlanguni.vn";
			Mock<HttpRequestBase> moqRequest = new Mock<HttpRequestBase>();
			Mock<HttpFileCollectionBase> moqFiles = new Mock<HttpFileCollectionBase>();
			Mock<HttpPostedFileBase> moqPostedFile = new Mock<HttpPostedFileBase>();
			Mock<ControllerContext> controllerContext = new Mock<ControllerContext>();
			Mock<HttpSessionStateBase> session = new Mock<HttpSessionStateBase>();
			Mock<HttpContextBase> mockHttpContext = new Mock<HttpContextBase>();
			//
			GenericIdentity identity = new GenericIdentity("64e10037-6c10-4544-a853-a2952330bf8e");
			Mock<HttpResponseBase> moqResponse = new Mock<HttpResponseBase>();
			Mock<HttpServerUtilityBase> moqServer = new Mock<HttpServerUtilityBase>();
			Mock<IPrincipal> moqUser = new Mock<IPrincipal>();
			Mock<IIdentity> moqIdentity = new Mock<IIdentity>();
			Mock<UrlHelper> moqUrlHelper = new Mock<UrlHelper>();
			//
			moqRequest.Setup(r => r.Files.Count).Returns(0);
			mockHttpContext.Setup(x => x.Request).Returns(moqRequest.Object);
			//Setup a fake HttpRequest
			HttpPostedFileBase httpPostedFile = Mock.Of<HttpPostedFileBase>();
			Mock<HttpPostedFileBase> mock = Mock.Get(httpPostedFile);
			mock.Setup(_ => _.FileName).Returns("fakeFileName.extension");
			MemoryStream memoryStream = new MemoryStream();
			//...populate fake stream
			//setup mock to return stream
			mock.Setup(_ => _.InputStream).Returns(memoryStream);
			//get session
			mockHttpContext.Setup(ctx => ctx.Session).Returns(session.Object);
			controllerContext.Setup(ctx => ctx.HttpContext).Returns(mockHttpContext.Object);
			controllerContext.Setup(p => p.HttpContext.Session["IdOwner"]).Returns(IdOwner);
			//Arrange
			TestControllerBuilder testControllerBuilder = new TestControllerBuilder();
			PMSEntities db = new PMSEntities();
			AspNetUser user = db.AspNetUsers.Find(IdOwner);
			ProcessController controller = new ProcessController();
			testControllerBuilder.InitializeController(controller);
			Process process = new Process();
			process.Id = db.Groups.First().Id;
			process.IdOwner = IdOwner;
			process.IdGroup = 4120;
			process.Name = "Process 13 UnitTest";
			process.Description = "Demo 1 unit test create process is correct";
			process.Avatar = "image.png";
			process.DataJson = "{ \"class\": \"go.GraphLinksModel\" , \"nodeDataArray\" : [ {\"text\" : \"Start\" ,\"figure\" : \"Circle\",\"fill\" : \"#1aff1a\",\"key\" : -1,\"loc\" : \"280 60\",\"input\" : 0,\"output\" : 1},{\"text\":\"Step1\",\"key\":-2,\"loc\":\"430 180\",\"input\":1,\"output\":1},{\"text\":\"dk\",\"figure\":\"Diamond\",\"fill\":\"lightskyblue\",\"key\":-3,\"loc\":\"600 250\",\"input\":1,\"output\":2},{ \"text\":\"Step2\",\"key\":-4,\"loc\":\"830 130\",\"input\":1,\"output\":1},{\"text\":\"Step3\",\"key\":-5,\"loc\":\"848.0393753051758 360\",\"input\":1,\"output\":1,\"size\":\"74 33.875448608398436\"},{ \"text\":\"Step\", \"key\":-6,\"loc\":\"1030 250\", \"input\":2,\"output\":1},{\"text\":\"End\",\"figure\":\"Circle\",\"fill\":\"#f20000\",\"key\":-7,\"loc\":\"1200 250\",\"input\":1,\"output\":0}] }"; ;
			controller.ControllerContext = new ControllerContext(mockHttpContext.Object, new RouteData(), controller);
			//get user id
			Mock<IPrincipal> principal = new Moq.Mock<IPrincipal>();
			principal.Setup(p => p.IsInRole("IdOwner")).Returns(true);
			principal.SetupGet(x => x.Identity.Name).Returns(userName);
			controllerContext.SetupGet(x => x.HttpContext.User).Returns(principal.Object);
			controller.ControllerContext = controllerContext.Object;
			// mock group serivce
			ProcessService processService = new ProcessService();
			processService.createProcess(4120, userName, process);

			//Act
			RedirectToRouteResult redirectRoute = controller.NewProcess(groupId, process, httpPostedFile) as RedirectToRouteResult;
			//Assert
			Assert.IsNotNull(redirectRoute);
			Assert.AreEqual("Draw", redirectRoute.RouteValues["controller"]);
			Assert.IsTrue(process.Name.ToString().Equals("Process 13 UnitTest"));
			Assert.IsTrue(process.Description.ToString().Equals("Demo 1 unit test create process is correct"));
		}
		/// <summary>
		/// Purpose of TC: 
		/// - Create Process Fail
		/// Require Name
		/// </summary>
		[TestMethod]
		public void CreateProcess2_WithValidModelFail_ExpectValidNavigation()
		{
			string IdOwner = "64e10037-6c10-4544-a853-a2952330bf8e";
			int groupId = 4120;
			string userName = "tuanho10@vanlanguni.vn";
			Mock<HttpRequestBase> moqRequest = new Mock<HttpRequestBase>();
			Mock<HttpFileCollectionBase> moqFiles = new Mock<HttpFileCollectionBase>();
			Mock<HttpPostedFileBase> moqPostedFile = new Mock<HttpPostedFileBase>();
			Mock<ControllerContext> controllerContext = new Mock<ControllerContext>();
			Mock<HttpSessionStateBase> session = new Mock<HttpSessionStateBase>();
			Mock<HttpContextBase> mockHttpContext = new Mock<HttpContextBase>();
			//
			GenericIdentity identity = new GenericIdentity("64e10037-6c10-4544-a853-a2952330bf8e");
			Mock<HttpResponseBase> moqResponse = new Mock<HttpResponseBase>();
			Mock<HttpServerUtilityBase> moqServer = new Mock<HttpServerUtilityBase>();
			Mock<IPrincipal> moqUser = new Mock<IPrincipal>();
			Mock<IIdentity> moqIdentity = new Mock<IIdentity>();
			Mock<UrlHelper> moqUrlHelper = new Mock<UrlHelper>();
			//
			moqRequest.Setup(r => r.Files.Count).Returns(0);
			mockHttpContext.Setup(x => x.Request).Returns(moqRequest.Object);
			//Setup a fake HttpRequest
			HttpPostedFileBase httpPostedFile = Mock.Of<HttpPostedFileBase>();
			Mock<HttpPostedFileBase> mock = Mock.Get(httpPostedFile);
			mock.Setup(_ => _.FileName).Returns("fakeFileName.extension");
			MemoryStream memoryStream = new MemoryStream();
			//...populate fake stream
			//setup mock to return stream
			mock.Setup(_ => _.InputStream).Returns(memoryStream);
			//get session
			mockHttpContext.Setup(ctx => ctx.Session).Returns(session.Object);
			controllerContext.Setup(ctx => ctx.HttpContext).Returns(mockHttpContext.Object);
			controllerContext.Setup(p => p.HttpContext.Session["IdOwner"]).Returns(IdOwner);
			//Arrange
			TestControllerBuilder testControllerBuilder = new TestControllerBuilder();
			PMSEntities db = new PMSEntities();
			AspNetUser user = db.AspNetUsers.Find(IdOwner);
			ProcessController controller = new ProcessController();
			testControllerBuilder.InitializeController(controller);
			Process process = new Process();
			process.Id = db.Groups.First().Id;
			process.IdOwner = IdOwner;
			process.IdGroup = 4120;
			process.Name = "";
			process.Description = "Demo 1 unit test create process is correct";
			process.Avatar = "image.png";
			controller.ControllerContext = new ControllerContext(mockHttpContext.Object, new RouteData(), controller);
			//get user id
			Mock<IPrincipal> principal = new Moq.Mock<IPrincipal>();
			principal.Setup(p => p.IsInRole("IdOwner")).Returns(true);
			principal.SetupGet(x => x.Identity.Name).Returns(userName);
			controllerContext.SetupGet(x => x.HttpContext.User).Returns(principal.Object);
			controller.ControllerContext = controllerContext.Object;
			// mock group serivce
			//ProcessService processService = new ProcessService();
			//processService.createProcess(4120, userName, process);

			//Act
			RedirectToRouteResult redirectRoute = controller.NewProcess(groupId, process, httpPostedFile) as RedirectToRouteResult;
			//Assert
			Assert.IsNull(redirectRoute);
			//Assert.AreEqual("Draw", redirectRoute.RouteValues["controller"]);
		}
		/// <summary>
		/// Purpose of TC: 
		/// - Create Task	
		/// </summary>
		[TestMethod]
		public void CreateTask_WithValidModel_ExpectValidNavigation()
		{
			TestControllerBuilder testControllerBuilder = new TestControllerBuilder();
			string userName = "tovo1@vanlanguni.vn";
			int processid = 3366;
			GenericIdentity identity = new GenericIdentity("tovo1@vanlanguni.vn");
			Mock<HttpRequestBase> moqRequest = new Mock<HttpRequestBase>();
			Mock<HttpResponseBase> moqResponse = new Mock<HttpResponseBase>();
			Mock<HttpServerUtilityBase> moqServer = new Mock<HttpServerUtilityBase>();
			Mock<IPrincipal> moqUser = new Mock<IPrincipal>();
			Mock<IIdentity> moqIdentity = new Mock<IIdentity>();
			Mock<UrlHelper> moqUrlHelper = new Mock<UrlHelper>();
			Mock<ControllerContext> controllerContext = new Mock<ControllerContext>();
			Mock<HttpSessionStateBase> session = new Mock<HttpSessionStateBase>();
			Mock<HttpContextBase> mockHttpContext = new Mock<HttpContextBase>();
			//Setup a fake HttpRequest
			Mock<HttpPostedFileBase> moqPostedFile = new Mock<HttpPostedFileBase>();
			Mock<IPrincipal> principal = new Mock<IPrincipal>();
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
			ProcessController controller = new ProcessController();
			controller.ControllerContext = controllerContext.Object;
			testControllerBuilder.InitializeController(controller);
			PMSEntities db = new PMSEntities();
			TaskProcess taskProcess = new TaskProcess();
			taskProcess.Id = db.TaskProcesses.First().Id;
			taskProcess.IdStep = 5030;
			taskProcess.Name = "Task UnitTest";
			taskProcess.Description = "Demo unittest create task is correct";
			taskProcess.Position = 13;
			//get user id
			principal.Setup(p => p.IsInRole("IdOWwner")).Returns(true);
			principal.SetupGet(x => x.Identity.Name).Returns(userName);
			controllerContext.SetupGet(x => x.HttpContext.User).Returns(principal.Object);
			controller.ControllerContext = controllerContext.Object;

			//act
			ViewResult result = controller.AddTask(5030) as ViewResult;
			//assert
			Assert.IsNotNull(result);
			Assert.IsTrue(taskProcess.Name.ToString().Equals("Task UnitTest"));
			Assert.IsTrue(taskProcess.Description.ToString().Equals("Demo unittest create task is correct"));
		}
		/// <summary>
		/// Purpose of TC: 
		/// - Create Form Task	
		/// </summary>
		[TestMethod]
		public void CreateFormTask_WithValidModel_ExpectValidNavigation()
		{
			TestControllerBuilder testControllerBuilder = new TestControllerBuilder();
			string userName = "tovo1@vanlanguni.vn";
			int processid = 3365;
			GenericIdentity identity = new GenericIdentity("tovo1@vanlanguni.vn");
			Mock<HttpRequestBase> moqRequest = new Mock<HttpRequestBase>();
			Mock<HttpResponseBase> moqResponse = new Mock<HttpResponseBase>();
			Mock<HttpServerUtilityBase> moqServer = new Mock<HttpServerUtilityBase>();
			Mock<IPrincipal> moqUser = new Mock<IPrincipal>();
			Mock<IIdentity> moqIdentity = new Mock<IIdentity>();
			Mock<UrlHelper> moqUrlHelper = new Mock<UrlHelper>();
			Mock<ControllerContext> controllerContext = new Mock<ControllerContext>();
			Mock<HttpSessionStateBase> session = new Mock<HttpSessionStateBase>();
			Mock<HttpContextBase> mockHttpContext = new Mock<HttpContextBase>();
			//Setup a fake HttpRequest
			Mock<HttpPostedFileBase> moqPostedFile = new Mock<HttpPostedFileBase>();
			Mock<IPrincipal> principal = new Mock<IPrincipal>();
			principal.Setup(p => p.IsInRole("IdOwner")).Returns(true);
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
			//emulator data

			//arrange
			HttpContext.Current = FakeHttpContext();
			ProcessController controller = new ProcessController();
			controller.ControllerContext = controllerContext.Object;
			testControllerBuilder.InitializeController(controller);
			PMSEntities db = new PMSEntities();
			TaskProcess taskProcess = new TaskProcess();
			taskProcess.Id = db.TaskProcesses.First().Id;
			taskProcess.IdStep = 4027;
			taskProcess.Name = "FormTask UnitTest";
			taskProcess.Description = "Demo unittest create formtask is correct";
			taskProcess.Position = 13;
			taskProcess.ValueFormJson = "{ \"type\": \"text\" , \"label\" : \"Text Field\"}";
			//taskProcess.ValueFormJson =/* "[{"type":"text","label":"Text Field","className":"form - control","name":"text - 1555143047575","subtype":"text"},{"type":"text","label":"Text Field","className":"form - control","name":"text - 1555143041116","subtype":"text"}]";*/
			//get user id
			principal.Setup(p => p.IsInRole("IdOWwner")).Returns(true);
			principal.SetupGet(x => x.Identity.Name).Returns(userName);
			controllerContext.SetupGet(x => x.HttpContext.User).Returns(principal.Object);
			controller.ControllerContext = controllerContext.Object;

			//act
			ViewResult result = controller.AddFormTask(4027) as ViewResult;
			//assert
			Assert.IsNotNull(result);
		}
		/// <summary>
		/// Purpose of TC: 
		/// - Return Task View Available
		/// </summary>
		[TestMethod]
		public void ReturnTaskView_Available()
		{
			ProcessController controller = new ProcessController();
			//user need get
			string userName = "duyho9@vanlanguni.vn";
			Mock<ControllerContext> controllerContext = new Mock<ControllerContext>();
			Mock<HttpSessionStateBase> session = new Mock<HttpSessionStateBase>();
			Mock<HttpContextBase> mockHttpContext = new Mock<HttpContextBase>();

			Mock<IPrincipal> principal = new Mock<IPrincipal>();
			//get user id
			principal.Setup(p => p.IsInRole("IsOwner")).Returns(true);
			principal.SetupGet(x => x.Identity.Name).Returns(userName);
			controllerContext.SetupGet(x => x.HttpContext.User).Returns(principal.Object);
			controller.ControllerContext = controllerContext.Object;

			principal.Setup(x => x.IsInRole(It.IsAny<string>())).Returns(true);
			//get session
			mockHttpContext.Setup(ctx => ctx.Session).Returns(session.Object);
			controllerContext.Setup(ctx => ctx.HttpContext).Returns(mockHttpContext.Object);
			controllerContext.Setup(p => p.HttpContext.Session["idTask"]).Returns(new TaskProcess
			{
				Id = 4909,
				IdRole = 3967,
			});
			controllerContext.SetupGet(x => x.HttpContext.User).Returns(principal.Object);
			controller.ControllerContext = controllerContext.Object;
			// action
			ViewResult result = controller.ShowTask(4909) as ViewResult;

			// assert
			Assert.IsNotNull(result);
		}
		/// <summary>
		/// Purpose of TC: 
		/// - Return Form Task View UnAvailable
		/// </summary>
		[TestMethod]
		public void ReturnFormTaskView_UnAvailable()
		{
			ProcessController controller = new ProcessController();
			//user need get
			string userName = "tovo1@vanlanguni.vn";
			Mock<ControllerContext> controllerContext = new Mock<ControllerContext>();
			Mock<HttpSessionStateBase> session = new Mock<HttpSessionStateBase>();
			Mock<HttpContextBase> mockHttpContext = new Mock<HttpContextBase>();

			Mock<IPrincipal> principal = new Mock<IPrincipal>();
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
			ViewResult result = controller.ShowFormTask(1212) as ViewResult;

			// assert
			Assert.IsNull(result);
		}
		/// <summary>
		/// Purpose of TC: 
		/// - Return Task Run View Available
		/// </summary>
		[TestMethod]
		public void ReturnTaskRunView_Available()
		{
			ProcessRunController controller = new ProcessRunController();
			//user need get
			string userName = "tovo1@vanlanguni.vn";
			Mock<ControllerContext> controllerContext = new Mock<ControllerContext>();
			Mock<HttpSessionStateBase> session = new Mock<HttpSessionStateBase>();
			Mock<HttpContextBase> mockHttpContext = new Mock<HttpContextBase>();

			Mock<IPrincipal> principal = new Mock<IPrincipal>();
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
				Id = 3597,
				IdRole = 3298,
			});
			controllerContext.SetupGet(x => x.HttpContext.User).Returns(principal.Object);
			controller.ControllerContext = controllerContext.Object;
			// action
			ViewResult result = controller.Detailtask(3597) as ViewResult;

			// assert
			Assert.IsNotNull(result);
		}
		/// <summary>
		/// Purpose of TC: 
		/// - Return Task Run View UnAvailable
		/// </summary>
		[TestMethod]
		public void ReturnTaskRunView_UnAvailable()
		{
			ProcessRunController controller = new ProcessRunController();
			//user need get
			string userName = "tovo1@vanlanguni.vn";
			Mock<ControllerContext> controllerContext = new Mock<ControllerContext>();
			Mock<HttpSessionStateBase> session = new Mock<HttpSessionStateBase>();
			Mock<HttpContextBase> mockHttpContext = new Mock<HttpContextBase>();

			Mock<IPrincipal> principal = new Mock<IPrincipal>();
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
			ViewResult result = controller.Detailtask(1212) as ViewResult;

			// assert
			Assert.IsNull(result);
		}
		/// <summary>
		/// Purpose of TC: 
		/// - Return Task View UnAvailable
		/// </summary>
		[TestMethod]
		public void ReturnTaskView_UnAvailable()
		{
			ProcessController controller = new ProcessController();
			//user need get
			string userName = "tovo1@vanlanguni.vn";
			Mock<ControllerContext> controllerContext = new Mock<ControllerContext>();
			Mock<HttpSessionStateBase> session = new Mock<HttpSessionStateBase>();
			Mock<HttpContextBase> mockHttpContext = new Mock<HttpContextBase>();

			Mock<IPrincipal> principal = new Mock<IPrincipal>();
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
		/// - Return Task View in run process available
		/// </summary>
		[TestMethod]
		public void ReturnTaskView_RunProcess_Available()
		{
			ProcessRunController controller = new ProcessRunController();
			//user need get
			string userName = "tovo1@vanlanguni.vn";
			Mock<ControllerContext> controllerContext = new Mock<ControllerContext>();
			Mock<HttpSessionStateBase> session = new Mock<HttpSessionStateBase>();
			Mock<HttpContextBase> mockHttpContext = new Mock<HttpContextBase>();

			Mock<IPrincipal> principal = new Mock<IPrincipal>();
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
				Id = 3549,
				IdRole = 2217,
			});
			controllerContext.SetupGet(x => x.HttpContext.User).Returns(principal.Object);
			controller.ControllerContext = controllerContext.Object;
			// action
			ViewResult result = controller.Detailtask(3549) as ViewResult;

			// assert
			Assert.IsNotNull(result);
		}
		/// <summary>
		/// Purpose of TC: 
		/// - Return Task View in Run Process UnAvailable
		/// </summary>
		[TestMethod]
		public void ReturnTaskView_RunProcess_UnAvailable()
		{
			ProcessController controller = new ProcessController();
			//user need get
			string userName = "tovo1@vanlanguni.vn";
			Mock<ControllerContext> controllerContext = new Mock<ControllerContext>();
			Mock<HttpSessionStateBase> session = new Mock<HttpSessionStateBase>();
			Mock<HttpContextBase> mockHttpContext = new Mock<HttpContextBase>();

			Mock<IPrincipal> principal = new Mock<IPrincipal>();
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
		/// - Return Form Task View in run process available
		/// </summary>
		[TestMethod]
		public void ReturnFormTaskView_RunProcess_Available()
		{
			ProcessRunController controller = new ProcessRunController();
			//user need get
			string userName = "tovo1@vanlanguni.vn";
			Mock<ControllerContext> controllerContext = new Mock<ControllerContext>();
			Mock<HttpSessionStateBase> session = new Mock<HttpSessionStateBase>();
			Mock<HttpContextBase> mockHttpContext = new Mock<HttpContextBase>();

			Mock<IPrincipal> principal = new Mock<IPrincipal>();
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
				Id = 3485,
				IdRole = 2193,
			});
			controllerContext.SetupGet(x => x.HttpContext.User).Returns(principal.Object);
			controller.ControllerContext = controllerContext.Object;
			// action
			ViewResult result = controller.Detailtask(3485) as ViewResult;

			// assert
			Assert.IsNotNull(result);
		}
		/// <summary>
		/// Purpose of TC: 
		/// - Return Form Task View in Run Process UnAvailable
		/// </summary>
		[TestMethod]
		public void ReturnFormTaskView_RunProcess_UnAvailable()
		{
			ProcessController controller = new ProcessController();
			//user need get
			string userName = "tovo1@vanlanguni.vn";
			Mock<ControllerContext> controllerContext = new Mock<ControllerContext>();
			Mock<HttpSessionStateBase> session = new Mock<HttpSessionStateBase>();
			Mock<HttpContextBase> mockHttpContext = new Mock<HttpContextBase>();

			Mock<IPrincipal> principal = new Mock<IPrincipal>();
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
			string userName = "tovo1@vanlanguni.vn";
			int IdProcess = 3366;
			Mock<HttpSessionStateBase> session = new Mock<HttpSessionStateBase>();
			Mock<HttpContextBase> mockHttpContext = new Mock<HttpContextBase>();
			Mock<ControllerContext> mockControllerContext = new Mock<ControllerContext>();
			GroupService groupService = new GroupService();
			TestControllerBuilder testControllerBuilder = new TestControllerBuilder();
			//get session
			mockHttpContext.Setup(ctx => ctx.Session).Returns(session.Object);
			mockControllerContext.Setup(ctx => ctx.HttpContext).Returns(mockHttpContext.Object);
			//mockControllerContext.Setup(p => p.HttpContext.Session["Process"]).Returns();
			PMSEntities db = new PMSEntities();
			Group group = new Group();
			group.Id = db.Groups.First().Id;

			Process process = new Process();
			process.Id = db.Processes.First().Id;
			ProcessController controller = new ProcessController();
			testControllerBuilder.InitializeController(controller);
			controller.ControllerContext = mockControllerContext.Object;
			//get user id
			Mock<ControllerContext> controllerContext = new Mock<ControllerContext>();
			Mock<IPrincipal> principal = new Moq.Mock<IPrincipal>();
			principal.Setup(p => p.IsInRole("owner")).Returns(true);
			principal.SetupGet(x => x.Identity.Name).Returns(userName);
			controllerContext.SetupGet(x => x.HttpContext.User).Returns(principal.Object);
			controller.ControllerContext = controllerContext.Object;
			//Act         
			ViewResult result = controller.ShowStep(IdProcess) as ViewResult;

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
			string userName = "tovo1@vanlanguni.vn";
			Mock<Group> mockGroup = new Mock<Group>();
			Mock<ControllerContext> controllerContext = new Mock<ControllerContext>();
			Mock<HttpSessionStateBase> session = new Mock<HttpSessionStateBase>();
			Mock<HttpContextBase> mockHttpContext = new Mock<HttpContextBase>();

			//get session
			mockHttpContext.Setup(ctx => ctx.Session).Returns(session.Object);
			controllerContext.Setup(ctx => ctx.HttpContext).Returns(mockHttpContext.Object);
			controllerContext.Setup(p => p.HttpContext.Session["Role"]).Returns(new Role
			{
				Id = 3299,
				IdProcess = 4369
			});
			//arrange
			//get user id
			Mock<IPrincipal> principal = new Moq.Mock<IPrincipal>();
			principal.Setup(p => p.IsInRole("owner")).Returns(true);
			principal.SetupGet(x => x.Identity.Name).Returns(userName);
			controllerContext.SetupGet(x => x.HttpContext.User).Returns(principal.Object);
			controllerUnderTest.ControllerContext = controllerContext.Object;

			ViewResult result = controllerUnderTest.EditRole(3299) as ViewResult;
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
			string userName = "tovo1@vanlanguni.vn";
			Mock<Group> mockGroup = new Mock<Group>();
			Mock<ControllerContext> controllerContext = new Mock<ControllerContext>();
			Mock<HttpSessionStateBase> session = new Mock<HttpSessionStateBase>();
			Mock<HttpContextBase> mockHttpContext = new Mock<HttpContextBase>();

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
			Mock<IPrincipal> principal = new Moq.Mock<IPrincipal>();
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
			ProcessController controller = new ProcessController();
			testControllerBuilder.InitializeController(controller);
			Role role = new Role();
			PMSEntities db = new PMSEntities();
			//arrange
			Role editrole = db.Roles.First();
			editrole.Name = "Role UnitTest";
			editrole.Description = "This is role unittest";

			using (TransactionScope scope = new TransactionScope())
			{
				ViewResult results = controller.EditRole(3299) as ViewResult;
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
			int idGroup = 6132;
			Mock<HttpApplicationStateBase> application = new Mock<HttpApplicationStateBase>();
			Mock<Group> mockGroup = new Mock<Group>();
			Mock<Participate> mockParicipate = new Mock<Participate>();
			Mock<ControllerContext> controllerContext = new Mock<ControllerContext>();
			Mock<HttpSessionStateBase> session = new Mock<HttpSessionStateBase>();
			Mock<HttpContextBase> mockHttpContext = new Mock<HttpContextBase>();

			TestControllerBuilder testControllerBuilder = new TestControllerBuilder();
			GroupController controller = new GroupController();
			testControllerBuilder.InitializeController(controller);
			Participate participate = new Participate();
			PMSEntities db = new PMSEntities();

			//get session
			mockHttpContext.Setup(ctx => ctx.Session).Returns(session.Object);
			controllerContext.Setup(ctx => ctx.HttpContext).Returns(mockHttpContext.Object);
			controllerContext.Setup(p => p.HttpContext.Session["groupid"]).Returns(idGroup);

			//arrange
			Group group = db.Groups.First();
			group.Id = idGroup;
			Participate editrolemenmber = db.Participates.First();
			editrolemenmber.IsManager = false;
			editrolemenmber.IdGroup = 6132;
			//check mock
			controller.ControllerContext = controllerContext.Object;

			using (TransactionScope scope = new TransactionScope())
			{
				ViewResult results = controller.EditRoleMember(6206) as ViewResult;
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
			ProcessController controller = new ProcessController();
			testControllerBuilder.InitializeController(controller);
			Step step = new Step();
			PMSEntities db = new PMSEntities();
			//arrange
			Step editstep = db.Steps.First();
			editstep.Id = 4020;
			editstep.Description = "Edit Step UnitTest Process";
			int group = 6126;

			using (TransactionScope scope = new TransactionScope())
			{
				RedirectToRouteResult redirectRoute = controller.EditStep(group, editstep) as RedirectToRouteResult;
				//assert
				Assert.IsNotNull(redirectRoute);
				Assert.AreEqual("ShowStep", redirectRoute.RouteValues["action"]);
			}
		}
		/// <summary>
		/// Purpose of TC: 
		/// - Hanle Edit Task	
		/// </summary>
		[TestMethod]
		public void EditTask_WithValidModel_ExpectValidNavigation()
		{
			var idTask = 2310;
			TestControllerBuilder testControllerBuilder = new TestControllerBuilder();
			string userName = "tovo1@vanlanguni.vn";
			GenericIdentity identity = new GenericIdentity("tovo1@vanlanguni.vn");
			Mock<HttpRequestBase> moqRequest = new Mock<HttpRequestBase>();
			Mock<HttpResponseBase> moqResponse = new Mock<HttpResponseBase>();
			Mock<HttpServerUtilityBase> moqServer = new Mock<HttpServerUtilityBase>();
			Mock<IPrincipal> moqUser = new Mock<IPrincipal>();
			Mock<IIdentity> moqIdentity = new Mock<IIdentity>();
			Mock<UrlHelper> moqUrlHelper = new Mock<UrlHelper>();
			Mock<ControllerContext> controllerContext = new Mock<ControllerContext>();
			Mock<HttpSessionStateBase> session = new Mock<HttpSessionStateBase>();
			Mock<HttpContextBase> mockHttpContext = new Mock<HttpContextBase>();

			//Setup a fake HttpRequest
			Mock<HttpPostedFileBase> moqPostedFile = new Mock<HttpPostedFileBase>();
			Mock<IPrincipal> principal = new Mock<IPrincipal>();
			principal.Setup(p => p.IsInRole("IdOwner")).Returns(true);
			principal.SetupGet(x => x.Identity.Name).Returns("tovo1@vanlanguni.vn");
			principal.Setup(x => x.Identity).Returns(identity);
			principal.Setup(x => x.IsInRole(It.IsAny<string>())).Returns(true);
			//get session
			mockHttpContext.Setup(ctx => ctx.Session).Returns(session.Object);
			controllerContext.Setup(ctx => ctx.HttpContext).Returns(mockHttpContext.Object);
			controllerContext.Setup(p => p.HttpContext.Session["idTask"]).Returns(idTask);
			controllerContext.SetupGet(x => x.HttpContext.User).Returns(principal.Object);
			moqRequest.Setup(r => r.Files.Count).Returns(0);
			mockHttpContext.Setup(x => x.Request).Returns(moqRequest.Object);
			principal.Setup(p => p.IsInRole("Manager")).Returns(true);
			controllerContext.SetupGet(p => p.HttpContext.Request.IsAuthenticated).Returns(true);
			moqIdentity.Setup(id => id.IsAuthenticated).Returns(true);
			Areas.API.Controllers.ProcessController areasController = new Areas.API.Controllers.ProcessController();
			testControllerBuilder.InitializeController(areasController);
			PMSEntities db = new PMSEntities();
			//arrange
			TaskProcess edittask = db.TaskProcesses.First();
			//get user id
			principal.Setup(p => p.IsInRole("IdOWwner")).Returns(true);
			principal.SetupGet(x => x.Identity.Name).Returns(userName);
			controllerContext.SetupGet(x => x.HttpContext.User).Returns(principal.Object);
			areasController.ControllerContext = controllerContext.Object;

			using (TransactionScope scope = new TransactionScope())
			{
				JsonResult redirectRoute = areasController.EditTask("EditTask demo unit test 13", 2310, "This is task to edittask demo unit test 13", "inputConfig unittest", "fileConfig unittest") as JsonResult;
				//assert
				Assert.IsNotNull(redirectRoute);
				//Assert.AreEqual("ShowFormTask", redirectRoute.RouteValues["action"]);

			}
		}
		/// <summary>
		/// Purpose of TC: 
		/// - Hanle Edit Group		
		/// </summary>
		[TestMethod]
		public void EditGroup_WithValidModel_ExpectValidNavigation()
		{
			int idGroup = 6136;
			Mock<ControllerContext> controllerContext = new Mock<ControllerContext>();
			Mock<HttpFileCollectionBase> moqFiles = new Mock<HttpFileCollectionBase>();
			Mock<HttpPostedFileBase> moqPostedFile = new Mock<HttpPostedFileBase>();
			//mock session
			Mock<HttpSessionStateBase> session = new Mock<HttpSessionStateBase>();
			Mock<HttpContextBase> mockHttpContext = new Mock<HttpContextBase>();
			//Setup a fake HttpRequest
			HttpPostedFileBase httpPostedFile = Mock.Of<HttpPostedFileBase>();
			Mock<HttpPostedFileBase> mock = Mock.Get(httpPostedFile);
			mock.Setup(_ => _.FileName).Returns("fakeFileName.extension");
			MemoryStream memoryStream = new MemoryStream();
			mock.Setup(_ => _.InputStream).Returns(memoryStream);
			//...populate fake stream
			//setup mock to return stream
			//get session
			mockHttpContext.Setup(ctx => ctx.Session).Returns(session.Object);
			controllerContext.Setup(ctx => ctx.HttpContext).Returns(mockHttpContext.Object);
			controllerContext.Setup(p => p.HttpContext.Session["groupid"]).Returns(idGroup);


			TestControllerBuilder testControllerBuilder = new TestControllerBuilder();
			GroupController controller = new GroupController();
			testControllerBuilder.InitializeController(controller);

			Group group = new Group();
			PMSEntities db = new PMSEntities();
			Group editgroup = db.Groups.First();
			editgroup.Name = "Group UnitTest";
			editgroup.Description = "This is group unittest";
			//check mock
			controller.ControllerContext = controllerContext.Object;
			using (TransactionScope scope = new TransactionScope())
			{
				RedirectToRouteResult redirectRoute = controller.Edit(editgroup, httpPostedFile) as RedirectToRouteResult;
				//assert
				Assert.IsNotNull(redirectRoute);
				Assert.AreEqual("setting", redirectRoute.RouteValues["action"]);
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
			ProcessController controller = new ProcessController();
			testControllerBuilder.InitializeController(controller);
			Process process = new Process();
			PMSEntities db = new PMSEntities();
			Process editprocess = db.Processes.First();
			editprocess.Name = "Process UnitTest";
			editprocess.Description = "This is process unittest";

			using (TransactionScope scope = new TransactionScope())
			{
				ViewResult results = controller.EditProcess(4369) as ViewResult;
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
			ProcessController controller = new ProcessController();
			testControllerBuilder.InitializeController(controller);
			Process process = new Process();
			PMSEntities db = new PMSEntities();
			Process editprocess = db.Processes.First();
			editprocess.DataJson = "";
			editprocess.IsRun = false;

			using (TransactionScope scope = new TransactionScope())
			{
				ViewResult results = controller.EditProcess(4369) as ViewResult;
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
			string userName = "tovo1@vanlanguni.vn";
			Mock<Group> mockGroup = new Mock<Group>();
			//arrange
			//get user id
			Mock<ControllerContext> controllerContext = new Mock<ControllerContext>();
			Mock<IPrincipal> principal = new Moq.Mock<IPrincipal>();
			principal.Setup(p => p.IsInRole("owner")).Returns(true);
			principal.SetupGet(x => x.Identity.Name).Returns(userName);
			controllerContext.SetupGet(x => x.HttpContext.User).Returns(principal.Object);
			controllerUnderTest.ControllerContext = controllerContext.Object;

			ViewResult result = controllerUnderTest.EditProcess(3366) as ViewResult;
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
			string userName = "tovo1@vanlanguni.vn";
			Mock<Group> mockGroup = new Mock<Group>();
			//arrange
			//get user id
			Mock<ControllerContext> controllerContext = new Mock<ControllerContext>();
			Mock<IPrincipal> principal = new Moq.Mock<IPrincipal>();
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
			string userName = "tovo1@vanlanguni.vn";
			Mock<Group> mockGroup = new Mock<Group>();
			//arrange
			//get user id
			Mock<ControllerContext> controllerContext = new Mock<ControllerContext>();
			Mock<IPrincipal> principal = new Moq.Mock<IPrincipal>();
			principal.Setup(p => p.IsInRole("owner")).Returns(true);
			principal.SetupGet(x => x.Identity.Name).Returns(userName);
			controllerContext.SetupGet(x => x.HttpContext.User).Returns(principal.Object);
			controllerUnderTest.ControllerContext = controllerContext.Object;

			RedirectToRouteResult redirectRoute = controllerUnderTest.Draw(3365) as RedirectToRouteResult;
			//assert
			Assert.IsNotNull(redirectRoute);
			Assert.AreEqual("editprocess", redirectRoute.RouteValues["action"]);
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
			string userName = "tovo1@vanlanguni.vn";
			Mock<Group> mockGroup = new Mock<Group>();
			//arrange
			//get user id
			Mock<ControllerContext> controllerContext = new Mock<ControllerContext>();
			Mock<IPrincipal> principal = new Moq.Mock<IPrincipal>();
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
			ProcessController controller = new ProcessController();
			testControllerBuilder.InitializeController(controller);
			PMSEntities db = new PMSEntities();

			ViewResult result1 = controller.DeleteRole(db.Roles.First().Id) as ViewResult;
			Assert.IsNotNull(result1);

			int id = 1079;

			ActionResult actual;
			TaskProcess delete = db.TaskProcesses.Find(id);
			//Role deleteRole = db.Roles.Find(id);
			actual = controller.DeleteRole(id);
			Assert.IsTrue(db.TaskProcesses.Contains(delete));
			//Assert.IsTrue(db.Roles.Contains(deleteRole));
		}
		/// <summary>
		/// Purpose of TC: 
		/// - Hanle Delete Group		
		/// </summary>
		[TestMethod]
		public void DeleteGroup_WithValidModel_ExpectValidNavigation()
		{
			TestControllerBuilder testControllerBuilder = new TestControllerBuilder();
			Areas.API.Controllers.GroupController controller = new Areas.API.Controllers.GroupController();
			testControllerBuilder.InitializeController(controller);
			PMSEntities db = new PMSEntities();

			JsonResult result1 = controller.DeleteGroup(13) as JsonResult;
			//Assert.IsNotNull(result1);

			int id = 1079;

			ActionResult actual;
			Group delete = db.Groups.Find(id);
			//Role deleteRole = db.Roles.Find(id);
			actual = controller.DeleteGroup(id);
			Assert.IsTrue(db.Groups.Contains(delete));
		}
		/// <summary>
		/// Purpose of TC: 
		/// - Hanle Delete Process	
		/// </summary>
		[TestMethod]
		public void DeleteProcess_WithValidModel_ExpectValidNavigation()
		{
			TestControllerBuilder testControllerBuilder = new TestControllerBuilder();
			Areas.API.Controllers.ProcessController controller = new Areas.API.Controllers.ProcessController();
			testControllerBuilder.InitializeController(controller);
			PMSEntities db = new PMSEntities();

			JsonResult result1 = controller.DeleteProcess(13) as JsonResult;
			//Assert.IsNotNull(result1);

			int id = 1079;

			ActionResult actual;
			Group delete = db.Groups.Find(id);
			//Role deleteRole = db.Roles.Find(id);
			actual = controller.DeleteProcess(id);
			Assert.IsTrue(db.Groups.Contains(delete));
		}
		/// <summary>
		/// Purpose of TC: 
		/// - Hanle Delete Process Run
		/// </summary>
		[TestMethod]
		public void DeleteProcessRun_WithValidModel_ExpectValidNavigation()
		{
			TestControllerBuilder testControllerBuilder = new TestControllerBuilder();
			Areas.API.Controllers.ProcessRunController controller = new Areas.API.Controllers.ProcessRunController();
			testControllerBuilder.InitializeController(controller);
			PMSEntities db = new PMSEntities();

			JsonResult result1 = controller.DeleteProcessRun(13) as JsonResult;
			//Assert.IsNotNull(result1);

			int id = 1079;

			ActionResult actual;
			Group delete = db.Groups.Find(id);
			//Role deleteRole = db.Roles.Find(id);
			actual = controller.DeleteProcessRun(id);
			Assert.IsTrue(db.Groups.Contains(delete));
		}
		/// <summary>
		/// Purpose of TC: 
		/// - Hanle Delete Task	
		/// </summary>
		[TestMethod]
		public void DeleteTask_WithValidModel_ExpectValidNavigation()
		{
			TestControllerBuilder testControllerBuilder = new TestControllerBuilder();
			string userName = "tovo1@vanlanguni.vn";
			GenericIdentity identity = new GenericIdentity("tovo1@vanlanguni.vn");
			Mock<HttpRequestBase> moqRequest = new Mock<HttpRequestBase>();
			Mock<HttpResponseBase> moqResponse = new Mock<HttpResponseBase>();
			Mock<HttpServerUtilityBase> moqServer = new Mock<HttpServerUtilityBase>();
			Mock<IPrincipal> moqUser = new Mock<IPrincipal>();
			Mock<IIdentity> moqIdentity = new Mock<IIdentity>();
			Mock<UrlHelper> moqUrlHelper = new Mock<UrlHelper>();
			Mock<ControllerContext> controllerContext = new Mock<ControllerContext>();
			Mock<HttpSessionStateBase> session = new Mock<HttpSessionStateBase>();
			Mock<HttpContextBase> mockHttpContext = new Mock<HttpContextBase>();
			//Setup a fake HttpRequest
			Mock<HttpPostedFileBase> moqPostedFile = new Mock<HttpPostedFileBase>();
			Mock<IPrincipal> principal = new Mock<IPrincipal>();
			principal.Setup(p => p.IsInRole("owner")).Returns(true);
			principal.SetupGet(x => x.Identity.Name).Returns("tovo1@vanlanguni.vn");
			principal.Setup(x => x.Identity).Returns(identity);
			principal.Setup(x => x.IsInRole(It.IsAny<string>())).Returns(true);

			//get user id
			principal.Setup(p => p.IsInRole("IdOWwner")).Returns(true);
			principal.SetupGet(x => x.Identity.Name).Returns(userName);
			controllerContext.SetupGet(x => x.HttpContext.User).Returns(principal.Object);

			ProcessController controller = new ProcessController();
			testControllerBuilder.InitializeController(controller);
			controller.ControllerContext = controllerContext.Object;
			PMSEntities db = new PMSEntities();
			ViewResult result1 = controller.DeleteRole(db.TaskProcesses.First().Id) as ViewResult;
			Assert.IsNull(result1);

			int id = 2498;

			ActionResult actual;
			TaskProcess delete = db.TaskProcesses.Find(id);
			actual = controller.DeleteTask(id);
			//Assert.IsFalse(db.TaskProcesses.Contains(delete));
		}
		/// <summary>
		/// Purpose of TC: 
		/// - Return FormTask View Available	
		/// </summary>
		[TestMethod]
		public void ReturnFormTaskView_Available()
		{
			TestControllerBuilder testControllerBuilder = new TestControllerBuilder();
			ProcessController controller = new ProcessController();
			testControllerBuilder.InitializeController(controller);
			//user need get
			string userName = "tovo1@vanlanguni.vn";
			Mock<ControllerContext> controllerContext = new Mock<ControllerContext>();
			Mock<HttpSessionStateBase> session = new Mock<HttpSessionStateBase>();
			Mock<HttpContextBase> mockHttpContext = new Mock<HttpContextBase>();

			Mock<IPrincipal> principal = new Mock<IPrincipal>();
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
				Id = 5037,
				IdRole = 2312,
			});
			controllerContext.SetupGet(x => x.HttpContext.User).Returns(principal.Object);
			controller.ControllerContext = controllerContext.Object;
			// action
			ViewResult result = controller.ShowFormTask(5037) as ViewResult;

			// assert
			Assert.IsNotNull(result);
		}
		/// <summary>
		/// Purpose of TC: 
		/// - Return view FileManager
		/// </summary>
		[TestMethod]
		public void FileManager1_WithValidModel_ExpectValidNavigation()
		{
			TestControllerBuilder testControllerBuilder = new TestControllerBuilder();
			GroupController controller = new GroupController();
			testControllerBuilder.InitializeController(controller);
			int IdGroup = 6136;

			ViewResult results = controller.FileManager(IdGroup) as ViewResult;
			//assert
			Assert.IsNotNull(results);
		}
		/// <summary>
		/// Purpose of TC: 
		/// - Test MultipleLanguage
		/// </summary>
		[TestMethod]
		public void TestMultipleLanguage()
		{
			// Arrange
			GlobalFilterCollection coll = new GlobalFilterCollection();
			// Act
			FilterConfig.RegisterGlobalFilters(coll);
			bool authorized = coll.Any(x => x.Instance is HandleErrorAttribute);
			//Assert
			Assert.IsTrue(authorized);
		}
		/// <summary>
		/// Purpose of TC: 
		/// - Test AssignRole
		/// </summary>
		[TestMethod]
		public void AssignRole_WithValidModel_ExpectValidNavigation()
		{
			TestControllerBuilder testControllerBuilder = new TestControllerBuilder();
			string userName = "tovo1@vanlanguni.vn";
			int processid = 3366;
			GenericIdentity identity = new GenericIdentity("tovo1@vanlanguni.vn");
			Mock<HttpRequestBase> moqRequest = new Mock<HttpRequestBase>();
			Mock<HttpResponseBase> moqResponse = new Mock<HttpResponseBase>();
			Mock<HttpServerUtilityBase> moqServer = new Mock<HttpServerUtilityBase>();
			Mock<IPrincipal> moqUser = new Mock<IPrincipal>();
			Mock<IIdentity> moqIdentity = new Mock<IIdentity>();
			Mock<UrlHelper> moqUrlHelper = new Mock<UrlHelper>();
			Mock<ControllerContext> controllerContext = new Mock<ControllerContext>();
			Mock<HttpSessionStateBase> session = new Mock<HttpSessionStateBase>();
			Mock<HttpContextBase> mockHttpContext = new Mock<HttpContextBase>();
			//Setup a fake HttpRequest
			Mock<HttpPostedFileBase> moqPostedFile = new Mock<HttpPostedFileBase>();
			Mock<IPrincipal> principal = new Mock<IPrincipal>();
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
			ProcessRunController controller = new ProcessRunController();
			controller.ControllerContext = controllerContext.Object;
			testControllerBuilder.InitializeController(controller);
			PMSEntities db = new PMSEntities();
			RoleRun rolerun = new RoleRun();
			rolerun.Id = db.RoleRuns.First().Id;
			rolerun.IdUser = "0e200bd0-826a-4ec6-b221-20760f817d8e";
			rolerun.IdRole = 3299;
			//get user id
			principal.Setup(p => p.IsInRole("IdOWwner")).Returns(true);
			principal.SetupGet(x => x.Identity.Name).Returns(userName);
			controllerContext.SetupGet(x => x.HttpContext.User).Returns(principal.Object);
			controller.ControllerContext = controllerContext.Object;
			//act
			ViewResult results = controller.AssignRole(processid) as ViewResult;
			//assert
			Assert.IsNotNull(results);
		}
		/// <summary>
		/// Purpose of TC: 
		/// - Create Role
		/// </summary>
		[TestMethod]
		public void CreateComment_WithValidModel_ExpectValidNavigation()
		{
			TestControllerBuilder testControllerBuilder = new TestControllerBuilder();
			string userName = "tovo1@vanlanguni.vn";
			//int processid = 137;
			GenericIdentity identity = new GenericIdentity("tovo1@vanlanguni.vn");
			Mock<HttpRequestBase> moqRequest = new Mock<HttpRequestBase>();
			Mock<HttpResponseBase> moqResponse = new Mock<HttpResponseBase>();
			Mock<HttpServerUtilityBase> moqServer = new Mock<HttpServerUtilityBase>();
			Mock<IPrincipal> moqUser = new Mock<IPrincipal>();
			Mock<IIdentity> moqIdentity = new Mock<IIdentity>();
			Mock<UrlHelper> moqUrlHelper = new Mock<UrlHelper>();
			Mock<ControllerContext> controllerContext = new Mock<ControllerContext>();
			Mock<HttpSessionStateBase> session = new Mock<HttpSessionStateBase>();
			Mock<HttpContextBase> mockHttpContext = new Mock<HttpContextBase>();
			//Setup a fake HttpRequest
			Mock<HttpPostedFileBase> moqPostedFile = new Mock<HttpPostedFileBase>();
			Mock<IPrincipal> principal = new Mock<IPrincipal>();
			principal.Setup(p => p.IsInRole("IdOwner")).Returns(true);
			principal.SetupGet(x => x.Identity.Name).Returns("tovo1@vanlanguni.vn");
			principal.Setup(x => x.Identity).Returns(identity);
			principal.Setup(x => x.IsInRole(It.IsAny<string>())).Returns(true);

			controllerContext.SetupGet(x => x.HttpContext.User).Returns(principal.Object);
			moqRequest.Setup(r => r.Files.Count).Returns(0);
			mockHttpContext.Setup(x => x.Request).Returns(moqRequest.Object);
			//principal.Setup(p => p.IsInRole("Manager")).Returns(true);
			controllerContext.SetupGet(p => p.HttpContext.Request.IsAuthenticated).Returns(true);
			moqIdentity.Setup(id => id.IsAuthenticated).Returns(true);
			//get session
			mockHttpContext.Setup(ctx => ctx.Session).Returns(session.Object);
			controllerContext.Setup(ctx => ctx.HttpContext).Returns(mockHttpContext.Object);
			controllerContext.Setup(p => p.HttpContext.Session["IdUser"]).Returns(userName);
			//arrange
			HttpContext.Current = FakeHttpContext();
			Areas.API.Controllers.ProcessRunController areasController = new Areas.API.Controllers.ProcessRunController();
			areasController.ControllerContext = controllerContext.Object;
			testControllerBuilder.InitializeController(areasController);
			PMSEntities db = new PMSEntities();
			Comment comment = new Comment();
			Direction direction = new Direction();
			direction.ToString("G");
			//get user id
			principal.Setup(p => p.IsInRole("IdOWwner")).Returns(true);
			principal.SetupGet(x => x.Identity.Name).Returns(userName);
			controllerContext.SetupGet(x => x.HttpContext.User).Returns(principal.Object);
			areasController.ControllerContext = controllerContext.Object;

			//act
			JsonResult actual = areasController.AddComment(1052, direction, "Comment UnitTest") as JsonResult;
			//assert
			Assert.IsNotNull(actual);
		}
		/// <summary>
		/// Purpose of TC: 
		/// - Create Role
		/// </summary>
		[TestMethod]
		public void MyTask_WithValidModel_ExpectValidNavigation()
		{
			TestControllerBuilder testControllerBuilder = new TestControllerBuilder();
			string userName = "tovo1@vanlanguni.vn";
			int groupid = 6133;
			GenericIdentity identity = new GenericIdentity("tovo1@vanlanguni.vn");
			Mock<HttpRequestBase> moqRequest = new Mock<HttpRequestBase>();
			Mock<HttpResponseBase> moqResponse = new Mock<HttpResponseBase>();
			Mock<HttpServerUtilityBase> moqServer = new Mock<HttpServerUtilityBase>();
			Mock<IPrincipal> moqUser = new Mock<IPrincipal>();
			Mock<IIdentity> moqIdentity = new Mock<IIdentity>();
			Mock<UrlHelper> moqUrlHelper = new Mock<UrlHelper>();
			Mock<ControllerContext> controllerContext = new Mock<ControllerContext>();
			Mock<HttpSessionStateBase> session = new Mock<HttpSessionStateBase>();
			Mock<HttpContextBase> mockHttpContext = new Mock<HttpContextBase>();
			//Setup a fake HttpRequest
			Mock<HttpPostedFileBase> moqPostedFile = new Mock<HttpPostedFileBase>();
			Mock<IPrincipal> principal = new Mock<IPrincipal>();
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
			controllerContext.Setup(p => p.HttpContext.Session["processid"]).Returns(groupid);
			//arrange
			HttpContext.Current = FakeHttpContext();
			GroupController controller = new GroupController();
			controller.ControllerContext = controllerContext.Object;
			testControllerBuilder.InitializeController(controller);
			PMSEntities db = new PMSEntities();
			//get user id
			principal.Setup(p => p.IsInRole("IdOWwner")).Returns(true);
			principal.SetupGet(x => x.Identity.Name).Returns(userName);
			controllerContext.SetupGet(x => x.HttpContext.User).Returns(principal.Object);
			controller.ControllerContext = controllerContext.Object;

			//act
			ViewResult result = controller.MyTask(groupid) as ViewResult;
			//assert
			Assert.IsNotNull(result);
		}
	}
}
