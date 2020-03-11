using MComponents.Shared.Attributes;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.Extensions.Localization;
using System;
using System.Globalization;
using System.Linq;

namespace MComponents.MGrid
{
    public class MGridDefaultObjectFormatter<T> : IMGridObjectFormatter<T>
    {
        public IStringLocalizer<MComponentsLocalization> L { get; set; }

        public virtual void AppendToTableRow(RenderTreeBuilder pBuilder, ref string pCssClass, T pRow, bool pSelected)
        {
        }

        public virtual void AppendToTableRowData(RenderTreeBuilder pBuilder, IMGridColumn pColumn, T pRow)
        {
        }

        public virtual string FormatPropertyColumnValue(IMGridPropertyColumn pColumn, IMPropertyInfo pPropertyInfo, T pRow)
        {
            object value = pPropertyInfo.GetValue(pRow);

            if (value == null)
                return null;

            if (HasAttribute(pColumn, pPropertyInfo, typeof(TimeAttribute)))
            {
                if (value is DateTime dt)
                {
                    return dt.ToString("HH:mm");
                }

                if (value is DateTime?)
                {
                    return ((DateTime?)value).Value.ToString("HH:mm");
                }
            }

            if (pColumn.StringFormat != null)
            {
                return String.Format(pColumn.StringFormat, value);
            }

            Type pType = Nullable.GetUnderlyingType(pPropertyInfo.PropertyType) ?? pPropertyInfo.PropertyType;

            if (pType == typeof(bool))
            {
                return (bool)value == true ? L["True"] : L["False"];
            }

            if (pType.IsEnum)
            {
                return ((Enum)value).ToName();
            }

            return value.ToString();
        }

        protected bool HasAttribute(IMGridPropertyColumn pColumn, IMPropertyInfo pPropertyInfo, Type pType)
        {
            return pPropertyInfo.GetCustomAttribute(pType) != null || (pColumn.Attributes != null && pColumn.Attributes.Any(a => a.GetType() == pType));
        }
    }
}
