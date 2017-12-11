// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FileController.cs" company="Cognite Ltd">
//   Copyright (c) 2013 Cognite Ltd (http://www.cognite.co.uk)
// </copyright>
// <summary>
//  File Controller Methods
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Web;
using System.Web.Mvc;
using WebMatrix.WebData;
using UniEBoard.Filters;
using UniEBoard.Service.Interfaces.ApplicationService;
using UniEBoard.Model.Enums;
using UniEBoard.Service.Models;
using UniEBoard.Service.ApplicationServices;

namespace UniEBoard.Controllers
{
    /// <summary>
    /// The File Controller
    /// </summary>
    [Authorize]
    [InitializeSimpleMembership]
    public class FileController : Controller
    {
        #region Members

        /// <summary>
        /// File Application Service 
        /// </summary>
        private IFileAppService _fileService;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="FileController"/> class.
        /// </summary>
        /// <param name="fileService">The file service.</param>
        public FileController(IFileAppService fileService)
        {
            this._fileService = fileService;
        }

        #endregion

        #region Index

        /// <summary>
        /// GET: /File/
        /// </summary>
        /// <returns></returns>
        [ActionName("Index")]
        public ActionResult Index()
        {
            return RedirectToAction("Index", "Home");
        }

        #endregion

        #region Download

        /// <summary>
        /// Downloads the specified file by id.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <returns></returns>
        public ActionResult Download(int id, Guid identityToken)
        {
            try
            {
                FileViewModel file = _fileService.GetFileByIdAndIdentityToken(id, identityToken);
                ContentDisposition contentDisposition = new System.Net.Mime.ContentDisposition
                {
                    FileName = file.FileName,
                    Inline = false,
                };

                Response.AppendHeader("Content-Disposition", contentDisposition.ToString());
                return File(file.Content, file.ContentType);
            }
            catch (Exception)
            {
                return RedirectToAction("Index", "Home");
            }
        }

        /// <summary>
        /// Removes the specified id.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <param name="identityToken">The identity token.</param>
        /// <returns></returns>
        [HttpPost]
        public void Remove(int id, Guid identityToken)
        {
            try
            {
                _fileService.RemoveFileByIdAndIdentityToken(id, identityToken);
            }
            catch (Exception)
            {
            }
        }

        #endregion
    }
}
