using UnityEngine;

public class UIController : MonoBehaviour
{
    #region Variables

    [SerializeField]
    [Tooltip("Container object holding reactor UI components")]
    private GameObject reactorUiParent;     // Parent transform of reactor UI components


    [SerializeField]
    [Tooltip("Settings menu displayed when the user elects to alter app settings")]
    private GameObject settingsMenuParent;  // Parent transform of the settings menu

    public static bool showingSettingsMenu { get; private set; }    // True when the settings menu is displayed, false, when it is not.

    #endregion

    #region UnityMethods

    private void Update()
    {
        // Check if the menu button was pressed
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // Toggle the state of the settings menu
            ToggleSettingsMenuState();
        }
    }

    #endregion

    #region StateOptions

    /// <summary>
    /// Toggle the state of the settings menu, making it visible or not.
    /// </summary>
    private void ToggleSettingsMenuState()
    {
        // Toggle the state of the reactor ui parent object
        reactorUiParent.SetActive(!reactorUiParent.activeSelf);

        // Toggle the state of the settings menu
        settingsMenuParent.SetActive(!settingsMenuParent.activeSelf);

        // Update the showing settings menu state flag
        showingSettingsMenu = settingsMenuParent.activeSelf;
    }

    #endregion
}
