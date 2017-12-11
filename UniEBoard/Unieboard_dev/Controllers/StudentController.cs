// --------------------------------------------------------------------------------------------------------------------
// <copyright file="StudentController.cs" company="Cognite Ltd">
//   Copyright (c) 2013 Cognite Ltd (http://www.cognite.co.uk)
// </copyright>
// <summary>
//  Student Controller Methods
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebMatrix.WebData;
using UniEBoard.Filters;
using UniEBoard.Service.Interfaces.ApplicationService;
using UniEBoard.Model.Enums;
using UniEBoard.Service.Models;
using UniEBoard.Service.Models.Quizzes;
using UniEBoard.Service.ApplicationServices;
using UniEBoard.Model.Entities;
using UniEBoard.Service.Factories;
using System.Web.Script.Serialization;


namespace UniEBoard.Controllers
{
    /// <summary>
    /// The Student Controller
    /// </summary>
    [Authorize]
    [InitializeSimpleMembership]
    public class StudentController : BaseController
    {
        #region Members

        /// <summary>
        /// Student Application Service 
        /// </summary>
        private IStudentAppService _studentService;

        /// <summary>
        /// User Application Service 
        /// </summary>
        private IUserAppService _userService;

        /// <summary>
        /// Assignment, Task and Submission Service
        /// </summary>
        private IAssignmentTaskSubmissionAppService _assignmentTaskAndSubmissionService;

        /// <summary>
        /// File Service
        /// </summary>
        private IFileAppService _fileService;

        /// <summary>
        /// Message Service
        /// </summary>
        private IMessageAppService _messageService;

        /// <summary>
        /// Schedule Service
        /// </summary>
        private IScheduleAppService _scheduleService;

        /// <summary>
        /// Course And Module Application Service 
        /// </summary>
        private ICourseModuleAppService _courseModuleService;

        /// <summary>
        /// Unit Application Service 
        /// </summary>
        private IUnitModuleAppService _unitModuleService;

        /// <summary>
        /// BaseQuestionTopic Application Service 
        /// </summary>
        private IBaseQuestionTopicAppService _baseQuestionTopicModuleService;


        #endregion

        #region Properties
        new protected UserViewModel User
        {
            get
            {
                return _userService.GetUserByUserName(WebSecurity.CurrentUserName);
            }
        }
        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="StudentController"/> class.
        /// </summary>
        /// <param name="studentService">The student service.</param>
        /// <param name="assignmentTaskAndSubmission">The assignment task and submission.</param>
        /// <param name="messageService">The message service.</param>
        /// <param name="courseModuleService">The course module service.</param>
        /// <param name="fileService">The file service.</param>
        /// <param name="basequestionTopicService">The basequestionTopic service.</param>
        public StudentController(
            IStudentAppService studentService,
            IUserAppService userAppService,
            IAssignmentTaskSubmissionAppService assignmentTaskAndSubmission,
            IMessageAppService messageService,
            ICourseModuleAppService courseModuleService,
            IFileAppService fileService,
            IUnitModuleAppService unitService,
            IScheduleAppService scheduleService,
            IBaseQuestionTopicAppService basequestionTopicService
            ) : base(userAppService)
        {
            this._studentService = studentService;
            this._userService = userAppService;
            this._assignmentTaskAndSubmissionService = assignmentTaskAndSubmission;
            this._messageService = messageService;
            this._courseModuleService = courseModuleService;
            this._fileService = fileService;
            this._unitModuleService = unitService;
            this._scheduleService = scheduleService;
            this._baseQuestionTopicModuleService = basequestionTopicService;
        }

        #endregion

        #region Methods

        #region Index

