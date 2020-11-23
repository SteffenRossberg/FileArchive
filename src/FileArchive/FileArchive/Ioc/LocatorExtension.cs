using System.Windows;

namespace FileArchive.Ioc
{
    public static class LocatorExtension
    {
        #region LocatorProperty

        public static readonly DependencyProperty LocatorProperty =
            DependencyProperty.RegisterAttached("Locator", typeof(ILocator), typeof(LocatorExtension),
                new FrameworkPropertyMetadata(default(ILocator), FrameworkPropertyMetadataOptions.Inherits));

        public static ILocator GetLocator(DependencyObject obj) => (ILocator) obj.GetValue(LocatorProperty);

        public static void SetLocator(DependencyObject obj, ILocator value) => obj.SetValue(LocatorProperty, value);

        #endregion
    }
}
