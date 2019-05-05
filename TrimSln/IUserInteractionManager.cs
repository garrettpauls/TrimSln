namespace TrimSln
{
    public interface IUserInteractionManager
    {
        string PromptToOpenSolution();
        string PromptToSaveSolution(string initialDirectory);
    }
}
