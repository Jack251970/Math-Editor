using System.Collections.Generic;

namespace Editor
{
    public sealed class EquationRowPasteAction : EquationAction
    {
        public required TextEquation ActiveTextEquation { get; set; }
        public required int SelectedItems { get; set; }
        public required int SelectionStartIndex { get; set; }

        public required int ActiveChildCaretIndex { get; set; }
        public required int ActiveChildSelectedItems { get; set; }
        public required int ActiveChildSelectionStartIndex { get; set; }
        public required string ActiveChildText { get; set; }
        public required int[] ActiveChildFormats { get; set; }
        public required EditorMode[] ActiveChildModes { get; set; }
        public required CharacterDecorationInfo[] ActiveChildDecorations { get; set; }

        public required string FirstNewText { get; set; }
        public required int[] FirstNewFormats { get; set; }
        public required EditorMode[] FirstNewModes { get; set; }
        public required CharacterDecorationInfo[] FirstNewDecorations { get; set; }

        public required string LastNewText { get; set; }
        public required int[] LastNewFormats { get; set; }
        public required EditorMode[] LastNewModes { get; set; }
        public required CharacterDecorationInfo[] LastNewDecorations { get; set; }

        public required List<EquationBase> Equations { get; set; }

        public EquationRowPasteAction(ISupportsUndo executor)
            : base(executor)
        {
        }
    }
}

