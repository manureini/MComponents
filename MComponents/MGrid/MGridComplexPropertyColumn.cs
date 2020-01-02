using MComponents.MForm;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;

namespace MComponents.MGrid
{
    public class MGridComplexPropertyColumn<T, TProperty> : ComponentBase, IMGridColumn, IMGridPropertyColumn, IMComplexColumn<TProperty>, IMGridColumnGenerator<T>
    {
        [Parameter]
        public RenderFragment<MComplexPropertyFieldContext<TProperty>> FormTemplate { get; set; }

        [Parameter]
        public RenderFragment<T> CellTemplate { get; set; }

        [Parameter]
        public string Identifier { get; set; }

        [Parameter]
        public bool EnableFilter { get; set; } = true;

        public Type PropertyType
        {
            get
            {
                return typeof(TProperty);
            }
            set
            {
            }
        }

        [Parameter]
        public string Property { get; set; }

        [Parameter]
        public Attribute[] Attributes { get; set; }

        [Parameter]
        public string StringFormat { get; set; }

        [Parameter(CaptureUnmatchedValues = true)]
        public IReadOnlyDictionary<string, object> AdditionalAttributes { get; set; }

        private string mHeaderText;

        [Parameter]
        public string HeaderText
        {
            get { return mHeaderText ?? Property; }
            set { mHeaderText = value; }
        }

        private IMRegister mGrid;

        [CascadingParameter]
        public IMRegister Grid
        {
            get
            {
                return mGrid;
            }
            set
            {
                if (value != mGrid)
                {
                    mGrid = value;
                    mGrid.RegisterColumn(this);
                }
            }
        }

        public bool ShouldRenderColumn => true;

        public RenderFragment GenerateContent(T pModel)
        {
            return CellTemplate(pModel);
        }

    }
}
