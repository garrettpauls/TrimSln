using System;
using System.Collections.Generic;
using System.Linq;
using DynamicData;
using Onion.SolutionParser.Parser.Model;
using ReactiveUI;
using TrimSln.Properties;

namespace TrimSln
{
    public sealed class FolderViewModel : ReactiveObject, ISolutionItemViewModel
    {
        private bool mIsExpanded = Settings.Default.ExpandedByDefault;
        private bool mMatchesFilter;

        public FolderViewModel(
            Project project,
            IReadOnlyCollection<(Project project, Guid? parentId)> allProjects,
            IObservable<ProjectFilter> filter)
        {
            Project = project;

            Children = allProjects
                .Where(x => x.parentId == project.Guid)
                .Select(x => SolutionViewModel.CreateItemViewModel(x.project, allProjects, filter))
                .OrderBy(x => x.Name)
                .ToArray();
            var childChanges = Children.AsObservableChangeSet();
            childChanges
                .AutoRefresh(x => x.IsIncluded)
                .Subscribe(_ => this.RaisePropertyChanged(nameof(IsIncluded)));
            childChanges
                .AutoRefresh(x => x.MatchesFilter)
                .Subscribe(_ => MatchesFilter = Children.Any(child => child.MatchesFilter));
        }

        public ISolutionItemViewModel[] Children
        {
            get;
        }

        public bool IsExpanded
        {
            get => mIsExpanded;
            set => this.RaiseAndSetIfChanged(ref mIsExpanded, value);
        }

        public bool? IsIncluded
        {
            get
            {
                if (Children.Any(child => child.IsIncluded == null)) return null;

                var includedCount = Children.Count(child => child.IsIncluded ?? true);
                if (includedCount == 0) return false;
                if (includedCount == Children.Length) return true;

                return null;
            }
            set
            {
                if (value.HasValue)
                    foreach (var child in Children) child.IsIncluded = value;
            }
        }

        public bool MatchesFilter
        {
            get => mMatchesFilter;
            private set => this.RaiseAndSetIfChanged(ref mMatchesFilter, value);
        }

        public string Name => Project.Name;

        public Project Project { get; }
    }
}
