public interface IToggleButton
{
    /// <summary>
    /// Gets the current state of the toggle button.
    /// </summary>
    bool IsOn { get; }

    /// <summary>
    /// Toggles the current state.
    /// </summary>
    void Toggle();

}