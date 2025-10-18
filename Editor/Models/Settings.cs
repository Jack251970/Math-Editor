using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Editor;

public class Settings : ObservableObject
{
    private EditorJsonStorage<Settings> _storage = null!;
    private readonly Lock _lock = new();

    public void SetStorage(EditorJsonStorage<Settings> storage)
    {
        _storage = storage;
    }

    public void Save()
    {
        lock (_lock)
        {
            _storage.Save();
        }
    }

    public ObservableCollection<string> RecentSymbolList { get; set; } = [];

    public Dictionary<string, int> UsedSymbolList { get; set; } = [];
}
