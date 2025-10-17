using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Editor;

public sealed class AutoGreyableImage : Image
{
    private BitmapSource? _colorSource;

    static AutoGreyableImage()
    {
        IsEnabledProperty.OverrideMetadata(
            typeof(AutoGreyableImage),
            new FrameworkPropertyMetadata(true, new PropertyChangedCallback(OnAutoGreyScaleImageIsEnabledPropertyChanged)));
    }

    public static readonly DependencyProperty UriProperty = DependencyProperty.Register(
        "Uri",
        typeof(string),
        typeof(AutoGreyableImage),
        new PropertyMetadata(null, OnUriChanged));

    public string Uri
    {
        get => (string)GetValue(UriProperty);
        set => SetValue(UriProperty, value);
    }

    private static void OnUriChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not AutoGreyableImage img) return;

        var uriStr = e.NewValue as string;
        if (string.IsNullOrWhiteSpace(uriStr)) return;

        try
        {
            var uri = new Uri(uriStr, UriKind.RelativeOrAbsolute);

            var bmp = new BitmapImage();
            bmp.BeginInit();
            bmp.UriSource = uri;
            bmp.CacheOption = BitmapCacheOption.OnLoad;
            bmp.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
            bmp.EndInit();
            if (bmp.CanFreeze) bmp.Freeze();

            img._colorSource = bmp;

            if (img.IsEnabled)
            {
                img.Source = bmp;
                img.OpacityMask = null;
            }
            else
            {
                var gray = new FormatConvertedBitmap(bmp, PixelFormats.Gray32Float, null, 0);
                if (gray.CanFreeze) gray.Freeze();

                var mask = new ImageBrush(bmp);
                if (mask.CanFreeze) mask.Freeze();

                img.Source = gray;
                img.OpacityMask = mask;
            }
        }
        catch
        {
            // Invalid URI; ignore
        }
    }

    private static void OnAutoGreyScaleImageIsEnabledPropertyChanged(DependencyObject source, DependencyPropertyChangedEventArgs args)
    {
        if (source is not AutoGreyableImage img) return;

        var enabled = (bool)args.NewValue;

        // Ensure we have a color source cached
        if (img._colorSource is null)
        {
            switch (img.Source)
            {
                case FormatConvertedBitmap fcb when fcb.Source is BitmapSource bs:
                    img._colorSource = bs;
                    break;

                case BitmapSource bs:
                    img._colorSource = bs;
                    break;

                default:
                    // Try to hydrate from Uri if available
                    var uriStr = img.Uri;
                    if (!string.IsNullOrWhiteSpace(uriStr))
                    {
                        try
                        {
                            var uri = new Uri(uriStr, UriKind.RelativeOrAbsolute);
                            var bmp = new BitmapImage();
                            bmp.BeginInit();
                            bmp.UriSource = uri;
                            bmp.CacheOption = BitmapCacheOption.OnLoad;
                            bmp.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
                            bmp.EndInit();
                            if (bmp.CanFreeze) bmp.Freeze();
                            img._colorSource = bmp;
                        }
                        catch
                        {
                            // Ignore load failures
                        }
                    }
                    break;
            }
        }

        if (enabled)
        {
            if (img._colorSource is not null)
            {
                img.Source = img._colorSource;
            }
            img.OpacityMask = null;
        }
        else
        {
            if (img._colorSource is null)
            {
                // Nothing we can do without a color source
                return;
            }

            var gray = new FormatConvertedBitmap(img._colorSource, PixelFormats.Gray32Float, null, 0);
            if (gray.CanFreeze) gray.Freeze();

            var mask = new ImageBrush(img._colorSource);
            if (mask.CanFreeze) mask.Freeze();

            img.Source = gray;
            img.OpacityMask = mask;
        }
    }
}
