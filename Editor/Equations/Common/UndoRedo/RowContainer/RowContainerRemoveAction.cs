using System.Collections.Generic;

namespace Editor
{
    public sealed class RowContainerRemoveAction : RowRemoveAction
    {
        public required EquationRow HeadEquationRow { get; set; }
        public required EquationRow TailEquationRow { get; set; }
        public required int FirstRowActiveIndex { get; set; }
        public required int LastRowActiveIndex { get; set; }
        public required int FirstRowSelectionIndex { get; set; }
        public required int LastRowSelectionIndex { get; set; }
        public required int FirstRowSelectedItems { get; set; }
        public required int LastRowSelectedItems { get; set; }
        public required int FirstRowActiveIndexAfterRemoval { get; set; }

        public required List<EquationBase> FirstRowDeletedContent { get; set; }
        public required List<EquationBase> LastRowDeletedContent { get; set; }

        public RowContainerRemoveAction(ISupportsUndo executor)
            : base(executor)
        {
        }
    }
}

