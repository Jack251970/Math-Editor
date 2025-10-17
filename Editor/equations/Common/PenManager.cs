using System;
using System.Collections.Generic;
using System.Windows.Media;
using iNKORE.UI.WPF.Modern;

namespace Editor
{
    public static class PenManager
    {
        static Dictionary<(double, ApplicationTheme), Pen> bevelPens = [];
        static Dictionary<(double, ApplicationTheme), Pen> miterPens = [];
        static Dictionary<(double, ApplicationTheme), Pen> roundPens = [];

        static readonly object bevelLock = new();
        static readonly object miterLock = new object();
        static readonly object roundLock = new object();

        public static Pen GetPen(double thickness, PenLineJoin lineJoin = PenLineJoin.Bevel)
        {
            if (lineJoin == PenLineJoin.Bevel)
            {
                return GetPen(bevelLock, bevelPens, thickness, lineJoin);
            }
            else if (lineJoin == PenLineJoin.Miter)
            {
                return GetPen(miterLock, miterPens, thickness, lineJoin);
            }
            else
            {
                return GetPen(roundLock, roundPens, thickness, lineJoin);
            }
        }

        static Pen GetPen(object lockObj, Dictionary<(double, ApplicationTheme), Pen> penDictionary, double thickness, PenLineJoin lineJoin, Brush brush = null)
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
