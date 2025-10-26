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
        /*
         * Pens
         */
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
                var theme = key.Item2;
                var newKey = (thickness, theme);
                if (!penDictionary.TryGetValue(newKey, out var value))
                {
                    var pen = new Pen(brush ?? GetTextFillColorPrimaryBrush(theme), thickness)
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

        private static readonly Lock _rowBoxPenLock = new();

        private static Pen? _rowBoxPen;
        public static Pen RowBoxPen
        {
            get
            {
                lock (_rowBoxPenLock)
                {
                    if (_rowBoxPen is null)
                    {
                        _rowBoxPen = new(GetAccentFillColorDefaultBrush(), 1.1)
                        {
                            StartLineCap = PenLineCap.Flat,
                            EndLineCap = PenLineCap.Flat,
                            DashStyle = DashStyles.Dash
                        };
                        _rowBoxPen.Freeze();
                    }
                    return _rowBoxPen;
                }
            }
        }

        /*
         * Brushes
         */
        public static SolidColorBrush TextFillColorPrimaryBrush =>
            GetTextFillColorPrimaryBrush(ThemeManager.Current.ActualApplicationTheme);

        private static readonly Lock _deleteableBrushLock = new();

        private static SolidColorBrush? _deleteableBrush;
        public static SolidColorBrush DeleteableBrush
        {
            get
            {
                lock (_deleteableBrushLock)
                {
                    if (_deleteableBrush is null)
                    {
                        _deleteableBrush = new SolidColorBrush(Colors.Gray)
                        {
                            Opacity = 0.5
                        };
                        _deleteableBrush.Freeze();
                    }
                    return _deleteableBrush;
                }
            }
        }

        private static SolidColorBrush GetTextFillColorPrimaryBrush(ApplicationTheme? theme)
        {
            theme ??= ThemeManager.Current.ActualApplicationTheme;
            return theme == ApplicationTheme.Light ? Brushes.Black : Brushes.White;
        }

        private static SolidColorBrush GetAccentFillColorDefaultBrush()
        {
            return new(((SolidColorBrush)Application.Current.Resources[ThemeKeys.AccentFillColorDefaultBrushKey]).Color);
        }
    }
}
