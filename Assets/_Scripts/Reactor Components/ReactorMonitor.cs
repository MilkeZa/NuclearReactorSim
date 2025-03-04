using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// This class monitors and controls reactor components.
/// </summary>
public class ReactorMonitor : MonoBehaviour
{
    #region Variables

    [Header("UI Components")]
    [SerializeField]
    [Tooltip("Text element displaying the target output in neutrons")]
    private TMP_Text targetPowerLabel;

    [SerializeField]
    [Tooltip("Text element displaying the current power output in neutrons")]
    private TMP_Text currentPowerLabel;

    [SerializeField]
    [Tooltip("Text element displaying the power level as a percentage")]
    private TMP_Text currentPowerPercentLabel;

    [SerializeField]
    [Tooltip("Text element displaying the difference between the target power and the actual power")]
    private TMP_Text currentPowerDifferenceLabel;

    [SerializeField]
    [Tooltip("Text element displaying the state of the control rods")]
    private TMP_Text controlRodEnabledLabel;

    [SerializeField]
    [Tooltip("Text element displaying the movement state of the control rods")]
    private TMP_Text controlRodStateLabel;

    [SerializeField]
    [Tooltip("Text element displaying the state of the moderator rods")]
    private TMP_Text moderatorRodEnabledLabel;

    [SerializeField]
    [Tooltip("Text element displaying the movement state of the moderator rods")]
    private TMP_Text moderatorRodStateLabel;

    [SerializeField]
    [Tooltip("Text element displaying the state of the water pumps")]
    private TMP_Text waterPumpStateLabel;

    [SerializeField]
    [Tooltip("Text element displaying the average temperature of the coolant water")]
    private TMP_Text waterTempLabel;

    [SerializeField]
    [Tooltip("Text element displaying the level of water remaining within the reactor")]
    private TMP_Text waterCapacityLabel;

    [SerializeField]
    [Tooltip("Water tank containing coolant water cells")]
    private CoolantWaterTank coolantWaterTank;

    [Header("Prefabs")]
    [SerializeField]
    [Tooltip("Assembly whose fuel is to be controlled.")]
    private FuelAssembly fuelAssembly;          // Assembly whose fuel is to be monitored
    private FuelRod[] fuelRods;                 // Fuel rods being monitored

    [SerializeField]
    [Tooltip("Array of rod movement controllers")]
    private RodArrayController rodArray;        // Rods to be moved to control the reaction

    [Header("Controls")]
    [SerializeField]
    [Tooltip("Enable/disable control rod movement")]
    private bool enableControlRods = true;      // Enable/disable the control rods movement
    private bool previousEnableControlRodsState = true;

    [SerializeField]
    [Tooltip("Enable/disable moderator rod movement")]
    private bool enableModeratorRods = false;   // Enable/disable the moderator rods movement
    private bool previousEnableModeratorRodsState = false;

    [SerializeField]
    [Tooltip("Enable/disable coolant water pumps")]
    private bool enableCoolantWaterPumps = true;
    private bool previousCoolantWaterPumpState = true;

    [SerializeField]
    [Tooltip("Target number of neutrons in the scene")]
    private int targetPower = 50;               // Target output power (number of neutrons desired in the scene)
    private int currentPower;                   // Power currently being produced (number of neutrons in the scene)

    // Update timer
    [SerializeField]
    [Tooltip("Number of seconds between updates")]
    private int updateTimerDuration = 5;        // Number of seconds between updates
    private bool updateTimerRunning = false;    // Update timer currently running?
    private DateTime updateTimerStart;          // When the timer started
    private DateTime updateTimerEnd;            // When the timer expires


    // Power output
    public float currentPowerPercent { get; private set; }          // Current power percentage compared to target power
    public int currentPowerDifference { get; private set; }         // Difference between target and current number of neutrons

    // Water metrics
    public float averageWaterTempF { get; private set; }            // Average water temperature in degrees Fahrenheit
    public float averageWaterTempC { get; private set; }            // Average water temperature in degrees Celsius

    // Fuel rod counts
    public bool fuelPresent { get; private set; }                   // Are any fuel rods present in the reactor?
    public int totalFuelRods { get; private set; }                  // Total number of fuel rods

    public int fissileFuelRods { get; private set; }                // Number of fissile fuel rods
    public int nonFissileFuelRods { get; private set; }             // Number of non-fissile fuel rods
    public int xenonSaturatedFuelRods { get; private set; }         // Number of fuel rods saturated with xenon
    public int reactiveFuelRods { get; private set; }               // Number of fuel rods capable of reacting with a neutron

    public float fissileFuelFactor { get; private set; }            // 0, 1 clamped value of how many fuel rods are fissile
    public float nonFissileFuelFactor { get; private set; }         // 0, 1 clamped value of how many fuel rods are non-fissile
    public float xenonSaturatedFuelFactor { get; private set; }     // 0, 1 clamped value of how many fuel rods are saturated with xenon
    public float reactiveFuelRodFactor { get; private set; }        // 0, 1 clamped value of how many fuel rods are reactive

