using MComponents.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MComponents.MForm
{
    public interface IMForm
    {
        Guid Id { get; }

        Type ModelType { get; }

        bool IsInTableRow { get; }

        bool EnableValidation { get; }

        EventCallback<MFormSubmitArgs> OnValidSubmit { get; set; }

        bool HasUnsavedChanges { get; }

        void RegisterField(IMField pField, bool pSkipRendering = false);

        void RegisterRow(MFieldRow pRow);

        void OnInputKeyUp(KeyboardEventArgs pArgs, IMPropertyInfo pPropertyInfo);

        Task OnInputValueChanged(IMField pField, IMPropertyInfo pPropertyInfo, object pNewValue);

        void UnregisterField(IMField pField);

        bool Validate();

        void InvokeStateHasChanged();

        IReadOnlyDictionary<string, object> AdditionalAttributes { get; set; }

        ITimezoneService TimezoneService { get; }
    }
}