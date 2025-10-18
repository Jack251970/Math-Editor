namespace Editor
{
    public interface ISupportsUndo
    {
        void ProcessUndo(EquationAction action);
    }
}

