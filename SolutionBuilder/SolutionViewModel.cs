using System;
using System.Collections.Generic;
using System.Linq;
using Onion.SolutionParser.Parser;
using Onion.SolutionParser.Parser.Model;
using ReactiveUI;

namespace SolutionBuilder
{
    public sealed class SolutionViewModel : ReactiveObject
    {
        private readonly ISolution mSolution;

        private SolutionViewModel(ISolution solution)
        {
            mSolution = solution;

            var childToParent = solution
                .Global.FirstOrDefault(section => section.Name == "NestedProjects")
                ?.Entries.Select(kvp => new KeyValuePair<Guid, Guid>(Guid.Parse(kvp.Key), Guid.Parse(kvp.Value)))
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value) ?? new Dictionary<Guid, Guid>();

            var allProjects = solution
                .Projects
                .Select(proj => (proj, childToParent.TryGetValue(proj.Guid, out var parentId) ? parentId : (Guid?)null))
                .ToArray();

            Projects = solution
                .Projects
                .Where(proj => !childToParent.ContainsKey(proj.Guid))
                .Select(proj => CreateItemViewModel(proj, allProjects))
                .ToArray();
        }

        public ISolutionItemViewModel[] Projects
        {
            get;
        }

        public static ISolutionItemViewModel CreateItemViewModel(Project project, IReadOnlyCollection<(Project project, Guid? parentId)> allProjects)
        {
            if (project.TypeGuid == ProjectTypeIds.Folder)
            {
                return new FolderViewModel(project, allProjects);
            }

            return new ProjectViewModel(project);
        }

        public static SolutionViewModel LoadFile(string file)
        {
            var solution = SolutionParser.Parse(file);
            return new SolutionViewModel(solution);
        }
    }
}
