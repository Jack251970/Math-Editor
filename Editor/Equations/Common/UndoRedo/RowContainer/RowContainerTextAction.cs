using System.Collections.Generic;

namespace Editor
{
    public sealed class RowContainerTextAction : EquationAction
    {
        public required int SelectionStartIndex { get; set; }
        public required int SelectedItems { get; set; }

        public required EquationBase ActiveEquation { get; set; }
        public EquationBase? ActiveEquationAfterChange { get; set; }
        public required int ActiveEquationSelectionIndex { get; set; }
        public required int ActiveEquationSelectedItems { get; set; }

        public required TextEquation ActiveTextInRow { get; set; }
        public required int CaretIndexOfActiveText { get; set; }
        public required int SelectionStartIndexOfTextEquation { get; set; }
        public required int SelectedItemsOfTextEquation { get; set; }
        public required string TextEquationContents { get; set; }
        public required int[] TextEquationFormats { get; set; }
        public EditorMode[] TextEquationModes { get; set; } = [];
        public CharacterDecorationInfo[] TextEquationDecoration { get; set; } = [];

        public required string FirstLineOfInsertedText { get; set; }
        public int[] FirstFormatsOfInsertedText { get; set; } = [];
        public EditorMode[] FirstModesOfInsertedText { get; set; } = [];
        public CharacterDecorationInfo[] FirstDecorationsOfInsertedText { get; set; } = [];

        public required List<EquationRow> Equations { get; set; }

        public RowContainerTextAction(ISupportsUndo executor)
            : base(executor)
        {
        }
    }
}

