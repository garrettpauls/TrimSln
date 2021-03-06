﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using Onion.SolutionParser.Parser;
using Onion.SolutionParser.Parser.Model;
using ReactiveUI;

namespace TrimSln
{
    public sealed class SolutionViewModel : ReactiveObject
    {
        private readonly string mHeader;
        private readonly ISolution mSolution;

        private SolutionViewModel(string filePath, ISolution solution, string header)
        {
            FilePath = filePath;
            mSolution = solution;
            mHeader = header;

            var childToParent = solution
                .Global.FirstOrDefault(section => section.Name == "NestedProjects")
                ?.Entries.Select(_ParseNestedProjectEntry)
                .ToDictionary(kvp => kvp.child, kvp => kvp.parent) ?? new Dictionary<Guid, Guid>();

            var allProjects = solution
                .Projects
                .Select(proj => (proj, childToParent.TryGetValue(proj.Guid, out var parentId) ? parentId : (Guid?)null))
                .ToArray();

            var filter = Filter.Changed.Select(x => (ProjectFilter) x.Sender);

            Projects = solution
                .Projects
                .Where(proj => !childToParent.ContainsKey(proj.Guid))
                .Select(proj => CreateItemViewModel(proj, allProjects, filter))
                .OrderBy(proj => proj.Name)
                .ToArray();

            CollapseCommand = ReactiveCommand.Create(() => _ForEachProject(vm => vm.IsExpanded = false), outputScheduler: DispatcherScheduler.Current);
            ExpandCommand = ReactiveCommand.Create(() => _ForEachProject(vm => vm.IsExpanded = true), outputScheduler: DispatcherScheduler.Current);
        }

        public ReactiveCommand<Unit, Unit> CollapseCommand { get; }

        public ReactiveCommand<Unit, Unit> ExpandCommand { get; }

        public string FilePath
        {
            get;
        }

        public ProjectFilter Filter { get; } = new ProjectFilter();

        public ISolutionItemViewModel[] Projects
        {
            get;
        }

        private IEnumerable<(Project project, bool included)> _FlattenProjects()
        {
            return Projects.SelectMany(Flatten);

            IEnumerable<(Project project, bool included)> Flatten(ISolutionItemViewModel item)
            {
                yield return (item.Project, item.IsIncluded ?? true);
                if (item is FolderViewModel folder)
                    foreach (var child in folder.Children.SelectMany(Flatten)) yield return child;
            }
        }

        private void _ForEachProject(Action<ISolutionItemViewModel> action)
        {
            foreach (var item in Projects) ForEachProject(item);

            void ForEachProject(ISolutionItemViewModel item)
            {
                action(item);

                if (item is FolderViewModel folder)
                    foreach (var child in folder.Children) ForEachProject(child);
            }
        }

        private static string _GetHeader(string file)
        {
            var sb = new StringBuilder();

            using (var reader = new StreamReader(file))
            {
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    if (line == null || line.StartsWith("Project", StringComparison.OrdinalIgnoreCase)) break;

                    sb.AppendLine(line);
                }
            }

            return sb.ToString();
        }

        private IReadOnlyCollection<GlobalSection> _GetTrimmedGlobals(ISet<Guid> removedProjects)
        {
            var globals = new List<GlobalSection>();
            foreach (var section in mSolution.Global)
                switch (section.Name)
                {
                    case "ProjectConfigurationPlatforms":
                        globals.Add(new GlobalSection(section.Name, section.Type)
                        {
                            Entries = section.Entries
                                .Where(kvp => !removedProjects.Contains(Guid.Parse(kvp.Key.Split('.').First())))
                                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value)
                        });
                        break;
                    case "NestedProjects":
                        globals.Add(new GlobalSection(section.Name, section.Type)
                        {
                            Entries = section.Entries
                                .Select(_ParseNestedProjectEntry)
                                .Where(x => !removedProjects.Contains(x.child) && !removedProjects.Contains(x.parent))
                                .ToDictionary(x => SolutionWriter.Format(x.child), x => SolutionWriter.Format(x.parent))
                        });
                        break;
                    default:
                        globals.Add(section);
                        break;
                }

            return globals;
        }

        private ISolution _GetTrimmedSolution(string relativePath)
        {
            var isProjectIncluded = _FlattenProjects().ToDictionary(x => x.project.Guid, x => x.included);

            var projects = new List<Project>();
            var removedProjects = new HashSet<Guid>();

            // a bit of extra work to ensure projects stay in the same order
            // doesn't really matter, but makes the resulting solution a bit closer to the original
            foreach (var project in mSolution.Projects)
                if (isProjectIncluded[project.Guid])
                {
                    var newPath = relativePath + project.Path;
                    var newProject = new Project(project.TypeGuid, project.Name, newPath, project.Guid)
                    {
                        ProjectSection = project.ProjectSection
                    };

                    if (project.ProjectSection?.Name == "SolutionItems")
                        project.ProjectSection.Entries = project
                            .ProjectSection.Entries
                            .ToDictionary(kvp => relativePath + kvp.Key, kvp => relativePath + kvp.Value);

                    projects.Add(newProject);
                }
                else
                {
                    removedProjects.Add(project.Guid);
                }

            var globals = _GetTrimmedGlobals(removedProjects);

            return new Solution
            {
                Projects = projects,
                Global = globals
            };
        }

        private (Guid child, Guid parent) _ParseNestedProjectEntry(KeyValuePair<string, string> item)
        {
            return (Guid.Parse(item.Key), Guid.Parse(item.Value));
        }

        public static ISolutionItemViewModel CreateItemViewModel(
            Project project,
            IReadOnlyCollection<(Project project, Guid? parentId)> allProjects,
            IObservable<ProjectFilter> filter)
        {
            if (project.TypeGuid == ProjectTypeIds.Folder) return new FolderViewModel(project, allProjects, filter);

            return new ProjectViewModel(project, filter);
        }

        public IEnumerable<ProjectViewModel> GetAllProjects()
        {
            return Projects.SelectMany(Flatten);

            IEnumerable<ProjectViewModel> Flatten(ISolutionItemViewModel item)
            {
                switch (item)
                {
                    case ProjectViewModel pvm:
                        yield return pvm;
                        break;
                    case FolderViewModel fvm:
                        foreach (var child in fvm.Children.SelectMany(Flatten)) yield return child;
                        break;
                }
            }
        }

        public static SolutionViewModel LoadFile(string file)
        {
            var solution = SolutionParser.Parse(file);
            var header = _GetHeader(file);
            return new SolutionViewModel(file, solution, header);
        }

        public async Task SaveToFileAsync(string file)
        {
            var newUri = new Uri(Path.GetDirectoryName(file) + "\\");
            var oldUri = new Uri(Path.GetDirectoryName(FilePath) + "\\");
            var relativePath = newUri.MakeRelativeUri(oldUri).OriginalString.Replace("/", "\\");

            var updated = _GetTrimmedSolution(relativePath);

            using (var writer = new StreamWriter(file, false, Encoding.UTF8))
            {
                await SolutionWriter.WriteAsync(writer, updated, mHeader);
            }
        }

        public void SelectProjectsInSolution(string slnFile)
        {
            ISolution solution;
            try
            {
                solution = SolutionParser.Parse(slnFile);
            }
            catch
            {
                return;
            }

            var projByPath = GetAllProjects()
                .ToDictionary(proj => proj.Project.Path, StringComparer.OrdinalIgnoreCase);
            foreach (var project in solution.Projects)
                if (projByPath.TryGetValue(project.Path, out var vm))
                {
                    vm.IsIncluded = true;
                    vm.IsExpanded = true;
                }
        }
    }
}
