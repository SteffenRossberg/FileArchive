using System;
using System.Windows.Input;

namespace FileArchive.Commands
{
    public interface ICommandFactory
    {
        ICommand Create(Action execute, Func<bool> canExecute = null);
        ICommand Create<TParameter>(Action<TParameter> execute, Func<TParameter, bool> canExecute = null);
        ICommand Create(Action<object> execute, Func<object, bool> canExecute);
    }

    public class CommandFactory : ICommandFactory
    {
        public ICommand Create(Action execute, Func<bool> canExecute = null)
        {
            canExecute ??= (() => true);
            return new DelegateCommand(parameter => execute(), parameter => canExecute());
        }

        public ICommand Create<TParameter>(Action<TParameter> execute, Func<TParameter, bool> canExecute = null)
        {
            canExecute ??= (parameter => true);
            return new DelegateCommand(parameter => execute((TParameter) parameter), parameter => canExecute((TParameter) parameter));
        }

        public ICommand Create(Action<object> execute, Func<object, bool> canExecute)
        {
            canExecute ??= (parameter => true);
            return new DelegateCommand(execute, canExecute);
        }
    }
}
