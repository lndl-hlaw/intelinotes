using System;
using System.Windows.Input;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InteliNotes
{
    class KeyCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;
        private Action function;
        public KeyCommand(Action function)
        {
            this.function = function;
        }
        public bool CanExecute(object parameter)
        {
            throw new NotImplementedException();
        }

        public void Execute(object parameter = null)
        {
            function.Invoke();
        }
    }
}
