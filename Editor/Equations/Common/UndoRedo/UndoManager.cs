using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Editor
{
    public partial class UndoManager : ObservableObject
    {
        public bool DisableAddingActions { get; set; }

        private readonly Stack<EquationAction> undoStack = new();
        private readonly Stack<EquationAction> redoStack = new();

        [ObservableProperty]
        public partial bool CanUndo { get; set; }

        [ObservableProperty]
        public partial bool CanRedo { get; set; }

        public void AddUndoAction(EquationAction equationAction)
        {
            if (!DisableAddingActions)
            {
                undoStack.Push(equationAction);
                redoStack.Clear();
                CanUndo = true;
                CanRedo = false;
            }
        }

        public void Undo()
        {
            if (undoStack.Count > 0)
            {
                var temp = undoStack.Peek();
                for (var i = 0; i <= temp.FurtherUndoCount; i++)
                {
                    var action = undoStack.Pop();
                    action.Executor.ProcessUndo(action);
                    action.UndoFlag = !action.UndoFlag;
                    redoStack.Push(action);
                }
                if (undoStack.Count == 0)
                {
                    CanUndo = false;
                }
                CanRedo = true;
            }
        }

        public void Redo()
        {
            if (redoStack.Count > 0)
            {
                var temp = redoStack.Peek();
                for (var i = 0; i <= temp.FurtherUndoCount; i++)
                {
                    var action = redoStack.Pop();
                    action.Executor.ProcessUndo(action);
                    action.UndoFlag = !action.UndoFlag;
                    undoStack.Push(action);
                }
                if (redoStack.Count == 0)
                {
                    CanRedo = false;
                }
                CanUndo = true;
            }
        }

        public void ClearAll()
        {
            undoStack.Clear();
            redoStack.Clear();
            CanUndo = false;
            CanRedo = false;
        }

        public int UndoCount => undoStack.Count;

        public void ChangeUndoCountOfLastAction(int newCount)
        {
            undoStack.Peek().FurtherUndoCount = newCount;
            for (var i = 0; i < newCount; i++)
            {
                redoStack.Push(undoStack.Pop());
            }
            undoStack.Peek().FurtherUndoCount = newCount;
            for (var i = 0; i < newCount; i++)
            {
                undoStack.Push(redoStack.Pop());
            }
        }
    }
}

