using MComponents.InputElements;
using MComponents.MForm;
using MComponents.MGrid;
using MComponents.MSelect;
using MComponents.Shared.Attributes;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.CompilerServices;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace MComponents
{
    public static class RenderHelper
    {
        private static readonly Type[] mNumberTypes = { typeof(int), typeof(long), typeof(float), typeof(double), typeof(decimal) };

        private static readonly List<Type> mSupportedTypes = new List<Type>();

        static RenderHelper()
        {
            mSupportedTypes.AddRange(mNumberTypes);
            mSupportedTypes.Add(typeof(string));
            mSupportedTypes.Add(typeof(DateTime));
            mSupportedTypes.Add(typeof(bool));
            mSupportedTypes.Add(typeof(Guid));
            mSupportedTypes.Add(typeof(Type));
        }

        public static bool IsTypeSupported(Type pType)
        {
            if (pType.IsEnum)
                return true;

            Type nullableType = Nullable.GetUnderlyingType(pType);
            if (nullableType != null)
            {
                return IsTypeSupported(nullableType);
            }

            return mSupportedTypes.Contains(pType);
        }

        public static void AppendInput<T>(RenderTreeBuilder pBuilder, IMPropertyInfo pPropertyInfo, object pModel, Guid pId, IMForm pParent, bool pIsInFilterRow, IMField pField, bool pUpdateOnInput)
        {
            try
            {
                if (!IsTypeSupported(typeof(T)) || IsPropertyHolderNull(pPropertyInfo, pModel))
                {
                    ShowNotSupportedType(pBuilder, pPropertyInfo, pModel, pId);
                    return;
                }

                T value = default(T);
                var val = pPropertyInfo.GetValue(pModel);

                if (val != null)
                {
                    value = (T)ReflectionHelper.ChangeType(val, typeof(T));
                }

                Type tType = Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T);

                bool isReadOnly = pPropertyInfo.IsReadOnly || pPropertyInfo.GetCustomAttribute<ReadOnlyAttribute>() != null;

                var restrictValues = pPropertyInfo.GetCustomAttribute<RestrictValuesAttribute>();

                if (typeof(T) == typeof(bool?) || tType.IsEnum || restrictValues != null)
                {
                    pBuilder.OpenComponent<MSelect<T>>(0);
                    if (pIsInFilterRow)
                        pBuilder.AddAttribute(10, "NullValueDescription", "\u200b");
                }
                else if (mNumberTypes.Contains(tType))
                {
                    if (pUpdateOnInput)
                    {
                        pBuilder.OpenComponent<InputNumberOnInput<T>>(0);
                    }
                    else
                    {                       
                        pBuilder.OpenComponent<InputNumber<T>>(0);
                    }
                }
                else if (tType == typeof(DateTime) || tType == typeof(DateTimeOffset))
                {
                    if (pPropertyInfo.GetCustomAttribute<TimeAttribute>() != null)
                    {
                        pBuilder.OpenComponent<InputTime<T>>(0);
                    }
                    else if (pPropertyInfo.GetCustomAttribute<DateTimeAttribute>() != null)
                    {
                        pBuilder.OpenComponent<InputDateTime<T>>(0);
                    }
                    else
                    {
                        pBuilder.OpenComponent<InputDate<T>>(0);
                    }
                }
                else if (typeof(T) == typeof(bool))
                {
                    pBuilder.OpenComponent<MInputCheckbox>(0);
                }
                else if (tType == typeof(Guid))
                {
                    pBuilder.OpenComponent<InputGuid<T>>(0);
                }
                else if (tType == typeof(Type))
                {
                    pBuilder.OpenComponent<InputType>(0);
                }
                else
                {
                    if (pPropertyInfo.GetCustomAttribute<TextAreaAttribute>() != null)
                    {
                        pBuilder.OpenComponent<InputTextArea>(0);
                    }
                    else
                    {
                        pBuilder.OpenComponent<InputText>(0);
                    }
                }

                if (pPropertyInfo.GetCustomAttribute<PasswordAttribute>() != null)
                {
                    pBuilder.AddAttribute(33, "type", "password");
                }

                if (pField.AdditionalAttributes != null)
                    pBuilder.AddMultipleAttributes(17, pField.AdditionalAttributes
                        .Where(a => a.Key != Extensions.MFORM_IN_TABLE_ROW_TD_STYLE_ATTRIBUTE)
                        .Where(a => a.Key != nameof(IMGridColumn))
                        .ToDictionary(a => a.Key, a => a.Value));

          //      pBuilder.SetUpdatesAttributeName(pPropertyInfo.Name);

                pBuilder.AddAttribute(1, "id", pId);
                pBuilder.AddAttribute(2, "Value", value);

                pBuilder.AddAttribute(129, "form", pParent.Id);

                pBuilder.AddAttribute(23, "ValueChanged", RuntimeHelpers.CreateInferredEventCallback<T>(pParent, async __value =>
                {
                    await InvokeValueChanged(pParent, pPropertyInfo, pField, pModel, __value);
                }, value));

                if (pUpdateOnInput && !mNumberTypes.Contains(tType))
                {
                    pBuilder.AddAttribute(23, "oninput", EventCallback.Factory.Create<ChangeEventArgs>(pParent, async a =>
                    {
                        object value = a.Value;
                        value = ReflectionHelper.ChangeType(value, typeof(T)); //maybe the TryParseValueFromString method of the input is better?
                        await InvokeValueChanged(pParent, pPropertyInfo, pField, pModel, value);
                    }));
                }

                pBuilder.AddAttribute(23, "onkeyup", EventCallback.Factory.Create<KeyboardEventArgs>(pParent, (a) =>
                {
                    pParent.OnInputKeyUp(a, pPropertyInfo);
                }));

                var valueExpression = GetValueExpression<T>(pPropertyInfo, pModel);

                pBuilder.AddAttribute(4, "ValueExpression", valueExpression);

                string cssClass = "m-form-control";

                if (isReadOnly)
                {
                    pBuilder.AddAttribute(33, "disabled", string.Empty);
                    pBuilder.AddAttribute(33, "IsDisabled", true);
                }

                pBuilder.AddAttribute(10, "class", cssClass);
                // pBuilder.SetUpdatesAttributeName(pPropertyInfo.Name); <- new code generator will add this, but I don't know why

                if (restrictValues != null)
                {
                    foreach (var allowedValue in restrictValues.AllowedValues)
                    {
                        if (typeof(T) != typeof(string) && !typeof(T).IsAssignableFrom(allowedValue.GetType()))
                        {
                            throw new Exception($"Allowed value {allowedValue} does not implement property type {typeof(T).AssemblyQualifiedName}");
                        }
                    }

                    IEnumerable<T> options;

                    if (typeof(T) == typeof(string))
                    {
                        options = restrictValues.AllowedValues.Select(v => (T)(object)v.ToString()).ToArray();
                    }
                    else
                    {
                        options = restrictValues.AllowedValues.Cast<T>().ToArray();
                    }

                    pBuilder.AddAttribute(10, "Options", options);
                }
                else if (typeof(T) == typeof(bool?))
                {
                    IEnumerable<bool?> options = new bool?[] { true, false };
                    pBuilder.AddAttribute(10, "Options", options);
                }

                pBuilder.CloseComponent();

                if (pParent.EnableValidation)
                {
                    pBuilder.OpenComponent<ValidationMessage<T>>(60);
                    pBuilder.AddAttribute(61, "For", valueExpression);
                    pBuilder.CloseComponent();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                throw;
            }
        }

        private static async Task InvokeValueChanged(IMForm pParent, IMPropertyInfo pPropertyInfo, IMField pField, object pModel, object pNewValue)
        {
            if (pPropertyInfo.GetCustomAttribute<DateAttribute>() != null)
            {
                var dateTime = pNewValue as DateTime?;

                if (dateTime != null && dateTime.Value.Kind == DateTimeKind.Unspecified)
                {
                    pNewValue = DateTime.SpecifyKind(dateTime.Value, DateTimeKind.Utc);
                }
            }

            pPropertyInfo.SetValue(pModel, pNewValue);
            await pParent.OnInputValueChanged(pField, pPropertyInfo, pNewValue);
        }

        public static void AppendComplexType<T, TProperty>(RenderTreeBuilder pBuilder, IMPropertyInfo pPropertyInfo, T pModel, Guid pId, IMForm pParent, MComplexPropertyField<TProperty> pComplexField,
            MFormGridContext pGridContext)
        {
            if (pComplexField.Template == null)
            {
                ShowNotSupportedType(pBuilder, pPropertyInfo, pModel, pId);
                return;
            }

            TProperty value = (TProperty)pPropertyInfo.GetValue(pModel);

            var context = new MComplexPropertyFieldContext<TProperty>
            {
                Row = pModel,
                InputId = pId.ToString(),
                FormId = pParent.Id.ToString(),
                Form = pParent,
                Value = value,
                MFormGridContext = pGridContext,

                ValueChanged = RuntimeHelpers.CreateInferredEventCallback<TProperty>(pParent, async __value =>
                {
                    pPropertyInfo.SetValue(pModel, __value);
                    await pParent.OnInputValueChanged(pComplexField, pPropertyInfo, __value);
                }, value),

                ValueExpression = GetValueExpression<TProperty>(pPropertyInfo, pModel)
            };

            pBuilder.AddContent(263, pComplexField.Template?.Invoke(context));

            if (pParent.EnableValidation)
            {
                pBuilder.OpenComponent<ValidationMessage<TProperty>>(236);
                pBuilder.AddAttribute(237, "For", context.ValueExpression);
                pBuilder.CloseComponent();
            }
        }

        private static Expression<Func<T>> GetValueExpression<T>(IMPropertyInfo pPropertyInfo, object pModel)
        {
            if (pModel is IDictionary<string, object>)
            {
                var fake = new FakePropertyInfo<T>(pPropertyInfo.Name);

                //just create a member expression with random values
                MemberExpression expression = Expression.Property(Expression.Constant(fake), nameof(fake.CanRead));

                Expression constantExpression = Expression.Constant(default(T), typeof(T));

                var constantExpressionValueBaseFields = constantExpression.GetType().BaseType.GetFields(BindingFlags.Instance | BindingFlags.NonPublic);
                var constantExpressionValueFields = constantExpression.GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic);

                var field = constantExpressionValueBaseFields.Concat(constantExpressionValueFields).First(f => f.FieldType == typeof(object));
                field.SetValue(constantExpression, pModel);

                //set generated constant expression
                var expressionField = expression.GetType().BaseType.GetFields(BindingFlags.Instance | BindingFlags.NonPublic).First(f => f.FieldType == typeof(Expression));
                expressionField.SetValue(expression, constantExpression);

                //set fake property type
                var propertyField = expression.GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic).First(f => f.FieldType == typeof(PropertyInfo));
                propertyField.SetValue(expression, fake);

                //now we have generated an MemberExpression which has the pModel as value and an FakePropertyInfo with correct type

                return Expression.Lambda<Func<T>>(expression);
            }
            else
            {
                var propertyholder = pPropertyInfo.GetPropertyHolder(pModel);
                return Expression.Lambda<Func<T>>(Expression.Property(Expression.Constant(propertyholder), pPropertyInfo.Name));
            }
        }

        private static bool IsPropertyHolderNull(IMPropertyInfo pPropertyInfo, object pModel)
        {
            if (pModel is IDictionary<string, object>)
            {
                return false;
            }

            return pPropertyInfo.GetPropertyHolder(pModel) == null;
        }


        internal static void ShowNotSupportedType(RenderTreeBuilder pBuilder, IMPropertyInfo pPropertyInfo, object pModel, Guid pId)
        {
            var value = pPropertyInfo.GetValue(pModel);

            pBuilder.OpenElement(45, "input");
            pBuilder.AddAttribute(1, "id", pId);
            pBuilder.AddAttribute(2, "Value", value);
            pBuilder.AddAttribute(33, "disabled", string.Empty);
            pBuilder.AddAttribute(33, "class", "m-form-control");
            pBuilder.CloseElement();
        }

        public static string GetCssClass(IReadOnlyDictionary<string, object> pAdditionalAttributes, string pDefaultCssClass)
        {
            if (pAdditionalAttributes != null && pAdditionalAttributes.TryGetValue("class", out object additionalCssClass))
            {
                return additionalCssClass.ToString() + " " + pDefaultCssClass;
            }

            return pDefaultCssClass;
        }
    }
}
