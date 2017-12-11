using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using UniEBoard.Service.Interfaces.ApplicationService;
using UniEBoard.Service.Models;

namespace UniEBoard.Controllers
{
    public class AutoCompleteController : BaseController
    {
        //
        // GET: /AutoComplete/
        #region Members
        /// <summary>
        /// Staff Application Service
        /// </summary>
        private IStaffAppService _staffService;

        /// <summary>
        /// Student Application Service
        /// </summary>
        private IStudentAppService _studentService;

        /// <summary>
        /// User Application Service
        /// </summary>
        private IUserAppService _userAppService;

        #endregion

        #region Constructors
        public AutoCompleteController(IStudentAppService studentAppService, IStaffAppService staffAppService, IUserAppService userAppService)
            : base(userAppService)
        {
            this._studentService = studentAppService;
            this._staffService = staffAppService;
            this._userAppService = userAppService;
        }
        #endregion

        #region Methods/Actions

        public ActionResult Users(string term)
        {
            var json = Json((from user in _userAppService.GetAllUsersByCompany(CurrentUser.CompanyId, 0)
                             where user.FullName.ToUpper().Contains(term.ToUpper())
                             select new { userName = string.Format("{0} ({1}) - {2}", user.FullName, user.UserName, (user is StudentViewModel ? "Student" : "Teacher")), id = user.Id }).ToArray(), JsonRequestBehavior.AllowGet);
            return json;
        }

        #endregion
    }
}
