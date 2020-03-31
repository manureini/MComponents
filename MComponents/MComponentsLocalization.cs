using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace MComponents
{
    public class MComponentsLocalization
    {
        public static CultureInfo[] SupportedCultures { get; } = new[] { new CultureInfo("en"), new CultureInfo("de"), new CultureInfo("fr") };
    }
}
