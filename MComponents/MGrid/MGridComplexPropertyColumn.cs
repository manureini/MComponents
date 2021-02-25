using DocumentFormat.OpenXml.Spreadsheet;
using MComponents.MForm;
using MComponents.MSelect;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;

namespace MComponents.MGrid
{
    public class MGridComplexPropertyColumn<T, TProperty> : ComponentBase, IMGridColumn, IMGridPropertyColumn, IMGridSortableColumn, IMGridCustomComparer<TProperty>, IMGridComplexEditableColumn<TProperty>, IMGridColumnGenerator<T>
    {
        private RenderFragment<MComplexPropertyFieldContext<TProperty>> mFormTemplate;

        [Parameter]
        public RenderFragment<MComplexPropertyFieldContext<TProperty>> FormTemplate
        {
            get => mFormTemplate ?? DefaultFormTemplate;
            set => mFormTemplate = value;
        }

        [Parameter]
        public RenderFragment<T> CellTemplate { get; set; }

        [Parameter]
        public string Identifier { get; set; }

        [Parameter]
        public bool EnableFilter { get; set; } = true;

        [Parameter]
        public Func<T, string> ExportText { get; set; }

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
        public string ReferencedPropertyToDisplay { get; set; }

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

        public bool VisibleInExport { get; set; } = true;

        [Parameter]
        public MSortDirection SortDirection { get; set; } = MSortDirection.None;

        [Parameter]
        public int SortIndex { get; set; }

        [Parameter]
        public bool EnableSorting { get; set; } = typeof(IComparable).IsAssignableFrom(typeof(TProperty)) || typeof(IComparable<TProperty>).IsAssignableFrom(typeof(TProperty));

        [Parameter]
        public IComparer<TProperty> Comparer { get; set; }

        [Parameter]
        public IEnumerable<TProperty> ReferencedValues { get; set; }

        protected override void OnParametersSet()
        {
            if (CellTemplate == null && ReferencedPropertyToDisplay == null)
                throw new ArgumentNullException(nameof(ReferencedPropertyToDisplay), $"Please specify {nameof(ReferencedPropertyToDisplay)} or {nameof(CellTemplate)}");

            if (mFormTemplate == null && ReferencedValues == null)
                throw new ArgumentNullException(nameof(ReferencedValues), $"Please specify {nameof(ReferencedValues)} or {nameof(FormTemplate)}");

            if (Identifier == null)
            {
                Identifier = Property + typeof(TProperty).FullName + ReferencedPropertyToDisplay;
            }

            if (Comparer != null)
            {
                EnableSorting = true;
            }
        }

        public RenderFragment GenerateContent(T pModel)
        {
            if (CellTemplate == null)
                return DefaultCell(pModel);

            return CellTemplate(pModel);
        }

        private RenderFragment<T> DefaultCell => m =>
                   (builder) =>
                   {
                       var propInfo = ReflectionHelper.GetIMPropertyInfo(typeof(T), Property, typeof(TProperty));

                       var otherObj = propInfo.GetValue(m);

                       if (otherObj == null)
                           return;

                       var otherPropDisplay = ReflectionHelper.GetIMPropertyInfo(typeof(TProperty), ReferencedPropertyToDisplay, typeof(string));

                       var value = otherPropDisplay.GetValue(otherObj);

                       builder.AddContent(22, value);
                   };

        private RenderFragment<MComplexPropertyFieldContext<TProperty>> DefaultFormTemplate => m =>
                    (builder) =>
                    {
                        builder.OpenComponent<MSelect<TProperty>>(100);
                        builder.AddAttribute(101, "id", m.InputId);
                        builder.AddAttribute(102, "class", "form-control");
                        builder.AddAttribute(102, nameof(MSelect<TProperty>.Property), ReferencedPropertyToDisplay);
                        builder.AddAttribute(102, nameof(MSelect<TProperty>.Options), ReferencedValues);
                        builder.AddAttribute(102, nameof(MSelect<TProperty>.Value), m.Value);
                        builder.AddAttribute(102, nameof(MSelect<TProperty>.ValueChanged), m.ValueChanged);
                        builder.AddAttribute(102, nameof(MSelect<TProperty>.ValueExpression), m.ValueExpression);
                        builder.CloseComponent();
                    };

    }
}
