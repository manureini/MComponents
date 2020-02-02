using Microsoft.AspNetCore.Components.Web;
using System;
using System.Threading.Tasks;

namespace MComponents.MGrid
{
    public interface IMGrid<T> : IMRegister
    {
        void RegisterEvents(MGridEvents<T> pEvents);

        Task StartEditRow(T pValue, MouseEventArgs pMouseEventArgs);

        Task StartDeleteRow(T value, MouseEventArgs pMouseEventArgs);

        Task SavePendingChanges(bool pUserInteracted);

        void ClearFilterValues();

        Guid GetId(T pModel);
        void Refresh();
        void InvokeStateHasChanged();
 
        bool EnableAdding { get; }

        bool EnableEditing { get; }

        bool EnableDeleting { get; }

        bool EnableUserSorting { get; }

        bool EnableFilterRow { get; }

        bool IsEditingRow { get; }
        bool IsFilterRowVisible { get; }
    }
}