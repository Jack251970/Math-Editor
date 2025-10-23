namespace Editor
{
    public sealed class RowContainerFormatAction : EquationAction
    {
        public required EquationBase ActiveChild { get; set; }
        public required int SelectionStartIndex { get; set; }
        public required int SelectedItems { get; set; }

        public required int FirstRowActiveChildIndex { get; set; }
        public required int FirstRowSelectionStartIndex { get; set; }
        public required int FirstRowSelectedItems { get; set; }

        public required int LastRowActiveChildIndex { get; set; }
        public required int LastRowSelectionStartIndex { get; set; }
        public required int LastRowSelectedItems { get; set; }

        public required int FirstTextCaretIndex { get; set; }
        public required int FirstTextSelectionStartIndex { get; set; }
        public required int FirstTextSelectedItems { get; set; }

        public required int LastTextCaretIndex { get; set; }
        public required int LastTextSelectionStartIndex { get; set; }
        public required int LastTextSelectedItems { get; set; }

        public required string Operation { get; set; }
        public required object Argument { get; set; }
        public required bool Applied { get; set; }

        public RowContainerFormatAction(ISupportsUndo executor)
            : base(executor)
        {
        }
    }
}
