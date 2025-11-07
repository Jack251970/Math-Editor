using System;
using System.Collections.Generic;
using System.Threading;
using Avalonia;
using Avalonia.Media;
using Avalonia.Styling;

namespace Editor
{
    public static class PenManager
    {
        /*
         * Pens
         */
        private static readonly Dictionary<(double, ThemeVariant), Pen> _bevelPens = [];
        private static readonly Dictionary<(double, ThemeVariant), Pen> _miterPens = [];
        private static readonly Dictionary<(double, ThemeVariant), Pen> _roundPens = [];

        private static readonly Lock _bevelLock = new();
        private static readonly Lock _miterLock = new();
        private static readonly Lock _roundLock = new();

        public static Pen GetBlackPen(double thickness, PenLineJoin lineJoin = PenLineJoin.Bevel)
        {
            var key = (thickness, ThemeVariant.Light);
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
            var key = (thickness, Application.Current!.RequestedThemeVariant!);
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

        private static Pen GetPen(Lock lockObj, Dictionary<(double, ThemeVariant), Pen> penDictionary, (double, ThemeVariant) key, PenLineJoin lineJoin, IBrush? brush = null)
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
                    // TODO: Do we need ToImmutable()?
                    if (_rowBoxPen is null)
                    {
                        _rowBoxPen = new(GetAccentFillColorDefaultBrush(), 1.1)
                        {
                            LineCap = PenLineCap.Flat,
                            DashStyle = new DashStyle([2, 2], 0)
                        };
                    }
                    return _rowBoxPen;
                }
            }
        }

        /*
         * Brushes
         */
        public static SolidColorBrush Black { get; } = new(Colors.Black);
        public static SolidColorBrush White { get; } = new(Colors.White);

        public static SolidColorBrush TextFillColorPrimaryBrush =>
            GetTextFillColorPrimaryBrush(Application.Current!.RequestedThemeVariant);

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
                    }
                    return _deleteableBrush;
                }
            }
        }

        private static readonly Lock _selectionBrushLock = new();

        private static SolidColorBrush? _selectionBrush;
        public static SolidColorBrush SelectionBrush
        {
            get
            {
                lock (_selectionBrushLock)
                {
                    // From WinUI3 TextBox #0063B1
                    _selectionBrush ??= new SolidColorBrush(Color.FromRgb(0, 99, 177));
                    return _selectionBrush;
                }
            }
        }

        private static SolidColorBrush GetTextFillColorPrimaryBrush(ThemeVariant? theme)
        {
            theme ??= Application.Current!.RequestedThemeVariant!;
            return theme == ThemeVariant.Light ? Black : White;
        }

        private static SolidColorBrush GetAccentFillColorDefaultBrush()
        {
            // From WinUI3 Gallery #40BDFF
            return new SolidColorBrush(Color.FromRgb(64, 189, 255));
        }
    }
}
