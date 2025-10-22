namespace Editor
{
    public sealed class EquationRowFormatAction : EquationAction
    {
        public required int SelectionStartIndex { get; set; }
        public required int SelectedItems { get; set; }
        public required int FirstChildSelectionStartIndex { get; set; }
        public required int FirstChildSelectedItems { get; set; }
        public required int LastChildSelectionStartIndex { get; set; }
        public required int LastChildSelectedItems { get; set; }
        public required string Operation { get; set; }
        public required string Argument { get; set; }
        public required bool Applied { get; set; }

        public EquationRowFormatAction(ISupportsUndo executor)
            : base(executor)
        {
        }
    }
}
