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
                    builder.AddAttribute(104, "type", "button");
                    builder.AddAttribute(105, "class", "m-btn m-btn-secondary m-btn-icon m-btn-sm");

                    builder.AddAttribute(107, "onclick", EventCallback.Factory.Create<MouseEventArgs>(this, (a) =>
                    {
                        mGrid?.ClearFilterValues();
                    }));
                    builder.AddEventStopPropagationClicksAttribute(110);

                    builder.OpenElement(111, "i");
                    builder.AddAttribute(112, "class", "fa-solid fa-eraser m-grid-action-icon");
                    builder.CloseElement(); //i

                    builder.CloseElement(); //button
                }
                else
                {
                    builder.OpenElement(121, "button");
                    builder.AddAttribute(122, "type", "button");
                    builder.AddAttribute(123, "class", "m-btn m-btn-secondary m-btn-icon m-btn-sm");

                    builder.AddAttribute(125, "onclick", EventCallback.Factory.Create<MouseEventArgs>(this, (a) =>
                    {
                        mGrid?.SavePendingChanges(true);
                    }));
                    builder.AddEventStopPropagationClicksAttribute(129);

                    builder.OpenElement(131, "i");
                    builder.AddAttribute(132, "class", "fa-solid fa-floppy-disk m-grid-action-icon");
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
                builder.OpenElement(148, "div");

                builder.AddAttribute(150, "class", "m-action-column-cell m-action-column-btn-group");

                if (AdditionalContent != null)
                    builder.AddContent(153, AdditionalContent.Invoke(pModel));

                if (mGrid.EnableEditing)
                {
                    builder.OpenElement(157, "button");
                    builder.AddAttribute(158, "type", "button");
                    builder.AddAttribute(159, "class", "m-btn m-btn-secondary m-btn-icon m-btn-sm");
                    builder.AddAttribute(160, "onclick", EventCallback.Factory.Create<MouseEventArgs>(this, (a) =>
                    {
                        Grid.StartEditRow(pModel, a);
                    }));
                    builder.AddEventStopPropagationClicksAttribute(164);

                    builder.OpenElement(166, "i");
                    builder.AddAttribute(167, "class", "fa-solid fa-pen-to-square m-grid-action-icon");

                    builder.CloseElement(); //i

                    builder.CloseElement(); //button
                }

                if (mGrid.EnableDeleting)
                {
                    builder.OpenElement(176, "button");
                    builder.AddAttribute(177, "type", "button");
                    builder.AddAttribute(178, "class", "m-btn m-btn-secondary m-btn-icon m-btn-sm");
                    builder.AddAttribute(179, "style", "margin-left: 4px;");
                    builder.AddAttribute(180, "onclick", EventCallback.Factory.Create<MouseEventArgs>(this, (a) =>
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
                    builder.AddEventStopPropagationClicksAttribute(209);

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
            mDeleteResetTimer?.Dispose();
        }
    }
}
