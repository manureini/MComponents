using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Microsoft.AspNetCore.Components.Web;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.AspNetCore.Components.Rendering;
using System.Linq.Expressions;
using Microsoft.Extensions.Localization;

namespace MComponents.MSelect
{
    public class MSelect<T> : InputSelect<T>, IMSelect
    {
        [Inject]
        private IJSRuntime JSRuntime { get; set; }

        [Inject]
        public IStringLocalizer<MComponentsLocalization> L { get; set; }

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
        public ICollection<T> Values { get; set; }

        [Parameter]
        public EventCallback<ICollection<T>> ValuesChanged { get; set; }

        [Parameter]
        public Expression<Func<ICollection<T>>> ValuesExpression { get; set; }

        protected ElementReference OptionsDiv { get; set; }

        protected bool mOptionsVisible = false;

        protected T[] DisplayValues = new T[0];

        protected T SelectedValue;

        protected ElementReference SearchInput { get; set; }

        protected string InputValue = string.Empty;

        protected IMPropertyInfo mPropertyInfo;

        protected List<MSelectOption> mAdditionalOptions = new List<MSelectOption>();

        protected Type mtType = Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T);

        protected bool MultipleSelectMode => ValuesExpression != null;


        public override async Task SetParametersAsync(ParameterView parameters)
        {
            //Workaround for bypass check is ValueExpression is set

            parameters.SetParameterProperties(this);

            if (MultipleSelectMode)
            {
                EditContext = CascadedEditContext2;
            }

            await base.SetParametersAsync(parameters);

            if (MultipleSelectMode)
            {
                FieldIdentifier = FieldIdentifier.Create(ValuesExpression);
            }

            mNullValueDescription = mNullValueDescription ?? L["Please select..."];
        }

        protected override void OnInitialized()
        {
            if (Values != null && Value != null)
                throw new ArgumentException($"Specify {nameof(Values)} or {nameof(Value)} through bind-value");

            if (Options == null && mtType.IsEnum)
            {
                Options = Enum.GetValues(mtType).Cast<T>();
            }

            if (Property != null)
            {
                mPropertyInfo = ReflectionHelper.GetIMPropertyInfo(typeof(T), Property, PropertyType);
            }

            if (Options != null)
            {
                DisplayValues = Options.ToArray();
            }

            if (MultipleSelectMode)
            {
                UpdateDescription();
            }

            base.OnInitialized();
        }

