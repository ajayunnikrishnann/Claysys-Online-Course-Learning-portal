using System;
using System.Web;
using System.Web.Mvc;

namespace Claysys_Online_Course_Learning_portal.Attributes
{
    public class AdminAuthorizeAttribute : AuthorizeAttribute
    {
        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            // If the user is not authenticated, redirect to the Signup page
            if (!filterContext.HttpContext.User.Identity.IsAuthenticated)
            {
                filterContext.Result = new RedirectResult("~/Admin/Signup");
            }
            else
            {
                base.HandleUnauthorizedRequest(filterContext);
            }
        }

        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            if (HttpContext.Current.Session["AdminID"] == null)
            {
                filterContext.Result = new RedirectResult("~/Admin/Signup");
            }
            else
            {
                base.OnAuthorization(filterContext);
            }
        }
    }
}
