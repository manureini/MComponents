namespace MComponents.MGrid
{
    public class MGridSettings
    {
        internal static MGridSettings Instance = new MGridSettings();

        public IMGridFormatterFactoryProvider FormatterFactory { get; set; }

        public bool EnableAdding { get; set; }

        public bool EnableEditing { get; set; }

        public bool EnableDeleting { get; set; }

        public bool EnableUserSorting { get; set; }

        public bool EnableFilterRow { get; set; }

        public bool EnableGrouping { get; set; }

        public bool EnableExport { get; set; }

        public bool EnableImport { get; set; }

        public bool EnableSaveState { get; set; }

        public bool UseDeleteDoubleClick { get; set; }

        public ToolbarItem ToolbarItems { get; set; }

        public MGridInitialState InitialState { get; set; } = MGridInitialState.Default;

        public string HtmlTableClass { get; set; } = "m-grid m-grid-striped m-grid-bordered m-grid-hover";
    }
}
