using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using System;
using System.Collections.Generic;

namespace MComponents.Tabs
{
    public class MTab : ComponentBase
    {
        [Parameter]
        public string Title { get; set; }

        [Parameter(CaptureUnmatchedValues = true)]
        public IReadOnlyDictionary<string, object> AdditionalAttributes { get; set; }

        [CascadingParameter]
        private MTabs Tabs { get; set; }

        [Parameter]
        public RenderFragment ChildContent { get; set; }

        protected override void OnInitialized()
        {
            base.OnInitialized();

            if (Tabs == null)
                throw new Exception($"{nameof(MTab)} can only be placed in a {nameof(MTabs)} element");

            Tabs.RegisterTab(this);
        }

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            if (Tabs.CurrentTab == this)
                ChildContent?.Invoke(builder);
        }
    }
}
