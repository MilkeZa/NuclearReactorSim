using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// This class is meant to act as a single "rod" of Uranium-235 nuclear reactor fuel.
/// </summary>
public class FuelRod : MonoBehaviour
{
    #region Variables

    private SpriteRenderer spriteRenderer;  // Used to change the color of the fuel rod sprite
    private AudioSource audioSource;        // Audio source of sound effects
    private System.Random random;           // Used to create randomness when checking if probability events occur

    [Header("State Based Sprite Coloring")]
    [SerializeField]
    [Tooltip("Display color when the fuel is fissile")]
    private Color fissileColor = new Color(42f / 255f, 141f / 255f, 251f / 255f, 255f / 255f);      // Color displayed when fuel is fissile
    
    [SerializeField]
    [Tooltip("Display color when the fuel is non-fissile")]
    private Color nonFissileColor = new Color(206f / 255f, 206f / 255f, 206f / 255f, 255f / 255f);  // Color displayed when fuel is non-fissile

    [SerializeField]
    [Tooltip("Display color when the fuel is saturated with xenon")]
    private Color xenonColor = new Color(63f / 255f, 69f / 255f, 75f / 255f, 255f / 255f);          // Color displayed when fuel is saturated with xenon

    [Header("State Based Values")]
    [SerializeField]
    [Tooltip("Probability a non-fissile fuel rod will become fissile during the current frame")]
    [Range(0f, 1f)]
    private float fissileProbability = 0.01f;           // Probability of a non-fissile fuel rod becoming fissile during the current frame

    [SerializeField]
    [Tooltip("Chance of a non-fissile fuel rod emitting a neutron")]
    private float randomReleaseProbability = 0.001f;    // Probability of a non-fissile fuel rod to randomly "decay" realeasing a neutron during the current frame

    [SerializeField]
    [Tooltip("Probability a fissile fuel rod will decay into xenon after reacting with a neutron")]
    private float xenonDecayProbability = 0.05f;        // Probability of a fissile fuel rod to decay into xenon after reacting with a neutron

    [SerializeField]
    [Tooltip("Number of neutrons released when fissile fuel reacts")]
    private int neutronCount = 3;                       // Number of neutrons released when a fissile fuel rod reacts with a neutron

    [SerializeField]
    [Tooltip("Prefab to be used when creating neutrons")]
    private GameObject neutronPrefab;                   // Neutron prefab used to spawn after a fissile fuel rod reacts

    public bool isFissile { get; private set; } = false;    // Is the fuel ready to react?
    public bool isXenon { get; private set; } = false;      // Is the fuel saturated with xenon?
    public bool isReactive { get; private set; } = false;   // Can the fuel rod react with a neutron?
    
    public Action<bool, bool, bool> OnFuelRodStateChanged;  // Called when the state of the fuel rod changes

    private bool randomlyMakeFissileInProgress;     // Is the random fissile timer running?
    private DateTime timerStart;                    // Timer start time
    private DateTime timerEnd;                      // Timer expiration time
    private int randomlyMakeFissileSeconds = 1;     // Non-fissile rods have a random chance of becoming fissile every n seconds

    #endregion

    #region UnityMethods

    private void Awake()
    {
        // Create the random object using this gameobjects instance id as a seed
        int _randomSeed = gameObject.GetInstanceID() + Randomizer.GetRandomInt(Randomizer.GetRandomInt(-10000, 10000), Randomizer.GetRandomInt(100000, 500000));
        random = new System.Random(_randomSeed);

        // Get the sprite renderer component
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Get the audio source component
        audioSource = GetComponent<AudioSource>();

        // Fuel rods by default start out non-fissile
        UpdateState(FuelRodState.NonFissile);
    }

    #endregion

    #region StateMethods

    /// <summary>
    /// Update the state of a fuel rod.
    /// </summary>
    /// <param name="_state">State with which the rod is to be updated to.</param>
    public void UpdateState(FuelRodState _state)
    {
        // Update the fuel rod data
        switch (_state)
        {
            case FuelRodState.Fissile:
                // Check if the randomly make fissile timer is running
                if (randomlyMakeFissileInProgress)
                {
                    // Stop the timers coroutine
                    StopCoroutine(RandomlyMakeFissileTimer());

                    // Set the progress flag to false
                    randomlyMakeFissileInProgress = false;
                }

                // Fuel is able to react
                isFissile = true;
                isXenon = false;
                isReactive = true;

                spriteRenderer.color = fissileColor;

                break;
            case FuelRodState.NonFissile:
                // Start the make random fissile timer
                StartRandomlyMakeFissileTimer();

                // Fuel is not able to react
                isFissile = false;
                isXenon = false;
                isReactive = false;

                spriteRenderer.color = nonFissileColor;

                break;
            case FuelRodState.XenonSaturated:
                // Fuel is saturated with xenon and not able to react, but it can absorb neutron
                isFissile = false;
                isXenon = true;
                isReactive = true;

                spriteRenderer.color = xenonColor;

                break;
        }

        // Invoke the OnFuelRodStateChanged event
        OnFuelRodStateChanged?.Invoke(isFissile, isXenon, isReactive);
    }

