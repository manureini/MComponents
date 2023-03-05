namespace MComponents.MToaster
{
    internal enum ToastState
    {
        Init,
        Showing,
        Hiding,
        Visible,
        MouseOver
    }

    internal static class ToastStateExtensions
    {
        public static bool IsShowing(this ToastState state) => state == ToastState.Showing;
        public static bool IsVisible(this ToastState state) => state == ToastState.Visible;
        public static bool IsHiding(this ToastState state) => state == ToastState.Hiding;
    }
}