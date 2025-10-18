namespace Editor.Models;

public class Settings
{
    private EditorJsonStorage<Settings> _storage = null!;

    public void SetStorage(EditorJsonStorage<Settings> storage)
    {
        _storage = storage;
    }

    public void Save()
    {
        _storage.Save();
    }
}
