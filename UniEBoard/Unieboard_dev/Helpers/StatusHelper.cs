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

namespace UniEBoard.Helpers
{
    public static class StatusHelper
    {
        public enum MessageStatus
        {
            Success = 1,
            Error = 2,
            Warning = 3
        }
        public static void SuccessMessage(string msg, ControllerBase cb)
        {
            cb.TempData["Status"] = MessageStatus.Success;
            cb.TempData["StatusMessage"] = msg;
        }
        public static void ErrorMessage(string msg, ControllerBase cb)
        {
            cb.TempData["Status"] = MessageStatus.Error;
            cb.TempData["StatusMessage"] = msg;
        }
        public static void WarningMessage(string msg, ControllerBase cb)
        {
            cb.TempData["Status"] = MessageStatus.Warning;
            cb.TempData["StatusMessage"] = msg;
        }
    }
}