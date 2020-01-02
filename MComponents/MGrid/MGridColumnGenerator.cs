using System.Collections.Generic;
using Microsoft.AspNetCore.Components;

namespace MComponents.MGrid
{
    public class MGridColumnGenerator<T> : ComponentBase, IMGridColumnGenerator<T>
    {
        [Parameter(CaptureUnmatchedValues = true)]
        public IReadOnlyDictionary<string, object> AdditionalAttributes { get; set; }

        [Parameter]
        public string HeaderText { get; set; } = string.Empty;

        [Parameter]
        public string Identifier { get; set; } = string.Empty;

        [Parameter]
        public RenderFragment<T> Template { get; set; }

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

        public bool EnableFilter => true;

        public bool ShouldRenderColumn => true;

        public RenderFragment GenerateContent(T pModel)
        {
            return Template(pModel);
        }
    }
}
