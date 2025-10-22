using System.Collections.Generic;

namespace Editor
{
    public sealed class RowContainerPasteAction : EquationAction
    {
        public required int SelectionStartIndex { get; set; }
        public required int SelectedItems { get; set; }

        public required EquationBase ActiveEquation { get; set; }
        public required int ActiveEquationSelectionIndex { get; set; }
        public required int ActiveEquationSelectedItems { get; set; }

        public required TextEquation ActiveTextInChildRow { get; set; }
        public required int CaretIndexOfActiveText { get; set; }
        public required int SelectionStartIndexOfTextEquation { get; set; }
        public required int SelectedItemsOfTextEquation { get; set; }
        public required string TextEquationContents { get; set; }

        public required int[] TextEquationFormats { get; set; }
        public required EditorMode[] TextEquationModes { get; set; }
        public required CharacterDecorationInfo[] TextEquationDecorations { get; set; }

        public required string HeadTextOfPastedRows { get; set; }
        public required string TailTextOfPastedRows { get; set; }

        public required int[] HeadFormatsOfPastedRows { get; set; }
        public required int[] TailFormatsOfPastedRows { get; set; }

        public required EditorMode[] HeadModeOfPastedRows { get; set; }
        public required EditorMode[] TailModesOfPastedRows { get; set; }

        public required CharacterDecorationInfo[] HeadDecorationsOfPastedRows { get; set; }
        public required CharacterDecorationInfo[] TailDecorationsOfPastedRows { get; set; }

        public required List<EquationRow> Equations { get; set; }

        public RowContainerPasteAction(ISupportsUndo executor)
            : base(executor)
        {
        }
    }
}

