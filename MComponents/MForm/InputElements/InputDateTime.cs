using Microsoft.AspNetCore.Components.CompilerServices;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Rendering;
using System;
using System.Linq.Expressions;

namespace MComponents.InputElements
{
    public class InputDateTime<T> : InputBase<T>
    {
        public T CurrentDateValue { get; set; }
        public T CurrentTimeValue { get; set; }

        //datetime-local is not supported in firefox - maybe in 10 years or something.....


        protected override void OnParametersSet()
        {
            base.OnParametersSet();

            CurrentDateValue = CurrentValue;
            CurrentTimeValue = CurrentValue;
        }

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            builder.OpenElement(0, "div");
            builder.AddAttribute(24, "class", "m-combined-input-container " + CssClass);

            builder.OpenComponent<InputDate<T>>(20);
            builder.AddAttribute(21, "Value", CurrentDateValue);
            builder.AddAttribute(22, "ValueChanged", RuntimeHelpers.CreateInferredEventCallback<T>(this, async __value =>
            {
                CurrentDateValue = __value;
                UpdateDateTimeValue();
            }, CurrentDateValue));
            builder.AddAttribute(23, "ValueExpression", Expression.Lambda<Func<T>>(Expression.Property(Expression.Constant(this), nameof(CurrentDateValue))));
            builder.AddAttribute(24, "class", "m-form-control m-combined-input");
            builder.AddAttribute(25, "style", "width: 66%;");
            builder.CloseComponent();

            builder.OpenComponent<InputTime<T>>(30);
            builder.AddAttribute(31, "Value", CurrentTimeValue);
            builder.AddAttribute(22, "ValueChanged", RuntimeHelpers.CreateInferredEventCallback<T>(this, async __value =>
            {
                CurrentTimeValue = __value;
                UpdateDateTimeValue();
            }, CurrentTimeValue));
            builder.AddAttribute(23, "ValueExpression", Expression.Lambda<Func<T>>(Expression.Property(Expression.Constant(this), nameof(CurrentTimeValue))));
            builder.AddAttribute(34, "class", "m-form-control m-combined-input");
            builder.AddAttribute(35, "style", "width: 34%;");
            builder.CloseComponent();

            builder.CloseElement(); //div
        }

        protected void UpdateDateTimeValue()
        {
            var cdate = CurrentDateValue as DateTime?;
            var ctime = CurrentTimeValue as DateTime?;

            if (cdate == null || ctime == null)
            {
                CurrentValue = default(T);
                return;
            }

            var date = cdate.Value;
            var time = ctime.Value;

            CurrentValue = (T)(object)new DateTime(date.Year, date.Month, date.Day, time.Hour, time.Minute, time.Second, time.Millisecond);
        }

        protected override bool TryParseValueFromString(string value, out T result, out string validationErrorMessage)
        {/*            
            validationErrorMessage = null;

            if (value == null || value == string.Empty)
            {
                result = default;
                return false;
            }

            if (!DateTime.TryParse(value, out DateTime datetime))
            {
                result = default;
                return false;
            }

            result = (T)(object)datetime;
            return true;*/
            throw new NotImplementedException();
        }

        protected override string FormatValueAsString(T value)
        {
            /*
            if (value == null)
                return null;

            dynamic v = value;
            return v.ToString("yyyy-MM-ddTHH:mm");
            */
            throw new NotImplementedException();
        }
    }
}