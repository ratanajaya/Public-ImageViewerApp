using MetadataEditor.AL;
using SharedLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using SimpleInjector;
using System.Net.Http;
using System.Configuration;

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
            container.Register<IAlbumInfoProvider, AlbumInfoProvider>(Lifestyle.Singleton);
            container.Register<IApiAccess, ApiAccess>(Lifestyle.Singleton);
            container.Register<FormMain>(Lifestyle.Singleton);
            //container.Register<HttpClient>(() => new HttpClient(), Lifestyle.Transient);
            var configuration = new ConfigurationModel {
                ApiBaseUrl = ConfigurationManager.AppSettings["ApiBaseUrl"]
            };
            container.RegisterInstance<ConfigurationModel>(configuration);

            container.Verify();

            Application.Run(container.GetInstance<FormMain>());

            //Application.Run(new FormMain(new AppLogic(new AlbumInfo(), new FileSystemAccess(), new ApiAccess(configuration))));
        }
    }
}