using DocumentFormat.OpenXml.Spreadsheet;
using MComponents.MForm;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Timers;

namespace MComponents.MGrid
{
    public class MGridActionColumn<T> : ComponentBase, IMGridColumnGenerator<T>, IMGridEditFieldGenerator<T>, IDisposable
    {
        [Inject]
        public IStringLocalizer<MComponentsLocalization> L { get; set; }

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
            HeaderText = HeaderText ?? L["Actions"];
        }

        private async void MDeleteResetTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            mDeleteResetTimer.Stop();
            RowDeleteEnabled = null;

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
                        if (RowDeleteEnabled == null || !RowDeleteEnabled.Equals(pModel))
                        {
                            RowDeleteEnabled = pModel;
                            mDeleteResetTimer.Stop();
                            mDeleteResetTimer.Start();
                            StateHasChanged();
                            Grid.InvokeStateHasChanged();
                            return;
                        }

                        mDeleteResetTimer.Stop();
                        mDeleteResetTimer.Start();

                        Grid.StartDeleteRow(pModel, a);
                    }));
                    builder.AddEventStopPropagationClicksAttribute(22);

                    builder.OpenElement(1, "i");

                    if (RowDeleteEnabled == null || !RowDeleteEnabled.Equals(pModel))
                    {
                        builder.AddAttribute(3, "class", "fas fa-trash-alt m-grid-action-icon m-grid-action-icon--disabled");
                    }
                    else
                    {
                        builder.AddAttribute(3, "class", "fas fa-trash-alt text-danger m-grid-action-icon");
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
