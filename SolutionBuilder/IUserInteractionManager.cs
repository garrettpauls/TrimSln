namespace SolutionBuilder
{
    public interface IUserInteractionManager
    {
        string PromptToOpenSolution();
        string PromptToSaveSolution(string initialDirectory);
    }
}