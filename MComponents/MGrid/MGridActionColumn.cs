using DocumentFormat.OpenXml.Spreadsheet;
using MComponents.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Localization;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Timers;

namespace MComponents.MGrid
{
    public class MGridActionColumn<T> : ComponentBase, IMGridColumnGenerator<T>, IMGridEditFieldGenerator<T>, IDisposable
    {
        [Inject]
        public IStringLocalizer L { get; set; }

        [Inject]
        public IJSRuntime JsRuntime { get; set; }

        [Inject]
        public MComponentSettings Settings { get; set; }

        [Parameter(CaptureUnmatchedValues = true)]
        public IReadOnlyDictionary<string, object> AdditionalAttributes { get; set; }

        [Parameter]
        public string HeaderText { get; set; }

        [Parameter]
        public string Identifier { get; set; } = "Actions";

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
            mDeleteResetTimer.Stop();
            RowDeleteEnabled = null;

            Grid.Formatter.ClearRowMetadata();
            await InvokeAsync(StateHasChanged);
            Grid.InvokeStateHasChanged();
        }

        public RenderFragment EditFieldTemplate(double pLeftOffset, BoundingBox pBoundingBox, bool pIsFilterRow)
        {
            return (builder) =>
            {
                builder.OpenElement(1, "div");
                builder.AddAttribute(2, "class", "m-action-column-cell");

                builder.OpenElement(1, "div");
                builder.AddAttribute(2, "class", "m-action-column-btn-group");

                if (pIsFilterRow)
                {
                    builder.OpenElement(1, "button");
                    builder.AddAttribute(2, "class", "m-btn m-btn-secondary m-btn-icon m-btn-sm");

                    builder.AddAttribute(21, "onclick", EventCallback.Factory.Create<MouseEventArgs>(this, (a) =>
                    {
                        mGrid.ClearFilterValues();
                    }));
                    builder.AddEventStopPropagationClicksAttribute(22);

                    builder.OpenElement(1, "i");
                    builder.AddAttribute(3, "class", "fas fa-eraser m-grid-action-icon");
                    builder.CloseElement(); //i

                    builder.CloseElement(); //button
                }
                else
                {
                    builder.OpenElement(1, "button");
                    builder.AddAttribute(2, "class", "m-btn m-btn-secondary m-btn-icon m-btn-sm");

                    builder.AddAttribute(21, "onclick", EventCallback.Factory.Create<MouseEventArgs>(this, (a) =>
                    {
                        mGrid.SavePendingChanges(true);
                    }));
                    builder.AddEventStopPropagationClicksAttribute(22);

                    builder.OpenElement(1, "i");
                    builder.AddAttribute(3, "class", "fas fa-save m-grid-action-icon");
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
                builder.OpenElement(1, "div");

                builder.AddAttribute(2, "class", "m-action-column-cell m-action-column-btn-group");

                if (mGrid.EnableEditing)
                {
                    builder.OpenElement(1, "button");
                    builder.AddAttribute(2, "class", "m-btn m-btn-secondary m-btn-icon m-btn-sm");
                    builder.AddAttribute(21, "onclick", EventCallback.Factory.Create<MouseEventArgs>(this, (a) =>
                    {
                        Grid.StartEditRow(pModel, a);
                    }));
                    builder.AddEventStopPropagationClicksAttribute(22);

                    builder.OpenElement(1, "i");
                    builder.AddAttribute(3, "class", "fas fa-edit m-grid-action-icon");

                    builder.CloseElement(); //i

                    builder.CloseElement(); //button
                }

                if (mGrid.EnableDeleting)
                {
                    builder.OpenElement(1, "button");
                    builder.AddAttribute(2, "class", "m-btn m-btn-secondary m-btn-icon m-btn-sm");
                    builder.AddAttribute(2, "style", "margin-left: 4px;");
                    builder.AddAttribute(21, "onclick", EventCallback.Factory.Create<MouseEventArgs>(this, (a) =>
                     {
                         if (Settings.UseDeleteConfirmationWithAlert)
                         {
                             RowDeleteEnabled = pModel;
                             Grid.Formatter.ClearRowMetadata();
                             Grid.Formatter.AddRowMetadata(pModel, MGridDefaultObjectFormatter<T>.ROW_DELETE_METADATA);
                             StateHasChanged();
                             Grid.InvokeStateHasChanged();
                             return;
                         }

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

                         _ = Grid.StartDeleteRow(pModel, a);
                     }));
                    builder.AddEventStopPropagationClicksAttribute(22);

                    builder.OpenElement(1, "i");

                    if (RowDeleteEnabled != null && RowDeleteEnabled.Equals(pModel))
                    {
                        builder.AddAttribute(3, "class", "fas fa-trash-alt m-grid-action-icon");

                        if (Settings.UseDeleteConfirmationWithAlert)
                        {
                            Task.Delay(150).ContinueWith(async t =>
                            {
                                bool confirmed = await JsRuntime.InvokeAsync<bool>("confirm", L["Are you sure?"].ToString());

                                RowDeleteEnabled = null;
                                Grid.Formatter.ClearRowMetadata();

                                if (confirmed)
                                {
                                    _ = InvokeAsync(() => _ = Grid.StartDeleteRow(pModel, null));
                                }
                                else
                                {
                                    Grid.InvokeStateHasChanged();
                                }
                            });
                        }
                    }
                    else
                    {
                        builder.AddAttribute(3, "class", "fas fa-trash-alt m-grid-action-icon m-grid-action-icon--disabled");
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
