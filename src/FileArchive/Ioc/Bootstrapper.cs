using FileArchive.Commands;
using FileArchive.Services;
using FileArchive.ViewModels;

namespace FileArchive.Ioc
{
    public class Bootstrapper
    {
        public ILocator Locator { get; }

        public Bootstrapper(IIocContainer container)
        {
            container.Register<MainViewModel, IMainViewModel>();
            container.Register<FileViewModel, IFileViewModel>();
            container.Register<ILocator>(() => new Locator(container));
            container.Register<FileSystemService, IFileSystemService>();
            container.Register<FileService, IFileService>();
            container.Register<ConfigurationService, IConfigurationService>();
            container.Register<CommandFactory, ICommandFactory>();
            container.Register(() =>
            {
                var wnd = new MainWindow();
                LocatorExtension.SetLocator(wnd, Locator);
                return wnd;
            });

            Locator = container.GetInstance<ILocator>();
        }
    }
}
