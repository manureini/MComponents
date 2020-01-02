using MComponents.MForm;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using System.Collections.Generic;

namespace MComponents
{
    public interface IMForm
    {
        EventCallback<MFormSubmitArgs> OnValidSubmit { get; set; }

        bool HasUnsavedChanges { get; }

        void RegisterField(IMField pField);

        void OnInputKeyUp(KeyboardEventArgs pArgs);

        void OnInputValueChanged(string pProperty, object pNewValue);

        bool IsInTableRow { get; }

        bool EnableValidation { get; }

        IReadOnlyDictionary<string, object> AdditionalAttributes { get; set; }
    }
}