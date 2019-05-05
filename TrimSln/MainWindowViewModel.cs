using System.IO;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using ReactiveUI;

namespace TrimSln
{
    public sealed class MainWindowViewModel : ReactiveObject
    {
        private readonly IUserInteractionManager mUserInteractionManager;

        private SolutionViewModel mSolution;

        public MainWindowViewModel(IUserInteractionManager userInteractionManager)
        {
            mUserInteractionManager = userInteractionManager;

            var canSave = this.WhenAnyValue(x => x.Solution).Select(sln => sln != null);
            SaveCommand = ReactiveCommand.Create(_Save, canSave, DispatcherScheduler.Current);
            OpenCommand = ReactiveCommand.Create(_Open, outputScheduler: DispatcherScheduler.Current);
        }

        public ReactiveCommand<Unit, Unit> OpenCommand
        {
            get;
        }

        public ReactiveCommand<Unit, Unit> SaveCommand
        {
            get;
        }

        public SolutionViewModel Solution
        {
            get => mSolution;
            private set => this.RaiseAndSetIfChanged(ref mSolution, value);
        }

        private void _Open()
        {
            var file = mUserInteractionManager.PromptToOpenSolution();
            if (file == null) return;

            var sln = SolutionViewModel.LoadFile(file);
            Solution = sln;
        }

        private async void _Save()
        {
            var sln = Solution;
            if (sln == null) return;

            var file = mUserInteractionManager.PromptToSaveSolution(Path.GetDirectoryName(sln.FilePath));
            if (file == null) return;

            await sln.SaveToFileAsync(file);
        }
    }
}
