using System;
using System.Collections.Generic;
using System.Linq;
using DynamicData;
using Onion.SolutionParser.Parser.Model;
using ReactiveUI;

namespace SolutionBuilder
{
    public sealed class FolderViewModel : ReactiveObject, ISolutionItemViewModel
    {
        private readonly Project mProject;

        private bool mIsExpanded;

        public FolderViewModel(Project project, IReadOnlyCollection<(Project project, Guid? parentId)> allProjects)
        {
            mProject = project;

            IsFolder = project.TypeGuid == ProjectTypeIds.Folder;
            Children = allProjects
                .Where(x => x.parentId == project.Guid)
                .Select(x => SolutionViewModel.CreateItemViewModel(x.project, allProjects))
                .ToArray();
            Children.AsObservableChangeSet()
                .AutoRefresh(x => x.IsIncluded)
                .Subscribe(x => this.RaisePropertyChanged(nameof(IsIncluded)));
        }

        public ISolutionItemViewModel[] Children
        {
            get;
        }

        public bool IsFolder
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
                if (Children.Any(child => child.IsIncluded == null))
                {
                    return null;
                }

                var includedCount = Children.Count(child => child.IsIncluded ?? true);
                if (includedCount == 0)
                {
                    return false;
                }
                if (includedCount == Children.Length)
                {
                    return true;
                }

                return null;
            }
            set
            {
                if (value.HasValue)
                {
                    foreach (var child in Children)
                    {
                        child.IsIncluded = value;
                    }
                }
            }
        }

        public string Name => mProject.Name;
    }
}
