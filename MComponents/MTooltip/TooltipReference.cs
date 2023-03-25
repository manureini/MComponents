using Microsoft.AspNetCore.Components;
using System;

namespace MComponents.MTooltip
{
    internal class TooltipReference
    {
        public Guid Id { get; set; }
        public string Text { get; set; }
        public RenderFragment Content { get; set; }
    }
}
