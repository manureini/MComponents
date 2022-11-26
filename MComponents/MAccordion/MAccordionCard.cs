﻿using Microsoft.AspNetCore.Components;

namespace MComponents.MAccordion
{
    public class MAccordionCard : ComponentBase
    {
        [Parameter]
        public string Identifier { get; set; }

        [Parameter]
        public string Title { get; set; }

        [Parameter]
        public bool InitialOpen { get; set; }

        [Parameter]
        public RenderFragment ChildContent { get; set; }

        private MAccordion mAccordion;

        [CascadingParameter]
        public MAccordion Accordion
        {
            get
            {
                return mAccordion;
            }
            set
            {
                if (mAccordion != null && value == mAccordion && mAccordion.CacheCards)
                    return;

                if (mAccordion == value)
                    InitialOpen = false;

                mAccordion = value;
                mAccordion.RegisterCard(this);
            }
        }
    }
}
