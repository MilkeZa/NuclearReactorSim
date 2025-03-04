using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIController : MonoBehaviour
{
    [SerializeField]
    [Tooltip("Container object holding reactor UI components")]
    private GameObject reactorUiParent;

    [SerializeField]
    [Tooltip("Settings menu displayed when the user elects to alter app settings")]
    private GameObject settingsMenuParent;

    public static bool showingSettingsMenu { get; private set; }

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
