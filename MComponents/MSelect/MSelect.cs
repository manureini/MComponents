using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Localization;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace MComponents.MSelect
{
    public class MSelect<T> : InputSelect<T>, IMSelect
    {
        [Inject]
        private IJSRuntime JSRuntime { get; set; }

        [Inject]
        public IStringLocalizer L { get; set; }

        [CascadingParameter]
        protected EditContext CascadedEditContext2 { get; set; }

        [Parameter]
        public IEnumerable<T> Options { get; set; }

        [Parameter]
        public string Property { get; set; }

        [Parameter]
        public Type PropertyType { get; set; }

        [Parameter]
        public bool EnableSearch { get; set; }

        [Parameter]
        public bool IsDisabled { get; set; }

        [Parameter]
        public EventCallback<SelectionChangedArgs<T>> OnSelectionChanged { get; set; }

        protected string mNullValueOverride;

        private string mNullValueDescription;

        [Parameter]
        public string NullValueDescription
        {
            get => mNullValueOverride ?? mNullValueDescription;
            set => mNullValueDescription = value;
        }

        [Parameter]
        public bool EnableSelectAll { get; set; }


        [Parameter]
        public Func<T, string> CustomFormatter { get; set; }

        [Parameter]
        public ICollection<T> Values { get; set; }

        [Parameter]
        public EventCallback<ICollection<T>> ValuesChanged { get; set; }

        [Parameter]
        public Expression<Func<ICollection<T>>> ValuesExpression { get; set; }

        protected ElementReference OptionsDiv { get; set; }
        protected ElementReference SelectSpan { get; set; }

        protected bool mOptionsVisible;
        protected DateTime mBlockUntil;

        protected T[] DisplayValues = Array.Empty<T>();

        protected T SelectedValue;

        protected ElementReference SearchInput { get; set; }

        protected string InputValue = string.Empty;

        protected IMPropertyInfo mPropertyInfo;

        protected List<MSelectOption> mAdditionalOptions = new List<MSelectOption>();

        protected Type mtType = Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T);
        protected bool mIsNullableType = typeof(T).IsGenericType && typeof(T).GetGenericTypeDefinition() == typeof(Nullable<>);

        protected DotNetObjectReference<MSelect<T>> mObjReference;

        protected bool mMultipleSelectMode;
        protected bool mEnumFlags;
        protected bool mAllEntriesSelected;

        public override async Task SetParametersAsync(ParameterView parameters)
        {
            //Workaround for bypass check if ValueExpression is set

            parameters.SetParameterProperties(this);

            mEnumFlags = mtType.IsEnum && mtType.GetCustomAttribute<FlagsAttribute>() != null;

            mMultipleSelectMode = ValuesExpression != null || mEnumFlags;

            if (mMultipleSelectMode && !mEnumFlags)
            {
                EditContext = CascadedEditContext2;
#if NET7_0_OR_GREATER
                typeof(InputBase<T>).GetField("_hasInitializedParameters", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(this, true);
#endif
            }

            if (mMultipleSelectMode && !mEnumFlags)
            {
                FieldIdentifier = FieldIdentifier.Create(ValuesExpression);
            }

            await base.SetParametersAsync(parameters);

            mNullValueDescription = mNullValueDescription ?? L["Please select..."];
        }

        protected override void OnParametersSet()
        {
            if (Property != null)
            {
                mPropertyInfo = ReflectionHelper.GetIMPropertyInfo(typeof(T), Property, PropertyType);
            }
            else
            {
                mPropertyInfo = null;
            }

            if (Options == null)
            {
                DisplayValues = Array.Empty<T>();
            }
            else
            {
                DisplayValues = Options.ToArray();
            }

            UpdateDescription();
        }

        protected override void OnInitialized()
        {
            if (ValuesExpression != null && ValueExpression != null)
                throw new ArgumentException($"use {nameof(Values)} or {nameof(Value)} through bind-, but not both");

            if (mMultipleSelectMode && Values == null && !mEnumFlags)
                throw new ArgumentException($"{nameof(Values)} must be != null");

            if (mEnumFlags && Value == null && !mIsNullableType)
                throw new ArgumentException($"{nameof(Value)} must be != null");

            if (Options == null && mtType.IsEnum)
            {
                Options = Enum.GetValues(mtType).Cast<T>();

                if (mEnumFlags)
                {
                    Options = Options.Where(v => (int)((object)v) != 0);
                }

                Options = Options.ToArray();
            }
        }

        protected override void OnAfterRender(bool firstRender)
        {
            if (firstRender)
            {
                mObjReference = DotNetObjectReference.Create(this);
            }

            if (mOptionsVisible)
            {
                _ = JSRuntime.InvokeVoidAsync("mcomponents.findBestMSelectOptionsPosition");
            }
        }

        [JSInvokable]
        public void JsInvokeMSelectFocusOut()
        {
            HideOptions(false);
        }

        protected override void BuildRenderTree(RenderTreeBuilder pBuilder)
        {
            if (this.ChildContent != null)
            {
                RenderFragment child() =>
                        (builder2) =>
                        {
                            builder2.AddContent(176, this.ChildContent);
                        };

                pBuilder.OpenComponent<CascadingValue<MSelect<T>>>(4);
                pBuilder.AddAttribute(195, "Value", this);
                pBuilder.AddAttribute(186, "ChildContent", child());
                pBuilder.CloseComponent();
            }

            pBuilder.OpenElement(200, "div");
            pBuilder.AddAttribute(201, "class", "m-select");

            if (!IsDisabled)
            {
                pBuilder.AddAttribute(205, "tabindex", "0");
                pBuilder.AddAttribute(206, "onkeydown", EventCallback.Factory.Create<KeyboardEventArgs>(this, InputKeyDown));
                pBuilder.AddEventStopPropagationAttribute(207, "onkeydown", true);
                pBuilder.AddEventStopPropagationAttribute(208, "onkeyup", true);

                pBuilder.AddAttribute(210, "onfocus", EventCallback.Factory.Create<FocusEventArgs>(this, OnFocusIn));
            }

            pBuilder.OpenElement(213, "span");

            if (!IsDisabled)
            {
                pBuilder.AddAttribute(217, "onclick", EventCallback.Factory.Create<MouseEventArgs>(this, OnComboboxClicked));
                pBuilder.AddEventStopPropagationClicksAttribute(218);
            }

            pBuilder.AddAttribute(221, "class", "m-form-control m-clickable " + CssClass + (IsDisabled ? " m-select--disabled" : string.Empty) + (mOptionsVisible ? " m-select--open" : string.Empty));

            if (AdditionalAttributes != null)
                pBuilder.AddMultipleAttributes(224, AdditionalAttributes.Where(a => a.Key.ToLower() != "class" && a.Key.ToLower() != "onkeydown" && a.Key.ToLower() != "onkeyup"));

            pBuilder.AddElementReferenceCapture(226, (__value) =>
            {
                SelectSpan = __value;
            });

            pBuilder.OpenElement(231, "span");
            pBuilder.AddAttribute(232, "class", "m-select-content");
            pBuilder.AddAttribute(233, "role", "textbox");

            pBuilder.AddContent(235, mMultipleSelectMode ? NullValueDescription : CurrentValueAsString);

            pBuilder.CloseElement();

            pBuilder.AddMarkupContent(242, "<span class=\"m-select-dropdown-icon fa-solid fa-angle-down\" role =\"presentation\"></span>");

            pBuilder.CloseElement(); //span

            if (mOptionsVisible)
            {
                pBuilder.OpenElement(248, "div");
                pBuilder.AddAttribute(249, "tabindex", "0");

                pBuilder.AddAttribute(251, "onkeydown", EventCallback.Factory.Create<KeyboardEventArgs>(this, InputKeyDown));
                pBuilder.AddEventStopPropagationAttribute(252, "onkeydown", true);
                pBuilder.AddEventStopPropagationAttribute(253, "onkeyup", true);
                pBuilder.AddEventPreventDefaultAttribute(254, "onkeydown", true);
                pBuilder.AddEventPreventDefaultAttribute(255, "onkeyup", true);

                pBuilder.AddAttribute(257, "class", "m-select-options-container"); //also used in mcomponents.js

                pBuilder.AddElementReferenceCapture(259, (__value) =>
                {
                    OptionsDiv = __value;
                });

                pBuilder.OpenElement(264, "div");
                pBuilder.AddAttribute(265, "class", "m-select-options-list-container");
                pBuilder.AddAttribute(266, "style", "visibility: hidden;"); //also used in mcomponents.js

                if (EnableSearch)
                {
                    pBuilder.OpenElement(270, "span");
                    pBuilder.AddAttribute(271, "class", "m-select-search-container");

                    pBuilder.OpenElement(273, "input");
                    pBuilder.AddAttribute(274, "oninput", EventCallback.Factory.Create<ChangeEventArgs>(this, OnSearchInputChanged));
                    pBuilder.AddAttribute(275, "onkeydown", EventCallback.Factory.Create<KeyboardEventArgs>(this, InputKeyDown));

                    pBuilder.AddEventStopPropagationAttribute(277, "onkeydown", true);
                    pBuilder.AddEventStopPropagationAttribute(278, "onkeyup", true);
                    pBuilder.AddEventStopPropagationAttribute(279, "oninput", true);

                    pBuilder.AddAttribute(281, "value", "");
                    pBuilder.AddAttribute(282, "class", "m-form-control m-select-search-input");
                    pBuilder.AddAttribute(283, "type", "search");
                    pBuilder.AddAttribute(284, "tabindex", "0");
                    pBuilder.AddAttribute(285, "autocomplete", "off");
                    pBuilder.AddAttribute(286, "autocorrect", "off");
                    pBuilder.AddAttribute(287, "autocapitalize", "none");
                    pBuilder.AddAttribute(288, "spellcheck", "false");
                    pBuilder.AddAttribute(289, "role", "search");

                    pBuilder.AddElementReferenceCapture(290, (__value) =>
                    {
                        SearchInput = __value;
                    });
                    pBuilder.CloseElement(); //input

                    pBuilder.CloseElement(); //span

                    Task.Delay(50).ContinueWith((a) =>
                    {
                        if (SearchInput.Id != null)
                            _ = JSRuntime.InvokeVoidAsync("mcomponents.focusElement", SearchInput);
                    });
                }
                else
                {
                    Task.Delay(50).ContinueWith((a) =>
                    {
                        if (OptionsDiv.Id != null)
                            _ = JSRuntime.InvokeVoidAsync("mcomponents.focusElement", OptionsDiv);
                    });
                }

                pBuilder.OpenElement(311, "ul");
                pBuilder.AddAttribute(312, "class", "m-select-options-list m-scrollbar");
                pBuilder.AddAttribute(313, "role", "listbox");

                int i = 0;

                if (EnableSelectAll && mMultipleSelectMode)
                {
                    pBuilder.OpenElement(319, "li");
                    pBuilder.AddAttribute(320, "class", "m-select-options-entry m-clickable");
                    pBuilder.AddAttribute(321, "onclick", EventCallback.Factory.Create<MouseEventArgs>(this, () => ToggleSelectAll()));
                    pBuilder.AddAttribute(322, "role", "option");

                    pBuilder.OpenElement(324, "label");
                    pBuilder.AddAttribute(325, "class", "m-select-checkbox m-clickable");
                    pBuilder.AddEventStopPropagationClicksAttribute(326);

                    pBuilder.OpenElement(328, "input");
                    pBuilder.AddAttribute(329, "type", "checkbox");

                    pBuilder.AddAttribute(331, "onchange", EventCallback.Factory.Create<ChangeEventArgs>(this, () => ToggleSelectAll()));
                    pBuilder.AddEventStopPropagationClicksAttribute(332);

                    if (mAllEntriesSelected)
                    {
                        pBuilder.AddAttribute(336, "checked", "checked");
                    }

                    pBuilder.CloseElement(); //input

                    pBuilder.OpenElement(341, "span"); //this is required for design magic
                    pBuilder.CloseElement();

                    pBuilder.AddContent(344, L["Select all entries"]);

                    pBuilder.CloseElement(); //label

                    pBuilder.CloseElement(); //li
                }

                foreach (var entry in DisplayValues)
                {
                    int index = i;

                    bool isSelected = entry != null && entry.Equals(SelectedValue);

                    pBuilder.OpenElement(357, "li");
                    pBuilder.AddAttribute(358, "class", "m-select-options-entry m-clickable" + (isSelected ? " m-select-options-entry--highlighted" : string.Empty));
                    pBuilder.AddAttribute(359, "onclick", EventCallback.Factory.Create<MouseEventArgs>(this, () => OnOptionSelect(index)));
                    pBuilder.AddAttribute(360, "role", "option");

                    if (mMultipleSelectMode)
                    {
                        pBuilder.OpenElement(364, "label");
                        pBuilder.AddAttribute(365, "class", "m-select-checkbox m-clickable");
                        pBuilder.AddEventStopPropagationClicksAttribute(366);

                        pBuilder.OpenElement(368, "input");
                        pBuilder.AddAttribute(369, "type", "checkbox");

                        pBuilder.AddAttribute(371, "onchange", EventCallback.Factory.Create<ChangeEventArgs>(this, () => OnOptionSelect(index)));
                        pBuilder.AddEventStopPropagationClicksAttribute(372);

                        if ((mEnumFlags && ValueEnumHasFlag(entry)) || (!mEnumFlags && Values.Contains(entry)))
                        {
                            pBuilder.AddAttribute(376, "checked", "checked");
                        }

                        pBuilder.CloseElement(); //input

                        pBuilder.OpenElement(381, "span"); //this is required for design magic
                        pBuilder.CloseElement();

                        pBuilder.AddContent(384, FormatValueAsString(entry));

                        pBuilder.CloseElement(); //label
                    }
                    else
                    {
                        pBuilder.AddAttribute(390, "onclick", EventCallback.Factory.Create<MouseEventArgs>(this, () => OnOptionSelect(index)));
                        pBuilder.AddContent(391, FormatValueAsString(entry));
                    }

                    pBuilder.CloseElement(); //li
                    i++;
                }

                foreach (var entry in mAdditionalOptions.Where(a => !a.ShowOnlyIfNoSearchResults || (i == 0 && a.ShowOnlyIfNoSearchResults)))
                {
                    bool isSelected = entry != null && entry.Equals(SelectedValue);

                    pBuilder.OpenElement(402, "li");
                    pBuilder.AddAttribute(403, "class", "m-select-options-entry m-clickable" + (isSelected ? " m-select-options-entry--highlighted" : string.Empty));
                    pBuilder.AddAttribute(404, "role", "option");
                    //        pBuilder.AddAttribute(79, "aria-selected", "false");

                    pBuilder.AddMultipleAttributes(407, entry.AdditionalAttributes);

                    pBuilder.AddContent(409, entry.Value.ToString());
                    pBuilder.CloseElement();
                }

                pBuilder.CloseElement();
                pBuilder.CloseElement();
                pBuilder.CloseElement();

                _ = JSRuntime.InvokeVoidAsync("mcomponents.scrollToSelectedEntry");
            }

            pBuilder.CloseElement(); //span
        }

        public void RegisterOption(MSelectOption pOption)
        {
            if (mAdditionalOptions.Any(o => o.Identifier == pOption.Identifier))
                return;

            mAdditionalOptions.Add(pOption);
            StateHasChanged();
        }

        protected void OnComboboxClicked(MouseEventArgs args)
        {
            if (DateTime.UtcNow.Subtract(mBlockUntil).TotalMilliseconds <= 0)
                return;

            mBlockUntil = DateTime.UtcNow.AddMilliseconds(300);
            _ = ToggleOptions(true);
        }

        protected void OnFocusIn(FocusEventArgs args)
        {
            if (DateTime.UtcNow.Subtract(mBlockUntil).TotalMilliseconds <= 0)
                return;

            mBlockUntil = DateTime.UtcNow.AddMilliseconds(300);
            _ = ToggleOptions(true);
        }

        protected void OnOptionSelect(int pIndex)
        {
            UpdateSelection(DisplayValues.ElementAt(pIndex));

            if (!mMultipleSelectMode)
                _ = ToggleOptions(true);
        }

        protected async Task ToggleOptions(bool pUserInteracted)
        {
            if (mOptionsVisible)
            {
                HideOptions(pUserInteracted);
            }
            else
            {
                await ShowOptions();
            }
        }

        public async Task ShowOptions()
        {
            if (mOptionsVisible || IsDisabled)
                return;

            if (Options != null)
                DisplayValues = Options.ToArray();
            SelectedValue = CurrentValue;

            mOptionsVisible = true;

            await JSRuntime.InvokeVoidAsync("mcomponents.registerMSelect", mObjReference);
            _ = InvokeAsync(() => StateHasChanged());
        }

        public void HideOptions(bool pUserInteracted)
        {
            mOptionsVisible = false;
            InvokeAsync(() => StateHasChanged());

            _ = JSRuntime.InvokeVoidAsync("mcomponents.unRegisterMSelect", mObjReference);

            if (pUserInteracted)
            {
                if (SelectSpan.Id != null)
                {
                    mBlockUntil = DateTime.UtcNow.AddMilliseconds(300);
                    _ = JSRuntime.InvokeVoidAsync("mcomponents.focusElement", SelectSpan);
                }
            }
        }

        protected void OnSearchInputChanged(ChangeEventArgs args)
        {
            InputValue = (string)args.Value;

            if (InputValue == string.Empty)
            {
                DisplayValues = Options.ToArray();
            }
            else
            {
                InputValue = InputValue.ToLower();
                DisplayValues = Options.Where(v => FormatValueAsString(v).ToLower().Contains(InputValue)).ToArray();
            }

            StateHasChanged();
            _ = JSRuntime.InvokeVoidAsync("mcomponents.scrollToSelectedEntry");
        }

        protected void InputKeyDown(KeyboardEventArgs args)
        {
            if (!mOptionsVisible)
            {
                if (args.Key == "Enter" || args.Key == " ")
                    _ = ShowOptions();

                return;
            }

            if (args.Key == "Escape" || (args.Key == "Backspace" && InputValue.Length == 0))
            {
                HideOptions(true);
                return;
            }

            if (args.Key == "Enter")
            {
                if (SelectedValue == null || DisplayValues.Count() == 1 || !DisplayValues.Contains(SelectedValue))
                {
                    UpdateSelection(DisplayValues.FirstOrDefault());
                    HideOptions(true);
                    return;
                }

                if (!mMultipleSelectMode)
                    UpdateSelection(SelectedValue);

                HideOptions(true);
                return;
            }

            if (args.Key == "ArrowDown")
            {
                int index = Array.IndexOf(DisplayValues, SelectedValue);

                if (index < 0)
                    index = -1;

                index++;

                if (index >= DisplayValues.Count())
                    return;

                SelectValue(DisplayValues.ElementAt(index));
                return;
            }

            if (args.Key == "ArrowUp")
            {
                int index = Array.IndexOf(DisplayValues, SelectedValue);

                if (index < 0)
                    index = 1;

                index--;

                if (index < 0)
                    return;

                SelectValue(DisplayValues.ElementAt(index));
                return;
            }

            if (args.Key == " ")
            {
                if (mMultipleSelectMode && !EnableSearch)
                {
                    int index = Array.IndexOf(DisplayValues, SelectedValue);
                    if (index >= 0)
                        OnOptionSelect(index);
                    return;
                }

                return;
            }

            if (args.Key == "End")
            {
                SelectValue(DisplayValues.LastOrDefault());
                return;
            }

            if (args.Key == "Home")
            {
                SelectValue(DisplayValues.FirstOrDefault());
                return;
            }

            if (args.Key == "PageDown")
            {
                int index = Array.IndexOf(DisplayValues, SelectedValue);
                if (index < 0)
                    return;

                index += 8;

                if (index > DisplayValues.Length - 1)
                    return;

                SelectValue(DisplayValues[index]);
            }

            if (args.Key == "PageUp")
            {
                int index = Array.IndexOf(DisplayValues, SelectedValue);
                if (index < 0)
                    return;

                index -= 8;

                if (index < 0)
                    return;

                SelectValue(DisplayValues[index]);
            }

            if (args.Key == "Tab")
            {
                HideOptions(false);
            }
        }

        protected void ToggleSelectAll()
        {
            if (mAllEntriesSelected)
            {
                foreach (var val in DisplayValues)
                {
                    if (Values.Contains(val))
                    {
                        Values.Remove(val);
                    }
                }

                mAllEntriesSelected = false;
            }
            else
            {
                foreach (var val in DisplayValues)
                {
                    if (!Values.Contains(val))
                    {
                        Values.Add(val);
                    }
                }

                mAllEntriesSelected = true;
            }

            UpdateDescription();
            ValuesChanged.InvokeAsync(Values);
            EditContext.NotifyFieldChanged(FieldIdentifier);
        }

        override protected string FormatValueAsString(T value)
        {
            if (value == null)
                return NullValueDescription;

            if (CustomFormatter != null)
            {
                return CustomFormatter(value);
            }

            if (Property != null && mPropertyInfo != null)
            {
                return mPropertyInfo.GetValue(value)?.ToString();
            }

            if (mtType.IsEnum)
            {
                var evalue = value as Enum;

                if (mEnumFlags && ((int)(object)evalue) != 0)
                {
                    var values = Options.Cast<Enum>().Where(v => evalue.HasFlag(v)).Select(v => v.ToName());
                    return string.Join(", ", values);
                }

                return evalue.ToName();
            }

            if (mtType == typeof(bool))
            {
                return (bool)(object)value ? L["True"] : L["False"];
            }

            return value.ToString();
        }

        private void UpdateSelection(T pSelectedValue)
        {
            if (mMultipleSelectMode && !mEnumFlags)
            {
                if (Values.Contains(pSelectedValue))
                {
                    Values.Remove(pSelectedValue);
                    mAllEntriesSelected = false;
                }
                else
                {
                    Values.Add(pSelectedValue);

                    if (DisplayValues.Length == Values.Count)
                    {
                        mAllEntriesSelected = true;
                    }
                }

                UpdateDescription();
                ValuesChanged.InvokeAsync(Values);
                EditContext.NotifyFieldChanged(FieldIdentifier);
            }
            else
            {
                if ((pSelectedValue == null && CurrentValue != null) || (CurrentValue == null && pSelectedValue != null) || (pSelectedValue != null && !pSelectedValue.Equals(CurrentValue)) || mEnumFlags)
                {
                    var oldValue = CurrentValue;

                    if (mEnumFlags)
                    {
                        if (ValueEnumHasFlag(pSelectedValue))
                        {
                            if (mIsNullableType)
                            {
                                CurrentValue = (T)ReflectionHelper.ChangeType((int)(object)CurrentValue & ~(int)(object)pSelectedValue, mtType);
                            }
                            else
                            {
                                CurrentValue = (T)(object)((int)(object)CurrentValue & ~(int)(object)pSelectedValue);
                            }
                        }
                        else
                        {
                            if (mIsNullableType)
                            {
                                if (CurrentValue == null)
                                {
                                    CurrentValue = pSelectedValue;
                                }
                                else
                                {
                                    CurrentValue = (T)ReflectionHelper.ChangeType((int)(object)CurrentValue | (int)(object)pSelectedValue, mtType);
                                }
                            }
                            else
                            {
                                CurrentValue = (T)(object)((int)(object)CurrentValue | (int)(object)pSelectedValue);
                            }
                        }
                    }
                    else
                    {
                        CurrentValue = pSelectedValue;
                    }

                    var args = new SelectionChangedArgs<T>()
                    {
                        NewValue = CurrentValue,
                        OldValue = oldValue
                    };

                    OnSelectionChanged.InvokeAsync(args);
                }
            }

            StateHasChanged();
        }

        protected void UpdateDescription()
        {
            if (mEnumFlags)
            {
                mNullValueOverride = FormatValueAsString(Value);
            }
            else if (ValuesExpression == null)
            {
                mNullValueOverride = null;
            }
            else if (Values != null)
            {
                if (Values.Count >= 3)
                {
                    mNullValueOverride = string.Format(L["{0} items selected"], Values.Count);
                }
                else
                {
                    mNullValueOverride = string.Join(", ", Values.Select(v => FormatValueAsString(v)));
                }
            }

            if (mNullValueOverride == string.Empty)
                mNullValueOverride = null;
        }

        protected bool ValueEnumHasFlag(object pFlag)
        {
            if (Value == null)
                return false;

            return ((Enum)(object)Value).HasFlag((Enum)pFlag);
        }

        public void SelectValue(T pValue)
        {
            SelectedValue = pValue;
            JSRuntime.InvokeVoidAsync("mcomponents.scrollToSelectedEntry");
            StateHasChanged();
        }

        public void Refresh()
        {
            UpdateDescription();
            StateHasChanged();
        }
    }
}
