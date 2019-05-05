using Onion.SolutionParser.Parser.Model;
using ReactiveUI;

namespace TrimSln
{
    public interface ISolutionItemViewModel : IReactiveObject
    {
        bool IsExpanded
        {
            get;
            set;
        }

        bool? IsIncluded
        {
            get;
            set;
        }

        bool MatchesFilter
        {
            get;
        }

        string Name
        {
            get;
        }

        Project Project
        {
            get;
        }
    }
}
