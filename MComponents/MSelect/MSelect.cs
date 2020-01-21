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

namespace MComponents.MSelect
{
    public class MSelect<T> : InputSelect<T>, IMSelect
    {

        [Inject]
        private IJSRuntime JSRuntime { get; set; }

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

        private string mNullValueDescription = "Bitte auswählen...";

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


        protected ElementReference ComboBox { get; set; }
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
            pBuilder.AddAttribute(1, "class", "select2 select2-container select2-container--default form-control " + CssClass + (IsDisabled ? " select2-container--disabled" : string.Empty));
            pBuilder.AddAttribute(2, "dir", "ltr");
            pBuilder.AddAttribute(3, "style", "height: unset; padding: 0;");

            if (AdditionalAttributes != null)
                pBuilder.AddMultipleAttributes(4, AdditionalAttributes.Where(a => a.Key.ToLower() != "class"));

            pBuilder.OpenElement(5, "span");
            pBuilder.AddAttribute(6, "class", "selection");

            pBuilder.OpenElement(8, "span");
            pBuilder.AddAttribute(9, "onclick", EventCallback.Factory.Create<MouseEventArgs>(this, OnComboboxClicked));
            pBuilder.AddAttribute(10, "class", "select2-selection select2-selection--single");
            pBuilder.AddAttribute(3, "style", "border: unset;");

            pBuilder.AddAttribute(11, "role", "combobox");
            pBuilder.AddAttribute(12, "aria-haspopup", "true");
            pBuilder.AddAttribute(13, "aria-expanded", "false");
            pBuilder.AddAttribute(14, "tabindex", "0");
            pBuilder.AddAttribute(15, "aria-disabled", "false");
            pBuilder.AddAttribute(16, "aria-labelledby", "select2-kt_select2_1-container");
            pBuilder.AddElementReferenceCapture(17, (__value) =>
            {
                ComboBox = __value;
            });

            pBuilder.OpenElement(19, "span");
            pBuilder.AddAttribute(20, "class", "select2-selection__rendered");
            pBuilder.AddAttribute(21, "role", "textbox");
            pBuilder.AddAttribute(22, "aria-readonly", "true");

            pBuilder.AddContent(24,
                  CurrentValueAsString == null || CurrentValueAsString == string.Empty ? NullValueDescription : CurrentValueAsString
            );

            pBuilder.CloseElement();

            pBuilder.AddMarkupContent(27, "<span class=\"fa select2-selection__arrow--fa\" role=\"presentation\"><b role=\"presentation\"></b></span>");
            pBuilder.CloseElement();

            pBuilder.CloseElement();
            pBuilder.AddMarkupContent(29, "<span class=\"dropdown-wrapper\" aria-hidden=\"true\"></span>\r\n");
            pBuilder.CloseElement();


