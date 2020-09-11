using Microsoft.AspNetCore.Components;
using System;
using System.Runtime.CompilerServices;

namespace MComponents.MGrid
{
    public class MGridPager : ComponentBase
    {
        [Parameter]
        public int CurrentPage { get; set; } = 1;
            
        [Parameter]
        public int PageCount { get; set; }

        [Parameter]
        public int PageSize { get; set; } = 10;

        [Parameter]
        public int[] SelectablePageSizes { get; set; } = { 10, 20, 30, 50, 100 };


        private IMRegister mGrid;
        private int mCurrentPage = 1;

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
                    mGrid.RegisterPagerSettings(this);
                }
            }
        }

    }
}
