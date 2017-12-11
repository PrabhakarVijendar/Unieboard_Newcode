// --------------------------------------------------------------------------------------------------------------------
// <copyright file="HomeController.cs" company="Cognite Ltd">
//   Copyright (c) 2013 Cognite Ltd (http://www.cognite.co.uk)
// </copyright>
// <summary>
//  Home Controller Methods
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using Cognite.Utility.MethodExtensions.StringExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using UniEBoard.Service.Models.Units;
using WebMatrix.WebData;
using UniEBoard.Filters;
using UniEBoard.Service.Interfaces.ApplicationService;
using UniEBoard.Model.Enums;
using UniEBoard.Service.Models;
using UniEBoard.Service.Models.Quizzes;
using UniEBoard.Service.ApplicationServices;
using UniEBoard.Security;
using System.IO;
using System.Web.Security;

namespace UniEBoard.Controllers
{
    /// <summary>
    /// The Home Controller
    /// </summary>
    [Authorize]
    [InitializeSimpleMembership]
    public class HomeController : BaseController
    {
        #region Members

        /// <summary>
        /// Student Application Service 
        /// </summary>
        private IStudentAppService _studentService;

        /// <summary>
        /// Staff Application Service 
        /// </summary>
        private IStaffAppService _staffService;

        /// <summary>
        /// User App Service
        /// </summary>
        private IUserAppService _userService;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="HomeController"/> class.
        /// </summary>
        /// <param name="studentService">The student service.</param>
        /// <param name="staffService">The staff service.</param>
        public HomeController(IStudentAppService studentService, IStaffAppService staffService, IUserAppService userAppService)
            : base (userAppService)
        {
            this._studentService = studentService;
            this._staffService = staffService;
            this._userService = userAppService;
        }

        #endregion

        #region Methods


        /// <summary>
        /// GET: /Home/
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {
            if (Request.IsAuthenticated)
            {
                UserViewModel user = _userService.GetUserByMemberShipId(CurrentUser.Id);
                if (user != null && user is StudentViewModel) { return RedirectToAction("Index", "Student"); }
                if (user != null && user is StaffViewModel) { return RedirectToAction("Index", "Teacher"); }
                return RedirectToAction("Index", "Student");
            }
            ViewBag.Message = "UnieBoard";
            return View();
        }

        /// <summary>
        /// GET: /Home/About
        /// </summary>
        /// <returns></returns>
        public ActionResult About()
        {
            ViewBag.Message = "Your app description page.";

            return View();
        }

        /// <summary>
        /// GET: /Home/Contact
        /// </summary>
        /// <returns></returns>
        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        #endregion

    }
}
