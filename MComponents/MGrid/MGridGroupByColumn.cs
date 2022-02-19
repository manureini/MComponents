using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using System.Collections.Generic;

namespace MComponents.MGrid
{
    public class MGridGroupByColumn<T> : ComponentBase, IMGridColumnGenerator<T>
    {
        [Inject]
        public IStringLocalizer L { get; set; }

        [Parameter(CaptureUnmatchedValues = true)]
        public IReadOnlyDictionary<string, object> AdditionalAttributes { get; set; }

        [Parameter]
        public string HeaderText { get; set; }

        [Parameter]
        public string Identifier { get; set; }

        private IMGrid<T> mGrid;

        [CascadingParameter]
        public IMGrid<T> Grid
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

        public bool EnableFilter => false;

        public bool ShouldRenderColumn => true;

        public bool VisibleInExport => true;

        public IMGridColumn GridColumn { get; set; }
        
        public RenderFragment GenerateContent(T pModel)
        {
            return builder =>
            {
            };
        } 
    }
}
