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
			//IMyMockDa mockDa = new MockDataAccess();
			//var service = new AuthenticationService(mockDa);

			//mockDa.AddUser("Name", "Password");

			//Assert.IsTrue(service.DoLogin("Name", "Password"));

			////Ensure data access layer was used
			//Assert.IsTrue(mockDa.GetUserFromDBWasCalled);
		}
		/// <summary>
		/// Purpose of TC: 
		/// - Ruturn view Group when login		
		/// </summary>
		[TestMethod]
		public void ReturnViewGroup()
		{
			var userid = "64e10037-6c10-4544-a853-a2952330bf8e";
			var identity = new GenericIdentity("tuanho10@vanlanguni.vn");
			var grController = new GroupController();
			var controllerContext = new Mock<ControllerContext>();
			var principal = new Mock<IPrincipal>();
			var mockGroup = new Mock<Group>();

			//arrange
			//principal.Setup(p => p.IsInRole("Member")).Returns(true);
			principal.SetupGet(x => x.Identity.Name).Returns("tuanho10@vanlanguni.vn");
			//principal.Setup(x => x.Identity.GetUserId).Returns("64e10037-6c10-4544-a853-a2952330bf8e");
			//mockGroup.SetupGet(x => x.IdOwner).Returns(userid);
			principal.Setup(x => x.Identity).Returns(identity);
			principal.Setup(x => x.IsInRole(It.IsAny<string>())).Returns(true);

			controllerContext.SetupGet(x => x.HttpContext.User).Returns(principal.Object);
			grController.ControllerContext = controllerContext.Object;
			GroupController controllerUnderTest = new GroupController();
			//var result = controllerUnderTest.Show(4103) as ViewResult;
			//assert
			//Assert.AreEqual("Show", result.ViewName);
		}
		[TestMethod]
		public void ReturnIndex()
		{
			var fakeHttpContext = new Mock<HttpContextBase>();
			var fakeIdentity = new GenericIdentity("tovo1@vanlanguni.vn");
			var principal = new GenericPrincipal(fakeIdentity, null);

			fakeHttpContext.Setup(t => t.User).Returns(principal);
			var controllerContext = new Mock<ControllerContext>();
			controllerContext.Setup(t => t.HttpContext).Returns(fakeHttpContext.Object);

			//var result = (ViewResult)_requestController.Index();
			//_requestController = new RequestController();

			//Set your controller ControllerContext with fake context
			//_requestController.ControllerContext = controllerContext.Object;
			//action
			//var result = accountController.Login(loginViewModel, url);

			//assert
			//Assert.IsNotNull(result);
		}
		/// <summary>
		/// Purpose of TC: 
		/// - Validate whether with valid input data, a group record is created and saved into database, 
		///     and then the user is redirected to the Index action
		/// </summary>
		[TestMethod]
		public void CreateGroup_WithValidModel_ExpectValidNavigation()
		{
			Mock<HttpContextBase> moqContext = new Mock<HttpContextBase>();
			Mock<HttpRequestBase> moqRequest = new Mock<HttpRequestBase>();
			Mock<HttpFileCollectionBase> moqFiles = new Mock<HttpFileCollectionBase>();
			Mock<HttpPostedFileBase> moqPostedFile = new Mock<HttpPostedFileBase>();
			Mock<ControllerContext> cc = new Mock<ControllerContext>();
			NewMethod(moqPostedFile);

			moqRequest.Setup(r => r.Files.Count).Returns(0);
			moqContext.Setup(x => x.Request).Returns(moqRequest.Object);
			//moqFiles.Setup(x => x.Count).Returns(1);

			//Arrange
			var controller = new GroupController();
			var group = new Group
			{
				Id = 1,
				Name = "Test Group Demo",
				Description = "test",
				ownerSlug = "Pet",
				groupSlug = "Pet_Group",
			};
			var image = "test.png";


			//HttpPostedFileBase fileUpload = Server.MapPath(string.Format("~/App_Data/Files/group/{0}/intro", 1));
			controller.ControllerContext = cc.Object;
			controller.ControllerContext = new ControllerContext(moqContext.Object, new RouteData(), controller);
			var validationResults = TestModelHelper.ValidateModel(controller, group);

			//Act
			//var redirectRoute = controller.NewGroup(group, image) as RedirectToRouteResult;

			//Assert
			//Assert.IsNotNull(redirectRoute);
			//Assert.IsInstanceOfType(redirectRoute, typeof(ContentResult));
			//Assert.AreEqual("Index", redirectRoute.RouteValues["action"]);
			//Assert.AreEqual("Home", redirectRoute.RouteValues["controller"]);
			//Assert.AreEqual(0, validationResults.Count);
		}

		private static void NewMethod(Mock<HttpPostedFileBase> moqPostedFile)
		{
			moqPostedFile.Expect(f => f.ContentLength).Returns(1);
			moqPostedFile.Expect(f => f.FileName).Returns("myFileName");
		}
		/// <summary>
		/// Purpose of TC: 
		/// - Create Role	
		/// </summary>
		[TestMethod]
		public void CreateRole_WithValidModel_ExpectValidNavigation()
		{
			var identity = new GenericIdentity("tovo1@vanlanguni.vn");
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
			var controllerContext = new Mock<ControllerContext>();
			var principal = new Mock<IPrincipal>();
			//principal.Setup(p => p.IsInRole("Manager")).Returns(true);
			//principal.SetupGet(x => x.Identity.Name).Returns("tovo1@vanlanguni.vn");
			//controllerContext.SetupGet(x => x.HttpContext.User).Returns(principal.Object);
			principal.SetupGet(x => x.Identity.Name).Returns("tovo1@vanlanguni.vn");
			principal.Setup(x => x.Identity).Returns(identity);
			principal.Setup(x => x.IsInRole(It.IsAny<string>())).Returns(true);

			controllerContext.SetupGet(x => x.HttpContext.User).Returns(principal.Object);
			moqRequest.Setup(r => r.Files.Count).Returns(0);
			moqContext.Setup(x => x.Request).Returns(moqRequest.Object);
			principal.Setup(p => p.IsInRole("Manager")).Returns(true);
			principal.SetupGet(x => x.Identity.Name).Returns("tovo1@vanlanguni.vn");
			//moqIdentity.Setup(id => id.IsAuthenticated).Returns(true);
			//moqIdentity.Setup(id => id.Name).Returns("99");
			//arrange
			HttpContext.Current = FakeHttpContext();
			HttpContext.Current.Session["processId"] = "101";
			//HttpContext.Current.Session.Add("processid", 101);
			var controller = new ProcessController();
			controller.ControllerContext = controllerContext.Object;
			var role = new Role()
			{
				Id = 99,
				IdProcess = 101,
				IsRun = false,
				Name = "Tester",
				Description = "Demo test is correct"
			};

			controller.ControllerContext = new ControllerContext(moqContext.Object, new RouteData(), controller);
			//var validationResults = TestModelHelper.ValidateModel(controller, role);

			//act
			//var redirectRoute = controller.AddRole(role) as RedirectToRouteResult;

			//assert
			//Assert.IsNotNull(redirectRoute);
			//Assert.AreEqual("ShowStep", redirectRoute.RouteValues["action"]);
			//Assert.AreEqual(0, validationResults.Count);
			//Assert.AreEqual(controller.AddRole(role), identity.Name);

		}
		/// <summary>
		/// Purpose of TC: 
		/// - Create Process	
		/// </summary>
		[TestMethod]
		public void CreateProcess_WithValidModel_ExpectValidNavigation()
		{
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

			//moqIdentity.Setup(id => id.IsAuthenticated).Returns(true);
			//moqIdentity.Setup(id => id.Name).Returns("99");
			//arrange 
			var controller = new ProcessController();
			var process = new Process()
			{
				Id = 100,
				IdOwner = "tovo1@vanlanguni.vn",
				IdGroup = 4103,
				Avatar = "image.png",
				DataJson = "",
				Name = "Process Test",
				Description = "Demo test create process is correct"
			};

			controller.ControllerContext = new ControllerContext(moqContext.Object, new RouteData(), controller);
			var validationResults = TestModelHelper.ValidateModel(controller, process);

			//act
			//var redirectRoute = controller.NewProcess(process) as RedirectToRouteResult;

			//assert
			//Assert.IsNotNull(redirectRoute);
			//Assert.AreEqual("ShowStep", redirectRoute.RouteValues["action"]);
			//Assert.AreEqual(0, validationResults.Count);
		}
		/// <summary>
		/// Purpose of TC: 
		/// - Create Task	
		/// </summary>
		[TestMethod]
		public void CreateTask_WithValidModel_ExpectValidNavigation()
		{
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

			//moqIdentity.Setup(id => id.IsAuthenticated).Returns(true);
			//moqIdentity.Setup(id => id.Name).Returns("99");
			//arrange 
			var controller = new ProcessController();
			var taskProcess = new TaskProcess()
			{
				Id = 1190,
				IdStep = 273,
				Name = "Task Test",
				Description = "Demo test create task is correct"
			};

			controller.ControllerContext = new ControllerContext(moqContext.Object, new RouteData(), controller);
			var validationResults = TestModelHelper.ValidateModel(controller, taskProcess);

			//act
			//var redirectRoute = controller.AddTask(taskProcess) as RedirectToRouteResult;

			//assert
			//Assert.IsNotNull(redirectRoute);
			//Assert.AreEqual("ShowStep", redirectRoute.RouteValues["action"]);
			//Assert.AreEqual(0, validationResults.Count);
		}
		[TestMethod]
		public void ReturnTaskView()
		{
			//var mockUserStore = new Mock<IUserStore<ApplicationUser>>();
			//var userManager = new UserManager<ApplicationUser>(mockUserStore.Object);
			//var objectUnderTest = new SweetService(userManager);

			//ProcessController processController = new ProcessController();
			//ActionResult result = processController.ShowTask(1164);
			//Assert.IsInstanceOfType(result, typeof(ViewResult));

			var identity = new GenericIdentity("tovo1@vanlanguni.vn");
			var controller = new ProcessController();
			

			var controllerContext = new Mock<ControllerContext>();
			var principal = new Mock<IPrincipal>();
			//principal.Setup(p => p.IsInRole("Member")).Returns(true);
			principal.SetupGet(x => x.Identity.Name).Returns("tovo1@vanlanguni.vn");
			principal.Setup(x => x.Identity).Returns(identity);
			principal.Setup(x => x.IsInRole(It.IsAny<string>())).Returns(true);

			controllerContext.SetupGet(x => x.HttpContext.User).Returns(principal.Object);
			controller.ControllerContext = controllerContext.Object;

			//Assert.AreEqual(controller.ShowTask(1164), identity.Name);
		}

		//[TestMethod]
		//      public void TestMultipleLanguage()
		//      {
		//          // Arrange
		//          HttpContext.Current = new HttpContext(new HttpRequest(null, "http://localhost:54325", null), new HttpResponse(null));
		//          LocalizedControllerActivator IControllerActivator = new LocalizedControllerActivator();

		//          var requestContext = HttpContext.Current.Request.RequestContext;
		//          var controllerType = HttpContext.Current.Request.RequestType.GetType();
		//          // Act
		//          IController result = IControllerActivator.Create(requestContext,controllerType) as IController;
		//          //Assert
		//          Assert.IsNull(result);
		//      }
		[TestMethod]
		public void SelectProcess_Test()
		{
			var IdProcess = 99;
			//var idUser = "64e10037 - 6c10 - 4544 - a853 - a2952330bf8e";

			var mockControllerContext = new Mock<ControllerContext>();
			var mockSession = new Mock<HttpSessionStateBase>();
			
			mockSession.SetupGet(s => s["Id"]).Returns("288"); //somevalue
			mockControllerContext.Setup(p => p.HttpContext.Session).Returns(mockSession.Object);

			var controller = new ProcessController();
			controller.ControllerContext = mockControllerContext.Object;

			//Act         
			var actual = controller.ShowStep(IdProcess);
		}
	}
}
