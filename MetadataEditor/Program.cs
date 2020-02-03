//using Autofac;
using MetadataEditor.AL;
using MetadataEditor.DAL;
using SharedLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using SimpleInjector;

namespace MetadataEditor
{
    static class Program
    {
        static readonly Container container;

        static Program() {
            container = new Container();
        }

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            container.Register<IAppLogic, AppLogic>(Lifestyle.Singleton);
            container.Register<IFileSystemAccess, FileSystemAccess>(Lifestyle.Singleton);
            container.Register<IAlbumInfoProvider, AlbumInfo>(Lifestyle.Singleton);
            container.Register<FormMain>(Lifestyle.Singleton);

            //container.Verify();

            Application.Run(container.GetInstance<FormMain>());
        }
    }
}