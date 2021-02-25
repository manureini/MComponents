using Microsoft.AspNetCore.Components;

namespace MComponents.MGrid
{
    public class MGridPager : ComponentBase
    {
        [Parameter]
        public long CurrentPage { get; set; } = 1;
            
        [Parameter]
        public long PageCount { get; set; }

        [Parameter]
        public int PageSize { get; set; } = 10;

        [Parameter]
        public int[] SelectablePageSizes { get; set; } = { 10, 20, 30, 50, 100 };


        private IMRegister mGrid;
        private long mCurrentPage = 1;

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
