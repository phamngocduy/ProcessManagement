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

namespace ProcessManagement.Tests.Controllers
{
	[TestClass]
	public class UnitTest1
	{
		GroupService groupService;
		/// <summary>
		/// Purpose of TC: 
		/// - Ruturn view Group when login		
		/// </summary>
		[TestMethod]
		public void ReturnViewGroup()
		{
			GroupController controllerUnderTest = new GroupController();
			var result = controllerUnderTest.Show(1) as ViewResult;
			Assert.AreEqual("Show", result.ViewName);
		}
		[TestMethod]
		public void ReturnIndex()
		{
			var fakeHttpContext = new Mock<HttpContextBase>();
			var fakeIdentity = new GenericIdentity("User");
			var principal = new GenericPrincipal(fakeIdentity, null);

			fakeHttpContext.Setup(t => t.User).Returns(principal);
			var controllerContext = new Mock<ControllerContext>();
			controllerContext.Setup(t => t.HttpContext).Returns(fakeHttpContext.Object);

			//var result = (ViewResult)_requestController.Index();
			//_requestController = new RequestController();

			//Set your controller ControllerContext with fake context
			//_requestController.ControllerContext = controllerContext.Object;
		}
		/// <summary>
		/// Purpose of TC: 
		/// - Validate whether with valid input data, a group record is created and saved into database, 
		///     and then the user is redirected to the Index action
		/// </summary>
		[TestMethod]
		public void ValidateCreateGroup_WithValidModel_ExpectValidNavigation()
		{
			Mock<HttpContextBase> moqContext = new Mock<HttpContextBase>();
			Mock<HttpRequestBase> moqRequest = new Mock<HttpRequestBase>();
			Mock<HttpPostedFileBase> moqPostedFile = new Mock<HttpPostedFileBase>();

			moqRequest.Setup(r => r.Files.Count).Returns(0);
			moqContext.Setup(x => x.Request).Returns(moqRequest.Object);
			
		

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
			//HttpPostedFileBase fileUpload = Server.MapPath(string.Format("~/App_Data/Files/group/{0}/intro", 1));

			controller.ControllerContext = new ControllerContext(moqContext.Object, new RouteData(), controller);
			var validationResults = TestModelHelper.ValidateModel(controller, group);

			//Act
			//var redirectRoute = controller.NewGroup(group) as RedirectToRouteResult;

			//Assert
			//Assert.IsNotNull(redirectRoute);
			//Assert.AreEqual("Index", redirectRoute.RouteValues["action"]);
			//Assert.AreEqual("Catalog", redirectRoute.RouteValues["controller"]);
			//Assert.AreEqual(0, validationResults.Count);
		}

		[TestMethod]
		public void Upload_Action_Should_Store_Files_In_The_App_Data_Folder()
		{
			// arrange
			var httpContextMock = new Mock<HttpContextBase>();
			var serverMock = new Mock<HttpServerUtilityBase>();
			serverMock.Setup(x => x.MapPath("~/app_data")).Returns(@"D:\IT\app_data");
			httpContextMock.Setup(x => x.Server).Returns(serverMock.Object);
			var sut = new HomeController();
			sut.ControllerContext = new ControllerContext(httpContextMock.Object, new RouteData(), sut);

			var file1Mock = new Mock<HttpPostedFileBase>();
			file1Mock.Setup(x => x.FileName).Returns("file1.pdf");
			var files = new[] { file1Mock.Object };

			// act
			//var actual = sut.UploadFile(files);

			// assert
			file1Mock.Verify(x => x.SaveAs(@"D:\IT\app_data\file1.pdf"));
			//file2Mock.Verify(x => x.SaveAs(@"c:\work\app_data\file2.doc"));
		}
		/// <summary>
		/// Purpose of TC: 
		/// - Ruturn view Group Detail		
		/// </summary>
		[TestMethod]
		public void ReturnGroupDetailView()
		{

		}
	}
}
