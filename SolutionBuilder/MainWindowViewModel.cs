using System.Reactive;
using System.Threading.Tasks;
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
            OpenCommand = ReactiveCommand.CreateFromTask(_Open);
        }

        public ReactiveCommand<Unit, Unit> OpenCommand { get; }

        public SolutionViewModel Solution
        {
            get => mSolution;
            private set => this.RaiseAndSetIfChanged(ref mSolution, value);
        }

        private async Task _Open()
        {
            var file = mUserInteractionManager.PromptToOpenSolution();
            if (file == null)
            {
                return;
            }

            var sln = await SolutionViewModel.LoadFile(file);
            Solution = sln;
        }
    }

    public sealed class SolutionViewModel : ReactiveObject
    {
        private SolutionViewModel()
        {
        }

        public static Task<SolutionViewModel> LoadFile(string file)
        {
            return Task.FromResult(new SolutionViewModel());
        }
    }
}
