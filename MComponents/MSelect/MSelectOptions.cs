using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace MComponents.MGrid
{
    public class MSelectOptions : ComponentBase
    {
        protected override void BuildRenderTree(RenderTreeBuilder pBuilder)
        {
            pBuilder.AddContent(0, ChildContent);
        }

        [Parameter]
        public RenderFragment ChildContent { get; set; }
    }
}
