using System;
using System.IO;
using System.Text.Json;
using Avalonia.Platform;

namespace Editor;

public static class JsonHelper
{
    public static T? Deserialize<T>(string path)
    {
        var uri = new Uri(path, UriKind.RelativeOrAbsolute);
        T? data;
        if (uri.IsAbsoluteUri && uri.Scheme.Equals("avares", StringComparison.OrdinalIgnoreCase))
        {
            using var jsonStream = AssetLoader.Open(uri);
            data = JsonSerializer.Deserialize<T>(jsonStream);
        }
        else
        {
            using var fileStream = File.OpenRead(path);
            data = JsonSerializer.Deserialize<T>(fileStream);
        }
        return data;
    }
}
