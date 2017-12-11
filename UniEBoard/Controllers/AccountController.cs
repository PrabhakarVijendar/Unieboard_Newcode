// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AccountController.cs" company="Cognite Ltd">
//   Copyright (c) 2013 Cognite Ltd (http://www.cognite.co.uk)
// </copyright>
// <summary>
//  Account Controller Methods
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using DotNetOpenAuth.AspNet;
using Microsoft.Web.WebPages.OAuth;
//using UniEBoard.Repository;
using WebMatrix.WebData;
using UniEBoard.Filters;
using UniEBoard.Models;
using UniEBoard.Service.Interfaces.ApplicationService;
using UniEBoard.Model.Enums;
using UniEBoard.Service.Models;
using UniEBoard.Service.ApplicationServices;
using System.Text;
using UniEBoard.Service.Helpers.Configuration;
using System.Net.Mail;

using cog = Cognite.MembershipProvider;

namespace UniEBoard.Controllers
{
    /// <summary>
    /// The Account Controller
    /// </summary>
    [Authorize]
    [InitializeSimpleMembership]
    public class AccountController : BaseController
    {
        #region Members

        /// <summary>
        /// The Student Service Instance
        /// </summary>
        private IStudentAppService _studentService;

        /// <summary>
        /// User App Service
        /// </summary>
        private IUserAppService _userService;

        /// <summary>
        /// The User Service Instance
        /// </summary>
        private ITypeAppService _typeService;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="AccountController"/> class.
        /// </summary>
        /// <param name="studentService">The student service.</param>
        /// <param name="typeService">The type service.</param>
        public AccountController(
            IStudentAppService studentService, 
            ITypeAppService typeService, 
            IUserAppService userAppService) 
            : base(userAppService)
        {
            this._studentService = studentService;
            this._typeService = typeService;
            this._userService = userAppService;
        }

        #endregion

        #region Methods

        /// <summary>
        /// GET: /Account/Login - Logins this instance.
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        [ActionName("Login")]
        public ActionResult Login(string returnUrl)
        {
            if (Request.IsAuthenticated)
            {
                UserViewModel user = _userService.GetUserByUserName(cog.WebSecurity.CurrentUserName);
                if (user != null && user is StudentViewModel) { return RedirectToAction("Index", "Student"); }
                if (user != null && user is StaffViewModel) { return RedirectToAction("Index", "Teacher"); }
                return RedirectToAction("Index", "Student");
            }
            ViewBag.ReturnUrl = returnUrl;
            ViewBag.LoginTypes = _typeService.GetAllLoginTypes();
            return View();
        }

        /// <summary>
        /// Logins the specified model.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns></returns>
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Login(UserLoginViewModel model, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                // by default a user is always set to teacher.
                //model.UserType = (int)LoginType.Teacher;
                UserViewModel user = _userService.GetUserByUserName(model.UserName);
                model.UserType = user is StudentViewModel ? (int)LoginType.Student : (int)LoginType.Teacher;
                if ((user != null && !user.AccountDisabled) || model.UserType == (int)LoginType.Teacher)
                {
                    if (cog.WebSecurity.Login(model.UserName, model.Password, persistCookie: model.RememberMe))
                    {
                        switch (model.UserType.Value)
                        {
                            case (int)LoginType.Teacher:
                                return !string.IsNullOrEmpty(returnUrl) ? RedirectToLocal(returnUrl) : RedirectToAction("Index", "Teacher");
                            case (int)LoginType.Student:
                                return !string.IsNullOrEmpty(returnUrl) ? RedirectToLocal(returnUrl) : RedirectToAction("Index", "Student");
                        }
                    }
                    else
                    {
                        // If we got this far, something failed, redisplay form
                        ModelState.AddModelError("", "We could not log you in. Please check the user type, user name and password provided is correct.");
                    }
                }
                else
                {
                    ModelState.AddModelError("", "We could not log you in. Your account is not active. Please contact Administrator.");
                }
            }
            ViewBag.LoginTypes = _typeService.GetAllLoginTypes();
            return View(model);          
        }

