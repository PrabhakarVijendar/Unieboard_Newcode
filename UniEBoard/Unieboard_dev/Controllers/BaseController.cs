using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using UniEBoard.Service.Interfaces.ApplicationService;
using UniEBoard.Service.Models;
using WebMatrix.WebData;

using cog = Cognite.MembershipProvider;

namespace UniEBoard.Controllers
{
    public class BaseController : Controller
    {
        //
        // GET: /Base/
        #region Members 
        
        /// <summary>
        /// User Application Service
        /// </summary>
        private IUserAppService _userAppService;

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
        public BaseController(IUserAppService userAppService)
        {
            this._userAppService = userAppService;
        }

        #endregion

        #region Properties
        public UserViewModel CurrentUser
        {
            get
            {
                return _userAppService.GetUserByUserName(cog.WebSecurity.CurrentUserName);
            }
        }

        public List<UserViewModel> OnlineUsers
        {
            get
            {
                if (HttpRuntime.Cache["OnlineUsers"] != null)
                {
                    return (List<UserViewModel>)HttpRuntime.Cache["OnlineUsers"];
                }
                else
                {
                    return new List<UserViewModel>();
                }
            }
        }
        #endregion

        #region Methods
        protected List<UserViewModel> AddOnlineUsers(UserViewModel user)
        {
            List<UserViewModel> onlineUsers = new List<UserViewModel>();
            if (HttpRuntime.Cache["OnlineUsers"] != null)
            {
                onlineUsers = (List<UserViewModel>)HttpRuntime.Cache["OnlineUsers"];
                var onlineUser = onlineUsers.Where(u => u.Id.Equals(user.Id)).FirstOrDefault();
                if(onlineUser == null) onlineUsers.Add(user);
                HttpRuntime.Cache["OnlineUsers"] = onlineUsers;
            }
            else
            {
                onlineUsers.Add(user);
                HttpRuntime.Cache["OnlineUsers"] = onlineUsers;
            }

            return onlineUsers.Where(u => !u.Id.Equals(user.Id)).ToList();
        }

        protected List<UserViewModel> RemoveOnlineUsers(UserViewModel user)
        {
            List<UserViewModel> onlineUsers = new List<UserViewModel>();
            if (HttpRuntime.Cache["OnlineUsers"] != null)
            {
                onlineUsers = (List<UserViewModel>)HttpRuntime.Cache["OnlineUsers"];
                var onlineUser = onlineUsers.Where(u => u.Id.Equals(user.Id)).FirstOrDefault();
                onlineUsers.Remove(onlineUser);
                HttpRuntime.Cache["OnlineUsers"] = onlineUsers;
            }

            return onlineUsers;
        }
        #endregion

    }
}
