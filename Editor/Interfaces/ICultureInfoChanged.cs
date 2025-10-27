using System.Globalization;

namespace Editor;

public interface ICultureInfoChanged
{
    void OnCultureInfoChanged(CultureInfo newCultureInfo);
}
