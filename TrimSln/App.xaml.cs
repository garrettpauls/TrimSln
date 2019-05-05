using System.Diagnostics;
using System.Windows;
using System.Windows.Threading;

namespace TrimSln
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void _UnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
#if DEBUG
            if (Debugger.IsAttached)
            {
                Debug.WriteLine(e.Exception);
                Debugger.Break();
            }
#endif
        }
    }
}
