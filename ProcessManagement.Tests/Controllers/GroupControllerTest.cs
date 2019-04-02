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
		/// - Ruturn view Group when login and available	
		/// </summary>
		[TestMethod]
		public void ReturnViewGroup()
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
		[TestMethod]
		public void ReturnIndex_Group_UserPossess()
		{
			//arrange
			var controller = new GroupController();
			var userName = "tovo1@vanlanguni.vn";
			//get user id
			var controllerContext = new Mock<ControllerContext>();
			var principal = new Moq.Mock<IPrincipal>();
			principal.Setup(p => p.IsInRole("owner")).Returns(true);
			principal.SetupGet(x => x.Identity.Name).Returns(userName);
			controllerContext.SetupGet(x => x.HttpContext.User).Returns(principal.Object);
			controller.ControllerContext = controllerContext.Object;

			// action
			ViewResult result = controller.Index() as ViewResult;

			// assert
			Assert.IsNotNull(result);
		}
		/// <summary>
		/// Purpose of TC: 
		/// - Validate whether with valid input data, a group record is created and saved into database, 
		///     and then the user is redirected to the Index action
		/// </summary>
		[TestMethod]
		public void CreateGroup_WithValidModel_ExpectValidNavigation()
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
			//Arrange
			var controller = new GroupController();
			var group = new Group
			{
				Id = 1,
				IdOwner = "b0245219 - c69e - 428f - bfc7 - ead7192d5936", 
				Name = "Test Group Demo",
				Description = "test",
				ownerSlug = "Pet",
				groupSlug = "Pet_Group",
			};
			controller.ControllerContext = new ControllerContext(mockHttpContext.Object, new RouteData(), controller);
			var validationResults = TestModelHelper.ValidateModel(controller, group);
			//get user id
			var principal = new Moq.Mock<IPrincipal>();
			principal.Setup(p => p.IsInRole("owner")).Returns(true);
			principal.SetupGet(x => x.Identity.Name).Returns(userName);
			controllerContext.SetupGet(x => x.HttpContext.User).Returns(principal.Object);
			controller.ControllerContext = controllerContext.Object;

			//Act
			var redirectRoute = controller.NewGroup(group, httpPostedFile) as RedirectToRouteResult;

			//Assert
			Assert.IsNotNull(redirectRoute);
		}
		/// <summary>
		/// Purpose of TC: 
		/// - Create Role	
		/// </summary>
		[TestMethod]
		public void CreateRole_WithValidModel_ExpectValidNavigation()
		{
			var userName = "tovo1@vanlanguni.vn";
			var processid = 120;
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
			//controllerContext.SetupGet(x => x.HttpContext.User).Returns(principal.Object);
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
			controllerContext.Setup(p => p.HttpContext.Session["processid"]).Returns(new Role
			{
				IdProcess = 120,
			});
			//arrange
			HttpContext.Current = FakeHttpContext();
			//HttpContext.Current.Session["processId"] = "101";
			HttpContext.Current.Session.Add("processid", 120);
			var controller = new ProcessController();
			controller.ControllerContext = controllerContext.Object;
			var role = new Role()
			{
				Id = 99,
				IdProcess = 101,
				IsRun = false,
				Name = "Tester",
				Description = "Demo test is correct",
			};
			//get user id
			principal.Setup(p => p.IsInRole("owner")).Returns(true);
			principal.SetupGet(x => x.Identity.Name).Returns(userName);
			controllerContext.SetupGet(x => x.HttpContext.User).Returns(principal.Object);
			controller.ControllerContext = controllerContext.Object;
			//var validationResults = TestModelHelper.ValidateModel(controller, role);

			//act
			ViewResult result = controller.AddRole(role) as ViewResult;
			//assert
			Assert.IsNotNull(result);
			//Assert.AreEqual("",result.ViewName);
			//Assert.AreEqual("ShowStep", redirectRoute.RouteValues["action"]);
			//Assert.AreEqual(0, validationResults.Count);
			//Assert.AreEqual(controller.AddRole(role) ,processid);

		}
		/// <summary>
		/// Purpose of TC: 
		/// - Create Process	
		/// </summary>
		[TestMethod]
		public void CreateProcess_WithValidModel_ExpectValidNavigation()
		{
			var userName = "tovo1@vanlanguni.vn";
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
			HttpPostedFileBase httpPostedFile = Mock.Of<HttpPostedFileBase>();
			var mock = Mock.Get(httpPostedFile);
			mock.Setup(_ => _.FileName).Returns("fakeFileName.extension");
			var memoryStream = new MemoryStream();
			//...populate fake stream
			//setup mock to return stream
			mock.Setup(_ => _.InputStream).Returns(memoryStream);

			moqRequest.Setup(r => r.Files.Count).Returns(0);
			mockHttpContext.Setup(x => x.Request).Returns(moqRequest.Object);
			mockHttpContext.Setup(ctx => ctx.Session).Returns(session.Object);
			//get session
			mockHttpContext.Setup(ctx => ctx.Session).Returns(session.Object);
			controllerContext.Setup(ctx => ctx.HttpContext).Returns(mockHttpContext.Object);
			controllerContext.Setup(p => p.HttpContext.Session["idgroup"]).Returns(new Group
			{
				Id = 4104,
				
			});
			controllerContext.Setup(p => p.HttpContext.Session["Process"]).Returns(new Process
			{
				Id = 200,
				IdGroup = 4104,
			});
			//arrange 
			var controller = new ProcessController();
			var process = new Process()
			{
				Id = 100,
				IdOwner = "tovo1@vanlanguni.vn",
				IdGroup = 4103,
				Avatar = "image.png",
				DataJson = "test",
				Name = "Process Test",
				Description = "Demo test create process is correct"
			};
			//get user id
			var principal = new Moq.Mock<IPrincipal>();
			principal.Setup(p => p.IsInRole("owner")).Returns(true);
			principal.SetupGet(x => x.Identity.Name).Returns(userName);
			controllerContext.SetupGet(x => x.HttpContext.User).Returns(principal.Object);
			controller.ControllerContext = controllerContext.Object;

			controller.ControllerContext = new ControllerContext(mockHttpContext.Object, new RouteData(), controller);
			var validationResults = TestModelHelper.ValidateModel(controller, process);

			//act
			var redirectRoute = controller.NewProcess(process, httpPostedFile) as RedirectToRouteResult;


			//assert
			Assert.IsNotNull(redirectRoute);
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
			var redirectRoute = controller.AddTask(1220) as RedirectToRouteResult;

			//assert
			Assert.IsNull(redirectRoute);
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
			//get session
			mockHttpContext.Setup(ctx => ctx.Session).Returns(session.Object);
			mockControllerContext.Setup(ctx => ctx.HttpContext).Returns(mockHttpContext.Object);
			mockControllerContext.Setup(p => p.HttpContext.Session["Process"]).Returns(new Process
			{
				Id = 99,
				IdGroup = 4104
			});
			mockControllerContext.Setup(p => p.HttpContext.Session["IdGroup"]).Returns(new Group
			{
				Id = 4104
			});
			mockControllerContext.Setup(p => p.HttpContext.Session["IdGroup"]).Returns(new GroupService
			{

			});
			var controller = new ProcessController();
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

			ViewResult result = controllerUnderTest.EditRole(69) as ViewResult;
			//assert
			Assert.IsNull(result);
			System.Diagnostics.Trace.WriteLine("HTTP not found");
		}
		/// <summary>
		/// Purpose of TC: 
		/// - Hanle Edit Role when login		
		/// </summary>
		[TestMethod]
		public void EditRole_WithValidModel_ExpectValidNavigation()
		{
			//int id = 66;
			//string expectedView = "ShowStep";
			//var editrole = new Role()
			//{
			//	Id = 66,
			//	Name = "Role unittest"
			//};
			//_mockContactService.Setup(x => x.GetContact(id)).Returns(stubContact);
			//var expectedVm = new ContactViewModel()
			//{
			//	Id = stubContact.Id,
			//	FirstName = stubContact.FirstName,
			//	LastName = stubContact.LastName,
			//	Email = stubContact.Email
			//};

			//var actual = _controller.Edit(id) as PartialViewResult;
			//var actualVm = actual.Model as ContactViewModel;

			//Assert.AreEqual(expectedView, actual.ViewName);
			//Assert.AreEqual(expectedVm.Email, actualVm.Email);
			//Assert.AreEqual(expectedVm.FirstName, actualVm.FirstName);
			//Assert.AreEqual(expectedVm.Id, actualVm.Id);
			//Assert.AreEqual(expectedVm.LastName, actualVm.LastName);
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

			ViewResult result = controllerUnderTest.Draw(135) as ViewResult;
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
			//const int DeletedId = 123;
			//var mockRepository = new Mock<IRepository>();
			//var controller = new MyController(mockRepository.Object);
			//controller.Delete(DeletedId);

			//mockRepository.Verify(v => v.DeleteRestrictions(DeletedID), Times.Once());
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
		/// - Hanle Delete Step		
		/// </summary>
		[TestMethod]
		public void DeleteStep_WithValidModel_ExpectValidNavigation()
		{

		}
		public void CreateTask_UnSuccessfull()
		{
			//using (var scope = new TransactionScope())
			//{

			//	var result1 = controller.Edit(db.Groups.First().ID.ToString()) as ViewResult;
			//	Assert.IsNotNull(result1);

			//	System.Diagnostics.Trace.WriteLine("Error data format, please try again");

			//}
		}
	}
}
