using System;
using System.IO;
using System.Web;
using System.Web.Mvc;

namespace Raven.WebConsole
{
    public static class HtmlHelperExtensions
    {
        public static IDisposable BeginHtmlTemplate(this HtmlHelper helper, string id)
        {
            var writer = helper.ViewContext.Writer;
            writer.WriteLine(string.Format("<script type=\"text/html\" id=\"{0}\">", id));
            return new HtmlTemplate(writer);
        }

        private class HtmlTemplate : IDisposable
        {
            private readonly TextWriter writer;

            public HtmlTemplate(TextWriter writer)
            {
                this.writer = writer;
            }

            private bool disposed;

            public void Dispose()
            {
                if (disposed) return;

                writer.WriteLine("</script>");
                disposed = true;
            }
        }
    }
}