        /// <summary>
        /// Logs the off.
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            RemoveOnlineUsers(CurrentUser);
            cog.WebSecurity.Logout();
            return RedirectToLocal(Url.Action("Index", "Home"));
        }

        /// <summary>
        /// Registers this instance.
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        public ActionResult Register()
        {
            if (Request.IsAuthenticated)
            {
                return RedirectToAction("Index", "Student");
            }
            ViewBag.GenderTypes = _typeService.GetAllGenderTypes();
            return View();
        }

        /// <summary>
        /// Registers the specified model.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns></returns>
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Register(StudentViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Attempt to register the user
                try
                {
                    cog.WebSecurity.CreateUserAndAccount(model.UserName, model.Password);
                    if (cog.WebSecurity.Login(model.UserName, model.Password))
                    {
                        var currentuserid = cog.WebSecurity.GetUserId(model.UserName);
                        if (_studentService.CreateStudentUser(currentuserid, model))
                        {
                            //Send email to register user.
                            SendRegisterEmail(model);
                        }
                    }
                    return RedirectToAction("Index", "Student");
                }
                catch (MembershipCreateUserException e)
                {
                    ModelState.AddModelError("", ErrorCodeToString(e.StatusCode));
                }
            }

            // If we got this far, something failed, redisplay form
            ViewBag.GenderTypes = _typeService.GetAllGenderTypes();
            return View(model);
        }

        protected string SendRegisterEmail(StudentViewModel model)
        {
            string email = model.Email.Trim();
            string url = "http://" + Request.Url.Authority + "/Account/Login";

            StringBuilder sb = new StringBuilder();
            sb.Append("<table><tr><td>Hello " + model.FirstName + " " + model.LastName + ",</td>");
            sb.Append("<br /><br />");
            sb.Append("<tr><td>You can login using below credentials:</td></tr><br />");

            sb.Append("<tr><td>Username: " + model.UserName + "</td></tr><br />");
            sb.Append("<tr><td>Password: " + model.Password + "</td></tr><br />");

            sb.Append("<tr><td><a href='" + url + "'>Click here</a> to Login to UniEBoard system.</td></tr><br />");

            sb.Append("<tr><td>Regards, UniEBoard </td></tr></table>");

            string bodyMessage = sb.ToString();

            var fromEmail = WebSite.Current.EmailSenderEmailAddress;
            var displayName = "";
            var subject = WebSite.Current.SubjectRegister.Replace("UserName", model.UserName);
            var smtpUserName = WebSite.Current.SMTPUserName;
            var smtpPassword = WebSite.Current.SMTPPassword;
            var smtpServer = WebSite.Current.SMTPServer;
            var smtpPort = WebSite.Current.SMTPPort;
            var cc = string.Empty;
            var bcc = string.Empty;
            var sslEnabled = WebSite.Current.SMTPEnableSSL;

            SmtpClient client = UniEBoard.Helpers.Email.EmailHelper.GetSmtpClient(smtpServer, smtpPort, smtpUserName, smtpPassword, sslEnabled);
            MailMessage message = UniEBoard.Helpers.Email.EmailHelper.GetMailMessage(fromEmail, displayName, email, subject, bodyMessage, cc, bcc);
            var emailSent = UniEBoard.Helpers.Email.EmailHelper.SendEmail(client, message);

            if (emailSent)
            {
                return "Email has been sent successfully to your email Id. You can reset your password.";
            }
            else
            {
                return "There was an error during sending email. Please contact the administrator";
            }
        }

        /// <summary>
        /// POST: /Account/Disassociate - Disassociates the specified provider.
        /// </summary>
        /// <param name="provider">The provider.</param>
        /// <param name="providerUserId">The provider user id.</param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Disassociate(string provider, string providerUserId)
        {
            string ownerAccount = OAuthWebSecurity.GetUserName(provider, providerUserId);
            ManageMessageId? message = null;

            // Only disassociate the account if the currently logged in user is the owner
            if (ownerAccount == User.Identity.Name)
            {
                // Use a transaction to prevent the user from deleting their last login credential
                using (var scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Serializable }))
                {
                    bool hasLocalAccount = OAuthWebSecurity.HasLocalAccount(cog.WebSecurity.GetUserId(User.Identity.Name));
                    if (hasLocalAccount || OAuthWebSecurity.GetAccountsFromUserName(User.Identity.Name).Count > 1)
                    {
                        OAuthWebSecurity.DeleteAccount(provider, providerUserId);
                        scope.Complete();
                        message = ManageMessageId.RemoveLoginSuccess;
                    }
                }
            }

            return RedirectToAction("Manage", new { Message = message });
        }

        /// <summary>
        /// Manages the specified message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <returns></returns>
        public ActionResult Manage(ManageMessageId? message)
        {
            ViewBag.StatusMessage =
                message == ManageMessageId.ChangePasswordSuccess ? "Your password has been changed."
                : message == ManageMessageId.SetPasswordSuccess ? "Your password has been set."
                : message == ManageMessageId.RemoveLoginSuccess ? "The external login was removed."
                : "";
            ViewBag.HasLocalPassword = OAuthWebSecurity.HasLocalAccount(cog.WebSecurity.GetUserId(User.Identity.Name));
            ViewBag.ReturnUrl = Url.Action("Manage");
            return View();
        }

        // POST: /Account/Manage
        /// <summary>
        /// Manages the specified model.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Manage(LocalPasswordModel model)
        {
            bool hasLocalAccount = OAuthWebSecurity.HasLocalAccount(cog.WebSecurity.GetUserId(User.Identity.Name));
            ViewBag.HasLocalPassword = hasLocalAccount;
            ViewBag.ReturnUrl = Url.Action("Manage");
            if (hasLocalAccount)
            {
                if (ModelState.IsValid)
                {
                    // ChangePassword will throw an exception rather than return false in certain failure scenarios.
                    bool changePasswordSucceeded;
                    try
                    {
                        changePasswordSucceeded = cog.WebSecurity.ChangePassword(User.Identity.Name, model.OldPassword, model.NewPassword);
                    }
                    catch (Exception)
                    {
                        changePasswordSucceeded = false;
                    }

                    if (changePasswordSucceeded)
                    {
                        return RedirectToAction("Manage", new { Message = ManageMessageId.ChangePasswordSuccess });
                    }
                    else
                    {
                        ModelState.AddModelError("", "The current password is incorrect or the new password is invalid.");
                    }
                }
            }
            else
            {
                // User does not have a local password so remove any validation errors caused by a missing
                // OldPassword field
                ModelState state = ModelState["OldPassword"];
                if (state != null)
                {
                    state.Errors.Clear();
                }

                if (ModelState.IsValid)
                {
                    try
                    {
                        cog.WebSecurity.CreateAccount(User.Identity.Name, model.NewPassword);
                        return RedirectToAction("Manage", new { Message = ManageMessageId.SetPasswordSuccess });
                    }
                    catch (Exception e)
                    {
                        ModelState.AddModelError("", e);
                    }
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        // POST: /Account/ExternalLogin
        /// <summary>
        /// Externals the login.
        /// </summary>
        /// <param name="provider">The provider.</param>
        /// <param name="returnUrl">The return URL.</param>
        /// <returns></returns>
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ExternalLogin(string provider, string returnUrl)
        {
            return new ExternalLoginResult(provider, Url.Action("ExternalLoginCallback", new { ReturnUrl = returnUrl }));
        }

        // GET: /Account/ExternalLoginCallback
        /// <summary>
        /// Externals the login callback.
        /// </summary>
        /// <param name="returnUrl">The return URL.</param>
        /// <returns></returns>
        [AllowAnonymous]
        public ActionResult ExternalLoginCallback(string returnUrl)
        {
            AuthenticationResult result = OAuthWebSecurity.VerifyAuthentication(Url.Action("ExternalLoginCallback", new { ReturnUrl = returnUrl }));
            if (!result.IsSuccessful)
            {
                return RedirectToAction("ExternalLoginFailure");
            }

            if (OAuthWebSecurity.Login(result.Provider, result.ProviderUserId, createPersistentCookie: false))
            {
                return RedirectToLocal(returnUrl);
            }

            if (User.Identity.IsAuthenticated)
            {
                // If the current user is logged in add the new account
                OAuthWebSecurity.CreateOrUpdateAccount(result.Provider, result.ProviderUserId, User.Identity.Name);
                return RedirectToLocal(returnUrl);
            }
            else
            {
                // User is new, ask for their desired membership name
                string loginData = OAuthWebSecurity.SerializeProviderUserId(result.Provider, result.ProviderUserId);
                ViewBag.ProviderDisplayName = OAuthWebSecurity.GetOAuthClientData(result.Provider).DisplayName;
                ViewBag.ReturnUrl = returnUrl;
                return View("ExternalLoginConfirmation", new RegisterExternalLoginModel { UserName = result.UserName, ExternalLoginData = loginData });
            }
        }

        // POST: /Account/ExternalLoginConfirmation
        /// <summary>
        /// Externals the login confirmation.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="returnUrl">The return URL.</param>
        /// <returns></returns>
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ExternalLoginConfirmation(RegisterExternalLoginModel model, string returnUrl)
        {
            string provider = null;
            string providerUserId = null;

            if (User.Identity.IsAuthenticated || !OAuthWebSecurity.TryDeserializeProviderUserId(model.ExternalLoginData, out provider, out providerUserId))
            {
                return RedirectToAction("Manage");
            }

            if (ModelState.IsValid)
            {
                // Insert a new user into the database
                using (UsersContext db = new UsersContext())
                {
                    UserProfile user = db.UserProfiles.FirstOrDefault(u => u.UserName.ToLower() == model.UserName.ToLower());
                    // Check if user already exists
                    if (user == null)
                    {
                        // Insert name into the profile table
                        db.UserProfiles.Add(new UserProfile { UserName = model.UserName });
                        db.SaveChanges();

                        OAuthWebSecurity.CreateOrUpdateAccount(provider, providerUserId, model.UserName);
                        OAuthWebSecurity.Login(provider, providerUserId, createPersistentCookie: false);

                        return RedirectToLocal(returnUrl);
                    }
                    else
                    {
                        ModelState.AddModelError("UserName", "User name already exists. Please enter a different user name.");
                    }
                }
            }

            ViewBag.ProviderDisplayName = OAuthWebSecurity.GetOAuthClientData(provider).DisplayName;
            ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }

        // GET: /Account/ExternalLoginFailure
        /// <summary>
        /// Externals the login failure.
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        public ActionResult ExternalLoginFailure()
        {
            return View();
        }

        /// <summary>
        /// Externals the logins list.
        /// </summary>
        /// <param name="returnUrl">The return URL.</param>
        /// <returns></returns>
        [AllowAnonymous]
        [ChildActionOnly]
        public ActionResult ExternalLoginsList(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return PartialView("_ExternalLoginsListPartial", OAuthWebSecurity.RegisteredClientData);
        }

        /// <summary>
        /// Removes the external logins.
        /// </summary>
        /// <returns></returns>
        [ChildActionOnly]
        public ActionResult RemoveExternalLogins()
        {
            ICollection<OAuthAccount> accounts = OAuthWebSecurity.GetAccountsFromUserName(User.Identity.Name);
            List<ExternalLogin> externalLogins = new List<ExternalLogin>();
            foreach (OAuthAccount account in accounts)
            {
                AuthenticationClientData clientData = OAuthWebSecurity.GetOAuthClientData(account.Provider);

                externalLogins.Add(new ExternalLogin
                {
                    Provider = account.Provider,
                    ProviderDisplayName = clientData.DisplayName,
                    ProviderUserId = account.ProviderUserId,
                });
            }

            ViewBag.ShowRemoveButton = externalLogins.Count > 1 || OAuthWebSecurity.HasLocalAccount(cog.WebSecurity.GetUserId(User.Identity.Name));
            return PartialView("_RemoveExternalLoginsPartial", externalLogins);
        }


        // Get: /Account/ForgetPassword
        /// <summary>
        /// Registers this instance.
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        public ActionResult ForgetPassword(ManageMessageId? message)
        {
            ViewBag.StatusMessage =
                message == ManageMessageId.SendEmailSuccess ? "Email has been sent successfully to your email Id.You can reset your password." : "";
            return View();
        }

        // POST: /Account/ForgetPassword
        /// <summary>
        /// ForgetPassword the specified model.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns></returns>
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ForgetPassword(UserModel model)
        {
            if (ModelState.IsValid)
            {
                string userName = string.Empty;

                UserViewModel userNameByEmail = _userService.GetUserByEmailId(model.UserName);
                if (userNameByEmail != null)
                {
                    userName = userNameByEmail.UserName;
                }
                else
                {
                    userName = model.UserName;
                }
                if (userName.Length > 0 && cog.WebSecurity.UserExists(userName))
                {
                    int UserID = cog.WebSecurity.GetUserId(userName);
                    UserViewModel user = _userService.GetUserByMemberShipId(UserID);
                    if (user != null)
                    {
                        string email = user.Email;                    
                        string url = "http://" + Request.Url.Authority + "/Account/ResetPassword/" + Convert.ToBase64String(Encoding.Unicode.GetBytes("user=" + userName + "&date=" + DateTime.Now));

                        StringBuilder sb = new StringBuilder();
                        sb.Append("<table><tr><td>Hello " + userName + ",</td>");
                        sb.Append("<br /><br />");
                        sb.Append("<tr><td>You can reset your password using below URL :</td></tr><br />");
                        sb.Append("<tr><td><a href='" + url + "'>Reset your password</a></td></tr><br />");
                        sb.Append("<tr><td>Regards, UniBoard </td></tr></table>");
                        string bodyMessage = sb.ToString();

                        var fromEmail = WebSite.Current.EmailSenderEmailAddress;
                        var displayName = "";
                        var subject = WebSite.Current.SubjectResetPassword;
                        var smtpUserName = WebSite.Current.SMTPUserName;
                        var smtpPassword = WebSite.Current.SMTPPassword;
                        var smtpServer = WebSite.Current.SMTPServer;
                        var smtpPort = WebSite.Current.SMTPPort;
                        var cc = string.Empty;
                        var bcc = string.Empty;
                        var sslEnabled = WebSite.Current.SMTPEnableSSL;

                        SmtpClient client = UniEBoard.Helpers.Email.EmailHelper.GetSmtpClient(smtpServer, smtpPort, smtpUserName, smtpPassword, sslEnabled);
                        MailMessage message = UniEBoard.Helpers.Email.EmailHelper.GetMailMessage(fromEmail, displayName, email, subject, bodyMessage, cc, bcc);
                        var emailSent = UniEBoard.Helpers.Email.EmailHelper.SendEmail(client, message);
                        if (emailSent)
                        {
                            ViewBag.EmailSend = 1;
                            ViewBag.StatusMessage = "Email has been sent successfully to your email Id.You can reset your password.";
                            return RedirectToLocal(Url.Action("Index", "Home"));
                        }
                        else
                        {
                            ViewBag.EmailSend = 0;
                            ViewBag.StatusMessage = "There was an error during sending email. Please contact the administrator";
                        }
                    }
                }
                else
                {
                    ViewBag.EmailSend = 0;
                    ModelState.AddModelError("", "User Name or Email does not exist. Please enter valid User Name and Email");
                }
            }

            //return RedirectToAction("Login");
            // If we got this far, something failed, redisplay form
            return View(model);
        }

        // Get: /Account/ResetPassword
        /// <summary>
        /// ResetPassword this instance.
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        public ActionResult ResetPassword(ManageMessageId? message, string id)
        {
            ViewBag.StatusMessage =
                message == ManageMessageId.ChangePasswordSuccess ? "Your password has been changed." :
                message == ManageMessageId.SetPasswordSuccess ? "Your password has been reset." : "";

            if (!String.IsNullOrEmpty(id))
            {
                string url = Encoding.Unicode.GetString(Convert.FromBase64String(id));
                string[] arrUrl = url.Split('&');
                string[] timeStr = arrUrl[1].Split('=');

                string[] userStr = arrUrl[0].Split('=');
                string userName = userStr[1].ToString().Trim();


                DateTime startDate = Convert.ToDateTime(timeStr[1].ToString());
                DateTime endDate = startDate.AddHours(24);
                DateTime CurrDate = DateTime.Now;
                if (CurrDate.Ticks > startDate.Ticks && CurrDate.Ticks < endDate.Ticks)
                {
                    ViewBag.URLExpires = 0;
                }
                else
                {
                    ViewBag.URLExpires = 1;
                    ModelState.AddModelError("", "URL has been expired!");
                }
            }
            return View();
        }

        // POST: /Account/ResetPassword
        /// <summary>
        /// ResetPassword the specified model.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns></returns>
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ResetPassword(ForgotPasswordModel model, string id)
        {
            string url = Encoding.Unicode.GetString(Convert.FromBase64String(id));
            string[] arrUrl = url.Split('&');
            string[] userStr = arrUrl[0].Split('=');
            string userName = userStr[1].ToString().Trim();

            if (cog.WebSecurity.UserExists(userName))
            {
                string pwdtoken = cog.WebSecurity.GeneratePasswordResetToken(userName, 1440);
                cog.WebSecurity.ResetPassword(pwdtoken, model.NewPassword);
                return RedirectToAction("ResetPassword", new { Message = ManageMessageId.SetPasswordSuccess });
            }
            else
            {
                ModelState.AddModelError("", "Invalid URL!");
            }
            return View(model);
        }


        #endregion

        #region Helpers

        /// <summary>
        /// Redirects to local.
        /// </summary>
        /// <param name="returnUrl">The return URL.</param>
        /// <returns></returns>
        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        public enum ManageMessageId
        {
            ChangePasswordSuccess,
            SetPasswordSuccess,
            RemoveLoginSuccess,
            SendEmailSuccess,
        }

        internal class ExternalLoginResult : ActionResult
        {
            public ExternalLoginResult(string provider, string returnUrl)
            {
                Provider = provider;
                ReturnUrl = returnUrl;
            }

            public string Provider { get; private set; }
            public string ReturnUrl { get; private set; }

            public override void ExecuteResult(ControllerContext context)
            {
                OAuthWebSecurity.RequestAuthentication(Provider, ReturnUrl);
            }
        }

        private static string ErrorCodeToString(MembershipCreateStatus createStatus)
        {
            // See http://go.microsoft.com/fwlink/?LinkID=177550 for
            // a full list of status codes.
            switch (createStatus)
            {
                case MembershipCreateStatus.DuplicateUserName:
                    return "User name already exists. Please enter a different user name.";

                case MembershipCreateStatus.DuplicateEmail:
                    return "A user name for that e-mail address already exists. Please enter a different e-mail address.";

                case MembershipCreateStatus.InvalidPassword:
                    return "The password provided is invalid. Please enter a valid password value.";

                case MembershipCreateStatus.InvalidEmail:
                    return "The e-mail address provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidAnswer:
                    return "The password retrieval answer provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidQuestion:
                    return "The password retrieval question provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidUserName:
                    return "The user name provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.ProviderError:
                    return "The authentication provider returned an error. Please verify your entry and try again. If the problem persists, please contact your system administrator.";

                case MembershipCreateStatus.UserRejected:
                    return "The user creation request has been canceled. Please verify your entry and try again. If the problem persists, please contact your system administrator.";

                default:
                    return "An unknown error occurred. Please verify your entry and try again. If the problem persists, please contact your system administrator.";
            }
        }
        #endregion
    }
}
