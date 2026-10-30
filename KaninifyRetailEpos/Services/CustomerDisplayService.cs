namespace EposRetail.Services;

/// <summary>
/// Signals the customer-facing display window that the checkout state has changed so it
/// re-renders. The display reads the shared <see cref="CheckoutStateService"/>, so only a
/// change notification needs to travel between the operator and customer windows.
/// </summary>
public class CustomerDisplayService
{
    /// <summary>Raised whenever the checkout state changes and the display should refresh.</summary>
    public event Action? Changed;

    public void NotifyChanged() => Changed?.Invoke();
}
