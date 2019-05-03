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
        }

        public static SolutionViewModel LoadFile(string file)
        {
            var solution = SolutionParser.Parse(file);
            return new SolutionViewModel(solution);
        }
    }
}
