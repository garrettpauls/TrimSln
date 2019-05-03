using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Onion.SolutionParser.Parser.Model;

namespace SolutionBuilder
{
    public static class SolutionWriter
    {
        private static string _Format(GlobalSectionType type)
        {
            switch (type)
            {
                case GlobalSectionType.PostSolution:
                    return "postSolution";
                case GlobalSectionType.PreSolution:
                    return "preSolution";
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }

        private static string _Format(ProjectSectionType type)
        {
            switch (type)
            {
                case ProjectSectionType.PostProject:
                    return "postProject";
                case ProjectSectionType.PreProject:
                    return "preProject";
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }

        private static async Task _WriteGlobal(TextWriter writer, IEnumerable<GlobalSection> globals)
        {
            await writer.WriteLineAsync("Global");

            foreach (var section in globals)
            {
                await _WriteGlobalSection(writer, section);
            }

            await writer.WriteLineAsync("EndGlobal");
        }

        private static async Task _WriteGlobalSection(TextWriter writer, GlobalSection section)
        {
            await writer.WriteLineAsync($"\tGlobalSection({section.Name}) = {_Format(section.Type)}");
            foreach (var entry in section.Entries)
            {
                await writer.WriteLineAsync($"\t\t{entry.Key} = {entry.Value}");
            }

            await writer.WriteLineAsync("\tEndGlobalSection");
        }

        private static async Task _WriteProjects(TextWriter writer, IEnumerable<Project> projects)
        {
            foreach (var project in projects)
            {
                var guid = _FormatGuid(project.Guid);
                var typeGuid = _FormatGuid(project.TypeGuid);
                await writer.WriteLineAsync($@"Project(""{typeGuid}"") = ""{project.Name}"", ""{project.Path}"", ""{guid}""");
                await _WriteProjectSection(writer, project.ProjectSection);
                await writer.WriteLineAsync("EndProject");
            }

            string _FormatGuid(Guid guid) => "{" + guid.ToString("D").ToUpper() + "}";
        }

        private static async Task _WriteProjectSection(TextWriter writer, ProjectSection section)
        {
            if (section == null)
            {
                return;
            }

            await writer.WriteLineAsync($"\tProjectSection({section.Name}) = {_Format(section.Type)}");

            foreach (var entry in section.Entries)
            {
                await writer.WriteLineAsync($"\t\t{entry.Key} = {entry.Value}");
            }

            await writer.WriteLineAsync("\tEndProjectSection");
        }

        public static async Task<string> ToString(ISolution solution)
        {
            var sb = new StringBuilder();

            using (var writer = new StringWriter(sb))
            {
                await WriteAsync(writer, solution);
            }

            return sb.ToString();
        }

        public static async Task WriteAsync(TextWriter writer, ISolution solution)
        {
            // https://docs.microsoft.com/en-us/visualstudio/extensibility/internals/solution-dot-sln-file?view=vs-2019
            await writer.WriteLineAsync("Microsoft Visual Studio Solution File, Format Version 12.00");
            await writer.WriteLineAsync("# Visual Studio 15");
            await writer.WriteLineAsync("VisualStudioVersion = 15.0.27703.2026");
            await writer.WriteLineAsync("MinimumVisualStudioVersion = 10.0.40219.1");

            await _WriteProjects(writer, solution.Projects);
            await _WriteGlobal(writer, solution.Global);
        }
    }
}
