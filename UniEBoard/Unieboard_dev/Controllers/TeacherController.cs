// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TeacherController.cs" company="Cognite Ltd">
//   Copyright (c) 2013 Cognite Ltd (http://www.cognite.co.uk)
// </copyright>
// <summary>
//  Teacher Controller Methods
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
using UniEBoard.Resource;
using UniEBoard.Service.Helpers.Comparer;
using Cognite.Utility.Helpers.Methods;
using System.Text;
using UniEBoard.Service.Helpers.Configuration;
using System.Net.Mail;
using UniEBoard.Model.Entities;

using cog = Cognite.MembershipProvider;

namespace UniEBoard.Controllers
{
    /// <summary>
    /// The Teacher Controller
    /// </summary>
    [CustomAuthorize(Roles = "Administrator, Teacher")]
    [InitializeSimpleMembership]
    public class TeacherController : BaseController
    {
        #region Members

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
        /// Course And Module Application Service 
        /// </summary>
        private ICourseModuleAppService _courseModuleService;

        /// <summary>
        /// BaseQuestionTopic Application Service 
        /// </summary>
        private IBaseQuestionTopicAppService _baseQuestionTopicModuleService;

        /// <summary>
        /// Staff Application Service
        /// </summary>
        private IStaffAppService _staffService;

        /// <summary>
        /// Student Application Service
        /// </summary>
        private IStudentAppService _studentService;

        /// <summary>
        /// Asset Application Service
        /// </summary>
        private IAssetAppService _assetService;

        /// <summary>
        /// Module Application Service
        /// </summary>
        private IUnitModuleAppService _unitModuleAppService;

        /// <summary>
        /// Video Application Service
        /// </summary>
        private IVideoAppService _videoAppService;

        /// <summary>
        /// Quiz Application Service
        /// </summary>
        private IQuizAppService _quizAppService;

        /// <summary>
        /// User Application Service
        /// </summary>
        private IUserAppService _userAppService;

        /// <summary>
        /// Question Application Service
        /// </summary>
        private IQuestionAppService _questionAppService;

        /// <summary>
        /// Answer Application Service
        /// </summary>
        public IAnswerAppService _answerAppService { get; set; }

        /// <summary>
        /// Schedule Application Service
        /// </summary>
        private IScheduleAppService _scheduleAppService;

        /// <summary>
        /// Discussion Application Service
        /// </summary>
        private IDiscussionAppService _discussionAppService;

        static List<AssetViewModel> assignmentAssets = new List<AssetViewModel>();

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="TeacherController"/> class.
        /// </summary>
        /// <param name="staffService">The staff service.</param>
        /// <param name="assetService">The asset service.</param>
        /// <param name="assignmentTaskAndSubmission">The assignment task and submission.</param>
        /// <param name="messageService">The message service.</param>
        /// <param name="courseModuleService">The course module service.</param>
        /// <param name="fileService">The file service.</param>
        /// <param name="basequestionTopicService">The basequestionTopic service.</param>
        public TeacherController(
            IStaffAppService staffService,
            IStudentAppService studentService,
            IAssetAppService assetService,
            IAssignmentTaskSubmissionAppService assignmentTaskAndSubmission,
            IMessageAppService messageService,
            ICourseModuleAppService courseModuleService,
            IFileAppService fileService,
            IUserAppService userAppService,
            IBaseQuestionTopicAppService basequestionTopicService,
            IUnitModuleAppService iUnitModuleAppService,
            IVideoAppService iVideoAppService,
            IQuizAppService quizAppService,
            IQuestionAppService questionAppService,
            IAnswerAppService answerAppService,
            IScheduleAppService scheduleAppService,
            IDiscussionAppService discussionAppService
            )
            : base(userAppService)
        {
            this._staffService = staffService;
            this._studentService = studentService;
            this._assetService = assetService;
            this._assignmentTaskAndSubmissionService = assignmentTaskAndSubmission;
            this._messageService = messageService;
            this._courseModuleService = courseModuleService;
            this._fileService = fileService;
            this._baseQuestionTopicModuleService = basequestionTopicService;
            this._unitModuleAppService = iUnitModuleAppService;
            this._videoAppService = iVideoAppService;
            this._quizAppService = quizAppService;
            this._userAppService = userAppService;
            this._questionAppService = questionAppService;
            this._answerAppService = answerAppService;
            this._scheduleAppService = scheduleAppService;
            this._discussionAppService = discussionAppService;
        }

        #endregion

        #region Methods

        #region Tasks

