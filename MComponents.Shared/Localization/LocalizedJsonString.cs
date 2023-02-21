using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace MComponents.Shared.Localization
{
    public sealed class LocalizedJsonString
    {
        public LocalizedJsonString(string culture, string value)
        {
            Culture = culture;
            Value = value;
        }

        [JsonPropertyName("c")]
        public string Culture { get; private set; }

        [JsonPropertyName("v")]
        public string Value { get; private set; }
    }
}
