using Microsoft.AspNetCore.Components;

namespace MComponents.MGrid
{
    public class MGridEvents<T> : ComponentBase
    {
        [Parameter]
        public EventCallback<BeginAddArgs<T>> OnBeginAdd { get; set; }

        [Parameter]
        public EventCallback<BeginEditArgs<T>> OnBeginEdit { get; set; }

        [Parameter]
        public EventCallback<BeginDeleteArgs<T>> OnBeginDelete { get; set; }


        [Parameter]
        public EventCallback<AfterAddArgs<T>> OnAfterAdd { get; set; }

        [Parameter]
        public EventCallback<AfterEditArgs<T>> OnAfterEdit { get; set; }

        [Parameter]
        public EventCallback<AfterDeleteArgs<T>> OnAfterDelete { get; set; }


        [Parameter]
        public EventCallback<MFormValueChangedArgs> OnFormValueChanged { get; set; }


        [Parameter]
        public EventCallback<BeginRowSelectArgs<T>> OnBeginRowSelect { get; set; }


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
                    mGrid.RegisterEvents(this);
                }
            }
        }
    }
}
