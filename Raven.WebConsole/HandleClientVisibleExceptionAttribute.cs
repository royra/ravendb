using System.Web.Mvc;
using Raven.Imports.Newtonsoft.Json;

namespace Raven.WebConsole
{
    public class HandleClientVisibleExceptionAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            var exception = filterContext.Exception as ClientVisibleException;
            if (exception == null || filterContext.ExceptionHandled)
                return;

            filterContext.Result = new ClientVisibleExceptionResult() {Exception = exception};
        }

        public class ClientVisibleExceptionResult : ActionResult
        {
            public ClientVisibleException Exception { get; set; }

            public override void ExecuteResult(ControllerContext context)
            {
                var response = context.HttpContext.Response;

                response.ContentType = "application/json";

                var content = new
                {
                    Code = Exception.Code,
                    Message = Exception.ClientVisibleMessage,
                };

                response.Write(JsonConvert.SerializeObject(content));
            }
        }
    }
}