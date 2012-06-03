using System;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Web;
using System.Web.Mvc;
using Raven.Bundles.Authentication;
using Raven.Client;

namespace Raven.WebConsole.Controllers
{
    [Authorize]
    [HandleClientVisibleException]
    public abstract class ContentController : BaseController
    {
        protected ContentController(IDocumentSession session) : base(session)
        {
        }

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            ViewBag.User = User.Identity.Name;
        }

        protected override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            if (!HttpContext.Items.Contains(Keys.HttpContextItems.HAS_UI_MESSAGE))
            {
                var messageCookie = Request.Cookies[Keys.Cookies.UI_MESSAGE];
                if (messageCookie != null)
                {
                    var parts = messageCookie.Value.Split(new[] {'/'}, 2);
                    if (parts.Length == 2 && !string.IsNullOrWhiteSpace(parts[1]))
                    {
                        ViewBag.Message = Encoding.UTF8.GetString(Convert.FromBase64String(parts[1]));
                        MessageLevel level;
                        if (!Enum.TryParse(parts[0], true, out level))
                            level = MessageLevel.Info;

                        ViewBag.MessageLevel = level.ToString().ToLowerInvariant();
                    }

                    RemoveMessage(); // only display once
                }
            }

            base.OnActionExecuted(filterContext);
        }

        protected void SetMessage(string message, MessageLevel level = MessageLevel.Info)
        {
            message = Convert.ToBase64String(Encoding.UTF8.GetBytes(message));
            HttpContext.Items[Keys.HttpContextItems.HAS_UI_MESSAGE] = true;
            Response.Cookies.Set(new HttpCookie(Keys.Cookies.UI_MESSAGE, string.Format("{0}/{1}", level.ToString().ToLowerInvariant(), message)));
        }

        private static readonly DateTime expireTime = new DateTime(1970, 1, 1);

        protected void RemoveMessage()
        {
            if (Request.Cookies[Keys.Cookies.UI_MESSAGE] != null)
                Response.Cookies.Set(new HttpCookie(Keys.Cookies.UI_MESSAGE, "") { Expires = expireTime });
        }

        protected enum MessageLevel
        {
            Info, Note, Warning, Tip
        }

        public virtual ActionResult Index()
        {
            return View();
        }
    }
}