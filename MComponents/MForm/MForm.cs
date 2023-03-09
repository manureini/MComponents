using MComponents.Services;
using MComponents.Shared.Attributes;
using MComponents.Shared.Localization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Web;

namespace MComponents.MForm
{
    public class MForm<T> : ComponentBase, IMForm
    {
        protected EditContext mEditContext;

        [CascadingParameter]
        MFormContainerContext ContainerContext { get; set; }

        [Parameter(CaptureUnmatchedValues = true)]
        public IReadOnlyDictionary<string, object> AdditionalAttributes { get; set; }

        [Parameter]
        public T Model { get; set; }

        [Parameter]
        public bool IsInTableRow { get; set; }

        [Parameter]
        public MFormGridContext MFormGridContext { get; set; }

        [Parameter]
        public bool EnableValidation { get; set; } = true;

        [Parameter]
        public bool EnableValidationSummary { get; set; } = true;

        [Parameter]
        public RenderFragment Fields { get; set; }

        [Parameter]
        public EventCallback<MFormSubmitArgs> OnValidSubmit { get; set; }

        [Parameter]
        public EventCallback<MFormValueChangedArgs<T>> OnValueChanged { get; set; }

        [Parameter]
        public bool PreventDefaultRendering { get; set; }

        [Parameter]
        public bool UpdateOnInput { get; set; }

        [Inject]
        public IStringLocalizer L { get; set; }

        [Inject]
        public MComponentSettings Settings { get; set; }

        public Type ModelType => Model?.GetType() ?? typeof(T);

        protected HashSet<IMPropertyInfo> ChangedValues { get; set; } = new HashSet<IMPropertyInfo>();

        protected ValidationMessageStore mValidationMessageStore;

        public List<IMField> FieldList = new List<IMField>();
        public List<MFieldRow> RowList = new List<MFieldRow>();

        [Parameter]
        public Guid Id { get; set; } = Guid.NewGuid();

        protected override void OnInitialized()
        {
            base.OnInitialized();

            mEditContext = new EditContext(Model);

            if (EnableValidation)
            {
                mValidationMessageStore = new ValidationMessageStore(mEditContext);

                mEditContext.OnValidationRequested += MEditContext_OnValidationRequested;
                mEditContext.OnFieldChanged += MEditContext_OnFieldChanged;
            }

            if (ContainerContext != null)
            {
                ContainerContext.RegisterForm(this);
                ContainerContext.OnFormSubmit += CascadedFormContext_OnFormSubmit;
            }
        }

        protected override void OnParametersSet()
        {
            if (IsInTableRow)
                ContainerContext = null;
        }

        private void NotifyContainer()
        {
            ContainerContext?.NotifySubmit(L);
        }

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            base.BuildRenderTree(builder);

            // If _fixedEditContext changes, tear down and recreate all descendants.
            // This is so we can safely use the IsFixed optimization on Cascadi ngValue,
            // optimizing for the common case where _fixedEditContext never changes.
            builder.OpenRegion(mEditContext.GetHashCode());

            if (!IsInTableRow)
            {
                builder.OpenElement(112, "div");
                builder.AddAttribute(113, "data-form-id", Id.ToString());

                string defaultCssClass = "m-form-container";

                if (EnableValidation)
                {
                    defaultCssClass += " m-form-validation";
                }

                builder.AddAttribute(11, "class", RenderHelper.GetCssClass(AdditionalAttributes, defaultCssClass));

                if (AdditionalAttributes != null)
                {
                    builder.AddMultipleAttributes(1, AdditionalAttributes.Where(a => a.Key != "class"));
                }

                builder.OpenElement(0, "form");
                builder.AddAttribute(1, "id", Id.ToString());
                builder.AddMultipleAttributes(1, AdditionalAttributes);
                builder.AddAttribute(2, "onsubmit", EventCallback.Factory.Create(this, NotifyContainer));

                if (EnableValidation)
                    builder.AddAttribute(11, "class", "m-form-validation");

                builder.CloseElement();
            }


