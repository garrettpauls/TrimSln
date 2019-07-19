using System.Windows;
using System.Windows.Controls.Primitives;
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

        private void _ShowContextMenu(object sender, RoutedEventArgs e)
        {
            if (!(sender is FrameworkElement element)) return;

            var menu = element.ContextMenu;
            if (menu == null) return;

            menu.PlacementTarget = element;
            menu.Placement = PlacementMode.Bottom;
            menu.IsOpen = true;
            e.Handled = true;
        }
    }
}
