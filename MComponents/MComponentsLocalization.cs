using System.Globalization;

namespace MComponents
{
    public class MComponentsLocalization
    {
        public static CultureInfo[] SupportedCultures { get; } = new[] { new CultureInfo("en"), new CultureInfo("de"), new CultureInfo("fr") };
    }
}
