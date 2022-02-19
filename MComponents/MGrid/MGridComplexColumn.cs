using MComponents.ExportData;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;

namespace MComponents.MGrid
{
    public class MGridComplexColumn<T> : ComponentBase, IMGridColumn, IMGridColumnGenerator<T>, IMGridComplexExport<T>
    {
        [Parameter]
        public RenderFragment<T> CellTemplate { get; set; }

        [Parameter]
        public string Identifier { get; set; }

        [Parameter]
        public bool EnableFilter { get; set; } = true;

        [Parameter]
        public Func<T, string> ExportText { get; set; }

        [Parameter(CaptureUnmatchedValues = true)]
        public IReadOnlyDictionary<string, object> AdditionalAttributes { get; set; }

        [Parameter]
        public string HeaderText { get; set; }

        protected IMRegister mGrid;

        [CascadingParameter]
        public IMRegister Grid
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

        public bool ShouldRenderColumn => true;

        public bool VisibleInExport { get; set; } = true;

        public RenderFragment GenerateContent(T pModel)
        {
            return CellTemplate(pModel);
        }

        public object GenerateExportCell(SharedStringTableWrapper pSharedStringTablePart, Dictionary<string, int> pSstCache, T pModel)
        {
            if (ExportText != null)
                return ExcelExportHelper.CreateTextCell(pSharedStringTablePart, pSstCache, ExportText(pModel));
            else
                return ExcelExportHelper.CreateGeneralCell(string.Empty);
        }
    }
}
