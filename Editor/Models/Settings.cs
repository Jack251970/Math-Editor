using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.Json.Serialization;
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

    public string Language { get; set; } = Constants.SystemLanguageCode;

    public ObservableCollection<string> RecentSymbolList { get; set; } = [];

    public Dictionary<string, int> UsedSymbolList { get; set; } = [];

    private FontType _defaultFont = FontType.STIXGeneral;
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public FontType DefaultFont
    {
        get => _defaultFont;
        set
        {
            if (_defaultFont != value)
            {
                _defaultFont = value;
                OnPropertyChanged();
            }
        }
    }

    private EditorMode _defaultMode = EditorMode.Math;
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public EditorMode DefaultMode
    {
        get => _defaultMode;
        set
        {
            if (_defaultMode != value)
            {
                _defaultMode = value;
                OnPropertyChanged();
            }
        }
    }

    private CopyType _copyType = CopyType.Image;
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public CopyType CopyType
    {
        get => _copyType;
        set
        {
            if (_copyType != value)
            {
                _copyType = value;
                OnPropertyChanged();
            }
        }
    }
}
