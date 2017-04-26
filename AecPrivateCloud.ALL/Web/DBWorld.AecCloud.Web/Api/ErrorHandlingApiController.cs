using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http.Results;
using AecCloud.BaseCore;
using log4net;
using Microsoft.Owin.Security;

namespace DBWorld.AecCloud.Web.Api
{
    public abstract class ErrorHandlingApiController : BaseApiController
    {
        protected IAuthenticationManager Authentication { get; private set; }

        protected ErrorHandlingApiController(IAuthenticationManager authentication) : base()
        {
            Authentication = authentication;
        }

        protected internal ResponseMessageResult CreateResponse<T>(HttpStatusCode status, T content)
        {
            return ResponseMessage(Request.CreateResponse(status, content));
        }
        protected internal ResponseMessageResult CreateErrorResponse(string messagePrefix, HttpStatusCode statusCode, Exception ex, ILog log)
        {
            if (log == null) log = Logger.Log;
            var ex0 = ex;
            //log.Error(messagePrefix + "Outer; StackTrace: " + ex0.StackTrace);
            if (ex.InnerException != null)
            {
                ex0 = ex.InnerException;
                //log.Error(messagePrefix + "Inner; StackTrace: " + ex0.StackTrace);
                if (ex0.InnerException != null)
                {
                    ex0 = ex0.InnerException;
                    //log.Error(messagePrefix + "Inner2; StackTrace: " + ex0.StackTrace);
                }
            }
            var message = messagePrefix + ex0.Message;
            log.Error(message, ex0);
            return ResponseMessage(Request.CreateErrorResponse(statusCode, message));
        }
    }
}