    public float percentFuelFissile { get; private set; }           // Percent out of 100 of fissile fuel
    public float percentFuelNonFissile { get; private set; }        // Percent out of 100 of non-fissile fuel
    public float percentFuelXenonSaturated { get; private set; }    // Percent out of 100 of fuel saturated with xenon
    public float percentFuelReactive { get; private set; }          // Percent out of 100 of fuel capable of reacting with a neutron

    #endregion

    #region UnityMethods

    private void Awake()
    {
        // Register to the fuel assembly's fuel rod insert/destroy actions
        fuelAssembly.OnFuelRodsInserted += InitializeStatistics;
        fuelAssembly.OnFuelRodsDestroyed += ResetMonitor;

        // Initialize the UI elements
        InitializeUI();
    }

    private void Start()
    {
        // Start the monitor update timer
        StartUpdateTimer();

        // Enable/disable the movement of moderating rods
        rodArray.enableControlRods = enableControlRods;
        rodArray.enableModeratorRods = enableModeratorRods;
    }

    private void Update()
    {
        // Check if control rod movement is enabled and there are fuel rods present
        if (enableControlRods && fuelPresent)
        {
            // Handle control rod movement based on current power output
            if (currentPower < targetPower)
            {
                // Current output is below target, begin raising the control rods
                rodArray.RaiseControlRods();
            }
            else if (currentPower > targetPower)
            {
                // Current output is above target, begin lower the control rods
                rodArray.LowerControlRods();
            }
            else
            {
                // Current output is equal to target, halt control rod movement
                rodArray.HaltControlRods();
            }
        }

        // Enable/disable movement of the control & moderator rods
        if (previousEnableControlRodsState != enableControlRods)
        {
            // Set the rod arrays enable control rods flag
            rodArray.enableControlRods = enableControlRods;

            // Update the previous flag
            previousEnableControlRodsState = enableControlRods;
        }

        if (previousEnableModeratorRodsState != enableModeratorRods)
        {
            // Set the rod arrays enable moderator rods flag
            rodArray.enableModeratorRods = enableModeratorRods;

            // Update the previous flag
            previousEnableModeratorRodsState = enableModeratorRods;
        }

        // Check if the enable water pump state has changed
        if (previousCoolantWaterPumpState != enableCoolantWaterPumps)
        {
            // State has changed, check if enable or disable
            if (enableCoolantWaterPumps)
            {
                // Enable water pumps
                coolantWaterTank.EnablePumps();
            }
            else
            {
                // Disable water pumps
                coolantWaterTank.DisablePumps();
            }

            // Update the previous coolant water pump state flag
            previousCoolantWaterPumpState = enableCoolantWaterPumps;
        }
    }

    #endregion

    #region MonitorMethods

    private void ResetMonitor()
    {
        // Check if the fuel rods are null
        if (fuelRods != null)
        {
            // Unsubscribe from each of the fuel rods actions
            foreach (FuelRod _fuelRod in fuelRods)
            {
                _fuelRod.OnFuelRodStateChanged -= UpdateFuelRodCounts;
            }
        }

        // Clear the fuel rods array and reset the fuel present flag
        fuelRods = null;
        fuelPresent = false;

        // Set each of the fuel rod variables to zero
        ResetFuelRodCounts();

        // Set each of the statistics to zero
        percentFuelFissile = 0f;
        percentFuelNonFissile = 0f;
        percentFuelXenonSaturated = 0f;
        percentFuelReactive = 0f;
    }

    private void InitializeStatistics(int _fuelRodCount)
    {
        // Initialize the fuel rod variables and statistics
        totalFuelRods = _fuelRodCount;

        // Intitialize the fuel rod array
        fuelRods = fuelAssembly.FlattenedFuelRods();

        // Reset and initialize the fuel rod counts
        InitializeFuelRodCounts();

        // Subscribe to each of the fuel rods actions
        foreach (FuelRod _fuelRod in fuelRods)
        {
            _fuelRod.OnFuelRodStateChanged += UpdateFuelRodCounts;
        }

        // Set the fuel present flag
        fuelPresent = true;

        // Update the statistics
        UpdateStatistics();
    }

    private void InitializeFuelRodCounts()
    {
        // Iterate through each row
        for (int i = 0; i < fuelRods.Length; i++)
        {
            // Check if the fuel rod is fissile
            if (fuelRods[i].isFissile)
            {
                // Increment the fissile and reactive fuel rod counts
                fissileFuelRods++;
                reactiveFuelRods++;
            }
            else
            {
                // Increment the non-fissile fuel rod count
                nonFissileFuelRods++;
            }

            // Check if the fuel rod is saturated with xenon
            if (fuelRods[i].isXenon)
            {
                // Increment the xenon and reactive fuel rod count
                xenonSaturatedFuelRods++;
                reactiveFuelRods++;
            }
        }
    }

    private void ResetFuelRodCounts()
    {
        // Reset the fuel rod counts to zero
        totalFuelRods = 0;
        fissileFuelRods = 0;
        nonFissileFuelRods = 0;
        xenonSaturatedFuelRods = 0;
        reactiveFuelRods = 0;
    }

