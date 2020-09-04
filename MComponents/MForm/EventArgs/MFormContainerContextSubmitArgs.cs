using System;

namespace MComponents.MForm
{
    public class MFormContainerContextSubmitArgs : EventArgs
    {
        public bool UserInterated { get; set; }

        internal MFormContainerContextSubmitArgs()
        {
        }
    }
}
