using Avalonia.Input;
using CommunityToolkit.Mvvm.Input;

namespace teams_phonemanager.Helpers
{
    /// <summary>
    /// Shared helper for dialog overlay event handling in code-behind files.
    /// Eliminates the repeated backdrop/card PointerPressed patterns.
    /// </summary>
    internal static class DialogEventHelper
    {
        /// <summary>
        /// Stops pointer event propagation. Use for dialog card containers
        /// to prevent clicks inside the dialog from closing it.
        /// </summary>
        public static void StopPropagation(object? sender, PointerPressedEventArgs e)
        {
            e.Handled = true;
        }

        /// <summary>
        /// Executes a close command when the backdrop is clicked.
        /// </summary>
        public static void CloseOnBackdropClick(object? dataContext, IRelayCommand? closeCommand)
        {
            closeCommand?.Execute(null);
        }
    }
}