    /// <summary>
    /// Randomly release a neutron due to radioactive decay (if possible).
    /// </summary>
    /// <returns>True if the random value is above the random release probability, otherwise, false.</returns>
    private bool RandomlyReleaseNeutron()
    {
        // Generate a random number between 0 and 1
        float _val = (float)random.NextDouble();

        // Check if the value is greater than the probability of releasing a neutron
        return _val > 1f - randomReleaseProbability;
    }

    /// <summary>
    /// Randomly make this fuel rod fissile, to help with the demonstration. To my knowledge, this does not occur in nature.
    /// </summary>
    /// <returns>True if the rod is to be made fissile, otherwise, false.</returns>
    private bool RandomlyMakeFissile()
    {
        // Generate a random number between 0 and 1
        float _val = (float)random.NextDouble();

        // Check if the value is greater than the probability of changing state to fissile and return the result
        return _val > 1f - fissileProbability;
    }

    #endregion

    #region ReactionMethods

    /// <summary>
    /// Handle a reaction with a neutron.
    /// </summary>
    /// <returns>True if the rod reacts with the neutron, otherwise, false.</returns>
    public bool NeutronReaction()
    {
        // Check if the fuel rod is in a reactive state
        if (!isReactive)
        {
            // Fuel rod is not reactive, return false as a reaction did not occur.
            return false;
        }

        // Play the neutron absorbed noise
        audioSource.Play();

        // The next state the fuel rod takes will need to be determined based on the current state
        FuelRodState _state;

        // Check if the fuel rod is xenon or not
        if (isXenon)
        {
            // Fuel rod is xenon and can "absorb" the neutron. Set the state to non-fissile
            _state = FuelRodState.NonFissile;
        }
        else
        {
            // Fuel rod is fissile and can absorb a neutron. Calculate if the next state is non-fissile or xenon decayed.
            // Update the neutron state, checking if the fuel will decay into xenon
            if ((float)random.NextDouble() > 1f - xenonDecayProbability)
            {
                // This fuel rod will decay into xenon
                _state = FuelRodState.XenonSaturated;
            }
            else
            {
                // This fuel rod will return to being non-fissile
                _state = FuelRodState.NonFissile;
            }
        
            // Release more neutrons
            ReleaseNeutrons(neutronCount);
        }

        // Set the state of the fuel rod
        UpdateState(_state);

        // Return true as a reaction occurred
        return true;
    }

    /// <summary>
    /// Release neutrons into the reactor core.
    /// </summary>
    /// <param name="_neutronCount">Number of neutrons to release.</param>
    private void ReleaseNeutrons(int _neutronCount)
    {
        // Instantiate neutron gameobjects
        for (int i = 0; i < _neutronCount; i++)
        {
            Instantiate(neutronPrefab, transform.position, Quaternion.identity, transform.parent);
        }
    }

    #endregion

    #region TimedMethods

    /// <summary>
    /// Start the timer controlling the random fissile event.
    /// </summary>
    private void StartRandomlyMakeFissileTimer()
    {
        // Mark the start time of the timer
        timerStart = DateTime.Now;

        // Generate a new timespan telling the timer when the end will be
        TimeSpan _time = new TimeSpan(0, 0, randomlyMakeFissileSeconds);

        // Calculate the end time using the timespan above
        timerEnd = timerStart.Add(_time);

        // Update the in progress flag
        randomlyMakeFissileInProgress = true;

        // Start the timer coroutine
        StartCoroutine(RandomlyMakeFissileTimer());
    }

    /// <summary>
    /// Control the timer handling the random fissile event.
    /// </summary>
    /// <returns></returns>
    private IEnumerator RandomlyMakeFissileTimer()
    {
        // Mark the start time
        DateTime start = DateTime.Now;

        // Calculate the number of seconds until finished
        double secondsToFinished = (timerEnd - start).TotalSeconds;

        // Wait for that many seconds
        yield return new WaitForSeconds(Convert.ToSingle(secondsToFinished));
        
        // Check if this fuel rod will become fissile
        if (RandomlyMakeFissile())
        {
            // This fuel rod is now fissile
            UpdateState(FuelRodState.Fissile);
        }
        else if (RandomlyReleaseNeutron())
        {
            // This fuel should release a single neutron
            ReleaseNeutrons(1);
        }

        // Restart the timer
        StartRandomlyMakeFissileTimer();
    }

    #endregion
}

/// <summary>
/// Enumerator used to set the state of a fuel rod.
/// </summary>
[System.Serializable]
public enum FuelRodState
{
    Fissile,
    NonFissile,
    XenonSaturated
}
