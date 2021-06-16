using Microsoft.Extensions.Localization;
using System.Collections.Generic;
using System.Reflection;

namespace MComponents
{
    public class StringLocalizer : IStringLocalizer
    {
        protected readonly IStringLocalizer mLocalizer;

        public StringLocalizer(IStringLocalizerFactory pFactory)
        {
            var type = typeof(StringLocalizer);
            var assemblyName = new AssemblyName(type.GetTypeInfo().Assembly.FullName);
            mLocalizer = pFactory.Create("MComponentsLocalization", assemblyName.Name);
        }

        public LocalizedString this[string name] => mLocalizer[name];

        public LocalizedString this[string name, params object[] arguments] => mLocalizer[name, arguments];

        public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures)
        {
            return mLocalizer.GetAllStrings(includeParentCultures);
        }
    }
}
