using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Helpers;
using System.Web.Http;
using System.Web.Mvc;
using UniEBoard.Filters;
using UniEBoard.Service.Interfaces.ApplicationService;
using UniEBoard.Service.Models;

namespace UniEBoard.Controllers
{
    [InitializeSimpleMembership]
    public class AdminManageUsersController : ApiController
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

        /// <summary>
        /// Initializes a new instance of the <see cref="AdminManageUsersController"/> class.
        /// </summary>
        /// <param name="staffService">The staff service.</param>
        /// <param name="studentService">The student service.</param>
        public AdminManageUsersController(IStaffAppService staffService, IStudentAppService studentService)
        {
            this._staffService = staffService;
            this._studentService = studentService;
        }

        // GET api/administrator
        [System.Web.Mvc.Authorize]
        public StudentViewModel[] Get(HttpRequestMessage request)
        {
            string username = User.Identity.Name;
            IEnumerable<string> headerValues = request.Headers.GetValues("Authorization");

            StudentViewModel[] students = _studentService.GetAllStudents().ToArray();
            return students;
        }

        // GET api/administrator/5
        public StudentViewModel Get(HttpRequestMessage request, int id)
        {
            StudentViewModel student = _studentService.GetStudentByMemberShipId(id);
            return student;
        }

        // POST api/administrator
        public HttpResponseMessage Post(StudentViewModel student)
        {
            var response = Request.CreateResponse(HttpStatusCode.Created, student);
            return response;
        }

        // PUT api/administrator/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/administrator/5
        public void Delete(int id)
        {
        }

        ///// <summary>
        ///// Authenticates the user.
        ///// </summary>
        //private HttpResponseMessage AuthenticateUser(HttpRequestMessage request)
        //{
        //    var authHeader = request.Headers.Authorization;
        //    if (authHeader == null)
        //    {
        //        return CreateUnauthorizedResponse();
        //    }
        //    if (authHeader.Scheme != "Basic")
        //    {
        //        return CreateUnauthorizedResponse();
        //    }
        //}

        ///// <summary>
        ///// Creates the unauthorized response.
        ///// </summary>
        ///// <returns></returns>
        //private Task<HttpResponseMessage> CreateUnauthorizedResponse()
        //{
        //    var response = new HttpResponseMessage(HttpStatusCode.Unauthorized);
        //    response.Headers.Add("AUthentication required", "Basic");
        //    var taskCompletionSource = new TaskCompletionSource<HttpResponseMessage>();
        //    taskCompletionSource.SetResult(response);
        //    return taskCompletionSource.Task;
        //}
    }
}
