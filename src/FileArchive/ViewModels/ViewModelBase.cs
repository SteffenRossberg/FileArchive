using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace FileArchive.ViewModels
{
    public class ViewModelBase : INotifyPropertyChanged
    {
        protected ViewModelBase(){}

        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        protected bool Set<TValue>(ref TValue field, TValue value, [CallerMemberName] string propertyName = null)
        {
            if (Equals(field, value)) return false;
            field = value;
            RaisePropertyChanged(propertyName);
            return true;
        }
    }
}
