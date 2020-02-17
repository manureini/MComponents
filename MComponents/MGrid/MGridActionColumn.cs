using MComponents.MForm;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Timers;

namespace MComponents.MGrid
{
    public class MGridActionColumn<T> : ComponentBase, IMGridColumnGenerator<T>, IMGridEditFieldGenerator<T>, IDisposable
    {
        [Parameter(CaptureUnmatchedValues = true)]
        public IReadOnlyDictionary<string, object> AdditionalAttributes { get; set; }

        [Parameter]
        public string HeaderText { get; set; } = "Aktionen";

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

        protected bool DeletingLocked = true;

        protected Timer mDeleteResetTimer;

        protected override void OnInitialized()
        {
            base.OnInitialized();

            mDeleteResetTimer = new Timer(2000);
            mDeleteResetTimer.Elapsed += MDeleteResetTimer_Elapsed;
        }

        private async void MDeleteResetTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            mDeleteResetTimer.Stop();

            DeletingLocked = true;

            await InvokeAsync(StateHasChanged);

            Grid.InvokeStateHasChanged();
        }

        public RenderFragment<MFieldGeneratorContext> Template(double pWidth, double pHeight)
        {
            return (templContext) => (builder) =>
            {
                builder.OpenElement(1, "div");
                builder.AddAttribute(2, "class", "m-action-column-cell");

                builder.AddStyleWithAttribute2(3, pWidth, pHeight);

                builder.OpenElement(1, "div");
                builder.AddAttribute(2, "class", "m-action-column-btn-group");
              
                if (templContext.Form.AdditionalAttributes != null && templContext.Form.AdditionalAttributes.ContainsKey("data-is-filterrow"))
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
                        if (DeletingLocked)
                        {
                            DeletingLocked = false;
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

                    if (DeletingLocked)
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