        protected override void BuildRenderTree(RenderTreeBuilder pBuilder)
        {
            if (this.ChildContent != null)
            {
                RenderFragment child() =>
                        (builder2) =>
                        {
                            builder2.AddContent(56, this.ChildContent);
                        };

                pBuilder.OpenComponent<CascadingValue<MSelect<T>>>(4);
                pBuilder.AddAttribute(5, "Value", this);
                pBuilder.AddAttribute(6, "ChildContent", child());
                pBuilder.CloseComponent();
            }

            pBuilder.OpenElement(0, "span");
            pBuilder.AddAttribute(9, "onclick", EventCallback.Factory.Create<MouseEventArgs>(this, OnComboboxClicked));
            pBuilder.AddAttribute(1, "class", "m-select m-form-control m-clickable " + CssClass + (IsDisabled ? " m-select--disabled" : string.Empty) + (mOptionsVisible ? " m-select--open" : string.Empty));

            if (AdditionalAttributes != null)
                pBuilder.AddMultipleAttributes(4, AdditionalAttributes.Where(a => a.Key.ToLower() != "class"));


            pBuilder.OpenElement(19, "span");
            pBuilder.AddAttribute(20, "class", "m-select-content");
            pBuilder.AddAttribute(21, "role", "textbox");

            pBuilder.AddContent(24,
                  CurrentValueAsString == null || CurrentValueAsString == string.Empty ? NullValueDescription : CurrentValueAsString
            );

            pBuilder.CloseElement();

            pBuilder.AddMarkupContent(27, "<span class=\"m-select-dropdown-icon fa fa-angle-down\" role =\"presentation\"></span>");

            pBuilder.CloseElement();

            if (mOptionsVisible)
            {
                pBuilder.OpenElement(32, "div");
                pBuilder.AddAttribute(33, "tabindex", "0");
                //             pBuilder.AddAttribute(34, "onfocusout", EventCallback.Factory.Create<FocusEventArgs>(this, OnFocusLost));

                pBuilder.AddAttribute(35, "class", "m-select-options-container");
                pBuilder.AddElementReferenceCapture(36, (__value) =>
                {
                    OptionsDiv = __value;
                });

                pBuilder.OpenElement(38, "div");
                pBuilder.AddAttribute(39, "class", "m-select-options-list-container");

                if (EnableSearch)
                {
                    pBuilder.OpenElement(43, "span");
                    pBuilder.AddAttribute(44, "class", "m-select-search-container");

                    pBuilder.OpenElement(46, "input");
                    pBuilder.AddAttribute(47, "oninput", EventCallback.Factory.Create<ChangeEventArgs>(this, OnSearchInputChanged));
                    pBuilder.AddAttribute(48, "onkeydown", EventCallback.Factory.Create<KeyboardEventArgs>(this, OnSearchInputKeyDown));
                    pBuilder.AddAttribute(49, "value", "");
                    pBuilder.AddAttribute(50, "class", "m-form-control m-select-search-input");
                    pBuilder.AddAttribute(51, "type", "search");
                    pBuilder.AddAttribute(52, "tabindex", "0");
                    pBuilder.AddAttribute(53, "autocomplete", "off");
                    pBuilder.AddAttribute(54, "autocorrect", "off");
                    pBuilder.AddAttribute(55, "autocapitalize", "none");
                    pBuilder.AddAttribute(56, "spellcheck", "false");
                    pBuilder.AddAttribute(57, "role", "search");

                    pBuilder.AddElementReferenceCapture(61, (__value) =>
                    {
                        SearchInput = __value;
                    });
                    pBuilder.CloseElement(); //input

                    pBuilder.CloseElement(); //span

                    Task.Run(async () =>
                    {
                        await Task.Delay(50);
                        if (SearchInput.Id != null)
                            await JSRuntime.InvokeVoidAsync("mcomponents.focusElement", SearchInput);
                    });
                }
                else
                {
                    Task.Run(async () =>
                    {
                        await Task.Delay(50);
                        if (OptionsDiv.Id != null)
                            await JSRuntime.InvokeVoidAsync("mcomponents.focusElement", OptionsDiv);
                    });
                }

                pBuilder.OpenElement(68, "ul");
                pBuilder.AddAttribute(69, "class", "m-select-options-list m-scrollbar");
                pBuilder.AddAttribute(70, "role", "listbox");

                int i = 0;

                foreach (var entry in DisplayValues)
                {
                    int index = i;

                    bool isSelected = entry != null && entry.Equals(SelectedValue);

                    pBuilder.OpenElement(76, "li");
                    pBuilder.AddAttribute(77, "class", "m-select-options-entry m-clickable" + (isSelected ? " m-select-options-entry--highlighted" : string.Empty));
                    pBuilder.AddAttribute(80, "onclick", EventCallback.Factory.Create<MouseEventArgs>(this, () => OnOptionSelect(index)));
                    pBuilder.AddAttribute(78, "role", "option");

                    if (MultipleSelectMode)
                    {
                        pBuilder.OpenElement(76, "label");
                        pBuilder.AddAttribute(78, "class", "m-checkbox m-select-checkbox m-clickable");
                        pBuilder.AddEventStopPropagationClicksAttribute(81);

                        pBuilder.OpenElement(76, "input");
                        pBuilder.AddAttribute(78, "type", "checkbox");

                        pBuilder.AddAttribute(80, "onchange", EventCallback.Factory.Create<ChangeEventArgs>(this, () => OnOptionSelect(index)));
                        pBuilder.AddEventStopPropagationClicksAttribute(81);

                        if (Values.Contains(entry))
                        {
                            pBuilder.AddAttribute(78, "checked", "checked");
                        }

                        pBuilder.CloseElement(); //input

                        pBuilder.OpenElement(81, "span"); //this is required for design magic
                        pBuilder.CloseElement();

                        pBuilder.AddContent(81, FormatValueAsString(entry));

                        pBuilder.CloseElement(); //label
                    }
                    else
                    {
                        pBuilder.AddAttribute(80, "onclick", EventCallback.Factory.Create<MouseEventArgs>(this, () => OnOptionSelect(index)));
                        pBuilder.AddContent(81, FormatValueAsString(entry));
                    }

                    pBuilder.CloseElement(); //li
                    i++;
                }

                foreach (var entry in mAdditionalOptions.Where(a => !a.ShowOnlyIfNoSearchResults || (i == 0 && a.ShowOnlyIfNoSearchResults)))
                {
                    bool isSelected = entry != null && entry.Equals(SelectedValue);

                    pBuilder.OpenElement(76, "li");
                    pBuilder.AddAttribute(77, "class", "m-select-options-entry m-clickable" + (isSelected ? " m-select-options-entry--highlighted" : string.Empty));
                    pBuilder.AddAttribute(78, "role", "option");
                    //        pBuilder.AddAttribute(79, "aria-selected", "false");

                    pBuilder.AddMultipleAttributes(80, entry.AdditionalAttributes);

                    pBuilder.AddContent(81, entry.Value.ToString());
                    pBuilder.CloseElement();
                }

                pBuilder.CloseElement();
                pBuilder.CloseElement();
                pBuilder.CloseElement();
            }
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
            ToggleOptions();
        }

