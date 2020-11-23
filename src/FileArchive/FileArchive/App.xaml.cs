using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using FileArchive.Ioc;

namespace FileArchive
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            var bootstrapper = new Bootstrapper(new IocContainer());
            var window = bootstrapper.Locator.Get<MainWindow>();
            window.Show();
            MainWindow = window;
            ShutdownMode = ShutdownMode.OnMainWindowClose;
        }
    }
}
