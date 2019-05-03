using System.Diagnostics;
using System.Windows;
using System.Windows.Threading;

namespace SolutionBuilder
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void _UnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
#if DEBUG
            if (System.Diagnostics.Debugger.IsAttached)
            {
                Debug.WriteLine(e.Exception);
                System.Diagnostics.Debugger.Break();
            }
#endif
        }
    }
}
