// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DiscussionController.cs" company="Cognite Ltd">
//   Copyright (c) 2013 Cognite Ltd (http://www.cognite.co.uk)
// </copyright>
// <summary>
//  Discussion Controller Methods
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
using WebMatrix.WebData;
using UniEBoard.Filters;
using UniEBoard.Models;
using UniEBoard.Service.Interfaces.ApplicationService;
using UniEBoard.Model.Enums;
using UniEBoard.Service.Models;
using UniEBoard.Service.ApplicationServices;
using PagedList;

namespace UniEBoard.Controllers
{
    /// <summary>
    /// The Discussion Controller
    /// </summary>
    [Authorize]
    [InitializeSimpleMembership]
    public class DiscussionController : Controller
    {
        #region Members

        /// <summary>
        /// Student Application Service 
        /// </summary>
       // private IStudentAppService _studentService;
        private ICourseModuleAppService _courseModuleAppService;
        private IDiscussionAppService _discussionAppService;
        private IUserAppService _userAppService;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="DiscussionController"/> class.
        /// </summary>
        /// <param name="courseModuleAppService">The course module app service.</param>
        /// <param name="discussionAppService">The discussion app service.</param>
        public DiscussionController(
            ICourseModuleAppService courseModuleAppService, 
            IDiscussionAppService discussionAppService,
            IUserAppService userAppService)
        {
            //this._studentService = studentService;
            this._courseModuleAppService = courseModuleAppService;
            this._discussionAppService = discussionAppService;
            this._userAppService = userAppService;
        }

        #endregion
        
        #region Properties
        public UserViewModel CurrentUser
        {
            get
            {
                return _userAppService.GetUserByUserName(WebSecurity.CurrentUserName);
            }
        }
        #endregion

        #region Methods

        /// <summary>
        /// GET: /Discussion/
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {
            //int userId = userapp
            UserViewModel user = _userAppService.GetUserByUserName(WebSecurity.CurrentUserName);
            if(user != null) ViewData["StudentCourses"] = new SelectList(_courseModuleAppService.GetAllStudentCourses(user.Id, false), "Id", "Title", "");
            ViewData["DiscussionThread"] = _discussionAppService.GetDiscussionsByStaffId(user.Id);
            ViewData["SelectedStudentCourses"] = 0;
            return View("Discussion");
        }

        /// <summary>
        /// GET: /Discussion/Details/5
        /// </summary>
        /// <param name="selectedCourse">The selected course.</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult DiscussionThread(int? selectedCourse)
        {
            List<DiscussionViewModel> list=new List<DiscussionViewModel>();

            ViewData["SelectedStudentCourses"] = selectedCourse;

            if (selectedCourse != null)
                list = _discussionAppService.GetCourseDiscussionsWithLatestPosts((int)selectedCourse);
            else
            {
                list = _discussionAppService.GetDiscussionsByStaffId(CurrentUser.Id);
                ViewData["SelectedStudentCourses"] = 0;
            }

            //if (list.Count > 0)
                return PartialView("_DiscussionThreadPartial", list);
            //else
            //    return Content("", "text/html");
        }

        /// <summary>
        /// Discussions the topic.
        /// </summary>
        /// <param name="topicId">The topic id.</param>
        /// <param name="selectedCourse">The selected course.</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult DiscussionTopic(int topicId,int selectedCourse)
        {
            List<TopicViewModel> list = _discussionAppService.GetTopicsByDiscussionId((topicId));

            ViewData["SelectedStudentCourses"] = selectedCourse;

            //if (list.Count > 0)
                return PartialView("_DiscussionTopicPartial", list);
            //else
            //    return Content("", "text/html");
        }

        /// <summary>
        /// GET: /Discussion/Create
        /// </summary>
        /// <returns></returns>
        public ActionResult Create()
        {
            return View();
        }

        /// <summary>
        /// POST: /Discussion/Create
        /// </summary>
        /// <param name="collection">The collection.</param>
        /// <returns></returns>
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

        /// <summary>
        /// GET: /Discussion/Edit/5
        /// </summary>
        /// <param name="id">The id.</param>
        /// <returns></returns>
        public ActionResult Edit(int id)
        {
            return View();
        }

        /// <summary>
        /// POST: /Discussion/Edit/5
        /// </summary>
        /// <param name="id">The id.</param>
        /// <param name="collection">The collection.</param>
        /// <returns></returns>
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

        /// <summary>
        /// GET: /Discussion/Delete/5
        /// </summary>
        /// <param name="id">The id.</param>
        /// <returns></returns>
        public ActionResult Delete(int id)
        {
            return View();
        }

        /// <summary>
        /// POST: /Discussion/Delete/5
        /// </summary>
        /// <param name="id">The id.</param>
        /// <param name="collection">The collection.</param>
        /// <returns></returns>
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

        #region Topics
        public ActionResult TopicPost(int id, int? page)
        {
            Session["TopicId"] = id;
            List<TopicPostViewModel> model = _discussionAppService.GetTopicPostsByTopic(id);

            int pageSize = 5;
            int pageNumber = (page ?? 1);

            return View("TopicPost", model.ToPagedList(pageNumber, pageSize));
        }

        [HttpPost]
        // when try add the html markup from the webpage a validation error is thrown.
        // "A potentially dangerous Request.Form value was detected from the client (Title="<h1>heading test</h1...")."
        // May not be the best of the solution but this is the only solution I have found so far
        [ValidateInput(false)] 
        public ActionResult CreateTopicPost(string Title, string Body)
        {
            int topicId = (int)Session["TopicId"];
            if (ModelState.IsValid)
            {
                int newTopicPostId = _discussionAppService.AddNewTopicPost(Title, Body, topicId, CurrentUser.Id);
            }
            return RedirectToAction("TopicPost", new { id = topicId });
        }

        [HttpPost]
        public ActionResult CreateTopicPostReply(int? topicPostId, string Title, string Body)
        {
            int topicId = (int)Session["TopicId"];
            if (ModelState.IsValid)
            {
                int newTopicPostId = _discussionAppService.AddTopicPostReply(topicPostId.Value, Title, Body, topicId, CurrentUser.Id);
            }
            return RedirectToAction("TopicPost", new { id = topicId });
        }
        #endregion

        #endregion
    }
}
