using System;
using System.Web;

namespace Raven.WebConsole
{
    public class ClientVisibleException : Exception
    {
        public string Code { get; set; }
        public string ClientVisibleMessage { get; set; }
    }
}