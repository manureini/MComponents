using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;

namespace MComponents.MGrid
{
    public class MGridColumn : ComponentBase, IMGridColumn, IMGridPropertyColumn, IMGridSortableColumn
    {
        [Parameter]
        public string Property { get; set; }

        [Parameter]
        public virtual Type PropertyType { get; set; }

        [Parameter]
        public Attribute[] Attributes { get; set; }

        [Parameter]
        public bool ExtendAttributes { get; set; }

        [Parameter]
        public string StringFormat { get; set; }

        [Parameter(CaptureUnmatchedValues = true)]
        public IReadOnlyDictionary<string, object> AdditionalAttributes { get; set; }

        [Parameter]
        public MSortDirection SortDirection { get; set; } = MSortDirection.None;

        [Parameter]
        public int SortIndex { get; set; }

        [Parameter]
        public bool EnableSorting { get; set; } = true;

        [Parameter]
        public bool EnableFilter { get; set; } = true;

        [Parameter]
        public string HeaderText { get; set; }

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

        public virtual string Identifier => Property;

        public bool VisibleInExport => true;

        public bool ShouldRenderColumn => true;
    }
}
