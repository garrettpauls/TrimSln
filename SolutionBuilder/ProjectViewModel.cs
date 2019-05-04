using Onion.SolutionParser.Parser.Model;
using ReactiveUI;

namespace SolutionBuilder
{
    public sealed class ProjectViewModel : ReactiveObject, ISolutionItemViewModel
    {
        private bool mIsExpanded;
        private bool? mIsIncluded = false;

        public ProjectViewModel(Project project)
        {
            Project = project;
        }

        public Project Project
        {
            get;
        }

        public bool IsExpanded
        {
            get => false;
            set => this.RaiseAndSetIfChanged(ref mIsExpanded, value);
        }

        public bool? IsIncluded
        {
            get => mIsIncluded;
            set => this.RaiseAndSetIfChanged(ref mIsIncluded, value);
        }

        public string Name => Project.Name;
    }
}
