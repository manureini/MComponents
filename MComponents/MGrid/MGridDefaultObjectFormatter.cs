using MComponents.Services;
using MComponents.Shared.Attributes;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MComponents.MGrid
{
    public class MGridDefaultObjectFormatter<T> : IMGridObjectFormatter<T>
    {
        public const string ROW_DELETE_METADATA = "delete-row";

        public IStringLocalizer L { get; set; }

        protected Dictionary<T, object> mRowMetadata = new Dictionary<T, object>();
        protected ITimezoneService mTimezoneService;

        public MGridDefaultObjectFormatter()
        {
        }

        public MGridDefaultObjectFormatter(ITimezoneService pTimezoneService)
        {
            mTimezoneService = pTimezoneService;
        }

        public virtual void AppendToTableRow(RenderTreeBuilder pBuilder, ref string pCssClass, T pRow, bool pSelected)
        {
            if (mRowMetadata.ContainsKey(pRow) && (string)mRowMetadata[pRow] == ROW_DELETE_METADATA)
            {
                pCssClass += " m-grid-row--delete";
            }
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
                var timeVal = (DateTime)value;

                if (mTimezoneService != null)
                {
                    timeVal = (DateTime)ReflectionHelper.AdjustTimezoneIfUtcInternal(mTimezoneService, timeVal, pPropertyInfo);
                }

                if (pPropertyInfo.GetCustomAttribute<TimeAttribute>() != null)
                    return string.Format("{0:t}", timeVal);

                if (pPropertyInfo.GetCustomAttribute<DateTimeAttribute>() != null)
                    return string.Format("{0:g}", timeVal);

                return string.Format("{0:d}", timeVal);
            }

            return value.ToString();
        }

        public void AddRowMetadata(T pRow, object pValue)
        {
            mRowMetadata.Remove(pRow); // right now we assume, that a user does not use this feature. Are there more use cases then delete row?
            mRowMetadata.Add(pRow, pValue);
        }

        public void RemoveRowMetadata(T pRow)
        {
            mRowMetadata.Remove(pRow);
        }

        public void ClearRowMetadata()
        {
            mRowMetadata.Clear();
        }
    }
}
