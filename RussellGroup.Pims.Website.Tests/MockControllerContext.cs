using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace RussellGroup.Pims.Website.Tests
{
    public static class MockControllerContext
    {
        private static HttpContextBase GetFakeAuthenticatedHttpContext(string userName)
        {
            var context = new Mock<HttpContextBase>();
            var request = new Mock<HttpRequestBase>();
            var response = new Mock<HttpResponseBase>();
            var session = new Mock<HttpSessionStateBase>();
            var server = new Mock<HttpServerUtilityBase>();
            var user = new Mock<IPrincipal>();
            var identity = new Mock<IIdentity>();
            var cache = new Mock<HttpCachePolicyBase>();

            context.Setup(c => c.Request).Returns(request.Object);
            context.Setup(c => c.Response).Returns(response.Object);
            context.Setup(c => c.Session).Returns(session.Object);
            context.Setup(c => c.Server).Returns(server.Object);
            context.Setup(c => c.User).Returns(user.Object);

            user.Setup(u => u.Identity).Returns(identity.Object);

            identity.Setup(i => i.IsAuthenticated).Returns(true);
            identity.Setup(i => i.Name).Returns(userName);

            context.Setup(c => c.Response.Cache).Returns(cache.Object);
            context.Setup(c => c.Items).Returns(new Dictionary<object, object>());

            return context.Object;
        }

        public static void SetFakeAuthenticatedControllerContext(this Controller controller)
        {
            SetFakeAuthenticatedControllerContext(controller, "Tester");
        }

        public static void SetFakeAuthenticatedControllerContext(this Controller controller, string userName)
        {
            var httpContext = GetFakeAuthenticatedHttpContext(userName);
            var controllerContext = new ControllerContext(new RequestContext(httpContext, new RouteData()), controller);

            controller.ControllerContext = controllerContext;
            controller.Url = new UrlHelper(controllerContext.RequestContext);
        }
    }
}
