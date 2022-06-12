using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;

namespace ProductScanner.App
{
    /// <summary>
    /// A dummy (null) command that can be used when a binding is not found or ready.
    /// </summary>
    public class NullCommand : ICommand
    {
        // warning CS0067 - The event 'ProductScanner.App.NullCommand.CanExecuteChanged' is never used

        #pragma warning disable 0067

        public bool CanExecute(object parameter)
        {
            return false;
        }

        public event EventHandler CanExecuteChanged;

        public void Execute(object parameter)
        {
            throw new NotImplementedException("NullCommand cannot be executed.");
        }

        #pragma warning restore 0067
    }
}
