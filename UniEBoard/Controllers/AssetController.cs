using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Web;
using System.Web.Mvc;
using WebMatrix.WebData;
using UniEBoard.Filters;
using UniEBoard.Service.Interfaces.ApplicationService;
using UniEBoard.Service.ApplicationServices;
using UniEBoard.Service.Models;

namespace UniEBoard.Controllers
{
    [Authorize]
    public class AssetController : Controller
    {
        #region Members

        /// <summary>
        /// Asset Application Service 
        /// </summary>
        private IAssetAppService _assetService;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="AssetController"/> class.
        /// </summary>
        /// <param name="assetService">The asset service.</param>
        public AssetController(IAssetAppService assetService)
        {
            this._assetService = assetService;
        }

        #endregion

        #region Methods

        /// <summary>
        /// GET: /Asset/
        /// </summary>
        /// <returns></returns>
        [ActionName("Index")]
        public ActionResult Index()
        {
            return RedirectToAction("Index", "Home");
        }

        #endregion

        /// <summary>
        /// GET: /Asset/Render
        /// </summary>
        /// <returns></returns>
        public ActionResult Render(int id)
        {
            try
            {
                AssetViewModel asset = _assetService.GetAssetById(id);
                return RenderContent(asset.Name, asset.Path, asset.ContentType, true);
            }
            catch (Exception)
            {
                return new HttpStatusCodeResult((int)System.Net.HttpStatusCode.NotFound);
            }
        }

        /// <summary>
        /// Renders the alternate video.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <returns></returns>
        public ActionResult RenderAlternateVideo(int id)
        {
            try
            {
                VideoViewModel asset = _assetService.GetVideoById(id);
                return RenderContent(asset.Name, asset.AlternatePath, asset.AlternateContentType, false);
            }
            catch (Exception)
            {
                return new HttpStatusCodeResult((int)System.Net.HttpStatusCode.NotFound);
            }
        }

        /// <summary>
        /// Renders the content.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="path">The path.</param>
        /// <param name="contentType">Type of the content.</param>
        /// <param name="inline">if set to <c>true</c> [inline].</param>
        /// <returns></returns>
        private ActionResult RenderContent(string fileName, string path, string contentType, bool inline)
        {
            string lowerpath = path.ToLower();
            if (lowerpath.StartsWith("http") || lowerpath.StartsWith("www")) { return new RedirectResult(path); }

            ContentDisposition contentDisposition = new System.Net.Mime.ContentDisposition
            {
                FileName = fileName,
                Inline = inline,
            };
            Response.AppendHeader("Content-Disposition", contentDisposition.ToString());
            return File(path, contentType);
        }
    }
}
