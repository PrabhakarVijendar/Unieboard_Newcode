using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using System.Web.Security;
using UniEBoard.Filters;
using System.Dynamic;
using UniEBoard.Model;
using UniEBoard.Model.Enums;
using UniEBoard.Models;
using UniEBoard.Security;
using UniEBoard.Service.Interfaces.ApplicationService;
using UniEBoard.Service.Models;
using WebMatrix.WebData;
//using RestSharp;

namespace UniEBoard.Controllers
{
    [CustomAuthorize]
    [InitializeSimpleMembership]
    public class AdminController : Controller
    {
        #region Members
        
        /// <summary>
        /// Staff Application Service
        /// </summary>
        private IStaffAppService _staffService;

        /// <summary>
        /// Student Application Service
        /// </summary>
        private IStudentAppService _studentService;

        #endregion

        #region AdminController

        /// <summary>
        /// Initializes a new instance of the <see cref="AdminController"/> class.
        /// </summary>
        /// <param name="staffService">The staff service.</param>
        /// <param name="studentService">The student service.</param>
        public AdminController(IStaffAppService staffService, IStudentAppService studentService)
        {
            this._staffService = staffService;
            this._studentService = studentService;
        }

        #endregion

        #region Admin Login

        /// <summary>
        /// GET: /Admin/Login - Logins this instance.
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            if (Request.IsAuthenticated)
            {
                return RedirectToAction("Index", "Admin");
            }
            ViewBag.ReturnUrl = returnUrl;
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
        public ActionResult Login(AdminLoginViewModel model, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                if (WebSecurity.Login(model.UserName, model.Password, persistCookie: false) && Roles.IsUserInRole(model.UserName, "Administrator"))
                {
                    return !string.IsNullOrEmpty(returnUrl) ? RedirectToLocal(returnUrl) : RedirectToAction("Index", "Admin");
                }

                WebSecurity.Logout();
                // If we got this far, something failed, redisplay form
                ModelState.AddModelError("", "We could not log you in. Please check that you have administrative access and the user name and password provided is correct.");
            }
            return View(model);
        }

        #endregion

        /// <summary>
        /// Students this instance.
        /// </summary>
        /// <returns></returns>
        public ActionResult Students()
        {
            List<StudentViewModel> students = _studentService.GetAllStudents();
            return View(students);
        }

        /// <summary>
        /// Students this instance.
        /// </summary>
        /// <returns></returns>
        public ActionResult Student(int id)
        {
            StudentViewModel student = _studentService.GetStudentByMemberShipId(id);
            return View(student);
        }

        /// <summary>
        /// The default Index view
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        public ActionResult Index()
        {
            //var client = new RestClient("http://localhost:58101");
            //client.Authenticator = new HttpBasicAuthenticator("Administrator","Password123");
            //var request = new RestRequest("api/adminmanageusers", Method.GET);
            //var response2 = client.Execute<List<StudentViewModel>>(request);
            //return View(response2.Data);
            List<StudentViewModel> students = _studentService.GetAllStudents();
            return View(students);
        }

        //
        // GET: /Admin/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        //
        // GET: /Admin/Create

        public ActionResult Create()
        {
            return View();
        }

        //
        // POST: /Admin/Create

        [HttpPost]
        public ActionResult Create(FormCollection collection)
        {
            try
            {
                // TODO: Add insert logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        //
        // GET: /Admin/Edit/5

        public ActionResult Edit(int id)
        {
            return View();
        }

        //
        // POST: /Admin/Edit/5

        [HttpPost]
        public ActionResult Edit(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add update logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        //
        // GET: /Admin/Delete/5

        public ActionResult Delete(int id)
        {
            return View();
        }

        //
        // POST: /Admin/Delete/5

        [HttpPost]
        public ActionResult Delete(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add delete logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

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
                return RedirectToAction("Index", "Admin");
            }
        }

        #endregion
    }
}
