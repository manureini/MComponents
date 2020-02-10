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
        public bool AllowMultipleOpenCards { get; set; } = false;

        [Parameter]
        public bool IsReadOnly { get; set; } = false;

        protected List<int> mOpenIndexes = new List<int>();

        protected List<MAccordionCard> CardsList { get; set; } = new List<MAccordionCard>();

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            builder.OpenRegion(23);

            if (Cards != null)
            {
                RenderFragment child3() =>
                        (builder2) =>
                        {
                            builder2.AddMarkupContent(1, "\r\n");
                            builder2.AddContent(2, this.Cards);
                            builder2.AddMarkupContent(3, "\r\n");
                        };

                builder.OpenComponent<CascadingValue<MAccordion>>(4);
                builder.AddAttribute(5, "Value", this);
                builder.AddAttribute(6, "ChildContent", child3());
                builder.CloseComponent();
            }

            builder.OpenElement(0, "div");
            builder.AddAttribute(1, "class", "m-accordion");


            for (int i = 0; i < CardsList.Count; i++)
            {
                var card = CardsList[i];
                int index = i;

                bool isVisible = mOpenIndexes.Contains(i);

                builder.OpenElement(4, "div");
                builder.AddAttribute(5, "class", "m-accordion-card");

                builder.OpenElement(7, "div");
                builder.AddAttribute(8, "class", "m-accordion-card-header");

                builder.OpenElement(10, "div");
                builder.AddAttribute(11, "class", "m-accordion-card-title m-collapsed");
                builder.AddAttribute(11, "style", "display: block;");

                builder.AddAttribute(12, "data-toggle", "collapse");
                builder.AddAttribute(21, "onclick", EventCallback.Factory.Create(this, () => OnClicked(index)));

                builder.AddContent(14, (MarkupString)card.Title);

                if (!IsReadOnly)
                {
                    if (isVisible)
                    {
                        builder.AddContent(15, (MarkupString)"<i class=\"fas fa-chevron-down\" style=\"float: right;\"></i>");
                    }
                    else
                    {
                        builder.AddContent(15, (MarkupString)"<i class=\"fas fa-chevron-right\" style=\"float: right;\"></i>");
                    }
                }

                builder.CloseElement();

                builder.CloseElement();

                builder.OpenElement(18, "div");


                string cssClass = "m-collapse";

                if (isVisible)
                    cssClass += " m-show";

                builder.AddAttribute(19, "class", cssClass);

                builder.OpenElement(23, "div");
                builder.AddAttribute(24, "class", "m-accordion-card-body");

                builder.OpenComponent<CascadingValue<object>>(4);
                builder.AddAttribute(5, "Value", string.Empty);
                builder.AddAttribute(6, "ChildContent", card.ChildContent);
                builder.CloseComponent();

                builder.CloseElement();

                builder.CloseElement();

                builder.CloseElement();
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
