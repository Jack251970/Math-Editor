using CommunityToolkit.Mvvm.ComponentModel;

namespace Editor.Models;

public class Settings : ObservableObject
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
