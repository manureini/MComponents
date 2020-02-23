using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace MComponents
{
    public static class Extensions
    {
        public static void AddEventStopPropagationClicksAttribute(this RenderTreeBuilder builder, int sequence)
        {
            builder.AddEventStopPropagationAttribute(sequence, "onclick", true);
            builder.AddEventStopPropagationAttribute(sequence, "ondblclick", true);
        }

        public static void AddStyleWithAttribute2(this RenderTreeBuilder builder, int sequence, double? pWidth, double pHeight)
        {
            string width = $"{Convert.ToString(pWidth, CultureInfo.InvariantCulture) }px";
            string height = $"{Convert.ToString(pHeight, CultureInfo.InvariantCulture) }px";
            builder.AddAttribute(sequence, "style", $"width: {width}; height: {height};");
        }
    }

}
