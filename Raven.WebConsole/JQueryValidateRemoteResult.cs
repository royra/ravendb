using System.Web.Mvc;

namespace Raven.WebConsole
{
    public class JQueryValidateRemoteResult : JsonResult
    {
        public bool IsValid { get; private set; }
        public string ErrorMessage { get; private set; }

        public JQueryValidateRemoteResult(string errorMessage = null)
        {
            Data = errorMessage ?? (object)true;
            JsonRequestBehavior = JsonRequestBehavior.AllowGet;
            IsValid = errorMessage == null;
            ErrorMessage = errorMessage;
        }
    }
}