        protected void OnOptionSelect(int pIndex)
        {
            UpdateSelection(DisplayValues.ElementAt(pIndex));

            if (!MultipleSelectMode)
                ToggleOptions();
        }

        protected async void OnFocusLost(FocusEventArgs pArgs)
        {
            await Task.Delay(100);

            if (mOptionsVisible)
                ToggleOptions();
        }

        protected void ToggleOptions()
        {
            if (IsDisabled)
                return;

            if (!mOptionsVisible)
            {
                if (Options != null)
                    DisplayValues = Options.ToArray();
                SelectedValue = CurrentValue;
            }

            mOptionsVisible = !mOptionsVisible;
            StateHasChanged();
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
        }

        protected void OnSearchInputKeyDown(KeyboardEventArgs args)
        {
            if (args.Key == "Escape" || (args.Key == "Backspace" && InputValue.Length == 0))
            {
                ToggleOptions();
                return;
            }

            if (args.Key == "Enter")
            {
                if (SelectedValue == null || DisplayValues.Count() == 1)
                {
                    UpdateSelection(DisplayValues.FirstOrDefault());
                    ToggleOptions();
                    return;
                }

                UpdateSelection(SelectedValue);
                ToggleOptions();
                return;
            }

            if (args.Key == "ArrowDown")
            {
                int index = Array.IndexOf(DisplayValues, SelectedValue);

                if (index < 0)
                {
                    index = -1;
                }

                index++;

                if (index >= DisplayValues.Count())
                    return;

                SelectedValue = DisplayValues.ElementAt(index);
                StateHasChanged();
                return;
            }

            if (args.Key == "ArrowUp")
            {
                int index = Array.IndexOf(DisplayValues, SelectedValue);

                if (index < 0)
                {
                    index = 1;
                }

                index--;

                if (index < 0)
                    return;

                SelectedValue = DisplayValues.ElementAt(index);
                StateHasChanged();
            }
        }

        override protected string FormatValueAsString(T value)
        {
            if (value == null)
                return NullValueDescription;

            if (Property != null)
            {
                return mPropertyInfo.GetValue(value)?.ToString();
            }

            if (mtType.IsEnum)
            {
                var evalue = value as Enum;
                return evalue.ToName();
            }

            return value.ToString();
        }

        private void UpdateSelection(T pSelectedValue)
        {
            if (MultipleSelectMode)
            {
                if (Values.Contains(pSelectedValue))
                {
                    Values.Remove(pSelectedValue);
                }
                else
                {
                    Values.Add(pSelectedValue);
                }

                UpdateDescription();

                ValuesChanged.InvokeAsync(Values);
                EditContext.NotifyFieldChanged(FieldIdentifier);
                StateHasChanged();
            }
            else
            {
                if ((pSelectedValue == null && CurrentValue != null) || (CurrentValue == null && pSelectedValue != null) || (pSelectedValue != null && !pSelectedValue.Equals(CurrentValue)))
                {
                    var oldValue = CurrentValue;
                    CurrentValue = pSelectedValue;

                    SelectionChangedArgs<T> args = new SelectionChangedArgs<T>()
                    {
                        NewValue = pSelectedValue,
                        OldValue = oldValue
                    };

                    OnSelectionChanged.InvokeAsync(args);
                }
            }
        }

        protected void UpdateDescription()
        {
            mNullValueOverride = string.Join(", ", Values.Select(v => FormatValueAsString(v)));

            if (mNullValueOverride == string.Empty)
                mNullValueOverride = null;
        }

    }
}
