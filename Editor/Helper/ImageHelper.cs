using System;
using System.IO;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

namespace Editor;

public static class ImageHelper
{
    private static readonly string ClassName = nameof(ImageHelper);

    // TODO: Check these two image path cases
    // avares://Editor/Images/Commands/SumsProducts/productBottomTop.png
    // avares://Editor/Images/Commands/Decorated/Character/VstrikeThrough.png
    public static Bitmap? GetBitmap(string path)
    {
        var uri = new Uri(path, UriKind.RelativeOrAbsolute);
        Bitmap? data;
        try
        {
            if (uri.IsAbsoluteUri && uri.Scheme.Equals("avares", StringComparison.OrdinalIgnoreCase))
            {
                using var jsonStream = AssetLoader.Open(uri);
                data = new Bitmap(jsonStream);
            }
            else
            {
                using var fileStream = File.OpenRead(uri.AbsolutePath);
                data = new Bitmap(fileStream);
            }
            return data;
        }
        catch (Exception e)
        {
            EditorLogger.Fatal(ClassName, $"Failed to load image from path: {path}", e);
            return null;
        }
    }
}