        /// <summary>
        /// GET: /Student/Tasks
        /// </summary>
        /// <returns></returns>
        [ActionName("Tasks")]
        public ActionResult Tasks()
        {
            ViewBag.Filter = _assignmentTaskAndSubmissionService.GetTaskDisplayFilter("Tasks", "Teacher", "tasks");
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
                        FromUserId = CurrentUser.Id,
                        RecipientUserId = CurrentUser.Id,
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

        #region Dashboard

        /// <summary>
        /// GET: /Teacher/
        /// </summary>
        /// <returns></returns>
        [ActionName("Index")]
        public ActionResult Index()
        {
            var onlineUsers = AddOnlineUsers(CurrentUser);

            ViewData["ClassTitle"] = @UniEBoard.Service.Helpers.Configuration.WebSite.Current.TitleClasses;            
            ViewData["AssetTitle"] = EntityDisplayNames.Assets;            
            ViewData["Videos"] = _assetService.GetVideosAssetsByCompany(CurrentUser.CompanyId).Select(v => new SelectListItem { Value = v.Id.ToString(), Text = v.Name }).ToArray();
            ViewData["Docs"] = _assetService.GetDocumentAssetsByCompany(CurrentUser.CompanyId).Select(d => new SelectListItem { Value = d.Id.ToString(), Text = d.Name }).ToArray();
            ViewData["Assets"] = _assetService.GetAllAssetsByCompany(CurrentUser.CompanyId, 2, (List<String>)Session["tagAssetFilter"]);
            ViewData["Assignment"] = _assignmentTaskAndSubmissionService.GetAllAssignments().ToList().Select(d => new SelectListItem { Value = d.Id.ToString(), Text = d.Title }).ToArray();
            ViewData["Quizes"] = _quizAppService.GetAllQuizzes().ToList().Select(d => new SelectListItem { Value = d.Id.ToString(), Text = d.Title }).ToArray();
            ViewData["Units"] = _unitModuleAppService.GetUnitsForStaff(CurrentUser.CompanyId)
                .Where(u => u.PublishFrom.Value.Date.Equals(DateTime.Today)).OrderBy(u => u.PublishFrom).ToList();
            ViewData["Messages"] = _messageService.GetAllStudentMessages(CurrentUser.Id);
            ViewBag.ShowTime = true;
            ViewData["DurationList"] = _unitModuleAppService.GetCourseDurations();
            ViewData["OnlineUsers"] = onlineUsers;
            return View(CurrentUser);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult UnitListDashboardFilteredPartial(int unitId, string filter = "")
        {
            StaffViewModel staff = _staffService.GetStaffByMemberShipId(CurrentUser.Id);
            ViewData["Quizes"] = _quizAppService.GetAllQuizzes().ToList().Select(d => new SelectListItem { Value = d.Id.ToString(), Text = d.Title }).ToArray();
            List<UnitViewModel> units;
            if (filter.Equals(""))
            {
                units = _unitModuleAppService.GetUnitsForStaff(staff.Id)
                   .OrderBy(u => u.PublishFrom).ToList();
                ViewBag.ShowTime = false;
            }
            else
            {
                units = _unitModuleAppService.GetUnitsForStaff(staff.Id)
                    .Where(u => u.PublishFrom.Value.Date.Equals(Convert.ToDateTime(filter).Date)).OrderBy(u => u.PublishFrom).ToList();
                ViewBag.ShowTime = true;
            }
            ViewBag.Unit = unitId;
            ViewData["DurationList"] = _unitModuleAppService.GetCourseDurations();

            return PartialView("_DashboardScheduleUnitGridPartial", units);
        }

        #endregion

        #region Assignments

        ///// <summary>
        ///// GET: /Teacher/Assignments
        ///// </summary>
        ///// <returns></returns>
        [ActionName("Assignments")]
        public ActionResult Assignments()
        {
            //ViewBag.Filter = _assignmentTaskAndSubmissionService.GetAssignmentDisplayFilter("Assignments", "Student", "assignments");
            //ViewData["NoAssignmentDisplayLabel"] = "active ";
            //ViewData["StudentCourses"] = new SelectList(_courseModuleService.GetAllStudentCourses(CurrentUser.Id, false),"Id","Title","");
            //List<AssignmentSubmissionViewModel> models = _assignmentTaskAndSubmissionService.GetAllStudentAssignmentSubmissions(CurrentUser.Id, StudentAssignmentFilterType.Active,0);
            //return View("AssignmentSubmissions", models);

            //ViewData["AssignmentList"] = _assignmentTaskAndSubmissionService.GetAllAssignments();

            ViewData["TeacherCourses"] = new SelectList(_courseModuleService.GetAllCourses(), "Id", "Title", "");

            IEnumerable<ModuleViewModel> cModule = _courseModuleService.GetModulesForTeacher(CurrentUser.Id);
            ViewData["ModulesList"] = cModule;

            ViewData["CourseId"] = "";
            ViewData["TeacherModules"] = new SelectList(cModule, "Id", "Title", "");
           
            ViewData["Students"] = new SelectList(_studentService.GetAllStudents(), "Membership_Id", "FirstName", "");
            ViewData["Quiz"] = new SelectList(_quizAppService.GetAllQuizzesForTeacherCourses(CurrentUser.Id, 0), "Id", "Title", "");

            assignmentAssets = new List<AssetViewModel>();

            return View();
        }
        /// <summary>
        /// Creates the asset list that will then be attached to assignment before adding the assignment to the database ref: CreateAssignment method
        /// </summary>
        /// <param name="assetName">Asset name</param>
        /// <returns>A partial view of the list of all assets</returns>
        public ActionResult CreateAssignmentAssetList(string assetName)
        {
            List<string> assetNameList = new List<string>();
            AssetViewModel asset = _assetService.GetAssetByName(assetName);
            assignmentAssets.Add(asset);
            foreach (AssetViewModel model in assignmentAssets)
            {
                assetNameList.Add(model.Name);
            }
            return PartialView("_AssetFilterTagPartial", assetNameList);
        }

        /// <summary>
        /// Creates : /Teacher/Assignments
        /// </summary>
        /// <param name="assignmentViewModel">The assignment view model.</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult CreateAssignment(AssignmentViewModel assignmentViewModel)
        {
            if (ModelState.IsValid)
            {
                try
                {
                assignmentViewModel.PublishFrom = DateTime.Now;
                assignmentViewModel.PublishTo = DateTime.Now;

                _assignmentTaskAndSubmissionService.CreateAssignment(assignmentViewModel, CurrentUser.Id);
                assignmentViewModel.Assets = assignmentAssets;

                // Upload Asset
                //StaffViewModel staff = _staffService.GetStaffByMemberShipId(CurrentUser.Id);

                //CreateAssetViewModel assetViewModel = new CreateAssetViewModel();
                //assetViewModel.AssetType = 2;
                //assetViewModel.Name = assignmentViewModel.Title;
                //assetViewModel.UploadFile = assignmentViewModel.UploadFile;
                //_assetService.CreateAsset(assetViewModel, "~/App_Assets", staff.CompanyId);


                    UniEBoard.Helpers.StatusHelper.SuccessMessage("Assignment has been created successfully.", this);
                }
                catch (Exception ex)
                {
                    UniEBoard.Helpers.StatusHelper.ErrorMessage("An error occurred while creating new assignment. Please contact the administrator", this);
                }
                return RedirectToAction("Assignments");
            }
            else
            {
                ViewData["TeacherCourses"] = new SelectList(_courseModuleService.GetAllCourses(), "Id", "Title", "");

                IEnumerable<ModuleViewModel> cModule = _courseModuleService.GetModulesForTeacher(CurrentUser.Id);
                ViewData["ModulesList"] = cModule;

                ViewData["CourseId"] = "";
                ViewData["TeacherModules"] = new SelectList(cModule, "Id", "Title", "");

                ViewData["Students"] = new SelectList(_studentService.GetAllStudents(), "Membership_Id", "FirstName", "");
                ViewData["Quiz"] = new SelectList(_quizAppService.GetAllQuizzesForTeacherCourses(CurrentUser.Id, 0), "Id", "Title", "");

                return View("Assignments");                
            }
            
        }

        [HttpPost]
        public ActionResult FilterModuleByCourse(int? id)
        {
            ViewData["AssignmentList"] = _assignmentTaskAndSubmissionService.GetAllAssignments();
            ViewData["CourseId"] = (!id.HasValue ? "" : id.ToString());
            
            var modules = new List<ModuleViewModel>().AsEnumerable();
            if (id.HasValue)
                modules = _courseModuleService.GetModulesForTeacherByCourseId(CurrentUser.Id, id.Value);
            //else
            //    modules = _courseModuleService.GetModulesForTeacher(CurrentUser.Id);

            ViewData["ModulesList"] = modules.ToList();
            return PartialView("_AssignmentListPartial", ViewData["AssignmentList"]);
        }

        public ActionResult AutoCompleteModule(string term)
        {
            //string search = '';
            var json = Json((from module in _courseModuleService.GetModulesByTeacherId(CurrentUser.Id)
                             where (module.Title.ToLower().Contains(term.ToLower()))
                             select new { label = module.Title, id = module.Title }).ToArray(), JsonRequestBehavior.AllowGet);
            return json;
        }

        public ActionResult GetCourseModule(int CourseId)
        {
            var TeacherModules = new SelectList(_courseModuleService.GetModulesForTeacherByCourseId(CurrentUser.Id, CourseId).Where(m => m.Course_Id.Equals(CourseId)), "Id", "Title", "");

            return Json(TeacherModules, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region Courses

        /// <summary>
        /// Courseses this instance.
        /// </summary>
        /// <returns></returns>
        [ActionName("Courses")]
        public ActionResult Courses()
        {
            var displayFilter = new PageViewAllFilterViewModel(Url.Action("Courses", "Teacher"));
            return Courses(displayFilter);
        }

        /// <summary>
        /// Courseses this instance.
        /// </summary>
        /// <returns></returns>
        [ActionName("Courses")]
        [HttpPost]
        public ActionResult Courses(PageViewAllFilterViewModel pageViewFilterModel)
        {
            ViewData["Pager"] = pageViewFilterModel;
            ViewData["CourseList"] = _courseModuleService.GetCoursesWithDepartmentByStaffId(CurrentUser.Id, pageViewFilterModel.SelectedFilter).OrderBy(o => o.SortOrder);
            ViewData["DepartmentList"] = _courseModuleService.GetAllDepartments().Select(d => new SelectListItem { Value = d.Id.ToString(), Text = d.Name }).ToArray();
            
            return View();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult CourseListFilteredPartial(string filter)
        {
            var courses = _courseModuleService.GetCoursesWithDepartmentByStaffId(CurrentUser.Id, filter).OrderBy(o => o.SortOrder);
            ViewData["DepartmentList"] = _courseModuleService.GetAllDepartments().Select(d => new SelectListItem { Value = d.Id.ToString(), Text = d.Name }).ToArray();

            return PartialView("_CourseListPartial", courses);
        }

        /// <summary>
        /// Creates the course.
        /// </summary>
        /// <param name="courseViewModel">The course view model.</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult CreateCourse(CourseViewModel courseViewModel)
        {
            try
            {
            /*if (courseViewModel != null)
            {
                unitViewModel.StaffId = CurrentUser.Id;
                _unitModuleAppService.CreateUnit(unitViewModel);
            }
            return RedirectToAction("Units");*/
            if (ModelState.IsValid)
            {
                DiscussionViewModel discussionViewModel = new DiscussionViewModel();
                courseViewModel.OwnerId = courseViewModel.OwnerId == null ? CurrentUser.Id : courseViewModel.OwnerId;
                int? courseId = _courseModuleService.CreateCourseByStaff(courseViewModel, CurrentUser.Id);

                if (courseId.HasValue)
                {
                    discussionViewModel.CourseId = courseId.Value;
                    discussionViewModel.Description = courseViewModel.Overview;
                    discussionViewModel.Title = courseViewModel.Title;
                    _discussionAppService.CreateDiscussion(discussionViewModel);
                }

                    UniEBoard.Helpers.StatusHelper.SuccessMessage("Course has been created successfully.", this);

                return RedirectToAction("Courses");
            }

            ViewData["CourseList"] = _courseModuleService.GetCoursesWithDepartmentByStaffId(CurrentUser.Id, 10).OrderBy(o => o.SortOrder);
            ViewData["DepartmentList"] = _courseModuleService.GetAllDepartments().Select(d => new SelectListItem { Value = d.Id.ToString(), Text = d.Name }).ToArray();
            }
            catch (Exception ex)
            {
                UniEBoard.Helpers.StatusHelper.ErrorMessage("An error occurred while creating new course. Please contact the administrator", this);
            }

            return View("Courses");
        }

        //[HttpPost]
        [Authorize]
        public ActionResult RemoveCourse(int courseId)
        {
            _courseModuleService.CourseManager.RemoveCourse(courseId);
            return RedirectToAction("Courses");
        }

        /// <summary>
        /// Updates the name of the asset.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="assetId">The asset id.</param>
        [ActionName("UpdateCourse")]
        [HttpPost]
        public void UpdateCourse(string courseId, string name, string description, string departmentId, string courseLength, string publishFrom, string publishTo)
        {
            CourseViewModel course = _courseModuleService.GetCourseById(courseId.ToInteger());
            if (course != null)
            {
                int dId;
                DateTime pFrom, pTo;
                course.Title = name;
                course.Overview = description;
                if (int.TryParse(departmentId, out dId)) course.DepartmentId = dId;
                if (DateTime.TryParse(publishFrom, out pFrom)) course.PublishFrom = pFrom;
                if (DateTime.TryParse(publishTo, out pTo)) course.PublishTo = pTo;
                course.Length = courseLength;

                _courseModuleService.UpdateCourse(course);
            }
        }

        [HttpPost]
        public void UpdateCoursesOrder(List<string> draggedItemId/*, bool draggedDown, int prevIndex, int newIndex*/)
        {
            var itemIds = draggedItemId;
            UpdateCourseOrder(draggedItemId);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="courseId"></param>
        /// <param name="isEnabled"></param>
        [HttpPost]
        public void ActivateCourse(string courseId, bool isEnabled)
        {
            CourseViewModel courseViewModel = _courseModuleService.GetCourseById(courseId.ToInteger());
            courseViewModel.Approved = isEnabled;
            _courseModuleService.UpdateCourse(courseViewModel);

        }

        #endregion

        #region Modules

        /// <summary>
        /// Modules this instance.
        /// </summary>
        /// <returns></returns>
        [ActionName("Modules")]
        public ActionResult Modules()
        {
            var displayFilter = new PageViewAllFilterViewModel(Url.Action("Modules", "Teacher"));
            ViewData["Pager"] = displayFilter;

            ViewBag.StatusMessage = Convert.ToString(TempData["StatusMessage"]);

            var courseId = Request.QueryString["courseId"];
            return Modules(displayFilter, courseId);
        }

        /// <summary>
        /// Modules this instance.
        /// </summary>
        /// <returns></returns>
        [ActionName("Modules")]
        [HttpPost]
        public ActionResult Modules(PageViewAllFilterViewModel pageViewFilterModel, string courseId)
        {
            ViewData["Pager"] = pageViewFilterModel;
            ViewData["CourseId"] = (courseId == null ? "" : courseId.ToString());
            var modulesAssociatedToAndCreatedByTeacher = string.IsNullOrEmpty(courseId) ? _courseModuleService.GetModulesForTeacher(CurrentUser.Id)
                : _courseModuleService.GetModulesForTeacherByCourseId(CurrentUser.Id, int.Parse(courseId));

            ViewData["ModuleList"] = pageViewFilterModel.SelectedFilter != 0 ? modulesAssociatedToAndCreatedByTeacher.Take(pageViewFilterModel.SelectedFilter).OrderBy(o => o.SortOrder) : modulesAssociatedToAndCreatedByTeacher.OrderBy(o => o.SortOrder);



            Session["CourseList"] = _courseModuleService.GetCoursesWithDepartmentByStaffId(CurrentUser.Id, pageViewFilterModel.SelectedFilter)
                .Select(d => new SelectListItem { Value = d.Id.ToString(), Text = d.Title }).ToArray();
            //Drop Down List for Courses
            var courses = from CourseViewModel c in _courseModuleService.GetAllCourses()
                          select new { Id = c.Id, Name = c.Title };

            ViewBag.CoursesDropDownList = new SelectList(courses, "Id", "Name");
            //List<ModuleViewModel> moduleList = this._courseModuleService.GetModulesByTeacherId(CurrentUser.Id, pageViewFilterModel.SelectedFilter).ToList();
            //.GetCoursesWithDepartmentByStaffId(CurrentUser.Id, pageViewFilterModel.SelectedFilter);
            return View();
        }

        [HttpPost]
        public ActionResult ModuleByCourse(int? id, int filter)
        {
            var displayFilter = (PageViewAllFilterViewModel)ViewData["Pager"];
            ViewData["CourseId"] = (!id.HasValue ? "" : id.ToString());
            var modules = new List<ModuleViewModel>().AsEnumerable();
            if (id.HasValue)
                modules = _courseModuleService.GetModulesForTeacherByCourseId(CurrentUser.Id, id.Value).Take(filter);
            else
                modules = _courseModuleService.GetModulesForTeacher(CurrentUser.Id).Take(filter);
            ViewData["ModuleList"] = modules.ToList();
            return PartialView("_ModuleListPartial", modules);
        }

        /// <summary>
        /// Creates the module.
        /// </summary>
        /// <param name="unitViewModel">The unit view model.</param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult CreateModule(ModuleViewModel moduleViewModel)
        {

            var displayFilter = (PageViewAllFilterViewModel)ViewData["Pager"];
            var selectedCourse = moduleViewModel.Course_Id;
            if (ModelState.IsValid)
            {
                moduleViewModel.CreatedByStaff_Id = CurrentUser.Id;
                if (_courseModuleService.CreateModule(moduleViewModel))
                {
                    DiscussionViewModel discussion = _discussionAppService.GetCourseDiscussionsWithLatestPosts(selectedCourse).FirstOrDefault();
                    if (discussion != null)
                    {
                        TopicViewModel topicViewModel = PrepareTopicViewModel(moduleViewModel, discussion.Id);
                        _discussionAppService.AddTopic(topicViewModel);
                    }
                } else {
                    UniEBoard.Helpers.StatusHelper.ErrorMessage("An error occurred while creating new module. Please contact the administrator", this);
                }

                    UniEBoard.Helpers.StatusHelper.SuccessMessage("Module has been created successfully.", this);
                return RedirectToAction("Modules");
            }
            else
            {

                ViewData["ModuleList"] = _courseModuleService.GetModulesForTeacher(CurrentUser.Id).OrderBy(o => o.SortOrder);

                ViewData["CourseId"] = "";

                Session["CourseList"] = _courseModuleService.GetCoursesWithDepartmentByStaffId(CurrentUser.Id, 10)
                .Select(d => new SelectListItem { Value = d.Id.ToString(), Text = d.Title }).ToArray();

                //Drop Down List for Courses
                var courses = from CourseViewModel c in _courseModuleService.GetAllCourses()
                                select new { Id = c.Id, Name = c.Title };

                ViewBag.CoursesDropDownList = new SelectList(courses, "Id", "Name");

                return View("Modules");
                //return RedirectToAction("Modules", new { pageViewFilterModel = displayFilter, courseId = selectedCourse });
            }
        }

        public ActionResult RemoveModule(int moduleId)
        {
            // Set ModuleId = 0 in Assignment
            var assignmentsList = _assignmentTaskAndSubmissionService.GetAllAssignments().Where(x => x.ModuleId == moduleId).ToList();
            foreach (var assignment in assignmentsList)
            {
                assignment.ModuleId = null;
                _assignmentTaskAndSubmissionService.UpdateAssignment(assignment);
            }

            // Delete Module Quiz
            var moduleQuizList = _quizAppService.ModuleQuizManager.FindAll().Where(x => x.ModuleId == moduleId).ToList();
            foreach (var moduleQuiz in moduleQuizList)
            {
                _quizAppService.ModuleQuizManager.Remove(moduleQuiz.Id);
            }

            // Set Module = null in Unit
            var UnitList = _unitModuleAppService.UnitManager.FindAll().Where(u => u.ModuleId.Equals(moduleId)).ToList(); //.FindUnitsByModuleId(moduleId, 0).ToList();
            foreach (var unit in UnitList)
            {
                unit.ModuleId = null;
                _unitModuleAppService.UnitManager.Update(unit);
            }

            // Delete Course Modules
            _courseModuleService.RemoveModuleFromCourse(moduleId);

            // Delete Module
            _courseModuleService.ModuleManager.Remove(moduleId);

            return RedirectToAction("Modules");
        }

        /// <summary>
        /// Updates the name of the asset.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="assetId">The asset id.</param>
        public PartialViewResult UpdateModule(string moduleId, string name, string description,
             string publishFrom, string publishTo, string courseName)
        {
            ModuleViewModel module = _courseModuleService.GetModuleById(moduleId.ToInteger());
            CourseViewModel course = _courseModuleService.GetCourseByName(courseName);

            if (module != null)
            {
                DateTime pFrom, pTo;
                module.Title = name;
                module.Description = description;
                if (DateTime.TryParse(publishFrom, out pFrom))
                {
                    module.PublishFrom = pFrom;
                }
                if (DateTime.TryParse(publishTo, out pTo))
                {
                    module.PublishTo = pTo;
                }

                _courseModuleService.UpdateModule(module);

                if (course != null)
                {

                    CourseModuleViewModel courseModuleViewModel = new CourseModuleViewModel();
                    courseModuleViewModel.Module_Id = module.Id;
                    courseModuleViewModel.Course_Id = course.Id;

                    _courseModuleService.AddCourseModule(courseModuleViewModel);
                }
            }
            return PartialView("_CourseModuleTagPartial", _courseModuleService.GetCourseModulesByModule(moduleId.ToInteger()));
        }

        [HttpPost]
        public void UpdateModulesOrder(List<string> draggedItemId)
        {
            UpdateModuleOrder(draggedItemId);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="courseModuleId"></param>
        /// <param name="moduleId"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult AddCourseToModule(int courseModuleId, int moduleId)
        {
            //_courseModuleService.RemoveCourseFromModule(courseModuleId);
            return PartialView("_CourseModuleTagPartial", _courseModuleService.GetCourseModulesByModule(moduleId));
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="courseModuleId"></param>
        public ActionResult RemoveCourseFromModule(int courseModuleId, int moduleId)
        {
            _courseModuleService.RemoveCourseModule(courseModuleId);
            return PartialView("_CourseModuleTagPartial", _courseModuleService.GetCourseModulesByModule(moduleId));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="term"></param>
        /// <returns></returns>
        public ActionResult AutoCompleteCourses(string term)
        {
            //string search = '';
            var json = Json((from course in _courseModuleService.GetCoursesByStaff(CurrentUser.Id)
                             where (course.Title.ToLower().Contains(term.ToLower()))
                             select new { label = course.Title, id = course.Title }).ToArray(), JsonRequestBehavior.AllowGet);
            return json;
        }

        #endregion

        #region Classes

        [ActionName("Classes")]
        public ActionResult Classes()
        {
            List<ClassViewModel> couserList = this._unitModuleAppService.FindClassesByStaffId(CurrentUser.Id);
            return View(couserList);
        }


        /// <summary>
        /// Removes the video from class.
        /// </summary>
        /// <param name="videoId">The video id.</param>
        /// <param name="classId">The class id.</param>
        /// <returns></returns>
        [ActionName("RemoveVideoFromClass")]
        [HttpPost]
        public ActionResult RemoveVideoFromClass(int videoId, int classId)
        {

            if (this._unitModuleAppService.ReomveVideoFromUnit(classId))
            {
                return PartialView("_VideoClassPartial", null);
            }

            else
            {
                VideoViewModel video = _videoAppService.GetVideoById(videoId);
                ViewData["ClassVideoIdentifier"] = classId;
                return PartialView("_VideoClassPartial", video);
            }
        }

        /// <summary>
        /// Removes the assignment from class.
        /// </summary>
        /// <param name="assignmentId">The assignment id.</param>
        /// <param name="classId">The class id.</param>
        /// <returns></returns>
        [ActionName("RemoveAssignmentFromClass")]
        [HttpPost]
        public ActionResult RemoveAssignmentFromClass(int assignmentId, int classId)
        {


            if (this._unitModuleAppService.DeleteAssignmentFromunit(classId, assignmentId))
            {
                return PartialView("_AssignmentClassPartial", GetClassViewModel(classId).ListAsignment);
            }
            else
            {

                ViewData["ClassAssignmentIdentifier"] = classId;
                return PartialView("_AssignmentClassPartial", GetClassViewModel(classId).ListAsignment);
            }
        }

        /// <summary>
        /// Removes the document from class.
        /// </summary>
        /// <param name="documentId">The document id.</param>
        /// <param name="classId">The class id.</param>
        /// <returns></returns>
        [ActionName("RemoveDocumentFromClass")]
        [HttpPost]
        public ActionResult RemoveDocumentFromClass(int documentId, int classId)
        {

            if (this._unitModuleAppService.DeleteDocumentFromUnit(classId))
            {
                return PartialView("_DocumentClassPartial", null);
            }

            else
            {
                DocumentViewModel document = GetClassViewModel(classId).DocumentAssets;
                ViewData["ClassDocumentIdentifier"] = classId;
                return PartialView("_DocumentClassPartial", document);
            }
        }

        /// <summary>
        /// Removes the schedule from class.
        /// </summary>
        /// <param name="scheduleId">The schedule id.</param>
        /// <param name="classId">The class id.</param>
        /// <returns></returns>
        [ActionName("RemoveScheduleFromClass")]
        [HttpPost]
        public ActionResult RemoveScheduleFromClass(int scheduleId, int classId)
        {


            if (this._unitModuleAppService.RemoveScheduleFromunit(classId, scheduleId))
            {
                return PartialView("_ScheduleClassPartial", GetClassViewModel(classId).ListScheduleViewModel);
            }
            else
            {

                ViewData["ClassScheduleIdentifier"] = classId;
                return PartialView("_ScheduleClassPartial", GetClassViewModel(classId).ListScheduleViewModel);
            }
        }

        #endregion

        #region Units

        /// <summary>
        /// The specified Unit page view filter model.
        /// </summary>
        /// <param name="pageViewFilterModel">The page view filter model.</param>
        /// <returns></returns>
        [ActionName("Units")]
        public ActionResult Units()
        {

            var displayFilter = new PageViewAllFilterViewModel(Url.Action("Units", "Teacher"));
            ViewBag.StatusMessage = Convert.ToString(TempData["StatusMessage"]);
            return Units(displayFilter);
        }


        /// <summary>
        /// The specified Unit page view filter model.
        /// </summary>
        /// <param name="pageViewFilterModel">The page view filter model.</param>
        /// <returns></returns>
        [ActionName("Units")]
        [HttpPost]
        public ActionResult Units(PageViewAllFilterViewModel pageViewFilterModel)
        {
            //StaffViewModel staff = _staffService.GetStaffByMemberShipId(CurrentUser.Id);
            var selectedModule = Session["SelectedModule"];
            ViewData["AvailableModules"] = _courseModuleService.GetModulesByTeacher(CurrentUser.Id);
            ViewData["Pager"] = pageViewFilterModel;
            ViewData["Videos"] = _assetService.GetVideosAssetsByCompany(CurrentUser.CompanyId).Select(v => new SelectListItem { Value = v.Id.ToString(), Text = v.Name }).ToArray();
            ViewData["Docs"] = _assetService.GetDocumentAssetsByCompany(CurrentUser.CompanyId).Select(d => new SelectListItem { Value = d.Id.ToString(), Text = d.Name }).ToArray();
            
            //ViewData["Units"] = _unitModuleAppService.FindUnitsByStaffId(CurrentUser.Id, pageViewFilterModel.SelectedFilter);
            ViewData["Units"] = selectedModule == null ? _unitModuleAppService.GetUnitsForStaff(CurrentUser.CompanyId) : _unitModuleAppService.GetUnitsForStaff(CurrentUser.CompanyId).Where(u => u.ModuleId.Equals(selectedModule));
            ViewData["DurationList"] = _unitModuleAppService.GetCourseDurations();
            
            ViewData["Assignment"] = _assignmentTaskAndSubmissionService.GetAllAssignments().ToList().Select(d => new SelectListItem { Value = d.Id.ToString(), Text = d.Title }).ToArray();

            ViewData["Quizes"] = _quizAppService.GetAllQuizzes().ToList().Select(d => new SelectListItem { Value = d.Id.ToString(), Text = d.Title }).ToArray();

            ViewBag.ShowTime = false;


            // modules dropdown list
            
            ViewData["ModuleList"] = _courseModuleService
                                        .GetModulesForTeacher(CurrentUser.Id)
                                        .Select(m => new SelectListItem { Value = m.Id.ToString(), Text = m.Title, Selected =  (selectedModule != null && m.Id.Equals(selectedModule)) ? true : false });
            Session["SelectedModule"] = null;
            return View();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult UnitListFilteredPartial(string filter)
        {
            StaffViewModel staff = _staffService.GetStaffByMemberShipId(CurrentUser.Id);
            
            ViewData["Videos"] = _assetService.GetVideosAssetsByCompany(staff.CompanyId).Select(v => new SelectListItem { Value = v.Id.ToString(), Text = v.Name }).ToArray();
            ViewData["Docs"] = _assetService.GetDocumentAssetsByCompany(staff.CompanyId).Select(d => new SelectListItem { Value = d.Id.ToString(), Text = d.Name }).ToArray();
            ViewData["Quizes"] = _quizAppService.GetAllQuizzes().ToList().Select(d => new SelectListItem { Value = d.Id.ToString(), Text = d.Title }).ToArray();

            var units = _unitModuleAppService.GetUnitsForStaff(staff.Id).Where(u => u.Title.ToLower().Contains(filter.ToLower()));
            return PartialView("_UnitsListPartial", units);
        }

        /// <summary>
        /// Creates the unit.
        /// </summary>
        /// <param name="unitViewModel">The unit view model.</param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult CreateUnit(UnitViewModel unitViewModel)
        {
            if (ModelState.IsValid)
            {
                unitViewModel.StaffId = CurrentUser.Id;
                if (_unitModuleAppService.CreateUnit(unitViewModel))
                {
                    ModuleViewModel module = _courseModuleService.GetModuleById(unitViewModel.ModuleId.Value);
                    if (module != null)
                    {
                        DiscussionViewModel discussionViewModel = _discussionAppService.GetCourseDiscussionsWithLatestPosts(module.Course_Id).FirstOrDefault();
                        if (discussionViewModel != null)
                        {
                            TopicViewModel topic = PrepareTopicViewModel(unitViewModel, discussionViewModel.Id);
                            _discussionAppService.AddTopic(topic);
                        }
                    }
                    UniEBoard.Helpers.StatusHelper.SuccessMessage("Class has been created successfully.", this);
                }
                else
                {
                    UniEBoard.Helpers.StatusHelper.ErrorMessage("An error occurred while creating new class. Please contact the administrator", this);
                }
                // selected module for the unit just created. This is going to be used when the Units are loaded again
                Session["SelectedModule"] = unitViewModel.ModuleId.Value;
                return RedirectToAction("Units");
            }
            else 
            {
                StaffViewModel staff = _staffService.GetStaffByMemberShipId(CurrentUser.Id);
                ViewData["AvailableModules"] = _courseModuleService.GetModulesByTeacher(CurrentUser.Id);
                //ViewData["Pager"] = 10;
                ViewData["Videos"] = _assetService.GetVideosAssetsByCompany(staff.CompanyId).Select(v => new SelectListItem { Value = v.Id.ToString(), Text = v.Name }).ToArray();
                ViewData["Docs"] = _assetService.GetDocumentAssetsByCompany(staff.CompanyId).Select(d => new SelectListItem { Value = d.Id.ToString(), Text = d.Name }).ToArray();

                //ViewData["Units"] = _unitModuleAppService.FindUnitsByStaffId(CurrentUser.Id, pageViewFilterModel.SelectedFilter);
                ViewData["Units"] = _unitModuleAppService.GetUnitsForStaff(staff.Id);
                ViewData["DurationList"] = _unitModuleAppService.GetCourseDurations();

                ViewData["Assignment"] = _assignmentTaskAndSubmissionService.GetAllAssignments().ToList().Select(d => new SelectListItem { Value = d.Id.ToString(), Text = d.Title }).ToArray();
                ViewData["Quizes"] = _quizAppService.GetAllQuizzes().ToList().Select(d => new SelectListItem { Value = d.Id.ToString(), Text = d.Title }).ToArray();

                ViewBag.ShowTime = false;

                // modules dropdown list
                ViewData["ModuleList"] = _courseModuleService.GetModulesForTeacher(CurrentUser.Id).Select(m => new SelectListItem { Value = m.Id.ToString(), Text = m.Title }); 
                
                
                return View("Units");
            }

            
            
        }

        /// <summary>
        /// Updates the name of the asset.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="assetId">The asset id.</param>
        [ActionName("UpdateUnit")]
        [HttpPost]
        public void UpdateUnit(string name, string unitId, string videoId, string documentId, string assignmentId, string quizId)
        {
            UnitViewModel unit = _unitModuleAppService.GetUnit(unitId.ToInteger());
            if (unit != null)
            {
                unit.Title = name;
                unit.VideoId = videoId.ToNullableInteger();
                unit.DocumentId = documentId.ToNullableInteger();
                unit.AssignmentId = assignmentId.ToInteger();
                unit.QuizId = quizId.ToInteger();
                _unitModuleAppService.UpdateUnit(unit);
            }

            /*AssignmentViewModel assg = _assignmentTaskAndSubmissionService.GetAssignment(assignmentId.ToInteger());
            if (assg != null)
            {
                assg.UnitId = unitId.ToInteger();
                _assignmentTaskAndSubmissionService.UpdateAssignment(assg);
            }*/
        }

        /// <summary>
        /// Updates the name of the asset.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="assetId">The asset id.</param>
        [ActionName("UpdateUnitDashboard")]
        [HttpPost]
        public void UpdateUnitDashboard(string name, string description, string publishFrom, string duration, string unitId, string quizId)
        {
            UnitViewModel unit = _unitModuleAppService.GetUnit(unitId.ToInteger());
            Double durationAdd = (Double.Parse(duration) / 2) * 60;

            if (unit != null)
            {
                unit.Title = name;
                unit.Description = description;
                unit.PublishFrom = Convert.ToDateTime(publishFrom);
                //unit.PublishTo = ((DateTime)unit.PublishFrom).AddMinutes(durationAdd);
                unit.Duration = Int32.Parse(duration);
                if (!String.IsNullOrEmpty(quizId))
                {
                    unit.QuizId = quizId.ToInteger();
                }
                
                _unitModuleAppService.UpdateUnit(unit);
            }
        }

        [HttpPost]
        public ActionResult UnitsByModule(int? id, int filter)
        {
            // TODO: Display filter is not implemented on Ajax response. 
            var displayFilter = (PageViewAllFilterViewModel)ViewData["Pager"];
            /////////////////////
            StaffViewModel staff = _staffService.GetStaffByMemberShipId(CurrentUser.Id);
            ViewData["AvailableModules"] = _courseModuleService.GetModulesByTeacher(CurrentUser.Id);
            ViewData["Videos"] = _assetService.GetVideosAssetsByCompany(staff.CompanyId).Select(v => new SelectListItem { Value = v.Id.ToString(), Text = v.Name }).ToArray();
            ViewData["Docs"] = _assetService.GetDocumentAssetsByCompany(staff.CompanyId).Select(d => new SelectListItem { Value = d.Id.ToString(), Text = d.Name }).ToArray();
            ViewData["Assignment"] = _assignmentTaskAndSubmissionService.GetAllAssignments().ToList().Select(d => new SelectListItem { Value = d.Id.ToString(), Text = d.Title }).ToArray();
            /////////////////////

            ViewData["DurationList"] = _unitModuleAppService.GetCourseDurations();
            ViewData["ModuleId"] = (!id.HasValue ? "" : id.ToString());
            IEnumerable<UnitViewModel> units = new List<UnitViewModel>().AsEnumerable();
            if (id.HasValue)
                units = _unitModuleAppService.GetUnitsForStaff(CurrentUser.Id).ToList().Where(m => m.ModuleId == id);
            else
                units = _unitModuleAppService.GetUnitsForStaff(CurrentUser.Id).ToList();
            ViewData["Units"] = units.ToList();
            //ViewData["Quizes"] = _quizAppService.GetAllQuizzes().ToList().Select(d => new SelectListItem { Value = d.Id.ToString(), Text = d.Title }).ToArray();

            ViewData["Quizes"] = _quizAppService.GetAllQuizzes().ToList().Select(d => new SelectListItem { Value = d.Id.ToString(), Text = d.Title }).ToArray();
            ViewBag.ShowTime = false;
            ViewData["ajaxCall"] = true;
            //return PartialView("_UnitsListPartial", units);
            return PartialView("_DashboardScheduleUnitGridPartial", units);
        }

        //[HttpPost]
        [Authorize]
        public ActionResult RemoveUnit(int unitId)
        {
            // Set  UnitId = 0 in Assignment
            var assignmentsList = _assignmentTaskAndSubmissionService.GetAllAssignments().Where(x => x.UnitId == unitId).ToList();
            foreach (var assignment in assignmentsList)
            {
                assignment.UnitId = 0;
                _assignmentTaskAndSubmissionService.UpdateAssignment(assignment);
            }

            // Delete Schedule
            var schedules = _scheduleAppService.ScheduleManager.FindAll().Where(x => x.UnitId == unitId).ToList();
            foreach (var item in schedules)
            {
                _scheduleAppService.ScheduleManager.Remove(item.Id);
            }

            // Delete Unit
            _unitModuleAppService.UnitManager.Remove(unitId);

            return RedirectToAction("Units");
        }
        #endregion

        #region Quizzes

        /// <summary>
        /// Quizzeses this instance.
        /// </summary>
        /// <returns></returns>
        [ActionName("Quizzes")]
        public ActionResult Quizzes()
        {
            var displayFilter = new PageViewAllFilterViewModel(Url.Action("Quizzes", "Teacher"));

            //Drop Down List for QuizDisplayEndResultsOptions
            var values = from QuizDisplayEndResultsOptions e in Enum.GetValues(typeof(QuizDisplayEndResultsOptions))
                         select new { Id = (int)e, Name = e };
            ViewBag.QuizDisplayEndResultsOptions = new SelectList(values, "Id", "Name");

            ViewBag.StatusMessage = Convert.ToString(TempData["StatusMessage"]);

            ViewData["AvailableModules"] = _courseModuleService.GetModulesForTeacher(CurrentUser.Id).Select(m => new SelectListItem() { Text = m.Title, Value = m.Id.ToString() }).ToList();
            ViewData["Pager"] = displayFilter;
            ViewData["Quizzes"] = _quizAppService.GetAllQuizzesForTeacherCourses(CurrentUser.Id, displayFilter.SelectedFilter);
            return View();
        }

        /// <summary>
        /// Quizzeses this instance.
        /// </summary>
        /// <returns></returns>
        [ActionName("Quizzes")]
        [HttpPost]
        public ActionResult Quizzes(PageViewAllFilterViewModel pageViewFilterModel)
        {
            //Drop Down List for QuizDisplayEndResultsOptions
            var values = from QuizDisplayEndResultsOptions e in Enum.GetValues(typeof(QuizDisplayEndResultsOptions))
                         select new { Id = (int)e, Name = e };
            ViewBag.QuizDisplayEndResultsOptions = new SelectList(values, "Id", "Name");

            ViewData["AvailableModules"] = _courseModuleService.GetModulesByTeacher(CurrentUser.Id);
            ViewData["Pager"] = pageViewFilterModel;
            ViewData["Quizzes"] = _quizAppService.GetAllQuizzesForTeacherCourses(CurrentUser.Id, pageViewFilterModel.SelectedFilter);
            return View();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult QuizListFilteredPartial(string filter)
        {
            var quizzes = _quizAppService.GetAllQuizzesForTeacherCourses(CurrentUser.Id, filter);
            return PartialView("_QuizListPartial", quizzes);
        }

        /// <summary>
        /// Creates the quiz.
        /// </summary>
        /// <param name="createQuizViewModel">The create quiz view model.</param>
        /// <returns></returns>
        [ActionName("CreateQuiz")]
        [HttpPost]
        //[AcceptVerbs(HttpVerbs.Post)]
        public ActionResult CreateQuiz(QuizzesViewModel createQuizViewModel)
        {
            if (ModelState.IsValid)
            {
                try
                {
                createQuizViewModel.DisplayEndResults = QuizDisplayEndResultsOptions.ShowAnswerAfterQuestion;
                createQuizViewModel.CorrectUserChoices = true;
                createQuizViewModel.Deadline = createQuizViewModel.PublishTo;
                _quizAppService.CreateQuiz(createQuizViewModel);

                    UniEBoard.Helpers.StatusHelper.SuccessMessage("Quiz has been created successfully.", this);
                }
                catch (Exception ex)
                {
                    UniEBoard.Helpers.StatusHelper.ErrorMessage("An error occurred while creating new quiz. Please contact the administrator", this);
                }
                return RedirectToAction("Quizzes");
            }
            else
            {
                //Drop Down List for QuizDisplayEndResultsOptions
                var values = from QuizDisplayEndResultsOptions e in Enum.GetValues(typeof(QuizDisplayEndResultsOptions))
                             select new { Id = (int)e, Name = e };
                ViewBag.QuizDisplayEndResultsOptions = new SelectList(values, "Id", "Name");

                ViewData["AvailableModules"] = _courseModuleService.GetModulesByTeacher(CurrentUser.Id);
                //ViewData["Pager"] = pageViewFilterModel;
                ViewData["Quizzes"] = _quizAppService.GetAllQuizzesForTeacherCourses(CurrentUser.Id, 10);
                return View("Quizzes");
            }
            
        }

        //[HttpPost]
        [Authorize]
        public ActionResult RemoveQuiz(int quizId)
        {
            // Set QuizId = 0 in Assignment
            var assignmentsList = _assignmentTaskAndSubmissionService.GetAllAssignments().Where(x => x.QuizId == quizId).ToList();
            foreach (var assignment in assignmentsList)
            {
                assignment.QuizId = null;
                _assignmentTaskAndSubmissionService.UpdateAssignment(assignment);
            }

            // Delete Questions and related Answers & Choices
            var quizQuestionsList = _questionAppService.GetQuestionsByQuizId(quizId).ToList();
            foreach (var ques in quizQuestionsList)
            {
                RemoveQuestion(ques.Id);
            }

            // Delete Quiz Entry
            var quizEntryList = _quizAppService.GetQuizEntry(quizId);
            foreach (var quizEntry in quizEntryList)
            {
                _quizAppService.QuizEntryManager.Remove(quizEntry.Id);
            }

            // Delete Module Quiz
            var moduleQuizList = _quizAppService.GetModuleQuizs(quizId).ToList();
            foreach (var moduleQuiz in moduleQuizList)
            {
                _quizAppService.ModuleQuizManager.Remove(moduleQuiz.Id);
            }

            // Delete Quiz
            _quizAppService.QuizManager.Remove(quizId);

            return RedirectToAction("Quizzes");
        }

        #endregion

        #region QuizQuestions

        /// <summary>
        /// Quizzeses this instance.
        /// </summary>
        /// <returns></returns>
        [ActionName("QuizQuestions")]
        public ActionResult QuizQuestionsForTeacher(int quizId)
        {
            var model = new QuestionViewModel();
            ViewData["QuestionChoices"] = new QuestionChoicesViewModel(true);

            var displayFilter = new PageViewAllFilterViewModel(Url.Action("QuizQuestions", "Teacher", new { quizId = quizId }));
            ViewData["QuestionList"] = _questionAppService.GetQuizQuestionsByTeacherAndQuiz(CurrentUser.Id, quizId, displayFilter.SelectedFilter).OrderBy(o => o.SortOrder);
            ViewData["Pager"] = displayFilter;

            //Set Value for QuizId for hidden field
            ViewBag.QuizId = quizId;

            //Drop Down List for AnswerType
            model.QuestionQuizTypeChoices = new SelectList(from QuestionQuizType e in Enum.GetValues(typeof(QuestionQuizType))
                                                           select new { Id = (int)e, Name = e }, "Id", "Name");

            // List of QuestionChoices
            model.QuestionChoices.Add(new QuestionChoicesViewModel());
            model.QuestionChoices.Add(new QuestionChoicesViewModel());
            model.QuestionChoices.Add(new QuestionChoicesViewModel());
            model.QuestionChoices.Add(new QuestionChoicesViewModel());

            return View(model);
        }

        /// <summary>
        /// Quizzeses this instance.
        /// </summary>
        /// <returns></returns>
        [ActionName("CreateQuizQuestion")]
        [HttpPost]
        public ActionResult CreateQuizQuestion(QuestionViewModel question)
        {
            if (true)
            {
                try
                {
                question.QuestionType_Id = (QuestionQuizType)question.QuestionQuizTypeSelected;
                List<QuestionChoicesViewModel> questionChoices = new List<QuestionChoicesViewModel>();
                foreach (var questionChoice in question.QuestionChoices)
                {
                    if (!string.IsNullOrEmpty(questionChoice.Name))
                        questionChoices.Add(questionChoice);
                }
                question.QuestionChoices = questionChoices;
                if (question != null)
                {
                    _questionAppService.CreateQuestion(question);
                        UniEBoard.Helpers.StatusHelper.SuccessMessage("Question has been created successfully.", this);
                    }
                }
                catch (Exception ex)
                {
                    UniEBoard.Helpers.StatusHelper.ErrorMessage("An error occurred while creating new question. Please contact the administrator", this);
                }

                //return RedirectToAction("QuizQuestions", new { quizId = question.Quiz_Id });
                return RedirectToAction("QuizQuestions", new { quizId = question.Quiz_Id });
            }
            else
            {
                ViewData["QuestionChoices"] = new QuestionChoicesViewModel(true);

                var displayFilter = new PageViewAllFilterViewModel(Url.Action("QuizQuestions", "Teacher", new { quizId = question.Quiz_Id }));
                //ViewBag.Questions = _questionAppService.GetQuizQuestionsByTeacherAndQuiz(CurrentUser.Id, quizId, displayFilter.SelectedFilter).OrderBy(o => o.SortOrder);
                ViewData["QuestionList"] = _questionAppService.GetQuizQuestionsByTeacherAndQuiz(CurrentUser.Id, question.Quiz_Id, displayFilter.SelectedFilter).OrderBy(o => o.SortOrder);
                //ViewData["Pager"] = displayFilter;

                //Set Value for QuizId for hidden field
                ViewBag.QuizId = question.Quiz_Id;

                //Drop Down List for AnswerType
                question.QuestionQuizTypeChoices = new SelectList(from QuestionQuizType e in Enum.GetValues(typeof(QuestionQuizType)) select new { Id = (int)e, Name = e }, "Id", "Name");

                //question.QuestionQuizTypeSelected = Convert.ToInt32(question.QuestionType_Id);
                /*
                var values = from QuestionQuizType e in Enum.GetValues(typeof(QuestionQuizType))
                             select new { Id = (int)e, Name = e };
                ViewBag.QuestionQuizType = new SelectList(values, "Id", "Name");
                */

               
                
                return View("QuizQuestions");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [ActionName("QuizQuestionEdit")]
        public ActionResult EditQuizQuestion(int questionId)
        {
            QuestionViewModel question = _questionAppService.GetQuestionById(questionId);

            //Set Value for QuizId for hidden field
            ViewBag.QuizId = question.Quiz_Id;

            //Drop Down List for AnswerType
            question.QuestionQuizTypeChoices = new SelectList(from QuestionQuizType e in Enum.GetValues(typeof(QuestionQuizType))
                                                           select new { Id = (int)e, Name = e }, "Id", "Name");

            question.QuestionQuizTypeSelected = Convert.ToInt32(question.QuestionType_Id);
            /*
            var values = from QuestionQuizType e in Enum.GetValues(typeof(QuestionQuizType))
                         select new { Id = (int)e, Name = e };
            ViewBag.QuestionQuizType = new SelectList(values, "Id", "Name");
             */ 

            return View(question);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult EditQuizQuestion(QuestionViewModel question)
        {
            if (question != null)
            {
                question.QuestionType_Id = (QuestionQuizType)question.QuestionQuizTypeSelected;
                _questionAppService.EditQuestion(question);
            }

            return RedirectToAction("QuizQuestions", new { quizId = question.Quiz_Id });
        }

        /// <summary>
        /// Blanks the question choice.
        /// </summary>
        /// <param name="quizId">The quiz id.</param>
        /// <param name="pageViewFilterModel">The page view filter model.</param>
        /// <returns></returns>
        [ActionName("BlankQuestionChoice")]
        [HttpPost]
        public ActionResult BlankQuestionChoice(int questionChoiceNumber)
        {
            ViewBag.QuestionChoiceNumber = questionChoiceNumber;

            return PartialView("_QuestionChoicePartial");
        }


        //[HttpPost]
        [Authorize]
        public ActionResult RemoveQuizQuestion(int questionId)
        {
            QuestionViewModel question = _questionAppService.GetQuestionById(questionId);
            int Quiz_Id = question.Quiz_Id;

            RemoveQuestion(questionId);

            return RedirectToAction("QuizQuestions", new { quizId = Quiz_Id });
        }
        protected void RemoveQuestion(int questionId)
        {
            var qChoiceList = _questionAppService.QuestionChoicesManager.FindAll().Where(x => x.Question_Id == questionId).ToList();
            foreach (var qChoice in qChoiceList)
            {
                // Delete AnswerQuesChoice
                var AnsQuesChoiceList = _answerAppService.AnswerQuestionChoiceManager.FindAll().Where(x => x.QuestionChoiceId == qChoice.Id).ToList();
                foreach (var ansQuesChoice in AnsQuesChoiceList)
                {
                    _answerAppService.AnswerQuestionChoiceManager.Remove(ansQuesChoice.Id);
                }

                // Delete QuesChoice
                _questionAppService.QuestionChoicesManager.Remove(qChoice.Id);
            }

            // Delete Answer
            var answerList = _answerAppService.AnswerManager.FindAll().Where(x => x.Question_Id == questionId).ToList();
            foreach (var answer in answerList)
            {
                _answerAppService.AnswerManager.Remove(answer.Id);
            }

            // Delete Ques
            _questionAppService.QuestionManager.RemoveQuestion(questionId);
        }

        #endregion

        #region Grades

        /// <summary>
        /// Gradeses this instance.
        /// </summary>
        /// <returns></returns>
        [ActionName("Grades")]
        public ActionResult Grades()
        {
            //Initializing Drop down lists
            var coursesDropList = from c in _courseModuleService.GetCoursesByStaff(CurrentUser.Id)
                                  select new { Id = c.Id, Name = c.Title };
            var modulesDropList = from c in _courseModuleService.GetModulesForTeacher(CurrentUser.Id)
                                  select new { Id = c.Id, Name = c.Title };
            var assignmentsDropList = from c in _courseModuleService.GetAssignmentsForTeacher(CurrentUser.Id)
                                      select new { Id = c.Id, Name = c.Title };
            var studentsDropList = from c in _courseModuleService.GetStudentsForTeacher(CurrentUser.Id)
                                   select new { Id = c.Id, Name = c.FullName };

            ViewData["CoursesDropList"] = new SelectList(coursesDropList, "Id", "Name");            
            ViewData["ModulesDropList"] = new SelectList(modulesDropList, "Id", "Name");            
            ViewData["AssignmentsDropList"] = new SelectList(assignmentsDropList, "Id", "Name");                           
            ViewData["StudentsDropList"] = new SelectList(studentsDropList, "Id", "Name");

            // Initializing Model
            List<AssignmentViewModel> assigmentsWithSubmissions = _courseModuleService.GetAssignmentsForTeacher(CurrentUser.Id, true);

            ViewData["ajaxCall"] = false;

            return View(assigmentsWithSubmissions);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult RefreshingSubmissionList(string course, string module, string assignment, string student, string filter)
        {
            // Initializing Model
            IEnumerable<AssignmentViewModel> assigmentsWithSubmissions = _courseModuleService.GetAssignmentsForTeacher(CurrentUser.Id, true);

            if (!String.IsNullOrEmpty(course))
            {
                assigmentsWithSubmissions = assigmentsWithSubmissions.Where(a => a.CourseId == Int32.Parse(course)).ToList();
            }

            if (!String.IsNullOrEmpty(module))
            {
                assigmentsWithSubmissions = assigmentsWithSubmissions.Where(a => a.ModuleId == Int32.Parse(module)).ToList();
            }

            if (!String.IsNullOrEmpty(assignment))
            {
                assigmentsWithSubmissions = assigmentsWithSubmissions.Where(a => a.Id == Int32.Parse(assignment)).ToList();
            }

            if (!String.IsNullOrEmpty(student))
            {
                
                foreach (var assign in assigmentsWithSubmissions)
                {
                    List<UniEBoard.Service.Models.SubmissionViewModel> subsToRemove = new List<UniEBoard.Service.Models.SubmissionViewModel>();
                    foreach (var sub in assign.Submissions)
                    {
                        if (sub.StudentId != Int32.Parse(student))
                            subsToRemove.Add(sub);
                    }
                    foreach (var sub in subsToRemove)
                    {
                        assign.Submissions.Remove(sub);
                    }
                }

                
            }

            //************** FILTERS ********************
            switch (filter)
            {
                case "All":
                    break;
                case "Submitted":
                    foreach (var assign in assigmentsWithSubmissions)
                    {
                        List<UniEBoard.Service.Models.SubmissionViewModel> subsToRemove = new List<UniEBoard.Service.Models.SubmissionViewModel>();
                        foreach (var sub in assign.Submissions)
                        {
                            if (sub.Status == 0 || sub.GradePointValue != null)
                                subsToRemove.Add(sub);
                        }
                        foreach (var sub in subsToRemove)
                        {
                            assign.Submissions.Remove(sub);
                        }
                    }
                    break;
                case "NotSubmitted":
                    foreach (var assign in assigmentsWithSubmissions)
                    {
                        List<UniEBoard.Service.Models.SubmissionViewModel> subsToRemove = new List<UniEBoard.Service.Models.SubmissionViewModel>();
                        foreach (var sub in assign.Submissions)
                        {
                            if (sub.Status != 0)
                                subsToRemove.Add(sub);
                        }
                        foreach (var sub in subsToRemove)
                        {
                            assign.Submissions.Remove(sub);
                        }                       
                    }
                    break;
                case "Marked":
                    foreach (var assign in assigmentsWithSubmissions) 
                    {

                        List<UniEBoard.Service.Models.SubmissionViewModel> subsToRemove = new List<UniEBoard.Service.Models.SubmissionViewModel>();
                        foreach (var sub in assign.Submissions) 
                        {
                            if (sub.GradePointValue == null)
                                subsToRemove.Add(sub);
                                //assign.Submissions.Remove(sub);
                        }

                        foreach (var sub in subsToRemove) 
                        {
                            assign.Submissions.Remove(sub);
                        }
                    
                    }


                    break;
            }

            return PartialView("_GradeSubmissionsListPartial", assigmentsWithSubmissions);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult RefreshingSubmissionFilters(string course, string module, string assignment, string student, string filter)
        {

            IEnumerable<CourseViewModel> courses = _courseModuleService.GetCoursesByStaff(CurrentUser.Id);
            IEnumerable<ModuleViewModel> modules = _courseModuleService.GetModulesForTeacher(CurrentUser.Id);
            IEnumerable<AssignmentViewModel> assignments = _courseModuleService.GetAssignmentsForTeacher(CurrentUser.Id);
            IEnumerable<StudentViewModel> students = _courseModuleService.GetStudentsForTeacher(CurrentUser.Id);

            IEnumerable<CourseViewModel> coursesOriginal = _courseModuleService.GetCoursesByStaff(CurrentUser.Id);
            IEnumerable<ModuleViewModel> modulesOriginal = _courseModuleService.GetModulesForTeacher(CurrentUser.Id);
            IEnumerable<AssignmentViewModel> assignmentsOriginal = _courseModuleService.GetAssignmentsForTeacher(CurrentUser.Id);
            IEnumerable<StudentViewModel> studentsOriginal = _courseModuleService.GetStudentsForTeacher(CurrentUser.Id);

            // updating lists
            if (!String.IsNullOrEmpty(course))
            {
                CourseViewModel courseViewModel = courses.First(c => c.Id == Int32.Parse(course));
                
                // assignments
                assignments = assignments.Where(a => a.CourseId == Int32.Parse(course));
                
                //modules
                modules = modules.Where(m => m.Course_Id == Int32.Parse(course));                
                
                //students
                List<StudentViewModel> studentsSearch = new List<StudentViewModel>();              
                foreach (var courseRegistration in courseViewModel.CourseRegistrations)
                {
                    studentsSearch.Add(courseRegistration.Student);
                }
                students = studentsSearch;
               
            }

            if (!String.IsNullOrEmpty(module))
            {
                ModuleViewModel moduleViewModel = modulesOriginal.First(m => m.Id == Int32.Parse(module));

                //assignments
                assignments = assignments.Where(a => a.ModuleId == Int32.Parse(module));
                
                //course
                courses = moduleViewModel.CourseModules.Select(cm => cm.Course);

                //students
                List<StudentViewModel> studentsSearch = new List<StudentViewModel>();
                //students1 = courses.ToList().ForEach(c => c.CourseRegistrations.ToList().ForEach(cr => students1.Add(cr.Student)));
                foreach (var courseViewModel in courses) 
                { 
                    foreach (var courseRegistration in courseViewModel.CourseRegistrations)
                    {
                        studentsSearch.Add(courseRegistration.Student);
                    }
                    
                }

                students = studentsSearch;
            }

            if (!String.IsNullOrEmpty(assignment))
            {
                ModuleViewModel moduleViewModel = new ModuleViewModel();
                if (!String.IsNullOrEmpty(module))
                {
                    moduleViewModel = modulesOriginal.First(m => m.Id == Int32.Parse(module));
                }

                AssignmentViewModel assignmentViewModel = assignments.FirstOrDefault(a => a.Id == Int32.Parse(assignment));

                //courses
                if (!String.IsNullOrEmpty(module))
                {
                    var coursesSearch = from cm in moduleViewModel.CourseModules
                              where (cm.Module_Id == moduleViewModel.Id)
                              select cm.Course;
                    courses = coursesSearch;
                }
                //modules
                if (!String.IsNullOrEmpty(module))
                {
                    List<ModuleViewModel> modulesSearch = new List<ModuleViewModel>();
                    modulesSearch.Add(moduleViewModel);
                    modules = modulesSearch;
                }

                //students
                if (!String.IsNullOrEmpty(module))
                {
                    List<StudentViewModel> studentsSearch = new List<StudentViewModel>();
                    foreach (var cm in moduleViewModel.CourseModules)
                    {
                        foreach (var cr in cm.Course.CourseRegistrations)
                        {
                            studentsSearch.Add(cr.Student);
                        }
                    }

                    students = studentsSearch;
                }
            }


            if (!String.IsNullOrEmpty(student))
            {
                StudentViewModel studentViewModel = studentsOriginal.First(s => s.Id == Int32.Parse(student));

                //course
                List<CourseViewModel> coursesSearch = new List<CourseViewModel>();
                foreach (var cr in studentViewModel.CourseRegistrations)
                {
                    coursesSearch.Add(cr.Course);
                }
                courses = coursesSearch;

                //modules
                List<ModuleViewModel> modulesSearch = new List<ModuleViewModel>();
                foreach (var c in courses)
                {
                    foreach (var cm in c.CourseModules) 
                    {
                        modulesSearch.Add(cm.Module);
                    }
                }
                modules = modulesSearch;

                //assignments
                List<AssignmentViewModel> assignmentsSearch = new List<AssignmentViewModel>();
                foreach (var m in modules)
                {
                    foreach (var a in m.Assignments)
                    {
                        assignmentsSearch.Add(a);
                    }
                
                }
                assignments = assignmentsSearch;
                

            }
          
            //Initializing Drop down lists
            var coursesDropList = from c in courses
                                  select new { Id = c.Id, Name = c.Title };
            var modulesDropList = from c in modules
                                  select new { Id = c.Id, Name = c.Title };
            var assignmentsDropList = from c in assignments
                                      select new { Id = c.Id, Name = c.Title };
            var studentsDropList = from c in students
                                   select new { Id = c.Id, Name = c.FullName };

            // setting nulls values to 0 for selected values in drop down list
            if (String.IsNullOrEmpty(course)) course = "0";
            if (String.IsNullOrEmpty(module)) module = "0";
            if (String.IsNullOrEmpty(assignment)) assignment = "0";
            if (String.IsNullOrEmpty(student)) student = "0"; 


            ViewData["CoursesDropList"] = new SelectList(coursesDropList, "Id", "Name", Int32.Parse(course));
            ViewData["ModulesDropList"] = new SelectList(modulesDropList, "Id", "Name", Int32.Parse(module));
            ViewData["AssignmentsDropList"] = new SelectList(assignmentsDropList, "Id", "Name", Int32.Parse(assignment));
            ViewData["StudentsDropList"] = new SelectList(studentsDropList, "Id", "Name", Int32.Parse(student));
            
            ViewData["ajaxCall"] = true;

            return PartialView("_GradeFilter");
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="comment"></param>
        /// <param name="gradeValue"></param>
        [HttpPost]
        public void SubmitGradesForSubmission(string id, string comment, string gradeValue) 
        {
            _courseModuleService.SubmitGradesForSubmission(Int32.Parse(id), comment, Int32.Parse(gradeValue));
        }


        #endregion

        #region Analytics

        /// <summary>
        /// Analytics this instance.
        /// </summary>
        /// <returns></returns>
        [ActionName("Analytics")]
        public ActionResult Analytics()
        {
            var courses = _courseModuleService.GetCoursesByStaff(CurrentUser.Id);

            var coursesDropList = from c in courses
                                  select new { Id = c.Id, Name = c.Title };

            ViewData["CoursesDropList"] = new SelectList(coursesDropList, "Id", "Name");
            ViewData["Courses"] = courses;
            ViewData["Assignments"] = _courseModuleService.GetAssignmentsForTeacher(CurrentUser.Id, true);
     
            return View();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult AnalyticsPerCourse(int? id)
        {

            if (id == null)
                return null;

            var assignments = _courseModuleService.GetAssignmentsForTeacher(CurrentUser.Id, true);
            var studentsNames = new List<string>();
            var studentsGrades = new List<int>();
            
            if (id.HasValue) 
            {
                // Filter Assignments
                List<AssignmentViewModel> assignmentsToKeep = new List<AssignmentViewModel>();
                foreach (var assign in assignments)
                {
                    foreach(var cm in assign.Module.CourseModules)
                    {
                        if(cm.Course_Id == id)
                        {
                            assignmentsToKeep.Add(assign);
                            break;
                        }
                    }
                }
                assignments = assignmentsToKeep;

                // Filter Students
                CourseViewModel course = new CourseViewModel();
                course = _courseModuleService.GetCourseByIdWithStudents(id ?? 0);
                foreach(var cr in course.CourseRegistrations){
                    int grade = _studentService.GetGradeForStudentByCourse(cr.Student_Id, id ?? 0);
                    string name = cr.Student.FullName;
                    studentsNames.Add(name);
                    studentsGrades.Add(grade);
                }
            }


            ViewData["StudentsName"] = studentsNames;
            ViewData["StudentsGrades"] = studentsGrades;
            ViewData["Assignments"] = assignments;
            //ViewData["Courses"] = _courseModuleService.GetCoursesByStaff(WebSecurity.CurrentUserId);            
            
            return PartialView("_AnalyticsPerCoursePartial");
        }

        #endregion

        #region Assets

        /// <summary>
        /// Assetses this instance.
        /// </summary>
        /// <returns></returns>
        [ActionName("Assets")]
        public ActionResult Assets()
        {
            var displayFilter = new PageViewAllFilterViewModel(Url.Action("Assets", "Teacher"));
            ViewData["AssetTypes"] = _assetService.GetAssetTypes();
            ViewData["UploadTypes"] = _assetService.GetUploadTypes();
            ViewData["Pager"] = displayFilter;
            ViewData["Assets"] = _assetService.GetAllAssetsByCompany(CurrentUser.CompanyId, displayFilter.SelectedFilter, (List<String>)Session["tagAssetFilter"]);
            return View();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult AssetsPartial() 
        {
            StaffViewModel staff = _staffService.GetStaffByMemberShipId(CurrentUser.Id);
            var displayFilter = new PageViewAllFilterViewModel(Url.Action("Assets", "Teacher"));
            ViewData["AssetTypes"] = _assetService.GetAssetTypes();
            ViewData["UploadTypes"] = _assetService.GetUploadTypes();
            ViewData["Pager"] = displayFilter;

            IEnumerable<UniEBoard.Service.Models.AssetViewModel> assets = _assetService.GetAllAssetsByCompany(staff.CompanyId, displayFilter.SelectedFilter, (List<String>)Session["tagAssetFilter"]);

            //ViewData["Assets"] = _assetService.GetAllAssetsByCompany(staff.CompanyId, displayFilter.SelectedFilter, (List<String>)Session["tagAssetFilter"]);
            return PartialView("_AssetListPartial", assets);
        }

        /// <summary>
        /// Assetses this instance.
        /// </summary>
        /// <returns></returns>
        [ActionName("Assets")]
        [HttpPost]
        public ActionResult Assets(PageViewAllFilterViewModel pageViewFilterModel)
        {
            StaffViewModel staff = _staffService.GetStaffByMemberShipId(CurrentUser.Id);
            ViewData["AssetTypes"] = _assetService.GetAssetTypes();
            ViewData["UploadTypes"] = _assetService.GetUploadTypes();
            ViewData["Pager"] = pageViewFilterModel;
            ViewData["Assets"] = _assetService.GetAllAssetsByCompany(staff.CompanyId, pageViewFilterModel.SelectedFilter, (List<String>)Session["tagAssetFilter"]);
            return View();
        }

        /// <summary>
        /// Assetses the specified create asset view model.
        /// </summary>
        /// <param name="createAssetViewModel">The create asset view model.</param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult CreateAsset(CreateAssetViewModel createAssetViewModel)
        {
            
            
            StaffViewModel staff = _staffService.GetStaffByMemberShipId(CurrentUser.Id);
            if (ModelState.IsValid)
            {
                try
                {
                _assetService.CreateAsset(createAssetViewModel, "~/App_Assets", staff.CompanyId);
                    UniEBoard.Helpers.StatusHelper.SuccessMessage("Asset has been created successfully.", this);
                }
                catch (Exception ex)
                {
                    UniEBoard.Helpers.StatusHelper.ErrorMessage("An error occurred while creating new asset. Please contact the administrator", this);
                }
                return RedirectToAction("Assets");
            }
            else
            {
                ViewData["AssetTypes"] = _assetService.GetAssetTypes();
                ViewData["UploadTypes"] = _assetService.GetUploadTypes();
                //ViewData["Pager"] = pageViewFilterModel;
                ViewData["Assets"] = _assetService.GetAllAssetsByCompany(staff.CompanyId, 10, (List<String>)Session["tagAssetFilter"]);

                return View("Assets");
            }
            
            
        }

        /// <summary>
        /// Removes the tag from asset.
        /// </summary>
        /// <param name="assetId">The asset id.</param>
        /// <param name="tagId">The tag id.</param>
        /// <returns></returns>
        [ActionName("RemoveTagFromAsset")]
        [HttpPost]
        public ActionResult RemoveTagFromAsset(int assetId, int tagId)
        {
            _assetService.RemoveTagFromAsset(assetId, tagId);
            AssetViewModel asset = _assetService.GetAssetById(assetId);
            ViewData["AssetIdentifier"] = asset.Id;
            return PartialView("_AssetTagPartial", asset.Tags);
        }

        /// <summary>
        /// Updates the name of the asset.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="assetId">The asset id.</param>
        [ActionName("UpdateAsset")]
        [HttpPost]
        public ActionResult UpdateAsset(string name, int assetId, string tagName)
        {
            AssetViewModel asset = _assetService.GetAssetById(assetId);
            asset.Name = name;

            if (!String.IsNullOrEmpty(tagName)) 
            {
                TagViewModel tag = new TagViewModel();
                tag.Name = tagName;
                asset.Tags.Add(tag);
            }

            _assetService.UpdateAsset(asset);

            asset = _assetService.GetAssetById(assetId);
            ViewData["AssetIdentifier"] = asset.Id;
            return PartialView("_AssetTagPartial", asset.Tags);
        }

        /// <summary>
        /// Delete asset.
        /// </summary>
        /// <param name="assetId">The asset id.</param>
        //[HttpPost]
        [Authorize]
        public ActionResult RemoveAsset(int assetId)
        {
            /*AssetViewModel asset = _assetService.GetAssetById(assetId);
            List<TagViewModel> tags = asset.Tags.ToList();
            foreach (var tag in tags)
                _assetService.RemoveTagFromAsset(assetId, tag.Id);*/
            List<Model.Entities.Unit> unitsByVideo = _unitModuleAppService.UnitManager.FindAll().Where(u => u.VideoId.Equals(assetId)).ToList();
            foreach (var unit in unitsByVideo)
            {
                unit.VideoId = null;
                _unitModuleAppService.UnitManager.Update(unit);
            }

            List<Model.Entities.Unit> unitsByDocument = _unitModuleAppService.UnitManager.FindAll().Where(u => u.DocumentId.Equals(assetId)).ToList();
            foreach (var unit in unitsByDocument)
            {
                unit.DocumentId = null;
                _unitModuleAppService.UnitManager.Update(unit);
            }

            _assetService.AssetManager.RemoveTags(assetId);
            _assetService.AssetManager.RemoveDocuments(assetId);
            _assetService.AssetManager.RemoveImages(assetId);
            _assetService.AssetManager.RemoveVideos(assetId);
            //List<DocumentViewModel> documents = _assetService.AssetManager.Remove(

            return RedirectToAction("Assets");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="assetName"></param>
        public ActionResult AddAssetForUnit(string assetName, string unitId) 
        {
            UnitViewModel unit = new UnitViewModel();
            if (!String.IsNullOrEmpty(assetName))
            {
                unit = _assetService.AddAssetForUnit(assetName, Int32.Parse(unitId));
            }
            return PartialView("_UnitAssetListPartial", unit);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="assetName"></param>
        public ActionResult RemoveAssetForUnit(string assetId, string unitId)
        {
            UnitViewModel unit = new UnitViewModel();
            _assetService.RemoveAssetForUnit(Int32.Parse(assetId), Int32.Parse(unitId));
            unit = _unitModuleAppService.GetUnit(Int32.Parse(unitId));
            ViewData["listType"] = "list";
            return PartialView("_UnitAssetListPartial", unit);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="term"></param>
        /// <returns></returns>
        public ActionResult AutoCompleteAssets(string term)
        {
            StaffViewModel staff = _staffService.GetStaffByMemberShipId(CurrentUser.Id);
            var assetList = _assetService.GetAllAssetsByCompany(staff.CompanyId, 0, null);
            
            //string search = '';
            var json = Json((from asset in assetList
                             where (asset.Name.ToLower().Contains(term.ToLower()))
                             select new { label = asset.Name, id = asset.Name, type = asset.AssetType }).ToArray(), JsonRequestBehavior.AllowGet);
            return json;
        }

        /// <summary>
        /// Returns a jSon string used for the autocomplete field tags on assets
        /// </summary>
        /// <param name="term"></param>
        /// <returns></returns>
        public ActionResult AutoCompleteAssetTags(string term)
        {
            //string search = '';
            var json = Json((from tag in _assetService.GetAllTags()
                             where (tag.Name.ToLower().Contains(term.ToLower()))
                             select new { label = tag.Name, id = tag.Name }).ToArray(), JsonRequestBehavior.AllowGet);
            return json;
        }

        /// <summary>
        /// Adds a tag to filter the asset list
        /// </summary>
        /// <param name="tagFilter"></param>
        /// <returns></returns>
        public ActionResult AddFilterTagAsset(String tagFilter) 
        {
            if (Session["tagAssetFilter"] == null)
            {
                Session["tagAssetFilter"] = new List<String>();
            }

            ((List<String>)Session["tagAssetFilter"]).Add(tagFilter);
            return PartialView("_AssetFilterTagPartial", (List<String>)Session["tagAssetFilter"]);            
        }

        /// <summary>
        /// Removes a tag to filter the asset list
        /// </summary>
        /// <param name="tagFilter"></param>
        /// <returns></returns>
        public ActionResult RemoveFilterTagAsset(String tagFilter, bool standard = true)
        {
            // standard is used to identify whether the request is coming from Assignment page or anywhere else
            // this is a hack and we probably would have use a ViewModel that holds the asset list and associated parameters i.e. target and controller etc.
            if (standard)
            {
                ((List<String>)Session["tagAssetFilter"]).Remove(tagFilter);
                return PartialView("_AssetFilterTagPartial", (List<String>)Session["tagAssetFilter"]);
            }
            else
            {
                List<string> assetNameList = new List<string>();
                AssetViewModel asset = assignmentAssets.Where(a => a.Name.ToLower().Equals(tagFilter.ToLower())).FirstOrDefault();
                if (asset != null) assignmentAssets.Remove(asset);
                foreach (var model in assignmentAssets)
                {
                    assetNameList.Add(model.Name);
                }
                return PartialView("_AssetFilterTagPartial", assetNameList);
            }
        }

        #endregion

        #region Users

        /// <summary>
        /// The Users instance.
        /// </summary>
        /// <returns></returns>
        [ActionName("Users")]
        public ActionResult Users()
        {
            //StaffViewModel staff = _staffService.GetStaffByMemberShipId(CurrentUser.Id);
            var displayFilter = new PageViewAllFilterViewModel(Url.Action("Users", "Teacher"));
            ViewData["Pager"] = displayFilter;
            ViewData["Courses"] = _courseModuleService.GetCoursesByStaff(CurrentUser.Id).Select(v => new SelectListItem { Value = v.Id.ToString(), Text = v.Title }).ToArray();
            ViewData["Users"] = _userAppService.GetStudentUsersByCompany(CurrentUser.CompanyId, displayFilter.SelectedFilter);
            ViewData["Roles"] = _userAppService.GetAllAvailableRoles().Select(r => new SelectListItem { Value = r.Id.ToString(), Text = r.Title }).ToArray();
            return View();
        }

        /// <summary>
        /// The Users instance.
        /// </summary>
        /// <returns></returns>
        [ActionName("Users")]
        [HttpPost]
        public ActionResult Users(PageViewAllFilterViewModel pageViewFilterModel)
        {
            //StaffViewModel staff = _staffService.GetStaffByMemberShipId(CurrentUser.Id);
            ViewData["Pager"] = pageViewFilterModel;
            ViewData["Courses"] = _courseModuleService.GetCoursesByStaff(CurrentUser.Id).Select(v => new SelectListItem { Value = v.Id.ToString(), Text = v.Title }).ToArray();
            ViewData["Users"] = _userAppService.GetStudentUsersByCompany(CurrentUser.CompanyId, pageViewFilterModel.SelectedFilter);
            ViewData["Roles"] = _userAppService.GetAllAvailableRoles().Select(r => new SelectListItem { Value = r.Id.ToString(), Text = r.Title }).ToArray();
            return View();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult UserListFilteredPartial(string filter)
        {
            StaffViewModel staff = _staffService.GetStaffByMemberShipId(CurrentUser.Id);
            var users = _userAppService.GetStudentUsersByCompany(staff.CompanyId, filter);
            return PartialView("_UserListPartial", users);
        }

        [HttpPost]
        public void UpdateUser(string userId, bool isEnabled)
        {
            var uId = userId;
            UserViewModel userViewModel = _userAppService.GetUserById(userId.ToInteger());
            userViewModel.AccountDisabled = !isEnabled;
            _userAppService.UpdateUser(userViewModel);
            //_userAppService.UserManager.Update(

        }

        [HttpPost]
        public ActionResult RemoveUserFromCourse(int userId, int courseId)
        {
            if (_userAppService.RemoveUserFromCourse(userId, courseId))
            {
                ViewData["UserIdentifier"] = userId;
                return PartialView("_CourseTagPartial", _courseModuleService.GetAllStudentCourses(userId));
            }
            return View();
        }

        public ActionResult AutoCompleteUsers(string term)
        {
            var json = Json((from user in _staffService.GetAllStaff()
                             where user.FullName.ToUpper().Contains(term.ToUpper())
                             select new { userName = user.FullName, id = user.Id }).ToArray(), JsonRequestBehavior.AllowGet);
            return json;
        }

        public void UpdateOrder(List<string> draggedItemId, string type)
        {
            switch (type)
            {
                case "courses":
                    UpdateCourseOrder(draggedItemId);
                    break;
                case "modules":
                    UpdateModuleOrder(draggedItemId);
                    break;
                case "questions":
                    UpdateQuestionOrder(draggedItemId);
                    break;
                case "units":
                    UpdateUnitOrder(draggedItemId);
                    break;
            }
        }

        /// <summary>
        /// The Users instance.
        /// </summary>
        /// <returns></returns>
        [ActionName("CreateStudentUser")]
        [HttpPost]
        public ActionResult CreateStudentUser(UserViewModel user)
        {
            StaffViewModel staff = _staffService.GetStaffByMemberShipId(CurrentUser.Id);
            try
            {
                if (ModelState.IsValid)
                {
                    
                    UserViewModel existingUser = _userAppService.GetUserByUserName(user.Email);
                    // if user exists then add it to the selected course
                    if (existingUser != null)
                    {
                        existingUser.FirstName = user.FirstName;
                        existingUser.LastName = user.LastName;
                        if (!SelectedCourseAlreadyExistsFor(existingUser, user.DefaultCourseId))
                        {
                            CourseRegistrationViewModel registration = new CourseRegistrationViewModel() { Course_Id = user.DefaultCourseId, Student_Id = existingUser.Id };
                            _courseModuleService.CreateCourseRegistration(registration);
                            _studentService.UpdateStudentUser((StudentViewModel)existingUser);
                        }
                        else
                        {
                            ModelState.AddModelError("", string.Format("The user {0} has already been assign to the course '{1}'", existingUser.UserName, _courseModuleService.GetCourseById(existingUser.DefaultCourseId).Title));
                        }

                    }
                    // if user doesn't exist then create a new one
                    else
                    {
                        UserViewModel studentViewModel = new StudentViewModel()
                        {
                            FirstName = user.FirstName,
                            LastName = user.LastName,
                            Email = user.Email,
                            UserName = user.Email,
                            Password = "Password123",
                            UserGender = (int)GenderType.NotSpecified,
                            CompanyId = staff.CompanyId
                        };

                        cog.WebSecurity.CreateUserAndAccount(studentViewModel.UserName, studentViewModel.Password);
                        if (cog.WebSecurity.UserExists(studentViewModel.UserName))
                        {
                            CourseRegistrationViewModel registration = new CourseRegistrationViewModel() { Course_Id = user.DefaultCourseId };
                            ((StudentViewModel)studentViewModel).CourseRegistrations = new List<CourseRegistrationViewModel> { registration };
                            var _user = _userAppService.CreateUser(cog.WebSecurity.GetUserId(studentViewModel.UserName), studentViewModel);
                            if (_user != null)
                            {
                                // TODO - pick the role from the Roles list availbable on the UserFormPartial view.
                                _userAppService.AssignRole(_user.Id, Service.C.Roles.Student);
                                SendRegisterEmail(studentViewModel as StudentViewModel);
                            }
                        }
                    }
                    return RedirectToAction("Users");
                }

            }
            catch (MembershipCreateUserException e)
            {
                ModelState.AddModelError("", ErrorCodeToString(e.StatusCode));
            }
            catch (Exception)
            {
                ModelState.AddModelError("", "Error creating user. Please try again. If the problem persists, please contact your system administrator.");
            }
            
            //StaffViewModel staff = _staffService.GetStaffByMemberShipId(CurrentUser.Id);
            //ViewData["Pager"] = pageViewFilterModel;
            ViewData["Courses"] = _courseModuleService.GetCoursesByStaff(staff.Id).Select(v => new SelectListItem { Value = v.Id.ToString(), Text = v.Title }).ToArray();
            ViewData["Users"] = _userAppService.GetStudentUsersByCompany(staff.CompanyId, 10);           
            return View("Users");
        }

        #endregion

        #region Private Methods
        /// <summary>
        /// Gets the class view model.
        /// </summary>
        /// <param name="calssid">The calssid.</param>
        /// <returns></returns>
        private ClassViewModel GetClassViewModel(int calssid)
        {
            List<ClassViewModel> couserList = this._unitModuleAppService.FindClassesByStaffId(CurrentUser.Id);

            return couserList.Where(p => p.Id.Equals(calssid)).FirstOrDefault();
        }

        /// <summary>
        /// Errors the code to string.
        /// </summary>
        /// <param name="createStatus">The create status.</param>
        /// <returns></returns>
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

        private bool SelectedCourseAlreadyExistsFor(UserViewModel user, int selectedCourse)
        {
            List<CourseViewModel> courses = user.Courses.Where(c => c.Id == selectedCourse).ToList();
            return (courses != null && courses.Count > 0);
        }

        private void UpdateCourseOrder(List<string> draggedItemId)
        {
            /*********************************
             * Current implementation works perfect with non-pagination results but the logic doesn't support pagination in place. 
             * This makes it more complex to handle the results. 
             *********************************/
            int sort = 0;
            foreach (var courseId in draggedItemId)
            {
                int id = 0;
                if (int.TryParse(courseId, out id))
                {
                    var course = _courseModuleService.GetCourseById(id);
                    course.SortOrder = sort++;
                    _courseModuleService.UpdateCourse(course);
                }
                else
                {
                    throw new InvalidCastException();
                }
            }

            /*
             * This was initially implemented to order the courses accross all staff/teachers.
             * To me the reordering should only be done by Administrators.
             */
            /*var courses = _courseModuleService.GetCoursesWithDepartmentByStaffId(CurrentUser.Id, 0).OrderBy(o => o.SortOrder).Where(c => c.Id != draggedItemId).ToList();
            var currentCourse = new CourseViewModel();
            currentCourse = (from c in _courseModuleService.GetAllCourses()
                             where c.Id == draggedItemId
                             select c).FirstOrDefault();
            if (draggedDown)
                currentCourse.SortOrder += (newIndex);
            else
                currentCourse.SortOrder -= (newIndex);
            courses.Add(currentCourse);
            foreach (var course in courses)
            {
                if (course.SortOrder == currentCourse.SortOrder && course.Id != currentCourse.Id)
                {
                    if (draggedDown)
                        course.SortOrder--;
                    else
                        course.SortOrder++;
                }
            }
            IEnumerable<CourseViewModel> newCoursesList = courses;

            newCoursesList = newCoursesList.OrderBy(o => o.SortOrder);*/
        }

        private void UpdateModuleOrder(List<string> draggedItemId)
        {
            /*********************************
             * Current implementation works perfect with non-pagination results but the logic doesn't support pagination in place. 
             * This makes it more complex to handle the results. 
             *********************************/
            int sort = 0;
            foreach (var moduleId in draggedItemId)
            {
                int id = 0;
                if (int.TryParse(moduleId, out id))
                {
                    var module = _courseModuleService.GetModuleById(id);
                    module.SortOrder = sort++;
                    _courseModuleService.UpdateModule(module);
                }
                else
                {
                    throw new InvalidCastException();
                }
            }
        }

        private void UpdateQuestionOrder(List<string> draggedItemId)
        {
            /*********************************
             * Current implementation works perfect with non-pagination results but the logic doesn't support pagination in place. 
             * This makes it more complex to handle the results. 
             *********************************/
            int sort = 0;
            foreach (var questionId in draggedItemId)
            {
                int id = 0;
                if (int.TryParse(questionId, out id))
                {
                    var question = _questionAppService.GetQuestionById(id);
                    question.SortOrder = sort++;
                    _questionAppService.EditQuestion(question);
                }
                else
                {
                    throw new InvalidCastException();
                }
            }
        }

        private void UpdateUnitOrder(List<string> draggedItemId)
        {
            /*********************************
             * Current implementation works perfect with non-pagination results but the logic doesn't support pagination in place. 
             * This makes it more complex to handle the results. 
             *********************************/
            int sort = 0;
            foreach (var unitId in draggedItemId)
            {
                int id = 0;
                if (int.TryParse(unitId, out id))
                {
                    var unit = _unitModuleAppService.GetUnit(id);
                    unit.SortOrder = sort++;
                    _unitModuleAppService.UpdateUnit(unit);
                }
                else
                {
                    throw new InvalidCastException();
                }
            }
        }

        private TopicViewModel PrepareTopicViewModel(ModuleViewModel module, int discussionId)
        {
            TopicViewModel topicViewModel = new TopicViewModel();
            topicViewModel.IsTopic = true;
            topicViewModel.OriginatorId = module.CreatedByStaff_Id;
            topicViewModel.Title = module.Title;
            topicViewModel.Description = module.Description;
            topicViewModel.DiscussionId = discussionId;
            
            return topicViewModel;
        }

        private TopicViewModel PrepareTopicViewModel(UnitViewModel unit, int discussionId)
        {
            TopicViewModel topicViewModel = new TopicViewModel();
            topicViewModel.IsTopic = true;
            topicViewModel.OriginatorId = unit.StaffId;
            topicViewModel.Title = unit.Title;
            topicViewModel.Description = unit.Description;
            topicViewModel.DiscussionId = discussionId;

            return topicViewModel;
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



        #endregion

        #endregion
    }

    public class OrderViewModel
    {
        public List<string> draggedItemId;
        public string type;
    }


}
