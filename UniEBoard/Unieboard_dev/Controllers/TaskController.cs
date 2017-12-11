using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using UniEBoard.Model.Entities;
using UniEBoard.Model.Enums;
using UniEBoard.Service.Interfaces.ApplicationService;
using UniEBoard.Service.Models;
using WebMatrix.WebData;

namespace UniEBoard.Controllers
{
    public class TaskController : Controller
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
        /// Message Service
        /// </summary>
        private IMessageAppService _messageService;

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
        public TaskController(
            IStudentAppService studentService,
            IUserAppService userAppService,
            IAssignmentTaskSubmissionAppService assignmentTaskAndSubmission,
            IMessageAppService messageService
            )
        {
            this._studentService = studentService;
            this._userService = userAppService;
            this._assignmentTaskAndSubmissionService = assignmentTaskAndSubmission;
            this._messageService = messageService;
        }

        #endregion

        //
        // GET: /Task/

        #region Actions

        /// <summary>
        /// GET: /Student/Tasks
        /// </summary>
        /// <returns></returns>
        [ActionName("Index")]
        public ActionResult Index()
        {
            var view = Request.QueryString["view"] != null ? Request.QueryString["view"].ToString() : "";
            ViewBag.Filter = _assignmentTaskAndSubmissionService.GetTaskDisplayFilter("Index", "Task", "tasks", TaskFilterType.Active, view);
            ViewBag.Tasks = _assignmentTaskAndSubmissionService.GetTasks(User.Id, TaskFilterType.Active);
            return View("Index");
        }

        /// <summary>
        /// Adds the task.
        /// </summary>
        /// <param name="task">The task.</param>
        /// <returns></returns>
        int hack = 0;
        [ActionName("AddTask")]
        [HttpPost]
        public ActionResult AddTask(TaskViewModel task)
        {
            if (ModelState.IsValid && hack == 0)
            {
                TaskViewModel taskViewModel = _assignmentTaskAndSubmissionService.AddTask(task, User.Id);
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
                ViewBag.Filter = _assignmentTaskAndSubmissionService.GetTaskDisplayFilter("Index", "Task", "tasks");
                List<TaskViewModel> tasks = _assignmentTaskAndSubmissionService.GetTasks(User.Id, TaskFilterType.Active);
                hack++;

                UniEBoard.Helpers.StatusHelper.SuccessMessage("Task has been created successfully.", this);

                return PartialView("_TaskPartial", tasks);

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
        [ActionName("Index")]
        [HttpPost]
        public ActionResult Index(int filter)
        {
            var view = Request.QueryString["view"] != null ? Request.QueryString["view"].ToString() : "";
            ViewBag.Filter = _assignmentTaskAndSubmissionService.GetTaskDisplayFilter("Index", "Task", "tasks", (TaskFilterType)filter, view);
            List<TaskViewModel> tasks = _assignmentTaskAndSubmissionService.GetTasks(User.Id, (TaskFilterType)filter);
            return PartialView("_TaskPartial", tasks);
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
            ViewBag.Filter = _assignmentTaskAndSubmissionService.GetTaskDisplayFilter("Index", "Task", "tasks");
            List<TaskViewModel> tasks = _assignmentTaskAndSubmissionService.GetTasks(User.Id, TaskFilterType.Active);
            ViewData["RequestFrom"] = "AddTask";
            return PartialView("_TaskPartial", tasks);
        }

        #endregion

    }
}