            RenderFragment child(EditContext context) =>
                    (builder2) =>
                    {
                        if (Fields != null)
                        {
                            RenderFragment child3() =>
                                    (builder3) =>
                                    {
                                        builder3.AddMarkupContent(1, "\r\n");
                                        builder3.AddContent(2, this.Fields);
                                        builder3.AddMarkupContent(3, "\r\n");
                                    };

                            builder2.OpenComponent<CascadingValue<MForm<T>>>(4);
                            builder2.AddAttribute(5, "Value", this);
                            builder2.AddAttribute(6, "ChildContent", child3());
                            builder2.CloseComponent();
                        }

                        if (EnableValidation)
                        {
                            builder2.OpenComponent<DataAnnotationsValidator>(0);
                            builder2.CloseComponent();
                        }

                        if (EnableValidationSummary && !IsInTableRow)
                        {
                            builder2.OpenComponent<ValidationSummary>(1);
                            builder2.CloseComponent();
                        }

                        if (FieldList.Any())
                        {
                            foreach (var groupResult in GroupByRow(FieldList))
                            {
                                Process(builder2, groupResult);
                            }
                        }
                        else
                        {
                            if (!PreventDefaultRendering)
                            {
                                foreach (var groupResult in GroupByRow(ReflectionHelper.GetProperties(Model).Select(pi => GetField(pi))))
                                {
                                    //    Console.WriteLine(property.PropertyType.FullName);

                                    Process(builder2, groupResult);
                                }
                            }
                        }

                        if (!IsInTableRow)
                            builder2.AddMarkupContent(27, $"<button form=\"{Id}\" type=\"submit\" style=\"display: none;\">Submit</button>\r\n");
                    };

            builder.OpenComponent<CascadingValue<EditContext>>(3);
            builder.AddAttribute(4, "IsFixed", true);
            builder.AddAttribute(5, "Value", mEditContext);
            builder.AddAttribute(6, "ChildContent", child(mEditContext));
            builder.CloseComponent();

            if (!IsInTableRow)
            {
                builder.CloseComponent(); //div
            }

            builder.CloseRegion();
        }

        private void MEditContext_OnFieldChanged(object sender, FieldChangedEventArgs e)
        {
            var field = FieldList.OfType<IMPropertyField>().FirstOrDefault(f => f.Property == e.FieldIdentifier.FieldName);

            if (field == null)
                return;

            ValidateField(field);
            mEditContext.NotifyValidationStateChanged();
        }

        private void MEditContext_OnValidationRequested(object sender, ValidationRequestedEventArgs e)
        {
            mValidationMessageStore.Clear();

            foreach (var field in FieldList.OfType<IMPropertyField>())
            {
                ValidateField(field);
            }

            mEditContext.NotifyValidationStateChanged();
        }

        private void ValidateField(IMPropertyField pField)
        {
            var propInfo = GetPropertyInfo(pField);

            if (propInfo.IsReadOnly)
                return;

            PropertyInfo oriPropInfo = null;

            var isExpando = typeof(IDictionary<string, object>).IsAssignableFrom(ModelType);

            if (!isExpando)
                oriPropInfo = ModelType.GetProperty(propInfo.Name);

            var fieldIdentifier = mEditContext.Field(propInfo.Name);

            bool messagesCleared = false;

            //if attribute is passed via field and not handled by DataAnnotationsValidator
            foreach (ValidationAttribute attribute in propInfo.GetAttributes().Where(a => a is ValidationAttribute))
            {
                if ((oriPropInfo != null && oriPropInfo.GetCustomAttribute(attribute.GetType()) == null) || isExpando)
                {
                    if (!messagesCleared)
                    {
                        mValidationMessageStore.Clear(fieldIdentifier);
                        messagesCleared = true;
                    }

                    var value = propInfo.GetValue(Model);

                    if (!attribute.IsValid(value))
                    {
                        string displayname = propInfo.GetDisplayName(L, false);
                        var msg = attribute.FormatErrorMessage(displayname);
                        mValidationMessageStore.Add(fieldIdentifier, msg);
                    }
                }
            }
        }

        private IEnumerable<IGrouping<int, IMField>> GroupByRow(IEnumerable<IMField> pFields)
        {
            return pFields.GroupBy(p =>
            {
                if (IsInTableRow)
                    return 0;

                if (p.FieldRow != null)
                {
                    return RowList.IndexOf(p.FieldRow);
                }

                var rowAttr = p.Attributes?.FirstOrDefault(a => a.GetType() == typeof(RowAttribute)) as RowAttribute;

                if (rowAttr == null)
                    return 0;

                return rowAttr.RowId;
            }).OrderByDescending(g => g.Key).Reverse();
        }

        protected IMPropertyInfo GetPropertyInfo(IMPropertyField pField)
        {
            if (pField.Property == null)
            {
                return new MEmptyPropertyInfo()
                {
                    PropertyType = pField.PropertyType
                };
            }

            var pi = ReflectionHelper.GetIMPropertyInfo(Model.GetType(), pField.Property, pField.PropertyType);

            if (pi.PropertyType == null)
            {
                pi.PropertyType = pField.PropertyType ?? throw new InvalidOperationException($"Could not find type for {pField.Property}. Please specify it");
            }

            if (pi is MPropertyExpandoInfo ei)
            {
                ei.Attributes = pField.Attributes;
            }

            if (pField.Attributes != null)
                pi.SetAttributes(pField.Attributes);

            return pi;
        }

