using MComponents.MGrid;
using System;
using System.Globalization;
using System.Threading.Tasks;

namespace MComponents.Services
{
    public class MComponentSettings
    {
        public static readonly CultureInfo[] AllSupportedCultures = new[] { new CultureInfo("en-US"), new CultureInfo("de-DE"), new CultureInfo("fr-FR") };

        public bool UseDeleteConfirmationWithAlert { get; set; }

        public bool RegisterResourceLocalizer { get; set; }

        public bool RegisterStringLocalizer { get; set; }

        public bool RegisterNavigation { get; set; }

        public CultureInfo[] SupportedCultures { get; set; } = AllSupportedCultures;

        public bool SetRequestLocalizationOptions { get; set; }

        public MGridSettings MGridSettings => MGridSettings.Instance;

        public Func<IServiceProvider, string, Task> EnsureAssemblyIsLoaded { get; set; }
    }
}
