using System.Collections.Generic;

namespace MComponents.MGrid
{
    public interface IMGridColumn : IIdentifyable
    {
        IReadOnlyDictionary<string, object> AdditionalAttributes { get; }

        string HeaderText { get; }

        bool EnableFilter { get; }

        bool ShouldRenderColumn { get; }

        bool VisibleInExport { get; }
    }
}
