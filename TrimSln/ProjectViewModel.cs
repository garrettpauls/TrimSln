using System;
using Onion.SolutionParser.Parser.Model;
using ReactiveUI;

namespace TrimSln
{
    public sealed class ProjectViewModel : ReactiveObject, ISolutionItemViewModel
    {
        private bool? mIsIncluded = false;
        private bool mMatchesFilter = true;

        public ProjectViewModel(Project project, IObservable<ProjectFilter> filter)
        {
            Project = project;
            filter.Subscribe(f => MatchesFilter = f.Matches(project));
        }

        public bool IsExpanded
        {
            get => false;
            set { }
        }

        public bool? IsIncluded
        {
            get => mIsIncluded;
            set => this.RaiseAndSetIfChanged(ref mIsIncluded, value);
        }

        public bool MatchesFilter
        {
            get => mMatchesFilter;
            private set => this.RaiseAndSetIfChanged(ref mMatchesFilter, value);
        }

        public string Name => Project.Name;

        public Project Project
        {
            get;
        }
    }
}
