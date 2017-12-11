// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CourseController.cs" company="Cognite Ltd">
//   Copyright (c) 2013 Cognite Ltd (http://www.cognite.co.uk)
// </copyright>
// <summary>
//  Course Controller Methods
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
using UniEBoard.Service.ApplicationServices;
using UniEBoard.Service.Models.Courses;
using UniEBoard.Service.Models.Quizzes;
using System.Web.SessionState;

namespace UniEBoard.Controllers
{
    /// <summary>
    /// The Course Controller
    /// </summary>
    [Authorize]
    [InitializeSimpleMembership]
    public class CourseController : BaseController
    {
        #region Members

                /// <summary>
        /// Student Application Service
        /// </summary>
	        private IStudentAppService _studentService;

        /// <summary>
        /// Course And Module Application Service 
        /// </summary>
        private ICourseModuleAppService _courseModuleService;

        /// <summary>
        /// Course And Module Application Service 
        /// </summary>
        private IUnitModuleAppService _unitModuleService;

        /// <summary>
        /// Course And Module Application Service 
        /// </summary>
        private IQuizAppService _quizService;

        /// <summary>
        /// questions Application Service 
        /// </summary>
        private IQuestionAppService _questionService;

        /// <summary>
        /// answer Application Service 
        /// </summary>
        private IAnswerAppService _answerService;

        private IUserAppService _userAppService;



        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="CourseController"/> class.
        /// </summary>
        /// <param name="courseModuleService">The course module service.</param>
        ///   /// <param name="courseService">The course  service.</param>
        public CourseController(
            IStudentAppService studentService, 
            ICourseModuleAppService courseModuleService, 
            IUnitModuleAppService unitModuleService, 
            IQuizAppService quizservice, 
            IQuestionAppService questionService, 
            IAnswerAppService answerService,
            IUserAppService userAppService) : base(userAppService)
            
        {
            this._courseModuleService = courseModuleService;
            _unitModuleService = unitModuleService;
            _quizService = quizservice;
            _questionService = questionService;
            _answerService = answerService;
               _studentService = studentService;
               _userAppService = userAppService;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Sessions
        /// </summary>
        /// <param name="filterContext"></param>
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            Session["Sid"] = Session.SessionID;
            base.OnActionExecuting(filterContext);
        }

        /// <summary>
        /// GET: /Course/
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// GET: /Course/Details/5
        /// </summary>
        /// <param name="id">The id of course.</param>
        /// <returns>View Course (Course details)</returns>
        [ActionName("Home")]
        //[OutputCache(CacheProfile = "CourseCacheVariable")]
        public ActionResult Home(int id)
        {
            CourseViewModel model = _courseModuleService.GetCourseById(id);
            return View(model);
        }

        /// <summary>
        /// GET: /Course/Syllabus/5
        /// </summary>
        /// <param name="id">The id of course.</param>
        /// <returns>View Course Syllabus (Course, modules plan)</returns>
        [ActionName("Syllabus")]
        [OutputCache(CacheProfile = "CourseCacheVariable")]
        public ActionResult CourseSyllabusView(int id)
        {
            //Module-Description
            CourseViewModel course = _courseModuleService.GetCourseById(id);
            ViewData["CourseModel"] = course;
            ViewData["CourseDates"] = "" + String.Format("{0:dd MMM yyyy}",course.PublishFrom) + " - " + String.Format("{0:dd MMM yyyy}",course.PublishTo);


            List<ModuleSyllabusModel> modulesSyllabus = new List<ModuleSyllabusModel>();
            if (course.Approved)
            {
                modulesSyllabus = _courseModuleService.GetModuleSyllabusById(id);
            }

            return View(modulesSyllabus);

        }

        /// <summary>
        /// GET: /Course/Syllabus/5
        /// </summary>
        /// <param name="id">The id of course.</param>
        /// <returns>View Course Syllabus (Course, modules plan)</returns>
        [ActionName("VideoLectures")]
        //[OutputCache(CacheProfile = "CourseCacheVariable")]
        public ActionResult VideoLectures(int id)
        {
            CourseViewModel course = _courseModuleService.GetCourseById(id);
            ViewData["CourseModel"] = course;
            List<VideoLecturesViewModel> model = _unitModuleService.GetUnitsByCourse(id);

            return View(model);
        }

