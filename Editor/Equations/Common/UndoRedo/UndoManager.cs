using System;
using System.Collections.Generic;

namespace Editor
{
    public class UndoManager
    {
        public bool DisableAddingActions { get; set; }
        private readonly Stack<EquationAction> undoStack = new();
        private readonly Stack<EquationAction> redoStack = new();

        public event EventHandler<UndoEventArgs>? CanUndo;
        public event EventHandler<UndoEventArgs>? CanRedo;

        public void AddUndoAction(EquationAction equationAction)
        {
            if (!DisableAddingActions)
            {
                undoStack.Push(equationAction);
                redoStack.Clear();
                CanUndo?.Invoke(this, new UndoEventArgs(true));
                CanRedo?.Invoke(this, new UndoEventArgs(false));
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
                    CanUndo?.Invoke(this, new UndoEventArgs(false));
                }
                CanRedo?.Invoke(this, new UndoEventArgs(true));
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
                    CanRedo?.Invoke(this, new UndoEventArgs(false));
                }
                CanUndo?.Invoke(this, new UndoEventArgs(true));
            }
        }

        public void ClearAll()
        {
            undoStack.Clear();
            redoStack.Clear();
            CanUndo?.Invoke(this, new UndoEventArgs(false));
            CanRedo?.Invoke(this, new UndoEventArgs(false));
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

