using System;
using System.IO;
using System.Threading.Tasks;

namespace Editor;

public class EditorJsonStorage<T> : JsonStorage<T> where T : new()
{
    public EditorJsonStorage()
    {
        DirectoryPath = Path.Combine(DataLocation.DataDirectory(), DirectoryName);
        if (!Directory.Exists(DirectoryPath))
        {
            Directory.CreateDirectory(DirectoryPath);
        }

        var filename = typeof(T).Name;
        FilePath = Path.Combine(DirectoryPath, $"{filename}{FileSuffix}");
    }

    public new void Save()
    {
        try
        {
            base.Save();
        }
        catch (Exception e)
        {
            // TODO: Handle exception
            //App.API.LogFatal(ClassName, $"Failed to save settings to path: {FilePath}", e);
        }
    }

    public new async Task SaveAsync()
    {
        try
        {
            await base.SaveAsync();
        }
        catch (Exception e)
        {
            // TODO: Handle exception
            //App.API.LogFatal(ClassName, $"Failed to save settings to path: {FilePath}", e);
        }
    }
}
