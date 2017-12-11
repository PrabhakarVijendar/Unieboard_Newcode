using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using StructureMap;
using UniEBoard.Service.Interfaces.ApplicationService;

namespace UniEBoard.Security
{
    public class CustomAuthorizeAttribute : System.Web.Mvc.AuthorizeAttribute
    {
        /// <summary>
        /// When overridden, provides an entry point for custom authorization checks.
        /// </summary>
        /// <param name="httpContext">The HTTP context, which encapsulates all HTTP-specific information about an individual HTTP request.</param>
        /// <returns>
        /// true if the user is authorized; otherwise, false.
        /// </returns>
        /// <exception cref="T:System.ArgumentNullException">The <paramref name="httpContext"/> parameter is null.</exception>
        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            ISecurityAppService securityservice =ObjectFactory.GetInstance<ISecurityAppService>();
            IUserAppService userAppService = ObjectFactory.GetInstance<IUserAppService>();
            var user = userAppService.GetUserByUserName(httpContext.User.Identity.Name);
            if (!string.IsNullOrEmpty(Roles))
            {
                foreach (string role in this.Roles.Split(','))
                {
                    foreach (var roleViewModel in user.Roles)
                    {
                        if (role.Trim().ToLower().Equals(roleViewModel.Title.Trim().ToLower()))
                        {
                            return true;
                        }
                    }
                }
            }
            
            return false;
        }

        protected override void HandleUnauthorizedRequest(System.Web.Mvc.AuthorizationContext filterContext)
        {
            filterContext.Result = new RedirectResult("/");
            base.HandleUnauthorizedRequest(filterContext);
        }
    }
}