    private void UpdateFuelRodCounts(bool _isFissile, bool _isXenon, bool _isReactive)
    {
        // Check for the fissile propery (and reactivity)
        if (_isFissile)
        {
            // Increment the number of fissile and reactive fuel rods
            fissileFuelRods++;
            reactiveFuelRods++;

            // Decrement the number of non-fissile fuel rods
            nonFissileFuelRods--;
        }
        else
        {
            // Increment the number of non-fissile fuel rods
            nonFissileFuelRods++;

            // Decrement the number of fissile and reactive fuel rods
            fissileFuelRods--;
            reactiveFuelRods--;
        }

        // Check for xenon (and reactivity)
        if (_isXenon)
        {
            // Increment the number of xenon and reactive fuel rods
            xenonSaturatedFuelRods++;
            reactiveFuelRods++;
        }
        else
        {
            // Decrement the number of xenon and reactive fuel rods
            xenonSaturatedFuelRods--;
            reactiveFuelRods--;
        }
    }

    private void UpdateStatistics()
    {
        // Recalculate the percent statistics
        percentFuelFissile = Mathf.Round(((fissileFuelRods / totalFuelRods) * 10f) / 10f) * 100f;
        percentFuelNonFissile = Mathf.Round(((nonFissileFuelRods / totalFuelRods) * 10f) / 10f) * 100f;
        percentFuelXenonSaturated = Mathf.Round(((xenonSaturatedFuelRods / totalFuelRods) * 10f) / 10f) * 100f;
        percentFuelReactive = Mathf.Round(((reactiveFuelRods / totalFuelRods) * 10f) / 10f) * 100f;
        currentPowerPercent = Mathf.Round(((currentPower / targetPower) * 10f) / 10f) * 100f;

        // Calculate the current power as a percentage
        currentPowerPercent = Mathf.Round(((currentPower / targetPower) * 10f) / 10f) * 100f;

        // Calculate the difference between the current number of neutrons and the target number. Positive when over powered, negative when underpowered
        currentPowerDifference = currentPower - targetPower;

        // Update the coolant water tank measurements
        coolantWaterTank.UpdateMeasurements();

        // Calculate the average water temperature [F]
        averageWaterTempF = Mathf.Round((coolantWaterTank.averageTemperature * 10f) / 10f);

        // Convert the Fahrenheit average to Celsius
        averageWaterTempC = Mathf.Round((((averageWaterTempF - 32.0f) * 5.0f / 9.0f) * 10f) / 10f);
    }

    private void CalculatePower()
    {
        // Count all objects of type neutron that are active in the scene
        currentPower = FindObjectsOfType<Neutron>(false).Length;
    }

    private void InitializeUI()
    {
        // Initialize the values of the target power field
        targetPowerLabel.text = targetPower.ToString();
    }

    private void UpdateUI()
    {
        // Set the text for the actual, actual % power fields
        currentPowerLabel.text = currentPower.ToString();
        currentPowerPercentLabel.text = currentPowerPercent.ToString();

        // Set the current power difference field
        currentPowerDifferenceLabel.text = currentPowerDifference.ToString();

        // Set the text for the control and moderator rods fields
        controlRodEnabledLabel.text = rodArray.enableControlRods ? "Enabled" : "Disabled";
        controlRodStateLabel.text = rodArray.controlRodMovementState ? rodArray.controlRodMovementDirection ? "Raising" : "Lowering" : "N/A";

        moderatorRodEnabledLabel.text = rodArray.enableModeratorRods ? "Enabled" : "Disabled";
        moderatorRodStateLabel.text = rodArray.moderatorRodMovementState ? rodArray.controlRodMovementDirection ? "Raising" : "Lowering" : "N/A";

        // Set the text for the water fields
        waterPumpStateLabel.text = enableCoolantWaterPumps ? "Enabled" : "Disabled";
        waterTempLabel.text = averageWaterTempF.ToString();
        waterCapacityLabel.text = coolantWaterTank.currentWaterLevel.ToString();
    }

    #endregion

    #region TimerMethods

    private void StartUpdateTimer()
    {
        // Mark the start of the timer
        updateTimerStart = DateTime.Now;

        // Generate a new timespan telling the timer when the is
        TimeSpan _span = new TimeSpan(0, 0, updateTimerDuration);

        // Calculate the end time using the timespan above
        updateTimerEnd = updateTimerStart.Add(_span);

        // Update the in progress flag
        updateTimerRunning = true;

        // Start the timer coroutine
        StartCoroutine(UpdateTimer());
    }

    private IEnumerator UpdateTimer()
    {
        // Mark the start time
        DateTime start = DateTime.Now;

        // Calculate the number of seconds until finished
        double secondsToFinished = (updateTimerEnd - updateTimerStart).TotalSeconds;

        // Wait for that many seconds
        yield return new WaitForSeconds(Convert.ToSingle(secondsToFinished));

        // Update the power values
        CalculatePower();

        // Check if the settings menu is currently being displayed to avoid updating UI when not in view
        if (!UIController.showingSettingsMenu)
        {
            // Settings menu is not being displayed, update the UI
            UpdateUI(); 
        }

        // Restart the timer
        StartUpdateTimer();
    }

    #endregion
}
