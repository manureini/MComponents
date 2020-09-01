using MComponents.MForm;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MComponents
{
    public interface IMForm
    {
        EventCallback<MFormSubmitArgs> OnValidSubmit { get; set; }

        bool HasUnsavedChanges { get; }

        void RegisterField(IMField pField);

        void OnInputKeyUp(KeyboardEventArgs pArgs);

        Task OnInputValueChanged(IMField pField, IMPropertyInfo pPropertyInfo,  object pNewValue);

        bool IsInTableRow { get; }

        bool EnableValidation { get; }

        IReadOnlyDictionary<string, object> AdditionalAttributes { get; set; }

        public Type ModelType { get; }
    }
}