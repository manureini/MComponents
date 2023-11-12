using MComponents.MForm;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.CompilerServices;
using Microsoft.AspNetCore.Components.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MComponents.MQueryBuilder
{
    public class MInputValue : ComponentBase
    {
        [Parameter]
        public Type ValueType { get; set; }

        [Parameter]
        public object Value { get; set; }

        [Parameter]
        public EventCallback<object> ValueChanged { get; set; }

        protected IDictionary<string, object> mModel = new Dictionary<string, object>()
        {
            ["Value"] = null
        };

        protected IMPropertyInfo mPropertyInfo;

        protected override void OnParametersSet()
        {
            base.OnParametersSet();
            mPropertyInfo = new MPropertyExpandoInfo("Value", ValueType, null);
        }

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            base.BuildRenderTree(builder);

            var getValueChangedMethod = typeof(MInputValue).GetMethod(nameof(MInputValue.GetValueChangedEvent), System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).MakeGenericMethod(ValueType);

            var field = new MFieldEmpty()
            {
                AdditionalAttributes = new Dictionary<string, object>()
                {
                    ["ValueChanged"] = getValueChangedMethod.Invoke(this, null)
                }
            };

            var method = typeof(RenderHelper).GetMethod(nameof(RenderHelper.AppendInput)).MakeGenericMethod(ValueType);
            method.Invoke(null, new object[] { builder, mPropertyInfo, mModel, Guid.Empty, null, false, field, false });
        }

        protected EventCallback<T> GetValueChangedEvent<T>()
        {
            return RuntimeHelpers.CreateInferredEventCallback<T>(this, async __value =>
            {
                await ValueChanged.InvokeAsync(__value);
            }, (T)Value);
        }

    }
}