            if (mOptionsVisible)
            {
                pBuilder.OpenElement(32, "div");
                pBuilder.AddAttribute(33, "tabindex", "0");
                pBuilder.AddAttribute(34, "onfocusout", EventCallback.Factory.Create<FocusEventArgs>(this, OnFocusLost));

                pBuilder.AddAttribute(35, "class", "select2-container select2-container--default select2-container--open");
                pBuilder.AddElementReferenceCapture(36, (__value) =>
                {
                    OptionsDiv = __value;
                });

                pBuilder.OpenElement(38, "div");
                pBuilder.AddAttribute(39, "class", "select2-dropdown select2-dropdown--below");
                pBuilder.AddAttribute(40, "dir", "ltr");

                if (EnableSearch)
                {
                    pBuilder.OpenElement(43, "span");
                    pBuilder.AddAttribute(44, "class", "select2-search select2-search--dropdown");
                    pBuilder.OpenElement(46, "input");
                    pBuilder.AddAttribute(47, "oninput", EventCallback.Factory.Create<ChangeEventArgs>(this, OnSearchInputChanged));
                    pBuilder.AddAttribute(48, "onkeydown", EventCallback.Factory.Create<KeyboardEventArgs>(this, OnSearchInputKeyDown));
                    pBuilder.AddAttribute(49, "value", "");
                    pBuilder.AddAttribute(50, "class", "select2-search__field");
                    pBuilder.AddAttribute(51, "type", "search");
                    pBuilder.AddAttribute(52, "tabindex", "0");
                    pBuilder.AddAttribute(53, "autocomplete", "off");
                    pBuilder.AddAttribute(54, "autocorrect", "off");
                    pBuilder.AddAttribute(55, "autocapitalize", "none");
                    pBuilder.AddAttribute(56, "spellcheck", "false");
                    pBuilder.AddAttribute(57, "role", "search");
                    pBuilder.AddAttribute(58, "aria-autocomplete", "list");
                    pBuilder.AddAttribute(59, "aria-controls", "select2-kt_select2_1-results");
                    pBuilder.AddAttribute(60, "aria-activedescendant", "select2-kt_select2_1-result-thse-OR");
                    pBuilder.AddElementReferenceCapture(61, (__value) =>
                    {
                        SearchInput = __value;
                    });
                    pBuilder.CloseElement();
                    pBuilder.CloseElement();

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


                pBuilder.OpenElement(65, "div");
                pBuilder.AddAttribute(66, "class", "select2-results");

                pBuilder.OpenElement(68, "ul");
                pBuilder.AddAttribute(69, "class", "select2-results__options webkitscrollbar");
                pBuilder.AddAttribute(70, "role", "listbox");
                pBuilder.AddAttribute(71, "id", "select2-kt_select2_1-results");
                pBuilder.AddAttribute(72, "aria-expanded", "true");
                pBuilder.AddAttribute(73, "aria-hidden", "false");


                int i = 0;

                foreach (var entry in DisplayValues)
                {
                    int index = i;

                    bool isSelected = entry != null && entry.Equals(SelectedValue);

                    pBuilder.OpenElement(76, "li");
                    pBuilder.AddAttribute(77, "class", "select2-results__option clickable" + (isSelected ? " select2-results__option--highlighted" : string.Empty));
                    pBuilder.AddAttribute(78, "role", "option");
                    pBuilder.AddAttribute(79, "aria-selected", isSelected);

                    if (MultipleSelectMode)
                    {
                        pBuilder.OpenElement(76, "label");
                        pBuilder.AddAttribute(78, "class", "kt-checkbox");

                        pBuilder.OpenElement(76, "input");
                        pBuilder.AddAttribute(78, "type", "checkbox");
                        pBuilder.AddAttribute(80, "onchange", EventCallback.Factory.Create<ChangeEventArgs>(this, () => OnOptionSelect(index)));

                        if (Values.Contains(entry))
                        {
                            pBuilder.AddAttribute(78, "checked", "checked");
                        }

                        pBuilder.CloseElement(); //input

                        pBuilder.AddContent(81, FormatValueAsString(entry));

                        pBuilder.OpenElement(76, "span");
                        pBuilder.CloseElement(); //spawn

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
                    pBuilder.OpenElement(76, "li");
                    pBuilder.AddAttribute(77, "class", "select2-results__option" + " " + (entry != null && entry.Equals(SelectedValue) ? " select2-results__option--highlighted" : string.Empty));
                    pBuilder.AddAttribute(78, "role", "option");
                    pBuilder.AddAttribute(79, "aria-selected", "false");
                    pBuilder.AddMultipleAttributes(80, entry.AdditionalAttributes);

                    pBuilder.AddContent(81, entry.Value.ToString());
                    pBuilder.CloseElement();
                }

                pBuilder.CloseElement();
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

        protected string FormatDouble(double pValue)
        {
            return pValue.ToString().Replace(',', '.');
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
                if (DisplayValues.Count() == 1)
                {
                    UpdateSelection(DisplayValues.First());
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
            if (!MultipleSelectMode)
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
            else
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
        }

        protected void UpdateDescription()
        {
            mNullValueOverride = string.Join(", ", Values.Select(v => FormatValueAsString(v)));

            if (mNullValueOverride == string.Empty)
                mNullValueOverride = null;
        }

    }
}
