using System.Xml.Linq;
using Avalonia.Media.Imaging;

namespace Editor
{
    public sealed class CopyDataObject
    {
        public Bitmap? Image { get; set; }
        public string? Text { get; set; }
        public required XElement XElement { get; set; }
    }
}
