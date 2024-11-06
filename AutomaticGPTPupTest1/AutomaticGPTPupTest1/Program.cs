//using CefSharp;
//using CefSharp.WinForms;

namespace AutomaticGPTPupTest1
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            //var settings = new CefSettings()
            //{
            //    //By default CefSharp will use an in-memory cache, you need to specify a Cache Folder to persist data
            //    CachePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "CefSharp\\Cache")
            //};
            //Cef.Initialize(settings, performDependencyCheck: true, browserProcessHandler: null);

            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();
            Application.Run(new Form1());
        }
    }
}