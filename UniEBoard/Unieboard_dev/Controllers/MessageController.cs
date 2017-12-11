using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using UniEBoard.Model.Entities;
using UniEBoard.Service.Factories;
using UniEBoard.Service.Interfaces.ApplicationService;
using UniEBoard.Service.Models;

namespace UniEBoard.Controllers
{
    public class MessageController : BaseController
    {
        #region Members

        IUserAppService _userAppService;
        IStudentAppService _studentAppService;
        IMessageAppService _messageAppService;
        ICourseModuleAppService _courseModuleAppService;

        static List<UserViewModel> userList = new List<UserViewModel>();

        #endregion

        #region Constructors

        public MessageController(IUserAppService userAppService, 
            IMessageAppService messageAppService,
            IStudentAppService studentAppService,
            ICourseModuleAppService courseModuleAppService) 
            : base(userAppService)
        {
            this._userAppService = userAppService;
            this._studentAppService = studentAppService;
            this._messageAppService = messageAppService;
            this._courseModuleAppService = courseModuleAppService;
        }

        #endregion

        //
        // GET: /Message/

        public ActionResult CreateUserList(int id)
        {
            UserViewModel user = _userAppService.GetUserById(id);
            userList.Add(user);

            return PartialView("_MessageUsersPartial", userList);
        }

        public ActionResult Index()
        {
            userList = new List<UserViewModel>();
            var courses = _courseModuleAppService.GetAllCourses().Where(c => c.CompanyId.Equals(CurrentUser.CompanyId)).Select(c => new SelectListItem()
            {
                Text = c.Title,
                Value = c.Id.ToString()
            }).ToList();
            return PartialView("_Message", courses);
        }

        [HttpPost]
        public void SendMessage(int? ddCourses, string textMessage)
        {
            var controller = User is StudentViewModel ? "Student" : "Teacher";
            var usersInCourse = _studentAppService.GetUsersForStudent(CurrentUser.Id).Where(s => s.Courses.Any(c => c.Id.Equals(ddCourses.Value)));
            var users = ddCourses != null ? _userAppService.GetUsersByCourse(ddCourses.Value).Where(u => !u.Id.Equals(CurrentUser.Id)).ToList() : new List<UserViewModel>();
            users.AddRange(userList);

            List<Message> messages = StudentMessageViewModelFactory.CreateUserMessage(users, textMessage, CurrentUser);
            _messageAppService.AddMessages(messages);

            userList = new List<UserViewModel>();
        }

    }
}
