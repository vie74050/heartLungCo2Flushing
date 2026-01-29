public interface ISwitch
{
    /// <summary>
    /// Gets the current state of the toggle button.
    /// </summary>
    bool IsOn { get; set; }

    /// <summary>
    /// IsActive indicates whether the switch can be interacted with.
    /// </summary>
    bool IsActive { get;  set;}

    /// <summary>
    /// Handles the logic when the switch is turned on.
    /// </summary>
    void OnTurnedOn();
    
    /// <summary>
    /// Handles the logic when the switch is turned off.
    /// </summary>
    void OnTurnedOff(); 

}