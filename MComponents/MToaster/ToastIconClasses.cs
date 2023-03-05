namespace MComponents.MToaster
{
    /// <summary>
    /// Defines the css classes to be used for the <see cref="ToastState"/>
    /// </summary>
    public class ToastIconClasses
    {
        /// <summary>
        /// The css class for the Info <see cref="ToastState"/>. Defaults to <see cref="ToasterDefaults.Classes.Icons.Info"/>
        /// </summary>
        public string Info { get; set; } = ToasterDefaults.Classes.Icons.Info;

        /// <summary>
        /// The css class for the Success <see cref="ToastState"/>. Defaults to <see cref="ToasterDefaults.Classes.Icons.Success"/>
        /// </summary>
        public string Success { get; set; } = ToasterDefaults.Classes.Icons.Success;

        /// <summary>
        /// The css class for the Warning <see cref="ToastState"/>. Defaults to <see cref="ToasterDefaults.Classes.Icons.Warning"/>
        /// </summary>
        public string Warning { get; set; } = ToasterDefaults.Classes.Icons.Warning;

        /// <summary>
        /// The css class for the Error <see cref="ToastState"/>. Defaults to <see cref="ToasterDefaults.Classes.Icons.Error"/>
        /// </summary>
        public string Error { get; set; } = ToasterDefaults.Classes.Icons.Error;
    }
}