        /// <summary>
        /// GET: /Course/Syllabus/5
        /// </summary>
        /// <param name="id">The id of course.</param>
        /// <returns>View Course Syllabus (Course, modules plan)</returns>
        [ActionName("Quizzes")]
        public ActionResult Quizzes(int id)
        {
            CourseViewModel course = _courseModuleService.GetCourseById(id);
            ViewData["CourseModel"] = course;
            int studentId = CurrentUser.Id;
            List<QuizzesViewModel> model = _quizService.GetQuizByCourse(id, studentId);
            //set up questions
            foreach (var item in model)
            {
                item.questions = _questionService.GetQuestionsByQuizId(item.Id);
            }
            return View(model);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="values"></param>
        /// <param name="idquiz"></param>
        /// <param name="idquestion"></param>
        /// <param name="QuestionType"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult GoNextQuestion(string values, int quizId, int questionId, string questionType, int counter)
        {
            List<QuestionViewModel> questions = _questionService.GetQuestionsByQuizId(quizId);
            QuestionViewModel QuestionEmpty = new QuestionViewModel();
            QuestionEmpty.Quiz_Id = quizId;

            int questionPosition = GetCurrentQuestion(quizId, questionId).questionPosition;
            if (HttpContext.Cache["QuizEntry_Id"] != null)
            {
                int quizEntryId = Convert.ToInt32(HttpContext.Cache["QuizEntry_Id"]);
                AddAnswer(values, quizEntryId, questionId, questionType);
                ViewData["QuizCounter"] = counter;
                if (questions.Count() > questionPosition + 1)
                {
                    QuestionViewModel nextQuestion = questions.ElementAt(questionPosition + 1);
                    return PartialView("_Question", nextQuestion);
                }
                else
                {
                    return PartialView("_Question", QuestionEmpty);
                }
            }
            else
            {
                ViewBag.ErrorMessage = "The quiz have expired.";
                return PartialView("_QuestionError");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id">QuizId</param>
        /// <returns></returns>
        public ActionResult ShowQuizAnswers(int id)
        {
            ViewBag.QuizId = id;
            List<QuestionViewModel> questionList = _questionService.GetQuestionsByQuizId(id);
            return PartialView("_QuizzAnswers", questionList);
        }


        /// <summary>
        /// Checks if the quiz is available to the student by the number of attempts
        /// Starts a quiz, showing its first question
        /// </summary>
        /// <param name="id">Quiz Id</param>
        /// <returns></returns>
        public ActionResult StartQuiz(int id)
        {
            if (_quizService.QuizAvailableToStudent(id, CurrentUser.Id))
            {
                ViewBag.ErrorMessage = "You have overpassed the number of attempts for this quiz.";
                return PartialView("_QuestionError");
            } 
            
            List<QuestionViewModel> questionList = _questionService.GetQuestionsByQuizId(id);

            if (questionList.Count > 0)
            {
                AddQuizEntry(id);
                ViewData["QuizCounter"] = Request.QueryString["counter"];
                return PartialView("_Question", questionList.FirstOrDefault());
            }
            else
            {
                ViewBag.ErrorMessage = "There are no available questions for this quiz. Please contact your teacher.";
                return PartialView("_QuestionError");
            }

        }

        /// <summary>
        /// Submits the quiz
        /// </summary>
        /// <param name="id"></param>
        [ActionName("SubmitQuiz")]
        public ActionResult SubmitQuiz(string values, int quizId, int questionId, string questionType)
        {
            int quizEntryId = 0;
            ResultQuizzModel resultQuiz = new ResultQuizzModel();
            if (HttpContext.Cache["QuizEntry_Id"] != null)
            {
                quizEntryId = Convert.ToInt32(HttpContext.Cache["QuizEntry_Id"].ToString());
                AddAnswer(values, quizEntryId, questionId, questionType);
                resultQuiz = _quizService.GetResult(quizEntryId, quizId);

            }

            //User Name
            StudentViewModel student = _studentService.GetStudentByMemberShipId(CurrentUser.Id);
            resultQuiz.Name = student.FirstName + " " + student.LastName;

            //Time Taken
            if (!string.IsNullOrEmpty(GetTimeStarQuiz()))
            {
                TimeSpan timestar = TimeSpan.Parse(GetTimeStarQuiz());
                resultQuiz.TimeTaken = (DateTime.Now.TimeOfDay - timestar);
            }

            //Add results to the table quizEntry
            //IMPORTANT
            QuizEntryViewModel quizentryModel = new QuizEntryViewModel();
            quizentryModel.Id = quizEntryId;
            quizentryModel.Quiz_Id = quizId;
            quizentryModel.Student_Id = CurrentUser.Id;
            quizentryModel.QuizResult = resultQuiz.Results;
            _quizService.updateQuizEntry(quizentryModel);

            RemoveQuizEntryCache();
            return PartialView("_QuizzResults", resultQuiz);
        }

        #endregion

        #region private Methods

        /// <summary>
        /// 
        /// </summary>
        /// <param name="values"></param>
        /// <param name="idquiz"></param>
        /// <param name="idquestion"></param>
        /// <param name="QuestionType"></param>
        private void AddAnswer(string values, int quizEntryId, int questionId, string questionType)
        {
            AnswerViewModel answer = new AnswerViewModel();
            answer.Question_Id = questionId;
            answer.QuizEntryId = quizEntryId;

            //Build the model of the answer
            string[] results = values.Split(',');
            results = _answerService.AddListAnswer(results, answer, questionType);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="idquiz"></param>
        /// <param name="idquestion"></param>
        /// <returns></returns>
        private QuestionViewModel GetCurrentQuestion(int idquiz, int idquestion)
        {
            List<QuestionViewModel> model = _questionService.GetQuestionsByQuizId(idquiz);
            
            //Get the current answer
            QuestionViewModel CurrentQuestion = model.Find(x => x.Id == idquestion);
            return CurrentQuestion;
        }

        /// <summary>
        /// Adds a Quiz Entry for the student
        /// </summary>
        /// <param name="id"></param>
        private void AddQuizEntry(int quizId)
        {
            QuizEntryViewModel quizentryModel = new QuizEntryViewModel();
            quizentryModel.Quiz_Id = quizId;
            quizentryModel.Student_Id = CurrentUser.Id;

            //Add a new Quiz Entry 
            int quizEntryId = _quizService.AddQuizEntry(quizentryModel);

            //Add the current quizEntryId to Cache. 
            AddCurrentQuizEntryIdCache(quizEntryId);

            //time
            AddTimeStarQuizCache(DateTime.Now.TimeOfDay);
        }


        /// <summary>
        /// load in cache the quiz entry id until the user finish the quiz. 
        /// </summary>
        /// <param name="QuizEntry_Id"></param>
        private void AddCurrentQuizEntryIdCache(int QuizEntry_Id)
        {
            //Create cache for  QuizEntry
            if (HttpContext.Cache["QuizEntry_Id"] != null)
                HttpContext.Cache.Remove("QuizEntry_Id");
            HttpContext.Cache["QuizEntry_Id"] = QuizEntry_Id.ToString();
        }

        /// <summary>
        /// Remove the current quiz entry 
        /// </summary>
        private void RemoveQuizEntryCache()
        {
            //Delete cache for quizEntryId
            HttpContext.Cache.Remove("QuizEntry_Id");
        }


        /// <summary>
        /// load in cache the quiz entry id until the user finish the quiz.
        /// </summary>
        /// <param name="QuizEntry_Id"></param>
        private void AddTimeStarQuizCache(TimeSpan time)
        {
            //Create cache for  QuizEntry
            if (HttpContext.Cache["TimeStar"] != null)
                HttpContext.Cache.Remove("TimeStar");
            HttpContext.Cache["TimeStar"] = time.ToString();
        }

        /// <summary>
        /// Remove the current quiz entry
        /// </summary>
        private void RemoveStarQuizCache()
        {
            //Delete cache for quizEntryId
            HttpContext.Cache.Remove("TimeStar");
        }

        private string GetTimeStarQuiz()
        {
            if (HttpContext.Cache["TimeStar"] != null)
                return HttpContext.Cache["TimeStar"].ToString();
            else
                return "";
        }

        #endregion

    }
}
