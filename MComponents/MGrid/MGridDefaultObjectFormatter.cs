using MComponents.Shared.Attributes;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.Extensions.Localization;
using System;
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

            if (pColumn.StringFormat != null)
                return string.Format(pColumn.StringFormat, value);

            if (pPropertyInfo.PropertyType == null)
                throw new ArgumentException($"{nameof(pPropertyInfo.PropertyType)} of column {pColumn.Identifier} is null, please specify it");

            Type pType = Nullable.GetUnderlyingType(pPropertyInfo.PropertyType) ?? pPropertyInfo.PropertyType;

            if (pType == typeof(bool))
            {
                return (bool)value ? L["True"] : L["False"];
            }

            if (pType.IsEnum)
            {
                return ((Enum)value).ToName();
            }

            if (pType == typeof(DateTime))
            {
                if (HasAttribute(pColumn, pPropertyInfo, typeof(TimeAttribute)))
                    return string.Format("{0:t}", ((DateTime)value));

                if (HasAttribute(pColumn, pPropertyInfo, typeof(DateTimeAttribute)))
                    return string.Format("{0:g}", ((DateTime)value));

                return string.Format("{0:d}", ((DateTime)value));
            }

            return value.ToString();
        }

        protected bool HasAttribute(IMGridPropertyColumn pColumn, IMPropertyInfo pPropertyInfo, Type pType)
        {
            return pPropertyInfo.GetCustomAttribute(pType) != null || (pColumn.Attributes != null && pColumn.Attributes.Any(a => a.GetType() == pType));
        }
    }
}