        protected IMPropertyField GetField(IMPropertyInfo pPropertyInfo)
        {
            return new MField()
            {
#pragma warning disable BL0005 // Component parameter should not be set outside of its component.

                Attributes = pPropertyInfo.GetAttributes()?.ToArray(),
                Property = pPropertyInfo.Name,
                PropertyType = pPropertyInfo.PropertyType

#pragma warning restore BL0005 // Component parameter should not be set outside of its component.
            };
        }

        protected void Process(RenderTreeBuilder builder2, IGrouping<int, IMField> groupResult)
        {
            if (groupResult.Key != 0 && !IsInTableRow)
            {
                builder2.OpenElement(10, "div");
                builder2.AddAttribute(11, "class", "m-form-row" + (groupResult.Count() > 1 ? " multiple-forms-in-row" : string.Empty));
            }

            foreach (var field in groupResult)
            {
                string cssClass = "form-group col-"; //TODO we use bootstrap here - good idea or bad?

                if (groupResult.Key == 0)
                {
                    cssClass += "12";
                }
                else
                {
                    cssClass += 12 / groupResult.Count();
                }

                if (field is IMPropertyField propField)
                {
                    var propertyInfo = GetPropertyInfo(propField);

                    if (propertyInfo.GetCustomAttribute<HiddenAttribute>() != null)
                        continue;

                    var inpId = Guid.NewGuid();

                    if (IsInTableRow)
                    {
                        builder2.OpenElement(16, "td");
                        builder2.AddAttribute(281, "data-is-in-table-row");

                        if (field.AdditionalAttributes != null && field.AdditionalAttributes.TryGetValue(Extensions.MFORM_IN_TABLE_ROW_TD_STYLE_ATTRIBUTE, out object value))
                        {
                            builder2.AddAttribute(247, "style", value);
                        }

                        if (propertyInfo.PropertyType != null)
                        {
                            AddInput(builder2, field, propertyInfo, inpId);
                        }

                        builder2.CloseElement();
                        continue;
                    }

                    if (groupResult.Key == 0)
                    {
                        builder2.OpenElement(10, "div");
                        builder2.AddAttribute(11, "class", "m-form-row");
                    }

                    //  <div class="form-group">
                    builder2.OpenElement(10, "div");
                    builder2.AddAttribute(272, "class", cssClass);

                    //  <label for="@inpId">@property.Name</label>
                    builder2.OpenElement(275, "label");
                    builder2.AddAttribute(276, "for", inpId);
                    builder2.AddAttribute(277, "class", "col-sm-12 col-form-label"); //TODO we use bootstrap here - good idea or bad?

                    builder2.AddMarkupContent(286, propertyInfo.GetDisplayName(L, true));

                    if (propertyInfo.GetCustomAttribute<RequiredAttribute>() != null)
                    {
                        builder2.OpenElement(291, "span");
                        builder2.AddAttribute(292, "style", "color: red;");
                        builder2.AddContent(293, " *");
                        builder2.CloseElement();
                    }

                    builder2.CloseElement();

                    if (propField.TemplateAfterLabel != null)
                    {
                        builder2.AddContent(294, propField.TemplateAfterLabel);
                    }

                    builder2.OpenElement(296, "div");
                    builder2.AddAttribute(297, "class", "col-sm-12");  //TODO we use bootstrap here - good idea or bad?

                    AddInput(builder2, field, propertyInfo, inpId);

                    builder2.CloseElement(); // </div>


                    builder2.CloseElement(); // </div>

                    if (groupResult.Key == 0)
                    {
                        builder2.CloseElement(); // </div>
                    }
                }
                else if (field is IMFieldGenerator fieldGenerator)
                {
                    MFieldGeneratorContext context = new MFieldGeneratorContext()
                    {
                        Form = this
                    };

                    if (IsInTableRow)
                    {
                        builder2.OpenElement(16, "td");
                    }

                    //  <div class="form-group">
                    builder2.OpenElement(10, "div");
                    builder2.AddAttribute(272, "class", cssClass);

                    builder2.AddContent(42, fieldGenerator.Template?.Invoke(context));

                    builder2.CloseElement();

                    if (IsInTableRow)
                    {
                        builder2.CloseElement();
                    }
                }
                else if (field is MFieldComponent mfieldComponent)
                {
                    builder2.OpenComponent(336, mfieldComponent.CompnentType);

                    if (mfieldComponent.AdditionalAttributes != null)
                    {
                        builder2.AddMultipleAttributes(339, mfieldComponent.AdditionalAttributes);
                    }

                    builder2.CloseComponent();
                }
            }

            if (groupResult.Key != 0 && !IsInTableRow)
            {
                builder2.CloseElement();
            }
        }

