﻿using Blazored.LocalStorage;
using MComponents.MGrid;
using MComponents.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;

namespace MComponents
{
    public static class Extensions
    {
        public const string MFORM_IN_TABLE_ROW_TD_STYLE_ATTRIBUTE = "mcomponent_mform_in_table_row_style";

        internal static void AddEventStopPropagationClicksAttribute(this RenderTreeBuilder builder, int sequence)
        {
            builder.AddEventStopPropagationAttribute(sequence, "onclick", true);
            builder.AddEventStopPropagationAttribute(sequence, "ondblclick", true);
        }

        internal static void AddStyleWithAttribute(this RenderTreeBuilder builder, int sequence, string pAttributeName, double pLeftOffset, BoundingBox pBoundingBox)
        {
            if (pBoundingBox == null)
                return;

            string width = $"{Convert.ToString(pBoundingBox.Width, CultureInfo.InvariantCulture) }px";
            string height = $"{Convert.ToString(pBoundingBox.Height, CultureInfo.InvariantCulture) }px";

            string top = $"{Convert.ToString(pBoundingBox.BorderTop, CultureInfo.InvariantCulture) }px";
            string left = $"{Convert.ToString(pLeftOffset, CultureInfo.InvariantCulture) }px";

            builder.AddAttribute(sequence, pAttributeName, $"width: {width}; height: {height}; top: {top}; left: {left}");
        }

        internal static double FromPixelToDouble(this string pPixelString)
        {
            string value = pPixelString.Contains("px") ? pPixelString.Substring(0, pPixelString.IndexOf("px")) : pPixelString;
            return double.Parse(value, CultureInfo.InvariantCulture);
        }

        internal static object GetComparer(this IMGridColumn pColumn)
        {
            object comparer = null;

            if (pColumn is IMGridCustomComparer)
                comparer = ((dynamic)pColumn).Comparer;

            return comparer;
        }

        internal static bool DictionaryEqual(IDictionary<string, object> pDict0, IDictionary<string, object> pDict1)
        {
            if (!pDict0.Count.Equals(pDict1.Count))
                return false;

            if (!pDict0.Keys.SequenceEqual(pDict1.Keys))
                return false;

            foreach (string key in pDict0.Keys)
            {
                if ((pDict0[key] == null && pDict1[key] != null) || (pDict0[key] != null && pDict1[key] == null))
                    return false;

                if (pDict0[key] == null && pDict1[key] == null)
                    continue;

                if (!pDict0[key].Equals(pDict1[key]))
                    return false;
            }

            return true;
        }

        public static bool DictionaryEqualIfContains(IDictionary<string, object> pDict0, IDictionary<string, object> pDict1)
        {
            foreach (var key in pDict0.Keys)
            {
                if (!pDict1.ContainsKey(key))
                    continue;

                var val1 = pDict0[key];
                var val2 = pDict1[key];

                if ((val1 == null && val2 != null) || (val1 != null && val2 == null))
                    return false;

                if (val1 == null && val2 == null)
                    continue;

                if (!val1.Equals(val2))
                    return false;
            }

            return true;
        }

        internal static object ToObject(this JsonElement element, Type pType, JsonSerializerOptions options = null)
        {
            var bufferWriter = new ArrayBufferWriter<byte>();
            using (var writer = new Utf8JsonWriter(bufferWriter))
                element.WriteTo(writer);
            return JsonSerializer.Deserialize(bufferWriter.WrittenSpan, pType, options);
        }

        public static void AddMComponents(this IServiceCollection pServices, Action<MComponentSettings> pOptions = null)
        {
            pServices.AddLocalization(options => options.ResourcesPath = "Resources");
            pServices.Configure<RequestLocalizationOptions>(options =>
            {
                options.SupportedUICultures = MComponentsLocalization.SupportedCultures;
            });

            pServices.AddBlazoredLocalStorage();

            pServices.AddScoped<MLocalStorageService>();
            pServices.AddScoped<MGridStateService>();

            var settings = new MComponentSettings();
            if (pOptions != null)
                pOptions(settings);
            pServices.AddSingleton(settings);
        }
    }

}
