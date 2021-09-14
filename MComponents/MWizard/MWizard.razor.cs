﻿using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MComponents.MWizard
{
    public partial class MWizard
    {
        [Inject]
        public IJSRuntime JsRuntime { get; set; }

        [Parameter]
        public RenderFragment Steps { get; set; }

        [Parameter]
        public EventCallback<StepChangedArgs> OnStepChanged { get; set; }

        [Parameter]
        public EventCallback<SubmitEventArgs> OnSubmit { get; set; }

        [Parameter]
        public bool EnableJumpToAnyStep { get; set; }

        [Parameter]
        public RenderFragment ButtonPrev { get; set; } = (r) =>
        {
            r.OpenComponent<MWizardPrevButton>(30);
            r.CloseComponent();
        };

        [Parameter]
        public RenderFragment ButtonNext { get; set; } = (r) =>
        {
            r.OpenComponent<MWizardNextButton>(38);
            r.CloseComponent();
        };

        [Parameter]
        public RenderFragment ButtonFinish { get; set; } = (r) =>
        {
            r.OpenComponent<MWizardFinishButton>(45);
            r.CloseComponent();
        };

        public bool FreezeCurrentStep { get; set; }

        public int CurrentStep { get; protected set; }

        public List<MWizardStep> WizardSteps { get; protected set; } = new List<MWizardStep>();

        protected SemaphoreSlim mLocker = new SemaphoreSlim(1, 1);

        protected bool mEmptyRender = true;

        protected DotNetObjectReference<MWizard> mObjReference;

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                mObjReference = DotNetObjectReference.Create(this);
                await JsRuntime.InvokeVoidAsync("mcomponents.registerKeyListener", GetHashCode(), mObjReference);
            }
        }

        ~MWizard()
        {
            _ = JsRuntime.InvokeVoidAsync("mcomponents.unRegisterKeyListener", GetHashCode());
        }

        [JSInvokable]
        public void JsInvokeKeyDown(string pKey)
        {
            if (pKey == "Escape")
            {
                InvokeAsync(OnPrevClicked);
            }
        }

        public void RegisterStep(MWizardStep pStep)
        {
            if (WizardSteps.Any(s => s.Identifier == pStep.Identifier))
                return;

            WizardSteps.Add(pStep);
            WizardSteps = WizardSteps.OrderBy(s => s.Position).ToList();

            if (CurrentStep == -1 && pStep.IsVisible)
                CurrentStep = WizardSteps.Count - 1;

            StateHasChanged();
        }

        protected override void OnAfterRender(bool firstRender)
        {
            if (mEmptyRender)
            {
                mEmptyRender = false;
                StateHasChanged();
            }
        }

        internal async void OnNextClicked()
        {
            if (FreezeCurrentStep)
                return;

            await InternalSetStep(CurrentStep, true, i => i + 1);
        }

        internal async void OnPrevClicked()
        {
            if (FreezeCurrentStep)
                return;

            await InternalSetStep(CurrentStep, true, i => i - 1);
        }

        protected void OnJumpToClicked(int pIndex)
        {
            if (FreezeCurrentStep)
                return;

            if (!CanJumpTo(pIndex))
                return;

            SetCurrentStep(pIndex, true);
        }

        public async void OnFinishClicked()
        {
            if (FreezeCurrentStep)
                return;

            await OnSubmit.InvokeAsync(new SubmitEventArgs());

            StateHasChanged();
        }

        protected string GetCurrentState()
        {
            if (FindNextVisible(CurrentStep, i => i - 1) == null)
                return "first";

            if (FindNextVisible(CurrentStep, i => i + 1) == null)
                return "last";

            return "between";
        }

        protected bool CanJumpTo(int pIndex)
        {
            if (EnableJumpToAnyStep)
                return true;

            if (FindNextVisible(CurrentStep, i => i - 1) == pIndex)
                return true;

            if (FindNextVisible(CurrentStep, i => i + 1) == pIndex)
                return true;

            return false;
        }

        public void InvokeStateHasChanged()
        {
            StateHasChanged();
        }

        protected int? FindNextVisible(int pStartIndex, Func<int, int> pModifier)
        {
            int index = pStartIndex;
            int? lastIndex = null;

            while (true)
            {
                index = pModifier(index);

                if (index < 0 || index >= WizardSteps.Count)
                    return null;

                if (WizardSteps[index].IsVisible)
                    return index;

                if (lastIndex == index)
                    return null;

                lastIndex = index;
            }
        }

        public void SetCurrentStep(int pIndex, bool pUserInteract)
        {
            if (FreezeCurrentStep)
                return;

            if (pIndex == CurrentStep)
                return;

            //pIndex could be invisible after delay, so it's better to use prev or next
            if (FindNextVisible(CurrentStep, i => i - 1) == pIndex)
            {
                OnPrevClicked();
                return;
            }

            if (FindNextVisible(CurrentStep, i => i + 1) == pIndex)
            {
                OnNextClicked();
                return;
            }

            _ = InternalSetStep(CurrentStep, true, i => pIndex);
        }

        protected async Task InternalSetStep(int pOldStep, bool pUserInteract, Func<int, int> pModifier)
        {
            try
            {
                var result = await mLocker.WaitAsync(1000);

                if (!result)
                    return;

                int? next = FindNextVisible(pOldStep, pModifier);
                if (!next.HasValue)
                    return;

                var newStep = next.Value;

                var args = new StepChangedArgs()
                {
                    OldStepIndex = pOldStep,
                    NewStepIndex = newStep,
                    OldStep = WizardSteps[pOldStep],
                    NewStep = WizardSteps[newStep],
                    UserInteract = pUserInteract
                };

                await InvokeAsync(async () =>
                {
                    await OnStepChanged.InvokeAsync(args);
                });

                if (args.Cancelled)
                    return;

                if (args.DelayStepTransition)
                {
                    int step = CurrentStep;
                    _ = Task.Delay(50).ContinueWith(async t =>
                    {
                        await mLocker.WaitAsync();

                        CurrentStep = FindNextVisible(step, pModifier) ?? step;
                        mEmptyRender = true;

                        mLocker.Release();

                        _ = InvokeAsync(StateHasChanged);
                    });
                    _ = InvokeAsync(StateHasChanged);
                    return;
                }

                CurrentStep = newStep;
                mEmptyRender = true;

                _ = InvokeAsync(StateHasChanged);
            }
            finally
            {
                mLocker.Release();
            }
        }
    }
}
