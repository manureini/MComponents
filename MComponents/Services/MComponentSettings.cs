using Microsoft.Extensions.Localization;
using System.Globalization;

namespace MComponents.Services
{
    public class MComponentSettings
    {
        public static readonly CultureInfo[] AllSupportedCultures = new[] { new CultureInfo("en"), new CultureInfo("de"), new CultureInfo("fr") };

        public bool UseDeleteConfirmationWithAlert { get; set; }

        public bool RegisterResourceLocalizer { get; set; }

        public CultureInfo[] SupportedCultures { get; set; } = AllSupportedCultures;

        public bool SetRequestLocalizationOptions { get; set; }
    }
}
