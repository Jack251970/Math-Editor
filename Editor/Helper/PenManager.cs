using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using iNKORE.UI.WPF.Modern;

namespace Editor
{
    public static class PenManager
    {
        // TextFillColorPrimaryBrush
        public static SolidColorBrush TextFillColorPrimaryBrush =>
            ThemeManager.Current.ActualApplicationTheme == ApplicationTheme.Light ? Brushes.Black : Brushes.White;

        private static readonly Dictionary<(double, ApplicationTheme), Pen> _bevelPens = [];
        private static readonly Dictionary<(double, ApplicationTheme), Pen> _miterPens = [];
        private static readonly Dictionary<(double, ApplicationTheme), Pen> _roundPens = [];

        private static readonly Lock _bevelLock = new();
        private static readonly Lock _miterLock = new();
        private static readonly Lock _roundLock = new();

        public static Pen GetBlackPen(double thickness, PenLineJoin lineJoin = PenLineJoin.Bevel)
        {
            var key = (thickness, ApplicationTheme.Light);
            if (lineJoin == PenLineJoin.Bevel)
            {
                return GetPen(_bevelLock, _bevelPens, key, lineJoin);
            }
            else if (lineJoin == PenLineJoin.Miter)
            {
                return GetPen(_miterLock, _miterPens, key, lineJoin);
            }
            else
            {
                return GetPen(_roundLock, _roundPens, key, lineJoin);
            }
        }

        public static Pen GetPen(double thickness, PenLineJoin lineJoin = PenLineJoin.Bevel)
        {
            var key = (thickness, ThemeManager.Current.ActualApplicationTheme);
            if (lineJoin == PenLineJoin.Bevel)
            {
                return GetPen(_bevelLock, _bevelPens, key, lineJoin);
            }
            else if (lineJoin == PenLineJoin.Miter)
            {
                return GetPen(_miterLock, _miterPens, key, lineJoin);
            }
            else
            {
                return GetPen(_roundLock, _roundPens, key, lineJoin);
            }
        }

        private static Pen GetPen(Lock lockObj, Dictionary<(double, ApplicationTheme), Pen> penDictionary, (double, ApplicationTheme) key, PenLineJoin lineJoin, Brush? brush = null)
        {
            lock (lockObj)
            {
                var thickness = Math.Round(key.Item1, 1);
                var newKey = (thickness, key.Item2);
                if (!penDictionary.TryGetValue(newKey, out var value))
                {
                    var pen = new Pen(brush ?? TextFillColorPrimaryBrush, thickness)
                    {
                        LineJoin = lineJoin
                    };
                    pen.Freeze();
                    value = pen;
                    penDictionary.Add(newKey, value);
                }
                return value;
            }
        }

        // AccentFillColorDefaultBrush
        private static Pen? _rowBoxPen;
        private static readonly object _rowBoxPenLock = new();

        public static Pen RowBoxPen
        {
            get
            {
                if (_rowBoxPen is null)
                {
                    lock (_rowBoxPenLock)
                    {
                        _rowBoxPen = new(
                            new SolidColorBrush(((SolidColorBrush)Application.Current.Resources[
                                ThemeKeys.AccentFillColorDefaultBrushKey]).Color),
                            1.1)
                        {
                            StartLineCap = PenLineCap.Flat,
                            EndLineCap = PenLineCap.Flat,
                            DashStyle = DashStyles.Dash
                        };
                        _rowBoxPen.Freeze();
                    }
                }
                return _rowBoxPen;
            }
        }
    }
}
