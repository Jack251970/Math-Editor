using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Media;
using iNKORE.UI.WPF.Modern;

namespace Editor
{
    public static class PenManager
    {
        private static readonly Dictionary<(double, ApplicationTheme), Pen> _bevelPens = [];
        private static readonly Dictionary<(double, ApplicationTheme), Pen> _miterPens = [];
        private static readonly Dictionary<(double, ApplicationTheme), Pen> _roundPens = [];

        private static readonly Lock _bevelLock = new();
        private static readonly Lock _miterLock = new();
        private static readonly Lock _roundLock = new();

        public static Pen GetPen(double thickness, PenLineJoin lineJoin = PenLineJoin.Bevel)
        {
            if (lineJoin == PenLineJoin.Bevel)
            {
                return GetPen(_bevelLock, _bevelPens, thickness, lineJoin);
            }
            else if (lineJoin == PenLineJoin.Miter)
            {
                return GetPen(_miterLock, _miterPens, thickness, lineJoin);
            }
            else
            {
                return GetPen(_roundLock, _roundPens, thickness, lineJoin);
            }
        }

        private static Pen GetPen(Lock lockObj, Dictionary<(double, ApplicationTheme), Pen> penDictionary, double thickness, PenLineJoin lineJoin, Brush? brush = null)
        {
            lock (lockObj)
            {
                thickness = Math.Round(thickness, 1);
                var key = (thickness, ThemeManager.Current.ActualApplicationTheme);
                if (!penDictionary.TryGetValue(key, out var value))
                {
                    var pen = new Pen(brush ??
                        (ThemeManager.Current.ActualApplicationTheme == ApplicationTheme.Light ?
                            Brushes.Black : Brushes.White), thickness)
                    {
                        LineJoin = lineJoin
                    };
                    pen.Freeze();
                    value = pen;
                    penDictionary.Add(key, value);
                }
                return value;
            }
        }
    }
}
