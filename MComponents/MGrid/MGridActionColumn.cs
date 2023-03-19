using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Localization;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Timers;

namespace MComponents.MGrid
{
    public class MGridActionColumn<T> : ComponentBase, IMGridColumnGenerator<T>, IMGridEditFieldGenerator<T>, IDisposable
    {
        [Inject]
        public IStringLocalizer L { get; set; }

        [Inject]
        public IJSRuntime JsRuntime { get; set; }

        [Parameter(CaptureUnmatchedValues = true)]
        public IReadOnlyDictionary<string, object> AdditionalAttributes { get; set; }

        [Parameter]
        public string HeaderText { get; set; }

        [Parameter]
        public string Identifier { get; set; } = "Actions";

        [Parameter]
        public bool UseDeleteDoubleClick { get; set; } = MGridSettings.Instance.UseDeleteDoubleClick;

        [Parameter]
        public RenderFragment<T> AdditionalContent { get; set; }

        private IMGrid<T> mGrid;

        [CascadingParameter]
        public IMGrid<T> Grid
        {
            get
            {
                return mGrid;
            }
            set
            {
                if (value != mGrid)
                {
                    mGrid = value;
                    mGrid.RegisterColumn(this);
                }
            }
        }

        public bool EnableFilter => true;

        public bool ShouldRenderColumn => mGrid.EnableEditing || mGrid.EnableDeleting || mGrid.IsFilterRowVisible || mGrid.IsEditingRow;

        public bool VisibleInExport => false;

        protected object RowDeleteEnabled;
        protected DateTime RowDeleteClicked;

        protected Timer mDeleteResetTimer;

        protected override void OnInitialized()
        {
            base.OnInitialized();

            mDeleteResetTimer = new Timer(2000);
            mDeleteResetTimer.Elapsed += MDeleteResetTimer_Elapsed;
        }

        protected override void OnParametersSet()
        {
            base.OnParametersSet();
            HeaderText ??= L["Actions"];
        }

        private async void MDeleteResetTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (!UseDeleteDoubleClick)
                return;

            mDeleteResetTimer.Stop();
            RowDeleteEnabled = null;

            Grid.Formatter.ClearRowMetadata();
            await InvokeAsync(StateHasChanged);
            Grid.InvokeStateHasChanged();
        }

        public RenderFragment EditFieldTemplate(bool pIsFilterRow)
        {
            return (builder) =>
            {
                builder.OpenElement(94, "div");
                builder.AddAttribute(95, "class", "m-action-column-cell m-form-control"); //m-form-control will set correct height

                builder.OpenElement(97, "div");
                builder.AddAttribute(98, "class", "m-action-column-btn-group");

                if (pIsFilterRow)
                {
                    builder.OpenElement(102, "button");
                    builder.AddAttribute(103, "class", "m-btn m-btn-secondary m-btn-icon m-btn-sm");

                    builder.AddAttribute(105, "onclick", EventCallback.Factory.Create<MouseEventArgs>(this, (a) =>
                    {
                        mGrid?.ClearFilterValues();
                    }));
                    builder.AddEventStopPropagationClicksAttribute(22);

                    builder.OpenElement(111, "i");
                    builder.AddAttribute(112, "class", "fa-solid fa-eraser m-grid-action-icon");
                    builder.CloseElement(); //i

                    builder.CloseElement(); //button
                }
                else
                {
                    builder.OpenElement(119, "button");
                    builder.AddAttribute(120, "class", "m-btn m-btn-secondary m-btn-icon m-btn-sm");

                    builder.AddAttribute(122, "onclick", EventCallback.Factory.Create<MouseEventArgs>(this, (a) =>
                    {
                        mGrid?.SavePendingChanges(true);
                    }));
                    builder.AddEventStopPropagationClicksAttribute(22);

                    builder.OpenElement(128, "i");
                    builder.AddAttribute(129, "class", "fa-solid fa-floppy-disk m-grid-action-icon");
                    builder.CloseElement(); //i

                    builder.CloseElement(); //button
                }

                builder.CloseElement(); //div

                builder.CloseElement(); //div
            };
        }

        public RenderFragment GenerateContent(T pModel)
        {
            return builder =>
            {
                builder.OpenElement(145, "div");

                builder.AddAttribute(147, "class", "m-action-column-cell m-action-column-btn-group");

                if (AdditionalContent != null)
                    builder.AddContent(149, AdditionalContent.Invoke(pModel));

                if (mGrid.EnableEditing)
                {
                    builder.OpenElement(153, "button");
                    builder.AddAttribute(154, "class", "m-btn m-btn-secondary m-btn-icon m-btn-sm");
                    builder.AddAttribute(155, "onclick", EventCallback.Factory.Create<MouseEventArgs>(this, (a) =>
                    {
                        Grid.StartEditRow(pModel, a);
                    }));
                    builder.AddEventStopPropagationClicksAttribute(22);

                    builder.OpenElement(161, "i");
                    builder.AddAttribute(162, "class", "fa-solid fa-pen-to-square m-grid-action-icon");

                    builder.CloseElement(); //i

                    builder.CloseElement(); //button
                }

                if (mGrid.EnableDeleting)
                {
                    builder.OpenElement(171, "button");
                    builder.AddAttribute(172, "class", "m-btn m-btn-secondary m-btn-icon m-btn-sm");
                    builder.AddAttribute(173, "style", "margin-left: 4px;");
                    builder.AddAttribute(174, "onclick", EventCallback.Factory.Create<MouseEventArgs>(this, (a) =>
                     {
                         if (UseDeleteDoubleClick)
                         {
                             if (RowDeleteEnabled == null || !RowDeleteEnabled.Equals(pModel))
                             {
                                 RowDeleteEnabled = pModel;
                                 Grid.Formatter.ClearRowMetadata();
                                 Grid.Formatter.AddRowMetadata(pModel, MGridDefaultObjectFormatter<T>.ROW_DELETE_METADATA);

                                 RowDeleteEnabled = pModel;
                                 mDeleteResetTimer.Stop();

                                 RowDeleteClicked = DateTime.UtcNow;
                                 mDeleteResetTimer.Start();
                                 StateHasChanged();
                                 Grid.InvokeStateHasChanged();
                                 return;
                             }

                             if (DateTime.UtcNow.Subtract(RowDeleteClicked).TotalMilliseconds < 500)
                                 return;

                             mDeleteResetTimer.Stop();
                             mDeleteResetTimer.Start();
                         }

                         _ = Grid.StartDeleteRow(pModel, a);
                     }));
                    builder.AddEventStopPropagationClicksAttribute(22);

                    builder.OpenElement(212, "i");

                    if (RowDeleteEnabled != null && RowDeleteEnabled.Equals(pModel))
                    {
                        builder.AddAttribute(240, "class", "fa-solid fa-trash-can m-grid-action-icon");
                    }
                    else
                    {
                        builder.AddAttribute(240, "class", "fa-solid fa-trash-can m-grid-action-icon m-grid-action-icon--disabled");
                    }

                    builder.CloseElement(); //i

                    builder.CloseElement(); //button
                }

                builder.CloseElement(); //div
            };
        }

        public void Dispose()
        {
            mDeleteResetTimer.Dispose();
        }
    }
}
