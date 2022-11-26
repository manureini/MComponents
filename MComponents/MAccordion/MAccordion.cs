using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using System.Collections.Generic;
using System.Linq;

namespace MComponents.MAccordion
{
    public class MAccordion : ComponentBase
    {
        [Parameter]
        public RenderFragment Cards { get; set; }

        [Parameter]
        public bool AllowMultipleOpenCards { get; set; }

        [Parameter]
        public bool IsReadOnly { get; set; }

        [Parameter]
        public bool RenderHiddenCards { get; set; }

        [Parameter]
        public bool CacheCards { get; set; }

        protected List<int> mOpenIndexes = new List<int>();

        protected List<MAccordionCard> CardsList { get; set; } = new List<MAccordionCard>();

        protected override void OnParametersSet()
        {
            base.OnParametersSet();

            if (CacheCards)
                return;

            CardsList.Clear();
        }

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            builder.OpenRegion(23);

            if (Cards != null)
            {
                RenderFragment child3() =>
                        (builder2) =>
                        {
                            builder2.AddMarkupContent(35, "\r\n");
                            builder2.AddContent(36, this.Cards);
                            builder2.AddMarkupContent(37, "\r\n");
                        };

                builder.OpenComponent<CascadingValue<MAccordion>>(4);
                builder.AddAttribute(41, "Value", this);
                builder.AddAttribute(42, "ChildContent", child3());
                builder.CloseComponent();
            }

            builder.OpenElement(46, "div");
            builder.AddAttribute(47, "class", "m-accordion");

            for (int i = 0; i < CardsList.Count; i++)
            {
                var card = CardsList[i];
                int index = i;

                bool isVisible = mOpenIndexes.Contains(i);

                builder.OpenElement(57, "div");
                builder.AddAttribute(58, "class", "m-accordion-card");

                builder.OpenElement(60, "div");
                builder.AddAttribute(61, "class", "m-accordion-card-header" + (IsReadOnly ? " m-readonly" : string.Empty));

                builder.OpenElement(63, "div");
                builder.AddAttribute(64, "class", "m-accordion-card-title");
                builder.AddAttribute(65, "style", "display: block;");

                builder.AddAttribute(67, "data-toggle", "collapse");
                builder.AddAttribute(68, "onclick", EventCallback.Factory.Create(this, () => OnClicked(index)));

                builder.AddContent(70, (MarkupString)card.Title);

                if (!IsReadOnly)
                {
                    if (isVisible)
                    {
                        builder.AddContent(76, (MarkupString)"<i class=\"fas fa-chevron-down\" style=\"float: right;\"></i>");
                    }
                    else
                    {
                        builder.AddContent(80, (MarkupString)"<i class=\"fas fa-chevron-right\" style=\"float: right;\"></i>");
                    }
                }

                builder.CloseElement();

                builder.CloseElement();

                if (isVisible || RenderHiddenCards)
                {
                    builder.OpenElement(90, "div");

                    string cssClass = "m-accordion-card-body";

                    if (isVisible)
                        cssClass += " m-show";

                    builder.AddAttribute(97, "class", cssClass);

                    builder.OpenComponent<CascadingValue<object>>(99);
                    builder.AddAttribute(100, "Value", string.Empty);
                    builder.AddAttribute(101, "ChildContent", card.ChildContent);
                    builder.CloseComponent();

                    builder.CloseElement(); //div m-accordion-card-body
                }

                builder.CloseElement(); //div accordion-card
            }

            builder.CloseElement();

            builder.CloseRegion();
        }

        public void OnClicked(int pIndex)
        {
            if (IsReadOnly)
                return;

            if (mOpenIndexes.Contains(pIndex))
            {
                if (AllowMultipleOpenCards)
                    mOpenIndexes.Remove(pIndex);
                else
                    mOpenIndexes.Clear();
            }
            else
            {
                if (!AllowMultipleOpenCards)
                    mOpenIndexes.Clear();
                mOpenIndexes.Add(pIndex);
            }
            StateHasChanged();
        }

        public void RegisterCard(MAccordionCard pCard)
        {
            if (CardsList.Any(c => c.Identifier == pCard.Identifier))
                return;

            CardsList.Add(pCard);

            if (pCard.InitialOpen)
                mOpenIndexes.Add(CardsList.Count - 1);

            StateHasChanged();
        }
    }
}
