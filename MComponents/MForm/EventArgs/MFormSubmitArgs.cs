using Microsoft.AspNetCore.Components.Forms;
using System.Collections.Generic;

namespace MComponents
{
    public class MFormSubmitArgs
    {
        public EditContext EditContext { get; protected set; }

        public IDictionary<string, object> ChangedValues { get; protected set; }

        public object Model { get; protected set; }

        public bool UserInteracted { get; protected set; }

        internal MFormSubmitArgs(EditContext pContext, IDictionary<string, object> pValues, object pModel, bool pUserInteracted)
        {
            EditContext = pContext;
            ChangedValues = pValues;
            Model = pModel;
            UserInteracted = pUserInteracted;
        }
    }
}
