using System;
using System.Collections.Generic;

namespace Editor
{
    public static class UndoManager
    {
        public static bool DisableAddingActions { get; set; }
        private static readonly Stack<EquationAction> undoStack = new();
        private static readonly Stack<EquationAction> redoStack = new();

        public static event EventHandler<UndoEventArgs> CanUndo = (a, b) => { };
        public static event EventHandler<UndoEventArgs> CanRedo = (a, b) => { };

        public static void AddUndoAction(EquationAction equationAction)
        {
            if (!DisableAddingActions)
            {
                undoStack.Push(equationAction);
                redoStack.Clear();
                CanUndo(null, new UndoEventArgs(true));
                CanRedo(null, new UndoEventArgs(false));
            }
        }

        public static void Undo()
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
                    CanUndo(null, new UndoEventArgs(false));
                }
                CanRedo(null, new UndoEventArgs(true));
            }
        }

        public static void Redo()
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
                    CanRedo(null, new UndoEventArgs(false));
                }
                CanUndo(null, new UndoEventArgs(true));
            }
        }

        public static void ClearAll()
        {
            undoStack.Clear();
            redoStack.Clear();
            CanUndo(null, new UndoEventArgs(false));
            CanRedo(null, new UndoEventArgs(false));
        }

        public static int UndoCount => undoStack.Count;

        public static void ChangeUndoCountOfLastAction(int newCount)
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

