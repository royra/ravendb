using System;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;

namespace Raven.WebConsole.Main 
{
    // taken from https://blogs.msdn.com/b/carlosag/archive/2008/04/14/hostyourownwebserverusingiis7.aspx?Redirected=true
    public class WebServer : IDisposable 
    {
        private readonly string appHostConfigPath;
        private readonly string rootWebConfigPath;

        public WebServer(string physicalPathToSite, int port, string name)
        {
            var baseResourcesDirectory = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, @"Resources"));

            var applicationHostConfigContents = File.ReadAllText(Path.Combine(baseResourcesDirectory, "applicationHost.config"));

            applicationHostConfigContents = applicationHostConfigContents
                .Replace("${name}", name)
                .Replace("${path}", physicalPathToSite)
                .Replace("${port}", port.ToString(CultureInfo.InvariantCulture));

            appHostConfigPath = Path.Combine(Path.GetTempPath(), Path.GetTempFileName() + ".applicationHost.config");
            File.WriteAllText(appHostConfigPath, applicationHostConfigContents);

            rootWebConfigPath = Path.Combine(baseResourcesDirectory, "root.web.config"); 

            //Console.WriteLine(appHostConfigPath);
            //Console.WriteLine(rootWebConfigPath);
        }

        ~WebServer() 
        {
            Dispose(false);
        }

        public void Dispose() 
        {
            Dispose(true);
        }

        private void Dispose(bool disposing) 
        {
            if (disposing) 
            {
                GC.SuppressFinalize(this);
            }

            Stop();
        }

        public void Start() 
        {
            if (!HostableWebCore.IsActivated) 
            {
                HostableWebCore.Activate(appHostConfigPath, rootWebConfigPath, Guid.NewGuid().ToString());
            }
        }

        public void Stop() 
        {
            if (HostableWebCore.IsActivated) 
            {
                HostableWebCore.Shutdown(false);
            }
        }

        private static class HostableWebCore 
        {
            private static bool isActivated;

            private delegate int FnWebCoreActivate(
                [In, MarshalAs(UnmanagedType.LPWStr)]string appHostConfig, 
                [In, MarshalAs(UnmanagedType.LPWStr)]string rootWebConfig, 
                [In, MarshalAs(UnmanagedType.LPWStr)]string instanceName);

            private delegate int FnWebCoreShutdown(bool immediate);

            private static readonly FnWebCoreActivate webCoreActivate;
            private static readonly FnWebCoreShutdown webCoreShutdown;

            static HostableWebCore() {
                // Load the library and get the function pointers for the WebCore entry points
                const string hwcPath = @"%windir%\system32\inetsrv\hwebcore.dll";
                IntPtr hwc = NativeMethods.LoadLibrary(Environment.ExpandEnvironmentVariables(hwcPath));

                IntPtr procaddr = NativeMethods.GetProcAddress(hwc, "WebCoreActivate");
                webCoreActivate = (FnWebCoreActivate)Marshal.GetDelegateForFunctionPointer(procaddr, typeof(FnWebCoreActivate));

                procaddr = NativeMethods.GetProcAddress(hwc, "WebCoreShutdown");
                webCoreShutdown = (FnWebCoreShutdown)Marshal.GetDelegateForFunctionPointer(procaddr, typeof(FnWebCoreShutdown));
            }

            /// <summary>
            /// Specifies if Hostable WebCore has been activated
            /// </summary>
            public static bool IsActivated { get { return isActivated; } }

            /// <summary>
            /// Activate the HWC
            /// </summary>
            /// <param name="appHostConfig">Path to ApplicationHost.config to use</param>
            /// <param name="rootWebConfig">Path to the Root Web.config to use</param>
            /// <param name="instanceName">Name for this instance</param>
            public static void Activate(string appHostConfig, string rootWebConfig, string instanceName) 
            {
                int result = webCoreActivate(appHostConfig, rootWebConfig, instanceName);
                if (result != 0) 
                {
                    Marshal.ThrowExceptionForHR(result);
                }

                isActivated = true;
            }

            /// <summary>
            /// Shutdown HWC
            /// </summary>
            public static void Shutdown(bool immediate) 
            {
                if (isActivated) 
                {
                    webCoreShutdown(immediate);
                    isActivated = false;
                }
            }

            private static class NativeMethods 
            {
                [DllImport("kernel32.dll")]
                internal static extern IntPtr LoadLibrary(String dllname);

                [DllImport("kernel32.dll")]
                internal static extern IntPtr GetProcAddress(IntPtr hModule, String procname);
            }
        }
    }
}
