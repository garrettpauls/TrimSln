using System.Reactive;
using System.Reactive.Concurrency;
using System.Windows.Threading;
using ReactiveUI;

namespace SolutionBuilder
{
    public sealed class MainWindowViewModel : ReactiveObject
    {
        private readonly IUserInteractionManager mUserInteractionManager;
        private SolutionViewModel mSolution;

        public MainWindowViewModel(IUserInteractionManager userInteractionManager)
        {
            mUserInteractionManager = userInteractionManager;
            OpenCommand = ReactiveCommand.Create(_Open, outputScheduler: new DispatcherScheduler(Dispatcher.CurrentDispatcher));
        }

        public ReactiveCommand<Unit, Unit> OpenCommand { get; }

        public SolutionViewModel Solution
        {
            get => mSolution;
            private set => this.RaiseAndSetIfChanged(ref mSolution, value);
        }

        private void _Open()
        {
            var file = mUserInteractionManager.PromptToOpenSolution();
            if (file == null)
            {
                return;
            }

            var sln = SolutionViewModel.LoadFile(file);
            Solution = sln;
        }
    }
}
