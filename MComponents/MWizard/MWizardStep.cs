﻿using Microsoft.AspNetCore.Components;

namespace MComponents.MWizard
{
    public class MWizardStep : ComponentBase
    {
        [Parameter]
        public string Identifier { get; set; }

        [Parameter]
        public string Title { get; set; }

        [Parameter]
        public RenderFragment<MWizardStepContext> Content { get; set; }

        [Parameter]
        public MWizardStepContext StepContext { get; set; }

        protected bool mIsVisible = true;

        [Parameter]
        public bool IsVisible
        {
            get
            {
                return mIsVisible;
            }
            set
            {
                bool newValue = value;
                bool oldValue = mIsVisible;
                mIsVisible = value;
                if (newValue != oldValue)
                    mWizard?.InvokeStateHasChanged();
            }
        }

        [Parameter]
        public int Position { get; set; } = 0;

        private IMWizard mWizard;

        [CascadingParameter]
        public IMWizard Wizard
        {
            get
            {
                return mWizard;
            }
            set
            {
                if (value != mWizard)
                {
                    mWizard = value;
                    mWizard.RegisterStep(this);
                }
            }
        }

        protected override void OnParametersSet()
        {
            if (StepContext == null)
                StepContext = new MWizardStepContext();

            StepContext.CurrentStep = this;
        }
    }
}
