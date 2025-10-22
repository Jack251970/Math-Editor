using System.Collections.Generic;

namespace Editor
{
    public class RowRemoveAction : EquationAction
    {
        public required EquationBase ActiveEquation { get; set; }
        public required TextEquation HeadTextEquation { get; set; }
        public required TextEquation TailTextEquation { get; set; }

        //public required int ParentSelectionStartIndex { get; set; }
        public required int SelectionStartIndex { get; set; }
        public required int SelectedItems { get; set; }

        public required int FirstTextCaretIndex { get; set; }
        public required int LastTextCaretIndex { get; set; }
        public required string FirstText { get; set; }
        public required string LastText { get; set; }
        public required int[] FirstFormats { get; set; }
        public required EditorMode[] FirstModes { get; set; }
        public required int[] LastFormats { get; set; }
        public required EditorMode[] LastModes { get; set; }
        public required CharacterDecorationInfo[] FirstDecorations { get; set; }
        public required CharacterDecorationInfo[] LastDecorations { get; set; }
        public required int FirstTextSelectionIndex { get; set; }
        public required int LastTextSelectionIndex { get; set; }
        public required int FirstTextSelectedItems { get; set; }
        public required int LastTextSelectedItems { get; set; }

        public required List<EquationBase> Equations { get; set; }

        public RowRemoveAction(ISupportsUndo executor)
            : base(executor)
        {
        }
    }
}

