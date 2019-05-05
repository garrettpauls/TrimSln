using System.Windows.Input;
using MahApps.Metro.Controls;
using Microsoft.Win32;

namespace TrimSln
{
    public partial class MainWindow : MetroWindow, IUserInteractionManager
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainWindowViewModel(this);
        }

        public string PromptToOpenSolution()
        {
            var ofd = new OpenFileDialog
            {
                Filter = "Visual Studio Solutions|*.sln",
                RestoreDirectory = true
            };

            if (ofd.ShowDialog(this) ?? false) return ofd.FileName;

            return null;
        }

        public string PromptToSaveSolution(string initialDirectory)
        {
            var sfd = new SaveFileDialog
            {
                Filter = "Visual Studio Solution|*.sln",
                InitialDirectory = initialDirectory,
                OverwritePrompt = true
            };

            if (sfd.ShowDialog(this) ?? false) return sfd.FileName;

            return null;
        }

        private void _HandleSearch(object sender, ExecutedRoutedEventArgs e)
        {
            mSearchBox.Focus();
        }
    }
}
