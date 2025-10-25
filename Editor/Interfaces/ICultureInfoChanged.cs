using System.Globalization;

namespace Editor;

public interface ICultureInfoChanged
{
    public void OnCultureInfoChanged(CultureInfo newCultureInfo);
}
