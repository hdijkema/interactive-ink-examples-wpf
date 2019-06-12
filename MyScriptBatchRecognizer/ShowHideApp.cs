using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MyScriptBatchRecognizer
{
    class ShowHideApp : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private MainWindow _win;

        public ShowHideApp()
        {

        }

        public void setWindow(MainWindow win)
        {
            _win = win;
        }

        public bool CanExecute(object parameter)
        {
            return _win != null;
        }

        public void hide()
        {
            _win.Hide();
        }

        public void Execute(object parameter)
        {
            if (_win.IsVisible)
            {
                _win.Hide();
            } else
            {
                _win.Show();
            }
        }
    }
}
