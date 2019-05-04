using ReactiveUI;

namespace SolutionBuilder
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

        string Name
        {
            get;
        }
    }
}
