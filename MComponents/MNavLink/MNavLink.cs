using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Routing;

namespace MComponents.MNavLink
{
    public class MNavLink : NavLink
    {
        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            /*
            builder.OpenElement(28, "li");
            builder.AddAttribute(29, "class", "kt-menu__item " + CssClass?.Replace("active", "kt-menu__item--active"));
            builder.AddAttribute(30, "aria-haspopup", "true");

            builder.OpenElement(0, "a");
            builder.AddAttribute(29, "class", "kt-menu__link");

            builder.AddMultipleAttributes(1, AdditionalAttributes);
            builder.AddContent(3, ChildContent);
            builder.CloseElement();

            builder.CloseElement();
            */
        }
    }
}