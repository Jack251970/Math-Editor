using System;
using System.Globalization;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace Editor;

/// <summary>
/// Serialize object using json format.
/// </summary>
public class JsonStorage<T> where T : new()
{
    protected T? Data;

    // need a new directory name
    public const string DirectoryName = Constants.Settings;
    public const string FileSuffix = ".json";

    protected string FilePath { get; init; } = null!;

    private string TempFilePath => $"{FilePath}.tmp";

    private string BackupFilePath => $"{FilePath}.bak";

    protected string DirectoryPath { get; init; } = null!;

    private readonly JsonSerializerOptions _jsonSerializerOptions = new() { WriteIndented = true };

    // Let the derived class to set the file path
    protected JsonStorage()
    {
    }

    public JsonStorage(string filePath)
    {
        FilePath = filePath;
        DirectoryPath = Path.GetDirectoryName(filePath) ?? throw new ArgumentException($"Invalid {nameof(filePath)}");

        if (!Directory.Exists(DirectoryPath))
        {
            Directory.CreateDirectory(DirectoryPath);
        }
    }

    public bool Exists()
    {
        return File.Exists(FilePath);
    }

    public void Delete()
    {
        foreach (var path in new[] { FilePath, BackupFilePath, TempFilePath })
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
    }

    public async Task<T> LoadAsync()
    {
        if (Data != null)
            return Data;

        string? serialized = null;

        if (File.Exists(FilePath))
        {
            serialized = await File.ReadAllTextAsync(FilePath);
        }

        if (!string.IsNullOrEmpty(serialized))
        {
            try
            {
                Data = JsonSerializer.Deserialize<T>(serialized) ?? await LoadBackupOrDefaultAsync();
            }
            catch (JsonException)
            {
                Data = await LoadBackupOrDefaultAsync();
            }
        }
        else
        {
            Data = await LoadBackupOrDefaultAsync();
        }

        return Data!;
    }

    private async ValueTask<T> LoadBackupOrDefaultAsync()
    {
        var backup = await TryLoadBackupAsync();

        return backup ?? LoadDefault();
    }

    private async ValueTask<T?> TryLoadBackupAsync()
    {
        if (!File.Exists(BackupFilePath))
            return default;

        try
        {
            await using var source = File.OpenRead(BackupFilePath);
            var data = await JsonSerializer.DeserializeAsync<T>(source) ?? default;

            if (data != null)
                RestoreBackup();

            return data;
        }
        catch (JsonException)
        {
            return default;
        }
    }

    private void RestoreBackup()
    {
        if (File.Exists(FilePath))
            File.Replace(BackupFilePath, FilePath, null);
        else
            File.Move(BackupFilePath, FilePath);
    }

    public T Load()
    {
        string? serialized = null;

        if (File.Exists(FilePath))
        {
            serialized = File.ReadAllText(FilePath);
        }

        if (!string.IsNullOrEmpty(serialized))
        {
            try
            {
                Data = JsonSerializer.Deserialize<T>(serialized) ?? TryLoadBackup() ?? LoadDefault();
            }
            catch (JsonException)
            {
                Data = TryLoadBackup() ?? LoadDefault();
            }
        }
        else
        {
            Data = TryLoadBackup() ?? LoadDefault();
        }

        return Data!;
    }

    private T LoadDefault()
    {
        if (File.Exists(FilePath))
        {
            BackupOriginFile();
        }

        return new T();
    }

    private T? TryLoadBackup()
    {
        if (!File.Exists(BackupFilePath))
            return default;

        try
        {
            var data = JsonSerializer.Deserialize<T>(File.ReadAllText(BackupFilePath));

            if (data != null)
                RestoreBackup();

            return data;
        }
        catch (JsonException)
        {
            return default;
        }
    }

    private void BackupOriginFile()
    {
        var timestamp = DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss-fffffff", CultureInfo.CurrentUICulture);
        var directory = Path.GetDirectoryName(FilePath)!;
        var originName = Path.GetFileNameWithoutExtension(FilePath);
        var backupName = $"{originName}-{timestamp}{FileSuffix}";
        var backupPath = Path.Combine(directory, backupName);
        File.Copy(FilePath, backupPath, true);
    }

    public void Save()
    {
        // User may delete the directory, so we need to check it
        if (!Directory.Exists(DirectoryPath))
        {
            Directory.CreateDirectory(DirectoryPath);
        }

        var serialized = JsonSerializer.Serialize(Data, _jsonSerializerOptions);

        File.WriteAllText(TempFilePath, serialized);

        AtomicWriteSetting();
    }

    public async Task SaveAsync()
    {
        // User may delete the directory, so we need to check it
        if (!Directory.Exists(DirectoryPath))
        {
            Directory.CreateDirectory(DirectoryPath);
        }

        await using var tempOutput = File.OpenWrite(TempFilePath);
        await JsonSerializer.SerializeAsync(tempOutput, Data, _jsonSerializerOptions);
        AtomicWriteSetting();
    }

    private void AtomicWriteSetting()
    {
        if (!File.Exists(FilePath))
        {
            File.Move(TempFilePath, FilePath);
        }
        else
        {
            var finalFilePath = new FileInfo(FilePath).LinkTarget ?? FilePath;
            File.Replace(TempFilePath, finalFilePath, BackupFilePath);
        }
    }
}