        private void AddInput(RenderTreeBuilder builder2, IMField field, IMPropertyInfo propertyInfo, Guid inpId)
        {
            if (field is IMPropertyField pf)
            {
                if (field is IMComplexField)
                {
                    var appendMethod = typeof(RenderHelper).GetMethod(nameof(RenderHelper.AppendComplexType)).MakeGenericMethod(typeof(T), pf.PropertyType);
                    appendMethod.Invoke(null, new object[] { builder2, propertyInfo, Model, inpId, this, field, MFormGridContext });
                    return;
                }

                bool isInFilterRow = AdditionalAttributes != null && AdditionalAttributes.ContainsKey("data-is-filterrow");

                if (!isInFilterRow && propertyInfo.GetCustomAttribute<LocalizedStringAttribute>() != null)
                {
                    var lmethod = typeof(RenderHelperInputLocalized).GetMethod(nameof(RenderHelperInputLocalized.AppendInput)).MakeGenericMethod(propertyInfo.PropertyType);
                    lmethod.Invoke(null, new object[] { builder2, propertyInfo, Model, this, field, UpdateOnInput, Settings.SupportedCultures });
                    return;
                }

                var method = typeof(RenderHelper).GetMethod(nameof(RenderHelper.AppendInput)).MakeGenericMethod(propertyInfo.PropertyType);
                method.Invoke(null, new object[] { builder2, propertyInfo, Model, inpId, this, isInFilterRow, field, UpdateOnInput });
            }
        }

        public bool HasUnsavedChanges
        {
            get { return ChangedValues.Count > 0; }
        }

        private async Task<bool> CascadedFormContext_OnFormSubmit(object sender, MFormContainerContextSubmitArgs e)
        {
            bool isValid;

            try
            {
                isValid = mEditContext.Validate(); // This will likely become ValidateAsync later
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return false;
            }

            if (!isValid)
            {
                Console.WriteLine(typeof(T) + ": Not valid");
                return false;
            }

            Dictionary<string, object> changedDict = new Dictionary<string, object>();

            if (HasUnsavedChanges)
            {
                foreach (var entry in ChangedValues)
                {
                    var fullname = entry.GetFullName();

                    if (changedDict.ContainsKey(fullname))
                        continue;

                    object value = entry.GetValue(Model);
                    changedDict.Add(fullname, value);
                }

                ChangedValues.Clear();
            }

            if (OnValidSubmit.HasDelegate)
            {
                await OnValidSubmit.InvokeAsync(new MFormSubmitArgs(mEditContext, changedDict, Model, e.UserInterated));
            }

            return true;
        }

        public Task<bool> CallLocalSubmit(bool pUserInteracted)
        {
            var args = new MFormContainerContextSubmitArgs()
            {
                UserInterated = pUserInteracted
            };

            return CascadedFormContext_OnFormSubmit(this, args);
        }

        public void RegisterField(IMField pField, bool pSkipRendering = false)
        {
            if (pField == null)
                return;

            FieldList.Add(pField);

            if (pSkipRendering)
                return;

            StateHasChanged();
        }

        public void UnregisterField(IMField pField)
        {
            FieldList.Remove(pField);
            StateHasChanged();
        }

        public void ClearFields()
        {
            FieldList.Clear();
            StateHasChanged();
        }

        public void OnInputKeyUp(KeyboardEventArgs pArgs, IMPropertyInfo pPropertyInfo)
        {
            if (pArgs.Key == "Enter" && pPropertyInfo.GetCustomAttribute<TextAreaAttribute>() == null)
            {
                if (ContainerContext == null)
                {
                    //value may not be updated
                    _ = Task.Delay(10).ContinueWith((a) =>
                     {
                         InvokeAsync(() => _ = CallLocalSubmit(true));
                     });
                }
            }
        }

        public async Task OnInputValueChanged(IMField pField, IMPropertyInfo pPropertyInfo, object pNewValue)
        {
            if (!ChangedValues.Contains(pPropertyInfo))
                ChangedValues.Add(pPropertyInfo);

            if (OnValueChanged.HasDelegate)
            {
                await OnValueChanged.InvokeAsync(new MFormValueChangedArgs<T>(pField, pPropertyInfo, pNewValue, Model));
            }

            if (mEditContext != null && pField is IMPropertyField propertyField)
            {
                mEditContext.NotifyFieldChanged(mEditContext.Field(propertyField.Property));
            }
        }

        public bool Validate()
        {
            return mEditContext.Validate();
        }

        public void InvokeStateHasChanged()
        {
            StateHasChanged();
        }

        public void RegisterRow(MFieldRow pRow)
        {
            RowList.Add(pRow);
            StateHasChanged();
        }
    }
}
