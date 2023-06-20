using System;

namespace MComponents.MToaster
{
    /// <summary>
    /// Represents the global <see cref="ToasterConfiguration"/> instance
    /// </summary>
    public class ToasterConfiguration : CommonToastOptions
    {
        private bool _newestOnTop;
        private bool _preventDuplicates;
        private int _maxDisplayedToasts;
        private string _positionClass;
        private bool _errorRequiresInteraction;
        private int? _errorVisibleStateDuration;

        internal event Action OnUpdate;

        /// <summary>
        /// Drives the toast display sequence: when true the newest displayable toast will be on top. Otherwise it will be on the bottom. Defaults to false.
        /// </summary>
        public bool NewestOnTop
        {
            get => _newestOnTop;
            set
            {
                _newestOnTop = value;
                OnUpdate?.Invoke();
            }
        }

        /// <summary>
        /// When true, a new toast with the same type, title and message of an already present toast will be ignored. Defaults to true.
        /// </summary>
        public bool PreventDuplicates
        {
            get => _preventDuplicates;
            set
            {
                _preventDuplicates = value;
                OnUpdate?.Invoke();
            }
        }

        /// <summary>
        /// The maximum number of toasts to be displayed at the same time. Defaults to 5
        /// </summary>
        public int MaxDisplayedToasts
        {
            get => _maxDisplayedToasts;
            set
            {
                _maxDisplayedToasts = value;
                OnUpdate?.Invoke();
            }
        }

        /// <summary>
        /// The css class driving the toast position in the screen. The predefined positions are contained in <see cref="ToasterDefaults.Classes.Position"/>. Defaults to <see cref="ToasterDefaults.Classes.Position.TopRight"/>
        /// </summary>
        public string PositionClass
        {
            get => _positionClass;
            set
            {
                _positionClass = value;
                OnUpdate?.Invoke();
            }
        }

        /// <summary>
        /// Error Toasts require user interaction to dismiss them. Defaults to false
        /// </summary>
        public bool ErrorRequiresInteraction
        {
            get => _errorRequiresInteraction;
            set
            {
                _errorRequiresInteraction = value;
                OnUpdate?.Invoke();
            }
        }

        /// <summary>
        /// How long the error toast remain visible without user interaction. A value less than 1 triggers the hiding immediately. Defaults to 5000 ms.
        /// </summary>
        public int? ErrorVisibleStateDuration
        {
            get => _errorVisibleStateDuration;
            set
            {
                _errorVisibleStateDuration = value;
                OnUpdate?.Invoke();
            }
        }

        /// <summary>
        ///  A <see cref="ToastIconClasses"/> instance containing the css classes for all the <see cref="ToastState"/> states.
        /// </summary>
        public ToastIconClasses IconClasses = new ToastIconClasses();

        public ToasterConfiguration()
        {
            PositionClass = ToasterDefaults.Classes.Position.TopRight;
            NewestOnTop = false;
            PreventDuplicates = true;
            MaxDisplayedToasts = 5;
        }

        internal string ToastTypeClass(ToastType type)
        {
            switch (type)
            {
                case ToastType.Info: return IconClasses.Info;
                case ToastType.Error: return IconClasses.Error;
                case ToastType.Success: return IconClasses.Success;
                case ToastType.Warning: return IconClasses.Warning;
                default: return IconClasses.Info;
            }
        }
    }
}