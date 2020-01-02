using Microsoft.AspNetCore.Components;
using System.Collections.Generic;

namespace MComponents.MSelect
{
    public class MSelectOption : ComponentBase
    {
        [Parameter]
        public object Value { get; set; }

        [Parameter]
        public string Identifier { get; set; }

        [Parameter]
        public bool ShowOnlyIfNoSearchResults { get; set; } = false;

        [Parameter(CaptureUnmatchedValues = true)]
        public IReadOnlyDictionary<string, object> AdditionalAttributes { get; set; }


        private IMSelect mSelect;

        [CascadingParameter]
        public IMSelect MSelect
        {
            get
            {
                return mSelect;
            }
            set
            {
                if (value != mSelect)
                {
                    mSelect = value;
                    mSelect.RegisterOption(this);
                }
            }
        }

    }
}
