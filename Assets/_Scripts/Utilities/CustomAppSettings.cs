using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;

public class CustomAppSettings : MonoBehaviour
{
    #region Variables

    [Header("Gameobjects")]
    [SerializeField]
    [Tooltip("Toggle that enables/disables fullscreen mode")]
    private UnityEngine.UI.Toggle fullscreenToggle;

    [SerializeField]
    [Tooltip("Dropdown menu that allows user to select desired resolution")]
    private TMP_Dropdown resolutionDropdown;

    [SerializeField]
    [Tooltip("Dropdown menu that allows user to select desired framerate")]
    private TMP_Dropdown framerateDropdown;

    [SerializeField]
    [Tooltip("Slider used to alter the volume of the application")]
    private UnityEngine.UI.Slider masterVolumeSlider;

    [SerializeField]
    [Tooltip("Slider used to alter the volume of sound effects")]
    private UnityEngine.UI.Slider sfxVolumeSlider;

    [SerializeField]
    [Tooltip("Master audi mixer used in the scene")]
    private AudioMixer masterAudioMixer;

    private int defaultFramerateIndex = 3;                              // Default framerate index
    private readonly int[] framerateOptions = { 12, 24, 40, 60, 144 };  // List of available framerates

    private int currentFramerateIndex;                                  // Index of current framerate option
    private int currentFramerate;                                       // Current framerate

    private Resolution[] availableResolutions;                          // Resolution options available based on the screen

    #endregion

    #region UnityMethods

    private void Awake()
    {
        // Initialize the graphics settings and associated gameobjects
        InitializeGraphics();
    }

    #endregion

    #region GraphicsMethods

    /// <summary>
    /// Initializes the graphics settings in one compact method.
    /// </summary>
    private void InitializeGraphics()
    {
        // Initialize the resolution
        InitializeResolutions();

        // Initialize the fullscreen
        InitializeFullscreen();

        // Set vSyncCount to 0 so that using the .targetFrameRate is enabled
        QualitySettings.vSyncCount = 0;

        // Initialize the framrate
        InitializeFramerate();
    }

    /// <summary>
    /// Initializes the resolution of the application.
    /// </summary>
    private void InitializeResolutions()
    {
        // Get the available resolutions
        availableResolutions = Screen.resolutions;

        // Format the resolutions as strings as that is what the dropdown takes
        List<string> _resolutionOptions = new List<string>();

        // Keep track of the current resolution index for setting the default resolution
        int _currentResolutionIndex = 0;
        Resolution _currentResolution = Screen.currentResolution;

        for (int i = 0; i < availableResolutions.Length; i++)
        {
            // Format the string as 'width x height'
            string _option = $"{availableResolutions[i].width} x {availableResolutions[i].height}";

            // Insert the option into the list
            _resolutionOptions.Add(_option);

            // Check if the resolution at this index is the same as the screen resolution
            Resolution _resolutionOption = availableResolutions[i];
            if (_resolutionOption.width == _currentResolution.width && _resolutionOption.height == _currentResolution.height)
            {
                // Current resolution found, update the index
                _currentResolutionIndex = i;
            }
        }

        // Clear the current options from the resolution dropdown
        resolutionDropdown.ClearOptions();

        // Add the resolution strings to the dropdown
        resolutionDropdown.AddOptions(_resolutionOptions);

        // Set the current resolution using the index found earlier
        resolutionDropdown.value = _currentResolutionIndex;
    }

    /// <summary>
    /// Sets the resolution of the application.
    /// </summary>
    /// <param name="_resolutionIndex">Index of the resolution option to use when selecting a resolution.</param>
    public void SetResolution(int _resolutionIndex)
    {
        // Verify the resolution index is valid
        if (_resolutionIndex < 0 || _resolutionIndex >= availableResolutions.Length)
        {
            // Invalid resolution index passed, fall back to the first available
            _resolutionIndex = 0;
        }

        // Get the resolution data from the available resolution array
        Resolution _resolution = availableResolutions[_resolutionIndex];

        // Set the screens resolution variable
        Screen.SetResolution(_resolution.width, _resolution.height, Screen.fullScreen);
    }

    private void InitializeFullscreen()
    {
        // Get the toggle buttons value then set the full screen
        bool _isFullscreen = fullscreenToggle.isOn;

        // Set the screens fullscreen variable to the is full screen toggle button value
        Screen.fullScreen = _isFullscreen;
    }

    /// <summary>
    /// Sets the fullscreen state of the application.
    /// </summary>
    /// <param name="_isFullscreen">Boolean correlating to fullscreen when true, windowed, otherwise.</param>
    public void SetFullScreen(bool _isFullscreen)
    {
        // Set the full screen option
        Screen.fullScreen = _isFullscreen;
    }

    /// <summary>
    /// Initializes the application framerate and dropdown.
    /// </summary>
    private void InitializeFramerate()
    {
        // Create a list to hold the framerate option strings
        List<string> _framerateOptionStrings = new List<string>();

        // Iterate through each of the available framerate options
        foreach (int _framerate in framerateOptions)
        {
            // Format the framerate as a string to be displayed in the dropdown
            string _framerateOption = $"{_framerate} fps";

            // Insert the option into the option list
            _framerateOptionStrings.Add(_framerateOption);
        }

        // Clear the current options from the framerate dropdown
        framerateDropdown.ClearOptions();

        // Add the framerate option strings to the dropdown
        framerateDropdown.AddOptions(_framerateOptionStrings);;

        // Set the framerate using the default framerate index
        SetFrameRateFromIndex(defaultFramerateIndex);

        // Set the framerate dropdown value to the current framerate index
        framerateDropdown.value = currentFramerateIndex;
    }

    /// <summary>
    /// Sets the applications target framerate based on an index retrieved from the framerate dropdown UI element.
    /// </summary>
    /// <param name="_framerateIndex">Index of the framerate option to use when selecting a framerate.</param>
    public void SetFrameRateFromIndex(int _framerateIndex)
    {
        // Verify the framerate index is valid
        if (_framerateIndex < 0 || _framerateIndex >= framerateOptions.Length)
        {
            // Invalid framerate passed, fall back to default
            _framerateIndex = defaultFramerateIndex;
        }

        // Update the current framerate index and framerate
        currentFramerateIndex = _framerateIndex;
        currentFramerate = framerateOptions[currentFramerateIndex];

        // Set the target framerate using the current frame rate
        Application.targetFrameRate = currentFramerate;
    }

    #endregion

    #region AudioMethods

    /// <summary>
    /// Sets the master volume of the application.
    /// </summary>
    /// <param name="_level">Volume level from the audio mixers master channel.</param>
    public void SetMasterVolume(float _level)
    {
        // Update the master audio mixers primary level
        masterAudioMixer.SetFloat("masterVolume", _level);
    }

    /// <summary>
    /// Sets the sound effect volume of the application
    /// </summary>
    /// <param name="_level">Volume level from the audio mixers sound effects channel.</param>
    public void SetSfxVolume(float _level)
    {
        // Update the master audio mixers sound effect channel level
        masterAudioMixer.SetFloat("sfxVolume", _level);
    }

    #endregion
}
