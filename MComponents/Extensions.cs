using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;
using System;
using System.Collections.Generic;
using System.Drawing;
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

        public static void AddStyleWithAttribute(this RenderTreeBuilder builder, int sequence, double pLeftOffset, BoundingBox pBoundingBox)
        {
            if (pBoundingBox == null)
                return;

            string width = $"{Convert.ToString(pBoundingBox.Width, CultureInfo.InvariantCulture) }px";
            string height = $"{Convert.ToString(pBoundingBox.Height, CultureInfo.InvariantCulture) }px";

            string top = $"{Convert.ToString(pBoundingBox.BorderTop, CultureInfo.InvariantCulture) }px";
            string left = $"{Convert.ToString(pLeftOffset, CultureInfo.InvariantCulture) }px";

            builder.AddAttribute(sequence, "style", $"width: {width}; height: {height}; top: {top}; left: {left}");
        }

        public static double FromPixelToDouble(this string pPixelString)
        {
            string value = pPixelString.Contains("px") ? pPixelString.Substring(0, pPixelString.IndexOf("px")) : pPixelString;
            return double.Parse(value, CultureInfo.InvariantCulture);
        }


    }

}
