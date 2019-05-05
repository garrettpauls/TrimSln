using System;
using Onion.SolutionParser.Parser.Model;
using ReactiveUI;

namespace TrimSln
{
    public sealed class ProjectFilter : ReactiveObject
    {
        private string mSearchText;

        public string SearchText
        {
            get => mSearchText;
            set => this.RaiseAndSetIfChanged(ref mSearchText, value);
        }

        public bool Matches(Project project)
        {
            var text = SearchText?.Trim();
            if (string.IsNullOrEmpty(text)) return true;

            return project.Name.Contains(text, StringComparison.OrdinalIgnoreCase);
        }
    }
}