        /// <summary>
        /// GET: /Student/
        /// </summary>
        /// <returns></returns>
        [ActionName("Index")]
        public ActionResult Index()
        {
            try
            {
                if (CurrentUser.IsAdmin) return RedirectToAction("Index", "Teacher");
                var onlineUsers = AddOnlineUsers(CurrentUser);

                ViewBag.Assignments = _assignmentTaskAndSubmissionService.GetAllStudentUpcomingTaskAndAssignmentDeadlines(CurrentUser.Id);
                ViewBag.Messages = _messageService.GetAllNotViewedStudentMessages(CurrentUser.Id);
                ViewBag.Courses = _courseModuleService.GetAllStudentCourses(CurrentUser.Id);
                ViewBag.Schedule = _scheduleService.GetSchedulesWithUnitsAndModulesByStudent(CurrentUser.Id);
                ViewBag.MyQuestions = _baseQuestionTopicModuleService.GetAllByStudent(CurrentUser.Id);
                ViewData["Units"] = _unitModuleService.GetUnitsByStudent(CurrentUser.Id)
                    .Where(u => u.PublishFrom.Value.Date.Equals(DateTime.Today)).OrderBy(u => u.PublishFrom);
                ViewData["Messages"] = _messageService.GetAllStudentMessages(User.Id);
                ViewData["OnlineUsers"] = onlineUsers;
                return View(CurrentUser);
            }
            catch (Exception)
            {
                return RedirectToAction("LogOff", "Account");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult UnitListDashboardFilteredPartial(string filter)
        {
            UserViewModel student = _userService.GetUserByUserName(WebSecurity.CurrentUserName);
            var units = _unitModuleService.GetUnitsByStudent(student.Id)
                .Where(u => u.PublishFrom.Value.Date.Equals(Convert.ToDateTime(filter).Date)).OrderBy(u => u.PublishFrom);

            return PartialView("_DashboardScheduleUnitGridPartial", units);
        }

        #endregion

        #region Messages

        /// <summary>
        /// Adds the student viewed message.
        /// </summary>
        /// <param name="messageId">The message id.</param>
        /// <param name="studentId">The student id.</param>
        [ActionName("AddStudentViewedMessage")]
        [HttpPost]
        public ActionResult AddStudentViewedMessage(int messageId, int userId, int messageType)
        {
            //_messageService.AddStudentViewedMessage(messageId, studentId);
            _messageService.MessageManager.Remove(messageId);
            return Content(_messageService.GetAllStudentMessages(userId).Where(m => m.MessageType.Equals(messageType)).ToList().Count.ToString());
        }

        #endregion

        #region Tasks

        /// <summary>
        /// GET: /Student/Tasks
        /// </summary>
        /// <returns></returns>
        [ActionName("Tasks")]
        public ActionResult Tasks()
        {
            ViewBag.Filter = _assignmentTaskAndSubmissionService.GetTaskDisplayFilter("Tasks", "Student", "tasks");
            ViewBag.Tasks = _assignmentTaskAndSubmissionService.GetTasks(CurrentUser.Id, TaskFilterType.Active);
            return View("Tasks");
        }

        /// <summary>
        /// Adds the task.
        /// </summary>
        /// <param name="task">The task.</param>
        /// <returns></returns>
        int hack = 0;
        [ActionName("AddTask")]
        public ActionResult AddTask(TaskViewModel task)
        {
            if (ModelState.IsValid && hack == 0)
            {
                TaskViewModel taskViewModel = _assignmentTaskAndSubmissionService.AddTask(task, CurrentUser.Id);
                if (taskViewModel != null)
                {
                    Message message = new Message()
            {
                        Body = task.Note,
                        Title = task.Title,
                        FromUserId = User.Id,
                        RecipientUserId = User.Id,
                        MessageType = MessageType.Task,
                        DateCreated = DateTime.UtcNow,
                        EntityId = taskViewModel.Id
                    };

                    _messageService.MessageManager.Add(message);
                }
                ViewBag.Filter = _assignmentTaskAndSubmissionService.GetTaskDisplayFilter("Tasks", "Student", "tasks");
                List<TaskViewModel> tasks = _assignmentTaskAndSubmissionService.GetTasks(CurrentUser.Id, TaskFilterType.Active);
                hack++;
                return PartialView("_StudentTaskPartial", tasks);
            }
            else 
            {
                return PartialView("_StudentAddTaskValidationPartial");
            }
        }

        /// <summary>
        /// GET: /Student/Tasks
        /// </summary>
        /// <returns></returns>
        [ActionName("Tasks")]
        [HttpPost]
        public ActionResult Tasks(int filter)
        {
            ViewBag.Filter = _assignmentTaskAndSubmissionService.GetTaskDisplayFilter("Tasks", "Student", "tasks", (TaskFilterType)filter);
            List<TaskViewModel> tasks = _assignmentTaskAndSubmissionService.GetTasks(CurrentUser.Id, (TaskFilterType)filter);
            return PartialView("_StudentTaskPartial", tasks);
        }

        /// <summary>
        /// Completes the task.
        /// </summary>
        /// <param name="taskId">The task id.</param>
        /// <returns></returns>
        [ActionName("CompleteTask")]
        public ActionResult CompleteTask(int taskId)
        {
            _assignmentTaskAndSubmissionService.CompleteTask(taskId);
            ViewBag.Filter = _assignmentTaskAndSubmissionService.GetTaskDisplayFilter("Tasks", "Student", "tasks");
            List<TaskViewModel> tasks = _assignmentTaskAndSubmissionService.GetTasks(CurrentUser.Id, TaskFilterType.Active);
            return PartialView("_StudentTaskPartial", tasks);
        }

        #endregion

        #region Courses

        /// <summary>
        /// GET: /Student/Courses
        /// </summary>
        /// <returns></returns>
        [ActionName("Courses")]
        public ActionResult Courses()
        {
            List<CourseViewModel> courses = _courseModuleService.GetAllStudentCourses(User.Id);
            return View(courses);
        }

        #endregion

        #region Grades

        /// <summary>
        /// GET: /Student/Grades
        /// </summary>
        /// <returns></returns>
        [ActionName("Grades")]
        public ActionResult Grades()
        {
            
            var courses = _courseModuleService.GetAllStudentCourses(User.Id, false);
            if (courses.Count() != 0)
            {
                var values = from c in courses
                             select new { Id = c.Id, Name = c.Title };

                int selectedCourse = courses.First().Id;

                ViewData["Validation"] = true;
                ViewData["Courses"] = new SelectList(values, "Id", "Name", selectedCourse);

                int gradeOverall = _studentService.GetGradeForStudentByCourse(User.Id, selectedCourse);

                ViewData["GradeOverall"] = gradeOverall > 100 ? 100 : gradeOverall;
                ViewData["GradePerModule"] = _studentService.GetGradeForStudentByCoursePerModule(User.Id, selectedCourse);
                ViewData["GradePerAssignment"] = _studentService.GetGradeForStudentByCoursePerAssignment(User.Id, selectedCourse);
            }
            else {
                ViewData["Validation"] = false;
            }
            return View();
        }

        [ActionName("GradesByStudentsPerCourse")]
        [HttpPost]
        public ActionResult GradesByStudentsPerCourse(int id)
        {
            //(13, 41)
            int gradeOverall = _studentService.GetGradeForStudentByCourse(User.Id, id);      
            
            
            ViewData["GradeOverall"] =  gradeOverall > 100 ? 100 : gradeOverall ;
            ViewData["GradePerModule"] = _studentService.GetGradeForStudentByCoursePerModule(User.Id, id);
            ViewData["GradePerAssignment"] = _studentService.GetGradeForStudentByCoursePerAssignment(User.Id, id);


            return PartialView("_GradesPartial");
        }

        #endregion

        #region Assignments

        /// <summary>
        /// GET: /Student/Assignments
        /// </summary>
        /// <returns></returns>
        [ActionName("Assignments")]
        public ActionResult Assignments()
        {
            ViewBag.Filter = _assignmentTaskAndSubmissionService.GetAssignmentDisplayFilter("Assignments", "Student", "assignments");
            ViewData["NoAssignmentDisplayLabel"] = "active ";
            ViewData["StudentCourses"] = new SelectList(_courseModuleService.GetAllStudentCourses(CurrentUser.Id, false), "Id", "Title", "");
            List<AssignmentSubmissionViewModel> models = _assignmentTaskAndSubmissionService.GetAllStudentAssignmentSubmissions(CurrentUser.Id, StudentAssignmentFilterType.Active, 0);
            return View("AssignmentSubmissions", models);
        }

        /// <summary>
        /// GET: /Student/Assignments
        /// </summary>
        /// <returns></returns>
        [ActionName("Assignments")]
        [HttpPost]
        public ActionResult Assignments(DisplayFilterViewModel filter, int? selectedCourse)
        {
            StudentAssignmentFilterType activeFilter = (StudentAssignmentFilterType)filter.ActiveFilter;
            ViewBag.Filter = _assignmentTaskAndSubmissionService.GetAssignmentDisplayFilter("Assignments", "Student", "assignments", activeFilter);
            ViewData["NoAssignmentDisplayLabel"] = (activeFilter == StudentAssignmentFilterType.Active) ? "active " : ((activeFilter == StudentAssignmentFilterType.Submitted) ? "submitted " : string.Empty);
            ViewData["StudentCourses"] = new SelectList(_courseModuleService.GetAllStudentCourses(CurrentUser.Id, false), "Id", "Title", selectedCourse.HasValue ? selectedCourse.Value.ToString() : string.Empty);
            List<AssignmentSubmissionViewModel> models = _assignmentTaskAndSubmissionService.GetAllStudentAssignmentSubmissions(CurrentUser.Id, activeFilter, selectedCourse.HasValue ? selectedCourse.Value : 0);
            return View("AssignmentSubmissions", models);
        }

        /// <summary>
        /// Adds the submission.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost]
        [AcceptVerbs(HttpVerbs.Post)]
        [ActionName("UploadSubmissionFile")]
        public ActionResult UploadSubmissionFile(AssignmentSubmissionViewModel model)
        {
            ICollection<BaseFileViewModel> files = new List<BaseFileViewModel>();
            try
            {
                _assignmentTaskAndSubmissionService.UploadStudentSubmissionDocument(CurrentUser.Id, model);
                AssignmentSubmissionViewModel viewModel = _assignmentTaskAndSubmissionService.GetStudentAssignmentSubmission(CurrentUser.Id, model.AssignmentId);
                files = viewModel.FileUploads;
            }
            catch (Exception)
            {
            }
            return PartialView("_SubmissionFilePartial", files);
        }

        /// <summary>
        /// Makes the submission.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost]
        [AcceptVerbs(HttpVerbs.Post)]
        [ActionName("MakeSubmission")]
        public ActionResult MakeSubmission(AssignmentSubmissionViewModel model)
        {
            AssignmentSubmissionViewModel viewModel = null;
            try
            {
                // Submit Assignment
                viewModel = _assignmentTaskAndSubmissionService.SaveStudentSubmission(CurrentUser.Id, model, true);
            }
            catch (Exception)
            {
            }

            //return PartialView("_AssignmentSubmissionsPartial", viewModel ?? model);
            return RedirectToAction("Assignments");
        }

        /// <summary>
        /// Saves the submission.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost]
        [AcceptVerbs(HttpVerbs.Post)]
        [ActionName("SaveSubmission")]
        public void SaveSubmission(AssignmentSubmissionViewModel model)
        {
            try
            {
                // Submit Assignment
                _assignmentTaskAndSubmissionService.SaveStudentSubmission(CurrentUser.Id, model);
            }
            catch (Exception)
            {
            }
        }

        /// <summary>
        /// Removes from submission.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <param name="identityToken">The identity token.</param>
        /// <param name="submissionId">The submission id.</param>
        /// <returns></returns>
        [HttpPost]
        public string RemoveSubmissionFile(int id, Guid identityToken)
        {
            //List<BaseFileViewModel> files = new List<BaseFileViewModel>();
            try
            {
                _fileService.RemoveFileByIdAndIdentityToken(id, identityToken);
                //files = _fileService.GetFilesBySubmission(submissionId);
            }
            catch (Exception)
            {
            }
            return string.Empty;
            //return PartialView("_SubmissionFilePartial", files);
        }

        /// <summary>
        /// GET: /Student/Assignment/5
        /// </summary>
        /// <param name="id">The id.</param>
        /// <returns></returns>
        [ActionName("Assignment")]
        public ActionResult Assignment(int id)
        {
            AssignmentSubmissionViewModel model = _assignmentTaskAndSubmissionService.GetStudentAssignmentSubmission(CurrentUser.Id, id);
            return View("Submission", model);
        }

        #endregion

        #region PrivateMethods

        /// <summary>
        /// 
        /// </summary>
        /// <param name="submissions"></param>
        private String ConvertSubmissionValuesToJson(List<SubmissionViewModel> submissions)
        {
            JavaScriptSerializer jss = new JavaScriptSerializer();

            return jss.Serialize(from sub in submissions
                                          select new { Asignment = sub.Assignment.Title, Grade = sub.GradePointValue });

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="moduleGrades"></param>
        private String ConvertModuleGradeValuesToJson(List<ModuleGradeViewModel> moduleGrades)
        {
            JavaScriptSerializer jss = new JavaScriptSerializer();
            return jss.Serialize(from mg in moduleGrades
                                          select new { Title = mg.Module.Title, Grade = mg.Grade });

        }
        
        #endregion


        #endregion
    }
}
